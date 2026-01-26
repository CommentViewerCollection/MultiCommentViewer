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
}

impl Default for ExePluginManager {
    fn default() -> Self {
        Self::new()
    }
}

#[async_trait::async_trait]
impl Plugin for ExePluginManager {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        tracing::info!("ExePluginManager::on_loaded called");

        // 初期化
        self.initialize(Arc::clone(&host)).await?;

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

        host.send_message(message).await?;

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
            if let MessageDestination::Plugin { plugin_id } = message.dst {
                // 該当するEXEプラグインへ送信
                websocket_server.send_to_plugin(plugin_id, message).await
                    .map_err(|e| PluginError::ConnectionError(e.to_string()))?;
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

/// プラグインを初期化
#[no_mangle]
pub extern "C" fn plugin_init(_host_context: *mut libc::c_void) -> i32 {
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

        if PLUGIN_INSTANCE.set(Arc::clone(&plugin)).is_err() {
            eprintln!("Failed to set PLUGIN_INSTANCE");
            return -1;
        }

        // ここではまだon_loadedは呼ばない（PluginHostActorのstarted内で呼ばれる）
        0
    }
}

/// コールバックを設定
#[no_mangle]
pub extern "C" fn plugin_set_callback(callback: extern "C" fn(*const c_char)) -> i32 {
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

        // TODO: メッセージをパースして処理
        // 現在はログ出力のみ
        tracing::debug!(message = %message_str, "Received message from Core");

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
