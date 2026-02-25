// Prevents additional console window on Windows in release, DO NOT REMOVE!!
#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod ws_tracing;
use mcv_messages::{
    AddSitePayload, CommentReceivedPayload, GetLogsDirPayload, LogsDirAckPayload,
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginRemovedPayload,
    SiteId,
};

/// リプレイ専用サイトの静的 ID（"replay_{uuid}" 形式）
const REPLAY_SITE_ID: &str = "replay_00000000-0000-0000-0000-000000000001";
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
    // SQLite セッション DB（起動ごとに session_{timestamp}.db を作成）
    db: Arc<std::sync::Mutex<Option<rusqlite::Connection>>>,
    // ログディレクトリパス（GetLogsDir で取得）
    logs_dir: Arc<RwLock<Option<String>>>,
}

/// メッセージに応じて状態を更新し、変化があればフロントエンドに通知する
#[allow(clippy::too_many_arguments)]
async fn handle_message_state_update(
    message: &McvMessage,
    plugins: Arc<RwLock<HashMap<Uuid, PluginInfo>>>,
    connections: Arc<RwLock<HashMap<Uuid, ConnectionInfo>>>,
    _sites: Arc<RwLock<HashMap<Uuid, SiteInfo>>>,
    _browsers: Arc<RwLock<HashMap<Uuid, BrowserInfo>>>,
    messages: Arc<RwLock<Vec<McvMessage>>>,
    db: Arc<std::sync::Mutex<Option<rusqlite::Connection>>>,
    logs_dir: Arc<RwLock<Option<String>>>,
    client: Arc<ExePluginClient>,
    plugin_id: Uuid,
    app: AppHandle,
) {
    use mcv_messages::MessageType;

    match message.message_type {
        MessageType::PluginAdded => {
            let self_pid = plugin_id; // 関数パラメータを保存（後でローカル変数に隠される前に）
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                // 各フィールドの取得を個別に確認
                let plugin_id = payload
                    .get("plugin_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());
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

                if let (Some(plugin_id), Some(name), Some(roles), Some(api_version)) =
                    (plugin_id, name, roles, api_version)
                {
                    let plugin_info = PluginInfo {
                        plugin_id,
                        name: name.to_string(),
                        roles: roles
                            .iter()
                            .filter_map(|v| v.as_str().map(|s| s.to_string()))
                            .collect(),
                        api_version: api_version.to_string(),
                    };
                    plugins.write().await.insert(plugin_id, plugin_info);
                    tracing::info!(target: "mcv::exe-plugin-sample", plugin_id = %plugin_id, name = %name, "Plugin added to state");
                    let _ = app.emit("plugins-updated", ());

                    // 自分自身の PluginAdded なら replay サイトを AddSite で登録
                    if plugin_id == self_pid {
                        let add_site_msg = McvMessage::new_request(
                            MessageType::AddSite,
                            MessageSource::Plugin { plugin_id: self_pid },
                            MessageDestination::Core,
                            serde_json::to_value(AddSitePayload {
                                site_id: SiteId::from_string(REPLAY_SITE_ID.to_string()),
                                display_name: "メッセージ再生".to_string(),
                                options_schema: serde_json::json!({
                                    "type": "object",
                                    "properties": {
                                        "url": {
                                            "type": "string",
                                            "title": "セッションファイル名",
                                            "description": "再生するセッションDBファイル名 (例: session_1234567890.db)"
                                        }
                                    }
                                }),
                            })
                            .unwrap(),
                        );
                        if let Err(e) = client.send_message(add_site_msg) {
                            tracing::error!(
                                target: "mcv::exe-plugin-sample",
                                error = %e,
                                "Failed to send AddSite for replay"
                            );
                        } else {
                            tracing::info!(
                                target: "mcv::exe-plugin-sample",
                                "AddSite sent for replay site"
                            );
                        }
                    }
                } else {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-added: failed to parse required fields");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "plugin-added: failed to parse payload as JSON object");
            }
        }
        MessageType::ConnectionAdded => {
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                // 各フィールドの取得を個別に確認
                let connection_id = payload
                    .get("connection_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());
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
                        plugin_id: payload
                            .get("plugin_id")
                            .and_then(|v| v.as_str())
                            .and_then(|s| Uuid::parse_str(s).ok()),
                        name: name.to_string(),
                        site_id: payload
                            .get("site_id")
                            .and_then(|v| v.as_str())
                            .and_then(|s| Uuid::parse_str(s).ok()),
                        site_name: payload
                            .get("site_name")
                            .and_then(|v| v.as_str())
                            .map(|s| s.to_string()),
                        url: payload
                            .get("url")
                            .and_then(|v| v.as_str())
                            .map(|s| s.to_string()),
                        browser_id: payload
                            .get("browser_id")
                            .and_then(|v| v.as_str())
                            .and_then(|s| Uuid::parse_str(s).ok()),
                        status: "disconnected".to_string(),
                    };
                    connections
                        .write()
                        .await
                        .insert(connection_id, connection_info);
                    tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, name = %name, "Connection added to state");
                    let _ = app.emit("connections-updated", ());

                } else {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-added: failed to parse required fields");
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connection-added: failed to parse payload as JSON object");
            }
        }
        MessageType::Connected => {
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                let connection_id = payload
                    .get("connection_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connected: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    if let Some(conn) = connections.write().await.get_mut(&connection_id) {
                        conn.status = "connected".to_string();
                        tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection status updated to connected");
                        let _ = app.emit("connections-updated", ());
                    } else {
                        tracing::error!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "connected: connection_id not found in state");
                    }
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connected: failed to parse payload as JSON object");
            }
        }
        MessageType::Disconnected => {
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                let connection_id = payload
                    .get("connection_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "disconnected: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    if let Some(conn) = connections.write().await.get_mut(&connection_id) {
                        conn.status = "disconnected".to_string();
                        tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection status updated to disconnected");
                        let _ = app.emit("connections-updated", ());
                    } else {
                        tracing::error!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "disconnected: connection_id not found in state");
                    }
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "disconnected: failed to parse payload as JSON object");
            }
        }
        MessageType::ConnectionRemoved => {
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                let connection_id = payload
                    .get("connection_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if connection_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "connection-removed: missing or invalid 'connection_id' field");
                }

                if let Some(connection_id) = connection_id {
                    connections.write().await.remove(&connection_id);
                    tracing::info!(target: "mcv::exe-plugin-sample", connection_id = %connection_id, "Connection removed from state");
                    let _ = app.emit("connections-updated", ());
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "connection-removed: failed to parse payload as JSON object");
            }
        }
        MessageType::PluginRemoved => {
            if let Ok(payload) =
                serde_json::from_value::<serde_json::Value>(message.payload.clone())
            {
                let plugin_id = payload
                    .get("plugin_id")
                    .and_then(|v| v.as_str())
                    .and_then(|s| Uuid::parse_str(s).ok());

                // 欠落フィールドをチェック
                if plugin_id.is_none() {
                    tracing::error!(target: "mcv::exe-plugin-sample", payload = ?payload, "plugin-removed: missing or invalid 'plugin_id' field");
                }

                if let Some(plugin_id) = plugin_id {
                    plugins.write().await.remove(&plugin_id);
                    tracing::info!(target: "mcv::exe-plugin-sample", plugin_id = %plugin_id, "Plugin removed from state");
                    let _ = app.emit("plugins-updated", ());
                }
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "plugin-removed: failed to parse payload as JSON object");
            }
        }
        MessageType::GetPlugins => {
            // get-pluginsレスポンス: GetPluginsPayload を処理してstate.pluginsに一括登録
            if let Ok(payload) =
                serde_json::from_value::<mcv_messages::GetPluginsPayload>(message.payload.clone())
            {
                let mut plugins_guard = plugins.write().await;
                for plugin_added in payload.plugins {
                    let plugin_id = plugin_added.plugin_id;
                    let plugin_info = PluginInfo {
                        plugin_id,
                        name: plugin_added.name,
                        roles: plugin_added.role,
                        api_version: plugin_added.api_version,
                    };
                    plugins_guard.insert(plugin_id, plugin_info);
                    tracing::info!(target: "mcv::exe-plugin-sample", plugin_id = %plugin_id, "Plugin added to state via get-plugins response");
                }
                drop(plugins_guard);
                let _ = app.emit("plugins-updated", ());
            } else {
                tracing::error!(target: "mcv::exe-plugin-sample", payload = ?message.payload, "get-plugins: failed to parse GetPluginsPayload");
            }
        }
        MessageType::LogEntry => {
            // メッセージログに追加（最新1000件まで保持）
            let mut msgs = messages.write().await;
            msgs.push(message.clone());
            let len = msgs.len();
            if len > 1000 {
                msgs.drain(0..len - 1000);
            }
        }
        MessageType::CommentReceived => {
            // メッセージログに追加（最新1000件まで保持）
            {
                let mut msgs = messages.write().await;
                msgs.push(message.clone());
                let len = msgs.len();
                if len > 1000 {
                    msgs.drain(0..len - 1000);
                }
            }

            // SQLite に保存
            if let Ok(payload) =
                serde_json::from_value::<CommentReceivedPayload>(message.payload.clone())
            {
                let envelope = &payload.envelope;
                let messages_json =
                    serde_json::to_string(&envelope.messages).unwrap_or_default();
                let db_clone = Arc::clone(&db);
                let (eid, cid, rat, raw) = (
                    envelope.event_id.to_string(),
                    envelope.connection_id.to_string(),
                    envelope.received_at,
                    envelope.raw_message.clone(),
                );
                tokio::task::spawn_blocking(move || {
                    if let Ok(guard) = db_clone.lock() {
                        if let Some(conn) = guard.as_ref() {
                            let result = conn.execute(
                                "INSERT OR IGNORE INTO envelopes \
                                 (event_id, connection_id, received_at, messages_json, raw_message) \
                                 VALUES (?1, ?2, ?3, ?4, ?5)",
                                rusqlite::params![eid, cid, rat, messages_json, raw],
                            );
                            if let Err(e) = result {
                                tracing::error!(
                                    target: "mcv::exe-plugin-sample",
                                    error = %e,
                                    "Failed to insert envelope into SQLite"
                                );
                            }
                        }
                    }
                });
            }
        }
        MessageType::LogsDirAck => {
            // logs ディレクトリパスを取得し、セッション DB を作成する
            if let Ok(payload) =
                serde_json::from_value::<LogsDirAckPayload>(message.payload.clone())
            {
                let path = payload.path.clone();
                if path.is_empty() {
                    tracing::warn!(
                        target: "mcv::exe-plugin-sample",
                        "LogsDirAck received but path is empty"
                    );
                } else {
                    *logs_dir.write().await = Some(path.clone());
                    let db_clone = Arc::clone(&db);
                    tokio::task::spawn_blocking(move || {
                        // ディレクトリが存在しない場合は作成
                        if let Err(e) = std::fs::create_dir_all(&path) {
                            tracing::error!(
                                target: "mcv::exe-plugin-sample",
                                error = %e,
                                path = %path,
                                "Failed to create logs directory"
                            );
                            return;
                        }
                        let timestamp = std::time::SystemTime::now()
                            .duration_since(std::time::UNIX_EPOCH)
                            .unwrap_or_default()
                            .as_secs();
                        let db_path = format!("{}/session_{}.db", path, timestamp);
                        tracing::info!(
                            target: "mcv::exe-plugin-sample",
                            db_path = %db_path,
                            "Creating session SQLite DB"
                        );
                        match rusqlite::Connection::open(&db_path) {
                            Ok(conn) => {
                                let schema = r#"
                                    PRAGMA journal_mode=WAL;
                                    CREATE TABLE IF NOT EXISTS envelopes (
                                        id            INTEGER PRIMARY KEY AUTOINCREMENT,
                                        event_id      TEXT NOT NULL UNIQUE,
                                        connection_id TEXT NOT NULL,
                                        received_at   INTEGER NOT NULL,
                                        messages_json TEXT NOT NULL,
                                        raw_message   TEXT
                                    );
                                    CREATE INDEX IF NOT EXISTS idx_conn
                                        ON envelopes(connection_id);
                                    CREATE INDEX IF NOT EXISTS idx_time
                                        ON envelopes(received_at);
                                "#;
                                if let Err(e) = conn.execute_batch(schema) {
                                    tracing::error!(
                                        target: "mcv::exe-plugin-sample",
                                        error = %e,
                                        "Failed to create SQLite schema"
                                    );
                                    return;
                                }
                                if let Ok(mut guard) = db_clone.lock() {
                                    *guard = Some(conn);
                                    tracing::info!(
                                        target: "mcv::exe-plugin-sample",
                                        "Session SQLite DB initialized"
                                    );
                                }
                            }
                            Err(e) => {
                                tracing::error!(
                                    target: "mcv::exe-plugin-sample",
                                    error = %e,
                                    db_path = %db_path,
                                    "Failed to open SQLite DB"
                                );
                            }
                        }
                    });
                }
            }
        }
        MessageType::Connect => {
            // replay サイトへの Connect を受信した場合、replay を開始する
            if let Ok(payload) =
                serde_json::from_value::<mcv_messages::ConnectPayload>(message.payload.clone())
            {
                let site_id = payload.site.id.to_string();
                if site_id == REPLAY_SITE_ID {
                    let trigger_conn_id = payload.connection_id;

                    // URL 欄からファイル名を取得
                    let file_name = payload
                        .input
                        .extra
                        .get("url")
                        .and_then(|v| v.as_str())
                        .map(|s| s.to_string());

                    if let Some(file_name) = file_name {
                        let logs_dir_guard = logs_dir.read().await;
                        let file_path = if let Some(dir) = logs_dir_guard.as_ref() {
                            if std::path::Path::new(&file_name).is_absolute() {
                                file_name.clone()
                            } else {
                                format!("{}/{}", dir, file_name)
                            }
                        } else {
                            file_name.clone()
                        };
                        drop(logs_dir_guard);

                        // Connected を即座に送信（apps/mcv の接続ボタンを更新）
                        let connected_msg = McvMessage::new_notification(
                            MessageType::Connected,
                            MessageSource::Plugin { plugin_id },
                            MessageDestination::Core,
                            serde_json::to_value(mcv_messages::ConnectedPayload {
                                connection_id: trigger_conn_id,
                            })
                            .unwrap(),
                        );
                        if let Err(e) = client.send_message(connected_msg) {
                            tracing::error!(
                                target: "mcv::exe-plugin-sample",
                                error = %e,
                                "Failed to send Connected for trigger connection"
                            );
                        }

                        // replay タスクを起動（trigger_conn_id を使って全コメントを送信）
                        let client_clone = client.clone();
                        tokio::spawn(async move {
                            run_replay_simple(
                                file_path,
                                Some(trigger_conn_id),
                                client_clone,
                                plugin_id,
                            )
                            .await;
                        });
                    } else {
                        tracing::warn!(
                            target: "mcv::exe-plugin-sample",
                            "Connect to replay site but no file URL provided"
                        );
                    }
                }
            }
        }
        _ => {
            // その他のメッセージは状態更新不要
        }
    }
}

/// replay タスク: 保存されたエンベロープを元の間隔で CommentReceived として送信する
/// trigger_conn_id が Some の場合はその connection_id を全コメントに使用し、完了後 Disconnected を送信する
/// trigger_conn_id が None の場合は新しい UUID を生成して使用する（exe-plugin-sample UI からの直接再生用）
async fn run_replay_simple(
    file_path: String,
    trigger_conn_id: Option<Uuid>,
    client: Arc<ExePluginClient>,
    plugin_id: Uuid,
) {
    tracing::info!(
        target: "mcv::exe-plugin-sample",
        file_path = %file_path,
        trigger_conn_id = ?trigger_conn_id,
        "Starting replay"
    );

    // DB からエンベロープを received_at 昇順で全件取得
    #[derive(Debug)]
    struct EnvelopeRow {
        received_at: i64,
        messages_json: String,
        raw_message: Option<String>,
    }

    let fp = file_path.clone();
    let rows: Vec<EnvelopeRow> = match tokio::task::spawn_blocking(move || {
        let conn = rusqlite::Connection::open(&fp)
            .map_err(|e| format!("Failed to open replay DB: {}", e))?;

        let mut stmt = conn
            .prepare(
                "SELECT received_at, messages_json, raw_message \
                 FROM envelopes ORDER BY received_at ASC",
            )
            .map_err(|e| format!("Failed to prepare: {}", e))?;

        let rows: Vec<EnvelopeRow> = stmt
            .query_map([], |row| {
                Ok(EnvelopeRow {
                    received_at: row.get(0)?,
                    messages_json: row.get(1)?,
                    raw_message: row.get(2)?,
                })
            })
            .map_err(|e| format!("Failed to query: {}", e))?
            .filter_map(|r| r.ok())
            .collect();

        Ok::<Vec<EnvelopeRow>, String>(rows)
    })
    .await
    {
        Ok(Ok(rows)) => rows,
        Ok(Err(e)) => {
            tracing::error!(
                target: "mcv::exe-plugin-sample",
                error = %e,
                file_path = %file_path,
                "Failed to read envelopes from replay DB"
            );
            return;
        }
        Err(e) => {
            tracing::error!(
                target: "mcv::exe-plugin-sample",
                error = %e,
                "spawn_blocking panicked while reading replay DB envelopes"
            );
            return;
        }
    };

    if rows.is_empty() {
        tracing::warn!(
            target: "mcv::exe-plugin-sample",
            "No envelopes found in replay DB"
        );
        return;
    }

    // trigger_conn_id が None の場合は新しい UUID を生成
    let conn_id = trigger_conn_id.unwrap_or_else(Uuid::new_v4);
    let base_replay_at = rows[0].received_at;
    let base_now = chrono::Utc::now().timestamp();

    for row in &rows {
        // 元の間隔で待機
        let target_offset = row.received_at - base_replay_at;
        let elapsed = chrono::Utc::now().timestamp() - base_now;
        let wait_secs = (target_offset - elapsed).max(0) as u64;
        if wait_secs > 0 {
            tokio::time::sleep(std::time::Duration::from_secs(wait_secs)).await;
        }

        let messages: Vec<mcv_messages::ProviderMessage> =
            serde_json::from_str(&row.messages_json).unwrap_or_default();

        let envelope = mcv_messages::McvEnvelope {
            event_id: Uuid::new_v4(),
            connection_id: conn_id,
            messages,
            received_at: chrono::Utc::now().timestamp(),
            raw_message: row.raw_message.clone(),
        };

        let payload = mcv_messages::CommentReceivedPayload {
            connection_id: conn_id,
            envelope,
        };

        let msg = McvMessage::new_notification(
            MessageType::CommentReceived,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(payload).unwrap(),
        );

        if let Err(e) = client.send_message(msg) {
            tracing::error!(
                target: "mcv::exe-plugin-sample",
                error = %e,
                "Failed to send CommentReceived during replay"
            );
        }
    }

    // 全件送信完了後、trigger_conn_id がある場合は Disconnected を送信
    if let Some(tcid) = trigger_conn_id {
        let disconnect_msg = McvMessage::new_notification(
            MessageType::Disconnected,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(mcv_messages::DisconnectedPayload {
                connection_id: tcid,
            })
            .unwrap(),
        );
        if let Err(e) = client.send_message(disconnect_msg) {
            tracing::error!(
                target: "mcv::exe-plugin-sample",
                error = %e,
                "Failed to send Disconnected after replay"
            );
        }
    }

    tracing::info!(
        target: "mcv::exe-plugin-sample",
        rows = rows.len(),
        "Replay completed"
    );
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

    // GetLogsDir リクエストを送信（LogsDirAck でセッション DB を作成する）
    let get_logs_dir_msg = McvMessage::new_request(
        MessageType::GetLogsDir,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(GetLogsDirPayload {}).unwrap(),
    );
    client
        .send_message(get_logs_dir_msg)
        .map_err(|e| format!("Failed to send GetLogsDir: {}", e))?;

    tracing::info!(target:"mcv::exe-plugin-sample","Sent GetLogsDir request");

    // メッセージハンドラーを登録
    let app_clone = app.clone();
    let plugins_clone = state.plugins.clone();
    let connections_clone = state.connections.clone();
    let sites_clone = state.sites.clone();
    let browsers_clone = state.browsers.clone();
    let messages_clone = state.messages.clone();
    let db_clone = state.db.clone();
    let logs_dir_clone = state.logs_dir.clone();
    let client_for_handler = client.clone();

    client
        .on_message(move |message| {
            let app = app_clone.clone();
            let plugins = plugins_clone.clone();
            let connections = connections_clone.clone();
            let sites = sites_clone.clone();
            let browsers = browsers_clone.clone();
            let messages = messages_clone.clone();
            let db = db_clone.clone();
            let logs_dir = logs_dir_clone.clone();
            let client_inner = client_for_handler.clone();

            tokio::spawn(async move {
                // 状態を更新し、変化があればフロントエンドに通知
                handle_message_state_update(
                    &message,
                    plugins,
                    connections,
                    sites,
                    browsers,
                    messages,
                    db,
                    logs_dir,
                    client_inner,
                    plugin_id,
                    app.clone(),
                )
                .await;

                // ログ表示用にメッセージ本文を転送
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
async fn get_plugin(
    plugin_id: String,
    state: State<'_, AppState>,
) -> Result<Option<PluginInfo>, String> {
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
async fn get_connection(
    connection_id: String,
    state: State<'_, AppState>,
) -> Result<Option<ConnectionInfo>, String> {
    let uuid = Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid UUID: {}", e))?;
    let connections = state.connections.read().await;
    Ok(connections.get(&uuid).cloned())
}

/// メッセージログを取得（最新N件）
#[tauri::command]
async fn get_messages(
    limit: Option<usize>,
    state: State<'_, AppState>,
) -> Result<Vec<McvMessage>, String> {
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

    let client = {
        let guard = state.client.read().await;
        guard.clone()
    };

    if let Some(client) = client {
        // PluginRemoved メッセージを送信してプラグイン登録を抹消
        let plugin_id_guard = state.plugin_id.read().await;
        if let Some(pid) = *plugin_id_guard {
            let msg = McvMessage::new_notification(
                MessageType::PluginRemoved,
                MessageSource::Plugin { plugin_id: pid },
                MessageDestination::Core,
                serde_json::to_value(PluginRemovedPayload { plugin_id: pid }).unwrap(),
            );
            let _ = client.send_message(msg);
            tracing::info!(target:"mcv::exe-plugin-sample", plugin_id = %pid, "Sent PluginRemoved before disconnect");
        }
        drop(plugin_id_guard);

        // WebSocket Closeフレームを送信
        let _ = client.send_close();

        // メッセージ送信を確実に行うため少し待機
        tokio::time::sleep(std::time::Duration::from_millis(100)).await;
    }

    *state.client.write().await = None;
    *state.connected.write().await = false;
    *state.plugin_id.write().await = None;

    // セッション DB を閉じる
    if let Ok(mut guard) = state.db.lock() {
        *guard = None;
        tracing::info!(target:"mcv::exe-plugin-sample","Session DB closed");
    }
    *state.logs_dir.write().await = None;

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

/// logs_dir 内の session_*.db ファイル名一覧を返す
#[tauri::command]
async fn list_session_files(state: State<'_, AppState>) -> Result<Vec<String>, String> {
    let logs_dir = state.logs_dir.read().await;
    let dir = match logs_dir.as_ref() {
        Some(d) => d.clone(),
        None => return Ok(vec![]),
    };
    drop(logs_dir);

    tokio::task::spawn_blocking(move || {
        let entries =
            std::fs::read_dir(&dir).map_err(|e| format!("Failed to read logs dir: {}", e))?;
        let mut files: Vec<String> = entries
            .filter_map(|e| e.ok())
            .filter_map(|e| {
                let name = e.file_name().to_string_lossy().into_owned();
                if name.starts_with("session_") && name.ends_with(".db") {
                    Some(name)
                } else {
                    None
                }
            })
            .collect();
        files.sort();
        Ok(files)
    })
    .await
    .map_err(|e| format!("Task error: {}", e))?
}

/// exe-plugin-sample UI からリプレイを直接開始する
#[tauri::command]
async fn start_replay(file_name: String, state: State<'_, AppState>) -> Result<(), String> {
    let logs_dir = state.logs_dir.read().await;
    let dir = logs_dir
        .as_ref()
        .ok_or_else(|| "接続されていません".to_string())?
        .clone();
    drop(logs_dir);

    let file_path = if std::path::Path::new(&file_name).is_absolute() {
        file_name
    } else {
        format!("{}/{}", dir, file_name)
    };

    let client = {
        let guard = state.client.read().await;
        guard
            .clone()
            .ok_or_else(|| "Not connected to MCV".to_string())?
    };
    let plugin_id = state
        .plugin_id
        .read()
        .await
        .ok_or_else(|| "Plugin ID not set".to_string())?;

    tokio::spawn(async move {
        run_replay_simple(file_path, None, client, plugin_id).await;
    });

    Ok(())
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
        db: Arc::new(std::sync::Mutex::new(None)),
        logs_dir: Arc::new(RwLock::new(None)),
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
            get_plugin_id,
            list_session_files,
            start_replay,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
