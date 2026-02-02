use std::sync::Arc;

use crate::ExePluginError;
use futures_util::{SinkExt, StreamExt};
use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use tokio::sync::mpsc::{self, UnboundedSender};
use tokio_tungstenite::connect_async;
use tungstenite::Message as WsMessage;
use uuid::Uuid;

/// EXEプラグインクライアント
///
/// WebSocket経由でmcvと通信するためのクライアント
pub struct ExePluginClient {
    plugin_id: Uuid,
    tx: UnboundedSender<WsMessage>,
    message_rx: Arc<tokio::sync::Mutex<Option<mpsc::UnboundedReceiver<McvMessage>>>>,
}

impl ExePluginClient {
    /// WebSocketサーバーに接続
    ///
    /// # Arguments
    /// * `url` - WebSocketサーバーのURL (例: "ws://localhost:28901")
    pub async fn connect(url: &str) -> Result<Arc<Self>, ExePluginError> {
        let (ws, _) = connect_async(url)
            .await
            .map_err(|e| ExePluginError::Connection(e.to_string()))?;

        let (write, mut read) = ws.split();
        let (tx, mut rx) = mpsc::unbounded_channel();
        let (message_tx, message_rx) = mpsc::unbounded_channel();

        let client = Arc::new(Self {
            plugin_id: Uuid::new_v4(),
            tx,
            message_rx: Arc::new(tokio::sync::Mutex::new(Some(message_rx))),
        });

        /* write loop */
        {
            let mut write = write;
            tokio::spawn(async move {
                while let Some(msg) = rx.recv().await {
                    println!("sending message: {}", msg);
                    let _ = write.send(msg).await;
                }
            });
        }

        /* read loop */
        {
            tokio::spawn(async move {
                loop {
                    let msg = read.next().await;
                    if msg.is_none() {
                        break;
                    }
                    let msg = msg.unwrap();
                    match msg {
                        Ok(WsMessage::Text(text)) => {
                            tracing::debug!(target: "mcv::plugin_exe_interface",message_text = %text, "Received message");
                            match serde_json::from_str::<McvMessage>(&text) {
                                Ok(mcv_message) => {
                                    tracing::debug!(target: "mcv::plugin_exe_interface",message_type = ?mcv_message.message_type, "Parsed message");
                                    // チャネル経由でメッセージを送信
                                    if let Err(e) = message_tx.send(mcv_message) {
                                        tracing::error!(target: "mcv::plugin_exe_interface",error = %e, "Failed to send message to handler");
                                    }
                                }
                                Err(e) => {
                                    tracing::error!(target: "mcv::plugin_exe_interface",error = %e, text = %text, "Failed to parse message");
                                }
                            }
                        }
                        Ok(WsMessage::Close(_)) => {
                            tracing::info!(target: "mcv::plugin_exe_interface","WebSocket closed");
                            break;
                        }
                        Ok(WsMessage::Ping(data)) => {
                            tracing::trace!(target: "mcv::plugin_exe_interface","Received ping");
                            // Pongは自動的に送信される
                            drop(data);
                        }
                        Ok(WsMessage::Pong(_)) => {
                            tracing::trace!(target: "mcv::plugin_exe_interface","Received pong");
                        }
                        Ok(WsMessage::Binary(_)) => {
                            tracing::warn!(target: "mcv::plugin_exe_interface","Received unexpected binary message");
                        }
                        Ok(WsMessage::Frame(_)) => {
                            tracing::trace!(target: "mcv::plugin_exe_interface","Received frame");
                        }
                        Err(e) => {
                            tracing::error!(target: "mcv::plugin_exe_interface",error = %e, "WebSocket error");
                            return Err(ExePluginError::WebSocket(e.to_string()));
                        }
                    }
                }
                Ok(())
            });
        }

        Ok(client)
    }

    /// plugin_idを取得
    pub fn plugin_id(&self) -> Uuid {
        self.plugin_id
    }

    /// plugin-helloメッセージを送信
    ///
    /// # Arguments
    /// * `name` - プラグイン名
    /// * `roles` - プラグインのロール
    pub async fn send_plugin_hello(
        &self,
        name: &str,
        roles: Vec<&str>,
    ) -> Result<(), ExePluginError> {
        let payload = PluginHelloPayload {
            name: name.to_string(),
            plugin_id: self.plugin_id,
            role: roles.iter().map(|s| s.to_string()).collect(),
            api_version: "v2".to_string(),
        };

        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload)?,
        );

        self.send_message(message)
    }

    /// get-pluginsメッセージを送信
    ///
    /// 既存のプラグイン一覧を取得する
    pub async fn send_get_plugins(&self) -> Result<(), ExePluginError> {
        let message = McvMessage::new(
            MessageType::GetPlugins,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::json!({}),
        );

        self.send_message(message)
    }

    /// メッセージを送信
    pub fn send_message(&self, message: McvMessage) -> Result<(), ExePluginError> {
        let json = serde_json::to_string(&message)?;
        let _ = self.tx.send(WsMessage::Text(json.into()));
        Ok(())
    }

    /// メッセージハンドラーを登録
    ///
    /// ハンドラーは別タスクで実行されるため、RwLockの競合問題を回避できます。
    /// 注意: このメソッドは一度だけ呼ばれることを想定しています。
    pub async fn on_message<F>(&self, handler: F)
    where
        F: Fn(McvMessage) + Send + Sync + 'static,
    {
        tracing::debug!(target: "mcv::plugin_exe_interface", "Registering message handler");

        let handler = Arc::new(handler);
        let mut rx_guard = self.message_rx.lock().await;

        // receiverの所有権を取得（一度のみ）
        let mut rx = match rx_guard.take() {
            Some(rx) => rx,
            None => {
                tracing::error!(target: "mcv::plugin_exe_interface", "Message handler already registered");
                return;
            }
        };

        // ハンドラータスクをspawn
        tokio::spawn(async move {
            while let Some(message) = rx.recv().await {
                let handler_clone = handler.clone();
                // ハンドラーを別タスクで実行（ブロッキングを避ける）
                tokio::spawn(async move {
                    handler_clone(message);
                });
            }
        });
    }
}

#[cfg(test)]
mod tests {
    #[tokio::test]
    #[ignore] // FIXME: ExePluginClient の構造が変更されたため、このテストは再設計が必要
    async fn test_plugin_id_generation() {
        // TODO: ExePluginClient の新しい構造に合わせてテストを書き直す
        // 現在の構造: { plugin_id, message_handler: RwLock<Option<Arc<...>>>, tx }
    }
}
