//! plugin-edge-cookie
//!
//! Microsoft Edge のプロファイルを検出し、AddBrowser メッセージを送信するプラグイン。
//! 各 Edge プロファイルに対して 1 つの AddBrowser を送信する。

mod profiles;

use std::path::PathBuf;
use std::sync::Arc;
use std::time::Duration;

use chromium_cookie_lib::{load_master_key, query_cookies, resolve_cookie_db_path};
use mcv_messages::{
    AddBrowserAckPayload, AddBrowserPayload, BrowserId, Cookie as McvCookie, GetCookieAckPayload,
    GetCookiePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloAckPayload, PluginHelloPayload, PluginId,
};
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

#[derive(Default)]
struct EdgeCookiePlugin {
    logical_plugin_id: PluginId,
    is_initialized: bool,
}

fn edge_user_data_dir() -> Option<PathBuf> {
    let local_app_data = std::env::var("LOCALAPPDATA").ok()?;
    Some(
        PathBuf::from(local_app_data)
            .join("Microsoft")
            .join("Edge")
            .join("User Data"),
    )
}

fn load_cookies(browser_id: &BrowserId, domain: &str) -> Vec<McvCookie> {
    let profile = match profiles::get_edge_profiles()
        .into_iter()
        .find(|p| p.browser_id == *browser_id)
    {
        Some(p) => p,
        None => return vec![],
    };

    let cookie_db_path = match resolve_cookie_db_path(&profile.profile_dir) {
        Some(path) => path,
        None => return vec![],
    };

    let master_key = edge_user_data_dir().and_then(|d| load_master_key(&d));
    query_cookies(&cookie_db_path, domain, master_key.as_deref())
}

impl EdgeCookiePlugin {
    fn initialize(&mut self, logical_plugin_id: PluginId) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = logical_plugin_id;
    }

    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: PluginId,
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
        plugin_id: PluginId,
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
impl PluginImplV3Async for EdgeCookiePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        let logical_plugin_id = PluginId::new(format!("EdgeCookiePlugin_logical_{}", uuid));
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
                    target: "mcv::plugin-edge-cookie",
                    "init_tracing() success"
                );
            }
            Err(_e) => {}
        }

        self.initialize(logical_plugin_id);

        // plugin-hello 送信
        let hello_payload = PluginHelloPayload {
            name: "Edge Cookie".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec!["browser-cookie".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        if let Err(e) = self
            .send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id.clone())
            .await
        {
            tracing::error!(
                target: "mcv::plugin-edge-cookie",
                error = %e,
                "PluginHello request failed"
            );
            return;
        }

        // Edge プロファイルを検出して AddBrowser を送信
        let edge_profiles = profiles::get_edge_profiles();
        tracing::info!(
            target: "mcv::plugin-edge-cookie",
            count = edge_profiles.len(),
            "Detected Edge profiles"
        );

        for profile in edge_profiles {
            let display_name = format!("Edge({})", profile.display_name);
            let browser_id = profile.browser_id;
            tracing::info!(
                target: "mcv::plugin-edge-cookie",
                browser_id = %browser_id,
                display_name = %display_name,
                "Sending AddBrowser for Edge profile"
            );
            let add_browser = AddBrowserPayload {
                browser_id: browser_id.clone(),
                browser_name: "Edge".to_string(),
                display_name,
            };
            if let Err(e) = self
                .send_add_browser(ctx.clone(), add_browser, self.logical_plugin_id.clone())
                .await
            {
                tracing::error!(
                    target: "mcv::plugin-edge-cookie",
                    error = %e,
                    browser_id = %browser_id,
                    "AddBrowser request failed"
                );
            }
        }
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let incoming = match serde_json::from_slice::<McvMessage>(msg) {
            Ok(message) => message,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-edge-cookie",
                    error = %e,
                    "Failed to parse message"
                );
                return;
            }
        };

        match incoming.message_type {
            MessageType::GetCookie => {
                let payload =
                    match serde_json::from_value::<GetCookiePayload>(incoming.payload.clone()) {
                        Ok(v) => v,
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-edge-cookie",
                                error = %e,
                                "Failed to parse GetCookie payload"
                            );
                            return;
                        }
                    };
                tracing::debug!(
                    target: "mcv::plugin-edge-cookie",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    "Received GetCookie request"
                );
                let cookies = load_cookies(&payload.browser_id, &payload.domain);
                tracing::debug!(
                    target: "mcv::plugin-edge-cookie",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    cookie_count = cookies.len(),
                    "Resolved cookies for GetCookie request"
                );
                let response = incoming.create_response(
                    MessageType::GetCookieAck,
                    serde_json::to_value(GetCookieAckPayload { cookies }).unwrap(),
                );
                if let Err(e) = ctx.send_notification(response).await {
                    tracing::warn!(
                        target: "mcv::plugin-edge-cookie",
                        error = %e,
                        "Failed to send GetCookieAck response"
                    );
                }
            }
            _ => {
                tracing::debug!(
                    target: "mcv::plugin-edge-cookie",
                    msg_type = ?incoming.message_type,
                    "Received message"
                );
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!(
            target: "mcv::plugin-edge-cookie",
            "EdgeCookiePlugin shutting down"
        );
    }
}

export_plugin_v3_async!(EdgeCookiePlugin);
