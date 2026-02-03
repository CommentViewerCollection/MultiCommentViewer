pub mod manifest;
pub mod process_manager;
pub mod routing;
pub mod websocket_server;

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use mcv_plugin_interface::{PluginError, PluginHost};
use plugin_abi_helper::v3::prelude::*;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use process_manager::ProcessManager;
use websocket_server::WebSocketServer;

/// PluginContext を PluginHost に変換するアダプター
struct PluginContextAdapter {
    ctx: PluginContext,
    _plugin_id: Uuid,  // 将来的な拡張用に保持
}

impl PluginContextAdapter {
    fn new(ctx: PluginContext, plugin_id: Uuid) -> Self {
        Self { ctx, _plugin_id: plugin_id }
    }
}

#[async_trait::async_trait]
impl PluginHost for PluginContextAdapter {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        let json = serde_json::to_vec(&message)
            .map_err(|e| PluginError::MessageHandlingFailed(e.to_string()))?;
        self.ctx.send_message(&json).await;
        Ok(())
    }
}

/// EXEプラグインマネージャー（v3実装）
///
/// WebSocketサーバーを起動し、EXEプラグインとの通信を仲介する
pub struct ExePluginManagerV3Impl {
    plugin_id: Uuid,
    websocket_server: Option<Arc<WebSocketServer>>,
    process_manager: Option<Arc<RwLock<ProcessManager>>>,
}

impl ExePluginManagerV3Impl {
    pub fn new() -> Self {
        Self {
            plugin_id: Uuid::nil(),  // on_loadedで設定される
            websocket_server: None,
            process_manager: None,
        }
    }

    /// WebSocketサーバーとプロセスマネージャーを初期化
    async fn initialize(&mut self, ctx: PluginContext) -> Result<(), String> {
        // PluginContextAdapterを作成
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone(), self.plugin_id));

        // WebSocketサーバーを起動（ポート競合時は自動的に次のポートを試行）
        let mut websocket_server = None;
        let mut last_error = None;

        for port in 28901..28911 {
            let addr = format!("127.0.0.1:{}", port);
            match WebSocketServer::new(&addr, Arc::clone(&adapter) as Arc<dyn PluginHost>).await {
                Ok(server) => {
                    tracing::info!(port, "WebSocket server started");
                    websocket_server = Some(Arc::new(server));
                    break;
                }
                Err(e) => {
                    tracing::warn!(port, error = %e, "Port unavailable, trying next");
                    last_error = Some(e);
                }
            }
        }

        let websocket_server = websocket_server.ok_or_else(|| {
            format!(
                "Failed to start WebSocket server: {}",
                last_error.map(|e| e.to_string()).unwrap_or_else(|| "unknown".to_string())
            )
        })?;

        self.websocket_server = Some(websocket_server);

        // プロセスマネージャーを初期化
        let process_manager =
            ProcessManager::new(self.websocket_server.as_ref().unwrap().get_port())
                .await
                .map_err(|e| e.to_string())?;

        self.process_manager = Some(Arc::new(RwLock::new(process_manager)));

        Ok(())
    }

    /// メッセージタイプがブロードキャストすべきかどうかを判定
    fn should_broadcast(message_type: &MessageType) -> bool {
        matches!(
            message_type,
            MessageType::PluginAdded
                | MessageType::PluginRemoved
                | MessageType::ConnectionAdded
                | MessageType::ConnectionRemoved
                | MessageType::Connected
                | MessageType::Disconnected
                | MessageType::CommentReceived
        )
    }
}

impl Default for ExePluginManagerV3Impl {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for ExePluginManagerV3Impl {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        // PluginContextから直接Uuidを取得
        self.plugin_id = ctx.plugin_uuid();

        tracing::info!(plugin_id = %self.plugin_id, "ExePluginManager v3 loaded");

        // mcv-tracing初期化
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone(), self.plugin_id));
        if let Err(e) = mcv_tracing::init_tracing(
            self.plugin_id,
            adapter.clone(),
            env!("CARGO_PKG_VERSION"),
            "trace",
        ) {
            tracing::error!(error = %e, "Failed to init tracing");
            return;
        }

        // 初期化
        if let Err(e) = self.initialize(ctx.clone()).await {
            tracing::error!(error = %e, "Initialization failed");
            return;
        }

        // plugin-hello送信
        let hello_payload = PluginHelloPayload {
            name: "EXE Plugin Manager".to_string(),
            plugin_id: self.plugin_id,
            role: vec!["exe-plugin-manager".to_string()],
            api_version: "v3".to_string(),
        };

        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );

        let json = serde_json::to_vec(&message).unwrap();
        ctx.send_message(&json).await;

        tracing::info!("ExePluginManager initialized and plugin-hello sent");
    }

    async fn on_message(&mut self, _ctx: PluginContext, msg: &[u8]) {
        // バイト列をMcvMessageに変換
        let message: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse message");
                return;
            }
        };

        // 既存のルーティングロジック
        if let Some(websocket_server) = &self.websocket_server {
            let router = websocket_server.get_router();

            if Self::should_broadcast(&message.message_type) {
                if let Err(e) = router.broadcast(message).await {
                    tracing::error!(error = %e, "Broadcast failed");
                }
            } else {
                match &message.dst {
                    MessageDestination::Plugin { plugin_id } => {
                        if let Err(e) = router.route_to_plugin(*plugin_id, message).await {
                            tracing::error!(error = %e, "Route to plugin failed");
                        }
                    }
                    MessageDestination::Core => {
                        // Coreへのメッセージはルーティング不要
                    }
                    MessageDestination::Broadcast => {
                        // ブロードキャストは上で処理済み
                    }
                }
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!("ExePluginManager shutting down");

        // ProcessManager shutdown
        if let Some(process_manager) = &self.process_manager {
            let mut pm = process_manager.write().await;
            if let Err(e) = pm.shutdown().await {
                tracing::error!(error = %e, "ProcessManager shutdown failed");
            }
        }

        // WebSocketServer shutdown
        if let Some(websocket_server) = &self.websocket_server {
            if let Err(e) = websocket_server.shutdown().await {
                tracing::error!(error = %e, "WebSocketServer shutdown failed");
            }
        }

        tracing::info!("ExePluginManager shutdown completed");
    }
}

// ============================================================================
// v3マクロでC ABI自動生成
// ============================================================================

export_plugin_v3_async!(ExePluginManagerV3Impl);

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_plugin_creation() {
        let plugin = ExePluginManagerV3Impl::new();
        assert!(plugin.websocket_server.is_none());
        assert!(plugin.process_manager.is_none());
    }
}
