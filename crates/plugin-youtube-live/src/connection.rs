//! YouTube Live接続管理
//!
//! 個々のYouTube Live配信への接続を管理し、
//! ライブチャットメッセージを定期的に取得します。

use mcv_messages::{
    ChannelId, CommentReceivedPayload, Cookie as McvCookie, DisconnectedPayload, McvEnvelope,
    Message as McvMessage, MessageDestination, MessagePart as McvMessagePart, MessageSource,
    MessageType, ProviderBadge, ProviderContent, ProviderMessage, ProviderMessageKind,
    ProviderSender, ServiceId,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::task::JoinHandle;
use tokio::time::{sleep, Duration};
use uuid::Uuid;
use youtube_live_lib::{
    extract_ytcfg, get_live_chat, get_live_chat_messages, get_yt_initial_data, Action,
    Continuation, LiveChatTextMessage, MessagePart, Vid,
};

use crate::video_id::extract_video_id;
use crate::YouTubeLivePlugin;

/// LiveChatTextMessageをProviderMessageに変換
fn convert_to_provider_message(msg: &LiveChatTextMessage) -> ProviderMessage {
    // message_partsをMcvMessagePartに変換
    let text = msg
        .message_parts
        .iter()
        .filter_map(|part| match part {
            MessagePart::Text(s) => Some(McvMessagePart::Text { text: s.clone() }),
            MessagePart::Emoji(emoji) => {
                // 絵文字を画像として扱う (最初のサムネイルを使用)
                emoji
                    .thumbnails
                    .first()
                    .map(|thumbnail| McvMessagePart::Image {
                        url: thumbnail.url.clone(),
                        width: Some(thumbnail.width as u32),
                        height: Some(thumbnail.height as u32),
                        alt: Some(emoji.label.clone()),
                    })
            }
        })
        .collect::<Vec<_>>();

    // author_badgesをProviderBadgeに変換
    let badges = msg
        .author_badges
        .iter()
        .map(|badge| ProviderBadge {
            id: badge.tooltip.clone(),
            name: badge.tooltip.clone(),
            image_url: badge.thumbnails.first().map(|t| t.url.clone()),
        })
        .collect::<Vec<_>>();

    // timestamp_usecをi64に変換 (マイクロ秒 → 秒)
    let timestamp = msg.timestamp_usec.parse::<i64>().unwrap_or(0) / 1000 / 1000;

    // idはtimestamp_usecを使用 (一意性を保証)
    let id = msg.timestamp_usec.clone();

    ProviderMessage {
        id: id.clone(),
        platform_message_id: Some(id),
        service: ServiceId("youtube".to_string()),
        channel: ChannelId("".to_string()),
        sender: ProviderSender {
            id: String::new(), // TODO: author_external_channel_idを取得する必要がある
            display_name: vec![McvMessagePart::Text {
                text: msg.author_name.clone(),
            }],
            badges,
            role: None,
        },
        timestamp,
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text { text },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

/// YouTube Live配信への接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: id.clone(),
            cancel_tx: None,
            task: None,
            running: false,
        }
    }

    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: Uuid,
        url: &str,
        cookies: Vec<McvCookie>,
    ) {
        tracing::trace!(
            target: "mcv::plugin-youtube-live",
            url = url,
            cookie_count = cookies.len(),
            "connect()"
        );
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-youtube-live",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }
        let vid = match extract_video_id(&url) {
            Some(a) => a,
            None => {
                tracing::debug!(
                    target: "mcv::plugin-youtube-live",
                    url = url,
                    "vidの抽出に失敗"
                );
                return;
            }
        };
        tracing::trace!(
            target: "mcv::plugin-youtube-live",
            vid = vid,
            "vidの抽出に成功"
        );

        let (cancel_tx, mut cancel_rx) = watch::channel(false);
        let connection_id = self.id;
        let vid = Vid::new(vid);
        let cookie_count = cookies.len();

        let task = tokio::spawn(async move {
            tracing::debug!(
                target: "mcv::plugin-youtube-live",
                connection_id = %connection_id,
                cookie_count = cookie_count,
                "Connection started with resolved cookies"
            );
            // TODO: youtube_live_libのHTTPリクエストへcookieを反映する
            let live_chat = match get_live_chat(&vid).await {
                Ok(v) => v,
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        error = %e,
                        raw = format!("{:?}", e.context()),
                        "Failed to get live chat"
                    );
                    return;
                }
            };
            let yt_initial_data = match get_yt_initial_data(&live_chat).await {
                Ok(v) => v,
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        error = %e,
                        raw = format!("{:?}", e.context()),
                        "Failed to get ytInitialData"
                    );
                    return;
                }
            };
            let ytcfg = match extract_ytcfg(&live_chat) {
                Ok(v) => v,
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        error = %e,
                        "Failed to extract ytcfg"
                    );
                    return;
                }
            };

            // YtInitialData の全 actions をまとめて1つの McvEnvelope に収める
            let provider_messages: Vec<ProviderMessage> = yt_initial_data
                .actions()
                .iter()
                .filter_map(|action| match action {
                    Action::LiveChatTextMessage1(msg) => Some(convert_to_provider_message(msg)),
                    Action::ParseError(raw) => {
                        tracing::error!(
                            target: "mcv::plugin-youtube-live",
                            connection_id = %connection_id,
                            raw = raw,
                            "Failed to parse action"
                        );
                        None
                    }
                    _ => None,
                })
                .collect();
            if !provider_messages.is_empty() {
                let envelope = McvEnvelope {
                    event_id: Uuid::new_v4(),
                    connection_id,
                    messages: provider_messages,
                    received_at: chrono::Utc::now().timestamp(),
                    raw_message: Some(yt_initial_data.raw().to_owned()),
                };
                let payload = CommentReceivedPayload {
                    connection_id,
                    envelope,
                };
                let message = McvMessage::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(payload).unwrap(),
                );
                YouTubeLivePlugin::send_message(ctx.clone(), message).await;
            }

            let mut next_continuation: Continuation = yt_initial_data.continuation().to_owned();
            let mut error_count = 0usize;

            loop {
                tokio::select! {
                    _ = cancel_rx.changed() => {
                        tracing::info!(
                            target: "mcv::plugin-youtube-live",
                            connection_id = %connection_id,
                            "Fetch loop cancelled"
                        );
                        break;
                    }
                    _ = sleep(Duration::from_secs(5)) => {}
                }

                if *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        "Fetch loop cancelled"
                    );
                    break;
                }

                match get_live_chat_messages(&vid, &ytcfg, &next_continuation).await {
                    Ok((maybe_cont, actions, raw_body)) => {
                        // ポーリング1レスポンス分の actions をまとめて1つの McvEnvelope に収める
                        let provider_messages: Vec<ProviderMessage> = actions
                            .iter()
                            .filter_map(|action| match action {
                                Action::LiveChatTextMessage1(msg) => {
                                    Some(convert_to_provider_message(msg))
                                }
                                Action::ParseError(raw) => {
                                    tracing::error!(
                                        target: "mcv::plugin-youtube-live",
                                        connection_id = %connection_id,
                                        raw = raw,
                                        "Failed to parse action"
                                    );
                                    None
                                }
                                _ => None,
                            })
                            .collect();
                        if !provider_messages.is_empty() {
                            let envelope = McvEnvelope {
                                event_id: Uuid::new_v4(),
                                connection_id,
                                messages: provider_messages,
                                received_at: chrono::Utc::now().timestamp(),
                                raw_message: Some(raw_body),
                            };
                            let payload = CommentReceivedPayload {
                                connection_id,
                                envelope,
                            };
                            let message = McvMessage::new_notification(
                                MessageType::CommentReceived,
                                MessageSource::Plugin {
                                    plugin_id: logical_plugin_id,
                                },
                                MessageDestination::Core,
                                serde_json::to_value(payload).unwrap(),
                            );
                            YouTubeLivePlugin::send_message(ctx.clone(), message).await;
                        }

                        if let Some(c) = maybe_cont {
                            next_continuation = c;
                            error_count = 0;
                        } else {
                            tracing::info!(
                                target: "mcv::plugin-youtube-live",
                                connection_id = %connection_id,
                                "No continuation. Stopping fetch loop."
                            );
                            break;
                        }
                    }
                    Err(e) => {
                        error_count += 1;
                        tracing::warn!(
                            target: "mcv::plugin-youtube-live",
                            connection_id = %connection_id,
                            error = %e,
                            error_count = error_count,
                            "Failed to get live chat messages"
                        );
                        if error_count >= 3 {
                            tracing::info!(
                                target: "mcv::plugin-youtube-live",
                                connection_id = %connection_id,
                                "Too many consecutive errors. Stopping fetch loop."
                            );
                            break;
                        }
                    }
                }
            }

            // ループを抜けた = 接続終了
            // CoreにDisconnectedメッセージを送信
            let message = McvMessage::new_notification(
                MessageType::Disconnected,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
            );
            YouTubeLivePlugin::send_message(ctx, message).await;
        });

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        // abort()を使わず、cancel_txでタスクを正常終了させる
        // これにより、loopを抜けた後のDisconnectedメッセージ送信が実行される
        self.cancel_tx = None;
        self.task = None;
        self.running = false;
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

        // 接続していない状態でstop()を呼んでもパニックしないことを確認
        conn.stop();

        assert!(!conn.running);
        assert!(conn.cancel_tx.is_none());
        assert!(conn.task.is_none());
    }

    #[test]
    fn test_connection_multiple_stop_calls() {
        let id = Uuid::new_v4();
        let mut conn = Connection::new(&id);

        // 複数回stop()を呼んでも安全であることを確認
        conn.stop();
        conn.stop();
        conn.stop();

        assert!(!conn.running);
    }
}
