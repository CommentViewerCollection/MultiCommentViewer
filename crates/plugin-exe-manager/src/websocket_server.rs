use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::PluginHost;
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
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
}

/// WebSocketクライアント（EXEプラグイン）
struct WebSocketClient {
    plugin_id: Uuid,
    // TODO: WebSocketストリームを保持
}

impl WebSocketServer {
    /// 新しいWebSocketサーバーを作成し、起動する
    pub async fn new(addr: &str, host: Arc<dyn PluginHost>) -> Result<Self, WebSocketError> {
        // TODO: 実際のWebSocketサーバーを起動
        // 現在はスタブ実装
        tracing::info!(addr = %addr, "WebSocketServer::new called");

        let port = addr.split(':').last()
            .and_then(|p| p.parse().ok())
            .unwrap_or(28901);

        Ok(Self {
            port,
            clients: Arc::new(RwLock::new(HashMap::new())),
            host,
        })
    }

    /// WebSocketサーバーのポート番号を取得
    pub fn get_port(&self) -> u16 {
        self.port
    }

    /// EXEプラグインへメッセージを送信
    pub async fn send_to_plugin(&self, plugin_id: Uuid, message: McvMessage) -> Result<(), WebSocketError> {
        tracing::debug!(plugin_id = %plugin_id, message_type = ?message.message_type, "Sending message to EXE plugin");

        let clients = self.clients.read().await;
        let _client = clients.get(&plugin_id)
            .ok_or(WebSocketError::PluginNotFound(plugin_id))?;

        // TODO: 実際にWebSocket経由でメッセージを送信

        Ok(())
    }

    /// WebSocketサーバーをシャットダウン
    pub async fn shutdown(&self) -> Result<(), WebSocketError> {
        tracing::info!("WebSocketServer::shutdown called");

        // TODO: 接続中のクライアントを切断
        let mut clients = self.clients.write().await;
        clients.clear();

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    // TODO: テストを追加
}
