pub mod client;

pub use client::ExePluginClient;

use mcv_messages::Message as McvMessage;
use thiserror::Error;

#[derive(Debug, Error)]
pub enum ExePluginError {
    #[error("WebSocket error: {0}")]
    WebSocket(String),

    #[error("Connection error: {0}")]
    Connection(String),

    #[error("Message error: {0}")]
    Message(String),

    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("JSON error: {0}")]
    Json(#[from] serde_json::Error),

    #[error("Other error: {0}")]
    Other(String),
}

/// メッセージハンドラー
pub type MessageHandler = Box<dyn Fn(McvMessage) + Send + Sync>;

#[cfg(test)]
mod tests {
    #[test]
    fn test_exe_plugin_error_creation() {
        let _error = super::ExePluginError::Connection("test".to_string());
    }
}
