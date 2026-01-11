use async_trait::async_trait;
use mcv_messages::*;
use mcv_plugin_interface::{Plugin, PluginError, PluginHost};
use rand::{Rng, SeedableRng};
use rand::rngs::StdRng;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use tokio::time::{sleep, Duration};
use uuid::Uuid;

/// ダミープラグイン
///
/// ランダムにコメントを生成するテスト用プラグイン
pub struct DummyPlugin {
    plugin_id: Uuid,
    connection_id: Option<Uuid>,
    is_running: Arc<AtomicBool>,
}

impl DummyPlugin {
    /// 新しいダミープラグインを作成
    pub fn new() -> Self {
        Self {
            plugin_id: Uuid::new_v4(),
            connection_id: None,
            is_running: Arc::new(AtomicBool::new(false)),
        }
    }

    /// コメント生成タスク（静的メソッドとして実装）
    fn spawn_comment_generator(
        plugin_id: Uuid,
        connection_id: Uuid,
        is_running: Arc<AtomicBool>,
        sender: tokio::sync::mpsc::UnboundedSender<Message>,
    ) {
        tokio::spawn(async move {
            let names = vec!["太郎", "花子", "次郎", "さくら", "けん"];
            let texts = vec![
                "こんにちは!",
                "面白い配信ですね",
                "草",
                "888888",
                "次回も楽しみです",
                "いいね!",
                "すごい!",
                "www",
            ];

            let mut rng = StdRng::from_entropy();
            println!("Comment generation started for connection: {}", connection_id);

            while is_running.load(Ordering::SeqCst) {
                // 1-5秒のランダム間隔
                let interval = rng.gen_range(1..=5);
                sleep(Duration::from_secs(interval)).await;

                if !is_running.load(Ordering::SeqCst) {
                    break;
                }

                // ランダムなコメントを生成
                let comment = Comment {
                    id: Uuid::new_v4().to_string(),
                    user_name: names[rng.gen_range(0..names.len())].to_string(),
                    user_id: format!("user_{}", rng.gen_range(1000..9999)),
                    text: texts[rng.gen_range(0..texts.len())].to_string(),
                    timestamp: chrono::Utc::now().timestamp(),
                };

                println!("Generated comment: {} - {}", comment.user_name, comment.text);

                let message = Message::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Plugin { plugin_id },
                    MessageDestination::Core,
                    serde_json::to_value(CommentReceivedPayload {
                        connection_id,
                        comment,
                    })
                    .unwrap(),
                );

                if sender.send(message).is_err() {
                    eprintln!("Failed to send comment - receiver dropped");
                    break;
                }
            }

            println!("Comment generation stopped for connection: {}", connection_id);
        });
    }
}

impl Default for DummyPlugin {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait]
impl Plugin for DummyPlugin {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        println!("=== DummyPlugin::on_loaded called, plugin_id: {} ===", self.plugin_id);

        // plugin-helloを送信
        let message = Message::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(PluginHelloPayload {
                name: "Dummy Plugin".to_string(),
                plugin_id: self.plugin_id,
                role: vec!["dummy".to_string()],
                api_version: "v2".to_string(),
            })
            .unwrap(),
        );

        println!("Sending plugin-hello message...");
        host.send_message(message).await?;
        println!("plugin-hello message sent successfully");

        Ok(())
    }

    async fn on_message(
        &mut self,
        message: Message,
        host: Arc<dyn PluginHost>,
    ) -> Result<(), PluginError> {
        println!("Dummy plugin received message: {:?}", message.message_type);

        match message.message_type {
            MessageType::PluginAdded => {
                println!("Dummy plugin added successfully");
            }
            MessageType::Connect => {
                // connectメッセージからconnection_idを取得
                let payload: ConnectPayload = serde_json::from_value(message.payload.clone())
                    .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to parse connect payload: {}", e)))?;

                let conn_id = payload.connection_id;
                self.connection_id = Some(conn_id);

                println!("Starting comment generation for connection: {}", conn_id);

                self.is_running.store(true, Ordering::SeqCst);

                // connectedを返信
                let response = Message::create_response(
                    &message,
                    MessageType::Connected,
                    serde_json::to_value(ConnectedPayload {
                        connection_id: conn_id,
                    })
                    .unwrap(),
                );

                host.send_message(response.clone()).await?;

                // メッセージチャネルを作成
                let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel();

                // コメント生成タスクを起動
                Self::spawn_comment_generator(
                    self.plugin_id,
                    conn_id,
                    self.is_running.clone(),
                    tx,
                );

                // メッセージ転送タスクを起動（hostをクローンして使用）
                let host_clone = host.clone();
                tokio::spawn(async move {
                    while let Some(msg) = rx.recv().await {
                        println!("Sending comment message: {:?}", msg.message_type);
                        if let Err(e) = host_clone.send_message(msg).await {
                            eprintln!("Failed to send comment message: {}", e);
                            break;
                        }
                    }
                });
            }
            MessageType::Disconnect => {
                // コメント生成を停止
                if let Some(conn_id) = self.connection_id {
                    println!("Stopping comment generation for connection: {}", conn_id);

                    self.is_running.store(false, Ordering::SeqCst);

                    // disconnectedを返信
                    let response = Message::create_response(
                        &message,
                        MessageType::Disconnected,
                        serde_json::to_value(DisconnectedPayload {
                            connection_id: conn_id,
                        })
                        .unwrap(),
                    );

                    host.send_message(response.clone()).await?;
                }
            }
            _ => {
                println!("Unhandled message type: {:?}", message.message_type);
            }
        }

        Ok(())
    }

    async fn on_shutdown(&mut self) -> Result<(), PluginError> {
        println!("Shutting down dummy plugin");
        self.is_running.store(false, Ordering::SeqCst);
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_dummy_plugin_creation() {
        let plugin = DummyPlugin::new();
        assert!(!plugin.is_running.load(Ordering::SeqCst));
        assert!(plugin.connection_id.is_none());
    }
}
