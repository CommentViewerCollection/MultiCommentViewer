use mcv_common::{BrowserId, SiteId};
use mcv_core::{
    BrowserInfo as CoreBrowserInfo, ConnectionInfo, DetectUrl, GetBrowsers, GetConnections,
    GetSites, RemoveConnection, RenameConnection, SendRequest, SetConnectionSite,
    SiteInfo as CoreSiteInfo, UpdateConnectionSettings,
};
use mcv_messages::{
    BrowserInfo as MsgBrowserInfo, ConnectPayload, ConnectionInputSchemaPayload, DisconnectPayload,
    GetConnectionInputSchemaPayload, InputInfo, Message as McvMessage, MessageDestination,
    MessageSource, MessageType, PluginId, SiteInfo as MsgSiteInfo,
};
use tauri::State;
use uuid::Uuid;

use crate::types::AppState;

/// UUID文字列をパースするヘルパー
fn parse_uuid(id_str: &str, id_type: &str) -> Result<Uuid, String> {
    Uuid::parse_str(id_str).map_err(|e| format!("Invalid {}: {}", id_type, e))
}

/// 接続情報を取得するヘルパー
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

/// 接続入力スキーマを取得するヘルパー
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

/// 接続を追加
#[tauri::command]
pub(crate) async fn add_connection(state: State<'_, AppState>) -> Result<String, String> {
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
pub(crate) async fn remove_connection(
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
pub(crate) async fn rename_connection(
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
pub(crate) async fn connect(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = parse_uuid(&connection_id, "connection_id")?;

    let conn_info = get_connection_info(&state, conn_id).await?;
    let input_schema = get_connection_input_schema(&state, conn_id).await?;

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
pub(crate) async fn disconnect(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

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
pub(crate) async fn get_connections(
    state: tauri::State<'_, AppState>,
) -> Result<Vec<ConnectionInfo>, String> {
    state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())
}

/// サイト一覧を取得
#[tauri::command]
pub(crate) async fn get_sites(
    state: tauri::State<'_, AppState>,
) -> Result<Vec<CoreSiteInfo>, String> {
    state
        .core_addr
        .send(GetSites)
        .await
        .map_err(|e| e.to_string())
}

/// ブラウザ一覧を取得
#[tauri::command]
pub(crate) async fn get_browsers(
    state: tauri::State<'_, AppState>,
) -> Result<Vec<CoreBrowserInfo>, String> {
    state
        .core_addr
        .send(GetBrowsers)
        .await
        .map_err(|e| e.to_string())
}

/// 接続にサイトを設定
#[tauri::command]
pub(crate) async fn set_connection_site(
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
        .map_err(|e| format!("Failed to set connection site: {}", e))??;

    Ok(())
}

/// 接続設定を更新
#[tauri::command]
pub(crate) async fn update_connection_settings(
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
        .map_err(|e| format!("Failed to update connection settings: {}", e))??;

    Ok(())
}

/// URLを処理できるサイトを検出
#[tauri::command]
pub(crate) async fn detect_url(
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
