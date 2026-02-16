//! YouTube Live plugin for MultiCommentViewer
//!
//! このプラグインはYouTube Liveのライブチャットからコメントを取得します。
//!
//! ## モジュール構成
//! - `adapter`: PluginContext→PluginHostアダプタ
//! - `connection`: 接続管理とチャット取得ロジック
//! - `message_handler`: メッセージハンドリングとペイロード解析
//! - `video_id`: YouTube動画ID抽出ユーティリティ

mod connection;
mod message_handler;
mod video_id;

use std::{collections::HashMap, sync::Arc, time::Duration};

use connection::Connection;
use message_handler::on_message_impl;
use mcv_common::SiteId;
use mcv_messages::{
    AddSiteAckPayload, AddSitePayload, Message as McvMessage,
    MessageDestination, MessageSource, MessageType, PluginHelloAckPayload, PluginHelloPayload,
};
use plugin_abi_helper::v3::prelude::*;

use uuid::Uuid;

#[derive(Default)]
struct YouTubeLivePlugin {
    logical_plugin_id: Uuid, //現在の実装ではplugin-helloは1回しか送らないから1つで良い。
    is_initialized: bool,
    connections: HashMap<Uuid, Connection>,
}
impl YouTubeLivePlugin {
    pub fn initialize(&mut self, logical_plugin_id: Uuid) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }
    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "send_notification failed"
            );
        }
    }
    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: Uuid,
    ) -> Result<PluginHelloAckPayload, RequestError> {
        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let response = ctx.send_request(message, Duration::from_secs(10)).await?;
        if response.message_type != MessageType::PluginHelloAck {
            return Err(RequestError::Internal(format!(
                "unexpected response for PluginHello: {:?}",
                response.message_type
            )));
        }
        serde_json::from_value(response.payload)
            .map_err(|e| RequestError::Internal(format!("invalid PluginHelloAck payload: {}", e)))
    }
    async fn send_add_site(
        &self,
        ctx: PluginContext,
        payload: AddSitePayload,
        plugin_id: Uuid,
    ) -> Result<AddSiteAckPayload, RequestError> {
        let message = McvMessage::new_request(
            MessageType::AddSite,
            MessageSource::Plugin {
                plugin_id: plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let response = ctx.send_request(message, Duration::from_secs(10)).await?;
        if response.message_type != MessageType::AddSiteAck {
            return Err(RequestError::Internal(format!(
                "unexpected response for AddSite: {:?}",
                response.message_type
            )));
        }
        serde_json::from_value(response.payload)
            .map_err(|e| RequestError::Internal(format!("invalid AddSiteAck payload: {}", e)))
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for YouTubeLivePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let logical_plugin_id = Uuid::new_v4();
        let adapter = Arc::new(PluginContextAdapter::new(ctx.clone()));
        let result_init_tracing = mcv_plugin_telemetry::init_tracing(
            logical_plugin_id,
            adapter,
            env!("CARGO_PKG_VERSION"),
            "info",
        );
        match result_init_tracing {
            Ok(_) => {
                tracing::trace!(target:"mcv::plugin-youtube-live::YouTubeLivePlugin", "init_tracing() success");
            }
            Err(_e) => {}
        }
        self.initialize(logical_plugin_id);
        // plugin-hello送信
        let hello_payload = PluginHelloPayload {
            name: "YouTubeLive".to_string(),
            plugin_id: self.logical_plugin_id,
            role: vec!["youtubelive".to_string()],
            api_version: "v3".to_string(),
        };
        if let Err(e) = self
            .send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id)
            .await
        {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "PluginHello request failed"
            );
            return;
        }

        let add_site = AddSitePayload {
            site_id: SiteId::new("YouTubeLive", "7a3b5c9d-1e2f-4a5b-8c7d-9e0f1a2b3c4d"),
            display_name: "YouTube Live".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        if let Err(e) = self
            .send_add_site(ctx.clone(), add_site, self.logical_plugin_id)
            .await
        {
            tracing::error!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "AddSite request failed"
            );
            return;
        }
    }
    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let message: McvMessage = match serde_json::from_slice(&msg) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-youtube-live",
                    error=%e,raw=msg,"受信したメッセージが復元できない"
                );
                return;
            }
        };
        if let Err(_e) = on_message_impl(&mut self, ctx.clone(), message).await {

        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {}
}

export_plugin_v3_async!(YouTubeLivePlugin);
