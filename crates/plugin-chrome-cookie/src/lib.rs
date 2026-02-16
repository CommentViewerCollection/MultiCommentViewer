//! plugin-chrome-cookie
//!
//! Chrome のプロファイルを検出し、AddBrowser メッセージを送信するプラグイン。
//! 各 Chrome プロファイルに対して 1 つの AddBrowser を送信する。

mod profiles;

use std::sync::Arc;
use std::time::Duration;

use mcv_messages::{
    AddBrowserAckPayload, AddBrowserPayload, Message as McvMessage, MessageDestination,
    MessageSource, MessageType, PluginHelloAckPayload, PluginHelloPayload,
};
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

#[derive(Default)]
struct ChromeCookiePlugin {
    logical_plugin_id: Uuid,
    is_initialized: bool,
}

impl ChromeCookiePlugin {
    fn initialize(&mut self, logical_plugin_id: Uuid) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }

    async fn send_message(ctx: PluginContext, message: McvMessage) {
        if let Err(e) = ctx.send_notification(message).await {
            tracing::error!(
                target: "mcv::plugin-chrome-cookie",
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
            MessageSource::Plugin { plugin_id },
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

    async fn send_add_browser(
        &self,
        ctx: PluginContext,
        payload: AddBrowserPayload,
        plugin_id: Uuid,
    ) -> Result<AddBrowserAckPayload, RequestError> {
        let message = McvMessage::new_request(
            MessageType::AddBrowser,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let response = ctx.send_request(message, Duration::from_secs(10)).await?;
        if response.message_type != MessageType::AddBrowserAck {
            return Err(RequestError::Internal(format!(
                "unexpected response for AddBrowser: {:?}",
                response.message_type
            )));
        }
        serde_json::from_value(response.payload)
            .map_err(|e| RequestError::Internal(format!("invalid AddBrowserAck payload: {}", e)))
    }
}

#[async_trait]
impl PluginImplV3Async for ChromeCookiePlugin {
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
                tracing::trace!(
                    target: "mcv::plugin-chrome-cookie",
                    "init_tracing() success"
                );
            }
            Err(_e) => {}
        }

        self.initialize(logical_plugin_id);

        // plugin-hello 送信
        let hello_payload = PluginHelloPayload {
            name: "Chrome Cookie".to_string(),
            plugin_id: self.logical_plugin_id,
            role: vec!["browser-cookie".to_string()],
            api_version: "v3".to_string(),
        };
        if let Err(e) = self
            .send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id)
            .await
        {
            tracing::error!(
                target: "mcv::plugin-chrome-cookie",
                error = %e,
                "PluginHello request failed"
            );
            return;
        }

        // Chrome プロファイルを検出して AddBrowser を送信
        let profiles = profiles::get_chrome_profiles();
        tracing::info!(
            target: "mcv::plugin-chrome-cookie",
            count = profiles.len(),
            "Detected Chrome profiles"
        );

        for profile in profiles {
            let display_name = format!("Chrome({})", profile.display_name);
            let browser_id = profile.browser_id;
            tracing::info!(
                target: "mcv::plugin-chrome-cookie",
                browser_id = %browser_id,
                display_name = %display_name,
                "Sending AddBrowser for Chrome profile"
            );
            let add_browser = AddBrowserPayload {
                browser_id: browser_id.clone(),
                browser_name: "Chrome".to_string(),
                display_name,
            };
            if let Err(e) = self
                .send_add_browser(ctx.clone(), add_browser, self.logical_plugin_id)
                .await
            {
                tracing::error!(
                    target: "mcv::plugin-chrome-cookie",
                    error = %e,
                    browser_id = %browser_id,
                    "AddBrowser request failed"
                );
            }
        }
    }

    async fn on_message(&mut self, _ctx: PluginContext, msg: &[u8]) {
        match serde_json::from_slice::<serde_json::Value>(msg) {
            Ok(message) => {
                let msg_type = message["type"].as_str().unwrap_or("unknown");
                tracing::debug!(
                    target: "mcv::plugin-chrome-cookie",
                    msg_type = %msg_type,
                    "Received message"
                );
            }
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-chrome-cookie",
                    error = %e,
                    "Failed to parse message"
                );
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!(
            target: "mcv::plugin-chrome-cookie",
            "ChromeCookiePlugin shutting down"
        );
    }
}

export_plugin_v3_async!(ChromeCookiePlugin);
