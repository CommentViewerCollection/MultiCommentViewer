//! Kick.com チャット接続管理
//!
//! Pusher WebSocket 経由で Kick.com のチャットルームに接続し、
//! コメントを受信して Core に転送します。

use futures_util::{stream::SplitSink, FutureExt, SinkExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, PluginId, ProviderBadge,
    ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
    StreamMetadataPayload, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::Deserialize;
use tokio::net::TcpStream;
use tokio::sync::watch;
use tokio::task::JoinHandle;
use tokio::time::{timeout, Duration};
use tokio_tungstenite::{
    connect_async, tungstenite::Message as WsMessage, MaybeTlsStream, WebSocketStream,
};
use uuid::Uuid;

use crate::KickPlugin;

const PUSHER_URL: &str =
    "wss://ws-us2.pusher.com/app/32cbd69e4b950bf97679?protocol=7&client=js&version=8.4.0&flash=false";

type WsWrite = SplitSink<WebSocketStream<MaybeTlsStream<TcpStream>>, WsMessage>;

/// Pusher WebSocket メッセージの外枠
#[derive(Deserialize)]
struct PusherMessage {
    event: String,
    data: String, // 二重エンコードされた JSON 文字列
    #[allow(dead_code)]
    channel: Option<String>,
}

/// App\\Events\\ChatMessageEvent の data フィールド
#[derive(Deserialize)]
struct KickChatEvent {
    id: String,
    content: String,
    created_at: String,
    sender: KickEventSender,
}

#[derive(Deserialize)]
struct KickIdentity {
    #[allow(dead_code)]
    color: Option<String>,
    #[serde(default)]
    badges: Vec<KickBadge>,
}

#[derive(Deserialize)]
struct KickBadge {
    #[serde(rename = "type")]
    badge_type: String,
    text: String,
    count: Option<u32>,
}

#[derive(Deserialize)]
struct KickEventSender {
    id: u64,
    username: String,
    #[allow(dead_code)]
    slug: String,
    identity: Option<KickIdentity>,
}

fn parse_timestamp(created_at: &str) -> i64 {
    chrono::DateTime::parse_from_rfc3339(created_at)
        .map(|dt| dt.timestamp())
        .unwrap_or_else(|_| chrono::Utc::now().timestamp())
}

/// "2026-03-17 05:12:57" 形式（UTC）の文字列を Unix 秒に変換する
fn parse_kick_start_time(s: &str) -> Option<i64> {
    chrono::NaiveDateTime::parse_from_str(s, "%Y-%m-%d %H:%M:%S")
        .ok()
        .map(|dt| dt.and_utc().timestamp())
}

fn push_text_part(parts: &mut Vec<MessagePart>, text: &str) {
    if text.is_empty() {
        return;
    }
    if let Some(MessagePart::Text { text: last }) = parts.last_mut() {
        last.push_str(text);
    } else {
        parts.push(MessagePart::Text {
            text: text.to_string(),
        });
    }
}

pub(crate) fn parse_kick_message_parts(content: &str) -> Vec<MessagePart> {
    const PREFIX: &str = "[emote:";
    let mut parts = Vec::new();
    let mut cursor = 0usize;

    while cursor < content.len() {
        let found = content[cursor..].find(PREFIX);
        let Some(rel_start) = found else {
            push_text_part(&mut parts, &content[cursor..]);
            break;
        };
        let start = cursor + rel_start;
        push_text_part(&mut parts, &content[cursor..start]);

        let emote_body_start = start + PREFIX.len();
        let Some(rel_colon) = content[emote_body_start..].find(':') else {
            push_text_part(&mut parts, &content[start..start + 1]);
            cursor = start + 1;
            continue;
        };
        let colon_pos = emote_body_start + rel_colon;
        let emote_id = &content[emote_body_start..colon_pos];
        if emote_id.is_empty() || !emote_id.chars().all(|c| c.is_ascii_digit()) {
            push_text_part(&mut parts, &content[start..start + 1]);
            cursor = start + 1;
            continue;
        }

        let name_start = colon_pos + 1;
        let Some(rel_end) = content[name_start..].find(']') else {
            push_text_part(&mut parts, &content[start..start + 1]);
            cursor = start + 1;
            continue;
        };
        let end = name_start + rel_end;
        let emote_name = &content[name_start..end];

        parts.push(MessagePart::Image {
            url: format!("https://files.kick.com/emotes/{emote_id}/fullsize"),
            width: None,
            height: None,
            alt: (!emote_name.is_empty()).then(|| emote_name.to_string()),
        });
        cursor = end + 1;
    }

    parts
}

fn kick_badges_to_provider(
    kick_badges: &[KickBadge],
    subscriber_badges: &[crate::api::KickSubscriberBadge],
) -> Vec<ProviderBadge> {
    kick_badges
        .iter()
        .map(|badge| {
            let image_url = if badge.badge_type == "subscriber" {
                // チャンネル固有のサブスクライバーバッジ画像を months で検索
                let months = badge.count.unwrap_or(1);
                let channel_url = subscriber_badges
                    .iter()
                    .filter(|sb| sb.months <= months)
                    .max_by_key(|sb| sb.months)
                    .map(|sb| sb.badge_image.src.clone());
                // チャンネル固有 URL がなければ汎用 SVG にフォールバック
                channel_url.or_else(|| crate::badge_svgs::badge_image_url("subscriber"))
            } else {
                crate::badge_svgs::badge_image_url(&badge.badge_type)
            };
            ProviderBadge {
                id: badge.badge_type.clone(),
                name: badge.text.clone(),
                image_url,
            }
        })
        .collect()
}

pub(crate) fn build_provider_message(
    channel_id: &str,
    platform_message_id: String,
    sender_id: u64,
    sender_username: String,
    content: &str,
    created_at: &str,
    kind: ProviderMessageKind,
    badges: Vec<ProviderBadge>,
) -> ProviderMessage {
    ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: Some(platform_message_id),
        service: ServiceId("kick".to_string()),
        channel: ChannelId(channel_id.to_string()),
        sender: ProviderSender {
            id: sender_id.to_string(),
            display_name: vec![MessagePart::Text {
                text: sender_username,
            }],
            badges,
            role: None,
            avatar_url: None,
        },
        timestamp: parse_timestamp(created_at),
        kind,
        content: ProviderContent::Text {
            text: parse_kick_message_parts(content),
        },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

/// Kick チャットルームへの接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            task: None,
            running: false,
        }
    }

    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: Uuid,
        chatroom_id: u64,
        subscriber_badges: Vec<crate::api::KickSubscriberBadge>,
        channel_slug: String,
        cookie_header: String,
    ) {
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-kick",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }

        let (cancel_tx, cancel_rx) = watch::channel(false);
        let connection_id = self.id;

        let task = match Self::start_connection_task(
            ctx,
            logical_plugin_id,
            connection_id,
            chatroom_id,
            cancel_rx,
            subscriber_badges,
            channel_slug,
            cookie_header,
        ) {
            Some(task) => task,
            None => return,
        };

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    fn start_connection_task(
        ctx: PluginContext,
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        chatroom_id: u64,
        mut cancel_rx: watch::Receiver<bool>,
        subscriber_badges: Vec<crate::api::KickSubscriberBadge>,
        channel_slug: String,
        cookie_header: String,
    ) -> Option<JoinHandle<()>> {
        let runtime_handle = match tokio::runtime::Handle::try_current() {
            Ok(handle) => handle,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    error = %e,
                    "Tokio runtime is not available; cannot spawn Kick task"
                );
                return None;
            }
        };

        let task = runtime_handle.spawn(async move {
            let task_result = std::panic::AssertUnwindSafe(async {
                let run_result: Result<(), ()> = async {
                    tracing::info!(
                        target: "mcv::plugin-kick",
                        connection_id = %connection_id,
                        chatroom_id = chatroom_id,
                        "Pusher WebSocket に接続中"
                    );

                    let connect_result =
                        timeout(Duration::from_secs(15), connect_async(PUSHER_URL)).await;
                    let (ws_stream, _response) = match connect_result {
                        Err(_) => {
                            tracing::error!(
                                target: "mcv::plugin-kick",
                                connection_id = %connection_id,
                                "Pusher WebSocket 接続タイムアウト"
                            );
                            return Err(());
                        }
                        Ok(Err(e)) => {
                            tracing::error!(
                                target: "mcv::plugin-kick",
                                connection_id = %connection_id,
                                error = %e,
                                "Pusher WebSocket 接続失敗"
                            );
                            return Err(());
                        }
                        Ok(Ok(v)) => v,
                    };

                    tracing::info!(
                        target: "mcv::plugin-kick",
                        connection_id = %connection_id,
                        "Pusher WebSocket 接続成功"
                    );

                    let (mut write, mut read) = ws_stream.split();

                    // 初回は 60 秒後、以降 60 秒ごとにメタデータをポーリング
                    let meta_start = tokio::time::Instant::now() + Duration::from_secs(60);
                    let mut metadata_interval =
                        tokio::time::interval_at(meta_start, Duration::from_secs(60));

                    loop {
                        tokio::select! {
                            _ = cancel_rx.changed() => {
                                if *cancel_rx.borrow() {
                                    tracing::info!(
                                        target: "mcv::plugin-kick",
                                        connection_id = %connection_id,
                                        "Kick WebSocket ループをキャンセル"
                                    );
                                    break;
                                }
                            }
                            recv = read.next() => {
                                match recv {
                                    Some(Ok(WsMessage::Text(text))) => {
                                        let should_continue = Self::handle_text_message(
                                            ctx.clone(),
                                            logical_plugin_id,
                                            connection_id,
                                            chatroom_id,
                                            &mut write,
                                            text.to_string(),
                                            &subscriber_badges,
                                        ).await;
                                        if !should_continue {
                                            break;
                                        }
                                    }
                                    Some(Ok(WsMessage::Close(frame))) => {
                                        tracing::info!(
                                            target: "mcv::plugin-kick",
                                            connection_id = %connection_id,
                                            close_frame = ?frame,
                                            "WebSocket がサーバーにより切断された"
                                        );
                                        break;
                                    }
                                    Some(Ok(_)) => {}
                                    Some(Err(e)) => {
                                        tracing::error!(
                                            target: "mcv::plugin-kick",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "WebSocket 受信エラー"
                                        );
                                        break;
                                    }
                                    None => {
                                        tracing::info!(
                                            target: "mcv::plugin-kick",
                                            connection_id = %connection_id,
                                            "WebSocket ストリーム終了"
                                        );
                                        break;
                                    }
                                }
                            }
                            _ = metadata_interval.tick() => {
                                fetch_and_send_metadata(
                                    ctx.clone(),
                                    logical_plugin_id,
                                    connection_id,
                                    &channel_slug,
                                    &cookie_header,
                                ).await;
                            }
                        }
                    }

                    Ok(())
                }
                .await;

                if run_result.is_err() {
                    tracing::debug!(
                        target: "mcv::plugin-kick",
                        connection_id = %connection_id,
                        "Kick タスクがエラーにより終了"
                    );
                }

                // 切断前にアカウント情報をクリア
                let clear_account = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: PluginId::new(logical_plugin_id.to_string()),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id,
                        account: None,
                    })
                    .unwrap(),
                );
                KickPlugin::send_message(ctx.clone(), clear_account).await;

                // タスク終了時に必ず Disconnected を送信
                let message = McvMessage::new_notification(
                    MessageType::Disconnected,
                    MessageSource::Plugin {
                        plugin_id: PluginId::new(logical_plugin_id.to_string()),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                );
                KickPlugin::send_message(ctx.clone(), message).await;
            })
            .catch_unwind()
            .await;

            if let Err(panic_payload) = task_result {
                let panic_message = if let Some(s) = panic_payload.downcast_ref::<&str>() {
                    s.to_string()
                } else if let Some(s) = panic_payload.downcast_ref::<String>() {
                    s.clone()
                } else {
                    "unknown panic payload".to_string()
                };

                tracing::error!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    panic = %panic_message,
                    "Kick タスクがパニックした"
                );
            }
        });

        Some(task)
    }

    async fn handle_text_message(
        ctx: PluginContext,
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        chatroom_id: u64,
        write: &mut WsWrite,
        raw_text: String,
        subscriber_badges: &[crate::api::KickSubscriberBadge],
    ) -> bool {
        let pusher_msg: PusherMessage = match serde_json::from_str(&raw_text) {
            Ok(m) => m,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    error = %e,
                    raw = raw_text,
                    "Pusher メッセージのパースに失敗"
                );
                return true;
            }
        };

        match pusher_msg.event.as_str() {
            "pusher:connection_established" => {
                // チャットルームを subscribe
                let subscribe = serde_json::json!({
                    "event": "pusher:subscribe",
                    "data": {
                        "auth": "",
                        "channel": format!("chatrooms.{chatroom_id}.v2")
                    }
                });
                let subscribe_text = subscribe.to_string();
                tracing::info!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    chatroom_id = chatroom_id,
                    "チャットルームに subscribe"
                );
                if let Err(e) = write.send(WsMessage::Text(subscribe_text.into())).await {
                    tracing::error!(
                        target: "mcv::plugin-kick",
                        connection_id = %connection_id,
                        error = %e,
                        "subscribe 送信失敗"
                    );
                    return false;
                }
            }
            "App\\Events\\ChatMessageEvent" => {
                tracing::trace!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    raw_data = %pusher_msg.data,
                    "ChatMessageEvent raw data"
                );
                let chat_event: KickChatEvent = match serde_json::from_str(&pusher_msg.data) {
                    Ok(e) => e,
                    Err(e) => {
                        tracing::warn!(
                            target: "mcv::plugin-kick",
                            connection_id = %connection_id,
                            error = %e,
                            raw_data = %pusher_msg.data,
                            "ChatMessageEvent のパースに失敗"
                        );
                        return true;
                    }
                };

                let kick_badges = chat_event
                    .sender
                    .identity
                    .as_ref()
                    .map(|id| id.badges.as_slice())
                    .unwrap_or_default();
                tracing::debug!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    sender = %chat_event.sender.username,
                    has_identity = chat_event.sender.identity.is_some(),
                    kick_badge_count = kick_badges.len(),
                    kick_badge_types = ?kick_badges.iter().map(|b| &b.badge_type).collect::<Vec<_>>(),
                    "ChatMessageEvent バッジ解析"
                );
                let badges = kick_badges_to_provider(kick_badges, subscriber_badges);
                tracing::debug!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    provider_badge_count = badges.len(),
                    "ProviderBadge 生成完了"
                );

                let provider_msg = build_provider_message(
                    &chatroom_id.to_string(),
                    chat_event.id,
                    chat_event.sender.id,
                    chat_event.sender.username,
                    &chat_event.content,
                    &chat_event.created_at,
                    ProviderMessageKind::Chat,
                    badges,
                );

                let envelope = McvEnvelope {
                    event_id: Uuid::new_v4(),
                    connection_id,
                    messages: vec![provider_msg],
                    received_at: chrono::Utc::now().timestamp(),
                    raw_message: Some(raw_text),
                };

                let comment_message = McvMessage::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Plugin {
                        plugin_id: PluginId::new(logical_plugin_id.to_string()),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(CommentReceivedPayload {
                        connection_id,
                        envelope,
                    })
                    .unwrap(),
                );
                KickPlugin::send_message(ctx, comment_message).await;
            }
            other => {
                tracing::trace!(
                    target: "mcv::plugin-kick",
                    connection_id = %connection_id,
                    event = other,
                    "未処理の Pusher イベント"
                );
            }
        }

        true
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        self.cancel_tx = None;
        self.task = None;
        self.running = false;
    }
}

/// チャンネル情報を取得して StreamMetadata を Core に送信する
async fn fetch_and_send_metadata(
    ctx: PluginContext,
    logical_plugin_id: Uuid,
    connection_id: Uuid,
    channel_slug: &str,
    cookie_header: &str,
) {
    match crate::api::fetch_channel(channel_slug, cookie_header).await {
        Ok(info) => {
            if let Some(ls) = &info.livestream {
                if !ls.is_live() {
                    return;
                }
                let start_time = ls.start_time.as_deref().and_then(parse_kick_start_time);
                let payload = StreamMetadataPayload {
                    connection_id,
                    title: ls.session_title.clone(),
                    viewer_count: ls.viewer_count,
                    total_viewer_count: None,
                    start_time,
                    others: None,
                };
                let msg = McvMessage::new_notification(
                    MessageType::StreamMetadata,
                    MessageSource::Plugin {
                        plugin_id: PluginId::new(logical_plugin_id.to_string()),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(payload).unwrap(),
                );
                KickPlugin::send_message(ctx, msg).await;
            }
        }
        Err(e) => {
            tracing::debug!(
                target: "mcv::plugin-kick",
                connection_id = %connection_id,
                error = %e,
                "StreamMetadata ポーリング中に API 取得失敗"
            );
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_connection_new() {
        let id = Uuid::new_v4();
        let conn = Connection::new(&id);
        assert_eq!(conn.id, id);
        assert!(!conn.running);
        assert!(conn.cancel_tx.is_none());
        assert!(conn.task.is_none());
    }

    #[test]
    fn test_connection_stop_when_not_running() {
        let id = Uuid::new_v4();
        let mut conn = Connection::new(&id);
        conn.stop();
        assert!(!conn.running);
        assert!(conn.cancel_tx.is_none());
        assert!(conn.task.is_none());
    }

    #[test]
    fn test_connection_multiple_stop_calls() {
        let id = Uuid::new_v4();
        let mut conn = Connection::new(&id);
        conn.stop();
        conn.stop();
        conn.stop();
        assert!(!conn.running);
    }

    #[test]
    fn test_parse_kick_message_parts_with_emote() {
        let parts = parse_kick_message_parts("[emote:3467766:test] hello");
        assert_eq!(parts.len(), 2);
        assert_eq!(
            parts[0],
            MessagePart::Image {
                url: "https://files.kick.com/emotes/3467766/fullsize".to_string(),
                width: None,
                height: None,
                alt: Some("test".to_string()),
            }
        );
        assert_eq!(
            parts[1],
            MessagePart::Text {
                text: " hello".to_string()
            }
        );
    }
}
