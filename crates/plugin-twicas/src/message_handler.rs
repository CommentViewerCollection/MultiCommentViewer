//! メッセージハンドリングとペイロード解析

use std::any::type_name;
use std::sync::Arc;
use std::time::Duration;

use mcv_messages::{
    CanHandleUrlPayload, CanHandleUrlResultPayload, ConnectFailedPayload, ConnectPayload,
    ConnectedPayload, ConnectionRemovedPayload, DisconnectPayload, GetBrowserPluginAckPayload,
    GetBrowserPluginPayload, GetCookieAckPayload, GetCookiePayload, Message as McvMessage,
    MessageDestination, MessageSource, MessageType, SendCommentPayload, SetConnectionSitePayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::TwicasPlugin;

pub(crate) async fn on_message_impl(
    plugin: &mut TwicasPlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let set_conn_site: SetConnectionSitePayload = parse_payload(&message.payload)?;
            let conn_id = set_conn_site.connection_id;
            plugin
                .connections
                .insert(conn_id, Connection::new(&conn_id));
        }
        MessageType::Connect => {
            let connect: ConnectPayload = parse_payload(&message.payload)?;
            let conn = match plugin.connections.get_mut(&connect.connection_id) {
                Some(v) => v,
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

            let user_name = match Connection::extract_user_name(&input_extra.url) {
                Some(v) => v,
                None => Err(mcv_plugin_telemetry::capture_context!(
                    "twicas user_name extraction failed",
                    url = input_extra.url.clone()
                ))?,
            };

            // twitcasting.tv / frontendapi.twitcasting.tv で共有するクッキージャー
            let jar = Arc::new(reqwest::cookie::Jar::default());
            let session_client = match reqwest::Client::builder()
                .cookie_provider(Arc::clone(&jar))
                .build()
            {
                Ok(c) => Arc::new(c),
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-twicas",
                        connection_id = %connect.connection_id,
                        error = %e,
                        "HTTPクライアントの構築に失敗しました"
                    );
                    let failed_msg = McvMessage::new_notification(
                        MessageType::ConnectFailed,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(ConnectFailedPayload {
                            connection_id: connect.connection_id,
                            reason: format!("HTTPクライアントの構築に失敗しました: {}", e),
                        })
                        .unwrap(),
                    );
                    TwicasPlugin::send_message(ctx, failed_msg).await;
                    return Ok(());
                }
            };

            // ブラウザcookieを取得して jar に追加する
            // GetBrowserPlugin → GetCookie の2ステップで取得する（YouTube live と同じフロー）
            // wpass 解決より先に追加することで、resolve_wpass でブラウザ認証済みか否かを
            // 判定できる（CsSessionIdNotFound → 認証済みとしてwpassスキップ）
            {
                let browser_id = connect.browser.id.clone();
                let get_browser_plugin_msg = McvMessage::new_request(
                    MessageType::GetBrowserPlugin,
                    MessageSource::Plugin {
                        plugin_id: plugin.logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(GetBrowserPluginPayload {
                        browser_id: browser_id.clone(),
                    })
                    .unwrap(),
                );
                let browser_plugin_id = match ctx
                    .send_request(get_browser_plugin_msg, Duration::from_secs(10))
                    .await
                {
                    Ok(resp) if resp.message_type == MessageType::GetBrowserPluginAck => {
                        match serde_json::from_value::<GetBrowserPluginAckPayload>(resp.payload) {
                            Ok(p) => Some(p.plugin_id),
                            Err(e) => {
                                tracing::warn!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connect.connection_id,
                                    error = %e,
                                    "GetBrowserPluginAck payload のパースに失敗"
                                );
                                None
                            }
                        }
                    }
                    _ => None,
                };

                if let Some(bp_id) = browser_plugin_id {
                    let get_cookie_msg = McvMessage::new_request(
                        MessageType::GetCookie,
                        MessageSource::Plugin {
                            plugin_id: plugin.logical_plugin_id.clone(),
                        },
                        MessageDestination::Plugin { plugin_id: bp_id },
                        serde_json::to_value(GetCookiePayload {
                            browser_id: browser_id.clone(),
                            domain: "twitcasting.tv".to_string(),
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
                                    let twitcasting_url: reqwest::Url =
                                        "https://twitcasting.tv/".parse().unwrap();
                                    for cookie in &p.cookies {
                                        jar.add_cookie_str(
                                            &format!("{}={}", cookie.name, cookie.value),
                                            &twitcasting_url,
                                        );
                                    }
                                    tracing::info!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connect.connection_id,
                                        cookie_count = p.cookies.len(),
                                        "ブラウザcookieをjarに追加しました"
                                    );
                                }
                                Err(e) => {
                                    tracing::warn!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connect.connection_id,
                                        error = %e,
                                        "GetCookieAck payload のパースに失敗"
                                    );
                                }
                            }
                        }
                        _ => {
                            tracing::warn!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connect.connection_id,
                                "ブラウザcookieの取得に失敗しました（GetCookieAck が得られませんでした）"
                            );
                        }
                    }
                } else {
                    tracing::warn!(
                        target: "mcv::plugin-twicas",
                        connection_id = %connect.connection_id,
                        "ブラウザプラグインIDの取得に失敗しました（GetBrowserPluginAck が得られませんでした）"
                    );
                }
            }

            // 合言葉（password）が指定されている場合は wpass クッキーを取得する
            // ブラウザcookieで認証済みの場合、resolve_wpass のGETで合言葉フォームが含まれない
            // HTMLが返ってくるため CsSessionIdNotFound になる。この場合はブラウザcookieで
            // 認証済みと判断してwpassなしで接続を続行する。
            let (wpass, cs_session_id_from_wpass): (Option<String>, Option<String>) =
                match input_extra.password.as_deref() {
                    Some(pw) if !pw.is_empty() => {
                        tracing::info!(
                            target: "mcv::plugin-twicas",
                            connection_id = %connect.connection_id,
                            user_name = %user_name,
                            "Resolving wpass from password"
                        );
                        match twicas_lib::resolve_wpass(&session_client, &user_name, pw).await {
                            Ok((w, cs)) => {
                                tracing::info!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connect.connection_id,
                                    "wpass resolved successfully"
                                );
                                // wpass を jar に登録（fetch_session_ids で自動送信される）
                                let twitcasting_url: reqwest::Url =
                                    "https://twitcasting.tv/".parse().unwrap();
                                jar.add_cookie_str(&format!("wpass={}", w), &twitcasting_url);
                                (Some(w), Some(cs))
                            }
                            Err(twicas_lib::WpassError::CsSessionIdNotFound) => {
                                // 合言葉フォームが見つからない = ブラウザcookieで認証済み、または
                                // 合言葉不要な配信。wpassなしで接続を続行する。
                                tracing::info!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connect.connection_id,
                                    "resolve_wpass: cs_session_id が見つかりません。ブラウザcookieで認証済みとしてwpassなしで続行します"
                                );
                                (None, None)
                            }
                            Err(e) => {
                                let reason = match &e {
                                    twicas_lib::WpassError::WpassNotFound => {
                                        "合言葉が間違っている可能性があります".to_string()
                                    }
                                    twicas_lib::WpassError::Http(_) => format!("通信エラー: {}", e),
                                    twicas_lib::WpassError::CsSessionIdNotFound => unreachable!(),
                                };
                                tracing::error!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connect.connection_id,
                                    reason = %reason,
                                    "resolve_wpass failed"
                                );
                                let failed_msg = McvMessage::new_notification(
                                    MessageType::ConnectFailed,
                                    MessageSource::Plugin {
                                        plugin_id: plugin.logical_plugin_id.clone(),
                                    },
                                    MessageDestination::Core,
                                    serde_json::to_value(ConnectFailedPayload {
                                        connection_id: connect.connection_id,
                                        reason,
                                    })
                                    .unwrap(),
                                );
                                TwicasPlugin::send_message(ctx, failed_msg).await;
                                return Ok(());
                            }
                        }
                    }
                    _ => (None, None),
                };

            tracing::info!(
                target: "mcv::plugin-twicas",
                connection_id = %connect.connection_id,
                url = %input_extra.url,
                user_name = %user_name,
                has_wpass = wpass.is_some(),
                "Connect prerequisites resolved"
            );

            let connected = McvMessage::new_notification(
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
            TwicasPlugin::send_message(ctx.clone(), connected).await;

            conn.connect(
                ctx,
                plugin.logical_plugin_id.clone(),
                &user_name,
                wpass.as_deref(),
                cs_session_id_from_wpass,
                session_client,
                jar,
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
        MessageType::SendComment => {
            let payload: SendCommentPayload = parse_payload(&message.payload)?;
            let conn = match plugin.connections.get(&payload.connection_id) {
                Some(v) => v,
                None => {
                    tracing::warn!(
                        target: "mcv::plugin-twicas",
                        connection_id = %payload.connection_id,
                        "SendComment: connection not found"
                    );
                    return Ok(());
                }
            };
            let anonymous = payload
                .extra
                .get("anonymous")
                .and_then(|v| v.as_bool())
                .unwrap_or(false);
            if let Err(e) = conn.post_comment(&payload.text, anonymous).await {
                tracing::warn!(
                    target: "mcv::plugin-twicas",
                    connection_id = %payload.connection_id,
                    error = %e,
                    "コメント投稿失敗"
                );
            }
        }
        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = Connection::extract_user_name(&payload.url).is_some();
            let site_id = supported.then(|| {
                mcv_common::SiteId::new("ツイキャス", "8cb52621-4f4f-4337-b6a7-8b4a436d91d7")
            });
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            TwicasPlugin::send_message(ctx, response).await;
        }
        _ => {}
    }
    Ok(())
}

#[allow(clippy::result_large_err)]
fn parse_payload<T>(payload: &serde_json::Value) -> Result<T, mcv_plugin_telemetry::TracingError>
where
    T: DeserializeOwned,
{
    let result = T::deserialize(payload);
    match result {
        Ok(v) => Ok(v),
        Err(e) => Err(mcv_plugin_telemetry::capture_context!(
            "payloadが復元できない",
            type = type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),
        )
        .into()),
    }
}

#[derive(Deserialize)]
struct Input {
    url: String,
    /// プライベート配信の合言葉。公開配信の場合は `None` または空文字。
    #[serde(default)]
    password: Option<String>,
}
