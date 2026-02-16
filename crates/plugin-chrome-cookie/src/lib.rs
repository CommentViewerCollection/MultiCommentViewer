//! plugin-chrome-cookie
//!
//! Chrome のプロファイルを検出し、AddBrowser メッセージを送信するプラグイン。
//! 各 Chrome プロファイルに対して 1 つの AddBrowser を送信する。

mod profiles;

use std::fs;
use std::path::{Path, PathBuf};
use std::sync::Arc;
use std::time::Duration;

use aes_gcm::aead::{Aead, KeyInit};
use aes_gcm::{Aes256Gcm, Nonce};
use base64::engine::general_purpose::STANDARD as BASE64_STANDARD;
use base64::Engine;
use mcv_messages::{
    AddBrowserAckPayload, AddBrowserPayload, BrowserId, Cookie as McvCookie, GetCookieAckPayload,
    GetCookiePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloAckPayload, PluginHelloPayload,
};
use plugin_abi_helper::v3::prelude::*;
use rusqlite::Connection;
use uuid::Uuid;

#[derive(Default)]
struct ChromeCookiePlugin {
    logical_plugin_id: Uuid,
    is_initialized: bool,
}

fn resolve_cookie_db_path(profile_dir: &Path) -> Option<PathBuf> {
    let new_path = profile_dir.join("Network").join("Cookies");
    if new_path.exists() {
        return Some(new_path);
    }
    let old_path = profile_dir.join("Cookies");
    if old_path.exists() {
        return Some(old_path);
    }
    None
}

fn decrypt_cookie_value(encrypted_value: &[u8], master_key: Option<&[u8]>) -> Option<String> {
    if encrypted_value.starts_with(b"v10") || encrypted_value.starts_with(b"v11") {
        let key = master_key?;
        if encrypted_value.len() < 3 + 12 + 16 {
            return None;
        }
        let nonce = Nonce::from_slice(&encrypted_value[3..15]);
        let ciphertext = &encrypted_value[15..];
        let cipher = Aes256Gcm::new_from_slice(key).ok()?;
        let plaintext = cipher.decrypt(nonce, ciphertext).ok()?;
        return String::from_utf8(plaintext).ok();
    }

    let decrypted = decrypt_dpapi(encrypted_value)?;
    String::from_utf8(decrypted).ok()
}

#[cfg(windows)]
fn decrypt_dpapi(data: &[u8]) -> Option<Vec<u8>> {
    use windows_sys::Win32::Foundation::LocalFree;
    use windows_sys::Win32::Security::Cryptography::{CryptUnprotectData, CRYPT_INTEGER_BLOB};

    let mut in_buf = data.to_vec();
    let mut in_blob = CRYPT_INTEGER_BLOB {
        cbData: in_buf.len() as u32,
        pbData: in_buf.as_mut_ptr(),
    };
    let mut out_blob = CRYPT_INTEGER_BLOB {
        cbData: 0,
        pbData: std::ptr::null_mut(),
    };

    let ok = unsafe {
        CryptUnprotectData(
            &mut in_blob,
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            std::ptr::null_mut(),
            0,
            &mut out_blob,
        )
    };
    if ok == 0 || out_blob.pbData.is_null() {
        return None;
    }

    let result =
        unsafe { std::slice::from_raw_parts(out_blob.pbData, out_blob.cbData as usize).to_vec() };
    unsafe {
        LocalFree(out_blob.pbData as *mut core::ffi::c_void);
    }
    Some(result)
}

#[cfg(not(windows))]
fn decrypt_dpapi(_data: &[u8]) -> Option<Vec<u8>> {
    None
}

fn load_chrome_master_key() -> Option<Vec<u8>> {
    let local_app_data = std::env::var("LOCALAPPDATA").ok()?;
    let local_state_path = PathBuf::from(local_app_data)
        .join("Google")
        .join("Chrome")
        .join("User Data")
        .join("Local State");
    let local_state = fs::read_to_string(local_state_path).ok()?;
    let json: serde_json::Value = serde_json::from_str(&local_state).ok()?;
    let encrypted_key_b64 = json["os_crypt"]["encrypted_key"].as_str()?;
    let mut encrypted_key = BASE64_STANDARD.decode(encrypted_key_b64).ok()?;
    if encrypted_key.starts_with(b"DPAPI") {
        encrypted_key.drain(0..5);
    }
    decrypt_dpapi(&encrypted_key)
}

fn load_cookies(browser_id: &BrowserId, domain: &str) -> Vec<McvCookie> {
    let profile = match profiles::get_chrome_profiles()
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

    let temp_db_path = std::env::temp_dir().join(format!("mcv_cookie_{}.db", Uuid::new_v4()));
    if fs::copy(&cookie_db_path, &temp_db_path).is_err() {
        return vec![];
    }

    let conn = match Connection::open(&temp_db_path) {
        Ok(c) => c,
        Err(_) => {
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let normalized_domain = domain.trim().trim_start_matches('.').to_lowercase();
    let exact = normalized_domain.clone();
    let dotted = format!(".{}", normalized_domain);
    let like = format!("%.{}", normalized_domain);

    let mut stmt = match conn.prepare(
        "SELECT host_key, name, value, encrypted_value, path
         FROM cookies
         WHERE host_key = ?1 OR host_key = ?2 OR host_key LIKE ?3",
    ) {
        Ok(s) => s,
        Err(_) => {
            let _ = fs::remove_file(&temp_db_path);
            return vec![];
        }
    };

    let master_key = load_chrome_master_key();
    let rows = stmt.query_map([exact, dotted, like], |row| {
        let host_key: String = row.get(0)?;
        let name: String = row.get(1)?;
        let value: String = row.get(2)?;
        let encrypted_value: Vec<u8> = row.get(3)?;
        let path: String = row.get(4)?;
        Ok((host_key, name, value, encrypted_value, path))
    });

    let mut cookies = vec![];
    if let Ok(iter) = rows {
        for row in iter.flatten() {
            let (host_key, name, value, encrypted_value, path) = row;
            let resolved_value = if value.is_empty() {
                decrypt_cookie_value(&encrypted_value, master_key.as_deref()).unwrap_or_default()
            } else {
                value
            };
            if resolved_value.is_empty() {
                continue;
            }
            cookies.push(McvCookie {
                name,
                value: resolved_value,
                domain: host_key,
                path,
            });
        }
    }

    let _ = fs::remove_file(&temp_db_path);
    cookies
}

impl ChromeCookiePlugin {
    fn initialize(&mut self, logical_plugin_id: Uuid) {
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
        let incoming = match serde_json::from_slice::<McvMessage>(msg) {
            Ok(message) => message,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-chrome-cookie",
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
                                target: "mcv::plugin-chrome-cookie",
                                error = %e,
                                "Failed to parse GetCookie payload"
                            );
                            return;
                        }
                    };
                tracing::debug!(
                    target: "mcv::plugin-chrome-cookie",
                    browser_id = %payload.browser_id,
                    domain = %payload.domain,
                    "Received GetCookie request"
                );
                let cookies = load_cookies(&payload.browser_id, &payload.domain);
                tracing::debug!(
                    target: "mcv::plugin-chrome-cookie",
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
                        target: "mcv::plugin-chrome-cookie",
                        error = %e,
                        "Failed to send GetCookieAck response"
                    );
                }
            }
            _ => {
                tracing::debug!(
                    target: "mcv::plugin-chrome-cookie",
                    msg_type = ?incoming.message_type,
                    "Received message"
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
