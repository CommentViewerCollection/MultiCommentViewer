use mcv_messages::{Message as McvMessage, MessageDestination};
use std::collections::HashMap;
use std::sync::Arc;
use thiserror::Error;
use tokio::sync::{mpsc, RwLock};
use uuid::Uuid;

#[derive(Debug, Error)]
pub enum RoutingError {
    #[error("Plugin not found: {0}")]
    PluginNotFound(Uuid),

    #[error("Send error: {0}")]
    SendError(String),

    #[error("Broadcast error: {0}")]
    BroadcastError(String),
}

/// WebSocketクライアント情報（routing用）
pub struct ClientInfo {
    pub plugin_id: Uuid,
    pub sender: mpsc::UnboundedSender<McvMessage>,
    pub roles: Vec<String>,
}

/// メッセージルーティング
///
/// Core ↔ EXEプラグイン間のメッセージをルーティングする
pub struct MessageRouter {
    clients: Arc<RwLock<HashMap<Uuid, ClientInfo>>>,
}

impl MessageRouter {
    /// 新しいメッセージルーターを作成
    pub fn new(clients: Arc<RwLock<HashMap<Uuid, ClientInfo>>>) -> Self {
        Self { clients }
    }

    /// 特定のEXEプラグインへメッセージを送信（ユニキャスト）
    pub async fn route_to_plugin(
        &self,
        plugin_id: Uuid,
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
            .ok_or(RoutingError::PluginNotFound(plugin_id))?;

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


    /// 接続中のクライアント数を取得
    pub async fn client_count(&self) -> usize {
        let clients = self.clients.read().await;
        clients.len()
    }

    /// 特定のプラグインが接続しているか確認
    pub async fn is_connected(&self, plugin_id: Uuid) -> bool {
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
        let plugin_id = Uuid::new_v4();
        let (tx, _rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
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

        let plugin_id = Uuid::new_v4();
        assert!(!router.is_connected(plugin_id).await);

        // クライアントを追加
        let (tx, _rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
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

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();

        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_request(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Plugin { plugin_id },
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
        let plugin_id_1 = Uuid::new_v4();
        let plugin_id_2 = Uuid::new_v4();

        let (tx1, mut rx1) = mpsc::unbounded_channel();
        let (tx2, mut rx2) = mpsc::unbounded_channel();

        clients.write().await.insert(
            plugin_id_1,
            ClientInfo {
                plugin_id: plugin_id_1,
                sender: tx1,
                roles: vec!["test".to_string()],
            },
        );

        clients.write().await.insert(
            plugin_id_2,
            ClientInfo {
                plugin_id: plugin_id_2,
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

    // ===== ブロードキャストメッセージタイプ別テスト =====

    #[tokio::test]
    async fn test_broadcast_plugin_added() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::PluginAdded);
    }

    #[tokio::test]
    async fn test_broadcast_plugin_removed() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::PluginRemoved,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::PluginRemoved);
    }

    #[tokio::test]
    async fn test_broadcast_connection_removed() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::ConnectionRemoved,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::ConnectionRemoved);
    }

    #[tokio::test]
    async fn test_broadcast_connected() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::Connected);
    }

    #[tokio::test]
    async fn test_broadcast_disconnected() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::Disconnected,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::Disconnected);
    }

    #[tokio::test]
    async fn test_broadcast_comment_received() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::CommentReceived,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();
        let received = rx.recv().await.unwrap();
        assert_eq!(received.message_type, MessageType::CommentReceived);
    }

    // ===== エラーハンドリングテスト =====

    #[tokio::test]
    async fn test_broadcast_with_closed_channel() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, rx) = mpsc::unbounded_channel::<McvMessage>();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        // チャネルをクローズ
        drop(rx);

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        // エラーが返されることを確認（送信失敗）
        let result = router.broadcast(message).await;
        assert!(result.is_err());
    }

    #[tokio::test]
    async fn test_broadcast_to_empty_clients() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(clients);

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        // クライアントが0でも成功する（エラーにならない）
        let result = router.broadcast(message).await;
        assert!(result.is_ok());
    }

    #[tokio::test]
    async fn test_route_to_plugin_not_found() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(clients);

        let plugin_id = Uuid::new_v4();
        let message = McvMessage::new_request(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Plugin { plugin_id },
            serde_json::json!({}),
        );

        // 存在しないプラグインへのルーティングはエラー
        let result = router.route_to_plugin(plugin_id, message).await;
        assert!(result.is_err());
    }

    #[tokio::test]
    async fn test_route_to_plugin_closed_channel() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, rx) = mpsc::unbounded_channel::<McvMessage>();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        // チャネルをクローズ
        drop(rx);

        let message = McvMessage::new_request(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Plugin { plugin_id },
            serde_json::json!({}),
        );

        // チャネルがクローズされている場合はエラー
        let result = router.route_to_plugin(plugin_id, message).await;
        assert!(result.is_err());
    }

    // ===== ストレステスト =====

    #[tokio::test]
    async fn test_broadcast_multiple_sequential() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        // 10個のメッセージを連続送信
        for i in 0..10 {
            let message = McvMessage::new_notification(
                MessageType::CommentReceived,
                MessageSource::Core,
                MessageDestination::Broadcast,
                serde_json::json!({"index": i}),
            );
            router.broadcast(message).await.unwrap();
        }

        // 10個すべて受信することを確認
        for _ in 0..10 {
            let received = rx.recv().await.unwrap();
            assert_eq!(received.message_type, MessageType::CommentReceived);
        }
    }

    #[tokio::test]
    async fn test_broadcast_with_many_clients() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        let mut receivers = vec![];

        // 50個のクライアントを追加
        for _ in 0..50 {
            let plugin_id = Uuid::new_v4();
            let (tx, rx) = mpsc::unbounded_channel();
            clients.write().await.insert(
                plugin_id,
                ClientInfo {
                    plugin_id,
                    sender: tx,
                    roles: vec!["test".to_string()],
                },
            );
            receivers.push(rx);
        }

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router.broadcast(message).await.unwrap();

        // 50個すべてのクライアントがメッセージを受信
        for mut rx in receivers {
            let received = rx.recv().await.unwrap();
            assert_eq!(received.message_type, MessageType::PluginAdded);
        }
    }

    #[tokio::test]
    async fn test_broadcast_concurrent() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = Arc::new(MessageRouter::new(Arc::clone(&clients)));

        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["test".to_string()],
            },
        );

        // 5個のブロードキャストを並行実行
        let mut handles = vec![];
        for i in 0..5 {
            let router_clone = Arc::clone(&router);
            let handle = tokio::spawn(async move {
                let message = McvMessage::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Core,
                    MessageDestination::Broadcast,
                    serde_json::json!({"index": i}),
                );
                router_clone.broadcast(message).await.unwrap();
            });
            handles.push(handle);
        }

        // すべての並行タスクが完了するのを待つ
        for handle in handles {
            handle.await.unwrap();
        }

        // 5個すべて受信することを確認
        for _ in 0..5 {
            let received = rx.recv().await.unwrap();
            assert_eq!(received.message_type, MessageType::CommentReceived);
        }
    }

    // ===== role-basedブロードキャストテスト =====

    #[tokio::test]
    async fn test_broadcast_to_role_basic() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        // role "comment-provider" のクライアント
        let plugin_id_1 = Uuid::new_v4();
        let (tx1, mut rx1) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id_1,
            ClientInfo {
                plugin_id: plugin_id_1,
                sender: tx1,
                roles: vec!["comment-provider".to_string()],
            },
        );

        // role "other" のクライアント
        let plugin_id_2 = Uuid::new_v4();
        let (tx2, mut rx2) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id_2,
            ClientInfo {
                plugin_id: plugin_id_2,
                sender: tx2,
                roles: vec!["other".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        router
            .broadcast_to_role("comment-provider", message)
            .await
            .unwrap();

        // comment-provider のみがメッセージを受信
        let received1 = rx1.recv().await.unwrap();
        assert_eq!(received1.message_type, MessageType::PluginAdded);

        // other は受信しない（タイムアウト）
        tokio::time::timeout(tokio::time::Duration::from_millis(100), rx2.recv())
            .await
            .expect_err("Should timeout");
    }

    #[tokio::test]
    async fn test_broadcast_to_role_no_matches() {
        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = MessageRouter::new(Arc::clone(&clients));

        // role "other" のクライアントのみ
        let plugin_id = Uuid::new_v4();
        let (tx, mut rx) = mpsc::unbounded_channel();
        clients.write().await.insert(
            plugin_id,
            ClientInfo {
                plugin_id,
                sender: tx,
                roles: vec!["other".to_string()],
            },
        );

        let message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::json!({}),
        );

        // マッチするクライアントがいない場合も成功
        router
            .broadcast_to_role("comment-provider", message)
            .await
            .unwrap();

        // メッセージは受信しない（タイムアウト）
        tokio::time::timeout(tokio::time::Duration::from_millis(100), rx.recv())
            .await
            .expect_err("Should timeout");
    }
}
