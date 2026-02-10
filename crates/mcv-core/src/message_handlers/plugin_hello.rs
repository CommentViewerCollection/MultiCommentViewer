use actix::Context;
use mcv_common::{LogicalPluginId, PhysicalPluginId};
use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginAddedPayload,
    PluginHelloPayload,
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
) {
    let payload: PluginHelloPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                physical_plugin_id = %physical_plugin_id,
                mcv_message = ?message,
                "Failed to parse plugin-hello payload"
            );
            return;
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
            let response = McvMessage::new(
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
        return;
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
            tracing::error!(
                target: "mcv::core::CoreActor",
                physical_plugin_id = %physical_plugin_id,
                "Physical plugin host not found"
            );
            return;
        };

    // LogicalPluginInfoを作成
    let logical_plugin_info = LogicalPluginInfo {
        logical_plugin_id,
        physical_plugin_id,
        name: payload.name.clone(),
        role: payload.role.clone(),
        api_version: payload.api_version.clone(),
        host_addr: physical_plugin_host_addr,
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
    let response = McvMessage::new(
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
        let plugin_added_message = McvMessage::new(
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
