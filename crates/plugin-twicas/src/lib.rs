//! TwitCasting plugin for MultiCommentViewer
//!
//! このプラグインはTwitCastingのコメントを取得します。

mod connection;
mod message_handler;

use std::{collections::HashMap, sync::Arc};

use connection::Connection;
use mcv_common::SiteId;
use mcv_messages::{
    AddSitePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload,
};
use message_handler::on_message_impl;
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

#[derive(Default)]
struct TwicasPlugin {
    logical_plugin_id: Uuid,
    is_initialized: bool,
    connections: HashMap<Uuid, Connection>,
}

impl TwicasPlugin {
    pub fn initialize(&mut self, logical_plugin_id: Uuid) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }

    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::warn!(
                target: "mcv::plugin-twicas",
                error = %e,
                "Failed to send notification"
            );
        }
    }

    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: Uuid,
    ) {
        let message = McvMessage::new_notification(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        Self::send_message(ctx, message).await;
    }

    async fn send_add_site(&self, ctx: PluginContext, payload: AddSitePayload, plugin_id: Uuid) {
        let message = McvMessage::new_notification(
            MessageType::AddSite,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        Self::send_message(ctx, message).await;
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for TwicasPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let logical_plugin_id = Uuid::new_v4();
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone()));
        #[cfg(feature = "alpha")]
        let log_level = "trace";
        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        let log_level = "info";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
        let log_level = "error";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
        let log_level = "trace";
        let result_init_tracing = mcv_plugin_telemetry::init_tracing(
            logical_plugin_id,
            adapter,
            env!("CARGO_PKG_VERSION"),
            log_level,
        );
        if result_init_tracing.is_ok() {
            tracing::trace!(target:"mcv::plugin-twicas::TwicasPlugin", "init_tracing() success");
        }

        self.initialize(logical_plugin_id);

        let hello_payload = PluginHelloPayload {
            name: "ツイキャス".to_string(),
            plugin_id: self.logical_plugin_id,
            role: vec!["ツイキャス".to_string(), "comment-provider".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: Some(serde_json::json!({
                "type": "object",
                "properties": {
                    "text": { "type": "string", "title": "コメント" },
                    "anonymous": { "type": "boolean", "title": "匿名", "default": false }
                },
                "required": ["text"]
            })),
        };
        self.send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id)
            .await;

        let add_site = AddSitePayload {
            site_id: SiteId::new("ツイキャス", "8cb52621-4f4f-4337-b6a7-8b4a436d91d7"),
            display_name: "ツイキャス".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        self.send_add_site(ctx.clone(), add_site, self.logical_plugin_id)
            .await;

        let add_site_private = AddSitePayload {
            site_id: SiteId::new(
                "ツイキャス（プライベート）",
                "6f3a9f72-9b0c-4e2f-8a1d-5c7e3b4d2f9a",
            ),
            display_name: "ツイキャス（プライベート）".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        self.send_add_site(ctx, add_site_private, self.logical_plugin_id)
            .await;
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let message: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                let error_text = e.to_string();
                if error_text.contains("unknown variant") {
                    tracing::warn!(
                        target: "mcv::plugin-twicas",
                        error = %e,
                        raw = ?msg,
                        "未知のメッセージ種別を受信したため処理をスキップ"
                    );
                } else {
                    tracing::error!(
                        target: "mcv::plugin-twicas",
                        error = %e,
                        raw = ?msg,
                        "受信したメッセージが復元できない"
                    );
                }
                return;
            }
        };

        if let Err(e) = on_message_impl(self, ctx, message).await {
            tracing::error!(
                target: "mcv::plugin-twicas",
                error = %e,
                "on_message_impl failed"
            );
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {}
}

export_plugin_v3_async!(TwicasPlugin);
