use crate::routing::{ClientInfo, MessageRouter};
use futures_util::{SinkExt, StreamExt};
use mcv_messages::{Message as McvMessage, MessageType, PluginHelloPayload, PluginId};
use mcv_plugin_interface::PluginHost;
use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use thiserror::Error;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::{RwLock, mpsc};
use tokio_tungstenite::tungstenite::Message as WsMessage;

#[derive(Debug, Error)]
pub enum WebSocketError {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("WebSocket error: {0}")]
    WebSocket(String),

    #[error("Plugin not found: {0}")]
    PluginNotFound(String),

    #[error("Send error: {0}")]
    SendError(String),
}

/// WebSocketサーバー
pub struct WebSocketServer {
    port: u16,
    clients: Arc<RwLock<HashMap<PluginId, ClientInfo>>>,
    router: Arc<MessageRouter>,
    #[allow(dead_code)]
    host: Arc<dyn PluginHost>,
    shutdown_tx: Option<mpsc::Sender<()>>,
}

impl WebSocketServer {
    /// 新しいWebSocketサーバーを作成し、起動する
    pub async fn new(addr: &str, host: Arc<dyn PluginHost>) -> Result<Self, WebSocketError> {
        tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer", addr = %addr, "WebSocket サーバー起動中");

        // アドレスをパース
        let socket_addr: SocketAddr = addr
            .parse()
            .map_err(|e| WebSocketError::WebSocket(format!("Invalid address: {}", e)))?;

        let listener = TcpListener::bind(&socket_addr).await?;
        let actual_addr = listener.local_addr()?;
        let port = actual_addr.port();

        tracing::info!(
            target: "mcv::plugin-exe-manager::WebSocketServer",
            port = port,
            "WebSocket サーバー起動完了"
        );

        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = Arc::new(MessageRouter::new(Arc::clone(&clients)));
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
            router,
            host,
            shutdown_tx: Some(shutdown_tx),
        })
    }

    /// サーバーループ
    async fn server_loop(
        listener: TcpListener,
        clients: Arc<RwLock<HashMap<PluginId, ClientInfo>>>,
        host: Arc<dyn PluginHost>,
        mut shutdown_rx: mpsc::Receiver<()>,
    ) {
        loop {
            tokio::select! {
                result = listener.accept() => {
                    match result {
                        Ok((stream, addr)) => {
                            tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer", addr = %addr, "新しい WebSocket 接続");
                            let clients_clone = Arc::clone(&clients);
                            let host_clone = Arc::clone(&host);
                            tokio::spawn(async move {
                                if let Err(e) = Self::handle_connection(stream, clients_clone, host_clone).await {
                                    tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer", error = %e, "接続ハンドラエラー");
                                }
                            });
                        }
                        Err(e) => {
                            tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer", error = %e, "接続受け入れ失敗");
                        }
                    }
                }
                _ = shutdown_rx.recv() => {
                    tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer","WebSocket server shutting down");
                    break;
                }
            }
        }
    }

    /// クライアント接続を処理
    async fn handle_connection(
        stream: TcpStream,
        clients: Arc<RwLock<HashMap<PluginId, ClientInfo>>>,
        host: Arc<dyn PluginHost>,
    ) -> Result<(), WebSocketError> {
        let ws_stream = tokio_tungstenite::accept_async(stream)
            .await
            .map_err(|e| WebSocketError::WebSocket(e.to_string()))?;

        tracing::debug!(
            target: "mcv::plugin-exe-manager::WebSocketServer",
            "WebSocket connection established"
        );

        let (mut ws_sender, mut ws_receiver) = ws_stream.split();
        let (tx, mut rx) = mpsc::unbounded_channel::<McvMessage>();

        let mut plugin_id: Option<PluginId> = None;

        // 送信タスク
        let send_task = tokio::spawn(async move {
            while let Some(message) = rx.recv().await {
                tracing::trace!(target: "mcv::plugin-exe-manager::WebSocketServer", message_type = ?message.message_type, "EXE プラグインへ送信");
                let json = match serde_json::to_string(&message) {
                    Ok(j) => j,
                    Err(e) => {
                        tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "Failed to serialize message");
                        continue;
                    }
                };

                if let Err(e) = ws_sender.send(WsMessage::Text(json.into())).await {
                    tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "Failed to send message");
                    break;
                }
            }
        });

        // 受信ループ
        while let Some(msg) = ws_receiver.next().await {
            match msg {
                Ok(WsMessage::Text(text)) => {
                    tracing::debug!(target:"mcv::plugin-exe-manager::WebSocketServer",message_text = %text, "Received message from EXE plugin");

                    match serde_json::from_str::<McvMessage>(&text) {
                        Ok(mcv_message) => {
                            // plugin-helloの場合は登録
                            if mcv_message.message_type == MessageType::PluginHello
                                && let Ok(payload) = serde_json::from_value::<PluginHelloPayload>(
                                    mcv_message.payload.clone(),
                                )
                            {
                                plugin_id = Some(payload.plugin_id.clone());

                                let client = ClientInfo {
                                    plugin_id: payload.plugin_id.clone(),
                                    sender: tx.clone(),
                                    roles: payload.role.clone(),
                                };

                                clients
                                    .write()
                                    .await
                                    .insert(payload.plugin_id.clone(), client);

                                tracing::info!(
                                    target:"mcv::plugin-exe-manager::WebSocketServer",
                                    plugin_id = %payload.plugin_id,
                                    plugin_name = %payload.name,
                                    roles = ?payload.role,
                                    "EXE プラグイン登録完了"
                                );
                            }

                            // Coreにメッセージをフォワード
                            if let Err(e) = host.send_message(mcv_message).await {
                                tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer", error = %e, "Core へのメッセージ転送失敗");
                            }
                        }
                        Err(e) => {
                            tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer", error = %e, text = %text, "メッセージパース失敗");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::info!(target: "mcv::plugin-exe-manager::WebSocketServer", "EXE プラグインが切断");
                    break;
                }
                Ok(WsMessage::Ping(data)) => {
                    tracing::trace!(
                        target: "mcv::plugin-exe-manager::WebSocketServer",
                        "Received ping"
                    );
                    // Pongは自動的に送信される
                    drop(data);
                }
                Ok(WsMessage::Pong(_)) => {
                    tracing::trace!(
                        target: "mcv::plugin-exe-manager::WebSocketServer",
                        "Received pong"
                    );
                }
                Ok(WsMessage::Binary(_)) => {
                    tracing::warn!(
                        target: "mcv::plugin-exe-manager::WebSocketServer",
                        "Received unexpected binary message"
                    );
                }
                Ok(WsMessage::Frame(_)) => {
                    tracing::trace!(
                        target: "mcv::plugin-exe-manager::WebSocketServer",
                        "Received frame"
                    );
                }
                Err(e) => {
                    tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "WebSocket error");
                    break;
                }
            }
        }

        // クライアントを登録解除
        if let Some(pid) = plugin_id {
            clients.write().await.remove(&pid);
            tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer",plugin_id = %pid, "EXE plugin disconnected");
        }

        // 送信タスクをキャンセル
        send_task.abort();

        Ok(())
    }

    /// WebSocketサーバーのポート番号を取得
    pub fn get_port(&self) -> u16 {
        self.port
    }

    /// MessageRouterへの参照を取得
    pub fn get_router(&self) -> &MessageRouter {
        &self.router
    }

    /// EXEプラグインへメッセージを送信（レガシー互換性のため残す）
    pub async fn send_to_plugin(
        &self,
        plugin_id: PluginId,
        message: McvMessage,
    ) -> Result<(), WebSocketError> {
        self.router
            .route_to_plugin(plugin_id, message)
            .await
            .map_err(|e| WebSocketError::SendError(e.to_string()))
    }

    /// WebSocketサーバーをシャットダウン
    pub async fn shutdown(&self) -> Result<(), WebSocketError> {
        tracing::info!(
            target: "mcv::plugin-exe-manager::WebSocketServer",
            "WebSocketServer::shutdown called"
        );

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
        let plugin_id = PluginId::new("test_plugin_logical_001");
        let client = ClientInfo {
            plugin_id: plugin_id.clone(),
            sender: tx,
            roles: vec!["test".to_string()],
        };

        assert_eq!(client.plugin_id, plugin_id);
    }
}
