use actix::Context;
use mcv_common::LogicalPluginId;
use mcv_messages::{
    BrowserInfo as MsgBrowserInfo, ConnectFailedPayload, ConnectPayload, ConnectedPayload,
    ConnectionAddedPayload, ConnectionInputSchemaPayload, DisconnectPayload, DisconnectedPayload,
    GetConnectionInputSchemaPayload, InputInfo, Message as McvMessage, MessageDestination,
    MessageSource, MessageType, SiteInfo as MsgSiteInfo, UpdateConnectionAccountPayload,
};
use uuid::Uuid;

use crate::connection_manager::ConnectionStatus;
use crate::core_actor::CoreActor;
use crate::message_handlers;
use crate::plugin_host_actor::SendMessageToPlugin;

fn effective_connect_payload(actor: &CoreActor, incoming: ConnectPayload) -> ConnectPayload {
    let Some(conn) = actor.connection_manager.get_connection(&incoming.connection_id) else {
        return incoming;
    };

    let site = match &conn.site_id {
        Some(site_id) => MsgSiteInfo {
            name: actor
                .site_browser_manager
                .get_site(site_id)
                .map(|s| s.display_name.clone())
                .unwrap_or_else(|| site_id.to_string()),
            id: site_id.clone(),
        },
        None => incoming.site,
    };

    let browser = match &conn.browser_id {
        Some(browser_id) => MsgBrowserInfo {
            name: actor
                .site_browser_manager
                .get_browser(browser_id)
                .map(|b| b.display_name.clone())
                .unwrap_or_else(|| browser_id.as_str().to_string()),
            id: browser_id.clone(),
        },
        None => incoming.browser,
    };

    let mut normalized_input = conn.input_state.clone().unwrap_or_else(|| {
        if incoming.input.extra.is_null() {
            serde_json::json!({})
        } else {
            incoming.input.extra.clone()
        }
    });
    if !normalized_input.is_object() {
        normalized_input = serde_json::json!({});
    }
    if let Some(obj) = normalized_input.as_object_mut() {
        if !obj.contains_key("url") {
            if let Some(url) = &conn.url {
                obj.insert("url".to_string(), serde_json::Value::String(url.clone()));
            }
        }
        if !obj.contains_key("advanced_settings") {
            if let Some(settings) = &conn.advanced_settings {
                obj.insert("advanced_settings".to_string(), settings.clone());
            }
        }
    }

    ConnectPayload {
        connection_id: incoming.connection_id,
        site,
        input: InputInfo {
            input_type: "url".to_string(),
            extra: normalized_input,
        },
        browser,
    }
}

/// デフォルトの接続名を生成
pub fn generate_connection_default_name(actor: &CoreActor) -> String {
    // 現在の接続を取得してデフォルト名を生成
    let connections = actor.connection_manager.get_connections();

    // 既存の接続名から#N形式の番号を抽出
    let mut used_numbers = std::collections::HashSet::new();
    for conn in &connections {
        if let Some(stripped) = conn.name.strip_prefix('#') {
            if let Ok(num) = stripped.parse::<u32>() {
                used_numbers.insert(num);
            }
        }
    }

    // #1から順に空いている番号を探す
    let mut next_number = 1;
    while used_numbers.contains(&next_number) {
        next_number += 1;
    }

    format!("#{}", next_number)
}

/// add-connection メッセージのハンドラー
pub fn handle_add_connection(
    actor: &mut CoreActor,
    _message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    tracing::trace!(
        target: "mcv::core::CoreActor",
        "handle_add_connection()"
    );

    let connection_id = Uuid::new_v4();

    // Connection Managerに登録
    let conn_name = generate_connection_default_name(actor);

    actor
        .connection_manager
        .add_connection(connection_id, conn_name.clone());
    tracing::debug!(
        target: "mcv::core::CoreActor",
        connection_id = %connection_id,
        name = %conn_name,
        "Connection added"
    );

    // 全論理プラグインにConnectionAddedをブロードキャスト
    // ユニキャスト送信は不要（送信元も含めて全員がブロードキャストで受信する）
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::to_value(ConnectionAddedPayload {
            connection_id,
            name: conn_name,
        })
        .unwrap(),
    );
    message_handlers::plugin_hello::broadcast_to_all_logical_plugins(actor, broadcast_msg);
    tracing::debug!(
        target: "mcv::core::CoreActor",
        "Broadcasted connection-added to all plugins"
    );

    // 保存
    let _ = actor.save_connections();
}

/// connect メッセージのハンドラー
pub fn handle_connect(actor: &mut CoreActor, message: &McvMessage, _ctx: &mut Context<CoreActor>) {
    let payload: ConnectPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "connect",
                "Failed to parse message payload"
            );
            return;
        }
    };

    let effective_payload = effective_connect_payload(actor, payload);
    let connection_id = effective_payload.connection_id;

    // Connection Managerから接続情報を取得してplugin_idを取得
    let plugins = actor.logical_plugins.clone();
    let mut msg = message.clone();
    if let Ok(v) = serde_json::to_value(&effective_payload) {
        msg.payload = v;
    }

    actor
        .connection_manager
        .update_status(&connection_id, ConnectionStatus::Connecting);

    // plugin_idを取得
    if let Some(conn_info) = actor.connection_manager.get_connection(&connection_id) {
        if let Some(plugin_id_uuid) = conn_info.plugin_id {
            let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);

            // デバッグ: 登録されている全plugin_idをログ出力
            let registered_plugin_ids: Vec<String> =
                plugins.keys().map(|id| id.to_string()).collect();
            tracing::debug!(
                target: "mcv::core::CoreActor",
                connection_id = %connection_id,
                plugin_id_from_connection = %plugin_id_uuid,
                registered_plugin_ids = ?registered_plugin_ids,
                "Attempting to find plugin for connection"
            );

            // プラグインへconnectメッセージを転送
            if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                tracing::debug!(
                    target: "mcv::core::CoreActor",
                    plugin_id = %plugin_id_uuid,
                    plugin_name = %plugin_info.name,
                    connection_id = %connection_id,
                    "Found plugin, forwarding connect message"
                );
                plugin_info
                    .host_addr
                    .do_send(SendMessageToPlugin { message: msg });
            } else {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    plugin_id = %plugin_id_uuid,
                    connection_id = %connection_id,
                    registered_plugin_count = plugins.len(),
                    "Plugin not found - plugin_id mismatch detected"
                );
            }
        } else {
            tracing::error!(
                target: "mcv::core::CoreActor",
                connection_id = %connection_id,
                "Connection has no plugin_id set"
            );
        }
    } else {
        tracing::error!(
            target: "mcv::core::CoreActor",
            connection_id = %connection_id,
            "Connection not found"
        );
    }
}

/// get-connection-input-schema メッセージのハンドラー（現状はURL入力のみ）
pub fn handle_get_connection_input_schema(
    _actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) -> Result<McvMessage, String> {
    let payload: GetConnectionInputSchemaPayload =
        serde_json::from_value(message.payload.clone())
            .map_err(|e| format!("Failed to parse GetConnectionInputSchemaPayload: {}", e))?;
    let schema = serde_json::json!({
        "type": "object",
        "properties": {
            "url": { "type": "string", "title": "URL" }
        },
        "required": ["url"]
    });
    Ok(message.create_response(
        MessageType::ConnectionInputSchema,
        serde_json::to_value(ConnectionInputSchemaPayload {
            connection_id: payload.connection_id,
            schema,
            ui_schema: None,
            initial_data: None,
        })
        .map_err(|e| format!("Failed to serialize ConnectionInputSchemaPayload: {}", e))?,
    ))
}

/// connected メッセージのハンドラー
pub fn handle_connected(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: ConnectedPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "connected",
                "Failed to parse message payload"
            );
            return;
        }
    };

    tracing::info!(
        connection_id = %payload.connection_id,
        "Connection established"
    );

    // Connection Managerのステータスを更新
    let connection_id = payload.connection_id;
    actor
        .connection_manager
        .update_status(&connection_id, ConnectionStatus::Connected);

    // 全論理プラグインにConnectedをブロードキャスト
    let broadcast_msg = McvMessage::new_notification(
        MessageType::Connected,
        MessageSource::Core,
        MessageDestination::Broadcast,
        message.payload.clone(),
    );
    message_handlers::plugin_hello::broadcast_to_all_logical_plugins(actor, broadcast_msg);

    // UIへイベント通知
    if let Some(callback) = &actor.event_callback {
        callback(message.clone());
    }
}

/// connect-failed メッセージのハンドラー
pub fn handle_connect_failed(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: ConnectFailedPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "connect-failed",
                "Failed to parse message payload"
            );
            return;
        }
    };

    tracing::warn!(
        connection_id = %payload.connection_id,
        reason = %payload.reason,
        "Connection failed"
    );

    actor
        .connection_manager
        .update_status(&payload.connection_id, ConnectionStatus::Disconnected);

    if let Some(callback) = &actor.event_callback {
        callback(message.clone());
    }
}

/// disconnect メッセージのハンドラー
pub fn handle_disconnect(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: DisconnectPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "disconnect",
                "Failed to parse message payload"
            );
            return;
        }
    };

    // プラグインへdisconnectメッセージを転送
    if let MessageSource::Core = message.src {
        // UIからのリクエストの場合、該当するプラグインへ転送
        let connection_id = payload.connection_id;
        let plugins = actor.logical_plugins.clone();
        let msg = message.clone();

        if let Some(conn_info) = actor.connection_manager.get_connection(&connection_id) {
            if let Some(plugin_id_uuid) = conn_info.plugin_id {
                let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);
                if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                    plugin_info
                        .host_addr
                        .do_send(SendMessageToPlugin { message: msg });
                }
            }
        }
    }
}

/// disconnected メッセージのハンドラー
pub fn handle_disconnected(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: DisconnectedPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "disconnected",
                "Failed to parse message payload"
            );
            return;
        }
    };

    tracing::info!(
        connection_id = %payload.connection_id,
        "Connection disconnected"
    );

    // Connection Managerのステータスを更新
    let connection_id = payload.connection_id;
    actor
        .connection_manager
        .update_status(&connection_id, ConnectionStatus::Disconnected);

    // UIへイベント通知
    if let Some(callback) = &actor.event_callback {
        callback(message.clone());
    }
}

/// update-connection-account メッセージのハンドラー
///
/// プラグインから接続中のログインアカウント情報を受け取り、ConnectionInfo に保存する。
/// account が None の場合はアカウント情報をクリアする（切断時など）。
pub fn handle_update_connection_account(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: UpdateConnectionAccountPayload =
        match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "update-connection-account",
                    "Failed to parse message payload"
                );
                return;
            }
        };

    tracing::debug!(
        connection_id = %payload.connection_id,
        has_account = payload.account.is_some(),
        "Updating connection account info"
    );

    // 送信元プラグインと現在の接続先プラグインが一致しない場合は破棄する。
    // サイト切り替え直後に古いプラグインから遅延通知が届いても上書きしないためのガード。
    let src_plugin_id = match message.src {
        MessageSource::Plugin { plugin_id } => Some(plugin_id),
        MessageSource::Core => None,
    };
    if let Some(current_conn) = actor.connection_manager.get_connection(&payload.connection_id) {
        if let (Some(src), Some(current)) = (src_plugin_id, current_conn.plugin_id) {
            if src != current {
                tracing::warn!(
                    connection_id = %payload.connection_id,
                    source_plugin_id = %src,
                    current_plugin_id = %current,
                    "Ignore stale update-connection-account from non-current plugin"
                );
                return;
            }
        }
    }

    actor
        .connection_manager
        .update_account(&payload.connection_id, payload.account);

    // UIへイベント通知
    if let Some(callback) = &actor.event_callback {
        callback(message.clone());
    }
}
