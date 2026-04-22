mod connection;
mod message_handler;
mod profiles;

use connection::Connection;
use std::{collections::HashMap, sync::Arc};

use mcv_common::SiteId;
use mcv_messages::{
    AddSitePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload, PluginId,
};
use message_handler::on_message_impl;
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

#[derive(Default)]
struct IrcPlugin {
    logical_plugin_id: PluginId,
    is_initialized: bool,
    connections: HashMap<Uuid, Connection>,
}

impl IrcPlugin {
    fn initialize(&mut self, id: PluginId) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = id;
    }

    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::warn!(
                target: "mcv::plugin-irc",
                error = %e,
                "Failed to send notification"
            );
        }
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for IrcPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        let plugin_id = PluginId::new(format!("IRC_logical_{uuid}"));
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone()));

        #[cfg(feature = "alpha")]
        let log_level = "trace";
        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        let log_level = "info";
        #[cfg(all(feature = "stable", not(feature = "alpha"), not(feature = "beta")))]
        let log_level = "error";
        #[cfg(not(any(feature = "alpha", feature = "beta", feature = "stable")))]
        let log_level = "trace";

        match mcv_plugin_telemetry::init_tracing(
            uuid,
            adapter,
            env!("CARGO_PKG_VERSION"),
            log_level,
        ) {
            Ok(_) => tracing::trace!(target: "mcv::plugin-irc", "init_tracing 成功"),
            Err(e) => eprintln!("[plugin-irc] init_tracing 失敗: {e}"),
        }

        self.initialize(plugin_id);

        // plugin-hello
        let hello = PluginHelloPayload {
            name: "IRC".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec!["irc".to_string(), "comment-provider".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        let hello_msg = McvMessage::new_notification(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(&hello).unwrap(),
        );
        Self::send_message(ctx.clone(), hello_msg).await;

        // add-site
        let add_site = AddSitePayload {
            site_id: SiteId::new("IRC", "b1c2d3e4-1234-5678-abcd-000000000001"),
            display_name: "IRC".to_string(),
            options_schema: serde_json::json!({}),
        };
        let site_msg = McvMessage::new_notification(
            MessageType::AddSite,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(&add_site).unwrap(),
        );
        Self::send_message(ctx, site_msg).await;
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let message: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                let s = e.to_string();
                if s.contains("unknown variant") {
                    tracing::warn!(
                        target: "mcv::plugin-irc",
                        error = %e,
                        "未知メッセージ種別をスキップ"
                    );
                } else {
                    tracing::error!(
                        target: "mcv::plugin-irc",
                        error = %e,
                        raw = ?msg,
                        "メッセージのデシリアライズ失敗"
                    );
                }
                return;
            }
        };
        if let Err(e) = on_message_impl(self, ctx.clone(), message).await {
            let log_msg = e.to_log_message(&self.logical_plugin_id, env!("CARGO_PKG_VERSION"));
            ctx.send_notification(log_msg).await.ok();
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        for conn in self.connections.values_mut() {
            conn.stop();
        }
    }
}

export_plugin_v3_async!(IrcPlugin);
