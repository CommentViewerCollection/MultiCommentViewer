//! OPENREC.tv チャット WebSocket 接続管理
//!
//! socket.io v2（EIO=3）プロトコルで `wss://chat.openrec.tv/socket.io/` に接続し、
//! CHAT_ADD イベントをコメントとして Core に転送します。
//!
//! 接続フロー:
//! 1. 配信ページ HTML を取得して movieId を抽出（openrec_lib::fetch_stream_info）
//! 2. socket.io WebSocket URL を構築して接続
//! 3. EIO プロトコルのハンドシェイク（open → 40 送信 → ping/pong 応答）
//! 4. Connected を Core に通知
//! 5. REST API で直近の過去コメントを取得して Core に転送（openrec_lib::fetch_chat_history）
//! 6. openrec_lib::parse_socketio_text でリアルタイムイベントを解析して Core に転送

use futures_util::{SinkExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, ConnectFailedPayload, ConnectedPayload, DisconnectedPayload,
    McvEnvelope, Message as McvMessage, MessageDestination, MessagePart, MessageSource,
    MessageType, MonetaryInfo, Money, PluginId, ProviderContent, ProviderMessage,
    ProviderMessageKind, ProviderSender, ServiceId, StreamMetadataPayload, SystemKind, UserRole,
};
use openrec_lib::{
    extract_channel_id, fetch_chat_history, fetch_stream_info, parse_socketio_text,
    ChatHistoryCursor, OpenrecChatData, OpenrecEvent, OpenrecHistoryChat, SocketIoFrame,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::task::JoinHandle;
use tokio::time::{timeout, Duration};
use tokio_tungstenite::{
    connect_async,
    tungstenite::{client::IntoClientRequest, Message as WsMessage},
};
use uuid::Uuid;

/// 1 つの OPENREC 配信への接続を管理する
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    cancel_tx: Option<watch::Sender<bool>>,
    task: Option<JoinHandle<()>>,
    running: bool,
}

impl Connection {
    pub fn new(id: Uuid) -> Self {
        Self {
            id,
            cancel_tx: None,
            task: None,
            running: false,
        }
    }

    pub fn connect(&mut self, ctx: PluginContext, plugin_id: PluginId, url: String) {
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-openrec",
                connection_id = %self.id,
                "connect() を無視（既に接続中）"
            );
            return;
        }

        let (cancel_tx, cancel_rx) = watch::channel(false);
        let conn_id = self.id;

        let task = tokio::spawn(async move {
            run_connection(ctx, plugin_id, conn_id, url, cancel_rx).await;
        });

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    pub fn stop(&mut self) {
        if let Some(tx) = self.cancel_tx.take() {
            let _ = tx.send(true);
        }
        self.task = None;
        self.running = false;
    }
}

// ---------------------------------------------------------------------------
// 接続メインループ
// ---------------------------------------------------------------------------

async fn run_connection(
    ctx: PluginContext,
    plugin_id: PluginId,
    conn_id: Uuid,
    url: String,
    mut cancel_rx: watch::Receiver<bool>,
) {
    // 1. URL から channel_id を取り出す
    let channel_id = match extract_channel_id(&url) {
        Some(id) => id,
        None => {
            tracing::warn!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                url = %url,
                "URL から channel_id を抽出できない"
            );
            send_connect_failed(
                &ctx,
                &plugin_id,
                conn_id,
                "不正なURLです（https://www.openrec.tv/live/<channel_id> 形式が必要）",
            )
            .await;
            return;
        }
    };

    // 2. 配信ページ HTML から movieId を取得
    tracing::info!(
        target: "mcv::plugin-openrec",
        conn_id = %conn_id,
        channel_id = %channel_id,
        "movieId を取得中"
    );

    let stream_info = match fetch_stream_info(&url).await {
        Some(v) => v,
        None => {
            tracing::warn!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                url = %url,
                "movieId の取得失敗（配信中でない可能性あり）"
            );
            send_connect_failed(
                &ctx,
                &plugin_id,
                conn_id,
                "配信の movieId を取得できませんでした（配信中でない可能性があります）",
            )
            .await;
            return;
        }
    };

    let movie_id = stream_info.movie_id;

    if !stream_info.is_live {
        tracing::warn!(
            target: "mcv::plugin-openrec",
            conn_id = %conn_id,
            "isLive=false: 配信が開始されていない可能性があります"
        );
    }

    tracing::info!(
        target: "mcv::plugin-openrec",
        conn_id = %conn_id,
        channel_id = %channel_id,
        movie_id = movie_id,
        "socket.io WebSocket に接続中"
    );

    // 3. socket.io WebSocket URL を構築
    let uuid = Uuid::new_v4();
    let connect_at = chrono::Utc::now().timestamp();
    let connection_uuid = Uuid::new_v4();
    let referrer_encoded = percent_encode(&url);
    let ws_url = format!(
        "wss://chat.openrec.tv/socket.io/?movieId={movie_id}&uuid={uuid}\
         &referrer={referrer_encoded}&connectAt={connect_at}\
         &connectionId={connection_uuid}&isExcludeLiveViewers=true\
         &EIO=3&transport=websocket"
    );

    // 4. WebSocket 接続（chat.openrec.tv は Origin ヘッダーが必須）
    let mut ws_request = ws_url.as_str().into_client_request().unwrap();
    {
        let headers = ws_request.headers_mut();
        headers.insert("Origin", "https://www.openrec.tv".parse().unwrap());
        headers.insert(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36".parse().unwrap(),
        );
    }
    let ws_result = timeout(Duration::from_secs(15), connect_async(ws_request)).await;

    let (ws_stream, _) = match ws_result {
        Err(_) => {
            tracing::error!(target: "mcv::plugin-openrec", conn_id = %conn_id, "WebSocket 接続タイムアウト");
            send_connect_failed(&ctx, &plugin_id, conn_id, "WebSocket 接続タイムアウト").await;
            return;
        }
        Ok(Err(e)) => {
            tracing::error!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                error = %e,
                "WebSocket 接続失敗"
            );
            send_connect_failed(&ctx, &plugin_id, conn_id, &e.to_string()).await;
            return;
        }
        Ok(Ok(v)) => v,
    };

    tracing::info!(
        target: "mcv::plugin-openrec",
        conn_id = %conn_id,
        movie_id = movie_id,
        "WebSocket 接続成功"
    );

    let (mut write, mut read) = ws_stream.split();

    // Connected を Core に通知
    let connected_msg = McvMessage::new_notification(
        MessageType::Connected,
        MessageSource::Plugin {
            plugin_id: plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(ConnectedPayload {
            connection_id: conn_id,
        })
        .unwrap(),
    );
    ctx.send_notification(connected_msg).await.ok();

    // タイトル・配信開始時刻などのメタデータを Core に送信
    if stream_info.title.is_some() || stream_info.start_time.is_some() {
        send_stream_metadata(
            &ctx,
            &plugin_id,
            conn_id,
            stream_info.title,
            stream_info.start_time,
        )
        .await;
    }

    let service_id = ServiceId("openrec".to_string());
    let channel = ChannelId(movie_id.to_string());
    let movie_id_str = movie_id.to_string();

    // 5. 直近の過去コメントを REST API で取得して送信
    // WS 接続後に取得することでリアルタイムコメントの取りこぼしを防ぐ
    // REST API は数値の movie_id ではなくスラッグ（例: "olryvq9qlr2"）を使用する
    let history_id = stream_info.movie_slug.as_deref().unwrap_or(&movie_id_str);
    let now = chrono::Utc::now();
    let to_created_at = now.format("%Y-%m-%dT%H:%M:%S%z").to_string();
    let c = now.timestamp_millis() as u64;
    match fetch_chat_history(
        history_id,
        &ChatHistoryCursor::BeforeTime {
            to_created_at,
            c: Some(c),
        },
        true,
    )
    .await
    {
        Ok(history) => {
            tracing::info!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                count = history.len(),
                "過去コメントを取得"
            );
            for chat in history {
                if let Some(provider_msg) =
                    build_history_message(chat, service_id.clone(), channel.clone())
                {
                    send_comment(&ctx, &plugin_id, conn_id, provider_msg).await;
                }
            }
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                error = %e,
                "過去コメントの取得に失敗（リアルタイム受信は継続）"
            );
        }
    }

    // 6. メッセージループ
    // ブラウザの動作: クライアントが 25 秒ごとに "2"（PING）を送り、サーバーが "3"（PONG）を返す
    let mut ping_interval = tokio::time::interval(Duration::from_secs(25));
    ping_interval.tick().await; // 接続直後の即時 tick をスキップ

    loop {
        tokio::select! {
            _ = cancel_rx.changed() => {
                if *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-openrec",
                        conn_id = %conn_id,
                        "接続をキャンセル"
                    );
                    break;
                }
            }
            msg = read.next() => {
                match msg {
                    Some(Ok(WsMessage::Text(text))) => {
                        tracing::trace!(target: "mcv::plugin-openrec", raw = %text, "WS テキスト受信");
                        let should_continue = handle_ws_text(
                            text.to_string(),
                            &ctx,
                            &plugin_id,
                            conn_id,
                            &service_id,
                            &channel,
                        )
                        .await;
                        if !should_continue {
                            break;
                        }
                    }
                    Some(Ok(WsMessage::Close(frame))) => {
                        tracing::info!(
                            target: "mcv::plugin-openrec",
                            conn_id = %conn_id,
                            close_frame = ?frame,
                            "WebSocket がサーバーにより切断された"
                        );
                        break;
                    }
                    Some(Ok(_)) => {}
                    Some(Err(e)) => {
                        tracing::error!(
                            target: "mcv::plugin-openrec",
                            conn_id = %conn_id,
                            error = %e,
                            "WebSocket 受信エラー"
                        );
                        break;
                    }
                    None => {
                        tracing::info!(
                            target: "mcv::plugin-openrec",
                            conn_id = %conn_id,
                            "WebSocket ストリーム終了"
                        );
                        break;
                    }
                }
            }
            _ = ping_interval.tick() => {
                tracing::trace!(target: "mcv::plugin-openrec", conn_id = %conn_id, "PING 送信");
                if let Err(e) = write.send(WsMessage::Text("2".into())).await {
                    tracing::warn!(target: "mcv::plugin-openrec", error = %e, "PING 送信失敗");
                    break;
                }
            }
        }
    }

    // Disconnected を Core に通知
    let disconnected_msg = McvMessage::new_notification(
        MessageType::Disconnected,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(DisconnectedPayload {
            connection_id: conn_id,
        })
        .unwrap(),
    );
    ctx.send_notification(disconnected_msg).await.ok();
}

// ---------------------------------------------------------------------------
// EIO / socket.io イベント処理
// ---------------------------------------------------------------------------

/// WebSocket テキストフレームを処理する
///
/// 戻り値: `true` = ループ継続、`false` = ループ終了
async fn handle_ws_text(
    text: String,
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    service_id: &ServiceId,
    channel: &ChannelId,
) -> bool {
    match parse_socketio_text(&text) {
        SocketIoFrame::Disconnect => {
            tracing::info!(
                target: "mcv::plugin-openrec",
                conn_id = %conn_id,
                reason = %text,
                "socket.io namespace disconnect を受信（配信終了または接続拒否）"
            );
            false
        }
        SocketIoFrame::Continue => true,
        SocketIoFrame::Events(events) => {
            process_events(events, ctx, plugin_id, conn_id, service_id, channel).await
        }
    }
}

/// イベントを処理する。戻り値: `true` = ループ継続、`false` = ループ終了
async fn process_events(
    events: Vec<OpenrecEvent>,
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    service_id: &ServiceId,
    channel: &ChannelId,
) -> bool {
    for event in events {
        match event {
            OpenrecEvent::Chat(data) => {
                if let Some(provider_msg) =
                    build_chat_message(data, service_id.clone(), channel.clone())
                {
                    send_comment(ctx, plugin_id, conn_id, provider_msg).await;
                }
            }
            OpenrecEvent::SystemMessage { chat_id, message } => {
                let provider_msg = ProviderMessage {
                    id: Uuid::new_v4().to_string(),
                    platform_message_id: chat_id.map(|n| n.to_string()),
                    service: service_id.clone(),
                    channel: channel.clone(),
                    sender: ProviderSender {
                        id: String::new(),
                        display_name: vec![],
                        badges: vec![],
                        role: None,
                        avatar_url: None,
                    },
                    timestamp: chrono::Utc::now().timestamp(),
                    kind: ProviderMessageKind::System(SystemKind::Notice),
                    content: ProviderContent::Text {
                        text: vec![MessagePart::Text { text: message }],
                    },
                    reply_to: None,
                    metadata: serde_json::Value::Null,
                };
                send_comment(ctx, plugin_id, conn_id, provider_msg).await;
            }
            OpenrecEvent::ViewerCount {
                viewers,
                live_viewers,
            } => {
                send_viewer_count(ctx, plugin_id, conn_id, viewers, live_viewers).await;
            }
            OpenrecEvent::StreamStart => {
                tracing::info!(
                    target: "mcv::plugin-openrec",
                    conn_id = %conn_id,
                    "配信開始"
                );
            }
            OpenrecEvent::StreamEnd => {
                tracing::info!(
                    target: "mcv::plugin-openrec",
                    conn_id = %conn_id,
                    "配信終了イベントを受信、接続を切断します"
                );
                return false;
            }
            OpenrecEvent::Subscribed => {
                tracing::debug!(
                    target: "mcv::plugin-openrec",
                    conn_id = %conn_id,
                    "サブスクライブイベント受信"
                );
            }
            OpenrecEvent::ChatlistMode => {
                // 高負荷時に REST API でチャット一括取得を促す信号。
                // 現状は未実装のためログのみ（一部メッセージが欠落する可能性あり）。
                tracing::debug!(
                    target: "mcv::plugin-openrec",
                    conn_id = %conn_id,
                    "CHATLIST_MODE: 一括チャット取得は未実装"
                );
            }
        }
    }
    true
}

// ---------------------------------------------------------------------------
// チャット履歴 → ProviderMessage 変換
// ---------------------------------------------------------------------------

/// REST API の `OpenrecHistoryChat` を `ProviderMessage` に変換する
fn build_history_message(
    chat: OpenrecHistoryChat,
    service_id: ServiceId,
    channel: ChannelId,
) -> Option<ProviderMessage> {
    // chat_type: 0=通常チャット, それ以外はシステム等
    let timestamp = chrono::DateTime::parse_from_rfc3339(&chat.messaged_at)
        .map(|dt| dt.timestamp())
        .unwrap_or_else(|_| chrono::Utc::now().timestamp());

    let role = if chat.user.is_official {
        Some(UserRole::Staff)
    } else if chat.user.is_premium {
        Some(UserRole::Member)
    } else {
        None
    };

    // スタンプ
    let stamp_url = chat
        .stamp
        .as_ref()
        .and_then(|s| s["image_url"].as_str())
        .filter(|s| !s.is_empty())
        .map(str::to_string);

    // イェル（マネタイズ）
    let (kind, content) = if let Some(yell) = &chat.yell {
        let points = yell["points"].as_i64().unwrap_or(0);
        let label = yell["label"].as_str().map(str::to_string);
        let kind = ProviderMessageKind::Monetary(MonetaryInfo {
            amount: Money {
                currency: "OPENREC".to_string(),
                value_minor: points,
            },
            tier: label.clone(),
            recurring: false,
        });
        let mut parts: Vec<MessagePart> = Vec::new();
        if !chat.message.is_empty() {
            parts.push(MessagePart::Text { text: chat.message });
        }
        if let Some(img) = yell["image_url"].as_str().filter(|s| !s.is_empty()) {
            parts.push(MessagePart::Image {
                url: img.to_string(),
                width: None,
                height: None,
                alt: label,
            });
        }
        let content = if parts.is_empty() {
            ProviderContent::Empty
        } else {
            ProviderContent::Text { text: parts }
        };
        (kind, content)
    } else {
        let mut parts: Vec<MessagePart> = Vec::new();
        if !chat.message.is_empty() {
            parts.push(MessagePart::Text { text: chat.message });
        }
        if let Some(img_url) = stamp_url {
            parts.push(MessagePart::Image {
                url: img_url,
                width: None,
                height: None,
                alt: None,
            });
        }
        let content = if parts.is_empty() {
            ProviderContent::Empty
        } else {
            ProviderContent::Text { text: parts }
        };
        (ProviderMessageKind::Chat, content)
    };

    Some(ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: Some(chat.id.to_string()),
        service: service_id,
        channel,
        sender: ProviderSender {
            id: chat.user.id,
            display_name: vec![MessagePart::Text {
                text: chat.user.nickname,
            }],
            badges: vec![],
            role,
            avatar_url: Some(chat.user.icon_image_url).filter(|s| !s.is_empty()),
        },
        timestamp,
        kind,
        content,
        reply_to: None,
        metadata: serde_json::Value::Null,
    })
}

// ---------------------------------------------------------------------------
// CHAT_ADD データ → ProviderMessage 変換
// ---------------------------------------------------------------------------

/// `OpenrecChatData` を `ProviderMessage` に変換する
fn build_chat_message(
    data: OpenrecChatData,
    service_id: ServiceId,
    channel: ChannelId,
) -> Option<ProviderMessage> {
    let role = if data.is_official {
        Some(UserRole::Staff)
    } else if data.is_moderator {
        Some(UserRole::Moderator)
    } else if data.is_premium {
        Some(UserRole::Member)
    } else {
        None
    };

    let (kind, content) = if let Some(yell) = data.yell {
        // イェル（マネタイズイベント）
        let kind = ProviderMessageKind::Monetary(MonetaryInfo {
            amount: Money {
                currency: "OPENREC".to_string(),
                value_minor: yell.points,
            },
            tier: yell.label.clone(),
            recurring: false,
        });

        let mut parts: Vec<MessagePart> = Vec::new();
        if !data.message.is_empty() {
            parts.push(MessagePart::Text { text: data.message });
        }
        if let Some(img) = yell.image_url {
            parts.push(MessagePart::Image {
                url: img,
                width: None,
                height: None,
                alt: yell.label,
            });
        }

        let content = if parts.is_empty() {
            ProviderContent::Empty
        } else {
            ProviderContent::Text { text: parts }
        };

        (kind, content)
    } else {
        // 通常チャットまたはスタンプ
        let mut parts: Vec<MessagePart> = Vec::new();
        if !data.message.is_empty() {
            parts.push(MessagePart::Text { text: data.message });
        }
        if let Some(img_url) = data.stamp_url {
            parts.push(MessagePart::Image {
                url: img_url,
                width: None,
                height: None,
                alt: None,
            });
        }

        let content = if parts.is_empty() {
            ProviderContent::Empty
        } else {
            ProviderContent::Text { text: parts }
        };

        (ProviderMessageKind::Chat, content)
    };

    Some(ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: Some(data.chat_id),
        service: service_id,
        channel,
        sender: ProviderSender {
            id: data.user_key,
            display_name: vec![MessagePart::Text {
                text: data.user_name,
            }],
            badges: vec![],
            role,
            avatar_url: data.user_icon,
        },
        timestamp: data.timestamp,
        kind,
        content,
        reply_to: None,
        metadata: serde_json::Value::Null,
    })
}

// ---------------------------------------------------------------------------
// ヘルパー関数
// ---------------------------------------------------------------------------

async fn send_comment(
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    provider_msg: ProviderMessage,
) {
    let envelope = McvEnvelope {
        event_id: Uuid::new_v4(),
        connection_id: conn_id,
        messages: vec![provider_msg],
        received_at: chrono::Utc::now().timestamp(),
        raw_message: None,
    };
    let msg = McvMessage::new_notification(
        MessageType::CommentReceived,
        MessageSource::Plugin {
            plugin_id: plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(CommentReceivedPayload {
            connection_id: conn_id,
            envelope,
        })
        .unwrap(),
    );
    if let Err(e) = ctx.send_notification(msg).await {
        tracing::warn!(target: "mcv::plugin-openrec", error = %e, "CommentReceived 送信失敗");
    }
}

async fn send_viewer_count(
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    viewers: u64,
    live_viewers: u64,
) {
    let msg = McvMessage::new_notification(
        MessageType::StreamMetadata,
        MessageSource::Plugin {
            plugin_id: plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(StreamMetadataPayload {
            connection_id: conn_id,
            title: None,
            viewer_count: Some(live_viewers),
            total_viewer_count: Some(viewers),
            start_time: None,
            others: None,
            clear: None,
        })
        .unwrap(),
    );
    if let Err(e) = ctx.send_notification(msg).await {
        tracing::warn!(target: "mcv::plugin-openrec", error = %e, "ViewerCount StreamMetadata 送信失敗");
    }
}

async fn send_stream_metadata(
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    title: Option<String>,
    start_time: Option<i64>,
) {
    let msg = McvMessage::new_notification(
        MessageType::StreamMetadata,
        MessageSource::Plugin {
            plugin_id: plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(StreamMetadataPayload {
            connection_id: conn_id,
            title,
            viewer_count: None,
            total_viewer_count: None,
            start_time,
            others: None,
            clear: None,
        })
        .unwrap(),
    );
    if let Err(e) = ctx.send_notification(msg).await {
        tracing::warn!(target: "mcv::plugin-openrec", error = %e, "StreamMetadata 送信失敗");
    }
}

async fn send_connect_failed(
    ctx: &PluginContext,
    plugin_id: &PluginId,
    conn_id: Uuid,
    reason: &str,
) {
    let msg = McvMessage::new_notification(
        MessageType::ConnectFailed,
        MessageSource::Plugin {
            plugin_id: plugin_id.clone(),
        },
        MessageDestination::Core,
        serde_json::to_value(ConnectFailedPayload {
            connection_id: conn_id,
            reason: reason.to_string(),
        })
        .unwrap(),
    );
    ctx.send_notification(msg).await.ok();
}

/// URL クエリパラメータ値を percent-encode する
fn percent_encode(s: &str) -> String {
    let mut out = String::with_capacity(s.len() * 2);
    for b in s.bytes() {
        match b {
            b'A'..=b'Z' | b'a'..=b'z' | b'0'..=b'9' | b'-' | b'_' | b'.' | b'~' => {
                out.push(b as char);
            }
            _ => {
                out.push('%');
                let hi = b >> 4;
                let lo = b & 0xF;
                out.push(if hi < 10 {
                    (b'0' + hi) as char
                } else {
                    (b'A' + hi - 10) as char
                });
                out.push(if lo < 10 {
                    (b'0' + lo) as char
                } else {
                    (b'A' + lo - 10) as char
                });
            }
        }
    }
    out
}
