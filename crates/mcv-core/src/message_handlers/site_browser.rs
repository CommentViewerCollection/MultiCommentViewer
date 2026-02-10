use actix::Context;
use mcv_common::LogicalPluginId;
use mcv_messages::{
    AddBrowserPayload, AddSitePayload, DiscardConnectionSitePayload, Message as McvMessage,
    MessageDestination, MessageSource, MessageType, SetConnectionSitePayload,
    UpdateConnectionSettingsPayload,
};

use crate::core_actor::CoreActor;
use crate::plugin_host_actor::SendMessageToPlugin;
use crate::site_browser_manager::{BrowserInfo, SiteInfo};

/// add-site メッセージのハンドラー
pub fn handle_add_site(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: AddSitePayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse AddSitePayload");
            return;
        }
    };

    let plugin_id = match &message.src {
        MessageSource::Plugin { plugin_id } => *plugin_id,
        _ => {
            tracing::error!(target: "mcv::core::CoreActor","AddSite must come from a plugin");
            return;
        }
    };

    let site_info = SiteInfo {
        site_id: payload.site_id,
        site_name: payload.site_name.clone(),
        display_name: payload.display_name.clone(),
        plugin_id,
        options_schema: payload.options_schema,
    };

    tracing::info!(
        target: "mcv::core::CoreActor",
        site_id = %payload.site_id,
        site_name = %payload.site_name,
        plugin_id_from_message_src = %plugin_id,
        "Registering site (plugin_id is from message.src)"
    );

    let site_info_clone = site_info.clone();
    actor.site_browser_manager.add_site(site_info_clone);

    // UIにイベント通知
    if let Some(callback) = &actor.event_callback {
        let event = McvMessage::new_notification(
            MessageType::AddSite,
            MessageSource::Core,
            MessageDestination::Core,
            serde_json::to_value(&site_info).unwrap(),
        );
        callback(event);
    }

    tracing::info!(
        target: "mcv::core::CoreActor",
        site_name = %payload.site_name,
        plugin_id = %plugin_id,
        "Site registered"
    );
}

/// add-browser メッセージのハンドラー
pub fn handle_add_browser(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: AddBrowserPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse AddBrowserPayload");
            return;
        }
    };

    let plugin_id = match &message.src {
        MessageSource::Plugin { plugin_id } => *plugin_id,
        _ => {
            tracing::error!(target: "mcv::core::CoreActor","AddBrowser must come from a plugin");
            return;
        }
    };

    let browser_info = BrowserInfo {
        browser_id: payload.browser_id,
        browser_name: payload.browser_name.clone(),
        display_name: payload.display_name.clone(),
        plugin_id,
    };

    let browser_info_clone = browser_info.clone();
    actor.site_browser_manager.add_browser(browser_info_clone);

    // UIにイベント通知
    if let Some(callback) = &actor.event_callback {
        let event = McvMessage::new_notification(
            MessageType::AddBrowser,
            MessageSource::Core,
            MessageDestination::Core,
            serde_json::to_value(&browser_info).unwrap(),
        );
        callback(event);
    }

    tracing::info!(
        target: "mcv::core::CoreActor",
        browser_name = %payload.browser_name,
        plugin_id = %plugin_id,
        "Browser registered"
    );
}

/// set-connection-site メッセージのハンドラー（UIから呼ばれる）
pub fn handle_set_connection_site(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: SetConnectionSitePayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse SetConnectionSitePayload");
            return;
        }
    };

    let connection_id = payload.connection_id;
    let site_id = payload.site_id;

    tracing::debug!(
        target: "mcv::core::CoreActor",
        connection_id = %connection_id,
        site_id = %site_id,
        "Setting connection site"
    );

    // 前のサイトを取得してDiscardConnectionSiteを送信
    let plugins = actor.logical_plugins.clone();

    // 前のplugin_idを取得
    let old_plugin_id = actor
        .connection_manager
        .get_connection(&connection_id)
        .and_then(|c| c.plugin_id);

    // 新しいサイト情報を取得
    if let Some(site_info) = actor.site_browser_manager.get_site(&site_id) {
        actor.connection_manager.set_site(
            &connection_id,
            site_id,
            site_info.display_name.clone(),
            site_info.plugin_id,
        );

        // 前のプラグインにDiscardConnectionSiteを送信
        if let Some(old_pid) = old_plugin_id {
            if old_pid != site_info.plugin_id {
                let old_logical_plugin_id = LogicalPluginId::from_uuid(old_pid);
                if let Some(old_plugin) = plugins.get(&old_logical_plugin_id) {
                    let discard_msg = McvMessage::new(
                        MessageType::DiscardConnectionSite,
                        MessageSource::Core,
                        MessageDestination::Plugin { plugin_id: old_pid },
                        serde_json::to_value(DiscardConnectionSitePayload {
                            connection_id,
                            site_id,
                        })
                        .unwrap(),
                    );
                    old_plugin.host_addr.do_send(SendMessageToPlugin {
                        message: discard_msg,
                    });
                }
            }
        }

        // 新しいプラグインにSetConnectionSiteを送信
        let new_logical_plugin_id = LogicalPluginId::from_uuid(site_info.plugin_id);
        if let Some(new_plugin) = plugins.get(&new_logical_plugin_id) {
            let set_msg = McvMessage::new(
                MessageType::SetConnectionSite,
                MessageSource::Core,
                MessageDestination::Plugin {
                    plugin_id: site_info.plugin_id,
                },
                serde_json::to_value(SetConnectionSitePayload {
                    connection_id,
                    site_id,
                })
                .unwrap(),
            );
            new_plugin
                .host_addr
                .do_send(SendMessageToPlugin { message: set_msg });
        }
    }
}

/// update-connection-settings メッセージのハンドラー
pub fn handle_update_connection_settings(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: UpdateConnectionSettingsPayload =
        match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse UpdateConnectionSettingsPayload");
                return;
            }
        };

    tracing::debug!(
        target: "mcv::core::CoreActor",
        connection_id = %payload.connection_id,
        has_url = payload.url.is_some(),
        has_browser = payload.browser_id.is_some(),
        has_settings = payload.advanced_settings.is_some(),
        "Updating connection settings"
    );

    if let Some(url) = payload.url {
        actor
            .connection_manager
            .update_url(&payload.connection_id, Some(url));
    }

    if let Some(browser_id) = payload.browser_id {
        let browser_name = actor
            .site_browser_manager
            .get_browser(&browser_id)
            .map(|b| b.display_name.clone());
        actor.connection_manager.update_browser(
            &payload.connection_id,
            Some(browser_id),
            browser_name,
        );
    }

    if let Some(settings) = payload.advanced_settings {
        actor
            .connection_manager
            .update_advanced_settings(&payload.connection_id, Some(settings));
    }
}
