use async_trait::async_trait;
use mcv_messages::Message;
use std::sync::Arc;

/// プラグインエラー
#[derive(Debug, thiserror::Error)]
pub enum PluginError {
    #[error("プラグインの初期化に失敗しました: {0}")]
    InitializationFailed(String),

    #[error("メッセージ処理に失敗しました: {0}")]
    MessageHandlingFailed(String),

    #[error("接続エラー: {0}")]
    ConnectionError(String),

    #[error("その他のエラー: {0}")]
    Other(String),
}

/// プラグインが実装すべきインターフェース
#[async_trait]
pub trait Plugin: Send + Sync {
    /// プラグイン読み込み時に呼ばれる
    ///
    /// このメソッド内でplugin-helloメッセージを送信することが期待される
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError>;

    /// coreからメッセージを受信したときに呼ばれる
    async fn on_message(
        &mut self,
        message: Message,
        host: Arc<dyn PluginHost>,
    ) -> Result<(), PluginError>;

    /// プラグイン終了時に呼ばれる
    async fn on_shutdown(&mut self) -> Result<(), PluginError>;
}

/// Plugin-Hostが提供するインターフェース
///
/// プラグインからcoreへメッセージを送信するために使用
#[async_trait]
pub trait PluginHost: Send + Sync {
    /// coreへメッセージを送信
    async fn send_message(&self, message: Message) -> Result<(), PluginError>;
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_plugin_error() {
        let error = PluginError::InitializationFailed("test".to_string());
        assert!(error.to_string().contains("初期化に失敗"));
    }
}
