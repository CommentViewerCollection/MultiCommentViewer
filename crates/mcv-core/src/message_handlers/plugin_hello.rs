use actix::Context;
use mcv_common::{LogicalPluginId, PhysicalPluginId};
use mcv_messages::{
    ConnectionAddedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginAddedPayload, PluginHelloPayload,
};

use crate::core_actor::{CoreActor, LogicalPluginInfo};
use crate::plugin_host_actor::SendMessageToPlugin;

/// plugin-hello メッセージのハンドラー
///
/// 物理プラグインから論理プラグインの登録を受け付ける
pub fn handle_plugin_hello(
    actor: &mut CoreActor,
    physical_plugin_id: PhysicalPluginId,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) -> Result<(), String> {
    let payload: PluginHelloPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            return Err(format!("Failed to parse plugin-hello payload: {}", e));
        }
    };

    // payload.plugin_id（Uuid）をLogicalPluginIdに変換
    let logical_plugin_id = LogicalPluginId::from_uuid(payload.plugin_id);

    // 既に論理プラグインとして登録済みか確認
    if actor.logical_plugins.contains_key(&logical_plugin_id) {
        tracing::debug!(
            target: "mcv::core::CoreActor",
            logical_plugin_id = %logical_plugin_id,
            "Logical plugin already registered, broadcasting plugin-added to all"
        );

        // 既に登録済みの場合でも、plugin-addedをブロードキャスト
        // ユニキャストは不要（送信元も含めて全員がブロードキャストで受信する）
        if let Some(plugin_info) = actor.logical_plugins.get(&logical_plugin_id) {
            let response = McvMessage::new_notification(
                MessageType::PluginAdded,
                MessageSource::Core,
                MessageDestination::Broadcast,
                serde_json::to_value(PluginAddedPayload {
                    name: plugin_info.name.clone(),
                    plugin_id: logical_plugin_id.inner(),
                    role: plugin_info.role.clone(),
                    api_version: plugin_info.api_version.clone(),
                })
                .unwrap(),
            );

            // 全論理プラグインにブロードキャスト
            broadcast_to_all_logical_plugins(actor, response);
        }
        return Ok(());
    }

    // 物理プラグインのPluginHostActorを取得
    let physical_plugin_host_addr =
        if let Some(addr) = actor.physical_plugin_hosts.get(&physical_plugin_id) {
            // DLL物理プラグインの場合
            tracing::trace!(
                target: "mcv::core::CoreActor",
                physical_plugin_id = %physical_plugin_id,
                logical_plugin_id = %logical_plugin_id,
                "Found physical plugin host for DLL logical plugin"
            );
            addr.clone()
        } else {
            return Err(format!(
                "Physical plugin host not found: {}",
                physical_plugin_id
            ));
        };

    // LogicalPluginInfoを作成
    let logical_plugin_info = LogicalPluginInfo {
        logical_plugin_id,
        physical_plugin_id,
        name: payload.name.clone(),
        role: payload.role.clone(),
        api_version: payload.api_version.clone(),
        host_addr: physical_plugin_host_addr,
        settings_schema: None,
        settings_data: None,
    };

    // 論理プラグインとして登録
    actor
        .logical_plugins
        .insert(logical_plugin_id, logical_plugin_info);

    tracing::info!(
        target: "mcv::core::CoreActor",
        physical_plugin_id = %physical_plugin_id,
        logical_plugin_id = %logical_plugin_id,
        logical_plugin_name = %payload.name,
        "Logical plugin registered (physical_plugin_id → logical_plugin_id mapping created)"
    );

    // plugin-addedを全論理プラグインにブロードキャスト
    let response = McvMessage::new_notification(
        MessageType::PluginAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::to_value(PluginAddedPayload {
            name: payload.name.clone(),
            plugin_id: logical_plugin_id.inner(),
            role: payload.role.clone(),
            api_version: payload.api_version.clone(),
        })
        .unwrap(),
    );

    // 全論理プラグインにブロードキャスト
    broadcast_to_all_logical_plugins(actor, response);

    tracing::info!(
        target: "mcv::core::CoreActor",
        logical_plugin_id = %logical_plugin_id,
        logical_plugins_count = actor.logical_plugins.len(),
        "Logical plugin registered and plugin-added broadcasted to all logical plugins"
    );

    // そのプラグインに関連する全接続のConnectionAddedをユニキャスト
    send_connection_added_for_plugin(actor, logical_plugin_id);
    Ok(())
}

/// プラグインに関連する全接続のConnectionAddedをユニキャスト
fn send_connection_added_for_plugin(actor: &CoreActor, logical_plugin_id: LogicalPluginId) {
    let connections = actor.connection_manager.get_connections();
    let plugin_uuid = logical_plugin_id.inner();

    // このプラグインに関連する接続をフィルター
    let related_connections: Vec<_> = connections
        .iter()
        .filter(|conn| conn.plugin_id == Some(plugin_uuid))
        .collect();

    if related_connections.is_empty() {
        tracing::debug!(
            target: "mcv::core::CoreActor",
            logical_plugin_id = %logical_plugin_id,
            "No connections related to this plugin"
        );
        return;
    }

    // プラグイン情報を取得
    let plugin_info = match actor.logical_plugins.get(&logical_plugin_id) {
        Some(info) => info,
        None => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                logical_plugin_id = %logical_plugin_id,
                "Plugin info not found"
            );
            return;
        }
    };

    let connection_count = related_connections.len();

    // 各接続に対してConnectionAddedをユニキャスト
    for conn in related_connections {
        let connection_added_msg = McvMessage::new_notification(
            MessageType::ConnectionAdded,
            MessageSource::Core,
            MessageDestination::Plugin {
                plugin_id: plugin_uuid,
            },
            serde_json::to_value(ConnectionAddedPayload {
                connection_id: conn.connection_id,
                name: conn.name.clone(),
            })
            .unwrap(),
        );

        plugin_info.host_addr.do_send(SendMessageToPlugin {
            message: connection_added_msg,
        });

        tracing::debug!(
            target: "mcv::core::CoreActor",
            connection_id = %conn.connection_id,
            connection_name = %conn.name,
            logical_plugin_id = %logical_plugin_id,
            "Sent ConnectionAdded to plugin"
        );
    }

    tracing::info!(
        target: "mcv::core::CoreActor",
        logical_plugin_id = %logical_plugin_id,
        connection_count = connection_count,
        "Sent all related ConnectionAdded messages to plugin"
    );
}

/// get-plugins メッセージのハンドラー
///
/// リクエスト元のプラグインに全論理プラグインの情報を送信
pub fn handle_get_plugins(
    actor: &mut CoreActor,
    physical_plugin_id: PhysicalPluginId,
    _message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    // リクエスト元の論理プラグインを探す
    // （物理plugin_idから論理plugin_idを特定）
    let requester_logical_plugin_info = actor
        .logical_plugins
        .values()
        .find(|logical_plugin_info| logical_plugin_info.physical_plugin_id == physical_plugin_id);

    let requester_logical_plugin_info = match requester_logical_plugin_info {
        Some(info) => info,
        None => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                physical_plugin_id = %physical_plugin_id,
                "Logical plugin not found for get-plugins request"
            );
            return;
        }
    };

    tracing::info!(
        target: "mcv::core::CoreActor",
        physical_plugin_id = %physical_plugin_id,
        logical_plugin_id = %requester_logical_plugin_info.logical_plugin_id,
        logical_plugins_count = actor.logical_plugins.len(),
        "Processing get-plugins request from logical plugin"
    );

    // 全論理プラグインの情報をplugin-addedメッセージとして送信
    for (logical_plugin_id, logical_plugin_info) in &actor.logical_plugins {
        let plugin_added_message = McvMessage::new_notification(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Plugin {
                plugin_id: requester_logical_plugin_info.logical_plugin_id.inner(),
            },
            serde_json::to_value(PluginAddedPayload {
                name: logical_plugin_info.name.clone(),
                plugin_id: logical_plugin_id.inner(),
                role: logical_plugin_info.role.clone(),
                api_version: logical_plugin_info.api_version.clone(),
            })
            .unwrap(),
        );

        // リクエスト元の物理プラグインのPluginHostActorに送信
        requester_logical_plugin_info
            .host_addr
            .do_send(SendMessageToPlugin {
                message: plugin_added_message,
            });

        tracing::debug!(
            target: "mcv::core::CoreActor",
            logical_plugin_id = %logical_plugin_id,
            logical_plugin_name = %logical_plugin_info.name,
            "Sent plugin-added for logical plugin in response to get-plugins"
        );
    }

    tracing::info!(
        target: "mcv::core::CoreActor",
        requester_logical_plugin_id = %requester_logical_plugin_info.logical_plugin_id,
        "get-plugins request completed, sent all logical plugin info"
    );
}

/// 全論理プラグインにメッセージをブロードキャスト
///
/// 各論理プラグインに送信する際、dstをBroadcastから個別のPlugin{plugin_id}に変更します。
/// これにより、EXE Plugin Managerなど同じhost_addrを共有するプラグインが
/// 自分宛てのメッセージのみを処理できるようになります。
pub fn broadcast_to_all_logical_plugins(actor: &CoreActor, message: McvMessage) {
    for (logical_plugin_id, logical_plugin_info) in &actor.logical_plugins {
        // dstを個別のプラグインIDに変更
        let mut personalized_message = message.clone();
        personalized_message.dst = MessageDestination::Plugin {
            plugin_id: logical_plugin_id.inner(),
        };

        logical_plugin_info.host_addr.do_send(SendMessageToPlugin {
            message: personalized_message,
        });
    }
}
