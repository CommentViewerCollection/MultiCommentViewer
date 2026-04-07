//! Twitch IRC セッションロジック
//!
//! WebSocket 接続確立から IRC セッション初期化、コメント受信ループまでを担う。
//! `Connection::connect()` から起動されるバックグラウンドタスクの実体。
//!
//! ## 処理フロー
//! 1. [`connect_ws`] — WebSocket 接続（15 秒タイムアウト）
//! 2. [`init_irc`] — CAP REQ / PASS / NICK / JOIN 送信
//! 3. [`fetch_and_send_history`] — 過去コメント取得・送信（失敗しても続行）
//! 4. [`build_badge_cache`] — グローバル・チャンネルバッジキャッシュ構築
//! 5. [`run_irc_loop`] — メッセージ受信ループ（キャンセルまたはサーバー切断で終了）

use std::sync::Arc;

use futures_util::{stream::SplitSink, stream::SplitStream, SinkExt, StreamExt};
use mcv_messages::{
    AccountInfo, ChannelId, CommentReceivedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, PluginId, ProviderBadge,
    ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId, SystemKind,
    UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::net::TcpStream;
use tokio::sync::watch;
use tokio::time::{interval, timeout, Duration};
use tokio_tungstenite::{
    connect_async,
    tungstenite::{Error as WsError, Message as WsMessage},
    MaybeTlsStream, WebSocketStream,
};
use twitch_lib::auth_token::AuthToken;
use twitch_lib::badges::{fetch_channel_badges_gql, fetch_global_badges_gql, BadgeCache};
use twitch_lib::irc::{parse_irc_line, to_twitch_event, TwitchEvent};
use uuid::Uuid;

use crate::TwitchPlugin;

type WsWrite = SplitSink<WebSocketStream<MaybeTlsStream<TcpStream>>, WsMessage>;
type WsRead = SplitStream<WebSocketStream<MaybeTlsStream<TcpStream>>>;

// ───────────────────────────────────────────────
// Public entry point
// ───────────────────────────────────────────────

/// IRC セッション全体を実行する。
///
/// 正常終了（キャンセル・サーバー切断）は `Ok(())`、
/// WebSocket 接続失敗など早期エラーは `Err(())` を返す。
pub(crate) async fn run_irc_session(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    url: String,
    username: String,
    password: Option<String>,
    join_command: String,
    channel_login: String,
    auth_token: Option<AuthToken>,
    cancel_rx: &mut watch::Receiver<bool>,
) -> Result<(), ()> {
    let ws_stream = connect_ws(connection_id, &url).await?;
    let (mut write, mut read) = ws_stream.split();

    let nick = if password.is_some() {
        username
    } else {
        // 匿名接続時は justinfan ユーザーを使う
        format!("justinfan{}", (connection_id.as_u128() % 1_000_000) as u64)
    };
    init_irc(
        connection_id,
        &mut write,
        &nick,
        password.as_deref(),
        &join_command,
    )
    .await?;

    fetch_and_send_history(
        ctx.clone(),
        logical_plugin_id.clone(),
        connection_id,
        &channel_login,
        auth_token.as_ref(),
    )
    .await;

    let badge_cache =
        Arc::new(build_badge_cache(connection_id, &channel_login, auth_token.as_ref()).await);

    run_irc_loop(
        ctx,
        logical_plugin_id,
        connection_id,
        &mut write,
        &mut read,
        badge_cache,
        cancel_rx,
    )
    .await;

    Ok(())
}

// ───────────────────────────────────────────────
// 接続・初期化
// ───────────────────────────────────────────────

/// WebSocket 接続を確立する（15 秒タイムアウト）。
async fn connect_ws(
    connection_id: Uuid,
    url: &str,
) -> Result<WebSocketStream<MaybeTlsStream<TcpStream>>, ()> {
    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        ws_url = %url,
        "Attempting to connect Twitch IRC WebSocket"
    );

    match timeout(Duration::from_secs(15), connect_async(url)).await {
        Err(_) => {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                ws_url = %url,
                "Timed out while connecting Twitch IRC WebSocket"
            );
            Err(())
        }
        Ok(Err(e)) => {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Failed to connect to Twitch IRC WebSocket"
            );
            Err(())
        }
        Ok(Ok((ws_stream, _))) => {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Twitch IRC WebSocket connected"
            );
            Ok(ws_stream)
        }
    }
}

/// IRC セッションを初期化する（CAP REQ / PASS / NICK / JOIN）。
async fn init_irc(
    connection_id: Uuid,
    write: &mut WsWrite,
    nick: &str,
    password: Option<&str>,
    join_command: &str,
) -> Result<(), ()> {
    let mut commands =
        vec!["CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership".to_string()];
    if let Some(pass) = password {
        commands.push(format!("PASS {pass}"));
    }
    commands.push(format!("NICK {nick}"));
    commands.push(join_command.to_string());

    for command in &commands {
        send_irc_line(write, command).await.map_err(|e| {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                command = %command,
                error = %e,
                "Failed to send IRC init command"
            );
        })?;
    }

    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        "Connected to Twitch IRC and initialized session"
    );
    Ok(())
}

/// 直近の過去コメント履歴を取得して Core へ送信する。失敗しても続行する。
async fn fetch_and_send_history(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: &str,
    auth_token: Option<&AuthToken>,
) {
    let client_id = twitch_lib::ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
    let history =
        match twitch_lib::fetch_recent_chat_messages(channel_login, &client_id, auth_token).await {
            Ok(h) => h,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    error = %e,
                    "直近コメント履歴の取得に失敗"
                );
                return;
            }
        };

    let provider_messages: Vec<ProviderMessage> = history
        .into_iter()
        .filter(|msg| msg.deleted_at.is_none())
        .map(|msg| {
            let timestamp = chrono::DateTime::parse_from_rfc3339(&msg.sent_at)
                .map(|dt| dt.timestamp())
                .unwrap_or_else(|_| chrono::Utc::now().timestamp());
            ProviderMessage {
                id: Uuid::new_v4().to_string(),
                platform_message_id: Some(msg.message_id),
                service: ServiceId("twitch".to_string()),
                channel: ChannelId(channel_login.to_string()),
                sender: ProviderSender {
                    id: msg.sender_id,
                    display_name: vec![MessagePart::Text {
                        text: msg.sender_display_name,
                    }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::Chat,
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: msg.text }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            }
        })
        .collect();

    if provider_messages.is_empty() {
        return;
    }

    let envelope = McvEnvelope {
        event_id: Uuid::new_v4(),
        connection_id,
        messages: provider_messages,
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

/// グローバル・チャンネルバッジキャッシュを構築する。
async fn build_badge_cache(
    connection_id: Uuid,
    channel_login: &str,
    auth_token: Option<&AuthToken>,
) -> BadgeCache {
    let client_id = twitch_lib::ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");

    let mut merged = match fetch_global_badges_gql(&client_id, auth_token).await {
        Ok(cache) => {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                badge_set_count = cache.len(),
                total_badge_count = cache.values().map(|v| v.len()).sum::<usize>(),
                "グローバルバッジを取得"
            );
            cache
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "グローバルバッジの取得に失敗（バッジなしで続行）"
            );
            BadgeCache::new()
        }
    };

    match fetch_channel_badges_gql(channel_login, &client_id, auth_token).await {
        Ok(channel_cache) => {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                channel = %channel_login,
                badge_set_count = channel_cache.len(),
                total_badge_count = channel_cache.values().map(|v| v.len()).sum::<usize>(),
                "チャンネルバッジを取得"
            );
            // チャンネル固有バッジでグローバルを上書き（subscriber カスタム画像等）
            for (set_id, versions) in channel_cache {
                merged.entry(set_id).or_default().extend(versions);
            }
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                channel = %channel_login,
                error = %e,
                "チャンネルバッジの取得に失敗（グローバルバッジのみで続行）"
            );
        }
    }

    tracing::debug!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        badge_set_count = merged.len(),
        total_badge_count = merged.values().map(|v| v.len()).sum::<usize>(),
        "バッジキャッシュ確定"
    );
    merged
}

// ───────────────────────────────────────────────
// 受信ループ
// ───────────────────────────────────────────────

/// IRC メッセージ受信ループ。キャンセル・サーバー切断・エラーで終了する。
async fn run_irc_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    write: &mut WsWrite,
    read: &mut WsRead,
    badge_cache: Arc<BadgeCache>,
    cancel_rx: &mut watch::Receiver<bool>,
) {
    let mut ping_interval = interval(Duration::from_secs(300));

    loop {
        tokio::select! {
            result = cancel_rx.changed() => {
                // Err は送信側がドロップされた（stop() 後）= 終了
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Twitch IRC loop cancelled"
                    );
                    break;
                }
            }
            _ = ping_interval.tick() => {
                if let Err(e) = send_irc_line(write, "PING :tmi.twitch.tv").await {
                    tracing::error!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        error = %e,
                        "Failed to send periodic PING"
                    );
                    break;
                }
            }
            frame = read.next() => {
                let should_continue = handle_ws_frame(
                    ctx.clone(),
                    logical_plugin_id.clone(),
                    connection_id,
                    write,
                    frame,
                    Arc::clone(&badge_cache),
                )
                .await;
                if !should_continue {
                    break;
                }
            }
        }
    }
}

/// WebSocket フレームを1つ処理する。
///
/// 受信処理を続けるべき場合は `true`、接続を閉じるべき場合は `false` を返す。
async fn handle_ws_frame(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    write: &mut WsWrite,
    frame: Option<Result<WsMessage, WsError>>,
    badge_cache: Arc<BadgeCache>,
) -> bool {
    match frame {
        Some(Ok(WsMessage::Text(text))) => {
            handle_irc_text(
                ctx,
                logical_plugin_id,
                connection_id,
                write,
                text.to_string(),
                &badge_cache,
            )
            .await
        }
        Some(Ok(WsMessage::Close(frame))) => {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                close_frame = ?frame,
                "WebSocket closed by server"
            );
            false
        }
        Some(Ok(WsMessage::Binary(_))) => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Received binary frame"
            );
            true
        }
        Some(Ok(WsMessage::Ping(_))) => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Received websocket ping frame"
            );
            true
        }
        Some(Ok(WsMessage::Pong(_))) => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Received websocket pong frame"
            );
            true
        }
        Some(Ok(WsMessage::Frame(_))) => {
            tracing::trace!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "Received websocket raw frame"
            );
            true
        }
        Some(Err(e)) => {
            tracing::error!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                error = %e,
                "Error while receiving websocket message"
            );
            false
        }
        None => {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                "WebSocket stream ended"
            );
            false
        }
    }
}

// ───────────────────────────────────────────────
// IRC テキストメッセージ処理
// ───────────────────────────────────────────────

/// IRC テキストフレームを処理し、ProviderMessage を Core へ送信する。
///
/// 続行する場合は `true`、PONG 送信失敗など切断すべき場合は `false` を返す。
async fn handle_irc_text(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    write: &mut WsWrite,
    raw_text: String,
    badge_cache: &BadgeCache,
) -> bool {
    let mut provider_messages = Vec::new();

    for line in raw_text.lines() {
        let event = to_twitch_event(parse_irc_line(line));
        match event {
            TwitchEvent::PrivMsg {
                channel,
                user,
                text,
                tags,
            } => {
                provider_messages.push(build_privmsg_message(
                    &channel,
                    &user,
                    &text,
                    &tags,
                    badge_cache,
                ));
            }
            TwitchEvent::UserNotice { channel, tags } => {
                if let Some(msg_id) = tags.get("msg-id") {
                    if let Some(msg) = build_usernotice_message(msg_id, &channel, &tags) {
                        provider_messages.push(msg);
                    }
                }
            }
            TwitchEvent::GlobalUserState {
                display_name: Some(name),
                user_id,
            } => {
                let msg = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id,
                        account: Some(AccountInfo {
                            user_id: user_id.unwrap_or_default(),
                            display_name: name,
                            avatar_url: None,
                        }),
                    })
                    .unwrap(),
                );
                TwitchPlugin::send_message(ctx.clone(), msg).await;
            }
            TwitchEvent::GlobalUserState { .. } => {}
            TwitchEvent::Ping => {
                if let Err(e) = send_irc_line(write, "PONG").await {
                    tracing::error!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        error = %e,
                        "Failed to send PONG"
                    );
                    return false;
                }
            }
            _ => {}
        }
    }

    if !provider_messages.is_empty() {
        let envelope = McvEnvelope {
            event_id: Uuid::new_v4(),
            connection_id,
            messages: provider_messages,
            received_at: chrono::Utc::now().timestamp(),
            raw_message: Some(raw_text),
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
    true
}

/// PRIVMSG イベントから ProviderMessage を構築する。
fn build_privmsg_message(
    channel: &str,
    user: &str,
    text: &str,
    tags: &std::collections::HashMap<String, String>,
    badge_cache: &BadgeCache,
) -> ProviderMessage {
    // badge-info タグから set_id → 値のマップを構築
    // 形式: "subscriber/10" または "founder/0,subscriber/30" 等
    let badge_info: std::collections::HashMap<String, String> = tags
        .get("badge-info")
        .map(|s| {
            s.split(',')
                .filter(|s| !s.is_empty())
                .filter_map(|entry| {
                    let mut parts = entry.splitn(2, '/');
                    let k = parts.next()?.to_string();
                    let v = parts.next()?.to_string();
                    Some((k, v))
                })
                .collect()
        })
        .unwrap_or_default();

    let badges = parse_irc_badges(
        tags.get("badges").map(|s| s.as_str()).unwrap_or(""),
        &badge_info,
        badge_cache,
    );

    // display-name が login と異なる（日本語名など）場合はブラウザ同様に "DisplayName (loginname)" と表示する
    let display_name_text = match tags.get("display-name").filter(|s| !s.is_empty()) {
        Some(dn) if dn.to_lowercase() != user => format!("{} ({})", dn, user),
        Some(dn) => dn.clone(),
        None => user.to_string(),
    };

    ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: None,
        service: ServiceId("twitch".to_string()),
        channel: ChannelId(channel.to_string()),
        sender: ProviderSender {
            id: user.to_string(),
            display_name: vec![MessagePart::Text {
                text: display_name_text,
            }],
            badges,
            role: None,
            avatar_url: None,
        },
        timestamp: chrono::Utc::now().timestamp(),
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text {
            text: parse_emote_content(text, tags.get("emotes")),
        },
        reply_to: None,
        metadata: serde_json::Value::Null,
    }
}

/// IRC `badges` タグ文字列を解析して ProviderBadge リストに変換する。
///
/// 形式: `"moderator/1,subscriber/36"`
fn parse_irc_badges(
    badges_str: &str,
    badge_info: &std::collections::HashMap<String, String>,
    badge_cache: &BadgeCache,
) -> Vec<ProviderBadge> {
    badges_str
        .split(',')
        .filter(|s| !s.is_empty())
        .map(|badge| {
            let mut parts = badge.splitn(2, '/');
            let set_id = parts.next().unwrap_or("").to_string();
            let version = parts.next().unwrap_or("1").to_string();
            let resolved = badge_cache
                .get(&set_id)
                .and_then(|versions| resolve_badge_version(versions, &version));
            let (image_url, gql_title) = resolved
                .map(|(url, t)| (Some(url), t))
                .unwrap_or_else(|| (None, String::new()));
            let name = build_badge_name(&set_id, &version, &gql_title, badge_info);
            ProviderBadge {
                id: set_id,
                name,
                image_url,
            }
        })
        .collect()
}

/// IRC USERNOTICE メッセージを ProviderMessage に変換する。
///
/// 対応している msg-id:
/// - `viewermilestone` + `msg-param-category=watch-streak`: 連続視聴記録達成
fn build_usernotice_message(
    msg_id: &str,
    channel: &str,
    tags: &std::collections::HashMap<String, String>,
) -> Option<ProviderMessage> {
    match msg_id {
        "viewermilestone" => {
            if tags.get("msg-param-category").map(|s| s.as_str()) != Some("watch-streak") {
                return None;
            }
            let display_name = tags.get("display-name").cloned().unwrap_or_default();
            let user_id = tags.get("user-id").cloned().unwrap_or_default();
            let streak: u64 = tags
                .get("msg-param-value")
                .and_then(|v| v.parse().ok())
                .unwrap_or(0);
            let copo_reward: u64 = tags
                .get("msg-param-copoReward")
                .and_then(|v| v.parse().ok())
                .unwrap_or(0);

            let mut sender_parts = vec![MessagePart::Text {
                text: display_name.clone(),
            }];
            if copo_reward > 0 {
                sender_parts.push(MessagePart::Text {
                    text: format!(" + ⭕ {}", copo_reward),
                });
            }

            Some(ProviderMessage {
                id: Uuid::new_v4().to_string(),
                platform_message_id: tags.get("id").cloned(),
                service: ServiceId("twitch".to_string()),
                channel: ChannelId(channel.trim_start_matches('#').to_string()),
                sender: ProviderSender {
                    id: user_id,
                    display_name: sender_parts,
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp: tags
                    .get("tmi-sent-ts")
                    .and_then(|v| v.parse::<i64>().ok())
                    .map(|ms| ms / 1000)
                    .unwrap_or_else(|| chrono::Utc::now().timestamp()),
                kind: ProviderMessageKind::System(SystemKind::Notice),
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text {
                        text: format!(
                            "連続視聴記録達成！：{}さんは現在、{}連続視聴中です！",
                            display_name, streak
                        ),
                    }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }
        _ => None,
    }
}

// ───────────────────────────────────────────────
// バッジ・エモート ユーティリティ
// ───────────────────────────────────────────────

/// IRC バッジバージョンを BadgeCache のバージョン文字列に解決する。
///
/// Twitch IRC は実際の月数やビッツ数をバージョンとして送る（例: `subscriber/36`）が、
/// `broadcastBadges` には `0`, `6`, `12` など段階的なバージョンしかない場合がある。
/// ブラウザと同様に「リクエスト以下の最大バージョン」に fallback する。
fn resolve_badge_version(
    versions: &std::collections::HashMap<String, (String, String)>,
    requested: &str,
) -> Option<(String, String)> {
    // 完全一致を優先
    if let Some(entry) = versions.get(requested) {
        return Some(entry.clone());
    }
    // 数値として解釈し、requested 以下の最大バージョンを探す
    let requested_n: u64 = requested.parse().ok()?;
    versions
        .iter()
        .filter_map(|(v, entry)| {
            let n: u64 = v.parse().ok()?;
            (n <= requested_n).then_some((n, entry.clone()))
        })
        .max_by_key(|(n, _)| *n)
        .map(|(_, entry)| entry)
}

/// バッジ名を決定する。
///
/// subscriber/founder は badge-info タグの実際の月数を使って動的に生成する。
/// bits は IRC バージョン（ビット数）から日本語フォーマットで生成する。
/// ブラウザはバッジ定義の汎用タイトルではなく、これらの値を表示する。
fn build_badge_name(
    set_id: &str,
    irc_version: &str,
    gql_title: &str,
    badge_info: &std::collections::HashMap<String, String>,
) -> String {
    let parse_months =
        |key: &str| -> Option<u32> { badge_info.get(key).and_then(|v| v.parse().ok()) };

    match set_id {
        "subscriber" => {
            if let Some(n) = parse_months("subscriber") {
                return format!("{n}ヶ月のサブスクライバー");
            }
        }
        "founder" => {
            let base = if gql_title.is_empty() {
                "Founder".to_string()
            } else {
                gql_title.to_string()
            };
            if let Some(n) = parse_months("founder") {
                return format!("{base}、{n}ヶ月のサブスクライバー");
            }
            return base;
        }
        "bits" => {
            if let Ok(n) = irc_version.parse::<u64>() {
                return format!("cheer {}", format_bits_number(n));
            }
        }
        "premium" => return "Prime".to_string(),
        _ => {}
    }

    if gql_title.is_empty() {
        set_id.to_string()
    } else {
        gql_title.to_string()
    }
}

/// ビット数を日本語ロケールの表記に変換する。
///
/// - 10000 の倍数: 万単位（例: 10000 → "1万"、50000 → "5万"）
/// - それ以外: カンマ区切り（例: 5000 → "5,000"、1500 → "1,500"）
fn format_bits_number(n: u64) -> String {
    if n >= 10_000 && n % 10_000 == 0 {
        format!("{}万", n / 10_000)
    } else {
        let s = n.to_string();
        let mut result = String::new();
        for (i, c) in s.chars().rev().enumerate() {
            if i > 0 && i % 3 == 0 {
                result.push(',');
            }
            result.push(c);
        }
        result.chars().rev().collect()
    }
}

/// IRC `emotes` タグからエモートを解析し、テキストを MessagePart のリストに変換する。
///
/// `emotes` タグ形式: `{emote_id}:{start}-{end},{start}-{end}/{emote_id}:{start}-{end}`
/// 位置はメッセージテキストの文字インデックス（包含）。
///
/// エモート画像 URL: `https://static-cdn.jtvnw.net/emoticons/v2/{emote_id}/default/dark/2.0`
fn parse_emote_content(text: &str, emotes_tag: Option<&String>) -> Vec<MessagePart> {
    let emotes_str = match emotes_tag {
        Some(s) if !s.is_empty() => s,
        _ => {
            return vec![MessagePart::Text {
                text: text.to_string(),
            }]
        }
    };

    // (start, end_inclusive, emote_id) の一覧を構築
    let mut positions: Vec<(usize, usize, String)> = Vec::new();
    for emote_spec in emotes_str.split('/') {
        let mut parts = emote_spec.splitn(2, ':');
        let emote_id = match parts.next() {
            Some(id) if !id.is_empty() => id.to_string(),
            _ => continue,
        };
        let positions_str = match parts.next() {
            Some(s) => s,
            None => continue,
        };
        for pos_str in positions_str.split(',') {
            let mut range = pos_str.splitn(2, '-');
            let start: usize = match range.next().and_then(|s| s.parse().ok()) {
                Some(n) => n,
                None => continue,
            };
            let end: usize = match range.next().and_then(|s| s.parse().ok()) {
                Some(n) => n,
                None => continue,
            };
            positions.push((start, end, emote_id.clone()));
        }
    }

    if positions.is_empty() {
        return vec![MessagePart::Text {
            text: text.to_string(),
        }];
    }

    // 開始位置でソート
    positions.sort_by_key(|(start, _, _)| *start);

    // 文字単位で分割（Twitch IRC のエモート位置は文字インデックス）
    let chars: Vec<char> = text.chars().collect();
    let mut result: Vec<MessagePart> = Vec::new();
    let mut cursor = 0usize;

    for (start, end, emote_id) in &positions {
        let (start, end) = (*start, *end); // inclusive

        // エモート前のテキスト
        if cursor < start && start <= chars.len() {
            let before: String = chars[cursor..start].iter().collect();
            if !before.is_empty() {
                result.push(MessagePart::Text { text: before });
            }
        }

        // エモート画像
        if start <= end && end < chars.len() {
            let alt: String = chars[start..=end].iter().collect();
            result.push(MessagePart::Image {
                url: format!(
                    "https://static-cdn.jtvnw.net/emoticons/v2/{}/default/dark/2.0",
                    emote_id
                ),
                width: Some(28),
                height: Some(28),
                alt: Some(alt),
            });
            cursor = end + 1;
        }
    }

    // 末尾の残りテキスト
    if cursor < chars.len() {
        let tail: String = chars[cursor..].iter().collect();
        if !tail.is_empty() {
            result.push(MessagePart::Text { text: tail });
        }
    }

    if result.is_empty() {
        result.push(MessagePart::Text {
            text: text.to_string(),
        });
    }
    result
}

// ───────────────────────────────────────────────
// ユーティリティ
// ───────────────────────────────────────────────

/// WebSocket ストリームに IRC 行を送信する。
pub(crate) async fn send_irc_line(write: &mut WsWrite, line: &str) -> Result<(), WsError> {
    write
        .send(WsMessage::Text(format!("{line}\r\n").into()))
        .await
}
