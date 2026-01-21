use async_trait::async_trait;
use mcv_messages::*;
use mcv_plugin_interface::{Plugin, PluginError, PluginHost};
use rand::{Rng, SeedableRng};
use rand::rngs::StdRng;
use std::collections::HashMap;
use std::sync::atomic::{AtomicBool, Ordering};
use std::sync::Arc;
use tokio::time::{sleep, Duration};
use uuid::Uuid;

/// ダミープラグイン
///
/// ランダムにコメントを生成するテスト用プラグイン
pub struct DummyPlugin {
    plugin_id: Uuid,
    // 複数の接続を管理するためのHashMap
    connections: HashMap<Uuid, Arc<AtomicBool>>,
    // コメント生成間隔（connection_id -> 秒数）
    comment_rates: HashMap<Uuid, Arc<tokio::sync::RwLock<u64>>>,
    // 一時停止フラグ（connection_id -> paused）
    paused: HashMap<Uuid, Arc<AtomicBool>>,
}

impl DummyPlugin {
    /// 新しいダミープラグインを作成
    pub fn new() -> Self {
        Self {
            plugin_id: Uuid::new_v4(),
            connections: HashMap::new(),
            comment_rates: HashMap::new(),
            paused: HashMap::new(),
        }
    }

    /// コメント生成タスク（静的メソッドとして実装）
    fn spawn_comment_generator(
        plugin_id: Uuid,
        connection_id: Uuid,
        is_running: Arc<AtomicBool>,
        is_paused: Arc<AtomicBool>,
        rate: Arc<tokio::sync::RwLock<u64>>,
        sender: tokio::sync::mpsc::UnboundedSender<Message>,
    ) {
        tokio::spawn(async move {
            let names = vec!["太郎", "花子", "次郎", "さくら", "けん"];
            let texts = vec![
                "こんにちは!",
                "面白い配信ですね",
                "草",
                "888888",
                "次回も楽しみです",
                "いいね!",
                "すごい!",
                "www",
            ];

            let mut rng = StdRng::from_entropy();
            tracing::info!(connection_id = %connection_id, "Comment generation started");

            while is_running.load(Ordering::SeqCst) {
                // rateに基づいた間隔（デフォルトは1-5秒のランダム）
                let rate_value = *rate.read().await;
                let interval = if rate_value > 0 {
                    rate_value
                } else {
                    rng.gen_range(1..=5)
                };
                sleep(Duration::from_secs(interval)).await;

                if !is_running.load(Ordering::SeqCst) {
                    break;
                }

                // pausedの場合はスキップ
                if is_paused.load(Ordering::SeqCst) {
                    continue;
                }

                // ランダムなコメントを生成
                let comment = Comment {
                    id: Uuid::new_v4().to_string(),
                    user_name: names[rng.gen_range(0..names.len())].to_string(),
                    user_id: format!("user_{}", rng.gen_range(1000..9999)),
                    text: texts[rng.gen_range(0..texts.len())].to_string(),
                    timestamp: chrono::Utc::now().timestamp(),
                };

                tracing::debug!(
                    connection_id = %connection_id,
                    user_name = %comment.user_name,
                    text = %comment.text,
                    "Generated comment"
                );

                let message = Message::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Plugin { plugin_id },
                    MessageDestination::Core,
                    serde_json::to_value(CommentReceivedPayload {
                        connection_id,
                        comment,
                    })
                    .unwrap(),
                );

                if sender.send(message).is_err() {
                    tracing::error!(connection_id = %connection_id, "Failed to send comment - receiver dropped");
                    break;
                }
            }

            tracing::info!(connection_id = %connection_id, "Comment generation stopped");
        });
    }
}

impl Default for DummyPlugin {
    fn default() -> Self {
        Self::new()
    }
}

impl DummyPlugin {
    /// コマンドを処理
    async fn handle_command(
        &mut self,
        connection_id: Uuid,
        command: &str,
        host: Arc<dyn PluginHost>,
    ) -> Result<String, String> {
        let parts: Vec<&str> = command.split_whitespace().collect();
        if parts.is_empty() {
            return Err("Empty command".to_string());
        }

        match parts[0] {
            "help" => Ok(Self::get_help()),
            "status" => Ok(self.get_status(connection_id)),
            "disconnect" => self.command_disconnect(connection_id, host).await,
            "connect" => self.command_connect(connection_id, host).await,
            "pause" => self.command_pause(connection_id),
            "resume" => self.command_resume(connection_id),
            "rate" => self.command_rate(connection_id, &parts[1..]),
            "comment" => self.command_comment(connection_id, &parts[1..], host).await,
            "log-error" => self.command_log(connection_id, "error", &parts[1..], host).await,
            "log-warn" => self.command_log(connection_id, "warn", &parts[1..], host).await,
            "log-info" => self.command_log(connection_id, "info", &parts[1..], host).await,
            "log-debug" => self.command_log(connection_id, "debug", &parts[1..], host).await,
            _ => Err(format!("Unknown command: {}", parts[0])),
        }
    }

    fn get_help() -> String {
        r#"Available commands:
- help: Show this help message
- status: Show connection status
- disconnect: Simulate disconnection from site
- connect: Simulate reconnection to site
- pause: Pause comment generation
- resume: Resume comment generation
- rate <seconds>: Set comment interval (0 = random)
- comment <user> <text>: Generate a manual comment
- log-error <message>: Send error log to mcv
- log-warn <message>: Send warning log to mcv
- log-info <message>: Send info log to mcv
- log-debug <message>: Send debug log to mcv"#
            .to_string()
    }

    fn get_status(&self, connection_id: Uuid) -> String {
        let is_connected = self.connections.contains_key(&connection_id);
        let is_paused = self
            .paused
            .get(&connection_id)
            .map(|p| p.load(Ordering::SeqCst))
            .unwrap_or(false);

        format!(
            "Connection {}: Connected={}, Paused={}",
            connection_id, is_connected, is_paused
        )
    }

    async fn command_disconnect(
        &mut self,
        connection_id: Uuid,
        host: Arc<dyn PluginHost>,
    ) -> Result<String, String> {
        if !self.connections.contains_key(&connection_id) {
            return Err("Connection not found".to_string());
        }

        // disconnectedメッセージを送信（配信サイト側からの切断をシミュレート）
        let message = Message::new_notification(
            MessageType::Disconnected,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
        );

        host.send_message(message)
            .await
            .map_err(|e| format!("Failed to send disconnect: {}", e))?;

        // ローカルで停止
        if let Some(is_running) = self.connections.get(&connection_id) {
            is_running.store(false, Ordering::SeqCst);
            self.connections.remove(&connection_id);
            self.paused.remove(&connection_id);
            self.comment_rates.remove(&connection_id);
        }

        Ok("Disconnected".to_string())
    }

    async fn command_connect(
        &mut self,
        connection_id: Uuid,
        _host: Arc<dyn PluginHost>,
    ) -> Result<String, String> {
        // 既に接続されている場合はエラー
        if self.connections.contains_key(&connection_id) {
            return Err("Already connected".to_string());
        }

        // 再接続は手動でUIから行う必要がある
        Ok("Use UI to reconnect".to_string())
    }

    fn command_pause(&mut self, connection_id: Uuid) -> Result<String, String> {
        if let Some(is_paused) = self.paused.get(&connection_id) {
            is_paused.store(true, Ordering::SeqCst);
            Ok("Paused".to_string())
        } else {
            Err("Connection not found".to_string())
        }
    }

    fn command_resume(&mut self, connection_id: Uuid) -> Result<String, String> {
        if let Some(is_paused) = self.paused.get(&connection_id) {
            is_paused.store(false, Ordering::SeqCst);
            Ok("Resumed".to_string())
        } else {
            Err("Connection not found".to_string())
        }
    }

    fn command_rate(&mut self, connection_id: Uuid, args: &[&str]) -> Result<String, String> {
        if args.is_empty() {
            return Err("Usage: rate <seconds>".to_string());
        }

        let new_rate: u64 = args[0]
            .parse()
            .map_err(|_| "Invalid number".to_string())?;

        if let Some(rate_lock) = self.comment_rates.get(&connection_id) {
            let rate_lock_clone = rate_lock.clone();
            tokio::spawn(async move {
                *rate_lock_clone.write().await = new_rate;
            });
            Ok(format!("Rate set to {}s", new_rate))
        } else {
            Err("Connection not found".to_string())
        }
    }

    async fn command_comment(
        &mut self,
        connection_id: Uuid,
        args: &[&str],
        host: Arc<dyn PluginHost>,
    ) -> Result<String, String> {
        if args.len() < 2 {
            return Err("Usage: comment <user> <text>".to_string());
        }

        let user_name = args[0];
        let text = args[1..].join(" ");

        let comment = Comment {
            id: Uuid::new_v4().to_string(),
            user_name: user_name.to_string(),
            user_id: format!("user_{}", rand::thread_rng().gen_range(1000..9999)),
            text,
            timestamp: chrono::Utc::now().timestamp(),
        };

        let message = Message::new_notification(
            MessageType::CommentReceived,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(CommentReceivedPayload {
                connection_id,
                comment,
            })
            .unwrap(),
        );

        host.send_message(message)
            .await
            .map_err(|e| format!("Failed to send comment: {}", e))?;

        Ok("Comment sent".to_string())
    }

    async fn command_log(
        &mut self,
        connection_id: Uuid,
        level: &str,
        args: &[&str],
        host: Arc<dyn PluginHost>,
    ) -> Result<String, String> {
        if args.is_empty() {
            return Err(format!("Usage: log-{} <message>", level));
        }

        let message_text = args.join(" ");

        let message = Message::new_notification(
            MessageType::LogEntry,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(LogEntryPayload {
                level: level.to_string(),
                message: message_text.clone(),
                context: Some(serde_json::json!({
                    "test_command": true,
                    "connection_id": connection_id.to_string(),
                })),
                connection_id: Some(connection_id),
                plugin_version: Some(env!("CARGO_PKG_VERSION").to_string()),
                plugin_build_profile: Self::get_build_profile(),
            })
            .unwrap(),
        );

        host.send_message(message)
            .await
            .map_err(|e| format!("Failed to send log: {}", e))?;

        Ok(format!(
            "Log sent: [{}] {}",
            level.to_uppercase(),
            message_text
        ))
    }

    fn get_build_profile() -> Option<String> {
        #[cfg(feature = "alpha")]
        return Some("alpha".to_string());

        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        return Some("beta".to_string());

        #[cfg(all(
            not(feature = "alpha"),
            not(feature = "beta"),
            feature = "stable"
        ))]
        return Some("stable".to_string());

        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
        None
    }
}

#[async_trait]
impl Plugin for DummyPlugin {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        println!("=== DummyPlugin::on_loaded called, plugin_id: {} ===", self.plugin_id);

        // プラグイン tracing を初期化
        mcv_tracing::init_tracing(
            self.plugin_id,
            Arc::clone(&host),
            env!("CARGO_PKG_VERSION"),
            "info",
        )
        .map_err(|e| PluginError::InitializationFailed(format!("Failed to init tracing: {}", e)))?;

        tracing::info!("Dummy plugin loaded");

        // plugin-helloを送信
        let message = Message::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(PluginHelloPayload {
                name: "Dummy Plugin".to_string(),
                plugin_id: self.plugin_id,
                role: vec!["dummy".to_string()],
                api_version: "v2".to_string(),
            })
            .unwrap(),
        );

        tracing::debug!("Sending plugin-hello message");
        host.send_message(message).await?;
        tracing::info!("Plugin-hello message sent successfully");

        Ok(())
    }

    async fn on_message(
        &mut self,
        message: Message,
        host: Arc<dyn PluginHost>,
    ) -> Result<(), PluginError> {
        tracing::debug!(message_type = ?message.message_type, "Received message");

        match message.message_type {
            MessageType::PluginAdded => {
                tracing::info!("Plugin added successfully");
            }
            MessageType::Connect => {
                // connectメッセージからconnection_idを取得
                let payload: ConnectPayload = serde_json::from_value(message.payload.clone())
                    .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to parse connect payload: {}", e)))?;

                let conn_id = payload.connection_id;

                println!("Starting comment generation for connection: {}", conn_id);

                // この接続用のフラグを作成
                let is_running = Arc::new(AtomicBool::new(true));
                let is_paused = Arc::new(AtomicBool::new(false));
                let rate = Arc::new(tokio::sync::RwLock::new(0u64)); // 0 = ランダム

                self.connections.insert(conn_id, is_running.clone());
                self.paused.insert(conn_id, is_paused.clone());
                self.comment_rates.insert(conn_id, rate.clone());

                // connectedを返信
                let response = Message::create_response(
                    &message,
                    MessageType::Connected,
                    serde_json::to_value(ConnectedPayload {
                        connection_id: conn_id,
                    })
                    .unwrap(),
                );

                host.send_message(response.clone()).await?;

                // メッセージチャネルを作成
                let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel();

                // コメント生成タスクを起動
                Self::spawn_comment_generator(
                    self.plugin_id,
                    conn_id,
                    is_running,
                    is_paused,
                    rate,
                    tx,
                );

                // メッセージ転送タスクを起動（hostをクローンして使用）
                let host_clone = host.clone();
                tokio::spawn(async move {
                    while let Some(msg) = rx.recv().await {
                        println!("Sending comment message: {:?}", msg.message_type);
                        if let Err(e) = host_clone.send_message(msg).await {
                            eprintln!("Failed to send comment message: {}", e);
                            break;
                        }
                    }
                });
            }
            MessageType::Disconnect => {
                // disconnectメッセージからconnection_idを取得
                let payload: DisconnectPayload = serde_json::from_value(message.payload.clone())
                    .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to parse disconnect payload: {}", e)))?;

                let conn_id = payload.connection_id;

                // この接続のis_runningフラグを停止
                if let Some(is_running) = self.connections.get(&conn_id) {
                    println!("Stopping comment generation for connection: {}", conn_id);
                    is_running.store(false, Ordering::SeqCst);
                    self.connections.remove(&conn_id);
                    self.paused.remove(&conn_id);
                    self.comment_rates.remove(&conn_id);

                    // disconnectedを返信
                    let response = Message::create_response(
                        &message,
                        MessageType::Disconnected,
                        serde_json::to_value(DisconnectedPayload {
                            connection_id: conn_id,
                        })
                        .unwrap(),
                    );

                    host.send_message(response.clone()).await?;
                }
            }
            MessageType::SendComment => {
                // send-commentメッセージからpayloadを取得
                let payload: SendCommentPayload = serde_json::from_value(message.payload.clone())
                    .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to parse send-comment payload: {}", e)))?;

                let conn_id = payload.connection_id;
                let command = payload.text.trim();

                println!("Received command for connection {}: {}", conn_id, command);

                // コマンドをパースして実行（結果は既存のメッセージタイプで通知される）
                let result = self.handle_command(conn_id, command, host.clone()).await;

                if let Err(e) = result {
                    eprintln!("Command execution failed: {}", e);
                }
            }
            _ => {
                println!("Unhandled message type: {:?}", message.message_type);
            }
        }

        Ok(())
    }

    async fn on_shutdown(&mut self) -> Result<(), PluginError> {
        tracing::info!("Shutting down dummy plugin");
        // 全ての接続を停止
        for (conn_id, is_running) in &self.connections {
            tracing::info!(connection_id = %conn_id, "Stopping connection");
            is_running.store(false, Ordering::SeqCst);
        }
        self.connections.clear();
        self.paused.clear();
        self.comment_rates.clear();
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::sync::Arc;

    #[test]
    fn test_dummy_plugin_creation() {
        let plugin = DummyPlugin::new();
        assert!(plugin.connections.is_empty());
    }

    #[test]
    fn test_get_help() {
        let help = DummyPlugin::get_help();
        assert!(help.contains("help"));
        assert!(help.contains("disconnect"));
        assert!(help.contains("pause"));
        assert!(help.contains("resume"));
        assert!(help.contains("rate"));
        assert!(help.contains("comment"));
    }

    #[test]
    fn test_get_status_no_connection() {
        let plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();
        let status = plugin.get_status(connection_id);
        assert!(status.contains("Connected=false"));
        assert!(status.contains("Paused=false"));
    }

    #[test]
    fn test_get_status_with_connection() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        // 接続を追加
        plugin.connections.insert(connection_id, Arc::new(AtomicBool::new(true)));
        plugin.paused.insert(connection_id, Arc::new(AtomicBool::new(false)));

        let status = plugin.get_status(connection_id);
        assert!(status.contains("Connected=true"));
        assert!(status.contains("Paused=false"));
    }

    #[test]
    fn test_command_pause() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        // 接続を追加
        plugin.paused.insert(connection_id, Arc::new(AtomicBool::new(false)));

        let result = plugin.command_pause(connection_id);
        assert!(result.is_ok());
        assert_eq!(result.unwrap(), "Paused");

        // pausedフラグが設定されていることを確認
        let is_paused = plugin.paused.get(&connection_id).unwrap();
        assert!(is_paused.load(Ordering::SeqCst));
    }

    #[test]
    fn test_command_resume() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        // 接続を追加（paused状態で）
        plugin.paused.insert(connection_id, Arc::new(AtomicBool::new(true)));

        let result = plugin.command_resume(connection_id);
        assert!(result.is_ok());
        assert_eq!(result.unwrap(), "Resumed");

        // pausedフラグが解除されていることを確認
        let is_paused = plugin.paused.get(&connection_id).unwrap();
        assert!(!is_paused.load(Ordering::SeqCst));
    }

    #[tokio::test]
    async fn test_command_rate_valid() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        // 接続を追加
        let rate_lock = Arc::new(tokio::sync::RwLock::new(0));
        plugin.comment_rates.insert(connection_id, rate_lock.clone());

        let result = plugin.command_rate(connection_id, &["5"]);
        assert!(result.is_ok());
        assert!(result.unwrap().contains("Rate set to 5s"));

        // 少し待機してtokio::spawnが完了するのを待つ
        tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

        // レートが設定されたことを確認
        let rate_value = *rate_lock.read().await;
        assert_eq!(rate_value, 5);
    }

    #[test]
    fn test_command_rate_invalid() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        // 接続を追加
        plugin.comment_rates.insert(connection_id, Arc::new(tokio::sync::RwLock::new(0)));

        let result = plugin.command_rate(connection_id, &["invalid"]);
        assert!(result.is_err());
        assert_eq!(result.unwrap_err(), "Invalid number");
    }

    #[test]
    fn test_command_rate_no_args() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        let result = plugin.command_rate(connection_id, &[]);
        assert!(result.is_err());
        assert!(result.unwrap_err().contains("Usage: rate <seconds>"));
    }

    #[test]
    fn test_command_pause_no_connection() {
        let mut plugin = DummyPlugin::new();
        let connection_id = Uuid::new_v4();

        let result = plugin.command_pause(connection_id);
        assert!(result.is_err());
        assert_eq!(result.unwrap_err(), "Connection not found");
    }

    #[test]
    fn test_multiple_connections() {
        let mut plugin = DummyPlugin::new();
        let connection_id1 = Uuid::new_v4();
        let connection_id2 = Uuid::new_v4();

        // 2つの接続を追加
        plugin.connections.insert(connection_id1, Arc::new(AtomicBool::new(true)));
        plugin.connections.insert(connection_id2, Arc::new(AtomicBool::new(true)));
        plugin.paused.insert(connection_id1, Arc::new(AtomicBool::new(false)));
        plugin.paused.insert(connection_id2, Arc::new(AtomicBool::new(false)));

        // connection1をpause
        let result1 = plugin.command_pause(connection_id1);
        assert!(result1.is_ok());

        // connection1がpausedでconnection2がpausedでないことを確認
        assert!(plugin.paused.get(&connection_id1).unwrap().load(Ordering::SeqCst));
        assert!(!plugin.paused.get(&connection_id2).unwrap().load(Ordering::SeqCst));
    }
}

// ============================================================================
// C ABI エクスポート関数（DLL化用）
// ============================================================================

use once_cell::sync::Lazy;
use std::ffi::{c_char, c_void, CStr, CString};
use std::sync::Mutex;

// グローバルステート
static PLUGIN_INSTANCE: Lazy<Mutex<Option<DummyPlugin>>> = Lazy::new(|| Mutex::new(None));
static MESSAGE_CALLBACK: Lazy<Mutex<Option<extern "C" fn(*const c_char)>>> = Lazy::new(|| Mutex::new(None));
static RUNTIME: Lazy<tokio::runtime::Runtime> = Lazy::new(|| {
    tokio::runtime::Runtime::new().expect("Failed to create Tokio runtime")
});

/// PluginHost実装（コールバック経由でmcvにメッセージ送信）
struct CApiPluginHost;

#[async_trait]
impl PluginHost for CApiPluginHost {
    async fn send_message(&self, message: Message) -> Result<(), PluginError> {
        let message_json = serde_json::to_string(&message)
            .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to serialize message: {}", e)))?;

        let message_cstr = CString::new(message_json)
            .map_err(|e| PluginError::MessageHandlingFailed(format!("Failed to create CString: {}", e)))?;

        let callback_guard = MESSAGE_CALLBACK.lock().unwrap();
        if let Some(cb) = *callback_guard {
            cb(message_cstr.as_ptr());
        }

        Ok(())
    }
}

/// プラグインメタデータ取得
///
/// # Safety
/// この関数はCから呼び出されることを想定しています。
#[no_mangle]
pub extern "C" fn plugin_get_metadata() -> *const c_char {
    let metadata = r#"{
  "id": "plugin-dummy",
  "name": "Dummy Plugin",
  "version": "0.1.0",
  "api_version": "v2",
  "roles": ["dummy"]
}"#;

    CString::new(metadata).unwrap().into_raw()
}

/// プラグイン初期化
///
/// # Safety
/// この関数はCから呼び出されることを想定しています。
#[no_mangle]
pub extern "C" fn plugin_init(_host_context: *mut c_void) -> i32 {
    println!("=== C ABI: plugin_init called ===");

    let mut instance = PLUGIN_INSTANCE.lock().unwrap();
    let mut plugin = DummyPlugin::new();

    // on_loadedを呼び出し
    let host = Arc::new(CApiPluginHost);
    let result = RUNTIME.block_on(plugin.on_loaded(host));

    if let Err(e) = result {
        eprintln!("plugin_init failed: {}", e);
        return -1;
    }

    *instance = Some(plugin);
    println!("=== C ABI: plugin_init completed successfully ===");
    0 // 成功
}

/// メッセージ送信（mcv→プラグイン）
///
/// # Safety
/// この関数はCから呼び出されることを想定しています。
#[no_mangle]
pub extern "C" fn plugin_send_message(message_json: *const c_char) -> i32 {
    if message_json.is_null() {
        eprintln!("plugin_send_message: null message_json");
        return -1;
    }

    let message_json_cstr = unsafe { CStr::from_ptr(message_json) };
    let message_json_str = match message_json_cstr.to_str() {
        Ok(s) => s,
        Err(e) => {
            eprintln!("plugin_send_message: Invalid UTF-8: {}", e);
            return -1;
        }
    };

    // JSONをパース
    let message: Message = match serde_json::from_str(message_json_str) {
        Ok(m) => m,
        Err(e) => {
            eprintln!("plugin_send_message: Failed to parse JSON: {}", e);
            return -1;
        }
    };

    // プラグインインスタンスを取得
    let mut instance_guard = PLUGIN_INSTANCE.lock().unwrap();
    if let Some(ref mut plugin) = *instance_guard {
        let host = Arc::new(CApiPluginHost);
        let result = RUNTIME.block_on(plugin.on_message(message, host));

        if let Err(e) = result {
            eprintln!("plugin_send_message: on_message failed: {}", e);
            return -1;
        }
    } else {
        eprintln!("plugin_send_message: Plugin not initialized");
        return -1;
    }

    0 // 成功
}

/// メッセージ受信コールバック設定（プラグイン→mcv）
///
/// # Safety
/// この関数はCから呼び出されることを想定しています。
#[no_mangle]
pub extern "C" fn plugin_set_callback(callback: extern "C" fn(*const c_char)) -> i32 {
    println!("=== C ABI: plugin_set_callback called ===");
    let mut cb = MESSAGE_CALLBACK.lock().unwrap();
    *cb = Some(callback);
    0 // 成功
}

/// プラグイン終了
///
/// # Safety
/// この関数はCから呼び出されることを想定しています。
#[no_mangle]
pub extern "C" fn plugin_shutdown() -> i32 {
    println!("=== C ABI: plugin_shutdown called ===");

    let mut instance = PLUGIN_INSTANCE.lock().unwrap();
    if let Some(ref mut plugin) = *instance {
        let result = RUNTIME.block_on(plugin.on_shutdown());

        if let Err(e) = result {
            eprintln!("plugin_shutdown: on_shutdown failed: {}", e);
            return -1;
        }
    }

    *instance = None;
    0 // 成功
}
