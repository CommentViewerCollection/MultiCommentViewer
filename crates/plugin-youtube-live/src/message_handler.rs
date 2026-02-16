//! メッセージハンドリングとペイロード解析
//!
//! Coreから受信したメッセージの処理と、
//! JSONペイロードの型安全なデシリアライゼーションを提供します。

use std::any::type_name;
use std::time::Duration;

use mcv_messages::{
    ConnectPayload, ConnectedPayload, ConnectionRemovedPayload, DisconnectPayload,
    GetBrowserPluginAckPayload, GetBrowserPluginPayload, GetCookieAckPayload, GetCookiePayload,
    Message as McvMessage, MessageDestination, MessageSource, MessageType,
    SetConnectionSitePayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::YouTubeLivePlugin;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut YouTubeLivePlugin,
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
            let cookies =
                fetch_cookies_for_connect(&ctx, plugin.logical_plugin_id, &connect.browser.id)
                    .await;

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
            YouTubeLivePlugin::send_message(ctx.clone(), message).await;
            let input_extra: Result<Input, serde_json::Error> =
                serde_json::from_value(connect.input.extra);
            let input_extra = input_extra.unwrap();
            conn.connect(ctx, plugin.logical_plugin_id, &input_extra.url, cookies);
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
        _ => {}
    }
    Ok(())
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
                        target: "mcv::plugin-youtube-live",
                        error = %e,
                        "GetBrowserPluginAck payload parse failed"
                    );
                    None
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-youtube-live",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetBrowserPlugin returned non-ack response"
            );
            None
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-youtube-live",
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
            domain: "www.youtube.com".to_string(),
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
                        target: "mcv::plugin-youtube-live",
                        cookie_count = payload.cookies.len(),
                        "GetCookie request succeeded"
                    );
                    payload.cookies
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-youtube-live",
                        error = %e,
                        "GetCookieAck payload parse failed"
                    );
                    vec![]
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-youtube-live",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetCookie returned non-ack response"
            );
            vec![]
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-youtube-live",
                error = %e,
                "GetCookie request failed"
            );
            vec![]
        }
    }
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
