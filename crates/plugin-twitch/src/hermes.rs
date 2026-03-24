//! Twitch Hermes WebSocket 接続
//!
//! `wss://hermes.twitch.tv/v1` に接続し、チャンネルポイント交換や視聴者数などの
//! PubSub イベントを受信します。
//!
//! ## 接続フロー
//! 1. WebSocket 接続
//! 2. `welcome` メッセージ受信
//! 3. 認証済みの場合: `authenticate` 送信（ack 待たず）
//! 4. `subscribe` を全トピック分送信（ack 待たず）
//! 5. `notification` / `keepalive` を受信し続ける

use futures_util::{SinkExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, McvEnvelope, Message as McvMessage, MessageDestination,
    MessagePart, MessageSource, MessageType, PluginId, ProviderContent, ProviderMessage,
    ProviderMessageKind, ProviderSender, ServiceId, StreamMetadataPayload, SystemKind,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::time::{timeout, Duration};
use tokio_tungstenite::{connect_async, tungstenite::Message as WsMessage};
use uuid::Uuid;

use crate::TwitchPlugin;
use twitch_lib::auth_token::AuthToken;

const HERMES_CLIENT_ID: &str = "kimne78kx3ncx6brgo4mv6wki5h1ko";
const HERMES_URL: &str = "wss://hermes.twitch.tv/v1?clientId=kimne78kx3ncx6brgo4mv6wki5h1ko";

/// 購読するトピックの一覧を生成する。
///
/// - `channel_id`: 配信チャンネルの数値 ID（配信者側）
/// - `user_id`: ログイン中ユーザーの数値 ID。`None` の場合は認証不要トピックのみ購読。
fn build_topics(channel_id: &str, user_id: Option<&str>) -> Vec<String> {
    let mut topics = vec![
        format!("ads.{channel_id}"),
        format!("video-playback-by-id.{channel_id}"),
        format!("feature-consequence-channel.{channel_id}"),
        format!("hype-train-events-v2.{channel_id}"),
        format!("shared-chat-channel-v1.{channel_id}"),
        format!("pv-watch-party-events.{channel_id}"),
        format!("content-policy-properties.{channel_id}"),
        format!("broadcast-settings-update.{channel_id}"),
        format!("content-classification-labels-v1.{channel_id}"),
        format!("community-points-channel-v1.{channel_id}"),
        format!("stream-chat-room-v1.{channel_id}"),
        format!("pinned-chat-updates-v1.{channel_id}"),
        format!("predictions-channel-v1.{channel_id}"),
        format!("polls.{channel_id}"),
    ];

    if let Some(uid) = user_id {
        topics.extend([
            format!("hype-train-events-v1.rewards.{uid}"),
            format!("user-drop-events.{uid}"),
            format!("chatrooms-user-v1.{uid}"),
            format!("user-subscribe-events-v1.{uid}"),
            format!("predictions-user-v1.{uid}"),
            format!("community-points-user-v1.{uid}"),
            format!("follows.{uid}"),
            format!("feature-consequence-user.{uid}"),
            format!("user-properties-update.{uid}"),
            format!("user-preferences-update-v1.{uid}"),
            format!("chat_moderator_actions.{uid}.{channel_id}"),
            format!("channel-unban-requests.{uid}.{channel_id}"),
            format!("automod-queue.{uid}.{channel_id}"),
            format!("user-moderation-notifications.{uid}.{channel_id}"),
            format!("low-trust-users.{uid}.{channel_id}"),
            format!("viewer-milestones.{uid}.{channel_id}"),
            format!("viewer-milestones.{uid}"),
        ]);
    }

    topics
}

/// Hermes WebSocket 接続ループ。
///
/// - `channel_login`: チャンネルのログイン名（URL から取得）
/// - `auth_token`: 認証トークン。`None` の場合は匿名接続（authenticate・ユーザートピック購読なし）
pub(crate) async fn hermes_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: String,
    auth_token: Option<AuthToken>,
    mut cancel_rx: watch::Receiver<bool>,
) {
    // broadcaster_id（数値チャンネル ID）を取得
    let client_id = twitch_lib::ClientId::new(HERMES_CLIENT_ID);
    let channel_id =
        match twitch_lib::fetch_broadcaster_id(&channel_login, &client_id, auth_token.as_ref())
            .await
        {
            Ok(Some(id)) => id,
            Ok(None) => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    channel = %channel_login,
                    "Hermes: broadcaster_id が取得できなかった（チャンネルが存在しない可能性）"
                );
                return;
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    error = %e,
                    "Hermes: broadcaster_id の取得に失敗"
                );
                return;
            }
        };

    // 認証時のみ user_id を取得（validate_token は auth_token が Some の場合のみ呼ぶ）
    let user_id = if let Some(token) = &auth_token {
        match twitch_lib::auth_token::validate_token(token.value()).await {
            Ok(info) => {
                tracing::debug!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    user_id = %info.user_id,
                    "Hermes: user_id 取得成功"
                );
                Some(info.user_id)
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    error = %e,
                    "Hermes: トークン検証失敗、匿名モードで接続"
                );
                None
            }
        }
    } else {
        None
    };

    // WebSocket 接続
    let connect_result = timeout(Duration::from_secs(15), connect_async(HERMES_URL)).await;
    let (ws_stream, _) = match connect_result {
        Err(_) => {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Hermes: 接続タイムアウト"
            );
            return;
        }
        Ok(Err(e)) => {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Hermes: 接続失敗"
            );
            return;
        }
        Ok(Ok(v)) => v,
    };

    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        channel_id = %channel_id,
        has_user_id = user_id.is_some(),
        "Hermes: WebSocket 接続成功"
    );

    let (mut write, mut read) = ws_stream.split();

    // welcome メッセージを待つ（Ping が来たら Pong を返して待ち続ける）
    loop {
        let msg = tokio::select! {
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Hermes: welcome 待機中にキャンセル"
                    );
                    return;
                }
                continue;
            }
            msg = read.next() => msg,
        };

        match msg {
            Some(Ok(WsMessage::Text(ref text))) => {
                tracing::debug!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    raw = %text,
                    "Hermes: welcome 受信"
                );
                break;
            }
            Some(Ok(WsMessage::Ping(data))) => {
                tracing::trace!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    "Hermes: welcome 待機中に Ping 受信、Pong を返送"
                );
                if let Err(e) = write.send(WsMessage::Pong(data)).await {
                    tracing::error!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        error = %e,
                        "Hermes: Pong 送信失敗"
                    );
                    return;
                }
            }
            other => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    raw = ?other,
                    "Hermes: welcome として予期しないメッセージを受信"
                );
                return;
            }
        }
    }

    let now = || chrono::Utc::now().to_rfc3339_opts(chrono::SecondsFormat::Millis, true);

    // authenticate（認証済みの場合のみ）
    if let Some(token) = &auth_token {
        let auth_msg = serde_json::json!({
            "id": Uuid::new_v4().to_string(),
            "type": "authenticate",
            "authenticate": { "token": token.value() },
            "timestamp": now()
        });
        if let Err(e) = write
            .send(WsMessage::Text(auth_msg.to_string().into()))
            .await
        {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Hermes: authenticate 送信失敗"
            );
            return;
        }
    }

    // subscribe（ack を待たずに全トピック送信）
    let topics = build_topics(&channel_id, user_id.as_deref());
    for topic in &topics {
        let sub_msg = serde_json::json!({
            "type": "subscribe",
            "id": Uuid::new_v4().to_string(),
            "subscribe": {
                "id": Uuid::new_v4().to_string(),
                "type": "pubsub",
                "pubsub": { "topic": topic }
            },
            "timestamp": now()
        });
        if let Err(e) = write
            .send(WsMessage::Text(sub_msg.to_string().into()))
            .await
        {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                topic = %topic,
                "Hermes: subscribe 送信失敗"
            );
            return;
        }
    }

    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        topic_count = topics.len(),
        "Hermes: 全トピックの subscribe 送信完了"
    );

    // メインループ
    loop {
        tokio::select! {
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Hermes: キャンセル受信、終了"
                    );
                    break;
                }
            }
            msg = read.next() => {
                match msg {
                    Some(Ok(WsMessage::Text(text))) => {
                        handle_hermes_message(
                            ctx.clone(),
                            logical_plugin_id.clone(),
                            connection_id,
                            &channel_login,
                            &text,
                        )
                        .await;
                    }
                    Some(Ok(WsMessage::Close(frame))) => {
                        tracing::info!(
                            target: "mcv::plugin-twitch",
                            connection_id = %connection_id,
                            close_frame = ?frame,
                            "Hermes: サーバーから切断"
                        );
                        break;
                    }
                    Some(Err(e)) => {
                        tracing::error!(
                            target: "mcv::plugin-twitch",
                            connection_id = %connection_id,
                            error = %e,
                            "Hermes: WebSocket エラー"
                        );
                        break;
                    }
                    None => {
                        tracing::info!(
                            target: "mcv::plugin-twitch",
                            connection_id = %connection_id,
                            "Hermes: ストリーム終了"
                        );
                        break;
                    }
                    _ => {}
                }
            }
        }
    }
}

async fn handle_hermes_message(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: &str,
    text: &str,
) {
    let value: serde_json::Value = match serde_json::from_str(text) {
        Ok(v) => v,
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Hermes: JSON パース失敗"
            );
            return;
        }
    };

    let msg_type = value["type"].as_str().unwrap_or("");
    match msg_type {
        "notification" => {
            handle_notification(ctx, logical_plugin_id, connection_id, channel_login, &value).await;
        }
        "keepalive" => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Hermes: keepalive 受信"
            );
        }
        "subscribeResponse" => {
            let result = value["subscribeResponse"]["result"]
                .as_str()
                .unwrap_or("unknown");
            tracing::debug!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                result,
                "Hermes: subscribeResponse 受信"
            );
        }
        "authenticateResponse" => {
            let result = value["authenticateResponse"]["result"]
                .as_str()
                .unwrap_or("unknown");
            tracing::debug!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                result,
                "Hermes: authenticateResponse 受信"
            );
        }
        other => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                msg_type = other,
                "Hermes: 未処理のメッセージタイプ"
            );
        }
    }
}

async fn handle_notification(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: &str,
    value: &serde_json::Value,
) {
    let notification = &value["notification"];
    if notification["type"].as_str() != Some("pubsub") {
        tracing::trace!(
            target: "mcv::plugin-twitch",
            connection_id = %connection_id,
            notification_type = notification["type"].as_str().unwrap_or(""),
            "Hermes: pubsub 以外の notification"
        );
        return;
    }

    let pubsub_str = match notification["pubsub"].as_str() {
        Some(s) => s,
        None => return,
    };

    let pubsub: serde_json::Value = match serde_json::from_str(pubsub_str) {
        Ok(v) => v,
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Hermes: pubsub JSON パース失敗"
            );
            return;
        }
    };

    let pubsub_type = pubsub["type"].as_str().unwrap_or("");
    match pubsub_type {
        "viewcount" => {
            if let Some(viewers) = pubsub["viewers"].as_u64() {
                let payload = StreamMetadataPayload {
                    connection_id,
                    title: None,
                    viewer_count: Some(viewers),
                    total_viewer_count: None,
                    start_time: None,
                    others: None,
                    clear: None,
                };
                let msg = McvMessage::new_notification(
                    MessageType::StreamMetadata,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(payload).unwrap(),
                );
                TwitchPlugin::send_message(ctx, msg).await;
            }
        }
        "reward-redeemed" => {
            let redemption = &pubsub["data"]["redemption"];
            let user_id = redemption["user"]["id"].as_str().unwrap_or("").to_string();
            let display_name = redemption["user"]["display_name"]
                .as_str()
                .unwrap_or("")
                .to_string();
            let reward_title = redemption["reward"]["title"]
                .as_str()
                .unwrap_or("")
                .to_string();
            let cost = redemption["reward"]["cost"].as_u64().unwrap_or(0);
            let timestamp = redemption["redeemed_at"]
                .as_str()
                .and_then(|s| chrono::DateTime::parse_from_rfc3339(s).ok())
                .map(|dt| dt.timestamp())
                .unwrap_or_else(|| chrono::Utc::now().timestamp());

            let provider_msg = ProviderMessage {
                id: Uuid::new_v4().to_string(),
                platform_message_id: redemption["id"].as_str().map(|s| s.to_string()),
                service: ServiceId("twitch".to_string()),
                channel: ChannelId(channel_login.to_string()),
                sender: ProviderSender {
                    id: user_id,
                    display_name: vec![MessagePart::Text { text: display_name }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::System(SystemKind::Notice),
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text {
                        text: format!("{}を引き換えました ⏱ {}", reward_title, cost),
                    }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            };
            let envelope = McvEnvelope {
                event_id: Uuid::new_v4(),
                connection_id,
                messages: vec![provider_msg],
                received_at: chrono::Utc::now().timestamp(),
                raw_message: None,
            };
            let msg = McvMessage::new_notification(
                MessageType::CommentReceived,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(CommentReceivedPayload {
                    connection_id,
                    envelope,
                })
                .unwrap(),
            );
            TwitchPlugin::send_message(ctx, msg).await;
        }
        other => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                pubsub_type = other,
                pubsub_data = %pubsub,
                "Hermes: 未処理の pubsub タイプ"
            );
        }
    }
}
