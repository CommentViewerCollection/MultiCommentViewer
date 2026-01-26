use mcv_messages::{Message as McvMessage, MessageType, PluginHelloPayload};
use mcv_plugin_interface::PluginHost;
use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::{mpsc, RwLock};
use futures_util::{SinkExt, StreamExt};
use tokio_tungstenite::tungstenite::Message as WsMessage;
use uuid::Uuid;
use thiserror::Error;

#[derive(Debug, Error)]
pub enum WebSocketError {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("WebSocket error: {0}")]
    WebSocket(String),

    #[error("Plugin not found: {0}")]
    PluginNotFound(Uuid),

    #[error("Send error: {0}")]
    SendError(String),
}

/// WebSocketサーバー
pub struct WebSocketServer {
    port: u16,
    clients: Arc<RwLock<HashMap<Uuid, WebSocketClient>>>,
    host: Arc<dyn PluginHost>,
    shutdown_tx: Option<mpsc::Sender<()>>,
}

/// WebSocketクライアント（EXEプラグイン）
struct WebSocketClient {
    plugin_id: Uuid,
    sender: mpsc::UnboundedSender<McvMessage>,
}

impl WebSocketServer {
    /// 新しいWebSocketサーバーを作成し、起動する
    pub async fn new(addr: &str, host: Arc<dyn PluginHost>) -> Result<Self, WebSocketError> {
        tracing::info!(addr = %addr, "Starting WebSocket server");

        // アドレスをパース
        let socket_addr: SocketAddr = addr.parse()
            .map_err(|e| WebSocketError::WebSocket(format!("Invalid address: {}", e)))?;

        let listener = TcpListener::bind(&socket_addr).await?;
        let actual_addr = listener.local_addr()?;
        let port = actual_addr.port();

        tracing::info!(port = port, "WebSocket server listening");

        let clients = Arc::new(RwLock::new(HashMap::new()));
        let (shutdown_tx, shutdown_rx) = mpsc::channel(1);

        // サーバーループを起動
        let clients_clone = Arc::clone(&clients);
        let host_clone = Arc::clone(&host);
        tokio::spawn(async move {
            Self::server_loop(listener, clients_clone, host_clone, shutdown_rx).await;
        });

        Ok(Self {
            port,
            clients,
            host,
            shutdown_tx: Some(shutdown_tx),
        })
    }

    /// サーバーループ
    async fn server_loop(
        listener: TcpListener,
        clients: Arc<RwLock<HashMap<Uuid, WebSocketClient>>>,
        host: Arc<dyn PluginHost>,
        mut shutdown_rx: mpsc::Receiver<()>,
    ) {
        loop {
            tokio::select! {
                result = listener.accept() => {
                    match result {
                        Ok((stream, addr)) => {
                            tracing::info!(addr = %addr, "New WebSocket connection");
                            let clients_clone = Arc::clone(&clients);
                            let host_clone = Arc::clone(&host);
                            tokio::spawn(async move {
                                if let Err(e) = Self::handle_connection(stream, clients_clone, host_clone).await {
                                    tracing::error!(error = %e, "Connection handler error");
                                }
                            });
                        }
                        Err(e) => {
                            tracing::error!(error = %e, "Failed to accept connection");
                        }
                    }
                }
                _ = shutdown_rx.recv() => {
                    tracing::info!("WebSocket server shutting down");
                    break;
                }
            }
        }
    }

    /// クライアント接続を処理
    async fn handle_connection(
        stream: TcpStream,
        clients: Arc<RwLock<HashMap<Uuid, WebSocketClient>>>,
        host: Arc<dyn PluginHost>,
    ) -> Result<(), WebSocketError> {
        let ws_stream = tokio_tungstenite::accept_async(stream)
            .await
            .map_err(|e| WebSocketError::WebSocket(e.to_string()))?;

        tracing::debug!("WebSocket connection established");

        let (mut ws_sender, mut ws_receiver) = ws_stream.split();
        let (tx, mut rx) = mpsc::unbounded_channel::<McvMessage>();

        let mut plugin_id: Option<Uuid> = None;

        // 送信タスク
        let send_task = tokio::spawn(async move {
            while let Some(message) = rx.recv().await {
                let json = match serde_json::to_string(&message) {
                    Ok(j) => j,
                    Err(e) => {
                        tracing::error!(error = %e, "Failed to serialize message");
                        continue;
                    }
                };

                if let Err(e) = ws_sender.send(WsMessage::Text(json.into())).await {
                    tracing::error!(error = %e, "Failed to send message");
                    break;
                }
            }
        });

        // 受信ループ
        while let Some(msg) = ws_receiver.next().await {
            match msg {
                Ok(WsMessage::Text(text)) => {
                    tracing::debug!(message_text = %text, "Received message from EXE plugin");

                    match serde_json::from_str::<McvMessage>(&text) {
                        Ok(mcv_message) => {
                            // plugin-helloの場合は登録
                            if mcv_message.message_type == MessageType::PluginHello {
                                if let Ok(payload) = serde_json::from_value::<PluginHelloPayload>(mcv_message.payload.clone()) {
                                    plugin_id = Some(payload.plugin_id);

                                    let client = WebSocketClient {
                                        plugin_id: payload.plugin_id,
                                        sender: tx.clone(),
                                    };

                                    clients.write().await.insert(payload.plugin_id, client);

                                    tracing::info!(
                                        plugin_id = %payload.plugin_id,
                                        plugin_name = %payload.name,
                                        "EXE plugin registered"
                                    );
                                }
                            }

                            // Coreにメッセージをフォワード
                            if let Err(e) = host.send_message(mcv_message).await {
                                tracing::error!(error = %e, "Failed to forward message to Core");
                            }
                        }
                        Err(e) => {
                            tracing::error!(error = %e, text = %text, "Failed to parse message");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::info!("WebSocket closed by client");
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
                    break;
                }
            }
        }

        // クライアントを登録解除
        if let Some(pid) = plugin_id {
            clients.write().await.remove(&pid);
            tracing::info!(plugin_id = %pid, "EXE plugin disconnected");
        }

        // 送信タスクをキャンセル
        send_task.abort();

        Ok(())
    }

    /// WebSocketサーバーのポート番号を取得
    pub fn get_port(&self) -> u16 {
        self.port
    }

    /// EXEプラグインへメッセージを送信
    pub async fn send_to_plugin(&self, plugin_id: Uuid, message: McvMessage) -> Result<(), WebSocketError> {
        tracing::debug!(plugin_id = %plugin_id, message_type = ?message.message_type, "Sending message to EXE plugin");

        let clients = self.clients.read().await;
        let client = clients.get(&plugin_id)
            .ok_or(WebSocketError::PluginNotFound(plugin_id))?;

        client.sender.send(message)
            .map_err(|e| WebSocketError::SendError(e.to_string()))?;

        Ok(())
    }

    /// WebSocketサーバーをシャットダウン
    pub async fn shutdown(&self) -> Result<(), WebSocketError> {
        tracing::info!("WebSocketServer::shutdown called");

        // シャットダウンシグナルを送信
        if let Some(tx) = &self.shutdown_tx {
            let _ = tx.send(()).await;
        }

        // 接続中のクライアントを切断
        let mut clients = self.clients.write().await;
        clients.clear();

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_websocket_client_creation() {
        let (tx, _rx) = mpsc::unbounded_channel();
        let client = WebSocketClient {
            plugin_id: Uuid::new_v4(),
            sender: tx,
        };

        assert!(!client.plugin_id.is_nil());
    }
}
