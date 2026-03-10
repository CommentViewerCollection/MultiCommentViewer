//! YouTube Live接続管理
//!
//! 個々のYouTube Live配信への接続を管理し、
//! ライブチャットメッセージを定期的に取得します。

use mcv_messages::{
    AccountInfo, ChannelId, CommentReceivedPayload, Cookie as McvCookie, DisconnectedPayload,
    McvEnvelope, Message as McvMessage, MessageDestination, MessagePart as McvMessagePart,
    MessageSource, MessageType, MonetaryInfo, Money, ProviderBadge, ProviderContent,
    ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId, SystemKind,
    UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use std::sync::Arc;
use tokio::sync::{watch, RwLock};
use tokio::task::JoinHandle;
use tokio::time::{sleep, Duration};
use uuid::Uuid;
use youtube_live_lib::{
    extract_ytcfg, get_live_chat, get_live_chat_messages, get_yt_initial_data, Action,
    Continuation, LiveChatPaidMessage, LiveChatTextMessage, MessagePart, Vid,
};

/// コメント投稿に必要なデータ（接続確立後に設定される）
struct CommentPostingData {
    cookies: Vec<youtube_live_lib::Cookie>,
    ytcfg: youtube_live_lib::Ytcfg,
    send_message_params: String,
}

use crate::video_id::extract_video_id;
use crate::YouTubeLivePlugin;

/// 金額テキスト（例: "￥8,000"、"$10.00"）から Money 構造体を生成する
fn parse_money(text: &str) -> Money {
    let text = text.trim();
    // 通貨記号を検出してISO 4217コードと数値文字列に分解
    let (currency, rest): (&str, &str) = if text.starts_with('¥') || text.starts_with('￥') {
        ("JPY", text.trim_start_matches(['¥', '￥']))
    } else if text.starts_with("HK$") {
        ("HKD", &text[3..])
    } else if text.starts_with("NT$") {
        ("TWD", &text[3..])
    } else if text.starts_with("A$") {
        ("AUD", &text[2..])
    } else if text.starts_with("C$") {
        ("CAD", &text[2..])
    } else if text.starts_with('$') {
        ("USD", text.trim_start_matches('$'))
    } else if text.starts_with('€') {
        ("EUR", text.trim_start_matches('€'))
    } else if text.starts_with('£') {
        ("GBP", text.trim_start_matches('£'))
    } else if text.ends_with('₩') {
        let numeric: String = text
            .trim_end_matches('₩')
            .chars()
            .filter(|c| c.is_ascii_digit())
            .collect();
        let value_minor = numeric.parse::<i64>().unwrap_or(0);
        return Money {
            currency: "KRW".to_string(),
            value_minor,
        };
    } else {
        ("", text)
    };
    // カンマ・空白を除去して数値をパース
    let cleaned: String = rest
        .chars()
        .filter(|c| c.is_ascii_digit() || *c == '.')
        .collect();
    let value_minor = match currency {
        // 少数部なし (JPY, KRW, TWD)
        "JPY" | "TWD" => cleaned.parse::<i64>().unwrap_or(0),
        // 少数2桁 (USD, EUR, GBP, AUD, CAD, HKD 等)
        _ => cleaned
            .parse::<f64>()
            .map(|v| (v * 100.0).round() as i64)
            .unwrap_or(0),
    };
    Money {
        currency: currency.to_string(),
        value_minor,
    }
}

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
            id: msg.author_external_channel_id.clone(),
            display_name: vec![McvMessagePart::Text {
                text: msg.author_name.clone(),
            }],
            badges,
            role: None,
            avatar_url: msg.author_photo_url.clone(),
        },
        timestamp,
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text { text },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

/// LiveChatPaidMessage をProviderMessageに変換
fn convert_paid_message_to_provider_message(msg: &LiveChatPaidMessage) -> ProviderMessage {
    let text = msg
        .message_parts
        .iter()
        .filter_map(|part| match part {
            MessagePart::Text(s) => Some(McvMessagePart::Text { text: s.clone() }),
            MessagePart::Emoji(emoji) => {
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

    let badges = msg
        .author_badges
        .iter()
        .map(|badge| ProviderBadge {
            id: badge.tooltip.clone(),
            name: badge.tooltip.clone(),
            image_url: badge.thumbnails.first().map(|t| t.url.clone()),
        })
        .collect::<Vec<_>>();

    let timestamp = msg.timestamp_usec.parse::<i64>().unwrap_or(0) / 1_000_000;
    let id = msg.timestamp_usec.clone();
    let monetary_info = MonetaryInfo {
        amount: parse_money(&msg.purchase_amount_text),
        tier: None,
        recurring: false,
    };

    ProviderMessage {
        id: id.clone(),
        platform_message_id: Some(id),
        service: ServiceId("youtube".to_string()),
        channel: ChannelId("".to_string()),
        sender: ProviderSender {
            id: msg.author_external_channel_id.clone(),
            display_name: vec![McvMessagePart::Text {
                text: msg.author_name.clone(),
            }],
            badges,
            role: None,
            avatar_url: msg.author_photo_url.clone(),
        },
        timestamp,
        kind: ProviderMessageKind::Monetary(monetary_info),
        content: ProviderContent::Text { text },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

/// Action を ProviderMessage に変換する（変換不要なアクションは None を返す）
fn convert_action_to_provider_message(
    action: &Action,
    connection_id: Uuid,
) -> Option<ProviderMessage> {
    match action {
        Action::TextMessage(msg) => Some(convert_to_provider_message(msg)),

        Action::GiftAnnouncement(msg) => {
            let mut provider_msg = convert_to_provider_message(msg);
            provider_msg.kind = ProviderMessageKind::System(SystemKind::GiftAnnouncement);
            Some(provider_msg)
        }

        Action::PaidMessage(msg) => Some(convert_paid_message_to_provider_message(msg)),

        // 承認待ちコメント: System(Placeholder) として追加
        Action::PlaceholderItem(placeholder) => {
            let timestamp = placeholder.timestamp_usec.parse::<i64>().unwrap_or(0) / 1_000_000;
            Some(ProviderMessage {
                id: placeholder.id.clone(),
                platform_message_id: Some(placeholder.id.clone()),
                service: ServiceId("youtube".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: String::new(),
                    display_name: vec![],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::System(SystemKind::Placeholder),
                content: ProviderContent::Empty,
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }

        // 置き換えコメント: System(MessageUpdate) として追加
        Action::ReplaceChatItem(replace_action) => {
            let mut msg = convert_to_provider_message(&replace_action.message);
            msg.kind = ProviderMessageKind::System(SystemKind::MessageUpdate {
                target_message_id: replace_action.target_item_id.clone(),
            });
            Some(msg)
        }

        // 特定コメント削除: System(MessageDelete) として追加
        Action::RemoveChatItem(remove_action) => Some(ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: ServiceId("youtube".to_string()),
            channel: ChannelId("".to_string()),
            sender: ProviderSender {
                id: String::new(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::MessageDelete {
                target_message_id: remove_action.target_item_id.clone(),
            }),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::Value::Null,
        }),

        // ユーザー全コメント削除: System(MessageDeleteAll) として追加
        Action::RemoveChatItemByAuthor(remove_action) => Some(ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: ServiceId("youtube".to_string()),
            channel: ChannelId("".to_string()),
            sender: ProviderSender {
                id: String::new(),
                display_name: vec![],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: 0,
            kind: ProviderMessageKind::System(SystemKind::MessageDeleteAll {
                user_id: remove_action.external_channel_id.clone(),
            }),
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::Value::Null,
        }),

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
    }
}

/// YouTube Live配信への接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
    /// 接続確立後に設定されるコメント投稿用データ
    comment_posting: Arc<RwLock<Option<CommentPostingData>>>,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: id.clone(),
            cancel_tx: None,
            task: None,
            running: false,
            comment_posting: Arc::new(RwLock::new(None)),
        }
    }

    /// YouTube LiveにコメントをYouTube内部API経由で投稿する
    pub(crate) async fn post_comment(&self, text: &str) {
        let posting = self.comment_posting.read().await;
        match posting.as_ref() {
            None => {
                tracing::warn!(
                    target: "mcv::plugin-youtube-live",
                    connection_id = %self.id,
                    "コメント投稿不可: 未接続またはsend_message_params未取得"
                );
            }
            Some(data) => {
                if let Err(e) = youtube_live_lib::send_chat_message(
                    &data.cookies,
                    &data.ytcfg,
                    &data.send_message_params,
                    text,
                )
                .await
                {
                    tracing::error!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %self.id,
                        error = %e,
                        "コメント投稿に失敗"
                    );
                }
            }
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

        // McvCookie → youtube_live_lib::Cookie に変換
        let yt_cookies: Vec<youtube_live_lib::Cookie> = cookies
            .iter()
            .map(|c| youtube_live_lib::Cookie {
                name: c.name.clone(),
                value: c.value.clone(),
            })
            .collect();

        let (cancel_tx, mut cancel_rx) = watch::channel(false);
        let connection_id = self.id;
        let vid = Vid::new(vid);
        let comment_posting = Arc::clone(&self.comment_posting);

        let task = tokio::spawn(async move {
            tracing::debug!(
                target: "mcv::plugin-youtube-live",
                connection_id = %connection_id,
                cookie_count = yt_cookies.len(),
                "Connection started with resolved cookies"
            );
            let live_chat = match get_live_chat(&vid, &yt_cookies).await {
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
            let yt_initial_data = match get_yt_initial_data(&live_chat) {
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
            let mut ytcfg = match extract_ytcfg(&live_chat) {
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

            // コメント投稿用データを設定
            if let Some(params) = yt_initial_data.send_message_params() {
                let mut posting = comment_posting.write().await;
                *posting = Some(CommentPostingData {
                    cookies: yt_cookies.clone(),
                    ytcfg: ytcfg.clone(),
                    send_message_params: params,
                });
                tracing::info!(
                    target: "mcv::plugin-youtube-live",
                    connection_id = %connection_id,
                    "コメント投稿の準備が完了しました"
                );
            } else {
                tracing::warn!(
                    target: "mcv::plugin-youtube-live",
                    connection_id = %connection_id,
                    "send_message_paramsが見つかりません（未ログイン・チャット制限の可能性）"
                );
            }

            // ログイン中のアカウント情報を Core に通知
            if let Some(name) = yt_initial_data.viewer_name() {
                let account = AccountInfo {
                    user_id: name.to_string(),
                    display_name: name.to_string(),
                    avatar_url: yt_initial_data.viewer_avatar_url().map(|s| s.to_string()),
                };
                tracing::info!(
                    target: "mcv::plugin-youtube-live",
                    connection_id = %connection_id,
                    display_name = %name,
                    "視聴者アカウント情報を取得しました"
                );
                let account_msg = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id,
                        account: Some(account),
                    })
                    .unwrap(),
                );
                YouTubeLivePlugin::send_message(ctx.clone(), account_msg).await;
            }

            // YtInitialData の全 actions をまとめて1つの McvEnvelope に収める
            let provider_messages: Vec<ProviderMessage> = yt_initial_data
                .actions()
                .iter()
                .filter_map(|action| convert_action_to_provider_message(action, connection_id))
                .map(|mut msg| {
                    if matches!(msg.kind, ProviderMessageKind::Chat) {
                        msg.kind = ProviderMessageKind::HistoryChat;
                    }
                    msg
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
                // reloadContinuationDataが来た場合、live_chatページを再取得してytcfgとcontinuationをリセット
                if next_continuation.needs_reload {
                    tracing::info!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        "reloadContinuationData received, reloading live_chat page"
                    );
                    let reload_ok = match get_live_chat(&vid, &yt_cookies).await {
                        Ok(new_live_chat) => {
                            let ytcfg_res = extract_ytcfg(&new_live_chat);
                            let initial_res = get_yt_initial_data(&new_live_chat);
                            match (ytcfg_res, initial_res) {
                                (Ok(new_ytcfg), Ok(new_initial)) => {
                                    ytcfg = new_ytcfg;
                                    next_continuation = new_initial.continuation().to_owned();
                                    true
                                }
                                (Err(e), _) => {
                                    tracing::warn!(
                                        target: "mcv::plugin-youtube-live",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "Failed to extract ytcfg on reload"
                                    );
                                    false
                                }
                                (_, Err(e)) => {
                                    tracing::warn!(
                                        target: "mcv::plugin-youtube-live",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "Failed to get ytInitialData on reload"
                                    );
                                    false
                                }
                            }
                        }
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-youtube-live",
                                connection_id = %connection_id,
                                error = %e,
                                "Failed to get live_chat on reload"
                            );
                            false
                        }
                    };
                    if reload_ok {
                        error_count = 0;
                        continue;
                    }
                    error_count += 1;
                    if error_count >= 3 {
                        tracing::info!(
                            target: "mcv::plugin-youtube-live",
                            connection_id = %connection_id,
                            "Too many consecutive reload errors. Stopping fetch loop."
                        );
                        break;
                    }
                    tokio::select! {
                        _ = cancel_rx.changed() => { break; }
                        _ = sleep(Duration::from_secs(5)) => {}
                    }
                    if *cancel_rx.borrow() {
                        break;
                    }
                    continue;
                }

                // YouTubeのAPIが返すtimeoutMsを尊重する（デフォルト5秒、最低500ms〜最大8秒）
                let sleep_ms = next_continuation
                    .timeout_ms
                    .unwrap_or(5000)
                    .clamp(500, 8000);
                tokio::select! {
                    _ = cancel_rx.changed() => {
                        tracing::info!(
                            target: "mcv::plugin-youtube-live",
                            connection_id = %connection_id,
                            "Fetch loop cancelled"
                        );
                        break;
                    }
                    _ = sleep(Duration::from_millis(sleep_ms)) => {}
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
                            .filter_map(|action| {
                                convert_action_to_provider_message(action, connection_id)
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
            // コメント投稿データをクリア
            {
                let mut posting = comment_posting.write().await;
                *posting = None;
            }
            // アカウント情報をクリア
            let clear_account_msg = McvMessage::new_notification(
                MessageType::UpdateConnectionAccount,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(UpdateConnectionAccountPayload {
                    connection_id,
                    account: None,
                })
                .unwrap(),
            );
            YouTubeLivePlugin::send_message(ctx.clone(), clear_account_msg).await;
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
