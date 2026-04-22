//! plugin-firefox-cookie
//!
//! Firefox のプロファイルを検出し、AddBrowser メッセージを送信するプラグイン。
//! 各 Firefox プロファイルに対して 1 つの AddBrowser を送信する。
//! Firefox の Cookie は暗号化されていないため、SQLite から直接読み取る。

mod profiles;

use std::fs;
use std::sync::Arc;
use std::time::Duration;

use mcv_messages::{
    AddBrowserAckPayload, AddBrowserPayload, BrowserId, Cookie as McvCookie, GetCookieAckPayload,
    GetCookiePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloAckPayload, PluginHelloPayload, PluginId,
};
use plugin_abi_helper::v3::prelude::*;
use rusqlite::Connection;
use uuid::Uuid;

#[derive(Default)]
struct FirefoxCookiePlugin {
    logical_plugin_id: PluginId,
    is_initialized: bool,
}

fn load_cookies(browser_id: &BrowserId, domain: &str) -> Vec<McvCookie> {
    let profile = match profiles::get_firefox_profiles()
        .into_iter()
        .find(|p| p.browser_id == *browser_id)
    {
        Some(p) => p,
        None => return vec![],
    };

    let cookie_db_path = profile.profile_dir.join("cookies.sqlite");
    if !cookie_db_path.exists() {
        return vec![];
    }

    // Firefox がロックしている可能性があるためテンポラリにコピー
    let temp_db_path =
        std::env::temp_dir().join(format!("mcv_firefox_cookie_{}.db", Uuid::new_v4()));
    if let Err(e) = fs::copy(&cookie_db_path, &temp_db_path) {
        tracing::warn!(
            db_path = %cookie_db_path.display(),
            error = %e,
            "Firefox cookie DB のコピーに失敗しました"
        );
        return vec![];
    }

    let conn = match Connection::open(&temp_db_path) {
        Ok(c) => c,
        Err(e) => {
            tracing::warn!(
                temp_db_path = %temp_db_path.display(),
                error = %e,
                "Firefox cookie DB のオープンに失敗しました"
            );
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let normalized_domain = domain.trim().trim_start_matches('.').to_lowercase();
    let exact = normalized_domain.clone();
    let dotted = format!(".{}", normalized_domain);
    let like = format!("%.{}", normalized_domain);

    let mut stmt = match conn.prepare(
        "SELECT host, name, value, path
         FROM moz_cookies
         WHERE host = ?1 OR host = ?2 OR host LIKE ?3",
    ) {
        Ok(s) => s,
        Err(e) => {
            tracing::warn!(
                error = %e,
                "Firefox cookie DB クエリのプリペアに失敗しました"
            );
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let rows = stmt.query_map([exact, dotted, like], |row| {
        let host: String = row.get(0)?;
        let name: String = row.get(1)?;
        let value: String = row.get(2)?;
        let path: String = row.get(3)?;
        Ok((host, name, value, path))
    });

    let mut cookies = vec![];
    if let Ok(iter) = rows {
        for row in iter.flatten() {
            let (host, name, value, path) = row;
            if value.is_empty() {
                continue;
            }
            cookies.push(McvCookie {
                name,
                value,
                domain: host,
                path,
            });
        }
    }

    let _ = fs::remove_file(&temp_db_path);
    cookies
}

impl FirefoxCookiePlugin {
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
impl PluginImplV3Async for FirefoxCookiePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        let logical_plugin_id = PluginId::new(format!("FirefoxCookiePlugin_logical_{}", uuid));
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
                    target: "mcv::plugin-firefox-cookie",
                    "init_tracing() success"
                );
            }
            Err(_e) => {}
        }

        self.initialize(logical_plugin_id);

        // plugin-hello 送信
        let hello_payload = PluginHelloPayload {
            name: "Firefox Cookie".to_string(),
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
                target: "mcv::plugin-firefox-cookie",
                error = %e,
                "PluginHello request failed"
            );
            return;
        }

        // Firefox プロファイルを検出して AddBrowser を送信
        let profiles = profiles::get_firefox_profiles();
        tracing::info!(
            target: "mcv::plugin-firefox-cookie",
            count = profiles.len(),
            "Detected Firefox profiles"
        );

        for profile in profiles {
            let display_name = format!("Firefox({})", profile.display_name);
            let browser_id = profile.browser_id;
            tracing::info!(
                target: "mcv::plugin-firefox-cookie",
                browser_id = %browser_id,
                display_name = %display_name,
                "Sending AddBrowser for Firefox profile"
            );
            let add_browser = AddBrowserPayload {
                browser_id: browser_id.clone(),
                browser_name: "Firefox".to_string(),
                display_name,
            };
            if let Err(e) = self
                .send_add_browser(ctx.clone(), add_browser, self.logical_plugin_id.clone())
                .await
            {
                tracing::error!(
                    target: "mcv::plugin-firefox-cookie",
                    error = %e,
                    browser_id = %browser_id,
                    "AddBrowser request failed"
                );
            }
        }
    }

    async fn on_message(&mut self, _ctx: PluginContext, msg: &[u8]) {
        let incoming = match serde_json::from_slice::<McvMessage>(msg) {
            Ok(message) => message,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-firefox-cookie",
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
                                target: "mcv::plugin-firefox-cookie",
                                error = %e,
                                "Failed to parse GetCookie payload"
                            );
                            return;
                        }
                    };
                tracing::debug!(
                    target: "mcv::plugin-firefox-cookie",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    "Received GetCookie request"
                );
                let cookies = load_cookies(&payload.browser_id, &payload.domain);
                tracing::debug!(
                    target: "mcv::plugin-firefox-cookie",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    cookie_count = cookies.len(),
                    "Resolved cookies for GetCookie request"
                );
                let response = incoming.create_response(
                    MessageType::GetCookieAck,
                    serde_json::to_value(GetCookieAckPayload { cookies }).unwrap(),
                );
                if let Err(e) = _ctx.send_notification(response).await {
                    tracing::warn!(
                        target: "mcv::plugin-firefox-cookie",
                        error = %e,
                        "Failed to send GetCookieAck response"
                    );
                }
            }
            _ => {
                tracing::debug!(
                    target: "mcv::plugin-firefox-cookie",
                    msg_type = ?incoming.message_type,
                    "Received message"
                );
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!(
            target: "mcv::plugin-firefox-cookie",
            "FirefoxCookiePlugin shutting down"
        );
    }
}

export_plugin_v3_async!(FirefoxCookiePlugin);
