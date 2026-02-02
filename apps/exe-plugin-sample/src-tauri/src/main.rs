// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod ws_tracing;
use mcv_messages::Message as McvMessage;
use mcv_plugin_exe_interface::ExePluginClient;
use std::collections::HashMap;
use std::sync::Arc;
use tauri::{AppHandle, Emitter, State};
use tokio::sync::RwLock;
use tracing_subscriber::prelude::*;
use uuid::Uuid;

/// プラグイン情報
#[derive(Clone, serde::Serialize, Debug)]
struct PluginInfo {
    plugin_id: Uuid,
    name: String,
    roles: Vec<String>,
    api_version: String,
}

/// 接続情報
#[derive(Clone, serde::Serialize, Debug)]
struct ConnectionInfo {
    connection_id: Uuid,
    name: String,
    site_id: Option<Uuid>,
    url: Option<String>,
    browser_id: Option<Uuid>,
    status: String, // "disconnected", "connecting", "connected"
}

/// サイト情報
#[derive(Clone, serde::Serialize, Debug)]
struct SiteInfo {
    site_id: Uuid,
    name: String,
    service_type: String,
}

/// ブラウザ情報
#[derive(Clone, serde::Serialize, Debug)]
struct BrowserInfo {
    browser_id: Uuid,
    name: String,
    browser_type: String,
}

/// アプリケーション状態
struct AppState {
    client: RwLock<Option<Arc<ExePluginClient>>>,
    plugin_id: Arc<RwLock<Option<Uuid>>>,
    connected: Arc<RwLock<bool>>,
    // 状態管理用フィールド
    plugins: Arc<RwLock<HashMap<Uuid, PluginInfo>>>,
    connections: Arc<RwLock<HashMap<Uuid, ConnectionInfo>>>,
    sites: Arc<RwLock<HashMap<Uuid, SiteInfo>>>,
    browsers: Arc<RwLock<HashMap<Uuid, BrowserInfo>>>,
    messages: Arc<RwLock<Vec<McvMessage>>>,
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
    tracing::info!(target:"mcv::exe-plugin-sample",url = %url, plugin_name = %plugin_name, "Connecting to MCV");

    // WebSocket接続
    let client = ExePluginClient::connect(&url)
        .await
        .map_err(|e| format!("Failed to connect: {}", e))?;
    // let client = Arc::clone(&client);

    let plugin_id = client.plugin_id();
    *state.plugin_id.write().await = Some(plugin_id);

    // tracing_subscriberを初期化（一度だけ）
    let ws_layer = ws_tracing::WsLayer::new(Some(client.clone())).with_filter(
        tracing_subscriber::filter::filter_fn(|meta| meta.target().starts_with("mcv")),
    );

    let _ = tracing_subscriber::registry()
        .with(ws_layer)
        .with(tracing_subscriber::fmt::layer())
        .try_init(); // try_init()なら複数回呼び出しても2回目以降は無視される

    // plugin-helloを送信
    let roles_str: Vec<&str> = roles.iter().map(|s| s.as_str()).collect();
    client
        .send_plugin_hello(&plugin_name, roles_str)
        .await
        .map_err(|e| format!("Failed to send plugin-hello: {}", e))?;

    tracing::info!(target:"mcv::exe-plugin-sample", plugin_id = %plugin_id, "Connected and sent plugin-hello");

    // get-pluginsを送信して既存プラグイン情報を取得
    client
        .send_get_plugins()
        .await
        .map_err(|e| format!("Failed to send get-plugins: {}", e))?;

    tracing::info!(target:"mcv::exe-plugin-sample","Sent get-plugins request");

    // メッセージハンドラーを登録
    let app_clone = app.clone();
    client.on_message(move |message| {
        let app = app_clone.clone();
        // フロントエンドにメッセージを転送
        if let Err(e) = app.emit("message-received", &message) {
            tracing::error!(target:"mcv::exe-plugin-sample",error = %e, "Failed to emit message-received event");
        }
        tracing::debug!(target:"mcv::exe-plugin-sample",message_type = ?message.message_type, "Message received and forwarded to frontend");
    }).await;

    // tokio::spawn(async move {
    //     //let mut client = client_clone.lock().await;
    //     if let Err(e) = client.run().await {
    //         tracing::error!(target:"mcv::exe-plugin-sample",error = %e, "WebSocket connection error");
    //         *connected_clone.write().await = false;
    //     }
    // });

    *state.client.write().await = Some(client);
    *state.connected.write().await = true;

    Ok(plugin_id.to_string())
}

/// WebSocketから切断
#[tauri::command]
async fn disconnect_from_mcv(state: State<'_, AppState>) -> Result<(), String> {
    tracing::info!(target:"mcv::exe-plugin-sample","Disconnecting from MCV");

    *state.client.write().await = None;
    *state.connected.write().await = false;
    *state.plugin_id.write().await = None;

    Ok(())
}

/// メッセージを送信
#[tauri::command]
async fn send_message(message_json: String, state: State<'_, AppState>) -> Result<(), String> {
    tracing::debug!(target:"mcv::exe-plugin-sample",message_json = %message_json, "Sending message");

    let message: McvMessage =
        serde_json::from_str(&message_json).map_err(|e| format!("Invalid JSON: {}", e))?;

    let client = {
        let guard = state.client.read().await;
        guard.clone().ok_or("Not connected to MCV")?
    };

    // ② lock の外で送信
    client
        .send_message(message)
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
    // // ロギング初期化
    // tracing_subscriber::fmt()
    //     .with_env_filter(
    //         tracing_subscriber::EnvFilter::try_from_default_env()
    //             .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
    //     )
    //     .init();

    tracing::info!(target:"mcv::exe-plugin-sample","Starting MCV EXE Plugin Sample");

    let app_state = AppState {
        client: RwLock::new(None),
        plugin_id: Arc::new(RwLock::new(None)),
        connected: Arc::new(RwLock::new(false)),
        plugins: Arc::new(RwLock::new(HashMap::new())),
        connections: Arc::new(RwLock::new(HashMap::new())),
        sites: Arc::new(RwLock::new(HashMap::new())),
        browsers: Arc::new(RwLock::new(HashMap::new())),
        messages: Arc::new(RwLock::new(Vec::new())),
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
