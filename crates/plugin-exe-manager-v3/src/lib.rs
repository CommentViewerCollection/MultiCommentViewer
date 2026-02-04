pub mod manifest;
pub mod process_manager;
pub mod routing;
pub mod websocket_server;

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use mcv_plugin_interface::{PluginError, PluginHost};
use plugin_abi_helper::v3::prelude::*;
use std::collections::HashMap;
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
    logical_plugin_id: Uuid,
    websocket_server: Option<Arc<WebSocketServer>>,
    process_manager: Option<Arc<RwLock<ProcessManager>>>,
    /// ルーティングテーブル: logical_plugin_id → internal_physical_plugin_id
    /// Coreから送られてきたメッセージのdstに基づいて、対応するEXEプラグインに転送する
    routing_table: Arc<RwLock<HashMap<Uuid, Uuid>>>,
}

impl ExePluginManagerV3Impl {
    pub fn new() -> Self {
        Self {
            logical_plugin_id: Uuid::new_v4(),  // on_loadedで設定される
            websocket_server: None,
            process_manager: None,
            routing_table: Arc::new(RwLock::new(HashMap::new())),
        }
    }

    /// WebSocketサーバーとプロセスマネージャーを初期化
    async fn initialize(&mut self, ctx: PluginContext) -> Result<(), String> {
        // PluginContextAdapterを作成
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone(), self.logical_plugin_id));

        // WebSocketサーバーを起動（ポート競合時は自動的に次のポートを試行）
        let mut websocket_server = None;
        let mut last_error = None;

        for port in 28901..28911 {
            let addr = format!("127.0.0.1:{}", port);
            match WebSocketServer::new(&addr, Arc::clone(&adapter) as Arc<dyn PluginHost>).await {
                Ok(server) => {
                    let actual_port = server.get_port();
                    tracing::info!(
                        port = actual_port,
                        "WebSocket server successfully started and listening"
                    );
                    println!("=== ExePluginManager: WebSocket server listening on port {} ===", actual_port);
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

        self.websocket_server = Some(Arc::clone(&websocket_server));

        // plugin-helloコールバックを設定（ルーティングテーブルに登録）
        let routing_table_clone = Arc::clone(&self.routing_table);
        websocket_server.set_on_plugin_registered(move |internal_id, logical_id| {
            // 非同期コンテキストで実行する必要があるため、tokio::spawnを使用
            let routing_table = Arc::clone(&routing_table_clone);
            tokio::spawn(async move {
                routing_table.write().await.insert(logical_id, internal_id);
                tracing::debug!(
                    logical_plugin_id = %logical_id,
                    internal_physical_plugin_id = %internal_id,
                    "Routing table entry added"
                );
            });
        }).await;

        // プロセスマネージャーを初期化
        let process_manager =
            ProcessManager::new(self.websocket_server.as_ref().unwrap().get_port())
                .await
                .map_err(|e| e.to_string())?;

        self.process_manager = Some(Arc::new(RwLock::new(process_manager)));

        Ok(())
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
        tracing::info!(plugin_id = %self.logical_plugin_id, "ExePluginManager v3 loaded");

        // mcv-tracing初期化
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone(), self.logical_plugin_id));
        if let Err(e) = mcv_tracing::init_tracing(
            self.logical_plugin_id,
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
            plugin_id: self.logical_plugin_id,
            role: vec!["exe-plugin-manager".to_string()],
            api_version: "v3".to_string(),
        };

        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );

        let json = serde_json::to_vec(&message).unwrap();
        ctx.send_message(&json).await;

        // WebSocketサーバーのポート番号をログ出力
        if let Some(ws_server) = &self.websocket_server {
            let port = ws_server.get_port();
            tracing::info!(
                port = port,
                websocket_url = format!("ws://127.0.0.1:{}", port),
                "ExePluginManager initialized successfully"
            );
            println!("=== ExePluginManager: WebSocket URL = ws://127.0.0.1:{} ===", port);
        } else {
            tracing::info!("ExePluginManager initialized and plugin-hello sent");
        }
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

        // dstに基づいてルーティング
        match message.dst {
            MessageDestination::Plugin { plugin_id } => {
                // 自分宛てかチェック
                if plugin_id == self.logical_plugin_id {
                    // EXE Plugin Manager自身宛てのメッセージ
                    // 現在は何もしない（将来の拡張用）
                    tracing::trace!(
                        target: "mcv::plugin_exe_manager",
                        "Received message for EXE Plugin Manager itself"
                    );
                    return;
                }

                // ルーティングテーブルを引いてEXEプラグインに転送
                if let Some(websocket_server) = &self.websocket_server {
                    let routing_table = self.routing_table.read().await;
                    if let Some(internal_physical_plugin_id) = routing_table.get(&plugin_id) {
                        let internal_physical_plugin_id = *internal_physical_plugin_id;
                        drop(routing_table); // ロック解放

                        let router = websocket_server.get_router();
                        if let Err(e) = router.route_to_plugin(internal_physical_plugin_id, message).await {
                            tracing::error!(
                                error = %e,
                                logical_plugin_id = %plugin_id,
                                internal_physical_plugin_id = %internal_physical_plugin_id,
                                "Failed to route message to EXE plugin"
                            );
                        }
                    } else {
                        tracing::warn!(
                            plugin_id = %plugin_id,
                            "Received message for unknown EXE plugin (not in routing table)"
                        );
                    }
                }
            }
            MessageDestination::Core => {
                // Coreへのメッセージ（通常は発生しない）
                tracing::trace!(
                    target: "mcv::plugin_exe_manager",
                    "Received message destined for Core (unexpected)"
                );
            }
            MessageDestination::Broadcast => {
                // Broadcastはbroadcast_to_all_logical_pluginsでPlugin{plugin_id}に変換されるはず
                tracing::warn!(
                    target: "mcv::plugin_exe_manager",
                    "Received Broadcast message (should have been converted to Plugin)"
                );
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
