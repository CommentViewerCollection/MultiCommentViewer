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
    plugin_id: Option<Uuid>,
    name: String,
    site_id: Option<Uuid>,
    site_name: Option<String>,
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

/// メッセージに応じて状態を更新
async fn handle_message_state_update(
    message: &McvMessage,
    plugins: Arc<RwLock<HashMap<Uuid, PluginInfo>>>,
    connections: Arc<RwLock<HashMap<Uuid, ConnectionInfo>>>,
    _sites: Arc<RwLock<HashMap<Uuid, SiteInfo>>>,
    _browsers: Arc<RwLock<HashMap<Uuid, BrowserInfo>>>,
    messages: Arc<RwLock<Vec<McvMessage>>>,
) {
    use mcv_messages::MessageType;

    match message.message_type {
        MessageType::PluginAdded => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                // 各フィールドの取得を個別に確認
                let plugin_id = payload.get("plugin_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());
                let name = payload.get("name").and_then(|v| v.as_str());
                let roles = payload.get("role").and_then(|v| v.as_array());
                let api_version = payload.get("api_version").and_then(|v| v.as_str());

                // 欠落フィールドをチェック
                if plugin_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: missing or invalid 'plugin_id' field");
                }
                if name.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: missing 'name' field");
                }
                if roles.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: missing 'role' field");
                }
                if api_version.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: missing 'api_version' field");
                }

                if let (Some(plugin_id), Some(name), Some(roles), Some(api_version)) = (plugin_id, name, roles, api_version) {
                    let plugin_info = PluginInfo {
                        plugin_id,
                        name: name.to_string(),
                        roles: roles.iter().filter_map(|v| v.as_str().map(|s| s.to_string())).collect(),
                        api_version: api_version.to_string(),
                    };
                    plugins.write().await.insert(plugin_id, plugin_info);
                    tracing::info!(target: "mcv::exe-plugin-sample", plugin_id = %plugin_id, name = %name, "Plugin added to state");
                } else {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: failed to parse required fields");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "plugin-added: failed to parse payload as JSON object");
            }
        }
        MessageType::ConnectionAdded => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                // 各フィールドの取得を個別に確認
                let connection_id = payload.get("connection_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());
                let name = payload.get("name").and_then(|v| v.as_str());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-added: missing or invalid 'connection_id' field");
                }
                if name.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-added: missing 'name' field");
                }

                if let (Some(connection_id), Some(name)) = (connection_id, name) {
                    let connection_info = ConnectionInfo {
                        connection_id,
                        plugin_id: payload.get("plugin_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok()),
                        name: name.to_string(),
                        site_id: payload.get("site_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok()),
                        site_name: payload.get("site_name").and_then(|v| v.as_str()).map(|s| s.to_string()),
                        url: payload.get("url").and_then(|v| v.as_str()).map(|s| s.to_string()),
                        browser_id: payload.get("browser_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok()),
                        status: "disconnected".to_string(),
                    };
                    connections.write().await.insert(connection_id, connection_info);
                    tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, name = %name, "Connection added to state");
                } else {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-added: failed to parse required fields");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connection-added: failed to parse payload as JSON object");
            }
        }
        MessageType::Connected => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                let connection_id = payload.get("connection_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connected: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    if let Some(conn) = connections.write().await.get_mut(&connection_id) {
                        conn.status = "connected".to_string();
                        tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection status updated to connected");
                    } else {
                        tracing::error!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "connected: connection_id not found in state");
                    }
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connected: failed to parse payload as JSON object");
            }
        }
        MessageType::Disconnected => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                let connection_id = payload.get("connection_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "disconnected: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    if let Some(conn) = connections.write().await.get_mut(&connection_id) {
                        conn.status = "disconnected".to_string();
                        tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection status updated to disconnected");
                    } else {
                        tracing::error!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "disconnected: connection_id not found in state");
                    }
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "disconnected: failed to parse payload as JSON object");
            }
        }
        MessageType::ConnectionRemoved => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                let connection_id = payload.get("connection_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-removed: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    connections.write().await.remove(&connection_id);
                    tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection removed from state");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connection-removed: failed to parse payload as JSON object");
            }
        }
        MessageType::PluginRemoved => {
            if let Ok(payload) = serde_json::from_value::<serde_json::Value>(message.payload.clone()) {
                let plugin_id = payload.get("plugin_id").and_then(|v| v.as_str()).and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if plugin_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-removed: missing or invalid 'plugin_id' field");
                }

                if let Some(plugin_id) = plugin_id {
                    plugins.write().await.remove(&plugin_id);
                    tracing::info!(target: "mcv::exe-plugin-sample", plugin_id = %plugin_id, "Plugin removed from state");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "plugin-removed: failed to parse payload as JSON object");
            }
        }
        MessageType::CommentReceived | MessageType::LogEntry => {
            // メッセージログに追加（最新1000件まで保持）
            let mut msgs = messages.write().await;
            msgs.push(message.clone());
            let len = msgs.len();
            if len > 1000 {
                msgs.drain(0..len - 1000);
            }
        }
        _ => {
            // その他のメッセージは状態更新不要
        }
    }
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
    let plugins_clone = state.plugins.clone();
    let connections_clone = state.connections.clone();
    let sites_clone = state.sites.clone();
    let browsers_clone = state.browsers.clone();
    let messages_clone = state.messages.clone();

    client
        .on_message(move |message| {
            let app = app_clone.clone();
            let plugins = plugins_clone.clone();
            let connections = connections_clone.clone();
            let sites = sites_clone.clone();
            let browsers = browsers_clone.clone();
            let messages = messages_clone.clone();

            tokio::spawn(async move {
                // 状態を更新
                handle_message_state_update(
                    &message,
                    plugins,
                    connections,
                    sites,
                    browsers,
                    messages,
                )
                .await;

                // フロントエンドにメッセージを転送
                if let Err(e) = app.emit("message-received", &message) {
                    tracing::error!(target:"mcv::exe-plugin-sample",error = %e, "Failed to emit message-received event");
                }
                tracing::debug!(target:"mcv::exe-plugin-sample",message_type = ?message.message_type, "Message received and forwarded to frontend");
            });
        })
        .await;

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

/// プラグイン一覧を取得
#[tauri::command]
async fn get_plugins(state: State<'_, AppState>) -> Result<Vec<PluginInfo>, String> {
    let plugins = state.plugins.read().await;
    Ok(plugins.values().cloned().collect())
}

/// 特定のプラグイン情報を取得
#[tauri::command]
async fn get_plugin(plugin_id: String, state: State<'_, AppState>) -> Result<Option<PluginInfo>, String> {
    let uuid = Uuid::parse_str(&plugin_id).map_err(|e| format!("Invalid UUID: {}", e))?;
    let plugins = state.plugins.read().await;
    Ok(plugins.get(&uuid).cloned())
}

/// 接続一覧を取得
#[tauri::command]
async fn get_connections(state: State<'_, AppState>) -> Result<Vec<ConnectionInfo>, String> {
    let connections = state.connections.read().await;
    Ok(connections.values().cloned().collect())
}

/// 特定の接続情報を取得
#[tauri::command]
async fn get_connection(connection_id: String, state: State<'_, AppState>) -> Result<Option<ConnectionInfo>, String> {
    let uuid = Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid UUID: {}", e))?;
    let connections = state.connections.read().await;
    Ok(connections.get(&uuid).cloned())
}

/// メッセージログを取得（最新N件）
#[tauri::command]
async fn get_messages(limit: Option<usize>, state: State<'_, AppState>) -> Result<Vec<McvMessage>, String> {
    let messages = state.messages.read().await;
    let limit = limit.unwrap_or(100);
    let start = if messages.len() > limit {
        messages.len() - limit
    } else {
        0
    };
    Ok(messages[start..].to_vec())
}

/// メッセージログをクリア
#[tauri::command]
async fn clear_messages(state: State<'_, AppState>) -> Result<(), String> {
    state.messages.write().await.clear();
    Ok(())
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
            get_plugins,
            get_plugin,
            get_connections,
            get_connection,
            get_messages,
            clear_messages,
            get_plugin_id
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
