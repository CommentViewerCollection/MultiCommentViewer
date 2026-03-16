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
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginId,
    SetConnectionSitePayload, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::NicoLivePlugin;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut NicoLivePlugin,
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
                None => Err(mcv_plugin_telemetry::capture_context!(
                    "connection not found"
                ))?,
            };

            let input_extra: Input = serde_json::from_value(connect.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect input.extra parse failed",
                    error = e.to_string()
                )
            })?;

            let live_id = Connection::extract_live_id(&input_extra.url).ok_or_else(|| {
                mcv_plugin_telemetry::capture_context!(
                    "live_id の抽出に失敗",
                    url = input_extra.url.as_str()
                )
            })?;

            tracing::info!(
                target: "mcv::plugin-nicolive",
                connection_id = %connect.connection_id,
                live_id = %live_id,
                "WebSocket URL を取得中"
            );

            let conn_data = match Connection::fetch_websocket_url(&live_id).await {
                Ok(data) => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connect.connection_id,
                        live_id = %live_id,
                        ws_url = %data.ws_url,
                        "WebSocket URL 取得成功"
                    );
                    data
                }
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connect.connection_id,
                        live_id = %live_id,
                        error = %e,
                        "WebSocket URL 取得失敗"
                    );
                    return Err(mcv_plugin_telemetry::capture_context!(
                        "WebSocket URL 取得失敗",
                        error = e
                    )
                    .into());
                }
            };

            // Connected を先に返信してから接続タスクを開始する
            let response = McvMessage::new_notification(
                MessageType::Connected,
                MessageSource::Plugin {
                    plugin_id: plugin.logical_plugin_id.clone(),
                },
                MessageDestination::Core,
                serde_json::to_value(ConnectedPayload {
                    connection_id: connect.connection_id,
                })
                .unwrap(),
            );
            NicoLivePlugin::send_message(ctx.clone(), response).await;

            // ログイン中の視聴者情報を送信（取得できた場合のみ）
            if let Some(viewer_name) = conn_data.viewer_name.as_deref() {
                let account_msg = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: plugin.logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id: connect.connection_id,
                        account: Some(AccountInfo {
                            user_id: conn_data
                                .viewer_id
                                .clone()
                                .unwrap_or_else(|| viewer_name.to_string()),
                            display_name: viewer_name.to_string(),
                            avatar_url: conn_data.viewer_icon_url.clone(),
                        }),
                    })
                    .unwrap(),
                );
                NicoLivePlugin::send_message(ctx.clone(), account_msg).await;
            }

            conn.connect(
                ctx,
                plugin.logical_plugin_id.clone(),
                &conn_data.ws_url,
                conn_data.title,
                conn_data.start_time,
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
            let cookies = fetch_cookies_for_connect(
                &ctx,
                plugin.logical_plugin_id.clone(),
                &payload.browser.id,
            )
            .await;
            let nico_cookies = cookies
                .iter()
                .map(|c| nicolive_lib::Cookie {
                    name: c.name.clone(),
                    value: c.value.clone(),
                })
                .collect::<Vec<_>>();

            match nicolive_lib::fetch_account_info_from_top(&nico_cookies).await {
                Ok(Some(account)) => {
                    tracing::debug!(
                        target: "mcv::plugin-nicolive",
                        display_name = %account.display_name,
                        "FetchAccountInfo: NicoLive ユーザー情報取得完了"
                    );
                    let account_msg = McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id: payload.connection_id,
                            account: Some(AccountInfo {
                                user_id: account.user_id,
                                display_name: account.display_name,
                                avatar_url: account.avatar_url,
                            }),
                        })
                        .unwrap(),
                    );
                    NicoLivePlugin::send_message(ctx, account_msg).await;
                }
                Ok(None) => {
                    tracing::debug!(
                        target: "mcv::plugin-nicolive",
                        "FetchAccountInfo: NicoLive ログインユーザー情報なし"
                    );
                    let clear_msg = McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id: payload.connection_id,
                            account: None,
                        })
                        .unwrap(),
                    );
                    NicoLivePlugin::send_message(ctx, clear_msg).await;
                }
                Err(e) => {
                    tracing::debug!(
                        target: "mcv::plugin-nicolive",
                        error = %e,
                        "FetchAccountInfo: NicoLive ユーザー情報取得失敗"
                    );
                    let clear_msg = McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id: payload.connection_id,
                            account: None,
                        })
                        .unwrap(),
                    );
                    NicoLivePlugin::send_message(ctx, clear_msg).await;
                }
            }
        }
        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = nicolive_lib::extract_live_id(&payload.url).is_some();
            let site_id = supported.then(|| {
                mcv_common::SiteId::new("NicoLive", "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d")
            });
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            NicoLivePlugin::send_message(ctx, response).await;
        }
        _ => {}
    }
    Ok(())
}

async fn fetch_cookies_for_connect(
    ctx: &PluginContext,
    logical_plugin_id: PluginId,
    browser_id: &mcv_messages::BrowserId,
) -> Vec<mcv_messages::Cookie> {
    let get_browser_plugin_message = McvMessage::new_request(
        MessageType::GetBrowserPlugin,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id.clone(),
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
                        target: "mcv::plugin-nicolive",
                        error = %e,
                        "GetBrowserPluginAck payload parse failed"
                    );
                    None
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-nicolive",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetBrowserPlugin returned non-ack response"
            );
            None
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-nicolive",
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
            plugin_id: browser_plugin_id.clone(),
        },
        serde_json::to_value(GetCookiePayload {
            browser_id: browser_id.clone(),
            domain: ".nicovideo.jp".to_string(),
        })
        .unwrap(),
    );

    match ctx
        .send_request(get_cookie_message, Duration::from_secs(10))
        .await
    {
        Ok(response) if response.message_type == MessageType::GetCookieAck => {
            match serde_json::from_value::<GetCookieAckPayload>(response.payload) {
                Ok(payload) => payload.cookies,
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-nicolive",
                        error = %e,
                        "GetCookieAck payload parse failed"
                    );
                    vec![]
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-nicolive",
                message_type = ?response.message_type,
                payload = ?response.payload,
                "GetCookie returned non-ack response"
            );
            vec![]
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-nicolive",
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
    T::deserialize(payload).map_err(|e| {
        mcv_plugin_telemetry::capture_context!(
            "payloadが復元できない",
            type = type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),
        )
        .into()
    })
}

#[derive(Deserialize)]
struct Input {
    url: String,
}
