pub mod connection_manager;
pub mod core_actor;
pub mod internal_message;
pub mod plugin_host_actor;
pub mod site_browser_manager;

// 公開エクスポート
pub use connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
pub use core_actor::{
    CoreActor, CreateConnection, GetBrowsers, GetConnections, GetSites, LogicalPluginInfo,
    PluginInfo, RegisterPhysicalPlugin, RemoveConnection, RenameConnection, SendMessageToCore,
    SendRequest, SetConnectionSite, UpdateConnectionSettings,
};
pub use plugin_host_actor::{PhysicalPluginHostActor, SendMessageToPlugin, ShutdownPlugin};
pub use site_browser_manager::{BrowserInfo, SiteAndBrowserManager, SiteInfo};

use actix::prelude::*;
use mcv_common::PhysicalPluginId;
use mcv_plugin_loader::PluginLoader;
use std::path::Path;
use uuid::Uuid;

/// プラグインマネージャー
///
/// プラグインの登録・管理を担当
pub struct PluginManager {
    core_addr: Option<Addr<CoreActor>>,
}

impl PluginManager {
    /// 新しいPlugin Managerを作成
    pub fn new() -> Self {
        Self { core_addr: None }
    }

    /// Core Actorのアドレスを設定
    pub fn set_core_addr(&mut self, addr: Addr<CoreActor>) {
        self.core_addr = Some(addr);
    }

    /// プラグインを登録（DLLから）
    ///
    /// # Arguments
    /// * `dll_path` - プラグインDLLのパス
    ///
    /// # Returns
    /// (physical_plugin_id, plugin_host_addr, plugin_name)
    pub async fn register_plugin_from_dll<P: AsRef<Path>>(
        &self,
        dll_path: P,
    ) -> Result<(PhysicalPluginId, Addr<PhysicalPluginHostActor>, String), String> {
        tracing::trace!(target: "mcv::core::PluginManager", "PluginManager::register_plugin_from_dll called");

        // DLLをロード
        let plugin_loader = PluginLoader::load(&dll_path).map_err(|e| {
            format!(
                "Failed to load plugin DLL from {:?}: {}",
                dll_path.as_ref(),
                e
            )
        })?;
        // 物理プラグインの名称はdll名とする
        let plugin_name = dll_path
            .as_ref()
            .file_stem()
            .and_then(|s| s.to_str())
            .unwrap_or("unknown_plugin")
            .to_string();
        //物理プラグインのIDを生成
        let physical_plugin_id = PhysicalPluginId::new();
        tracing::trace!(target: "mcv::core::PluginManager", "Generated physical_plugin_id: {} for {}", physical_plugin_id, plugin_name);

        tracing::trace!(target: "mcv::core::PluginManager", "Loaded plugin: {} (id: {})", plugin_name, physical_plugin_id);

        // Plugin-Host Actorを起動
        let mut plugin_host =
            PhysicalPluginHostActor::new_from_dll(physical_plugin_id, plugin_loader);
        tracing::trace!(target: "mcv::core::PluginManager", "PluginHostActor created from DLL");

        if let Some(core_addr) = &self.core_addr {
            tracing::trace!(target: "mcv::core::PluginManager", "Setting core_addr to PluginHostActor");
            plugin_host.set_core_addr(core_addr.clone());
        } else {
            tracing::error!(target: "mcv::core::PluginManager", "ERROR: Core actor not set in PluginManager");
            return Err("Core actor not set".to_string());
        }

        tracing::trace!(target: "mcv::core::PluginManager", "Starting PluginHostActor...");
        let plugin_host_addr = plugin_host.start();
        tracing::trace!(target: "mcv::core::PluginManager", "PluginHostActor started, addr: {:?}", plugin_host_addr);

        Ok((physical_plugin_id, plugin_host_addr, plugin_name))
    }

    /// 指定ディレクトリ内のDLLプラグインをスキャンして登録
    ///
    /// # Arguments
    /// * `plugins_dir` - プラグインディレクトリのパス
    ///
    /// # Returns
    /// Vec<(physical_plugin_id, plugin_host_addr, plugin_name)>
    pub async fn scan_and_load_plugins<P: AsRef<Path>>(
        &self,
        plugins_dir: P,
    ) -> Vec<(PhysicalPluginId, Addr<PhysicalPluginHostActor>, String)> {
        let plugins_dir = plugins_dir.as_ref();
        tracing::info!(plugins_dir = %plugins_dir.display(), "Scanning for DLL plugins");

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

        for entry in entries {
            let entry = match entry {
                Ok(e) => e,
                Err(e) => {
                    tracing::warn!(target: "mcv::core::PluginManager", error = %e, "Failed to read directory entry");
                    continue;
                }
            };

            let path = entry.path();

            // .dllファイルのみ処理
            if path.extension().and_then(|s| s.to_str()) != Some("dll") {
                continue;
            }

            tracing::info!(target: "mcv::core::PluginManager", dll_path = %path.display(), "Found DLL plugin");

            // DLLをロード
            match self.register_plugin_from_dll(&path).await {
                Ok((physical_plugin_id, plugin_host_addr, plugin_name)) => {
                    // plugin_nameはregister_plugin_from_dllから取得（metadata().name）
                    tracing::info!(
                        target: "mcv::core::PluginManager",
                        plugin_id = %physical_plugin_id,
                        plugin_name = %plugin_name,
                        dll_path = %path.display(),
                        "Successfully loaded DLL plugin"
                    );

                    loaded_plugins.push((physical_plugin_id, plugin_host_addr, plugin_name));
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

    /// 指定ディレクトリ内のDLLプラグインをスキャンして登録（新形式）
    ///
    /// # Arguments
    /// * `plugins_dir` - プラグインディレクトリのパス
    ///
    /// # Returns
    /// Vec<(physical_plugin_id, plugin_host_addr, plugin_name)>
    pub async fn scan_and_load_plugins_new<P: AsRef<Path>>(
        &self,
        plugins_dir: P,
    ) -> Vec<(PhysicalPluginId, Addr<PhysicalPluginHostActor>, String)> {
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

        for entry in entries {
            let entry = match entry {
                Ok(e) => e,
                Err(e) => {
                    tracing::warn!(target: "mcv::core::PluginManager", error = %e, "Failed to read directory entry");
                    continue;
                }
            };

            let path = entry.path();
            if path.is_file() {
                // ファイルの場合はスキップ
                continue;
            }
            //path中のmanifest.jsonを探す
            let manifest_path = path.join("manifest.json");
            if !manifest_path.exists() {
                //manifest.jsonが存在しない場合はスキップ
                tracing::warn!(target: "mcv::core::PluginManager", dir_path = %path.display(), "No manifest.json found in plugin directory, skipping");
                continue;
            }
            // manifest.jsonを読み込む（将来使用予定）
            let _manifest_content = match std::fs::read_to_string(&manifest_path) {
                Ok(content) => content,
                Err(e) => {
                    tracing::warn!(target: "mcv::core::PluginManager", error = %e, "Failed to read manifest.json in plugin directory: {}", path.display());
                    continue;
                }
            };

            tracing::info!(target: "mcv::core::PluginManager", dll_path = %path.display(), "Found DLL plugin");

            // DLLをロード
            match self.register_plugin_from_dll(&path).await {
                Ok((physical_plugin_id, plugin_host_addr, plugin_name)) => {
                    // plugin_nameはregister_plugin_from_dllから取得（metadata().name）
                    tracing::info!(
                        target: "mcv::core",
                        plugin_id = %physical_plugin_id,
                        plugin_name = %plugin_name,
                        dll_path = %path.display(),
                        "Successfully loaded DLL plugin"
                    );

                    loaded_plugins.push((physical_plugin_id, plugin_host_addr, plugin_name));
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
}
