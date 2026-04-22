use crate::core_actor::CoreActor;
use crate::plugin_host_actor::SendMessageToPlugin;
use actix::Context;
use mcv_common::PluginId;
use mcv_messages::*;
use mcv_settings_core::SettingsEntry;

/// get-settings-schema メッセージハンドラ
pub fn handle_get_settings_schema(
    core: &CoreActor,
    message: &Message,
    _ctx: &mut Context<CoreActor>,
) -> Result<Message, String> {
    let payload: GetSettingsSchemaPayload = serde_json::from_value(message.payload.clone())
        .map_err(|e| format!("Failed to parse get-settings-schema payload: {}", e))?;

    if payload.target == "core" {
        // Core の設定スキーマを返す
        let response_payload = SettingsSchemaPayload {
            target: "core".to_string(),
            schema: CoreActor::get_core_settings_schema(),
        };

        Ok(Message::create_response(
            message,
            MessageType::SettingsSchema,
            serde_json::to_value(response_payload)
                .map_err(|e| format!("Failed to serialize response: {}", e))?,
        ))
    } else {
        // プラグインの設定スキーマを取得（プラグインに転送）
        let logical_plugin_id = PluginId::new(&payload.target);

        if let Some(plugin_info) = core.logical_plugins.get(&logical_plugin_id) {
            // キャッシュがあればそれを返す
            if let Some(cached_schema) = &plugin_info.settings_schema {
                let response_payload = SettingsSchemaPayload {
                    target: payload.target.clone(),
                    schema: cached_schema.clone(),
                };
                return Ok(Message::create_response(
                    message,
                    MessageType::SettingsSchema,
                    serde_json::to_value(response_payload)
                        .map_err(|e| format!("Failed to serialize response: {}", e))?,
                ));
            }

            // キャッシュ未登録の場合はプラグインに転送して空を返す（暫定）
            let forward_message = Message::new_notification(
                MessageType::GetSettingsSchema,
                message.src.clone(),
                MessageDestination::Plugin {
                    plugin_id: logical_plugin_id.clone(),
                },
                message.payload.clone(),
            );

            plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: forward_message,
            });

            Ok(Message::create_response(
                message,
                MessageType::SettingsSchema,
                serde_json::json!({
                    "target": payload.target,
                    "schema": {}
                }),
            ))
        } else {
            Err(format!("Plugin not found: {}", payload.target))
        }
    }
}

/// get-settings メッセージハンドラ
pub fn handle_get_settings(
    core: &CoreActor,
    message: &Message,
    _ctx: &mut Context<CoreActor>,
) -> Result<Message, String> {
    let payload: GetSettingsPayload = serde_json::from_value(message.payload.clone())
        .map_err(|e| format!("Failed to parse get-settings payload: {}", e))?;

    if payload.target == "core" {
        // Core の設定を取得
        let data = if let Some(storage) = &core.settings_storage {
            let storage = storage.lock().map_err(|e| format!("Lock error: {}", e))?;

            match storage.get_settings("core") {
                Ok(Some(entry)) => entry.data,
                Ok(None) => {
                    // 設定がない場合はデフォルト値を返す
                    serde_json::json!({
                        "theme": "dark",
                        "enable_color_by_plugin_or_connection": false,
                        "color_mode": "site",
                        "site_colors": {},
                        "custom_theme_colors": {}
                    })
                }
                Err(e) => return Err(format!("Failed to get settings: {}", e)),
            }
        } else {
            // ストレージが設定されていない場合はデフォルト値を返す
            serde_json::json!({
                "theme": "dark",
                "auto_scroll": true,
                "max_comments": 1000,
                "enable_color_by_plugin_or_connection": false,
                "color_mode": "site",
                "site_colors": {},
                "custom_theme_colors": {}
            })
        };

        let response_payload = SettingsDataPayload {
            target: "core".to_string(),
            data,
        };

        Ok(Message::create_response(
            message,
            MessageType::SettingsData,
            serde_json::to_value(response_payload)
                .map_err(|e| format!("Failed to serialize response: {}", e))?,
        ))
    } else {
        // プラグインの設定を取得（プラグインに転送）
        let logical_plugin_id = PluginId::new(&payload.target);

        if let Some(plugin_info) = core.logical_plugins.get(&logical_plugin_id) {
            // キャッシュがあればそれを返す
            if let Some(cached_data) = &plugin_info.settings_data {
                let response_payload = SettingsDataPayload {
                    target: payload.target.clone(),
                    data: cached_data.clone(),
                };
                return Ok(Message::create_response(
                    message,
                    MessageType::SettingsData,
                    serde_json::to_value(response_payload)
                        .map_err(|e| format!("Failed to serialize response: {}", e))?,
                ));
            }

            // キャッシュ未登録の場合はプラグインに転送して空を返す（暫定）
            let forward_message = Message::new_notification(
                MessageType::GetSettings,
                message.src.clone(),
                MessageDestination::Plugin {
                    plugin_id: logical_plugin_id.clone(),
                },
                message.payload.clone(),
            );

            plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: forward_message,
            });

            Ok(Message::create_response(
                message,
                MessageType::SettingsData,
                serde_json::json!({
                    "target": payload.target,
                    "data": {}
                }),
            ))
        } else {
            Err(format!("Plugin not found: {}", payload.target))
        }
    }
}

/// update-settings メッセージハンドラ
pub fn handle_update_settings(
    core: &mut CoreActor,
    message: &Message,
    _ctx: &mut Context<CoreActor>,
) -> Result<Message, String> {
    let payload: UpdateSettingsPayload = serde_json::from_value(message.payload.clone())
        .map_err(|e| format!("Failed to parse update-settings payload: {}", e))?;

    if payload.target == "core" {
        // Core の設定を保存
        if let Some(storage) = &core.settings_storage {
            let storage = storage.lock().map_err(|e| format!("Lock error: {}", e))?;

            // Rust/ドラッグで管理するフィールド（ウィンドウ状態・サイドバー幅）を既存データから引き継ぐ
            let mut data = payload.data;
            if let Ok(Some(existing)) = storage.get_settings("core") {
                for key in [
                    "window_maximized",
                    "window_x",
                    "window_y",
                    "window_width",
                    "window_height",
                    "sidebar_width",
                ] {
                    if let Some(v) = existing.data.get(key) {
                        if data.get(key).is_none() {
                            data[key] = v.clone();
                        }
                    }
                }
            }

            let entry = SettingsEntry {
                target: "core".to_string(),
                schema: CoreActor::get_core_settings_schema(),
                data,
                updated_at: chrono::Utc::now().timestamp(),
            };

            storage
                .save_settings(&entry)
                .map_err(|e| format!("Failed to save settings: {}", e))?;

            // 成功応答を返す（空のペイロード）
            Ok(Message::create_response(
                message,
                MessageType::UpdateSettings,
                serde_json::json!({}),
            ))
        } else {
            Err("Settings storage is not configured".to_string())
        }
    } else {
        // プラグインの設定を更新（プラグインに転送）
        let logical_plugin_id = PluginId::new(&payload.target);

        if let Some(plugin_info) = core.logical_plugins.get_mut(&logical_plugin_id) {
            // 設定データをキャッシュに保存（次回 get-settings でキャッシュから返す）
            plugin_info.settings_data = Some(payload.data.clone());

            // プラグインにメッセージを転送
            let forward_message = Message::new_notification(
                MessageType::UpdateSettings,
                message.src.clone(),
                MessageDestination::Plugin {
                    plugin_id: logical_plugin_id.clone(),
                },
                message.payload.clone(),
            );

            plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: forward_message,
            });

            // 成功応答を返す
            Ok(Message::create_response(
                message,
                MessageType::UpdateSettings,
                serde_json::json!({}),
            ))
        } else {
            Err(format!("Plugin not found: {}", payload.target))
        }
    }
}
