pub mod connection_manager;
pub mod connection_persistence;
pub mod core_actor;
pub mod internal_message;
pub(crate) mod message_handlers;
pub mod plugin_host_actor;
pub mod plugin_host_actor_v3;
pub mod plugin_loader_strategy;
pub mod site_browser_manager;

// 公開エクスポート
pub use connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
pub use connection_persistence::{ConnectionsStorage, PersistedConnection};
pub use core_actor::{
    CoreActor, CreateConnection, GetBrowsers, GetConnections, GetLogicalPlugins, GetSites, LogicalPluginInfo,
    PluginInfo, RegisterPhysicalPlugin, RemoveConnection, RenameConnection, SendMessageToCore,
    SendRequest, SetConnectionSite, UpdateConnectionSettings,
};
pub use plugin_host_actor::{PhysicalPluginHostActor, SendMessageToPlugin, ShutdownPlugin};
pub use plugin_loader_strategy::{
    LoadedPluginInfo, PluginHostAddr, PluginLoaderRegistry, PluginLoaderStrategy, V2LoaderStrategy,
    V3LoaderStrategy,
};
use serde::Deserialize;
pub use site_browser_manager::{BrowserInfo, SiteAndBrowserManager, SiteInfo};

use actix::prelude::*;
use mcv_common::PhysicalPluginId;
use mcv_plugin_loader::{PluginLoader, PluginLoaderError};
use std::{
    collections::HashMap,
    fs::{DirEntry, File},
    io::{BufReader, Read},
    path::{Path, PathBuf},
    str::FromStr,
};
#[derive(Deserialize)]
struct Manifest {
    path: String,
}
/// プラグインマネージャー
///
/// プラグインの登録・管理を担当
pub struct PluginManager {
    core_addr: Option<Addr<CoreActor>>,
    registry: PluginLoaderRegistry,
}

impl PluginManager {
    /// 新しいPlugin Managerを作成
    pub fn new() -> Self {
        let mut registry = PluginLoaderRegistry::new();

        // デフォルト戦略を登録（v3 → v2 の順で検出）
        registry.register(Box::new(V3LoaderStrategy));
        registry.register(Box::new(V2LoaderStrategy));

        Self {
            core_addr: None,
            registry,
        }
    }

    /// Core Actorのアドレスを設定
    pub fn set_core_addr(&mut self, addr: Addr<CoreActor>) {
        self.core_addr = Some(addr);
    }
    fn is_bare_dll(is_file: bool, path: &Path) -> Option<PathBuf> {
        is_file
            .then(|| path.to_path_buf())
            .filter(|p| p.extension().and_then(|e| e.to_str()) == Some("dll"))
    }
    /// pluginsディレクトリに直接置かれたdllプラグインのpathを取得する
    fn get_bare_dll_plugin_path(entry: &DirEntry) -> Option<PathBuf> {
        let is_file = entry.file_type().ok()?.is_file();
        let path = entry.path();
        Self::is_bare_dll(is_file, &path)
    }
    /// pluginsディレクトリに各プラグイン専用のディレクトリを置いているdllプラグインのpathを取得する
    fn get_dir_dll_plugin_path(entry: &DirEntry) -> Option<PathBuf> {
        let dir_path = (entry.file_type().ok()?.is_dir()).then(|| entry.path())?;
        tracing::trace!(target:"mcv::mcv-core::PluginManager", "dir_path={:?}", dir_path);
        let manifest_path = dir_path.join("manifest.json");
        tracing::trace!(target:"mcv::mcv-core::PluginManager", "manifest_path={:?}", manifest_path);
        let reader = BufReader::new(File::open(manifest_path).ok()?);

        Self::get_dll_plugin_path_from_manifest(reader, &dir_path)
    }

fn read_dll_path_from_manifest<R: Read>(mut reader: R) -> Option<PathBuf> {
    let mut buf = Vec::new();
    reader.read_to_end(&mut buf).ok()?;

    // UTF-8 BOM を除去
    const BOM: &[u8] = b"\xEF\xBB\xBF";
    let buf = if buf.starts_with(BOM) {
        &buf[BOM.len()..]
    } else {
        &buf[..]
    };

    let manifest: Manifest = match serde_json::from_slice(buf) {
        Ok(m) => m,
        Err(e) => {
            tracing::error!("manifest parse failed: {}", e);
            return None;
        }
    };

    Some(PathBuf::from(manifest.path))
}

fn get_dll_plugin_path_from_manifest<R: Read>(
    reader: R,
    manifest_dir: &Path,
) -> Option<PathBuf> {
    let manifest_path = Self::read_dll_path_from_manifest(reader)?;
    tracing::trace!(target:"mcv::mcv-core::PluginManager", "manifest relative path = {:?}", manifest_path);

    // 絶対パスまたはルート相対パスはディレクトリ外への脱出になるため拒否
    if manifest_path.is_absolute() || manifest_path.has_root() {
        return None;
    }

    // ".." コンポーネントによるパストラバーサルを防止
    if manifest_path.components().any(|c| c == std::path::Component::ParentDir) {
        return None;
    }

    let joined_path = manifest_dir.join(manifest_path);
    tracing::trace!(target:"mcv::mcv-core::PluginManager", "joined_path = {:?}", joined_path);

    Some(joined_path)
}
    /// zip化されたプラグインのpathを取得する
    fn get_zip_dll_plugin_path(_entry: &DirEntry) -> Option<PathBuf> {
        None
    }
    /// 指定ディレクトリ内のDLLプラグインをスキャンして登録（新形式）
    ///
    /// # Arguments
    /// * `plugins_dir` - プラグインディレクトリのパス
    ///
    /// # Returns
    /// Vec<(physical_plugin_id, plugin_host_addr, plugin_name)>
    pub async fn scan_and_load_plugins<P: AsRef<Path>>(
        &self,
        plugins_dir: P,
    ) -> Vec<LoadedPluginInfo> {
        //dllファイルでcoreが直接読み込むプラグインを物理プラグインと呼ぶ
        //物理プラグインは以下の3つの形状をしている
        //1. zip化されているプラグイン
        //2. 各プラグイン専用のディレクトリに展開されているプラグイン
        //3. dllファイル単体で配置されているプラグイン
        //
        //1と2のプラグインは、プラグイン専用のディレクトリにmanifest.jsonが存在し、その中のpathでdllファイルのパスを指定する

        //TODO: zip化されている場合は一時ディレクトリに展開してから読み込む処理が必要になる。

        let plugins_dir = plugins_dir.as_ref();
        tracing::info!(target: "mcv::core::PluginManager", plugins_dir = %plugins_dir.display(), "Scanning for DLL plugins");

        let mut loaded_plugins = Vec::new();

        // ディレクトリが存在しない場合は作成
        if !plugins_dir.exists() {
            tracing::warn!(
                target: "mcv::core::PluginManager",
                "Plugins directory does not exist, creating: {}",
                plugins_dir.display()
            );
            if let Err(e) = std::fs::create_dir_all(plugins_dir) {
                tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to create plugins directory");
                return loaded_plugins;
            }
        }

        // ディレクトリ内の.dllファイルをスキャン
        let entries = match std::fs::read_dir(plugins_dir) {
            Ok(entries) => entries,
            Err(e) => {
                tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to read plugins directory");
                return loaded_plugins;
            }
        };
        //entryのタイプによってdllプラグインの配置方法が違う。タイプに合った読み込み方法を採用する
        let entries = entries.filter_map(|e| e.ok());
        let a :Vec<DirEntry> = entries.collect();
                tracing::info!(target:"mcv",count = a.len(), "候補DirEntry数");
        let plugin_paths = a.iter().filter_map(|entry| {
            Self::get_bare_dll_plugin_path(&entry)
                .or_else(|| Self::get_dir_dll_plugin_path(&entry))
                .or_else(|| Self::get_zip_dll_plugin_path(&entry))
        });
        // Core Actorのアドレスを取得
        let core_addr = match &self.core_addr {
            Some(addr) => addr.clone(),
            None => {
                tracing::error!(target: "mcv::core::PluginManager", "Core actor not set");
                return loaded_plugins;
            }
        };
        let plugin_paths:Vec<PathBuf> = plugin_paths.collect();
        tracing::info!(target:"mcv",count = plugin_paths.len(), "候補プラグイン数");
        for path in plugin_paths {
            // Registryを使用してDLLをロード
            match self.registry.load_plugin(&path, core_addr.clone()).await {
                Ok(loaded_info) => {
                    tracing::info!(
                        target: "mcv::core::PluginManager",
                        plugin_id = %loaded_info.physical_plugin_id,
                        abi_version = loaded_info.abi_version,
                        plugin_name = ?loaded_info.plugin_name,
                        dll_path = %path.display(),
                        "Successfully loaded DLL plugin"
                    );

                    loaded_plugins.push(loaded_info);
                }
                Err(e) => {
                    tracing::error!(
                        target: "mcv::core::PluginManager",
                        dll_path = %path.display(),
                        error = %e,
                        "Failed to load DLL plugin"
                    );
                }
            }
        }

        tracing::info!(target: "mcv::core::PluginManager", count = loaded_plugins.len(), "DLL plugins scan completed");
        loaded_plugins
    }
}

impl Default for PluginManager {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use uuid::Uuid;

    #[test]
    fn test_plugin_manager_creation() {
        let manager = PluginManager::new();
        assert!(manager.core_addr.is_none());
    }

    #[test]
    fn test_connection_manager() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Created)
        );
    }
    #[test]
    fn test_is_bare_dll() {
        assert!(PluginManager::is_bare_dll(true, Path::new("a.dll")).is_some());
    }

    mod get_dll_plugin_path_from_manifest {
        use std::path::PathBuf;

        use crate::PluginManager;

        #[test]
        fn valid_manifest_returns_absolute_path() {
            let json = r#"{ "path": "plugin.dll" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            let result = PluginManager::get_dll_plugin_path_from_manifest(reader, &base_dir);
            assert_eq!(result, Some(base_dir.join("plugin.dll")));
        }

        #[test]
        fn missing_path_returns_none() {
            let json = r#"{ "name": "test" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::get_dll_plugin_path_from_manifest(reader, &base_dir).is_none());
        }

        #[test]
        fn non_string_path_returns_none() {
            let json = r#"{ "path": 123 }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::get_dll_plugin_path_from_manifest(reader, &base_dir).is_none());
        }

        #[test]
        fn invalid_json_returns_none() {
            let json = r#"{ path: }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::get_dll_plugin_path_from_manifest(reader, &base_dir).is_none());
        }

        #[test]
        fn absolute_manifest_path_outside_dir_returns_none() {
            let json = r#"{ "path": "/other/plugin.dll" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::get_dll_plugin_path_from_manifest(reader, &base_dir).is_none());
        }
    }
}
