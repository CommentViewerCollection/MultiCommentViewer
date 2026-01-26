pub mod connection_manager;
pub mod core_actor;
pub mod plugin_host_actor;
pub mod site_browser_manager;

// 公開エクスポート
pub use connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
pub use core_actor::{CoreActor, PluginInfo, RegisterPlugin, SendMessageToCore, SendRequest, GetConnections, CreateConnection, RemoveConnection, RenameConnection};
pub use plugin_host_actor::{PluginHostActor, SendMessageToPlugin, ShutdownPlugin};
pub use site_browser_manager::{SiteAndBrowserManager, SiteInfo, BrowserInfo};

use actix::prelude::*;
use mcv_plugin_interface::Plugin;
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
    /// (plugin_id, plugin_host_addr)
    pub async fn register_plugin_from_dll<P: AsRef<Path>>(
        &self,
        dll_path: P,
    ) -> Result<(Uuid, Addr<PluginHostActor>), String> {
        println!("PluginManager::register_plugin_from_dll called");

        // DLLをロード
        let plugin_loader = PluginLoader::load(&dll_path).map_err(|e| {
            format!("Failed to load plugin DLL from {:?}: {}", dll_path.as_ref(), e)
        })?;

        let plugin_id = Uuid::new_v4();
        println!("Generated plugin_id: {}", plugin_id);
        println!("Loaded plugin: {} ({})", plugin_loader.metadata().name, plugin_loader.metadata().id);

        // Plugin-Host Actorを起動
        let mut plugin_host = PluginHostActor::new_from_dll(plugin_id, plugin_loader);
        println!("PluginHostActor created from DLL");

        if let Some(core_addr) = &self.core_addr {
            println!("Setting core_addr to PluginHostActor");
            plugin_host.set_core_addr(core_addr.clone());
        } else {
            eprintln!("ERROR: Core actor not set in PluginManager");
            return Err("Core actor not set".to_string());
        }

        println!("Starting PluginHostActor...");
        let plugin_host_addr = plugin_host.start();
        println!("PluginHostActor started, addr: {:?}", plugin_host_addr);

        Ok((plugin_id, plugin_host_addr))
    }

    /// プラグインを登録（旧形式、静的リンク用）
    ///
    /// # Returns
    /// (plugin_id, plugin_host_addr)
    pub async fn register_plugin(
        &self,
        plugin: Box<dyn Plugin>,
    ) -> Result<(Uuid, Addr<PluginHostActor>), String> {
        println!("PluginManager::register_plugin called");
        let plugin_id = Uuid::new_v4();
        println!("Generated plugin_id: {}", plugin_id);

        // Plugin-Host Actorを起動
        let mut plugin_host = PluginHostActor::new(plugin_id, plugin);
        println!("PluginHostActor created");

        if let Some(core_addr) = &self.core_addr {
            println!("Setting core_addr to PluginHostActor");
            plugin_host.set_core_addr(core_addr.clone());
        } else {
            eprintln!("ERROR: Core actor not set in PluginManager");
            return Err("Core actor not set".to_string());
        }

        println!("Starting PluginHostActor...");
        let plugin_host_addr = plugin_host.start();
        println!("PluginHostActor started, addr: {:?}", plugin_host_addr);

        Ok((plugin_id, plugin_host_addr))
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
        let plugin_id = Uuid::new_v4();

        manager.add_connection(
            conn_id,
            Some(plugin_id),  // 変更: Optionで渡す
            "Test Site".to_string(),
            "Test Input".to_string(),
            "#1".to_string(),
        );
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Created)
        );
    }
}
