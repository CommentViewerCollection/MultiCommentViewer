//! Core からのメッセージ処理
//!
//! `on_message_impl` が各 MessageType に応じた処理を行う。
//! WHOWATCH Cookie の取得には Cookie プラグインを利用する。

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
use whowatch_lib::WhoWatchClient;

use crate::connection::Connection;
use crate::WhoWatchPlugin;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut WhoWatchPlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let set_conn_site: SetConnectionSitePayload = parse_payload(&message.payload)?;
            let conn_id = set_conn_site.connection_id;
            let conn = Connection::new(&conn_id);
            plugin.connections.insert(conn_id, conn);
            tracing::debug!(
                target: "mcv::plugin-whowatch",
                connection_id = %conn_id,
                "SetConnectionSite: 接続エントリを作成"
            );
        }

        MessageType::Connect => {
            let connect: ConnectPayload = parse_payload(&message.payload)?;
            let conn_id = connect.connection_id;

            let conn = match plugin.connections.get_mut(&conn_id) {
                Some(c) => c,
                None => Err(mcv_plugin_telemetry::capture_context!(
                    "Connect: 接続エントリが見つからない",
                    connection_id = conn_id.to_string()
                ))?,
            };

            // Connected を返信
            WhoWatchPlugin::send_message(
                ctx.clone(),
                McvMessage::new_notification(
                    MessageType::Connected,
                    MessageSource::Plugin {
                        plugin_id: plugin.logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(ConnectedPayload {
                        connection_id: conn_id,
                    })
                    .unwrap(),
                ),
            )
            .await;

            let input_extra: Input = serde_json::from_value(connect.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect: input.extra のパースに失敗",
                    error = e.to_string()
                )
            })?;

            // WHOWATCH Cookie を取得
            let cookies = fetch_cookies_for_connect(
                &ctx,
                plugin.logical_plugin_id.clone(),
                &connect.browser.id,
            )
            .await;

            let whowatch_token = cookies
                .iter()
                .find(|c| c.name == "WHOWATCH")
                .map(|c| c.value.clone())
                .unwrap_or_default();

            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %conn_id,
                url = %input_extra.url,
                has_token = !whowatch_token.is_empty(),
                cookie_count = cookies.len(),
                "Connect: 前提条件を確認済み"
            );

            if whowatch_token.is_empty() {
                tracing::warn!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %conn_id,
                    "WHOWATCH Cookie が見つからない。未ログイン状態で接続を試みます"
                );
            }

            conn.connect(
                ctx,
                plugin.logical_plugin_id.clone(),
                &input_extra.url,
                &whowatch_token,
            );
        }

        MessageType::Disconnect => {
            let disconnect: DisconnectPayload = parse_payload(&message.payload)?;
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %disconnect.connection_id,
                "Disconnect 受信"
            );
            if let Some(conn) = plugin.connections.get_mut(&disconnect.connection_id) {
                conn.stop();
            }
        }

        MessageType::ConnectionRemoved => {
            let removed: ConnectionRemovedPayload = parse_payload(&message.payload)?;
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %removed.connection_id,
                "ConnectionRemoved 受信"
            );
            if let Some(mut conn) = plugin.connections.remove(&removed.connection_id) {
                conn.stop();
            }
        }

        MessageType::FetchAccountInfo => {
            let payload: FetchAccountInfoPayload = parse_payload(&message.payload)?;
            let conn_id = payload.connection_id;

            let cookies = fetch_cookies_for_connect(
                &ctx,
                plugin.logical_plugin_id.clone(),
                &payload.browser.id,
            )
            .await;

            let whowatch_token = cookies
                .iter()
                .find(|c| c.name == "WHOWATCH")
                .map(|c| c.value.clone());

            match whowatch_token {
                Some(token) if !token.is_empty() => match WhoWatchClient::new(&token, None) {
                    Ok(client) => match client.get_my_user().await {
                        Ok(user) => {
                            tracing::debug!(
                                target: "mcv::plugin-whowatch",
                                user_id = %user.id,
                                name = %user.name,
                                "FetchAccountInfo: ユーザー情報取得完了"
                            );
                            let account_msg = McvMessage::new_notification(
                                MessageType::UpdateConnectionAccount,
                                MessageSource::Plugin {
                                    plugin_id: plugin.logical_plugin_id.clone(),
                                },
                                MessageDestination::Core,
                                serde_json::to_value(UpdateConnectionAccountPayload {
                                    connection_id: conn_id,
                                    account: Some(AccountInfo {
                                        user_id: user.id.to_string(),
                                        display_name: user.name,
                                        avatar_url: if user.icon_url.is_empty() {
                                            None
                                        } else {
                                            Some(user.icon_url)
                                        },
                                    }),
                                })
                                .unwrap(),
                            );
                            WhoWatchPlugin::send_message(ctx, account_msg).await;
                        }
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-whowatch",
                                error = %e,
                                "FetchAccountInfo: /users/me の取得に失敗"
                            );
                            send_clear_account(ctx, &plugin.logical_plugin_id, conn_id).await;
                        }
                    },
                    Err(e) => {
                        tracing::warn!(
                            target: "mcv::plugin-whowatch",
                            error = %e,
                            "FetchAccountInfo: WhoWatchClient 生成失敗"
                        );
                        send_clear_account(ctx, &plugin.logical_plugin_id, conn_id).await;
                    }
                },
                _ => {
                    tracing::debug!(
                        target: "mcv::plugin-whowatch",
                        "FetchAccountInfo: WHOWATCH Cookie なし（未ログイン）"
                    );
                    send_clear_account(ctx, &plugin.logical_plugin_id, conn_id).await;
                }
            }
        }

        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = Connection::extract_live_id(&payload.url).is_some();
            let site_id = supported.then(|| {
                mcv_common::SiteId::new("WhoWatch", "b4e7a2c9-1f35-4d8e-a601-9c3b7f52d084")
            });
            tracing::debug!(
                target: "mcv::plugin-whowatch",
                url = %payload.url,
                supported,
                "CanHandleUrl"
            );
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            WhoWatchPlugin::send_message(ctx, response).await;
        }

        _ => {}
    }
    Ok(())
}

// ---------------------------------------------------------------------------
// ユーティリティ
// ---------------------------------------------------------------------------

#[allow(clippy::result_large_err)]
fn parse_payload<T>(payload: &serde_json::Value) -> Result<T, mcv_plugin_telemetry::TracingError>
where
    T: DeserializeOwned,
{
    T::deserialize(payload).map_err(|e| {
        mcv_plugin_telemetry::capture_context!(
            "payload が復元できない",
            type = type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),
        )
        .into()
    })
}

async fn send_clear_account(ctx: PluginContext, plugin_id: &PluginId, conn_id: uuid::Uuid) {
    WhoWatchPlugin::send_message(
        ctx,
        McvMessage::new_notification(
            MessageType::UpdateConnectionAccount,
            MessageSource::Plugin {
                plugin_id: plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(UpdateConnectionAccountPayload {
                connection_id: conn_id,
                account: None,
            })
            .unwrap(),
        ),
    )
    .await;
}

#[derive(Deserialize)]
struct Input {
    url: String,
}

/// Cookie プラグイン経由で whowatch.tv ドメインの Cookie を取得する。
async fn fetch_cookies_for_connect(
    ctx: &PluginContext,
    logical_plugin_id: PluginId,
    browser_id: &mcv_messages::BrowserId,
) -> Vec<mcv_messages::Cookie> {
    // GetBrowserPlugin でブラウザプラグインの plugin_id を取得
    let get_browser_msg = McvMessage::new_request(
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
        .send_request(get_browser_msg, Duration::from_secs(10))
        .await
    {
        Ok(resp) if resp.message_type == MessageType::GetBrowserPluginAck => {
            match serde_json::from_value::<GetBrowserPluginAckPayload>(resp.payload) {
                Ok(p) => Some(p.plugin_id),
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-whowatch",
                        error = %e,
                        "GetBrowserPluginAck payload パース失敗"
                    );
                    None
                }
            }
        }
        Ok(resp) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                message_type = ?resp.message_type,
                "GetBrowserPlugin が非 Ack レスポンスを返した"
            );
            None
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                error = %e,
                "GetBrowserPlugin リクエスト失敗"
            );
            None
        }
    };

    let Some(browser_plugin_id) = browser_plugin_id else {
        return vec![];
    };

    // GetCookie で whowatch.tv Cookie を取得
    let get_cookie_msg = McvMessage::new_request(
        MessageType::GetCookie,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id.clone(),
        },
        MessageDestination::Plugin {
            plugin_id: browser_plugin_id,
        },
        serde_json::to_value(GetCookiePayload {
            browser_id: browser_id.clone(),
            domain: ".whowatch.tv".to_string(),
        })
        .unwrap(),
    );

    match ctx
        .send_request(get_cookie_msg, Duration::from_secs(10))
        .await
    {
        Ok(resp) if resp.message_type == MessageType::GetCookieAck => {
            match serde_json::from_value::<GetCookieAckPayload>(resp.payload) {
                Ok(p) => {
                    tracing::debug!(
                        target: "mcv::plugin-whowatch",
                        cookie_count = p.cookies.len(),
                        "GetCookie 成功"
                    );
                    p.cookies
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-whowatch",
                        error = %e,
                        "GetCookieAck payload パース失敗"
                    );
                    vec![]
                }
            }
        }
        Ok(resp) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                message_type = ?resp.message_type,
                "GetCookie が非 Ack レスポンスを返した"
            );
            vec![]
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                error = %e,
                "GetCookie リクエスト失敗"
            );
            vec![]
        }
    }
}
