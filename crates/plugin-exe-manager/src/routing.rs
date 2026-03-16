use mcv_messages::{Message as McvMessage, MessageDestination, PluginId};
use std::collections::HashMap;
use std::sync::Arc;
use thiserror::Error;
use tokio::sync::{mpsc, RwLock};

#[derive(Debug, Error)]
pub enum RoutingError {
    #[error("Plugin not found: {0}")]
    PluginNotFound(String),

    #[error("Send error: {0}")]
    SendError(String),

    #[error("Broadcast error: {0}")]
    BroadcastError(String),
}

/// WebSocketクライアント情報（routing用）
pub struct ClientInfo {
    pub plugin_id: PluginId,
    pub sender: mpsc::UnboundedSender<McvMessage>,
    pub roles: Vec<String>,
}

/// メッセージルーティング
///
/// Core ↔ EXEプラグイン間のメッセージをルーティングする
pub struct MessageRouter {
    clients: Arc<RwLock<HashMap<PluginId, ClientInfo>>>,
}

impl MessageRouter {
    /// 新しいメッセージルーターを作成
    pub fn new(clients: Arc<RwLock<HashMap<PluginId, ClientInfo>>>) -> Self {
        Self { clients }
    }

    /// 特定のEXEプラグインへメッセージを送信（ユニキャスト）
    pub async fn route_to_plugin(
        &self,
        plugin_id: PluginId,
        message: McvMessage,
    ) -> Result<(), RoutingError> {
        tracing::debug!(
            target: "mcv::plugin_exe_manager::MessageRouter",
            plugin_id = %plugin_id,
            message_type = ?message.message_type,
            "Routing message to specific plugin"
        );

        let clients = self.clients.read().await;
        let client = clients
            .get(&plugin_id)
            .ok_or(RoutingError::PluginNotFound(plugin_id.to_string()))?;

        client
            .sender
            .send(message)
            .map_err(|e| RoutingError::SendError(e.to_string()))?;

        Ok(())
    }

    /// すべてのEXEプラグインへメッセージをブロードキャスト
    pub async fn broadcast(&self, message: McvMessage) -> Result<(), RoutingError> {
        tracing::debug!(
            target: "mcv::plugin_exe_manager::MessageRouter",
            message_type = ?message.message_type,
            "Broadcasting message to all EXE plugins"
        );

        let clients = self.clients.read().await;
        let mut errors = Vec::new();

        for (plugin_id, client) in clients.iter() {
            if let Err(e) = client.sender.send(message.clone()) {
                tracing::error!(
                    target: "mcv::plugin_exe_manager::MessageRouter",
                    plugin_id = %plugin_id,
                    error = %e.to_string(),
                    "Failed to broadcast message to plugin"
                );
                errors.push(format!("{}: {}", plugin_id, e));
            }
        }

        if !errors.is_empty() {
            return Err(RoutingError::BroadcastError(format!(
                "Failed to send to {} plugins: {}",
                errors.len(),
                errors.join(", ")
            )));
        }

        tracing::debug!(target: "mcv::plugin_exe_manager::MessageRouter",count = clients.len(), "Broadcast completed");

        Ok(())
    }

    /// 特定のroleを持つEXEプラグインへメッセージをブロードキャスト
    pub async fn broadcast_to_role(
        &self,
        role: &str,
        message: McvMessage,
    ) -> Result<(), RoutingError> {
        tracing::debug!(
            target: "mcv::plugin_exe_manager::MessageRouter",
            role = %role,
            message_type = ?message.message_type,
            "Broadcasting message to plugins with specific role"
        );

        let clients = self.clients.read().await;
        let mut sent_count = 0;
        let mut errors = Vec::new();

        for (plugin_id, client) in clients.iter() {
            if client.roles.contains(&role.to_string()) {
                if let Err(e) = client.sender.send(message.clone()) {
                    tracing::error!(
                        target: "mcv::plugin_exe_manager::MessageRouter",
                        plugin_id = %plugin_id,
                        error = %e.to_string(),
                        "Failed to broadcast message to plugin"
                    );
                    errors.push(format!("{}: {}", plugin_id, e));
                } else {
                    sent_count += 1;
                }
            }
        }

        if !errors.is_empty() {
            return Err(RoutingError::BroadcastError(format!(
                "Failed to send to {} plugins: {}",
                errors.len(),
                errors.join(", ")
            )));
        }

        tracing::debug!(
            role = %role,
            count = sent_count,
            "Role-based broadcast completed"
        );

        Ok(())
    }

    /// メッセージの宛先に基づいて適切にルーティング
    pub async fn route(&self, message: McvMessage) -> Result<(), RoutingError> {
        match &message.dst {
            MessageDestination::Plugin { plugin_id } => {
                // 特定のプラグインへユニキャスト
                self.route_to_plugin(plugin_id.clone(), message).await
            }
            MessageDestination::Core => {
                // Coreへのメッセージはここではルーティングしない
                tracing::warn!("Message to Core should not be routed through MessageRouter");
                Ok(())
            }
            MessageDestination::Broadcast => {
                // 全プラグインへブロードキャスト
                self.broadcast(message).await
            }
        }
    }

    /// 接続中のクライアント数を取得
    pub async fn client_count(&self) -> usize {
        let clients = self.clients.read().await;
        clients.len()
    }

    /// 特定のプラグインが接続しているか確認
    pub async fn is_connected(&self, plugin_id: PluginId) -> bool {
        let clients = self.clients.read().await;
        clients.contains_key(&plugin_id)
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use mcv_messages::{MessageSource, MessageType};

    #[tokio::test]
    async fn test_router_creation() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let _router = MessageRouter::new(clients);
    }

    #[tokio::test]
    async fn test_client_count() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        assert_eq!(router.client_count().await, 0);

        // クライアントを追加
        let plugin_id = PluginId::new("test_plugin_logical_001");
        let (tx, _rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id.clone(),
            ClientInfo {
                plugin_id: plugin_id.clone(),
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        assert_eq!(router.client_count().await, 1);
    }

    #[tokio::test]
    async fn test_is_connected() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = PluginId::new("test_plugin_logical_002");
        assert!(!router.is_connected(plugin_id.clone()).await);

        // クライアントを追加
        let (tx, _rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id.clone(),
            ClientInfo {
                plugin_id: plugin_id.clone(),
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        assert!(router.is_connected(plugin_id).await);
    }

    #[tokio::test]
    async fn test_route_to_plugin() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = PluginId::new("test_plugin_logical_003");
        let (tx, mut rx) = mpsc::unbounded_channel();

        clients.write().await.insert(
            plugin_id.clone(),
            ClientInfo {
                plugin_id: plugin_id.clone(),
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_request(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Plugin {
                plugin_id: plugin_id.clone(),
            },
            serde_json::json!({}),
        );

        router
            .route_to_plugin(plugin_id, message.clone())
            .await
            .unwrap();

        // メッセージが受信されることを確認
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::Connected);
    }

    #[tokio::test]
    async fn test_broadcast() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        // 2つのクライアントを追加
        let plugin_id_1 = PluginId::new("test_plugin_logical_004");
        let plugin_id_2 = PluginId::new("test_plugin_logical_005");

        let (tx1, mut rx1) = mpsc::unbounded_channel();
        let (tx2, mut rx2) = mpsc::unbounded_channel();

        clients.write().await.insert(
            plugin_id_1.clone(),
            ClientInfo {
                plugin_id: plugin_id_1.clone(),
                sender: tx1,
                roles: vec!["test".to_string()],
            },
        );

        clients.write().await.insert(
            plugin_id_2.clone(),
            ClientInfo {
                plugin_id: plugin_id_2.clone(),
                sender: tx2,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::ConnectionAdded,
            MessageSource::Core,
            MessageDestination::Core, // ブロードキャストなので宛先は任意
            serde_json::json!({}),
        );

        router.broadcast(message.clone()).await.unwrap();

        // 両方のクライアントがメッセージを受信
        let received1 = rx1.recv().await.unwrap();
        let received2 = rx2.recv().await.unwrap();

        assert_eq!(received1.message_type, MessageType::ConnectionAdded);
        assert_eq!(received2.message_type, MessageType::ConnectionAdded);
    }
}
