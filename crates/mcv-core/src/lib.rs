pub mod connection_manager;
pub mod core_actor;
pub mod plugin_host_actor;

// 公開エクスポート
pub use connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
pub use core_actor::{CoreActor, PluginInfo, RegisterPlugin, SendMessageToCore, SendRequest, GetConnections, CreateConnection, RemoveConnection};
pub use plugin_host_actor::{PluginHostActor, SendMessageToPlugin, ShutdownPlugin};

use actix::prelude::*;
use mcv_plugin_interface::Plugin;
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

    /// プラグインを登録
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

        manager.add_connection(conn_id, plugin_id);
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Created)
        );
    }
}
