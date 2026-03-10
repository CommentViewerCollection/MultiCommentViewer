//! メッセージハンドリングとペイロード解析
//!
//! Coreから受信したメッセージの処理と、
//! JSONペイロードの型安全なデシリアライゼーションを提供します。

use std::any::type_name;
use std::time::Duration;

use mcv_messages::{
    AccountInfo, CanHandleUrlPayload, CanHandleUrlResultPayload, CommentReceivedPayload,
    ConnectPayload, ConnectedPayload, ConnectionRemovedPayload, DisconnectPayload,
    DisconnectedPayload, FetchAccountInfoPayload, GetBrowserPluginAckPayload,
    GetBrowserPluginPayload, GetCookieAckPayload, GetCookiePayload, McvEnvelope,
    Message as McvMessage, MessageDestination, MessageSource, MessageType, ProviderMessageKind,
    SetConnectionSitePayload, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::api;
use crate::connection::{build_provider_message, Connection};
use crate::KickPlugin;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut KickPlugin,
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

            let input_extra: Input = serde_json::from_value(connect.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect input.extra parse failed",
                    error = e.to_string()
                )
            })?;

            // URL から channelSlug を抽出
            let channel_slug = extract_channel_slug(&input_extra.url).ok_or_else(|| {
                mcv_plugin_telemetry::capture_context!(
                    "channelSlug の抽出に失敗",
                    url = input_extra.url.clone()
                )
            })?;

            tracing::info!(
                target: "mcv::plugin-kick",
                connection_id = %connect.connection_id,
                channel_slug = %channel_slug,
                "Kick チャンネル情報を取得中"
            );

            // ブラウザから kick.com の Cookie を取得
            let cookies =
                fetch_cookies_for_connect(&ctx, plugin.logical_plugin_id, &connect.browser.id)
                    .await;
            let cookie_header: String = cookies
                .iter()
                .map(|c| format!("{}={}", c.name, c.value))
                .collect::<Vec<_>>()
                .join("; ");
            tracing::info!(
                target: "mcv::plugin-kick",
                connection_id = %connect.connection_id,
                cookie_count = cookies.len(),
                "Cookie 取得完了"
            );

            // Kick API でチャンネル情報を取得
            let channel_info = match api::fetch_channel(&channel_slug, &cookie_header).await {
                Ok(info) => info,
                Err(api::FetchChannelError::Parse {
                    status,
                    body,
                    error,
                }) => {
                    tracing::error!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        http_status = status,
                        response_body = %body,
                        parse_error = %error,
                        "Kick API レスポンスのパースに失敗"
                    );
                    let msg = McvMessage::new_notification(
                        MessageType::Disconnected,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id,
                        },
                        MessageDestination::Core,
                        serde_json::to_value(DisconnectedPayload {
                            connection_id: connect.connection_id,
                        })
                        .unwrap(),
                    );
                    KickPlugin::send_message(ctx, msg).await;
                    return Ok(());
                }
                Err(api::FetchChannelError::Http(e)) => {
                    tracing::error!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        error = %e,
                        "Kick API HTTP リクエスト失敗"
                    );
                    let msg = McvMessage::new_notification(
                        MessageType::Disconnected,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id,
                        },
                        MessageDestination::Core,
                        serde_json::to_value(DisconnectedPayload {
                            connection_id: connect.connection_id,
                        })
                        .unwrap(),
                    );
                    KickPlugin::send_message(ctx, msg).await;
                    return Ok(());
                }
            };

            // ライブ中でなければ切断扱い
            // livestream フィールドが null → 配信中でない
            // livestream フィールドが存在する → is_live() で確認
            let is_live = channel_info
                .livestream
                .as_ref()
                .map(|l| l.is_live())
                .unwrap_or(false);

            if !is_live {
                tracing::info!(
                    target: "mcv::plugin-kick",
                    connection_id = %connect.connection_id,
                    channel_slug = %channel_slug,
                    "配信中ではないため切断"
                );
                let msg = McvMessage::new_notification(
                    MessageType::Disconnected,
                    MessageSource::Plugin {
                        plugin_id: plugin.logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(DisconnectedPayload {
                        connection_id: connect.connection_id,
                    })
                    .unwrap(),
                );
                KickPlugin::send_message(ctx, msg).await;
                return Ok(());
            }

            let chatroom_id = channel_info.chatroom.id;
            let channel_id = channel_info.id;

            tracing::info!(
                target: "mcv::plugin-kick",
                connection_id = %connect.connection_id,
                chatroom_id = chatroom_id,
                "WebSocket 接続を開始"
            );

            // Connected を返信
            let connected_msg = McvMessage::new_notification(
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
            KickPlugin::send_message(ctx.clone(), connected_msg).await;

            // ログイン中のユーザー情報を取得して送信（Cookie がある場合のみ成功する）
            match api::fetch_current_user(&cookie_header).await {
                Ok(user) => {
                    tracing::debug!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        username = %user.username,
                        "Kick ログインユーザー情報を取得"
                    );
                    let account_msg = McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id,
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id: connect.connection_id,
                            account: Some(AccountInfo {
                                user_id: user.id.to_string(),
                                display_name: user.username,
                                avatar_url: user.profile_pic,
                            }),
                        })
                        .unwrap(),
                    );
                    KickPlugin::send_message(ctx.clone(), account_msg).await;
                }
                Err(e) => {
                    tracing::debug!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        error = %e,
                        "Kick ログインユーザー情報の取得に失敗（未ログインの可能性あり）"
                    );
                }
            }

            match api::fetch_chat_history(channel_id, &cookie_header).await {
                Ok(history_items) => {
                    let mut provider_messages = history_items
                        .into_iter()
                        .map(|item| {
                            build_provider_message(
                                &chatroom_id.to_string(),
                                item.id,
                                item.sender.id,
                                item.sender.username,
                                &item.content,
                                &item.created_at,
                                ProviderMessageKind::HistoryChat,
                            )
                        })
                        .collect::<Vec<_>>();

                    provider_messages.sort_by_key(|m| m.timestamp);

                    if !provider_messages.is_empty() {
                        let envelope = McvEnvelope {
                            event_id: uuid::Uuid::new_v4(),
                            connection_id: connect.connection_id,
                            messages: provider_messages,
                            received_at: chrono::Utc::now().timestamp(),
                            raw_message: Some(format!("kick-history channel_id={channel_id}")),
                        };

                        let history_message = McvMessage::new_notification(
                            MessageType::CommentReceived,
                            MessageSource::Plugin {
                                plugin_id: plugin.logical_plugin_id,
                            },
                            MessageDestination::Core,
                            serde_json::to_value(CommentReceivedPayload {
                                connection_id: connect.connection_id,
                                envelope,
                            })
                            .unwrap(),
                        );
                        KickPlugin::send_message(ctx.clone(), history_message).await;
                    }
                }
                Err(api::FetchChatHistoryError::Parse {
                    status,
                    body,
                    error,
                }) => {
                    tracing::warn!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        channel_id = channel_id,
                        http_status = status,
                        response_body = %body,
                        parse_error = %error,
                        "Kick chat history parse failed"
                    );
                }
                Err(api::FetchChatHistoryError::Http(e)) => {
                    tracing::warn!(
                        target: "mcv::plugin-kick",
                        connection_id = %connect.connection_id,
                        channel_id = channel_id,
                        error = %e,
                        "Kick chat history request failed"
                    );
                }
            }

            conn.connect(ctx, plugin.logical_plugin_id, chatroom_id);
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
            let cookie_header = cookies
                .iter()
                .map(|c| format!("{}={}", c.name, c.value))
                .collect::<Vec<_>>()
                .join("; ");

            match api::fetch_current_user(&cookie_header).await {
                Ok(user) => {
                    tracing::debug!(
                        target: "mcv::plugin-kick",
                        username = %user.username,
                        "FetchAccountInfo: Kick ユーザー情報取得完了"
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
                                user_id: user.id.to_string(),
                                display_name: user.username,
                                avatar_url: user.profile_pic,
                            }),
                        })
                        .unwrap(),
                    );
                    KickPlugin::send_message(ctx, account_msg).await;
                }
                Err(e) => {
                    tracing::debug!(
                        target: "mcv::plugin-kick",
                        error = %e,
                        "FetchAccountInfo: Kick ユーザー情報の取得に失敗（未ログインの可能性あり）"
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
                    KickPlugin::send_message(ctx, clear_msg).await;
                }
            }
        }
        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = extract_channel_slug(&payload.url).is_some();
            let site_id = supported
                .then(|| mcv_common::SiteId::new("Kick", "a1b2c3d4-e5f6-7890-abcd-ef1234567890"));
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            KickPlugin::send_message(ctx, response).await;
        }
        _ => {}
    }
    Ok(())
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

/// `https://kick.com/onadan` → `"onadan"`
fn extract_channel_slug(url: &str) -> Option<String> {
    let trimmed = url.trim();
    let path = trimmed
        .strip_prefix("https://kick.com/")
        .or_else(|| trimmed.strip_prefix("http://kick.com/"))?;

    let slug = path
        .split(['/', '?', '#'])
        .next()
        .unwrap_or_default()
        .trim();

    if slug.is_empty() {
        return None;
    }

    Some(slug.to_string())
}

/// ブラウザプラグインから kick.com の Cookie を取得する
///
/// GetBrowserPlugin でブラウザプラグインの plugin_id を取得し、
/// GetCookie で `.kick.com` ドメインの Cookie を取得して返す。
/// ブラウザが未設定の場合や取得に失敗した場合は空 Vec を返す。
async fn fetch_cookies_for_connect(
    ctx: &PluginContext,
    logical_plugin_id: uuid::Uuid,
    browser_id: &mcv_messages::BrowserId,
) -> Vec<mcv_messages::Cookie> {
    // ステップ 1: GetBrowserPlugin → ブラウザプラグインの plugin_id を取得
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
                        target: "mcv::plugin-kick",
                        error = %e,
                        "GetBrowserPluginAck payload parse failed"
                    );
                    None
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-kick",
                message_type = ?response.message_type,
                "GetBrowserPlugin returned non-ack response"
            );
            None
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-kick",
                error = %e,
                "GetBrowserPlugin request failed"
            );
            None
        }
    };

    let Some(browser_plugin_id) = browser_plugin_id else {
        return vec![];
    };

    // ステップ 2: GetCookie → .kick.com の Cookie を取得
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
            domain: ".kick.com".to_string(),
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
                        target: "mcv::plugin-kick",
                        cookie_count = payload.cookies.len(),
                        "GetCookie request succeeded"
                    );
                    payload.cookies
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-kick",
                        error = %e,
                        "GetCookieAck payload parse failed"
                    );
                    vec![]
                }
            }
        }
        Ok(response) => {
            tracing::warn!(
                target: "mcv::plugin-kick",
                message_type = ?response.message_type,
                "GetCookie returned non-ack response"
            );
            vec![]
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-kick",
                error = %e,
                "GetCookie request failed"
            );
            vec![]
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_extract_channel_slug() {
        assert_eq!(
            extract_channel_slug("https://kick.com/onadan"),
            Some("onadan".to_string())
        );
        assert_eq!(
            extract_channel_slug("https://kick.com/onadan/"),
            Some("onadan".to_string())
        );
        assert_eq!(
            extract_channel_slug("https://kick.com/onadan?foo=bar"),
            Some("onadan".to_string())
        );
        assert_eq!(extract_channel_slug("https://kick.com/"), None);
        assert_eq!(extract_channel_slug("https://twitch.tv/user"), None);
    }
}
