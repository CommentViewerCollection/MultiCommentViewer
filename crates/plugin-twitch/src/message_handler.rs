//! メッセージハンドリングとペイロード解析
//!
//! Coreから受信したメッセージの処理と、
//! JSONペイロードの型安全なデシリアライゼーションを提供します。

use std::any::type_name;
use std::time::Duration;

use mcv_messages::{
    AccountInfo, CanHandleUrlPayload, CanHandleUrlResultPayload, ConnectPayload, ConnectedPayload,
    ConnectionRemovedPayload, DisconnectPayload, FetchAccountInfoPayload,
    GetBrowserPluginAckPayload, GetBrowserPluginPayload, GetCookieAckPayload, GetCookiePayload,
    Message as McvMessage, MessageDestination, MessageSource, MessageType,
    SetConnectionSitePayload, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::TwitchPlugin;
use twitch_lib::auth_token::AuthToken;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut TwitchPlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let set_conn_site: SetConnectionSitePayload = parse_payload(&message.payload)?;
            let conn_id = set_conn_site.connection_id;
            let conn = Connection::new(&conn_id);
            plugin.connections.insert(conn_id, conn);
        }
        MessageType::Connect => {
            let connect: ConnectPayload = parse_payload(&message.payload)?;
            let conn = match plugin.connections.get_mut(&connect.connection_id) {
                Some(a) => a,
                None => Err(mcv_plugin_telemetry::capture_context!(""))?,
            };
            // connectedを返信
            let message = McvMessage::new_notification(
                MessageType::Connected,
                MessageSource::Plugin {
                    plugin_id: plugin.logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(ConnectedPayload {
                    connection_id: connect.connection_id,
                })
                .unwrap(),
            );
            TwitchPlugin::send_message(ctx.clone(), message).await;
            let input_extra: Input = serde_json::from_value(connect.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect input.extra parse failed",
                    error = e.to_string()
                )
            })?;

            let cookies =
                fetch_cookies_for_connect(&ctx, plugin.logical_plugin_id, &connect.browser.id)
                    .await;
            let username = "kv501k";
            let auth_token = cookies
                .iter()
                .find(|c| c.name == "auth_token")
                .map(|c| AuthToken::new(&c.value));
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connect.connection_id,
                browser_id = %connect.browser.id,
                url = %input_extra.url,
                cookie_count = cookies.len(),
                has_auth_token = auth_token.is_some(),
                "Connect prerequisites resolved"
            );
            conn.connect(
                ctx,
                plugin.logical_plugin_id,
                &input_extra.url,
                &username,
                auth_token,
            );
        }
        MessageType::Disconnect => {
            let disconnect: DisconnectPayload = parse_payload(&message.payload)?;
            if let Some(conn) = plugin.connections.get_mut(&disconnect.connection_id) {
                conn.stop();
            }
        }
        MessageType::ConnectionRemoved => {
            let removed: ConnectionRemovedPayload = parse_payload(&message.payload)?;
            if let Some(mut conn) = plugin.connections.remove(&removed.connection_id) {
                conn.stop();
            }
        }
        MessageType::FetchAccountInfo => {
            let payload: FetchAccountInfoPayload = parse_payload(&message.payload)?;
            let conn_id = payload.connection_id;
            let cookies =
                fetch_cookies_for_connect(&ctx, plugin.logical_plugin_id, &payload.browser.id)
                    .await;
            let auth_token = cookies
                .iter()
                .find(|c| c.name == "auth_token")
                .map(|c| c.value.clone());

            if let Some(token) = auth_token {
                match twitch_lib::auth_token::validate_token(&token).await {
                    Ok(info) => {
                        tracing::debug!(
                            target: "mcv::plugin-twitch",
                            login = %info.login,
                            "FetchAccountInfo: Twitch ユーザー情報取得完了"
                        );
                        let account_msg = McvMessage::new_notification(
                            MessageType::UpdateConnectionAccount,
                            MessageSource::Plugin {
                                plugin_id: plugin.logical_plugin_id,
                            },
                            MessageDestination::Core,
                            serde_json::to_value(UpdateConnectionAccountPayload {
                                connection_id: conn_id,
                                account: Some(AccountInfo {
                                    user_id: info.user_id,
                                    display_name: info.login,
                                    avatar_url: None,
                                }),
                            })
                            .unwrap(),
                        );
                        TwitchPlugin::send_message(ctx, account_msg).await;
                    }
                    Err(e) => {
                        tracing::debug!(
                            target: "mcv::plugin-twitch",
                            error = %e,
                            "FetchAccountInfo: Twitch トークン検証に失敗（未ログインの可能性あり）"
                        );
                        let clear_msg = McvMessage::new_notification(
                            MessageType::UpdateConnectionAccount,
                            MessageSource::Plugin {
                                plugin_id: plugin.logical_plugin_id,
                            },
                            MessageDestination::Core,
                            serde_json::to_value(UpdateConnectionAccountPayload {
                                connection_id: conn_id,
                                account: None,
                            })
                            .unwrap(),
                        );
                        TwitchPlugin::send_message(ctx, clear_msg).await;
                    }
                }
            } else {
                let clear_msg = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: plugin.logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id: conn_id,
                        account: None,
                    })
                    .unwrap(),
                );
                TwitchPlugin::send_message(ctx, clear_msg).await;
            }
        }
        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = Connection::extract_channel_id(&payload.url).is_some();
            let site_id = supported
                .then(|| mcv_common::SiteId::new("Twitch", "f3c2a1d7-6e4b-4f8c-9a21-5d7b3e2c9f64"));
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            TwitchPlugin::send_message(ctx, response).await;
        }
        _ => {}
    }
    Ok(())
}
fn parse_payload<T>(payload: &serde_json::Value) -> Result<T, mcv_plugin_telemetry::TracingError>
where
    T: DeserializeOwned,
{
    let result = T::deserialize(payload);

    match result {
        Ok(v) => Ok(v),
        Err(e) => Err(mcv_plugin_telemetry::capture_context!(
            "payloadが復元できない",
            type =   type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),

        )
        .into()),
    }
}

#[derive(Deserialize)]
struct Input {
    url: String,
}
async fn fetch_cookies_for_connect(
    ctx: &PluginContext,
    logical_plugin_id: uuid::Uuid,
    browser_id: &mcv_messages::BrowserId,
) -> Vec<mcv_messages::Cookie> {
    let get_browser_plugin_message = McvMessage::new_request(
        MessageType::GetBrowserPlugin,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id,
        },
        MessageDestination::Core,
        serde_json::to_value(GetBrowserPluginPayload {
            browser_id: browser_id.clone(),
        })
        .unwrap(),
    );
    let browser_plugin_id = match ctx
        .send_request(get_browser_plugin_message, Duration::from_secs(10))
        .await
    {
        Ok(response) if response.message_type == MessageType::GetBrowserPluginAck => {
            match serde_json::from_value::<GetBrowserPluginAckPayload>(response.payload) {
                Ok(payload) => Some(payload.plugin_id),
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-twitch",
                        error = %e,
                        "GetBrowserPluginAck payload parse failed"
                    );
                    None
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetBrowserPlugin returned non-ack response"
            );
            None
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                error = %e,
                "GetBrowserPlugin request failed"
            );
            None
        }
    };

    let Some(browser_plugin_id) = browser_plugin_id else {
        return vec![];
    };

    let get_cookie_message = McvMessage::new_request(
        MessageType::GetCookie,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id,
        },
        MessageDestination::Plugin {
            plugin_id: browser_plugin_id,
        },
        serde_json::to_value(GetCookiePayload {
            browser_id: browser_id.clone(),
            domain: ".twitch.tv".to_string(),
        })
        .unwrap(),
    );

    match ctx
        .send_request(get_cookie_message, Duration::from_secs(10))
        .await
    {
        Ok(response) if response.message_type == MessageType::GetCookieAck => {
            match serde_json::from_value::<GetCookieAckPayload>(response.payload) {
                Ok(payload) => {
                    tracing::debug!(
                        target: "mcv::plugin-twitch",
                        cookie_count = payload.cookies.len(),
                        "GetCookie request succeeded"
                    );
                    payload.cookies
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-twitch",
                        error = %e,
                        "GetCookieAck payload parse failed"
                    );
                    vec![]
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetCookie returned non-ack response"
            );
            vec![]
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                error = %e,
                "GetCookie request failed"
            );
            vec![]
        }
    }
}
