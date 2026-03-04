use actix::Context;
use mcv_messages::{
    AddBrowserPayload, AddSitePayload, ConnectionAddedPayload, Message as McvMessage,
    MessageDestination, MessageSource, MessageType, RemoveBrowserPayload, SetConnectionSitePayload,
    UpdateConnectionSettingsPayload,
};

use crate::core_actor::CoreActor;
use crate::site_browser_manager::{BrowserInfo, SiteInfo};

/// add-site メッセージのハンドラー
pub fn handle_add_site(actor: &mut CoreActor, message: &McvMessage, _ctx: &mut Context<CoreActor>) {
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
        site_id: payload.site_id.clone(),
        display_name: payload.display_name.clone(),
        plugin_id,
        options_schema: payload.options_schema,
    };

    tracing::info!(
        target: "mcv::core::CoreActor",
        site_id = %payload.site_id,
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
        site_id = %payload.site_id,
        plugin_id = %plugin_id,
        "Site registered"
    );

    // サイト登録完了後、Pending接続を確認して有効化
    let activated_connections = actor
        .connection_manager
        .activate_pending_connections_by_site(&site_info.site_id, plugin_id);

    if !activated_connections.is_empty() {
        tracing::info!(
            target: "mcv::core::CoreActor",
            site_id = %site_info.site_id,
            activated_count = activated_connections.len(),
            "Activated pending connections after add-site"
        );

        // 有効化された接続に対してSetConnectionSiteメッセージを送信
        for conn_id in &activated_connections {
            actor.send_set_connection_site(*conn_id, site_info.site_id.clone());
        }

        // 有効化された接続をUIに通知（connection-added再送信）
        if let Some(ref callback) = actor.event_callback {
            for conn_id in activated_connections {
                if let Some(conn) = actor.connection_manager.get_connection(&conn_id) {
                    let msg = McvMessage::new_notification(
                        MessageType::ConnectionAdded,
                        MessageSource::Core,
                        MessageDestination::Core,
                        serde_json::to_value(ConnectionAddedPayload {
                            connection_id: conn.connection_id,
                            name: conn.name.clone(),
                        })
                        .unwrap(),
                    );
                    callback(msg);
                }
            }
        }

        // 保存（有効化された接続のstatus変更を反映）
        let _ = actor.save_connections();
    }
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

/// remove-browser メッセージのハンドラー
pub fn handle_remove_browser(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: RemoveBrowserPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(target: "mcv::core::CoreActor", error = %e, "Failed to parse RemoveBrowserPayload");
            return;
        }
    };

    let removed = actor.site_browser_manager.remove_browser(&payload.browser_id);
    if !removed {
        return;
    }

    if let Some(callback) = &actor.event_callback {
        let event = McvMessage::new_notification(
            MessageType::RemoveBrowser,
            MessageSource::Core,
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        callback(event);
    }

    tracing::info!(
        target: "mcv::core::CoreActor",
        browser_id = %payload.browser_id,
        "Browser removed"
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

    // 共通メソッドを呼び出し
    actor.send_set_connection_site(payload.connection_id, payload.site_id);

    // 保存
    let _ = actor.save_connections();
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
        actor.connection_manager.update_input_state(
            &payload.connection_id,
            Some(serde_json::json!({ "url": url })),
        );
    }

    if let Some(browser_id) = payload.browser_id {
        actor
            .connection_manager
            .update_browser(&payload.connection_id, Some(browser_id));
    }

    if let Some(settings) = payload.advanced_settings {
        actor
            .connection_manager
            .update_advanced_settings(&payload.connection_id, Some(settings));
    }

    // 保存
    let _ = actor.save_connections();
}
