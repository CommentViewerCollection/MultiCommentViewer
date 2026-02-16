pub mod manifest;
pub mod process_manager;
pub mod routing;
pub mod websocket_server;

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use mcv_plugin_interface::{Plugin, PluginError, PluginHost};
use plugin_abi_helper::v2 as abi;
use std::ffi::{CString, c_void};
use std::os::raw::c_char;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use process_manager::ProcessManager;
use websocket_server::WebSocketServer;

/// EXEプラグインマネージャー
///
/// WebSocketサーバーを起動し、EXEプラグインとの通信を仲介する
pub struct ExePluginManager {
    plugin_id: Uuid,
    websocket_server: Option<Arc<WebSocketServer>>,
    process_manager: Option<Arc<RwLock<ProcessManager>>>,
    host: Option<Arc<dyn PluginHost>>,
}

impl ExePluginManager {
    pub fn new() -> Self {
        let plugin_id = Uuid::new_v4();
        println!("ExePluginManager plugin_id: {plugin_id}");
        Self {
            plugin_id,
            websocket_server: None,
            process_manager: None,
            host: None,
        }
    }

    /// WebSocketサーバーとプロセスマネージャーを初期化
    async fn initialize(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        self.host = Some(Arc::clone(&host));

        // WebSocketサーバーを起動（ポート競合時は自動的に次のポートを試行）
        let mut websocket_server = None;
        let mut last_error = None;

        for port in 28901..28911 {
            let addr = format!("127.0.0.1:{}", port);
            match WebSocketServer::new(&addr, Arc::clone(&host)).await {
                Ok(server) => {
                    tracing::info!(target: "mcv::plugin_exe_manager", port = port, "WebSocket server started successfully");
                    websocket_server = Some(Arc::new(server));
                    break;
                }
                Err(e) => {
                    tracing::warn!(target: "mcv::plugin_exe_manager", port = port, error = %e, "Failed to start WebSocket server on port, trying next");
                    last_error = Some(e);
                }
            }
        }

        let websocket_server = websocket_server.ok_or_else(|| {
            let err_msg = format!(
                "Failed to start WebSocket server on any port (28901-28910): {}",
                last_error
                    .map(|e| e.to_string())
                    .unwrap_or_else(|| "unknown error".to_string())
            );
            PluginError::InitializationFailed(err_msg)
        })?;

        self.websocket_server = Some(websocket_server);

        // プロセスマネージャーを初期化
        let process_manager =
            ProcessManager::new(self.websocket_server.as_ref().unwrap().get_port())
                .await
                .map_err(|e| PluginError::InitializationFailed(e.to_string()))?;

        self.process_manager = Some(Arc::new(RwLock::new(process_manager)));

        tracing::info!(
            target: "mcv::plugin_exe_manager",
            plugin_id = %self.plugin_id,
            websocket_port = self.websocket_server.as_ref().unwrap().get_port(),
            "ExePluginManager initialized"
        );

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

impl Default for ExePluginManager {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait::async_trait]
impl Plugin for ExePluginManager {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        println!(
            "=== ExePluginManager::on_loaded called, plugin_id: {} ===",
            self.plugin_id
        );

        // トレーシングを初期化し、Core への LogEntry 自動転送を有効にする
        mcv_plugin_telemetry::init_tracing(
            self.plugin_id,
            Arc::clone(&host),
            env!("CARGO_PKG_VERSION"),
            "trace",
        )
        .map_err(|e| PluginError::InitializationFailed(format!("Failed to init tracing: {}", e)))?;

        tracing::info!("ExePluginManager::on_loaded called");

        // 初期化
        println!("=== ExePluginManager: Starting initialization ===");
        self.initialize(Arc::clone(&host)).await?;
        println!("=== ExePluginManager: Initialization completed ===");

        // plugin-helloを送信
        let hello_payload = PluginHelloPayload {
            name: "EXE Plugin Manager".to_string(),
            plugin_id: self.plugin_id,
            role: vec!["exe-plugin-manager".to_string()],
            api_version: "v2".to_string(),
        };

        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );

        tracing::trace!(
            target: "mcv::plugin_exe_manager",
            "Sending plugin-hello message"
        );
        host.send_message(message).await?;
        tracing::trace!(
            target: "mcv::plugin_exe_manager",
            "ExePluginManager plugin-hello sent"
        );

        Ok(())
    }

    async fn on_message(
        &mut self,
        message: McvMessage,
        _host: Arc<dyn PluginHost>,
    ) -> Result<(), PluginError> {
        tracing::trace!(
            target: "mcv::plugin_exe_manager",
            message_type = ?message.message_type,
            "ExePluginManager received message: {:?}", message
        );
        eprintln!(
            "=== ExePluginManager::on_message called, message_type: {:?} ===",
            message.message_type
        );

        // EXEプラグインへメッセージをルーティング
        if let Some(websocket_server) = &self.websocket_server {
            let router = websocket_server.get_router();

            // ブロードキャストが必要なメッセージタイプかチェック
            if Self::should_broadcast(&message.message_type) {
                tracing::debug!(
                    target: "mcv::plugin_exe_manager",
                    message_type = ?message.message_type,
                    "Broadcasting message to all EXE plugins"
                );
                router
                    .broadcast(message)
                    .await
                    .map_err(|e| PluginError::ConnectionError(e.to_string()))?;
            } else {
                // ユニキャスト: 特定のプラグインへ送信
                match &message.dst {
                    MessageDestination::Plugin { plugin_id } => {
                        tracing::debug!(
                            target: "mcv::plugin_exe_manager",
                            plugin_id = %plugin_id,
                            message_type = ?message.message_type,
                            "Routing message to specific EXE plugin"
                        );
                        router
                            .route_to_plugin(*plugin_id, message)
                            .await
                            .map_err(|e| PluginError::ConnectionError(e.to_string()))?;
                    }
                    MessageDestination::Core => {
                        // Coreへのメッセージはルーティングしない
                        tracing::debug!(target: "mcv::plugin_exe_manager","Message to Core, not routing to EXE plugins");
                    }
                    MessageDestination::Broadcast => {
                        // ブロードキャストは既に上でハンドリングされているはず
                        tracing::warn!(target: "mcv::plugin_exe_manager","Broadcast message reached unicast branch");
                    }
                }
            }
        }

        Ok(())
    }

    async fn on_shutdown(&mut self) -> Result<(), PluginError> {
        tracing::info!("ExePluginManager::on_shutdown called");

        // プロセスマネージャーをシャットダウン
        if let Some(process_manager) = &self.process_manager {
            let mut pm = process_manager.write().await;
            pm.shutdown()
                .await
                .map_err(|e| PluginError::Other(e.to_string()))?;
        }

        // WebSocketサーバーをシャットダウン
        if let Some(websocket_server) = &self.websocket_server {
            websocket_server
                .shutdown()
                .await
                .map_err(|e| PluginError::Other(e.to_string()))?;
        }

        tracing::info!("ExePluginManager shutdown completed");

        Ok(())
    }
}

// ============================================================================
// C ABI エクスポート関数
// ============================================================================

/// プラグインのメタデータを取得
#[unsafe(no_mangle)]
pub extern "C" fn plugin_get_metadata() -> *const c_char {
    let metadata = serde_json::json!({
        "id": "com.mcv.exe-plugin-manager",
        "name": "EXE Plugin Manager",
        "version": env!("CARGO_PKG_VERSION"),
        "api_version": "v2",
        "roles": ["exe-plugin-manager"]
    });

    let metadata_str = metadata.to_string();
    match CString::new(metadata_str) {
        Ok(c_str) => c_str.into_raw(),
        Err(_) => std::ptr::null(),
    }
}

/// プラグインを初期化
#[unsafe(no_mangle)]
pub extern "C" fn plugin_init(_host_context: *mut libc::c_void) -> i32 {
    abi::init_plugin(Box::new(ExePluginManager::new()))
}

/// プラグインon_loaded呼び出し
#[unsafe(no_mangle)]
pub extern "C" fn plugin_on_loaded() -> i32 {
    abi::call_on_loaded()
}

/// コールバックを設定
#[unsafe(no_mangle)]
pub extern "C" fn plugin_set_callback(
    cb: extern "C" fn(*const c_char, *mut c_void),
    userdata: *mut c_void,
) -> i32 {
    abi::set_callback(cb, userdata);
    0
}

/// プラグインへメッセージを送信
#[unsafe(no_mangle)]
pub extern "C" fn plugin_send_message(msg: *const c_char) -> i32 {
    abi::call_on_message(msg)
}

/// プラグインをシャットダウン
#[unsafe(no_mangle)]
pub extern "C" fn plugin_shutdown() -> i32 {
    abi::shutdown();
    0
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_plugin_creation() {
        let plugin = ExePluginManager::new();
        assert!(plugin.websocket_server.is_none());
        assert!(plugin.process_manager.is_none());
    }
}
