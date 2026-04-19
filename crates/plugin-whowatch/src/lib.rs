//! ふわっち (WhoWatch) プラグイン for MultiCommentViewer
//!
//! ## モジュール構成
//! - `connection`        : 接続ライフサイクル管理
//! - `ws_session`        : Phoenix WebSocket セッション（コメント受信）
//! - `metadata_polling`  : ユーザープロフィールポーリング（タイトル・視聴者数）
//! - `message_handler`   : Core からのメッセージ処理

mod connection;
mod message_handler;
mod metadata_polling;
mod ws_session;

use std::{collections::HashMap, sync::Arc};

use connection::Connection;
use mcv_common::SiteId;
use mcv_messages::{
    AddSitePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload, PluginId,
};
use message_handler::on_message_impl;
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

#[derive(Default)]
struct WhoWatchPlugin {
    logical_plugin_id: PluginId,
    is_initialized: bool,
    connections: HashMap<Uuid, Connection>,
}

impl WhoWatchPlugin {
    pub fn initialize(&mut self, logical_plugin_id: PluginId) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }

    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                error = %e,
                "Failed to send notification"
            );
        }
    }

    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: PluginId,
    ) {
        let message = McvMessage::new_notification(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        Self::send_message(ctx, message).await;
    }

    async fn send_add_site(
        &self,
        ctx: PluginContext,
        payload: AddSitePayload,
        plugin_id: PluginId,
    ) {
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
impl PluginImplV3Async for WhoWatchPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        let logical_plugin_id = PluginId::new(format!("WhoWatch_logical_{}", uuid));
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone()));

        #[cfg(feature = "alpha")]
        let log_level = "trace";
        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        let log_level = "info";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
        let log_level = "error";
        #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
        let log_level = "trace";

        let result_init_tracing =
            mcv_plugin_telemetry::init_tracing(uuid, adapter, env!("CARGO_PKG_VERSION"), log_level);
        match result_init_tracing {
            Ok(_) => {
                tracing::trace!(
                    target: "mcv::plugin-whowatch::WhoWatchPlugin",
                    "init_tracing() success"
                );
            }
            Err(_e) => {}
        }

        self.initialize(logical_plugin_id);

        // plugin-hello 送信
        let hello_payload = PluginHelloPayload {
            name: "WhoWatch".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec!["whowatch".to_string(), "comment-provider".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        self.send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id.clone())
            .await;

        let add_site = AddSitePayload {
            site_id: SiteId::new("WhoWatch", "b4e7a2c9-1f35-4d8e-a601-9c3b7f52d084"),
            display_name: "ふわっち".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        self.send_add_site(ctx.clone(), add_site, self.logical_plugin_id.clone())
            .await;
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let message: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                let error_text = e.to_string();
                if error_text.contains("unknown variant") {
                    tracing::warn!(
                        target: "mcv::plugin-whowatch",
                        error = %e,
                        raw = ?msg,
                        "未知のメッセージ種別を受信したため処理をスキップ"
                    );
                } else {
                    tracing::error!(
                        target: "mcv::plugin-whowatch",
                        error = %e,
                        raw = ?msg,
                        "受信したメッセージが復元できない"
                    );
                }
                return;
            }
        };
        if let Err(e) = on_message_impl(&mut self, ctx.clone(), message).await {
            let log_msg = e.to_log_message(&self.logical_plugin_id, env!("CARGO_PKG_VERSION"));
            ctx.send_notification(log_msg).await.ok();
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {}
}

export_plugin_v3_async!(WhoWatchPlugin);
