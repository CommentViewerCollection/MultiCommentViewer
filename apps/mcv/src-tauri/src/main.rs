#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod comment_store;
mod crash_handler;

use actix::prelude::*;
use mcv_common::{BrowserId, SiteId};
use mcv_core::{
    BrowserInfo as CoreBrowserInfo, // mcv-coreから明示的にインポート
    ConnectionInfo,
    CoreActor,
    DetectUrl,
    GetBrowsers,
    GetConnections,
    GetLogicalPlugins,
    GetSites,
    PluginManager,
    RemoveConnection,
    RenameConnection,
    ScanAndLoadNewPlugins,
    SendRequest,
    SetConnectionSite,
    SiteInfo as CoreSiteInfo,
    UpdateConnectionSettings,
};
use mcv_messages::{
    self, BrowserInfo as MsgBrowserInfo, CommentReceivedPayload, ConnectPayload,
    ConnectionInputSchemaPayload, DisconnectPayload, DisconnectedPayload, FetchAccountInfoPayload,
    GetConnectionInputSchemaPayload, GetSendCommentSchemaPayload, InputInfo, Message as McvMessage,
    MessageDestination, MessageSource, MessageType, Money, PluginId, ProviderContent,
    ProviderMessageKind, SendCommentPayload, SendCommentSchemaPayload, SiteInfo as MsgSiteInfo,
    SystemKind,
};
use mcv_updater::{McvUpdateInfo, PluginListItem, PluginVersionDetail, UpdateChecker};
use std::path::PathBuf;
use std::sync::{Arc, Mutex};
use tauri::{AppHandle, Emitter, Manager, State};
use uuid::Uuid;
use zip::ZipArchive;

const API_BASE_URL: &str = "http://localhost";

/// McvEnvelope をフロントエンド表示用に変換した行
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct CommentRow {
    id: String,
    user_name: Vec<mcv_messages::MessagePart>,
    user_id: String,
    badges: Vec<mcv_messages::ProviderBadge>,
    text: Vec<mcv_messages::MessagePart>,
    timestamp: i64,
    connection_id: String,
    /// false の場合は非表示（承認待ちプレースホルダー）
    is_visible: bool,
    /// 置き換え・削除対象の CommentRow の id
    replaces_id: Option<String>,
    /// メッセージ種別: "chat" | "history_chat" | "monetary" | "system"
    kind: String,
    /// ユーザーアイコン URL（省略可）
    #[serde(skip_serializing_if = "Option::is_none")]
    avatar_url: Option<String>,
    /// 投げ銭・スーパーチャットの金額テキスト（例: "¥8,000"）。monetary 種別のみ設定。
    #[serde(skip_serializing_if = "Option::is_none")]
    amount_text: Option<String>,
}

/// Money を表示用テキストに変換する（例: "¥8,000"、"$10.00"）
fn format_money(money: &Money) -> String {
    match money.currency.as_str() {
        "JPY" => format!("¥{}", money.value_minor),
        "KRW" => format!("₩{}", money.value_minor),
        "TWD" => format!("NT${}", money.value_minor),
        "USD" => format!("${:.2}", money.value_minor as f64 / 100.0),
        "EUR" => format!("€{:.2}", money.value_minor as f64 / 100.0),
        "GBP" => format!("£{:.2}", money.value_minor as f64 / 100.0),
        "AUD" => format!("A${:.2}", money.value_minor as f64 / 100.0),
        "CAD" => format!("C${:.2}", money.value_minor as f64 / 100.0),
        "HKD" => format!("HK${:.2}", money.value_minor as f64 / 100.0),
        _ if !money.currency.is_empty() => {
            format!("{} {}", money.currency, money.value_minor)
        }
        _ => format!("{}", money.value_minor),
    }
}

/// "delete-all-by-user" イベントのペイロード
#[derive(Debug, Clone, serde::Serialize)]
struct DeleteAllByUserPayload {
    user_id: String,
    connection_id: String,
}

/// McvEnvelope の ProviderMessage を CommentRow のリストに変換する
/// MessageDeleteAll は CommentRow に変換しない（別イベントで処理）
fn envelope_to_comment_rows(envelope: &mcv_messages::McvEnvelope) -> Vec<CommentRow> {
    envelope
        .messages
        .iter()
        .filter(|msg| {
            !matches!(
                &msg.kind,
                ProviderMessageKind::System(SystemKind::MessageDeleteAll { .. })
            )
        })
        .map(|msg| {
            let extract_text = |content: &ProviderContent| match content {
                ProviderContent::Text { text } => text.clone(),
                ProviderContent::Empty => vec![],
            };

            let (is_visible, replaces_id, text) = match &msg.kind {
                ProviderMessageKind::System(SystemKind::Placeholder) => (false, None, vec![]),
                ProviderMessageKind::System(SystemKind::MessageUpdate { target_message_id }) => (
                    true,
                    Some(target_message_id.clone()),
                    extract_text(&msg.content),
                ),
                ProviderMessageKind::System(SystemKind::MessageDelete { target_message_id }) => {
                    (false, Some(target_message_id.clone()), vec![])
                }
                _ => (true, None, extract_text(&msg.content)),
            };

            let (kind, amount_text) = match &msg.kind {
                ProviderMessageKind::Chat => ("chat", None),
                ProviderMessageKind::HistoryChat => ("history_chat", None),
                ProviderMessageKind::Monetary(info) => {
                    ("monetary", Some(format_money(&info.amount)))
                }
                _ => ("system", None),
            };
            let kind = kind.to_string();

            CommentRow {
                id: msg.id.clone(),
                user_name: msg.sender.display_name.clone(),
                user_id: msg.sender.id.clone(),
                badges: msg.sender.badges.clone(),
                text,
                timestamp: msg.timestamp,
                connection_id: envelope.connection_id.to_string(),
                is_visible,
                replaces_id,
                kind,
                avatar_url: msg.sender.avatar_url.clone(),
                amount_text,
            }
        })
        .collect()
}

/// アプリケーションの状態
struct AppState {
    core_addr: Addr<CoreActor>,
    plugin_manager: Arc<tokio::sync::Mutex<PluginManager>>,
    comment_store: Arc<Mutex<comment_store::CommentStore>>,
}

// ヘルパー関数

/// UUID文字列をパースするヘルパー関数
fn parse_uuid(id_str: &str, id_type: &str) -> Result<Uuid, String> {
    Uuid::parse_str(id_str).map_err(|e| format!("Invalid {}: {}", id_type, e))
}

/// 接続情報を取得するヘルパー関数
async fn get_connection_info(
    state: &State<'_, AppState>,
    connection_id: Uuid,
) -> Result<ConnectionInfo, String> {
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    connections
        .into_iter()
        .find(|c| c.connection_id == connection_id)
        .ok_or_else(|| "Connection not found".to_string())
}

async fn get_connection_input_schema(
    state: &State<'_, AppState>,
    connection_id: Uuid,
) -> Result<ConnectionInputSchemaPayload, String> {
    let request = McvMessage::new_request(
        MessageType::GetConnectionInputSchema,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(GetConnectionInputSchemaPayload { connection_id })
            .map_err(|e| format!("Failed to serialize GetConnectionInputSchemaPayload: {}", e))?,
    );
    let response = state
        .core_addr
        .send(SendRequest { message: request })
        .await
        .map_err(|e| format!("Failed to send get-connection-input-schema: {}", e))?
        .map_err(|e| format!("Core get-connection-input-schema error: {}", e))?;
    if response.message_type != MessageType::ConnectionInputSchema {
        return Err(format!(
            "Unexpected response type for get-connection-input-schema: {:?}",
            response.message_type
        ));
    }
    serde_json::from_value::<ConnectionInputSchemaPayload>(response.payload)
        .map_err(|e| format!("Invalid connection-input-schema payload: {}", e))
}

async fn get_send_comment_schema(
    state: &State<'_, AppState>,
    connection_id: Uuid,
) -> Result<SendCommentSchemaPayload, String> {
    let request = McvMessage::new_request(
        MessageType::GetSendCommentSchema,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(GetSendCommentSchemaPayload { connection_id })
            .map_err(|e| format!("Failed to serialize GetSendCommentSchemaPayload: {}", e))?,
    );
    let response = state
        .core_addr
        .send(SendRequest { message: request })
        .await
        .map_err(|e| format!("Failed to send get-send-comment-schema: {}", e))?
        .map_err(|e| format!("Core get-send-comment-schema error: {}", e))?;
    if response.message_type != MessageType::SendCommentSchema {
        return Err(format!(
            "Unexpected response type for get-send-comment-schema: {:?}",
            response.message_type
        ));
    }
    serde_json::from_value::<SendCommentSchemaPayload>(response.payload)
        .map_err(|e| format!("Invalid send-comment-schema payload: {}", e))
}

/// 接続を追加
#[tauri::command]
async fn add_connection(state: State<'_, AppState>) -> Result<String, String> {
    tracing::debug!(target:"mcv::core","add_connection called");

    let msg = mcv_core::SendRequest {
        message: mcv_messages::Message {
            message_type: MessageType::AddConnection,
            src: MessageSource::Core,
            dst: MessageDestination::Core,
            request_id: None,
            timestamp: 0,
            payload: serde_json::json!({}),
        },
    };
    let response = state
        .core_addr
        .send(msg)
        .await
        .map_err(|e| format!("Failed to send message to core: {}", e))?
        .map_err(|e| format!("Core actor error: {}", e))?;
    tracing::debug!(target: "mcv::main", response = ?response, "add_connection response");
    Ok("".to_string())
}

/// 接続を削除
#[tauri::command]
async fn remove_connection(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    state
        .core_addr
        .send(RemoveConnection {
            connection_id: conn_id,
        })
        .await
        .map_err(|e| e.to_string())?
}

/// 接続名を変更
#[tauri::command]
async fn rename_connection(
    state: State<'_, AppState>,
    connection_id: String,
    new_name: String,
) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    state
        .core_addr
        .send(RenameConnection {
            connection_id: conn_id,
            new_name,
        })
        .await
        .map_err(|e| e.to_string())?
}

/// 接続を開始
#[tauri::command]
async fn connect(state: tauri::State<'_, AppState>, connection_id: String) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    // 接続情報を取得
    let conn_info = get_connection_info(&state, conn_id).await?;
    let input_schema = get_connection_input_schema(&state, conn_id).await?;

    // サイトが選択されていない場合はエラー
    let plugin_id = conn_info.plugin_id.ok_or("サイトが選択されていません")?;
    let site_id = conn_info.site_id.ok_or("サイトが選択されていません")?;
    let url = conn_info.url.clone().ok_or("URLが入力されていません")?;

    tracing::debug!(
        connection_id = %conn_id,
        plugin_id = %plugin_id,
        site_id = %site_id,
        "Connecting to site"
    );

    let mut input_extra = input_schema
        .initial_data
        .and_then(|v| v.as_object().cloned())
        .unwrap_or_default();
    input_extra.insert("url".to_string(), serde_json::Value::String(url));
    if let Some(settings) = conn_info.advanced_settings.clone() {
        input_extra.insert("advanced_settings".to_string(), settings);
    }

    // connectメッセージを送信
    let message = McvMessage::new_request(
        MessageType::Connect,
        MessageSource::Core,
        MessageDestination::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
        serde_json::to_value(ConnectPayload {
            connection_id: conn_id,
            site: MsgSiteInfo {
                name: site_id.to_string(),
                id: site_id.clone(),
            },
            input: InputInfo {
                input_type: "url".to_string(),
                extra: serde_json::Value::Object(input_extra),
            },
            browser: MsgBrowserInfo {
                name: conn_info
                    .browser_id
                    .as_ref()
                    .map(|b| b.as_str().to_string())
                    .unwrap_or_else(|| "none".to_string()),
                id: conn_info
                    .browser_id
                    .unwrap_or_else(|| BrowserId::from_string("none".to_string())),
            },
        })
        .map_err(|e| format!("Failed to serialize ConnectPayload: {}", e))?,
    );

    let _ = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// 切断
#[tauri::command]
async fn disconnect(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    // disconnectメッセージを送信
    let message = McvMessage::new_request(
        MessageType::Disconnect,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(DisconnectPayload {
            connection_id: conn_id,
        })
        .map_err(|e| format!("Failed to serialize DisconnectPayload: {}", e))?,
    );

    let _ = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// 接続一覧を取得
#[tauri::command]
async fn get_connections(state: tauri::State<'_, AppState>) -> Result<Vec<ConnectionInfo>, String> {
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    Ok(connections)
}

/// サイト一覧を取得
#[tauri::command]
async fn get_sites(state: tauri::State<'_, AppState>) -> Result<Vec<CoreSiteInfo>, String> {
    let sites = state
        .core_addr
        .send(GetSites)
        .await
        .map_err(|e| e.to_string())?;

    Ok(sites)
}

/// ブラウザ一覧を取得
#[tauri::command]
async fn get_browsers(state: tauri::State<'_, AppState>) -> Result<Vec<CoreBrowserInfo>, String> {
    let browsers = state
        .core_addr
        .send(GetBrowsers)
        .await
        .map_err(|e| e.to_string())?;

    Ok(browsers)
}

/// 接続にサイトを設定
#[tauri::command]
async fn set_connection_site(
    state: tauri::State<'_, AppState>,
    connection_id: String,
    site_id: String,
) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;
    let s_id = SiteId::from_string(site_id);

    tracing::debug!(
        connection_id = %conn_id,
        site_id = %s_id,
        "Setting connection site"
    );

    state
        .core_addr
        .send(SetConnectionSite {
            connection_id: conn_id,
            site_id: s_id,
        })
        .await
        .map_err(|e| format!("Failed to set connection site: {}", e))?
        .map_err(|e| e)?;

    Ok(())
}

/// 接続設定を更新
#[tauri::command]
async fn update_connection_settings(
    state: tauri::State<'_, AppState>,
    connection_id: String,
    url: Option<String>,
    browser_id: Option<String>,
    advanced_settings: Option<serde_json::Value>,
    input_state: Option<serde_json::Value>,
) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    let b_id = browser_id.map(BrowserId::from_string);

    tracing::debug!(
        connection_id = %conn_id,
        has_url = url.is_some(),
        has_browser = b_id.is_some(),
        has_settings = advanced_settings.is_some(),
        has_input_state = input_state.is_some(),
        "Updating connection settings"
    );

    state
        .core_addr
        .send(UpdateConnectionSettings {
            connection_id: conn_id,
            url,
            browser_id: b_id,
            advanced_settings,
            input_state,
        })
        .await
        .map_err(|e| format!("Failed to update connection settings: {}", e))?
        .map_err(|e| e)?;

    Ok(())
}

/// URLを処理できるサイトを検出
#[tauri::command]
async fn detect_url(
    state: tauri::State<'_, AppState>,
    url: String,
) -> Result<Option<String>, String> {
    let (tx, rx) = tokio::sync::oneshot::channel::<Option<SiteId>>();
    state.core_addr.do_send(DetectUrl { url, tx });
    match tokio::time::timeout(std::time::Duration::from_secs(1), rx).await {
        Ok(Ok(Some(site_id))) => Ok(Some(site_id.into_string())),
        Ok(Ok(None)) => Ok(None),
        Ok(Err(_)) => Err("URL検出チャンネルが閉じました".to_string()),
        Err(_) => Ok(None),
    }
}

/// コメントを送信
#[tauri::command]
async fn send_comment(
    state: State<'_, AppState>,
    connection_id: String,
    text: String,
    extra: serde_json::Value,
) -> Result<String, String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    // 接続情報を取得してplugin_idを取得
    let conn_info = get_connection_info(&state, conn_id).await?;

    let plugin_id = conn_info
        .plugin_id
        .ok_or("Plugin not assigned to this connection")?;

    // send-commentメッセージを送信
    let message = McvMessage::new_request(
        MessageType::SendComment,
        MessageSource::Core,
        MessageDestination::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
        serde_json::to_value(SendCommentPayload {
            connection_id: conn_id,
            text,
            extra,
        })
        .map_err(|e| format!("Failed to serialize SendCommentPayload: {}", e))?,
    );

    let _ = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok("Comment sent".to_string())
}

/// コメント投稿フォームスキーマを取得
#[tauri::command]
async fn get_comment_schema(
    state: State<'_, AppState>,
    connection_id: String,
) -> Result<SendCommentSchemaPayload, String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;
    get_send_comment_schema(&state, conn_id).await
}

/// サイト＋ブラウザ選択時にアカウント情報をプリフェッチ
///
/// サイト（plugin_id）とブラウザ（browser_id）が両方設定されている場合のみ、
/// プラグインに FetchAccountInfo メッセージを送信する。
/// どちらか未設定の場合はエラーにせず Ok(()) を返す。
#[tauri::command]
async fn fetch_account_info(
    state: State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    tracing::info!(
        target: "mcv::main",
        connection_id = %connection_id,
        "fetch_account_info command called"
    );
    let conn_id = parse_uuid(&connection_id, "connection_id")?;
    let conn_info = get_connection_info(&state, conn_id).await?;

    let plugin_id = match conn_info.plugin_id {
        Some(id) => id,
        None => {
            tracing::debug!(
                target: "mcv::main",
                connection_id = %connection_id,
                "fetch_account_info skipped: plugin_id is not set"
            );
            return Ok(());
        } // サイト未選択は無視
    };
    let browser_id = match conn_info.browser_id {
        Some(id) => id,
        None => {
            tracing::debug!(
                target: "mcv::main",
                connection_id = %connection_id,
                "fetch_account_info skipped: browser_id is not set"
            );
            return Ok(());
        } // ブラウザ未選択は無視
    };
    tracing::info!(
        target: "mcv::main",
        connection_id = %connection_id,
        plugin_id = %plugin_id,
        browser_id = %browser_id,
        "Sending FetchAccountInfo message to plugin"
    );

    let message = McvMessage::new_notification(
        MessageType::FetchAccountInfo,
        MessageSource::Core,
        MessageDestination::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
        serde_json::to_value(FetchAccountInfoPayload {
            connection_id: conn_id,
            browser: MsgBrowserInfo {
                name: browser_id.as_str().to_string(),
                id: browser_id,
            },
        })
        .map_err(|e| format!("Failed to serialize FetchAccountInfoPayload: {}", e))?,
    );

    let _ = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    tracing::debug!(
        target: "mcv::main",
        connection_id = %connection_id,
        "FetchAccountInfo message dispatched"
    );

    Ok(())
}

/// フロントエンド診断ログを tracing に転送する
#[tauri::command]
async fn frontend_trace(
    level: String,
    message: String,
    fields: Option<serde_json::Value>,
    source: Option<FrontendTraceSource>,
) -> Result<(), String> {
    // subscriber 側がイベント metadata ではなくフロント由来の位置情報を優先できるよう、
    // source を tracing field に展開して渡す。
    let source_file = source
        .as_ref()
        .map(|s| s.file.as_str())
        .unwrap_or("unknown");
    let source_line = source.as_ref().and_then(|s| s.line).unwrap_or(0) as u64;
    let source_column = source.as_ref().and_then(|s| s.column).unwrap_or(0) as u64;
    let source_module = source
        .as_ref()
        .and_then(|s| s.function.as_deref())
        .unwrap_or("frontend");

    match level.to_ascii_lowercase().as_str() {
        "trace" => {
            tracing::trace!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "debug" => {
            tracing::debug!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "warn" => {
            tracing::warn!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        "error" => {
            tracing::error!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
        _ => {
            tracing::info!(
                target: "mcv::frontend",
                frontend_fields = ?fields,
                frontend_source_file = source_file,
                frontend_source_line = source_line,
                frontend_source_column = source_column,
                frontend_source_module = source_module,
                "{message}"
            );
        }
    }
    Ok(())
}

#[derive(Debug, Clone, serde::Deserialize)]
struct FrontendTraceSource {
    file: String,
    line: Option<u32>,
    column: Option<u32>,
    function: Option<String>,
}

/// mcv本体の更新をチェック
#[tauri::command]
async fn check_for_updates() -> Result<Option<McvUpdateInfo>, String> {
    const CURRENT_VERSION: &str = env!("CARGO_PKG_VERSION");

    tracing::info!(current_version = CURRENT_VERSION, "Checking for updates");

    let updater = UpdateChecker::new(API_BASE_URL);

    match updater.check_mcv_update(CURRENT_VERSION).await {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                tracing::info!(
                    current_version = CURRENT_VERSION,
                    new_version = %info.version,
                    "Update available"
                );
            } else {
                tracing::info!("No update available");
            }
            Ok(update_info)
        }
        Err(e) => {
            tracing::error!(
                error = %e,
                "Failed to check for updates"
            );
            Err(format!("Failed to check for updates: {}", e))
        }
    }
}

#[tauri::command]
async fn get_current_version() -> Result<String, String> {
    Ok(env!("CARGO_PKG_VERSION").to_string())
}

/// 配布サーバーのプラグイン一覧を取得
#[tauri::command]
async fn list_registry_plugins() -> Result<Vec<PluginListItem>, String> {
    let updater = UpdateChecker::new(API_BASE_URL);
    updater
        .list_plugins()
        .await
        .map_err(|e| format!("Failed to list registry plugins: {}", e))
}

fn ps_escape_single_quoted(input: &str) -> String {
    input.replace('\'', "''")
}

fn can_write_to_dir(dir: &PathBuf) -> Result<(), String> {
    let probe = dir.join(".mcv_write_probe.tmp");
    std::fs::write(&probe, b"probe").map_err(|e| {
        format!(
            "Update requires write permission to install directory ({}): {}",
            dir.display(),
            e
        )
    })?;
    let _ = std::fs::remove_file(&probe);
    Ok(())
}

fn extract_zip_file(zip_path: &PathBuf, dest_dir: &PathBuf) -> Result<(), String> {
    if dest_dir.exists() {
        std::fs::remove_dir_all(dest_dir)
            .map_err(|e| format!("Failed to clean temporary directory: {}", e))?;
    }
    std::fs::create_dir_all(dest_dir)
        .map_err(|e| format!("Failed to create temporary directory: {}", e))?;

    let zip_file =
        std::fs::File::open(zip_path).map_err(|e| format!("Failed to open ZIP file: {}", e))?;
    let mut archive =
        ZipArchive::new(zip_file).map_err(|e| format!("Failed to read ZIP archive: {}", e))?;

    for i in 0..archive.len() {
        let mut file = archive
            .by_index(i)
            .map_err(|e| format!("Failed to access ZIP entry: {}", e))?;
        let out_path = match file.enclosed_name() {
            Some(path) => dest_dir.join(path),
            None => continue,
        };

        if file.name().ends_with('/') {
            std::fs::create_dir_all(&out_path)
                .map_err(|e| format!("Failed to create directory: {}", e))?;
        } else {
            if let Some(parent) = out_path.parent() {
                std::fs::create_dir_all(parent)
                    .map_err(|e| format!("Failed to create parent directory: {}", e))?;
            }
            let mut output = std::fs::File::create(&out_path)
                .map_err(|e| format!("Failed to create extracted file: {}", e))?;
            std::io::copy(&mut file, &mut output)
                .map_err(|e| format!("Failed to extract ZIP entry: {}", e))?;
        }
    }

    Ok(())
}

fn find_file_recursive(root: &PathBuf, file_name: &str) -> Option<PathBuf> {
    if !root.is_dir() {
        return None;
    }

    let entries = std::fs::read_dir(root).ok()?;
    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_file() {
            if path.file_name().and_then(|n| n.to_str()) == Some(file_name) {
                return Some(path);
            }
        } else if path.is_dir() {
            if let Some(found) = find_file_recursive(&path, file_name) {
                return Some(found);
            }
        }
    }
    None
}

/// mcv本体アップデートZIPをダウンロードしてチェックサム検証
#[tauri::command]
async fn download_core_update(
    version: String,
    channel: String,
    sha256: String,
) -> Result<String, String> {
    let updater = UpdateChecker::new(API_BASE_URL);
    let temp_dir = std::env::temp_dir().join("mcv-updater");
    std::fs::create_dir_all(&temp_dir)
        .map_err(|e| format!("Failed to create temp directory: {}", e))?;
    let zip_path = temp_dir.join(format!("mcv-core-{}-{}.zip", version, channel));

    let download_url = updater.build_mcv_download_url(&version, &channel);
    updater
        .download(&download_url, &zip_path, |_downloaded, _total| {})
        .await
        .map_err(|e| format!("Failed to download update package: {}", e))?;

    updater
        .verify_checksum(&zip_path, &sha256)
        .await
        .map_err(|e| format!("Failed to verify update package checksum: {}", e))?;

    Ok(zip_path.to_string_lossy().to_string())
}

/// ダウンロード済みアップデートを適用し、再起動
#[tauri::command]
async fn apply_core_update(zip_path: String, app_handle: AppHandle) -> Result<(), String> {
    let current_exe =
        std::env::current_exe().map_err(|e| format!("Failed to get current exe path: {}", e))?;
    let exe_dir = current_exe
        .parent()
        .ok_or_else(|| "Failed to resolve executable directory".to_string())?
        .to_path_buf();

    can_write_to_dir(&exe_dir)?;

    let update_zip = PathBuf::from(&zip_path);
    if !update_zip.exists() {
        return Err(format!("Update ZIP not found: {}", update_zip.display()));
    }

    let extracted_dir =
        std::env::temp_dir().join(format!("mcv-update-extracted-{}", std::process::id()));
    extract_zip_file(&update_zip, &extracted_dir)?;

    let exe_name = current_exe
        .file_name()
        .and_then(|n| n.to_str())
        .ok_or_else(|| "Failed to resolve executable name".to_string())?;
    let new_exe = find_file_recursive(&extracted_dir, exe_name)
        .ok_or_else(|| format!("{} not found in update package", exe_name))?;

    let ps_command = format!(
        "Start-Sleep -Milliseconds 700; \
         Copy-Item -Path '{src}' -Destination '{dst}' -Force; \
         Start-Process -FilePath '{dst}'; \
         Remove-Item -Path '{zip}' -Force -ErrorAction SilentlyContinue; \
         Remove-Item -Path '{extract}' -Recurse -Force -ErrorAction SilentlyContinue",
        src = ps_escape_single_quoted(&new_exe.to_string_lossy()),
        dst = ps_escape_single_quoted(&current_exe.to_string_lossy()),
        zip = ps_escape_single_quoted(&update_zip.to_string_lossy()),
        extract = ps_escape_single_quoted(&extracted_dir.to_string_lossy()),
    );

    std::process::Command::new("powershell")
        .args(["-WindowStyle", "Hidden", "-Command", &ps_command])
        .spawn()
        .map_err(|e| format!("Failed to launch update script: {}", e))?;

    tracing::info!("Core update script launched, exiting mcv");
    app_handle.exit(0);
    Ok(())
}

/// レジストリからプラグインZIPをダウンロードしてインストール
#[tauri::command]
async fn install_registry_plugin(
    plugin_id: String,
    version: String,
    channel: String,
    state: tauri::State<'_, AppState>,
) -> Result<(), String> {
    let updater = UpdateChecker::new(API_BASE_URL);
    let plugin_detail = updater
        .get_plugin_detail(&plugin_id)
        .await
        .map_err(|e| format!("Failed to fetch plugin detail: {}", e))?;

    let target_version: &PluginVersionDetail = plugin_detail
        .versions
        .iter()
        .find(|v| v.version == version && v.channel == channel && !v.is_deleted && v.is_public)
        .ok_or_else(|| format!("Plugin version not found: {plugin_id} {version} {channel}"))?;

    let temp_dir = std::env::temp_dir().join("mcv-plugin-install");
    std::fs::create_dir_all(&temp_dir)
        .map_err(|e| format!("Failed to create temp directory: {}", e))?;
    let zip_path = temp_dir.join(format!("{}-{}-{}.zip", plugin_id, version, channel));

    let download_url = updater.build_plugin_download_url(&plugin_id, &version, &channel);
    updater
        .download(&download_url, &zip_path, |_downloaded, _total| {})
        .await
        .map_err(|e| format!("Failed to download plugin package: {}", e))?;

    updater
        .verify_checksum(&zip_path, &target_version.sha256)
        .await
        .map_err(|e| format!("Failed to verify plugin package checksum: {}", e))?;

    // ZIPをそのままpluginsディレクトリに配置する（依存ファイルを含めて保持するため）
    let plugin_dir = get_plugin_dir();
    std::fs::create_dir_all(&plugin_dir)
        .map_err(|e| format!("Failed to create plugin directory: {}", e))?;

    let dest_path = plugin_dir.join(format!("{}.zip", plugin_id));
    std::fs::copy(&zip_path, &dest_path).map_err(|e| format!("Failed to install plugin: {}", e))?;

    // 再インストール時に古いキャッシュが残らないよう削除する
    let cache_dir = plugin_dir.join(".cache").join(&plugin_id);
    let _ = std::fs::remove_dir_all(&cache_dir);

    let _ = std::fs::remove_file(&zip_path);

    // actix コンテキスト内で新プラグインをスキャン・ロードする
    state
        .core_addr
        .send(ScanAndLoadNewPlugins {
            plugin_manager: Arc::clone(&state.plugin_manager),
            plugins_dir: plugin_dir,
        })
        .await
        .map_err(|e| format!("Failed to load plugin after install: {}", e))?;

    Ok(())
}

/// plugin.json から id フィールドを読み取る（BOM 対応）
fn read_plugin_id_from_manifest(manifest_path: &std::path::Path) -> Option<String> {
    #[derive(serde::Deserialize)]
    struct Manifest {
        #[serde(default)]
        id: Option<String>,
    }
    let content = std::fs::read(manifest_path).ok()?;
    let content = if content.starts_with(b"\xEF\xBB\xBF") {
        &content[3..]
    } else {
        &content
    };
    let manifest: Manifest = serde_json::from_slice(content).ok()?;
    manifest.id.filter(|s| !s.is_empty())
}

/// インストール済みプラグインのメタ情報（ID + バージョン + チャンネル）
#[derive(serde::Serialize)]
struct InstalledPluginMeta {
    id: String,
    version: Option<String>,
    channel: Option<String>,
}

/// plugin.json からバージョンとチャンネルを読み取る（BOM 対応）
fn read_installed_plugin_meta(
    plugin_dir: &std::path::Path,
    id: &str,
) -> (Option<String>, Option<String>) {
    #[derive(serde::Deserialize)]
    struct PluginJsonMeta {
        #[serde(default)]
        version: Option<String>,
        #[serde(default)]
        channel: Option<String>,
    }
    let try_read = |path: &std::path::Path| -> Option<(Option<String>, Option<String>)> {
        let content = std::fs::read(path).ok()?;
        let content = if content.starts_with(b"\xEF\xBB\xBF") {
            &content[3..]
        } else {
            &content[..]
        };
        let meta: PluginJsonMeta = serde_json::from_slice(content).ok()?;
        Some((meta.version, meta.channel))
    };
    // ZIP 形式: .cache/{id}/plugin.json (起動時に展開済み)
    let cache_path = plugin_dir.join(".cache").join(id).join("plugin.json");
    if let Some(pair) = try_read(&cache_path) {
        return pair;
    }
    // ディレクトリ形式: {id}/plugin.json
    let dir_path = plugin_dir.join(id).join("plugin.json");
    try_read(&dir_path).unwrap_or((None, None))
}

/// pluginsディレクトリを直接スキャンしてインストール済みプラグインIDの一覧を返す
///
/// - サブディレクトリ形式（plugin.json あり）: manifest の `id` フィールド、またはディレクトリ名
/// - ZIP 形式: ZIPのファイル名（拡張子除く）
fn scan_installed_plugin_ids(plugin_dir: &std::path::Path) -> Result<Vec<String>, String> {
    if !plugin_dir.exists() {
        return Ok(vec![]);
    }
    let entries = std::fs::read_dir(plugin_dir)
        .map_err(|e| format!("Failed to read plugin directory: {}", e))?;

    let mut ids = std::collections::HashSet::new();
    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_dir() {
            let dir_name = match path.file_name().and_then(|n| n.to_str()) {
                Some(n) => n.to_string(),
                None => continue,
            };
            // 隠しディレクトリ（.cache 等）はスキップ
            if dir_name.starts_with('.') {
                continue;
            }
            let manifest_path = path.join("plugin.json");
            if !manifest_path.exists() {
                continue;
            }
            let id = read_plugin_id_from_manifest(&manifest_path).unwrap_or(dir_name);
            // アンインストールマーカーがある場合はスキップ（次回起動時に削除予定）
            let marker = plugin_dir.join(format!(".uninstall-{}", &id));
            if marker.exists() {
                continue;
            }
            ids.insert(id);
        } else if path
            .extension()
            .and_then(|e| e.to_str())
            .map(|e| e.eq_ignore_ascii_case("zip"))
            .unwrap_or(false)
        {
            if let Some(stem) = path.file_stem().and_then(|s| s.to_str()) {
                ids.insert(stem.to_string());
            }
        }
    }
    Ok(ids.into_iter().collect())
}

/// インストール済みプラグイン一覧をバージョン情報付きで取得
#[tauri::command]
async fn list_installed_plugins() -> Result<Vec<InstalledPluginMeta>, String> {
    let plugin_dir = get_plugin_dir();
    let ids = scan_installed_plugin_ids(&plugin_dir)?;
    Ok(ids
        .into_iter()
        .map(|id| {
            let (version, channel) = read_installed_plugin_meta(&plugin_dir, &id);
            InstalledPluginMeta {
                id,
                version,
                channel,
            }
        })
        .collect())
}

/// インストール済みプラグインを削除する（ZIP形式・ディレクトリ形式の両方に対応）
///
/// DLL がプロセスにロードされているため即時削除できない場合は、
/// `.uninstall-{id}` にリネームして次回起動時にクリーンアップする。
#[tauri::command]
async fn uninstall_registry_plugin(plugin_id: String) -> Result<(), String> {
    let plugin_dir = get_plugin_dir();
    let zip_path = plugin_dir.join(format!("{}.zip", plugin_id));
    let dir_path = plugin_dir.join(&plugin_id);
    let cache_dir = plugin_dir.join(".cache").join(&plugin_id);

    let mut found = false;

    if zip_path.exists() {
        std::fs::remove_file(&zip_path)
            .map_err(|e| format!("Failed to remove plugin '{}': {}", plugin_id, e))?;
        found = true;
    }

    if dir_path.exists() {
        // まず即時削除を試みる
        if std::fs::remove_dir_all(&dir_path).is_err() {
            // DLL ロック中のためリネームを試みる
            let pending_path = plugin_dir.join(format!(".uninstall-{}", plugin_id));
            if std::fs::rename(&dir_path, &pending_path).is_err() {
                // Windows では DLL がロード中だとリネームも拒否されることがある。
                // その場合はマーカーファイルを作成し、次回起動時にディレクトリを削除する。
                tracing::warn!(
                    target: "mcv::main",
                    id = %plugin_id,
                    "Cannot rename locked plugin directory; creating uninstall marker for next startup"
                );
                std::fs::write(&pending_path, b"").map_err(|e| {
                    format!("Failed to queue plugin '{}' for removal: {}", plugin_id, e)
                })?;
            }
        }
        found = true;
    }

    // キャッシュも同様に処理（best-effort、失敗時はリネームして次回起動時にクリーンアップ）
    if cache_dir.exists() {
        if std::fs::remove_dir_all(&cache_dir).is_err() {
            let pending_cache = plugin_dir
                .join(".cache")
                .join(format!(".uninstall-{}", plugin_id));
            let _ = std::fs::rename(&cache_dir, &pending_cache);
        }
    }

    if !found {
        return Err(format!("Plugin '{}' is not installed.", plugin_id));
    }

    Ok(())
}

/// 前回セッションでアンインストール待ちになったエントリを削除する
///
/// `.uninstall-{id}` がディレクトリの場合はそのまま削除（リネーム成功済み）。
/// ファイルの場合はマーカーファイルで、対応する `{id}/` ディレクトリの削除を試みてからマーカーを消す。
fn cleanup_pending_uninstalls(plugin_dir: &std::path::Path) {
    // plugin_dir 直下の .uninstall-* エントリを処理
    if let Ok(entries) = std::fs::read_dir(plugin_dir) {
        for entry in entries.flatten() {
            let path = entry.path();
            let name = match path.file_name().and_then(|n| n.to_str()) {
                Some(n) => n.to_string(),
                None => continue,
            };
            if !name.starts_with(".uninstall-") {
                continue;
            }
            if path.is_dir() {
                // リネーム済みのディレクトリ → 削除
                tracing::info!(target: "mcv::main", path = %path.display(), "Cleaning up pending uninstall directory");
                let _ = std::fs::remove_dir_all(&path);
            } else if path.is_file() {
                // マーカーファイル → 実際のディレクトリ削除を試みてからマーカーを消す
                let plugin_id = &name[".uninstall-".len()..];
                let target_dir = plugin_dir.join(plugin_id);
                tracing::info!(target: "mcv::main", id = %plugin_id, "Cleaning up plugin directory from uninstall marker");
                if target_dir.exists() {
                    if std::fs::remove_dir_all(&target_dir).is_ok() {
                        let _ = std::fs::remove_file(&path);
                    }
                    // まだ失敗する場合はマーカーを残して次回再試行
                } else {
                    let _ = std::fs::remove_file(&path);
                }
            }
        }
    }
    // .cache/ 下の .uninstall-* ディレクトリも削除
    let cache_root = plugin_dir.join(".cache");
    if let Ok(entries) = std::fs::read_dir(&cache_root) {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_dir()
                && path
                    .file_name()
                    .and_then(|n| n.to_str())
                    .map(|n| n.starts_with(".uninstall-"))
                    .unwrap_or(false)
            {
                tracing::info!(target: "mcv::main", path = %path.display(), "Cleaning up pending uninstall cache directory");
                let _ = std::fs::remove_dir_all(&path);
            }
        }
    }
}

// ログビューア関連のコマンド

/// ログクエリパラメータ
#[derive(serde::Deserialize)]
struct LogQueryParams {
    levels: Option<Vec<String>>,
    search: Option<String>,
    from: Option<i64>,
    to: Option<i64>,
    limit: Option<usize>,
    offset: Option<usize>,
}

/// ビルドプロファイル情報
#[derive(serde::Serialize)]
struct BuildProfileInfo {
    profile: String,
}

/// ログを取得
#[tauri::command]
async fn get_logs(params: LogQueryParams) -> Result<Vec<mcv_log_core::LogEntry>, String> {
    let storage = mcv_log_core::get_storage();

    // チャンネルに応じたレベルフィルタを適用
    let build_profile = get_build_profile();
    let effective_levels = match build_profile {
        "stable" | "beta" => {
            // stable/betaはerrorのみ
            Some(vec![mcv_log_core::LogLevel::Error])
        }
        "alpha" => {
            // alphaはユーザー指定のレベル、または指定なしなら全て
            params.levels.as_ref().map(|levels| {
                levels
                    .iter()
                    .filter_map(|s| match s.to_lowercase().as_str() {
                        "trace" => Some(mcv_log_core::LogLevel::Trace),
                        "debug" => Some(mcv_log_core::LogLevel::Debug),
                        "info" => Some(mcv_log_core::LogLevel::Info),
                        "warn" => Some(mcv_log_core::LogLevel::Warn),
                        "error" => Some(mcv_log_core::LogLevel::Error),
                        _ => None,
                    })
                    .collect()
            })
        }
        _ => None,
    };

    let filters = mcv_log_core::storage::LogQueryFilters {
        levels: effective_levels,
        search: params.search,
        from: params.from,
        to: params.to,
        limit: params.limit,
        offset: params.offset,
    };

    let storage_guard = storage
        .lock()
        .map_err(|e| format!("Failed to lock storage: {}", e))?;

    storage_guard
        .query_logs(filters)
        .map_err(|e| format!("Failed to query logs: {}", e))
}

/// ビルドプロファイル情報を取得
#[tauri::command]
async fn get_build_profile_info() -> Result<BuildProfileInfo, String> {
    Ok(BuildProfileInfo {
        profile: get_build_profile().to_string(),
    })
}

/// 設定スキーマを取得
#[tauri::command]
async fn get_settings_schema(
    target: String,
    state: State<'_, AppState>,
) -> Result<serde_json::Value, String> {
    let message = McvMessage::new_request(
        MessageType::GetSettingsSchema,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(mcv_messages::GetSettingsSchemaPayload { target })
            .map_err(|e| e.to_string())?,
    );

    let response = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?
        .map_err(|e| e.to_string())?;

    let payload: mcv_messages::SettingsSchemaPayload =
        serde_json::from_value(response.payload).map_err(|e| e.to_string())?;

    Ok(payload.schema)
}

/// 設定値を取得
#[tauri::command]
async fn get_settings(
    target: String,
    state: State<'_, AppState>,
) -> Result<serde_json::Value, String> {
    let message = McvMessage::new_request(
        MessageType::GetSettings,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(mcv_messages::GetSettingsPayload { target })
            .map_err(|e| e.to_string())?,
    );

    let response = state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?
        .map_err(|e| e.to_string())?;

    let payload: mcv_messages::SettingsDataPayload =
        serde_json::from_value(response.payload).map_err(|e| e.to_string())?;

    Ok(payload.data)
}

/// 設定を更新
#[tauri::command]
async fn update_settings(
    target: String,
    data: serde_json::Value,
    state: State<'_, AppState>,
) -> Result<(), String> {
    let message = McvMessage::new_request(
        MessageType::UpdateSettings,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(mcv_messages::UpdateSettingsPayload { target, data })
            .map_err(|e| e.to_string())?,
    );

    state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// プラグイン一覧情報
#[derive(serde::Serialize)]
struct PluginInfoResponse {
    plugin_id: String,
    name: String,
}

/// プラグイン一覧を取得
#[tauri::command]
async fn get_plugins(state: State<'_, AppState>) -> Result<Vec<PluginInfoResponse>, String> {
    let plugins = state
        .core_addr
        .send(GetLogicalPlugins)
        .await
        .map_err(|e| e.to_string())?;

    Ok(plugins
        .into_iter()
        .map(|p| PluginInfoResponse {
            plugin_id: p.plugin_id.to_string(),
            name: p.name,
        })
        .collect())
}

// ============================================================
// コメント検索・ユーザー一覧コマンド
// ============================================================

#[tauri::command]
fn search_comments(
    query: String,
    limit: u32,
    offset: u32,
    state: State<'_, AppState>,
) -> Result<Vec<CommentRow>, String> {
    let store = state.comment_store.lock().map_err(|e| e.to_string())?;
    store
        .search_comments(&query, limit as usize, offset as usize)
        .map_err(|e| e.to_string())
}

#[tauri::command]
fn get_users(
    limit: u32,
    offset: u32,
    state: State<'_, AppState>,
) -> Result<Vec<comment_store::UserInfo>, String> {
    let store = state.comment_store.lock().map_err(|e| e.to_string())?;
    store
        .get_users(limit as usize, offset as usize)
        .map_err(|e| e.to_string())
}

// ============================================================
// ウィンドウ状態の保存・復元（settings/core.json に直接読み書き）
// ============================================================

/// タイトルバー領域（上部 50px）がいずれかのモニターと重なっているか確認する
fn is_title_bar_visible(monitors: &[tauri::Monitor], x: i32, y: i32, width: u32) -> bool {
    let title_bar_h = 50i32;
    for m in monitors {
        let mp = m.position();
        let ms = m.size();
        let m_right = mp.x + ms.width as i32;
        let m_bottom = mp.y + ms.height as i32;
        if x < m_right && (x + width as i32) > mp.x && y < m_bottom && (y + title_bar_h) > mp.y {
            return true;
        }
    }
    false
}

/// ウィンドウ状態を core.json にマージ保存する
///
/// 最大化中はサイズ・位置を保存しない（最大化解除後のサイズを維持するため）。
fn save_window_state(window: &tauri::WebviewWindow, settings_dir: &std::path::Path) {
    let core_json = settings_dir.join("core.json");
    let mut data: serde_json::Value = std::fs::read_to_string(&core_json)
        .ok()
        .and_then(|s| serde_json::from_str(&s).ok())
        .unwrap_or(serde_json::json!({}));

    let is_maximized = window.is_maximized().unwrap_or(false);
    data["window_maximized"] = serde_json::json!(is_maximized);

    if !is_maximized {
        if let (Ok(pos), Ok(size)) = (window.outer_position(), window.outer_size()) {
            data["window_x"] = serde_json::json!(pos.x);
            data["window_y"] = serde_json::json!(pos.y);
            data["window_width"] = serde_json::json!(size.width);
            data["window_height"] = serde_json::json!(size.height);
        }
    }

    if let Ok(json) = serde_json::to_string_pretty(&data) {
        let _ = std::fs::write(&core_json, json);
    }
}

/// core.json からウィンドウ状態を復元する（マルチモニター対応）
fn restore_window_state(window: &tauri::WebviewWindow, settings_dir: &std::path::Path) {
    let core_json = settings_dir.join("core.json");
    let Ok(json) = std::fs::read_to_string(&core_json) else {
        return;
    };
    let Ok(data): Result<serde_json::Value, _> = serde_json::from_str(&json) else {
        return;
    };

    let maximized = data
        .get("window_maximized")
        .and_then(|v| v.as_bool())
        .unwrap_or(false);
    if maximized {
        let _ = window.maximize();
        return;
    }

    let x = data
        .get("window_x")
        .and_then(|v| v.as_i64())
        .map(|v| v as i32);
    let y = data
        .get("window_y")
        .and_then(|v| v.as_i64())
        .map(|v| v as i32);
    let w = data
        .get("window_width")
        .and_then(|v| v.as_u64())
        .map(|v| v as u32);
    let h = data
        .get("window_height")
        .and_then(|v| v.as_u64())
        .map(|v| v as u32);

    if let (Some(x), Some(y), Some(w), Some(h)) = (x, y, w, h) {
        let monitors = window.available_monitors().unwrap_or_default();
        if monitors.is_empty() || is_title_bar_visible(&monitors, x, y, w) {
            let _ = window.set_size(tauri::Size::Physical(tauri::PhysicalSize {
                width: w,
                height: h,
            }));
            let _ =
                window.set_position(tauri::Position::Physical(tauri::PhysicalPosition { x, y }));
        }
        // タイトルバーが見えない場合（モニター切断等）はデフォルト位置のまま
    }
}

// ============================================================
// 列設定の保存・復元（settings/core.json に直接読み書き）
// ============================================================

/// 列設定（順序・幅・表示状態）
#[derive(serde::Serialize, serde::Deserialize, Clone)]
struct ColumnSettings {
    order: Vec<String>,
    widths: std::collections::HashMap<String, u32>,
    visibility: std::collections::HashMap<String, bool>,
}

/// settings_dir を Tauri コマンドへ渡すための管理状態
struct SettingsDirState {
    settings_dir: std::path::PathBuf,
}

/// core.json から列設定を読み込む
#[tauri::command]
fn get_column_settings(state: tauri::State<'_, SettingsDirState>) -> Option<ColumnSettings> {
    let core_json = state.settings_dir.join("core.json");
    let data: serde_json::Value = std::fs::read_to_string(&core_json)
        .ok()
        .and_then(|s| serde_json::from_str(&s).ok())?;
    serde_json::from_value(data.get("columns")?.clone()).ok()
}

/// 列設定を core.json にマージ保存する
#[tauri::command]
fn save_column_settings(settings: ColumnSettings, state: tauri::State<'_, SettingsDirState>) {
    let core_json = state.settings_dir.join("core.json");
    let mut data: serde_json::Value = std::fs::read_to_string(&core_json)
        .ok()
        .and_then(|s| serde_json::from_str(&s).ok())
        .unwrap_or(serde_json::json!({}));
    data["columns"] = serde_json::to_value(&settings).unwrap_or(serde_json::json!({}));
    if let Ok(json) = serde_json::to_string_pretty(&data) {
        let _ = std::fs::write(&core_json, json);
    }
}

fn main() {
    // ロガーを初期化
    let local_app_data = std::env::var("LOCALAPPDATA").expect("Failed to get LOCALAPPDATA");
    let app_data_dir = PathBuf::from(&local_app_data).join("MultiCommentViewer");
    let log_db_path = app_data_dir.join("logs.db");

    // ログディレクトリを作成
    std::fs::create_dir_all(&app_data_dir).expect("Failed to create app data directory");

    mcv_log_core::init_logger(&log_db_path, env!("CARGO_PKG_VERSION"))
        .expect("Failed to initialize logger");

    // 前回クラッシュ時の panic.log を SQLite にリカバリ
    let panic_log_path = app_data_dir.join("panic.log");
    if mcv_log_core::recover_panic_log(&panic_log_path) {
        tracing::warn!(
            target: "mcv::main",
            panic_log_path = %panic_log_path.display(),
            "前回のクラッシュ情報を panic.log から SQLite へ復旧しました"
        );
    }

    // パニックフックをインストール（SQLite 書き込み失敗時は panic.log へフォールバック）
    mcv_log_core::install_panic_hook(panic_log_path, env!("CARGO_PKG_VERSION").to_string());

    // Windows SEH ハンドラを設置（panic! では捕捉できない 0xe0000008 等を crash_seh_*.txt に記録）
    crash_handler::install(&app_data_dir);

    tracing::info!(
        target: "mcv::main",
        version = env!("CARGO_PKG_VERSION"),
        log_db_path = %log_db_path.display(),
        "mcv started"
    );

    // LogSenderActorを起動するためのストレージを取得
    let log_storage = mcv_log_core::get_storage();

    // 設定ストレージを初期化（settings/ ディレクトリに JSON ファイルとして保存）
    let settings_dir = app_data_dir.join("settings");
    std::fs::create_dir_all(&settings_dir).expect("Failed to create settings directory");
    let settings_storage = Arc::new(std::sync::Mutex::new(
        mcv_settings_core::SettingsStorage::new(&settings_dir)
            .expect("Failed to initialize settings storage"),
    ));
    tracing::info!(
        target: "mcv::main",
        settings_dir = %settings_dir.display(),
        "Settings storage initialized"
    );

    // Tauri セットアップでも settings_dir を使うためにクローン
    let settings_dir_for_tauri = settings_dir.clone();

    // ログディレクトリを作成（EXEプラグインのセッションDB保存先）
    let logs_dir = app_data_dir.join("logs");
    std::fs::create_dir_all(&logs_dir).expect("Failed to create logs directory");

    // actixのシステムをセットアップするためのチャネル
    let (tx, rx) = std::sync::mpsc::channel();

    // Actixシステムを別スレッドで実行
    std::thread::spawn(move || {
        let actix_system = System::new();

        actix_system.block_on(async {
            tracing::info!(target: "mcv::main","Actix system thread started");

            // LogSenderActorを起動
            let _log_sender_addr = mcv_log_core::LogSenderActor::new(
                log_storage,
                API_BASE_URL.to_string(),
            ).start();
            tracing::info!(target: "mcv::main", api_base_url = API_BASE_URL, "LogSenderActor started");

            // Core Actorを起動
            let mut core_actor = CoreActor::new();

            // Plugin Managerを作成
            let mut plugin_manager = PluginManager::new();

            // AppHandleを保持するためのプレースホルダー
            let app_handle: Arc<tokio::sync::Mutex<Option<AppHandle>>> =
                Arc::new(tokio::sync::Mutex::new(None));

            // connection_id ごとの表示タイミング基準
            // エンベロープをまたいで正確な timestamp ベースのタイミングを実現するために使用
            // Value: (base_ts: i64, base_instant: Instant) — 接続内の最初のメッセージ到着時に初期化
            let comment_timing: Arc<
                tokio::sync::Mutex<
                    std::collections::HashMap<Uuid, (i64, std::time::Instant)>,
                >,
            > = Arc::new(tokio::sync::Mutex::new(std::collections::HashMap::new()));

            // コメントストアを作成（exe と同じディレクトリに session.db を配置）
            let session_db_path = std::env::current_exe()
                .expect("Failed to get current exe path")
                .parent()
                .expect("Failed to get exe directory")
                .join("session.db");
            let comment_store = Arc::new(Mutex::new(
                comment_store::CommentStore::new(&session_db_path)
                    .expect("Failed to create comment store"),
            ));

            // イベントコールバックを設定
            let app_handle_clone = app_handle.clone();
            let comment_timing_clone = comment_timing.clone();
            let comment_store_for_callback = comment_store.clone();
            let event_callback = Arc::new(move |message: McvMessage| {
                let app_handle_clone2 = app_handle_clone.clone();
                let timing_clone = comment_timing_clone.clone();
                let store_clone = comment_store_for_callback.clone();
                actix::spawn(async move {
                    // app_handle の Mutex を最小限の期間だけ保持してすぐに解放する
                    let Some(app_handle) = app_handle_clone2.lock().await.as_ref().cloned() else {
                        return;
                    };
                    match message.message_type {
                            MessageType::CommentReceived => {
                                let payload: CommentReceivedPayload =
                                    match serde_json::from_value(message.payload) {
                                        Ok(p) => p,
                                        Err(e) => {
                                            tracing::error!(
                                                target: "mcv::main",
                                                error = %e,
                                                "Failed to parse CommentReceivedPayload"
                                            );
                                            return;
                                        }
                                    };
                                tracing::debug!(
                                    target: "mcv::main",
                                    connection_id = %payload.connection_id,
                                    message_count = payload.envelope.messages.len(),
                                    "Processing comment-received event"
                                );
                                // MessageDeleteAll を "delete-all-by-user" イベントとして即座に emit
                                for msg in &payload.envelope.messages {
                                    if let ProviderMessageKind::System(SystemKind::MessageDeleteAll { user_id }) = &msg.kind {
                                        // サイトNGフラグをストアに記録（spawn_blocking で actix スレッドをブロックしない）
                                        let store_for_ng = store_clone.clone();
                                        let user_id_for_ng = user_id.clone();
                                        tokio::task::spawn_blocking(move || {
                                            if let Ok(store) = store_for_ng.lock() {
                                                let _ = store.set_site_ng(&user_id_for_ng);
                                            }
                                        })
                                        .await
                                        .ok();
                                        let evt = DeleteAllByUserPayload {
                                            user_id: user_id.clone(),
                                            connection_id: payload.envelope.connection_id.to_string(),
                                        };
                                        if let Err(e) = app_handle.emit("delete-all-by-user", evt) {
                                            tracing::error!(
                                                target: "mcv::main",
                                                error = %e,
                                                "Failed to emit delete-all-by-user event"
                                            );
                                        }
                                    }
                                }
                                // HistoryChat は timestamp 差分を再生せず短間隔でバースト送信し、
                                // それ以外（Chat/Monetary/System）は従来どおり timestamp 再生を行う。
                                let mut history_messages = Vec::new();
                                let mut timed_messages = Vec::new();
                                for msg in payload.envelope.messages.into_iter() {
                                    if matches!(
                                        &msg.kind,
                                        ProviderMessageKind::System(
                                            SystemKind::MessageDeleteAll { .. }
                                        )
                                    ) {
                                        continue;
                                    }
                                    if matches!(&msg.kind, ProviderMessageKind::HistoryChat) {
                                        history_messages.push(msg);
                                    } else {
                                        timed_messages.push(msg);
                                    }
                                }
                                let connection_id = payload.envelope.connection_id;
                                let received_at = payload.envelope.received_at;
                                let event_id = payload.envelope.event_id;

                                if !history_messages.is_empty() || !timed_messages.is_empty() {
                                    let mut all_rows = Vec::new();
                                    for msg in history_messages.into_iter().chain(timed_messages) {
                                        let single_envelope = mcv_messages::McvEnvelope {
                                            event_id,
                                            connection_id,
                                            messages: vec![msg],
                                            received_at,
                                            raw_message: None,
                                        };
                                        all_rows.extend(envelope_to_comment_rows(&single_envelope));
                                    }
                                    // SQLite 挿入を spawn_blocking で実行（actix スレッドをブロックしない）
                                    let store_for_insert = store_clone.clone();
                                    let all_rows = tokio::task::spawn_blocking(move || {
                                        if let Ok(store) = store_for_insert.lock() {
                                            for row in &all_rows {
                                                let _ = store.insert_comment(row);
                                            }
                                        }
                                        all_rows
                                    })
                                    .await
                                    .unwrap_or_default();
                                    tracing::info!(
                                        target: "mcv::main",
                                        batch_size = all_rows.len(),
                                        connection_id = %connection_id,
                                        working_set_mb = crash_handler::get_process_memory_mb(),
                                        "Emitting comment-received batch"
                                    );
                                    if let Err(e) = app_handle.emit("comment-received", all_rows) {
                                        tracing::error!(
                                            target: "mcv::main",
                                            error = %e,
                                            "Failed to emit comment-received event"
                                        );
                                    }
                                } // if history or timed messages exist
                            }
                            MessageType::StreamMetadata => {
                                tracing::debug!(target: "mcv::main", "Emitting stream-metadata event");
                                if let Err(e) = app_handle.emit("stream-metadata", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit stream-metadata event"
                                    );
                                }
                            }
                            MessageType::Connected => {
                                tracing::debug!(target: "mcv::main","Emitting connected event");
                                if let Err(e) = app_handle.emit("connected", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit connected event"
                                    );
                                }
                            }
                            MessageType::ConnectFailed => {
                                tracing::debug!(target: "mcv::main","Emitting connect-failed event");
                                if let Err(e) = app_handle.emit("connect-failed", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit connect-failed event"
                                    );
                                }
                            }
                            MessageType::Disconnected => {
                                tracing::debug!(target: "mcv::main","Emitting disconnected event");
                                // 切断時に connection_id 単位のタイミング基準をクリア
                                if let Ok(disc) = serde_json::from_value::<DisconnectedPayload>(
                                    message.payload.clone(),
                                ) {
                                    timing_clone.lock().await.remove(&disc.connection_id);
                                }
                                if let Err(e) = app_handle.emit("disconnected", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit disconnected event"
                                    );
                                }
                            }
                            MessageType::AddSite => {
                                tracing::trace!(target: "mcv::main", "Emitting site-added event");
                                if let Err(e) = app_handle.emit("site-added", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit site-added event"
                                    );
                                }
                            }
                            MessageType::AddBrowser => {
                                tracing::trace!(target: "mcv::main", "Emitting browser-added event");
                                if let Err(e) = app_handle.emit("browser-added", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit browser-added event"
                                    );
                                }
                            }
                            MessageType::RemoveBrowser => {
                                tracing::trace!(target: "mcv::main", "Emitting browser-removed event");
                                if let Err(e) = app_handle.emit("browser-removed", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit browser-removed event"
                                    );
                                }
                            }
                            MessageType::UpdateConnectionAccount => {
                                tracing::debug!(target: "mcv::main", "Emitting connection-account-updated event");
                                if let Err(e) = app_handle.emit("connection-account-updated", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit connection-account-updated event"
                                    );
                                }
                            }
                            _ => {}
                        }
                });
            });

            core_actor.set_event_callback(event_callback);
            core_actor.set_log_storage(mcv_log_core::get_storage());
            core_actor.set_settings_storage(settings_storage.clone());

            // 接続永続化ファイルのパスを設定
            let connections_file_path = settings_dir.join("connections.json");
            core_actor.set_connections_file_path(connections_file_path.clone());

            // ログディレクトリパスを設定（GetLogsDir への応答用）
            core_actor.set_logs_dir(logs_dir);

            // 接続を復元（プラグイン読み込み前に実行）
            tracing::info!(
                target: "mcv::main",
                path = ?connections_file_path,
                "Restoring connections from file"
            );
            if let Err(e) = core_actor.restore_connections() {
                tracing::warn!(
                    target: "mcv::main",
                    error = %e,
                    "Failed to restore connections"
                );
            }

            tracing::debug!(target: "mcv::main","Starting CoreActor");
            let core_addr = core_actor.start();
            tracing::info!(target: "mcv::main","CoreActor started");

            tracing::debug!(target: "mcv::main","Setting core_addr to PluginManager");
            plugin_manager.set_core_addr(core_addr.clone());
            tracing::debug!(target: "mcv::main","core_addr set to PluginManager");

            // プラグインディレクトリを決定
            let plugins_dir = get_plugin_dir();

            tracing::info!(target: "mcv::main", plugins_dir = %plugins_dir.display(), "Loading DLL plugins from directory");

            // 前回セッションでアンインストール待ちになったディレクトリを先にクリーンアップ
            cleanup_pending_uninstalls(&plugins_dir);

            // プラグインディレクトリをスキャンして自動ロード
            let loaded_plugins = plugin_manager.scan_and_load_plugins(&plugins_dir).await;

            tracing::info!(target: "mcv::main", count = loaded_plugins.len(), "Loaded DLL plugins");

            // ロードされた物理プラグインをCoreActorに登録
            for loaded_info in loaded_plugins {
                tracing::debug!(
                    target: "mcv::main",
                    physical_plugin_id = %loaded_info.physical_plugin_id,
                    abi_version = loaded_info.abi_version,
                    plugin_name = ?loaded_info.plugin_name,
                    "Registering physical plugin with CoreActor"
                );

                core_addr.do_send(mcv_core::core_actor::RegisterPhysicalPlugin {
                    physical_plugin_id: loaded_info.physical_plugin_id,
                    host_addr: loaded_info.host_addr,
                });
            }

            // 定期メモリ使用量ログ（60秒ごと）
            actix::spawn(async {
                let mut interval =
                    tokio::time::interval(std::time::Duration::from_secs(60));
                loop {
                    interval.tick().await;
                    if let Some(mb) = crash_handler::get_process_memory_mb() {
                        tracing::info!(
                            target: "mcv::main",
                            working_set_mb = mb,
                            "Periodic memory usage"
                        );
                    }
                }
            });

            // AppStateを作成してメインスレッドに送信
            let app_state = AppState {
                core_addr,
                plugin_manager: Arc::new(tokio::sync::Mutex::new(plugin_manager)),
                comment_store,
            };

            tracing::debug!(target: "mcv::main", "Sending AppState to main thread");
            tx.send((app_state, app_handle)).expect("Failed to send AppState");

            tracing::info!(target: "mcv::main", "Actix system setup complete, keeping system alive");
        });

        // actixシステムを実行し続ける
        actix_system.run().expect("Actix system failed");
    });

    // メインスレッドでAppStateを受信
    tracing::debug!(target: "mcv::main", "Waiting for AppState from actix thread");
    let (app_state, app_handle) = rx.recv().expect("Failed to receive AppState");
    tracing::info!(target: "mcv::main", "Received AppState, starting Tauri");

    // Tauriアプリを起動
    let settings_dir_for_manage = settings_dir_for_tauri.clone();
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .setup(move |app| {
            // ウィンドウタイトルの設定・ウィンドウ状態の復元
            if let Some(window) = app.get_webview_window("main") {
                let title = get_title();
                let _ = window.set_title(&title);
                tracing::debug!(target: "mcv::main", title = %title, "Window title set");

                // 前回のウィンドウ状態（位置・サイズ・最大化）を復元（core.json から読み込み）
                restore_window_state(&window, &settings_dir_for_tauri);
                tracing::debug!(target: "mcv::main", settings_dir = %settings_dir_for_tauri.display(), "Window state restore attempted");

                // ウィンドウを閉じる時にウィンドウ状態を保存（core.json にマージ）
                let window_for_close = window.clone();
                let settings_dir_for_close = settings_dir_for_tauri.clone();
                window.on_window_event(move |event| {
                    if let tauri::WindowEvent::CloseRequested { .. } = event {
                        save_window_state(&window_for_close, &settings_dir_for_close);
                    }
                });
            }


            // AppHandleを保存（ブロッキング操作）
            let handle = app.handle().clone();
            let app_handle_clone = app_handle.clone();
            std::thread::spawn(move || {
                let rt = tokio::runtime::Runtime::new().unwrap();
                rt.block_on(async move {
                    *app_handle_clone.lock().await = Some(handle);
                    tracing::debug!(target: "mcv::main", "AppHandle set successfully");
                });
            });
            Ok(())
        })
        .manage(app_state)
        .manage(SettingsDirState { settings_dir: settings_dir_for_manage })
        .invoke_handler(tauri::generate_handler![
            add_connection,
            remove_connection,
            rename_connection,
            connect,
            disconnect,
            get_connections,
            get_sites,
            get_browsers,
            set_connection_site,
            update_connection_settings,
            send_comment,
            get_comment_schema,
            fetch_account_info,
            frontend_trace,
            check_for_updates,
            get_current_version,
            list_registry_plugins,
            download_core_update,
            apply_core_update,
            install_registry_plugin,
            uninstall_registry_plugin,
            list_installed_plugins,
            get_logs,
            get_build_profile_info,
            get_settings_schema,
            get_settings,
            update_settings,
            get_plugins,
            search_comments,
            get_users,
            detect_url,
            get_column_settings,
            save_column_settings
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
fn get_title() -> String {
    let version = env!("CARGO_PKG_VERSION");

    #[cfg(feature = "alpha")]
    let title = format!("MultiCommentViewer v{} (アルファ版)", version);

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    let title = format!("MultiCommentViewer v{} (ベータ版)", version);

    #[cfg(all(not(feature = "alpha"), not(feature = "beta")))]
    let title = format!("MultiCommentViewer v{}", version);
    title
}

/// ビルドプロファイルを取得
fn get_build_profile() -> &'static str {
    #[cfg(feature = "alpha")]
    return "alpha";

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "beta";

    #[cfg(all(not(feature = "alpha"), not(feature = "beta")))]
    return "stable";
}
fn exe_dir() -> PathBuf {
    std::env::current_exe()
        .expect("failed to get current_exe")
        .parent()
        .expect("exe has no parent")
        .to_path_buf()
}
#[cfg(debug_assertions)]
fn get_plugin_dir() -> PathBuf {
    // デバッグ環境: exe と同じ場所にある plugins/ サブディレクトリ
    exe_dir().join("plugins")
}

#[cfg(not(debug_assertions))]
fn get_plugin_dir() -> PathBuf {
    // 本番環境: %LOCALAPPDATA%\MultiCommentViewer\plugins\
    let local_app_data = std::env::var("LOCALAPPDATA").unwrap_or_else(|_| {
        tracing::warn!(
            target: "mcv::main",
            "LOCALAPPDATA not set, using fallback path"
        );
        String::from("C:\\Users\\Default\\AppData\\Local")
    });
    PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("plugins")
}
