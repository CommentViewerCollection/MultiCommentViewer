use actix::Context;
use mcv_common::LogicalPluginId;
use mcv_messages::{
    ConnectPayload, ConnectedPayload, ConnectionAddedPayload, DisconnectPayload,
    DisconnectedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
};
use uuid::Uuid;

use crate::connection_manager::ConnectionStatus;
use crate::core_actor::CoreActor;
use crate::message_handlers;
use crate::plugin_host_actor::SendMessageToPlugin;

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
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    tracing::trace!(
        target: "mcv::core::CoreActor",
        "handle_add_connection()"
    );

    let connection_id = Uuid::new_v4();

    // Connection Managerに登録
    let response_message = message.clone();
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

    // connection-addedを返信
    let response = McvMessage::create_response(
        &response_message,
        MessageType::ConnectionAdded,
        serde_json::to_value(ConnectionAddedPayload { connection_id }).unwrap(),
    );

    // プラグインへ返信
    if let MessageSource::Plugin { plugin_id } = message.src {
        let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id);
        if let Some(plugin_info) = actor.logical_plugins.get(&logical_plugin_id) {
            plugin_info
                .host_addr
                .do_send(SendMessageToPlugin { message: response });
        }
    }
    // 全論理プラグインにConnectionAddedをブロードキャスト
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::to_value(ConnectionAddedPayload { connection_id }).unwrap(),
    );
    message_handlers::plugin_hello::broadcast_to_all_logical_plugins(actor, broadcast_msg);
    tracing::debug!(
        target: "mcv::core::CoreActor",
        "Broadcasted connection-added to all plugins"
    );
}

/// connect メッセージのハンドラー
pub fn handle_connect(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
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

    let connection_id = payload.connection_id;

    // Connection Managerから接続情報を取得してplugin_idを取得
    let plugins = actor.logical_plugins.clone();
    let msg = message.clone();

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
