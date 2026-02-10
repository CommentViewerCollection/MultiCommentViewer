use crate::core_actor::CoreActor;
use mcv_messages::*;
use mcv_settings_core::SettingsEntry;

/// get-settings-schema メッセージハンドラ
pub async fn handle_get_settings_schema(
    _core: &CoreActor,
    payload: GetSettingsSchemaPayload,
) -> Result<SettingsSchemaPayload, String> {
    if payload.target == "core" {
        // Core の設定スキーマを返す
        Ok(SettingsSchemaPayload {
            target: "core".to_string(),
            schema: CoreActor::get_core_settings_schema(),
        })
    } else {
        // プラグインの設定スキーマを取得（プラグインに転送する必要がある）
        Err(format!(
            "Plugin settings schema request should be forwarded to plugin: {}",
            payload.target
        ))
    }
}

/// get-settings メッセージハンドラ
pub async fn handle_get_settings(
    core: &CoreActor,
    payload: GetSettingsPayload,
) -> Result<SettingsDataPayload, String> {
    if payload.target == "core" {
        // Core の設定を取得
        if let Some(storage) = &core.settings_storage {
            let storage = storage.lock().map_err(|e| format!("Lock error: {}", e))?;

            match storage.get_settings("core") {
                Ok(Some(entry)) => {
                    Ok(SettingsDataPayload {
                        target: "core".to_string(),
                        data: entry.data,
                    })
                }
                Ok(None) => {
                    // 設定がない場合はデフォルト値を返す
                    Ok(SettingsDataPayload {
                        target: "core".to_string(),
                        data: serde_json::json!({
                            "theme": "dark",
                            "auto_scroll": true,
                            "max_comments": 1000
                        }),
                    })
                }
                Err(e) => Err(format!("Failed to get settings: {}", e)),
            }
        } else {
            // ストレージが設定されていない場合はデフォルト値を返す
            Ok(SettingsDataPayload {
                target: "core".to_string(),
                data: serde_json::json!({
                    "theme": "dark",
                    "auto_scroll": true,
                    "max_comments": 1000
                }),
            })
        }
    } else {
        // プラグインの設定を取得（プラグインに転送する必要がある）
        Err(format!(
            "Plugin settings request should be forwarded to plugin: {}",
            payload.target
        ))
    }
}

/// update-settings メッセージハンドラ
pub async fn handle_update_settings(
    core: &mut CoreActor,
    payload: UpdateSettingsPayload,
) -> Result<(), String> {
    if payload.target == "core" {
        // Core の設定を保存
        if let Some(storage) = &core.settings_storage {
            let storage = storage.lock().map_err(|e| format!("Lock error: {}", e))?;

            let entry = SettingsEntry {
                target: "core".to_string(),
                schema: CoreActor::get_core_settings_schema(),
                data: payload.data,
                updated_at: chrono::Utc::now().timestamp(),
            };

            storage
                .save_settings(&entry)
                .map_err(|e| format!("Failed to save settings: {}", e))?;

            Ok(())
        } else {
            Err("Settings storage is not configured".to_string())
        }
    } else {
        // プラグインの設定を更新（プラグインに転送する必要がある）
        Err(format!(
            "Plugin settings update should be forwarded to plugin: {}",
            payload.target
        ))
    }
}
