use mcv_core::SendRequest;
use mcv_messages::{Message as McvMessage, MessageDestination, MessageSource, MessageType};
use tauri::State;

use crate::types::{AppState, ColumnSettings, SettingsDirState};

/// 設定スキーマを取得
#[tauri::command]
pub(crate) async fn get_settings_schema(
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
pub(crate) async fn get_settings(
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
pub(crate) async fn update_settings(
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

/// core.json から列設定を読み込む
#[tauri::command]
pub(crate) fn get_column_settings(
    state: tauri::State<'_, SettingsDirState>,
) -> Option<ColumnSettings> {
    let core_json = state.settings_dir.join("core.json");
    let data: serde_json::Value = std::fs::read_to_string(&core_json)
        .ok()
        .and_then(|s| serde_json::from_str(&s).ok())?;
    serde_json::from_value(data.get("columns")?.clone()).ok()
}

/// 列設定を core.json にマージ保存する
#[tauri::command]
pub(crate) fn save_column_settings(
    settings: ColumnSettings,
    state: tauri::State<'_, SettingsDirState>,
) {
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
