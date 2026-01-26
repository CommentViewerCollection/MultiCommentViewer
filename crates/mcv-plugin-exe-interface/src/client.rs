use crate::{ExePluginError, MessageHandler};
use futures_util::{SinkExt, StreamExt};
use mcv_messages::{Message as McvMessage, MessageSource, MessageDestination, MessageType, PluginHelloPayload};
use tokio::net::TcpStream;
use tokio_tungstenite::{MaybeTlsStream, WebSocketStream, connect_async};
use tungstenite::Message as WsMessage;
use uuid::Uuid;

/// EXEプラグインクライアント
///
/// WebSocket経由でmcvと通信するためのクライアント
pub struct ExePluginClient {
    plugin_id: Uuid,
    websocket: Option<WebSocketStream<MaybeTlsStream<TcpStream>>>,
    message_handler: Option<MessageHandler>,
}

impl ExePluginClient {
    /// WebSocketサーバーに接続
    ///
    /// # Arguments
    /// * `url` - WebSocketサーバーのURL (例: "ws://localhost:28901")
    pub async fn connect(url: &str) -> Result<Self, ExePluginError> {
        tracing::info!(url = %url, "Connecting to MCV WebSocket server");

        let (ws_stream, _) = connect_async(url)
            .await
            .map_err(|e| ExePluginError::Connection(e.to_string()))?;

        tracing::info!("Connected to MCV WebSocket server");

        Ok(Self {
            plugin_id: Uuid::new_v4(),
            websocket: Some(ws_stream),
            message_handler: None,
        })
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
    pub async fn send_plugin_hello(&mut self, name: &str, roles: Vec<&str>) -> Result<(), ExePluginError> {
        tracing::info!(name = %name, "Sending plugin-hello");

        let payload = PluginHelloPayload {
            name: name.to_string(),
            plugin_id: self.plugin_id,
            role: roles.iter().map(|s| s.to_string()).collect(),
            api_version: "v2".to_string(),
        };

        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id: self.plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload)?,
        );

        self.send_message(message).await?;

        tracing::info!("plugin-hello sent");

        Ok(())
    }

    /// メッセージを送信
    pub async fn send_message(&mut self, message: McvMessage) -> Result<(), ExePluginError> {
        let json = serde_json::to_string(&message)?;

        tracing::debug!(message_type = ?message.message_type, "Sending message");

        if let Some(ws) = &mut self.websocket {
            ws.send(WsMessage::Text(json.into()))
                .await
                .map_err(|e| ExePluginError::WebSocket(e.to_string()))?;
        } else {
            return Err(ExePluginError::Connection("WebSocket not connected".to_string()));
        }

        Ok(())
    }

    /// メッセージハンドラーを登録
    pub fn on_message<F>(&mut self, handler: F)
    where
        F: Fn(McvMessage) + Send + Sync + 'static,
    {
        self.message_handler = Some(Box::new(handler));
    }

    /// メッセージループを実行
    ///
    /// WebSocketからメッセージを受信し、ハンドラーを呼び出す
    pub async fn run(&mut self) -> Result<(), ExePluginError> {
        tracing::info!("Starting message loop");

        let ws = self.websocket.take()
            .ok_or_else(|| ExePluginError::Connection("WebSocket not connected".to_string()))?;

        let (_write, mut read) = ws.split();

        while let Some(msg) = read.next().await {
            match msg {
                Ok(WsMessage::Text(text)) => {
                    tracing::debug!(message_text = %text, "Received message");

                    match serde_json::from_str::<McvMessage>(&text) {
                        Ok(mcv_message) => {
                            tracing::debug!(message_type = ?mcv_message.message_type, "Parsed message");

                            if let Some(handler) = &self.message_handler {
                                handler(mcv_message);
                            }
                        }
                        Err(e) => {
                            tracing::error!(error = %e, text = %text, "Failed to parse message");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::info!("WebSocket closed");
                    break;
                }
                Ok(WsMessage::Ping(data)) => {
                    tracing::trace!("Received ping");
                    // Pongは自動的に送信される
                    drop(data);
                }
                Ok(WsMessage::Pong(_)) => {
                    tracing::trace!("Received pong");
                }
                Ok(WsMessage::Binary(_)) => {
                    tracing::warn!("Received unexpected binary message");
                }
                Ok(WsMessage::Frame(_)) => {
                    tracing::trace!("Received frame");
                }
                Err(e) => {
                    tracing::error!(error = %e, "WebSocket error");
                    return Err(ExePluginError::WebSocket(e.to_string()));
                }
            }
        }

        tracing::info!("Message loop ended");

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn test_plugin_id_generation() {
        // 接続先がないのでエラーになるが、plugin_idは生成される
        let client = ExePluginClient {
            plugin_id: Uuid::new_v4(),
            websocket: None,
            message_handler: None,
        };

        assert!(!client.plugin_id().is_nil());
    }
}
