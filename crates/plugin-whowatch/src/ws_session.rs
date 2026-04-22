//! ふわっち Phoenix WebSocket セッション
//!
//! `wss://ws.whowatch.tv/socket/websocket?vsn=2.0.0` に接続し、
//! コメントを受信して Core へ転送する。
//!
//! ## 接続フロー
//! 1. `GET /lives/{live_id}?last_updated_at=0` で WebSocket トークンと `live.user.id` を取得
//! 2. `GET /users/{live.user.id}/profile` でアカウント情報を取得
//! 3. `wss://ws.whowatch.tv/socket/websocket?vsn=2.0.0` へ接続
//! 4. `phx_join` を送信してルームに参加
//! 5. 30 秒ごとに `heartbeat` を送信
//! 6. `shout` イベントでコメントを受信し Core へ転送

use futures_util::{SinkExt, StreamExt};
use mcv_messages::{
    AccountInfo, ChannelId, CommentReceivedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, PluginId, ProviderBadge,
    ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
    UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::time::{timeout, Duration};
use tokio_tungstenite::{
    connect_async,
    tungstenite::{Error as WsError, Message as WsMessage},
};
use uuid::Uuid;
use whowatch_lib::{live::Comment, WhoWatchClient};

use crate::WhoWatchPlugin;

const WS_URL: &str = "wss://ws.whowatch.tv/socket/websocket?vsn=2.0.0";
const HEARTBEAT_INTERVAL_SECS: u64 = 30;
const CONNECT_TIMEOUT_SECS: u64 = 15;
/// phx_join 後に phx_reply を待つ最大時間
const JOIN_REPLY_TIMEOUT_SECS: u64 = 10;

/// セッション終了理由
#[derive(Debug)]
pub(crate) enum SessionResult {
    /// ユーザーがキャンセルした
    Cancelled,
    /// WebSocket エラー、API エラーなどで終了（再接続対象）
    Error(&'static str),
    /// 配信中ではない（JWT 未取得）。ポーリングして配信開始を待つ
    NotLive,
}

/// Phoenix WebSocket セッションを実行する。
///
/// 終了理由を [`SessionResult`] として返す。
pub(crate) async fn run_ws_session(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    live_id: u64,
    whowatch_token: String,
    cancel_rx: &mut watch::Receiver<bool>,
) -> SessionResult {
    // -------------------------------------------------
    // 1. WhoWatchClient を生成
    // -------------------------------------------------
    let client = match WhoWatchClient::new(&whowatch_token, None) {
        Ok(c) => c,
        Err(e) => {
            tracing::error!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                error = %e,
                "WhoWatchClient の生成に失敗"
            );
            return SessionResult::Error("WhoWatchClient 生成失敗");
        }
    };

    // -------------------------------------------------
    // 2. GET /lives/{live_id} で ws_token と user_id を取得
    // -------------------------------------------------
    let live_response = match client.get_live(live_id, 0).await {
        Ok(r) => r,
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                error = %e,
                "GET /lives/{live_id} 失敗"
            );
            return SessionResult::Error("GET /lives/{id} 失敗");
        }
    };

    // 配信中でない場合は WebSocket 接続を行わずに返す
    if !live_response.live.is_publishing() {
        tracing::info!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            live_id = %live_id,
            live_status = ?live_response.live.live_status,
            "配信中ではないため WebSocket には接続しません"
        );
        return SessionResult::NotLive;
    }

    let ws_token = match &live_response.jwt {
        Some(t) if !t.is_empty() => {
            tracing::debug!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                "jwt トークン取得成功"
            );
            t.clone()
        }
        _ => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                "配信中だが jwt フィールドがない。WebSocket 接続を見送ります"
            );
            return SessionResult::NotLive;
        }
    };

    // WebSocket URL を live レスポンスから取得する
    // comment_server_url 例: "wss://ws.whowatch.tv/socket"
    // Phoenix の WebSocket エンドポイントは /websocket?vsn=2.0.0
    let ws_url = match &live_response.comment_server_url {
        Some(base) if !base.is_empty() => format!("{}/websocket?vsn=2.0.0", base),
        _ => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                "comment_server_url がない。定数 URL にフォールバック"
            );
            WS_URL.to_string()
        }
    };

    // -------------------------------------------------
    // 3. live.user.id でプロフィールを取得してアカウント情報を送信
    // -------------------------------------------------
    let user_id = live_response.live.user.as_ref().map(|u| u.id);

    match user_id {
        Some(uid) => match client.get_user_profile(uid).await {
            Ok(profile) => {
                tracing::info!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    live_id = %live_id,
                    user_id = %uid,
                    user_name = %profile.name,
                    "アカウント情報取得成功"
                );
                WhoWatchPlugin::send_message(
                    ctx.clone(),
                    McvMessage::new_notification(
                        MessageType::UpdateConnectionAccount,
                        MessageSource::Plugin {
                            plugin_id: logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(UpdateConnectionAccountPayload {
                            connection_id,
                            account: Some(AccountInfo {
                                user_id: uid.to_string(),
                                display_name: profile.name,
                                avatar_url: if profile.icon_url.is_empty() {
                                    None
                                } else {
                                    Some(profile.icon_url)
                                },
                            }),
                        })
                        .unwrap(),
                    ),
                )
                .await;
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    live_id = %live_id,
                    user_id = %uid,
                    error = %e,
                    "プロフィール取得失敗。アカウント情報なしで続行"
                );
            }
        },
        None => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                "live.user が null。アカウント情報なしで続行"
            );
        }
    }

    // -------------------------------------------------
    // 4. 過去コメントを Core へ送信（WebSocket 接続前）
    // -------------------------------------------------
    if !live_response.comments.is_empty() {
        tracing::info!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            count = %live_response.comments.len(),
            "過去コメントを Core へ送信します"
        );
        let channel_for_past = live_id.to_string();
        send_past_comments(
            ctx.clone(),
            logical_plugin_id.clone(),
            connection_id,
            &channel_for_past,
            &live_response.comments,
        )
        .await;
    }

    // -------------------------------------------------
    // 5. WebSocket 接続
    // -------------------------------------------------
    tracing::info!(
        target: "mcv::plugin-whowatch",
        connection_id = %connection_id,
        ws_url = %ws_url,
        "WebSocket 接続を試みます"
    );
    let ws_stream = match timeout(
        Duration::from_secs(CONNECT_TIMEOUT_SECS),
        connect_async(ws_url),
    )
    .await
    {
        Err(_) => {
            tracing::error!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                timeout_secs = CONNECT_TIMEOUT_SECS,
                "WebSocket 接続タイムアウト"
            );
            return SessionResult::Error("WS 接続タイムアウト");
        }
        Ok(Err(e)) => {
            tracing::error!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                error = %e,
                "WebSocket 接続失敗"
            );
            return SessionResult::Error("WS 接続失敗");
        }
        Ok(Ok((stream, _))) => {
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                "WebSocket 接続成功"
            );
            stream
        }
    };

    let (mut write, mut read) = ws_stream.split();
    let topic = format!("room:{}", live_id);

    // -------------------------------------------------
    // 5. phx_join 送信
    // -------------------------------------------------
    let join_msg = serde_json::json!(["1", "1", topic, "phx_join", {"p": ws_token}]);
    let join_text = join_msg.to_string();
    tracing::debug!(
        target: "mcv::plugin-whowatch",
        connection_id = %connection_id,
        payload = %join_text,
        "phx_join 送信"
    );
    if let Err(e) = write.send(WsMessage::Text(join_text.into())).await {
        tracing::error!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            error = %e,
            "phx_join 送信失敗"
        );
        return SessionResult::Error("phx_join 送信失敗");
    }

    // phx_reply (status: ok) を待つ
    match timeout(
        Duration::from_secs(JOIN_REPLY_TIMEOUT_SECS),
        wait_for_join_reply(&mut read, connection_id),
    )
    .await
    {
        Err(_) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                timeout_secs = JOIN_REPLY_TIMEOUT_SECS,
                "phx_join への reply がタイムアウト"
            );
            return SessionResult::Error("phx_join タイムアウト");
        }
        Ok(false) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                "phx_join が失敗ステータスで返答"
            );
            return SessionResult::Error("phx_join 失敗ステータス");
        }
        Ok(true) => {
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                live_id = %live_id,
                "phx_join 成功、コメント受信を開始"
            );
        }
    }

    // -------------------------------------------------
    // 6. メインループ（コメント受信 + ハートビート）
    // -------------------------------------------------
    let channel_str = live_id.to_string();
    let mut heartbeat_ref: u64 = 2;
    let mut heartbeat_interval =
        tokio::time::interval(Duration::from_secs(HEARTBEAT_INTERVAL_SECS));
    // 最初の tick を即時消費して重複しないようにする
    heartbeat_interval.tick().await;

    loop {
        tokio::select! {
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-whowatch",
                        connection_id = %connection_id,
                        "WS ループ: キャンセル受信"
                    );
                    return SessionResult::Cancelled;
                }
            }

            _ = heartbeat_interval.tick() => {
                let hb = serde_json::json!([null, heartbeat_ref.to_string(), "phoenix", "heartbeat", {}]);
                tracing::trace!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    heartbeat_ref = %heartbeat_ref,
                    "heartbeat 送信"
                );
                if let Err(e) = write.send(WsMessage::Text(hb.to_string().into())).await {
                    tracing::error!(
                        target: "mcv::plugin-whowatch",
                        connection_id = %connection_id,
                        error = %e,
                        "heartbeat 送信失敗、切断します"
                    );
                    return SessionResult::Error("heartbeat 送信失敗");
                }
                heartbeat_ref += 1;
            }

            frame = read.next() => {
                match frame {
                    Some(Ok(WsMessage::Text(text))) => {
                        handle_text_frame(
                            ctx.clone(),
                            logical_plugin_id.clone(),
                            connection_id,
                            &channel_str,
                            &text,
                        )
                        .await;
                    }
                    Some(Ok(WsMessage::Close(frame))) => {
                        tracing::info!(
                            target: "mcv::plugin-whowatch",
                            connection_id = %connection_id,
                            close_code = ?frame.as_ref().map(|f| f.code),
                            close_reason = ?frame.as_ref().map(|f| f.reason.as_ref() as &str),
                            "サーバーから WebSocket が切断された"
                        );
                        return SessionResult::Error("サーバーから Close 受信");
                    }
                    Some(Ok(WsMessage::Ping(data))) => {
                        tracing::trace!(
                            target: "mcv::plugin-whowatch",
                            connection_id = %connection_id,
                            "WebSocket Ping 受信、Pong を返送"
                        );
                        if let Err(e) = write.send(WsMessage::Pong(data)).await {
                            tracing::error!(
                                target: "mcv::plugin-whowatch",
                                connection_id = %connection_id,
                                error = %e,
                                "Pong 送信失敗"
                            );
                            return SessionResult::Error("Pong 送信失敗");
                        }
                    }
                    Some(Ok(_)) => {
                        // Binary / Pong / Frame は無視
                    }
                    Some(Err(e)) => {
                        tracing::error!(
                            target: "mcv::plugin-whowatch",
                            connection_id = %connection_id,
                            error = %e,
                            "WebSocket 受信エラー"
                        );
                        return SessionResult::Error("WS 受信エラー");
                    }
                    None => {
                        tracing::info!(
                            target: "mcv::plugin-whowatch",
                            connection_id = %connection_id,
                            "WebSocket ストリームが終了"
                        );
                        return SessionResult::Error("WS ストリーム終了");
                    }
                }
            }
        }
    }
}

// ---------------------------------------------------------------------------
// phx_join reply 待機
// ---------------------------------------------------------------------------

/// `phx_reply` が届くまでフレームを消費する。
/// `true` = status が "ok"、`false` = エラーまたは切断
async fn wait_for_join_reply<S>(read: &mut S, connection_id: Uuid) -> bool
where
    S: futures_util::Stream<Item = Result<WsMessage, WsError>> + Unpin,
{
    while let Some(frame) = read.next().await {
        match frame {
            Ok(WsMessage::Text(text)) => {
                tracing::trace!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    raw = %text,
                    "phx_join 待機中に受信"
                );
                let value: serde_json::Value = match serde_json::from_str(&text) {
                    Ok(v) => v,
                    Err(_) => continue,
                };
                // Phoenix メッセージ: [join_ref, ref, topic, event, payload]
                let event = value.get(3).and_then(|v| v.as_str()).unwrap_or("");
                if event == "phx_reply" {
                    let status = value
                        .get(4)
                        .and_then(|p| p.get("status"))
                        .and_then(|s| s.as_str())
                        .unwrap_or("");
                    tracing::debug!(
                        target: "mcv::plugin-whowatch",
                        connection_id = %connection_id,
                        status = %status,
                        "phx_reply 受信"
                    );
                    return status == "ok";
                }
            }
            Ok(WsMessage::Close(_)) | Err(_) | Ok(WsMessage::Binary(_)) => {
                return false;
            }
            _ => {}
        }
    }
    false
}

// ---------------------------------------------------------------------------
// テキストフレーム処理
// ---------------------------------------------------------------------------

/// WebSocket テキストフレームを処理する。
async fn handle_text_frame(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel: &str,
    text: &str,
) {
    let value: serde_json::Value = match serde_json::from_str(text) {
        Ok(v) => v,
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                error = %e,
                raw = %text,
                "JSON パース失敗"
            );
            return;
        }
    };

    // Phoenix メッセージ: [join_ref, ref, topic, event, payload]
    let event = match value.get(3).and_then(|v| v.as_str()) {
        Some(e) => e,
        None => return,
    };

    match event {
        "shout" => {
            handle_shout(ctx, logical_plugin_id, connection_id, channel, &value, text).await;
        }
        "phx_reply" => {
            // heartbeat reply など。ログだけ残す
            let status = value
                .get(4)
                .and_then(|p| p.get("status"))
                .and_then(|s| s.as_str())
                .unwrap_or("unknown");
            tracing::trace!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                status = %status,
                "phx_reply 受信"
            );
        }
        "phx_error" => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                raw = %text,
                "phx_error 受信"
            );
        }
        "phx_close" => {
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                "phx_close 受信"
            );
        }
        other => {
            tracing::trace!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                event = %other,
                "未処理イベント"
            );
        }
    }
}

// ---------------------------------------------------------------------------
// shout イベント処理
// ---------------------------------------------------------------------------

/// `shout` イベントからコメントを取り出して Core へ送信する。
///
/// Phoenix メッセージ構造:
/// ```json
/// [join_ref, null, "room:live_id", "shout", { ... }]
/// ```
async fn handle_shout(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel: &str,
    value: &serde_json::Value,
    raw_text: &str,
) {
    let payload = match value.get(4) {
        Some(p) => p,
        None => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                raw = %raw_text,
                "shout: payload (index 4) がない"
            );
            return;
        }
    };

    let comment = match payload.get("comment") {
        Some(c) => c,
        None => {
            tracing::debug!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                shout_payload = %payload,
                "shout: comment フィールドがない（システムイベントの可能性）"
            );
            return;
        }
    };

    // --- 確認済みフィールド ---

    let comment_id = comment
        .get("id")
        .and_then(|v| v.as_u64())
        .map(|n| n.to_string())
        .unwrap_or_else(|| Uuid::new_v4().to_string());

    let message = match comment.get("message").and_then(|v| v.as_str()) {
        Some(m) => m.to_string(),
        None => {
            tracing::warn!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                comment_json = %comment,
                "shout: message フィールドがない"
            );
            return;
        }
    };

    // タイムスタンプは posted_at（Unix ミリ秒）
    let timestamp_ms = comment.get("posted_at").and_then(|v| v.as_i64());

    if timestamp_ms.is_none() {
        tracing::warn!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            comment_json = %comment,
            "shout: posted_at フィールドが未検出"
        );
    }
    let timestamp = timestamp_ms
        .map(|ms| ms / 1000)
        .unwrap_or_else(|| chrono::Utc::now().timestamp());

    // --- 要確認フィールド: ユーザー情報 ---
    let user = comment.get("user");
    if user.is_none() {
        tracing::warn!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            comment_json = %comment,
            "shout: user フィールドがない"
        );
    }

    let user_id = user
        .and_then(|u| u.get("id"))
        .and_then(|v| v.as_u64())
        .map(|n| n.to_string())
        .unwrap_or_default();

    let user_name = user
        .and_then(|u| u.get("name"))
        .and_then(|v| v.as_str())
        .unwrap_or("")
        .to_string();

    let icon_url = user
        .and_then(|u| u.get("icon_url"))
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());

    // アイテム（スタンプ・ギフト）情報
    let item_image_url = comment
        .get("play_item_pattern")
        .and_then(|p| p.get("image_url"))
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());

    let item_count = comment
        .get("item_count")
        .and_then(|v| v.as_u64())
        .unwrap_or(1);

    let item_name = comment
        .get("play_item_pattern")
        .and_then(|p| p.get("name"))
        .and_then(|v| v.as_str())
        .unwrap_or("")
        .to_string();

    let is_first_comment = comment
        .get("is_first_comment")
        .and_then(|v| v.as_bool())
        .unwrap_or(false);

    // コンテンツ（テキスト＋オプションでアイテム画像）
    let mut text_parts = vec![MessagePart::Text { text: message }];
    if let Some(image_url) = item_image_url {
        text_parts.push(MessagePart::Image {
            url: image_url,
            width: Some(32),
            height: Some(32),
            alt: Some(item_name),
        });
        if item_count > 1 {
            text_parts.push(MessagePart::Text {
                text: format!("×{}", item_count),
            });
        }
    }

    // バッジ（初コメ）
    let mut badges = vec![];
    if is_first_comment {
        badges.push(ProviderBadge {
            id: "first_comment".to_string(),
            name: "初コメ".to_string(),
            image_url: None,
        });
    }

    let provider_msg = ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: Some(comment_id),
        service: ServiceId("whowatch".to_string()),
        channel: ChannelId(channel.to_string()),
        sender: ProviderSender {
            id: user_id,
            display_name: vec![MessagePart::Text { text: user_name }],
            badges,
            role: None,
            avatar_url: icon_url,
        },
        timestamp,
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text { text: text_parts },
        reply_to: None,
        metadata: serde_json::Value::Null,
    };

    let envelope = McvEnvelope {
        event_id: Uuid::new_v4(),
        connection_id,
        messages: vec![provider_msg],
        received_at: chrono::Utc::now().timestamp(),
        raw_message: Some(raw_text.to_string()),
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
    WhoWatchPlugin::send_message(ctx, msg).await;
}

// ---------------------------------------------------------------------------
// 過去コメント送信
// ---------------------------------------------------------------------------

/// `GET /lives/{live_id}?last_updated_at=0` のレスポンスに含まれる過去コメントを
/// WebSocket 接続前に Core へ一括送信する。
async fn send_past_comments(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel: &str,
    comments: &[Comment],
) {
    for comment in comments {
        let comment_id = comment.id.to_string();
        let message = comment.message.clone();
        let timestamp = (comment.posted_at / 1000) as i64;

        let user_id = comment
            .user
            .as_ref()
            .and_then(|u| u.id)
            .map(|n| n.to_string())
            .unwrap_or_default();

        let user_name = comment
            .user
            .as_ref()
            .and_then(|u| u.name.as_deref())
            .unwrap_or("")
            .to_string();

        let icon_url = comment
            .user
            .as_ref()
            .and_then(|u| u.icon_url.as_deref())
            .map(|s| s.to_string());

        let is_first_comment = comment.is_first_comment.unwrap_or(false);

        let mut badges = vec![];
        if is_first_comment {
            badges.push(ProviderBadge {
                id: "first_comment".to_string(),
                name: "初コメ".to_string(),
                image_url: None,
            });
        }

        let provider_msg = ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: Some(comment_id),
            service: ServiceId("whowatch".to_string()),
            channel: ChannelId(channel.to_string()),
            sender: ProviderSender {
                id: user_id,
                display_name: vec![MessagePart::Text { text: user_name }],
                badges,
                role: None,
                avatar_url: icon_url,
            },
            timestamp,
            kind: ProviderMessageKind::Chat,
            content: ProviderContent::Text {
                text: vec![MessagePart::Text { text: message }],
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
                plugin_id: logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(CommentReceivedPayload {
                connection_id,
                envelope,
            })
            .unwrap(),
        );
        WhoWatchPlugin::send_message(ctx.clone(), msg).await;
    }
}
