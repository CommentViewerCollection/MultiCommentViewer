// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use mcv_messages::Message as McvMessage;
use mcv_plugin_exe_interface::ExePluginClient;
use std::sync::Arc;
use tauri::{AppHandle, Emitter, State};
use tokio::sync::{Mutex, RwLock};
use uuid::Uuid;

/// アプリケーション状態
struct AppState {
    client: Arc<Mutex<Option<Arc<Mutex<ExePluginClient>>>>>,
    plugin_id: Arc<RwLock<Option<Uuid>>>,
    connected: Arc<RwLock<bool>>,
}

/// WebSocketサーバーに接続
#[tauri::command]
async fn connect_to_mcv(
    url: String,
    plugin_name: String,
    roles: Vec<String>,
    state: State<'_, AppState>,
    app: AppHandle,
) -> Result<String, String> {
    tracing::info!(url = %url, plugin_name = %plugin_name, "Connecting to MCV");

    // WebSocket接続
    let mut client = ExePluginClient::connect(&url)
        .await
        .map_err(|e| format!("Failed to connect: {}", e))?;

    let plugin_id = client.plugin_id();
    *state.plugin_id.write().await = Some(plugin_id);

    // plugin-helloを送信
    let roles_str: Vec<&str> = roles.iter().map(|s| s.as_str()).collect();
    client
        .send_plugin_hello(&plugin_name, roles_str)
        .await
        .map_err(|e| format!("Failed to send plugin-hello: {}", e))?;

    tracing::info!(plugin_id = %plugin_id, "Connected and sent plugin-hello");

    // get-pluginsを送信して既存プラグイン情報を取得
    client
        .send_get_plugins()
        .await
        .map_err(|e| format!("Failed to send get-plugins: {}", e))?;

    tracing::info!("Sent get-plugins request");

    // メッセージハンドラーを登録
    let app_clone = app.clone();
    client.on_message(move |message| {
        let app = app_clone.clone();
        // フロントエンドにメッセージを転送
        if let Err(e) = app.emit("message-received", &message) {
            tracing::error!(error = %e, "Failed to emit message-received event");
        }
        tracing::debug!(message_type = ?message.message_type, "Message received and forwarded to frontend");
    });

    // メッセージ受信ループをバックグラウンドで実行
    let client_arc = Arc::new(Mutex::new(client));
    let client_clone = Arc::clone(&client_arc);
    let connected_clone = Arc::clone(&state.connected);

    tokio::spawn(async move {
        let mut client = client_clone.lock().await;
        if let Err(e) = client.run().await {
            tracing::error!(error = %e, "WebSocket connection error");
            *connected_clone.write().await = false;
        }
    });

    *state.client.lock().await = Some(client_arc);
    *state.connected.write().await = true;

    Ok(plugin_id.to_string())
}

/// WebSocketから切断
#[tauri::command]
async fn disconnect_from_mcv(state: State<'_, AppState>) -> Result<(), String> {
    tracing::info!("Disconnecting from MCV");

    *state.client.lock().await = None;
    *state.connected.write().await = false;
    *state.plugin_id.write().await = None;

    Ok(())
}

/// メッセージを送信
#[tauri::command]
async fn send_message(
    message_json: String,
    state: State<'_, AppState>,
) -> Result<(), String> {
    tracing::debug!(message_json = %message_json, "Sending message");

    let message: McvMessage =
        serde_json::from_str(&message_json).map_err(|e| format!("Invalid JSON: {}", e))?;

    let client_option = state.client.lock().await;
    let client_arc = client_option
        .as_ref()
        .ok_or("Not connected to MCV")?;

    let mut client = client_arc.lock().await;
    client
        .send_message(message)
        .await
        .map_err(|e| format!("Failed to send message: {}", e))?;

    Ok(())
}

/// 接続状態を取得
#[tauri::command]
async fn get_connection_status(state: State<'_, AppState>) -> Result<bool, String> {
    Ok(*state.connected.read().await)
}

/// plugin_idを取得
#[tauri::command]
async fn get_plugin_id(state: State<'_, AppState>) -> Result<Option<String>, String> {
    Ok(state.plugin_id.read().await.map(|id| id.to_string()))
}

fn main() {
    // ロギング初期化
    tracing_subscriber::fmt()
        .with_env_filter(
            tracing_subscriber::EnvFilter::try_from_default_env()
                .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
        )
        .init();

    tracing::info!("Starting MCV EXE Plugin Sample");

    let app_state = AppState {
        client: Arc::new(Mutex::new(None)),
        plugin_id: Arc::new(RwLock::new(None)),
        connected: Arc::new(RwLock::new(false)),
    };

    tauri::Builder::default()
        .plugin(tauri_plugin_shell::init())
        .manage(app_state)
        .invoke_handler(tauri::generate_handler![
            connect_to_mcv,
            disconnect_from_mcv,
            send_message,
            get_connection_status,
            get_plugin_id
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
