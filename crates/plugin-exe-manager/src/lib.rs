pub mod manifest;
pub mod websocket_server;
pub mod process_manager;
pub mod routing;

use mcv_plugin_interface::{Plugin, PluginError, PluginHost};
use mcv_messages::{
    Message as McvMessage, MessageSource, MessageDestination, MessageType,
    PluginHelloPayload,
};
use std::ffi::{CStr, CString};
use std::os::raw::c_char;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use websocket_server::WebSocketServer;
use process_manager::ProcessManager;

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
        Self {
            plugin_id: Uuid::new_v4(),
            websocket_server: None,
            process_manager: None,
            host: None,
        }
    }

    /// WebSocketサーバーとプロセスマネージャーを初期化
    async fn initialize(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        self.host = Some(Arc::clone(&host));

        // WebSocketサーバーを起動
        let websocket_server = WebSocketServer::new("127.0.0.1:28901", Arc::clone(&host))
            .await
            .map_err(|e| PluginError::InitializationFailed(e.to_string()))?;

        self.websocket_server = Some(Arc::new(websocket_server));

        // プロセスマネージャーを初期化
        let process_manager = ProcessManager::new(self.websocket_server.as_ref().unwrap().get_port())
            .await
            .map_err(|e| PluginError::InitializationFailed(e.to_string()))?;

        self.process_manager = Some(Arc::new(RwLock::new(process_manager)));

        tracing::info!(
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
        println!("=== ExePluginManager::on_loaded called, plugin_id: {} ===", self.plugin_id);
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

        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id: self.plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );

        println!("=== ExePluginManager: Sending plugin-hello message ===");
        host.send_message(message).await?;

        println!("=== ExePluginManager: plugin-hello message sent successfully ===");
        tracing::info!("ExePluginManager plugin-hello sent");

        Ok(())
    }

    async fn on_message(&mut self, message: McvMessage, _host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        tracing::debug!(
            message_type = ?message.message_type,
            "ExePluginManager received message"
        );

        // EXEプラグインへメッセージをルーティング
        if let Some(websocket_server) = &self.websocket_server {
            let router = websocket_server.get_router();

            // ブロードキャストが必要なメッセージタイプかチェック
            if Self::should_broadcast(&message.message_type) {
                tracing::debug!(
                    message_type = ?message.message_type,
                    "Broadcasting message to all EXE plugins"
                );
                router.broadcast(message).await
                    .map_err(|e| PluginError::ConnectionError(e.to_string()))?;
            } else {
                // ユニキャスト: 特定のプラグインへ送信
                match &message.dst {
                    MessageDestination::Plugin { plugin_id } => {
                        tracing::debug!(
                            plugin_id = %plugin_id,
                            message_type = ?message.message_type,
                            "Routing message to specific EXE plugin"
                        );
                        router.route_to_plugin(*plugin_id, message).await
                            .map_err(|e| PluginError::ConnectionError(e.to_string()))?;
                    }
                    MessageDestination::Core => {
                        // Coreへのメッセージはルーティングしない
                        tracing::debug!("Message to Core, not routing to EXE plugins");
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
            pm.shutdown().await
                .map_err(|e| PluginError::Other(e.to_string()))?;
        }

        // WebSocketサーバーをシャットダウン
        if let Some(websocket_server) = &self.websocket_server {
            websocket_server.shutdown().await
                .map_err(|e| PluginError::Other(e.to_string()))?;
        }

        tracing::info!("ExePluginManager shutdown completed");

        Ok(())
    }
}

// ============================================================================
// C ABI エクスポート関数
// ============================================================================

use once_cell::sync::OnceCell;
use tokio::runtime::Runtime;

/// グローバルなプラグインインスタンス
static mut PLUGIN_INSTANCE: OnceCell<Arc<tokio::sync::Mutex<ExePluginManager>>> = OnceCell::new();

/// グローバルなメッセージコールバック
static mut MESSAGE_CALLBACK: Option<extern "C" fn(*const c_char)> = None;

/// グローバルなTokioランタイム
static mut RUNTIME: OnceCell<Runtime> = OnceCell::new();

/// プラグインのメタデータを取得
#[no_mangle]
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

/// PluginHost実装（コールバック経由でmcvにメッセージ送信）
struct CApiPluginHost;

#[async_trait::async_trait]
impl PluginHost for CApiPluginHost {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        let message_json = serde_json::to_string(&message)
            .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to serialize message: {}", e)))?;

        let message_cstr = CString::new(message_json)
            .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to create CString: {}", e)))?;

        unsafe {
            if let Some(cb) = MESSAGE_CALLBACK {
                cb(message_cstr.as_ptr());
            }
        }

        Ok(())
    }
}

/// プラグインを初期化
#[no_mangle]
pub extern "C" fn plugin_init(_host_context: *mut libc::c_void) -> i32 {
    println!("=== C ABI: plugin_init called (ExePluginManager) ===");

    unsafe {
        // Tokioランタイムを初期化
        let runtime = match Runtime::new() {
            Ok(rt) => rt,
            Err(e) => {
                eprintln!("Failed to create Tokio runtime: {}", e);
                return -1;
            }
        };

        if RUNTIME.set(runtime).is_err() {
            eprintln!("Failed to set RUNTIME");
            return -1;
        }

        // プラグインインスタンスを作成
        let plugin = Arc::new(tokio::sync::Mutex::new(ExePluginManager::new()));

        let plugin_id = {
            let plugin_guard = RUNTIME.get().unwrap().block_on(plugin.lock());
            plugin_guard.plugin_id
        };

        println!("=== C ABI: Generated plugin_id: {} ===", plugin_id);

        if PLUGIN_INSTANCE.set(Arc::clone(&plugin)).is_err() {
            eprintln!("Failed to set PLUGIN_INSTANCE");
            return -1;
        }

        println!("=== C ABI: plugin_init completed successfully (ExePluginManager) ===");
        0
    }
}

/// プラグインon_loaded呼び出し
#[no_mangle]
pub extern "C" fn plugin_on_loaded() -> i32 {
    println!("=== C ABI: plugin_on_loaded called (ExePluginManager) ===");

    unsafe {
        if let Some(plugin) = PLUGIN_INSTANCE.get() {
            if let Some(runtime) = RUNTIME.get() {
                let host = Arc::new(CApiPluginHost);
                let result = runtime.block_on(async {
                    let mut plugin_guard = plugin.lock().await;
                    plugin_guard.on_loaded(host).await
                });

                if let Err(e) = result {
                    eprintln!("plugin_on_loaded: on_loaded failed: {}", e);
                    return -1;
                }

                println!("=== C ABI: plugin_on_loaded completed successfully (ExePluginManager) ===");
                return 0;
            } else {
                eprintln!("plugin_on_loaded: Runtime not initialized");
                return -1;
            }
        } else {
            eprintln!("plugin_on_loaded: Plugin not initialized");
            return -1;
        }
    }
}

/// コールバックを設定
#[no_mangle]
pub extern "C" fn plugin_set_callback(callback: extern "C" fn(*const c_char)) -> i32 {
    println!("=== C ABI: plugin_set_callback called (ExePluginManager) ===");
    unsafe {
        MESSAGE_CALLBACK = Some(callback);
        0
    }
}

/// プラグインへメッセージを送信
#[no_mangle]
pub extern "C" fn plugin_send_message(message_json: *const c_char) -> i32 {
    unsafe {
        if message_json.is_null() {
            eprintln!("plugin_send_message: message_json is null");
            return -1;
        }

        let message_cstr = CStr::from_ptr(message_json);
        let message_str = match message_cstr.to_str() {
            Ok(s) => s,
            Err(e) => {
                eprintln!("plugin_send_message: Failed to convert CStr to str: {}", e);
                return -1;
            }
        };

        // JSONをパース
        let message: McvMessage = match serde_json::from_str(message_str) {
            Ok(m) => m,
            Err(e) => {
                eprintln!("plugin_send_message: Failed to parse JSON: {}", e);
                return -1;
            }
        };

        // プラグインインスタンスを取得
        if let Some(plugin) = PLUGIN_INSTANCE.get() {
            if let Some(runtime) = RUNTIME.get() {
                let host = Arc::new(CApiPluginHost);
                let result = runtime.block_on(async {
                    let mut plugin_guard = plugin.lock().await;
                    plugin_guard.on_message(message, host).await
                });

                if let Err(e) = result {
                    eprintln!("plugin_send_message: on_message failed: {}", e);
                    return -1;
                }
            } else {
                eprintln!("plugin_send_message: Runtime not initialized");
                return -1;
            }
        } else {
            eprintln!("plugin_send_message: Plugin not initialized");
            return -1;
        }

        0
    }
}

/// プラグインをシャットダウン
#[no_mangle]
pub extern "C" fn plugin_shutdown() -> i32 {
    unsafe {
        if let Some(plugin) = PLUGIN_INSTANCE.get() {
            if let Some(runtime) = RUNTIME.get() {
                runtime.block_on(async {
                    let mut plugin_guard = plugin.lock().await;
                    match plugin_guard.on_shutdown().await {
                        Ok(_) => tracing::info!("Plugin shutdown completed"),
                        Err(e) => tracing::error!(error = %e, "Plugin shutdown failed"),
                    }
                });
            }
        }

        // グローバル変数をクリア
        MESSAGE_CALLBACK = None;

        0
    }
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
