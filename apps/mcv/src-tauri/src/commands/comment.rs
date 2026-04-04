use mcv_core::{GetLogicalPlugins, SendRequest};
use mcv_messages::{
    BrowserInfo as MsgBrowserInfo, FetchAccountInfoPayload, GetSendCommentSchemaPayload,
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginId,
    SendCommentPayload, SendCommentSchemaPayload,
};
use tauri::State;
use uuid::Uuid;

use crate::comment_store::UserInfo;
use crate::types::AppState;
#[cfg(feature = "comment-search")]
use crate::types::CommentRow;

/// コメント投稿フォームスキーマを取得するヘルパー
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

/// コメントを送信
#[tauri::command]
pub(crate) async fn send_comment(
    state: State<'_, AppState>,
    connection_id: String,
    text: String,
    extra: serde_json::Value,
) -> Result<String, String> {
    let conn_id =
        Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid connection_id: {}", e))?;

    // 接続情報を取得してplugin_idを取得
    let connections = state
        .core_addr
        .send(mcv_core::GetConnections)
        .await
        .map_err(|e| e.to_string())?;
    let conn_info = connections
        .into_iter()
        .find(|c| c.connection_id == conn_id)
        .ok_or_else(|| "Connection not found".to_string())?;
    let plugin_id = conn_info
        .plugin_id
        .ok_or("Plugin not assigned to this connection")?;

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
pub(crate) async fn get_comment_schema(
    state: State<'_, AppState>,
    connection_id: String,
) -> Result<SendCommentSchemaPayload, String> {
    let conn_id =
        Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid connection_id: {}", e))?;
    get_send_comment_schema(&state, conn_id).await
}

/// サイト＋ブラウザ選択時にアカウント情報をプリフェッチ
///
/// サイト（plugin_id）とブラウザ（browser_id）が両方設定されている場合のみ、
/// プラグインに FetchAccountInfo メッセージを送信する。
/// どちらか未設定の場合はエラーにせず Ok(()) を返す。
#[tauri::command]
pub(crate) async fn fetch_account_info(
    state: State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    tracing::info!(
        target: "mcv::main",
        connection_id = %connection_id,
        "fetch_account_info command called"
    );
    let conn_id =
        Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid connection_id: {}", e))?;

    let connections = state
        .core_addr
        .send(mcv_core::GetConnections)
        .await
        .map_err(|e| e.to_string())?;
    let conn_info = connections
        .into_iter()
        .find(|c| c.connection_id == conn_id)
        .ok_or_else(|| "Connection not found".to_string())?;

    let plugin_id = match conn_info.plugin_id {
        Some(id) => id,
        None => {
            tracing::debug!(
                target: "mcv::main",
                connection_id = %connection_id,
                "fetch_account_info skipped: plugin_id is not set"
            );
            return Ok(());
        }
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
        }
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

/// プラグイン一覧情報
#[derive(serde::Serialize)]
pub(crate) struct PluginInfoResponse {
    plugin_id: String,
    name: String,
}

/// プラグイン一覧を取得
#[tauri::command]
pub(crate) async fn get_plugins(
    state: State<'_, AppState>,
) -> Result<Vec<PluginInfoResponse>, String> {
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

/// コメント検索
#[cfg(feature = "comment-search")]
#[tauri::command]
pub(crate) fn search_comments(
    query: String,
    connection_ids: Vec<String>,
    state: State<'_, AppState>,
) -> Result<Vec<CommentRow>, String> {
    let store = state.comment_store.lock().map_err(|e| e.to_string())?;
    store
        .search_comments(&query, &connection_ids)
        .map_err(|e| e.to_string())
}

/// ユーザー一覧を取得
#[tauri::command]
pub(crate) fn get_users(
    limit: u32,
    offset: u32,
    state: State<'_, AppState>,
) -> Result<Vec<UserInfo>, String> {
    let store = state.comment_store.lock().map_err(|e| e.to_string())?;
    store
        .get_users(limit as usize, offset as usize)
        .map_err(|e| e.to_string())
}
