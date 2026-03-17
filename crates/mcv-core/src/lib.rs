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
    CoreActor, CreateConnection, DetectUrl, GetBrowsers, GetConnections, GetLogicalPlugins,
    GetPhysicalPlugins, GetSites, LogicalPluginInfo, PluginInfo, RegisterPhysicalPlugin,
    RemoveConnection, RenameConnection, ScanAndLoadNewPlugins, SendMessageToCore, SendRequest,
    SetConnectionSite, UpdateConnectionSettings,
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
use mcv_plugin_loader::PluginLoaderError;
use std::{
    collections::HashSet,
    fs::{DirEntry, File},
    io::{BufReader, Read},
    path::{Path, PathBuf},
};
use zip::ZipArchive;

/// plugin.json のマニフェスト（ディレクトリ形式・ZIP 内どちらにも対応）
#[derive(Deserialize)]
struct PluginManifest {
    /// プラグイン ID（省略時はディレクトリ名や ZIP stem をフォールバック）
    #[serde(default)]
    id: Option<String>,
    /// DLL ファイル名（"entry" エイリアスも受け付ける）
    #[serde(alias = "entry")]
    path: String,
}

/// プラグインマネージャー
///
/// プラグインの登録・管理を担当
pub struct PluginManager {
    core_addr: Option<Addr<CoreActor>>,
    registry: PluginLoaderRegistry,
    /// 既にロード済みのプラグイン ID（再スキャン時の重複ロードを防ぐ）
    loaded_physical_ids: HashSet<String>,
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
            loaded_physical_ids: HashSet::new(),
        }
    }

    /// Core Actorのアドレスを設定
    pub fn set_core_addr(&mut self, addr: Addr<CoreActor>) {
        self.core_addr = Some(addr);
    }

    /// plugin.json を読み込んで (dll_path, plugin_id) を返す共通処理
    ///
    /// - `base_dir`: DLL の検索基点となるディレクトリ
    /// - `fallback_id`: `plugin.json` に `id` が無い場合のフォールバック
    fn read_manifest<R: Read>(
        mut reader: R,
        base_dir: &Path,
        fallback_id: &str,
    ) -> Option<(PathBuf, String)> {
        let mut buf = Vec::new();
        reader.read_to_end(&mut buf).ok()?;

        // UTF-8 BOM を除去
        const BOM: &[u8] = b"\xEF\xBB\xBF";
        let buf = if buf.starts_with(BOM) {
            &buf[BOM.len()..]
        } else {
            &buf[..]
        };

        let manifest: PluginManifest = match serde_json::from_slice(buf) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!("plugin.json parse failed: {}", e);
                return None;
            }
        };

        let dll_rel = PathBuf::from(&manifest.path);

        // 絶対パスおよびパストラバーサルを拒否
        if dll_rel.is_absolute() || dll_rel.has_root() {
            tracing::error!(
                "plugin.json: absolute path is not allowed: {}",
                manifest.path
            );
            return None;
        }
        if dll_rel
            .components()
            .any(|c| c == std::path::Component::ParentDir)
        {
            tracing::error!(
                "plugin.json: path traversal is not allowed: {}",
                manifest.path
            );
            return None;
        }

        let plugin_id = manifest
            .id
            .filter(|s| !s.is_empty())
            .unwrap_or_else(|| fallback_id.to_string());

        Some((base_dir.join(dll_rel), plugin_id))
    }

    /// サブディレクトリ形式のプラグイン（{id}/plugin.json + DLL）から (dll_path, plugin_id) を取得する
    fn get_dir_plugin(entry: &DirEntry) -> Option<(PathBuf, String)> {
        let is_dir = entry.file_type().ok()?.is_dir();
        if !is_dir {
            return None;
        }
        let dir_path = entry.path();

        // 隠しディレクトリ（.cache など）は除外
        let dir_name = dir_path.file_name().and_then(|s| s.to_str())?;
        if dir_name.starts_with('.') {
            return None;
        }

        let manifest_path = dir_path.join("plugin.json");
        if !manifest_path.exists() {
            return None;
        }

        let reader = BufReader::new(File::open(&manifest_path).ok()?);
        let (dll_path, plugin_id) = Self::read_manifest(reader, &dir_path, dir_name)?;

        if !dll_path.exists() {
            tracing::debug!(
                target: "mcv::core::PluginManager",
                dll = %dll_path.display(),
                "DLL not found in plugin directory, skipping"
            );
            return None;
        }

        Some((dll_path, plugin_id))
    }

    /// ZIP 形式のプラグインから (dll_path, plugin_id) を取得する
    ///
    /// ZIPファイルを `.cache/{stem}/` に展開し、`plugin.json` の `id` フィールド（または ZIP stem）を
    /// plugin_id、`entry`（または `path`）フィールドで指定された DLL のパスを返す。
    fn get_zip_plugin(entry: &DirEntry) -> Option<(PathBuf, String)> {
        let is_file = entry.file_type().ok()?.is_file();
        if !is_file {
            return None;
        }
        let zip_path = entry.path();
        if zip_path
            .extension()
            .and_then(|e| e.to_str())
            .map(|e| e.to_lowercase())
            .as_deref()
            != Some("zip")
        {
            return None;
        }

        let stem = zip_path.file_stem().and_then(|s| s.to_str())?.to_string();
        let plugins_dir = zip_path.parent()?;
        let cache_dir = plugins_dir.join(".cache").join(&stem);

        // キャッシュが存在しない場合はZIPを展開する
        if !cache_dir.exists() {
            tracing::info!(target: "mcv::core::PluginManager", zip = %zip_path.display(), cache = %cache_dir.display(), "Extracting plugin ZIP to cache");
            if let Err(e) = std::fs::create_dir_all(&cache_dir) {
                tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to create cache directory");
                return None;
            }

            let file = match File::open(&zip_path) {
                Ok(f) => f,
                Err(e) => {
                    tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to open plugin ZIP");
                    return None;
                }
            };
            let mut archive = match ZipArchive::new(file) {
                Ok(a) => a,
                Err(e) => {
                    tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to read plugin ZIP");
                    let _ = std::fs::remove_dir_all(&cache_dir);
                    return None;
                }
            };

            for i in 0..archive.len() {
                let mut zip_file = match archive.by_index(i) {
                    Ok(f) => f,
                    Err(e) => {
                        tracing::warn!(target: "mcv::core::PluginManager", error = %e, "Skipping ZIP entry");
                        continue;
                    }
                };
                let out_path = match zip_file.enclosed_name() {
                    Some(p) => cache_dir.join(p),
                    None => continue,
                };
                if zip_file.name().ends_with('/') {
                    let _ = std::fs::create_dir_all(&out_path);
                } else {
                    if let Some(parent) = out_path.parent() {
                        let _ = std::fs::create_dir_all(parent);
                    }
                    let mut out_file = match File::create(&out_path) {
                        Ok(f) => f,
                        Err(e) => {
                            tracing::warn!(target: "mcv::core::PluginManager", error = %e, path = %out_path.display(), "Failed to create extracted file");
                            continue;
                        }
                    };
                    if let Err(e) = std::io::copy(&mut zip_file, &mut out_file) {
                        tracing::warn!(target: "mcv::core::PluginManager", error = %e, "Failed to extract ZIP entry");
                    }
                }
            }
        }

        let manifest_path = cache_dir.join("plugin.json");
        // ZIP stem をフォールバック ID として使用（plugin.json に id がない場合）
        let reader = BufReader::new(File::open(&manifest_path).ok()?);
        Self::read_manifest(reader, &cache_dir, &stem)
    }

    /// 指定ディレクトリ内のDLLプラグインをスキャンして登録
    ///
    /// ① サブディレクトリ形式（{id}/plugin.json + DLL）をロード
    /// ② ZIP 形式プラグインをロード（① でロード済みの ID はスキップ）
    pub async fn scan_and_load_plugins<P: AsRef<Path>>(
        &mut self,
        plugins_dir: P,
    ) -> Vec<LoadedPluginInfo> {
        let plugins_dir = plugins_dir.as_ref();
        tracing::info!(target: "mcv::core::PluginManager", plugins_dir = %plugins_dir.display(), "Scanning for DLL plugins");

        let mut loaded_plugins = Vec::new();
        // 今回の呼び出し内での重複防止 + 既にロード済みの ID はスキップ
        let mut loaded_ids: HashSet<String> = self.loaded_physical_ids.clone();

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

        // Core Actorのアドレスを取得
        let core_addr = match &self.core_addr {
            Some(addr) => addr.clone(),
            None => {
                tracing::error!(target: "mcv::core::PluginManager", "Core actor not set");
                return loaded_plugins;
            }
        };

        let entries = match std::fs::read_dir(plugins_dir) {
            Ok(e) => e,
            Err(e) => {
                tracing::error!(target: "mcv::core::PluginManager", error = %e, "Failed to read plugins directory");
                return loaded_plugins;
            }
        };

        let dir_entries: Vec<DirEntry> = entries.flatten().collect();
        // ① サブディレクトリ形式を先に処理
        for entry in &dir_entries {
            if let Some((dll_path, plugin_id)) = Self::get_dir_plugin(entry) {
                if loaded_ids.contains(&plugin_id) {
                    tracing::warn!(
                        target: "mcv::core::PluginManager",
                        id = %plugin_id,
                        dll = %dll_path.display(),
                        "Directory plugin already loaded (duplicate plugin.json id), skipping"
                    );
                    continue;
                }
                let physical_plugin_id = PhysicalPluginId::from_id(&plugin_id);
                tracing::info!(
                    target: "mcv::core::PluginManager",
                    id = %plugin_id,
                    dll = %dll_path.display(),
                    "Loading directory plugin"
                );
                match self
                    .registry
                    .load_plugin(&dll_path, physical_plugin_id, core_addr.clone())
                    .await
                {
                    Ok(info) => {
                        self.loaded_physical_ids.insert(plugin_id.clone());
                        loaded_ids.insert(plugin_id);
                        loaded_plugins.push(info);
                    }
                    Err(PluginLoaderError::NotApplicable) => {}
                    Err(e) => {
                        tracing::error!(
                            target: "mcv::core::PluginManager",
                            id = %plugin_id,
                            dll = %dll_path.display(),
                            error = %e,
                            "Failed to load directory plugin"
                        );
                    }
                }
            }
        }

        // ② ZIP 形式プラグインをスキャン（① でロード済みの ID はスキップ）
        for entry in &dir_entries {
            if let Some((dll_path, plugin_id)) = Self::get_zip_plugin(entry) {
                if loaded_ids.contains(&plugin_id) {
                    tracing::debug!(
                        target: "mcv::core::PluginManager",
                        id = %plugin_id,
                        "ZIP plugin already loaded as directory plugin, skipping"
                    );
                    continue;
                }
                let physical_plugin_id = PhysicalPluginId::from_id(&plugin_id);
                tracing::info!(
                    target: "mcv::core::PluginManager",
                    id = %plugin_id,
                    dll = %dll_path.display(),
                    "Loading ZIP plugin"
                );
                match self
                    .registry
                    .load_plugin(&dll_path, physical_plugin_id, core_addr.clone())
                    .await
                {
                    Ok(info) => {
                        self.loaded_physical_ids.insert(plugin_id.clone());
                        loaded_ids.insert(plugin_id);
                        loaded_plugins.push(info);
                    }
                    Err(PluginLoaderError::NotApplicable) => {}
                    Err(e) => {
                        tracing::error!(
                            target: "mcv::core::PluginManager",
                            dll = %dll_path.display(),
                            error = %e,
                            "Failed to load ZIP plugin"
                        );
                    }
                }
            }
        }

        // dir_entries は読み取り済みなので drop
        drop(dir_entries);

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

    mod read_manifest {
        use std::path::PathBuf;

        use crate::PluginManager;

        #[test]
        fn valid_manifest_with_id() {
            let json = r#"{ "id": "chrome-cookie", "entry": "plugin.dll" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/chrome-cookie");
            let result = PluginManager::read_manifest(reader, &base_dir, "fallback");
            assert_eq!(
                result,
                Some((base_dir.join("plugin.dll"), "chrome-cookie".to_string()))
            );
        }

        #[test]
        fn valid_manifest_without_id_uses_fallback() {
            let json = r#"{ "path": "plugin.dll" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            let result = PluginManager::read_manifest(reader, &base_dir, "example");
            assert_eq!(
                result,
                Some((base_dir.join("plugin.dll"), "example".to_string()))
            );
        }

        #[test]
        fn missing_path_returns_none() {
            let json = r#"{ "name": "test" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::read_manifest(reader, &base_dir, "example").is_none());
        }

        #[test]
        fn non_string_path_returns_none() {
            let json = r#"{ "path": 123 }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::read_manifest(reader, &base_dir, "example").is_none());
        }

        #[test]
        fn invalid_json_returns_none() {
            let json = r#"{ path: }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::read_manifest(reader, &base_dir, "example").is_none());
        }

        #[test]
        fn absolute_path_returns_none() {
            let json = r#"{ "path": "/other/plugin.dll" }"#;
            let reader = std::io::Cursor::new(json);
            let base_dir = PathBuf::from("plugins/example");
            assert!(PluginManager::read_manifest(reader, &base_dir, "example").is_none());
        }
    }
}
