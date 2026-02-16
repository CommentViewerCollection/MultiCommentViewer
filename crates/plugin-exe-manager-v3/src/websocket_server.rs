use crate::routing::{ClientInfo, MessageRouter};
use futures_util::{SinkExt, StreamExt};
use mcv_messages::{Message as McvMessage, MessageType, PluginHelloPayload};
use mcv_plugin_interface::PluginHost;
use std::collections::HashMap;
use std::net::SocketAddr;
use std::sync::Arc;
use thiserror::Error;
use tokio::net::{TcpListener, TcpStream};
use tokio::sync::{RwLock, mpsc};
use tokio_tungstenite::tungstenite::Message as WsMessage;
use uuid::Uuid;

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
    clients: Arc<RwLock<HashMap<Uuid, ClientInfo>>>,
    router: Arc<MessageRouter>,
    shutdown_tx: Option<mpsc::Sender<()>>,
    /// plugin-hello受信時のコールバック (internal_physical_plugin_id, logical_plugin_id)
    on_plugin_registered: Arc<RwLock<Option<Arc<dyn Fn(Uuid, Uuid) + Send + Sync>>>>,
}

impl WebSocketServer {
    /// 新しいWebSocketサーバーを作成し、起動する
    pub async fn new(addr: &str, host: Arc<dyn PluginHost>) -> Result<Self, WebSocketError> {
        println!(
            "=== WebSocketServer: Starting WebSocket server on {} ===",
            addr
        );
        tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer",addr = %addr, "Starting WebSocket server");

        // アドレスをパース
        let socket_addr: SocketAddr = addr
            .parse()
            .map_err(|e| WebSocketError::WebSocket(format!("Invalid address: {}", e)))?;

        let listener = TcpListener::bind(&socket_addr).await?;
        let actual_addr = listener.local_addr()?;
        let port = actual_addr.port();

        println!("=== WebSocketServer: Listening on port {} ===", port);
        tracing::info!(
            target: "mcv::plugin-exe-manager::WebSocketServer",
            port = port,
            "WebSocket server listening"
        );

        let clients = Arc::new(RwLock::new(HashMap::new()));
        let router = Arc::new(MessageRouter::new(Arc::clone(&clients)));
        let (shutdown_tx, shutdown_rx) = mpsc::channel(1);
        let on_plugin_registered = Arc::new(RwLock::new(None));

        // サーバーループを起動
        let clients_clone = Arc::clone(&clients);
        let host_clone = Arc::clone(&host);
        let on_plugin_registered_clone = Arc::clone(&on_plugin_registered);
        tokio::spawn(async move {
            Self::server_loop(
                listener,
                clients_clone,
                host_clone,
                on_plugin_registered_clone,
                shutdown_rx,
            )
            .await;
        });

        Ok(Self {
            port,
            clients,
            router,
            shutdown_tx: Some(shutdown_tx),
            on_plugin_registered,
        })
    }

    /// サーバーループ
    async fn server_loop(
        listener: TcpListener,
        clients: Arc<RwLock<HashMap<Uuid, ClientInfo>>>,
        host: Arc<dyn PluginHost>,
        on_plugin_registered: Arc<RwLock<Option<Arc<dyn Fn(Uuid, Uuid) + Send + Sync>>>>,
        mut shutdown_rx: mpsc::Receiver<()>,
    ) {
        loop {
            tokio::select! {
                result = listener.accept() => {
                    match result {
                        Ok((stream, addr)) => {
                            println!("=== WebSocketServer: New connection from {} ===", addr);
                            tracing::info!(target:"mcv::plugin-exe-manager::WebSocketServer",addr = %addr, "New WebSocket connection");
                            let clients_clone = Arc::clone(&clients);
                            let host_clone = Arc::clone(&host);
                            let on_plugin_registered_clone = Arc::clone(&on_plugin_registered);
                            tokio::spawn(async move {
                                if let Err(e) = Self::handle_connection(stream, clients_clone, host_clone, on_plugin_registered_clone).await {
                                    println!("=== WebSocketServer: Connection handler error: {} ===", e);
                                    tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "Connection handler error");
                                }
                            });
                        }
                        Err(e) => {
                            println!("=== WebSocketServer: Failed to accept connection: {} ===", e);
                            tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "Failed to accept connection");
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
        clients: Arc<RwLock<HashMap<Uuid, ClientInfo>>>,
        host: Arc<dyn PluginHost>,
        on_plugin_registered: Arc<RwLock<Option<Arc<dyn Fn(Uuid, Uuid) + Send + Sync>>>>,
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

        let mut plugin_id: Option<Uuid> = None;

        // 送信タスク
        let send_task = tokio::spawn(async move {
            tracing::debug!(target:"mcv::plugin-exe-manager::WebSocketServer","送信タスクには入ってる");
            while let Some(message) = rx.recv().await {
                tracing::debug!(target: "mcv::plugin-exe-manager::WebSocketServer",message=?message, "送信用のデータが来た");
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
                            println!(
                                "=== WebSocketServer: Received message, type: {:?} ===",
                                mcv_message.message_type
                            );

                            // plugin-helloの場合は登録
                            if mcv_message.message_type == MessageType::PluginHello {
                                if let Ok(payload) = serde_json::from_value::<PluginHelloPayload>(
                                    mcv_message.payload.clone(),
                                ) {
                                    let internal_physical_plugin_id = payload.plugin_id; // WebSocket接続のID

                                    // mcv_messageのsrcからlogical_plugin_idを取得
                                    let logical_plugin_id = match &mcv_message.src {
                                        mcv_messages::MessageSource::Plugin { plugin_id } => {
                                            *plugin_id
                                        }
                                        _ => payload.plugin_id, // フォールバック
                                    };

                                    plugin_id = Some(internal_physical_plugin_id);

                                    let client = ClientInfo {
                                        plugin_id: internal_physical_plugin_id,
                                        sender: tx.clone(),
                                        roles: payload.role.clone(),
                                    };

                                    clients
                                        .write()
                                        .await
                                        .insert(internal_physical_plugin_id, client);

                                    println!(
                                        "=== WebSocketServer: EXE plugin registered, internal_id: {}, logical_id: {}, name: {} ===",
                                        internal_physical_plugin_id,
                                        logical_plugin_id,
                                        payload.name
                                    );
                                    tracing::info!(
                                        target:"mcv::plugin-exe-manager::WebSocketServer",
                                        internal_physical_plugin_id = %internal_physical_plugin_id,
                                        logical_plugin_id = %logical_plugin_id,
                                        plugin_name = %payload.name,
                                        roles = ?payload.role,
                                        "EXE plugin registered"
                                    );

                                    // コールバック呼び出し
                                    let callback_guard = on_plugin_registered.read().await;
                                    if let Some(callback) = callback_guard.as_ref() {
                                        callback(internal_physical_plugin_id, logical_plugin_id);
                                        tracing::debug!(
                                            target:"mcv::plugin-exe-manager::WebSocketServer",
                                            "plugin-hello callback invoked"
                                        );
                                    }
                                }
                            }

                            // Coreにメッセージをフォワード
                            println!("=== WebSocketServer: Forwarding message to Core ===");
                            if let Err(e) = host.send_message(mcv_message).await {
                                println!(
                                    "=== WebSocketServer: Failed to forward message to Core: {} ===",
                                    e
                                );
                                tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, "Failed to forward message to Core");
                            } else {
                                println!(
                                    "=== WebSocketServer: Message forwarded to Core successfully ==="
                                );
                            }
                        }
                        Err(e) => {
                            tracing::error!(target:"mcv::plugin-exe-manager::WebSocketServer",error = %e, text = %text, "Failed to parse message");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::info!("WebSocket closed by client");
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

    /// plugin-hello受信時のコールバックを設定
    pub async fn set_on_plugin_registered<F>(&self, callback: F)
    where
        F: Fn(Uuid, Uuid) + Send + Sync + 'static,
    {
        let mut cb = self.on_plugin_registered.write().await;
        *cb = Some(Arc::new(callback));
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
        let client = ClientInfo {
            plugin_id: Uuid::new_v4(),
            sender: tx,
            roles: vec!["test".to_string()],
        };

        assert!(!client.plugin_id.is_nil());
    }
}
