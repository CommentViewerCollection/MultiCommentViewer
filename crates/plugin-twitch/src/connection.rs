//! Twitch 接続管理
//!
//! 個々の Twitch チャンネルへの接続を管理し、
//! IRC 経由でコメントを取得します。
//! また、定期的にストリームメタデータ（視聴者数・配信開始時刻）を取得します。

use std::sync::Arc;

use futures_util::{stream::SplitSink, FutureExt, SinkExt, StreamExt};
use mcv_messages::{
    AccountInfo, ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope,
    Message as McvMessage, MessageDestination, MessagePart, MessageSource, MessageType, PluginId,
    ProviderBadge, ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender,
    ServiceId, StreamMetadataPayload, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::fs::OpenOptions;
use tokio::io::AsyncWriteExt;
use tokio::net::TcpStream;
use tokio::sync::watch;
use tokio::task::JoinHandle;
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

/// Twitch チャンネルへの接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    /// メタデータポーリングタスク用キャンセル送信側
    pub(crate) metadata_cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

type WsWrite = SplitSink<WebSocketStream<MaybeTlsStream<TcpStream>>, WsMessage>;

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            metadata_cancel_tx: None,
            task: None,
            running: false,
        }
    }

    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        url: &str,
        username: &str,
        auth_token: Option<AuthToken>,
    ) {
        tracing::trace!(
            target: "mcv::plugin-twitch",
            url = url,
            "connect()"
        );
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-twitch",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }
        let channel_login = match Self::extract_channel_id(url) {
            Some(channel) => channel,
            None => {
                tracing::debug!(
                    target: "mcv::plugin-twitch",
                    url = url,
                    "channel_idの抽出に失敗"
                );
                return;
            }
        };
        tracing::trace!(
            target: "mcv::plugin-twitch",
            channel_id = channel_login,
            "channel_idの抽出に成功"
        );

        let (cancel_tx, cancel_rx) = watch::channel(false);
        let connection_id = self.id;
        let ws_url = "wss://irc-ws.chat.twitch.tv/";
        let username = username.to_string();
        let password = auth_token.as_ref().map(|a| format!("oauth:{}", a.value()));
        let join_command = format!("JOIN #{channel_login}");
        let task = match Self::start_connection_task(
            ctx.clone(),
            logical_plugin_id.clone(),
            connection_id,
            ws_url.to_string(),
            username,
            password,
            join_command,
            channel_login.clone(),
            auth_token.clone(),
            cancel_rx,
        ) {
            Some(task) => task,
            None => return,
        };

        // メタデータポーリングタスクを起動
        let (metadata_cancel_tx, metadata_cancel_rx) = watch::channel(false);
        Self::start_metadata_polling_task(
            ctx,
            logical_plugin_id,
            connection_id,
            channel_login,
            auth_token,
            metadata_cancel_rx,
        );

        self.cancel_tx = Some(cancel_tx);
        self.metadata_cancel_tx = Some(metadata_cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    #[allow(clippy::too_many_arguments)]
    fn start_connection_task(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        url: String,
        username: String,
        password: Option<String>,
        join_command: String,
        channel_login: String,
        auth_token: Option<AuthToken>,
        mut cancel_rx: watch::Receiver<bool>,
    ) -> Option<JoinHandle<()>> {
        let runtime_handle = match tokio::runtime::Handle::try_current() {
            Ok(handle) => handle,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    error = %e,
                    "Tokio runtime is not available; cannot spawn Twitch IRC task"
                );
                return None;
            }
        };

        let task = runtime_handle.spawn(async move {
            let task_result = std::panic::AssertUnwindSafe(async {
                let run_result: Result<(), ()> = async {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        ws_url = %url,
                        "Attempting to connect Twitch IRC WebSocket"
                    );

                    let connect_result =
                        timeout(Duration::from_secs(15), connect_async(&url)).await;
                    let (ws_stream, _response) = match connect_result {
                        Err(_) => {
                            tracing::error!(
                                target: "mcv::plugin-twitch",
                                connection_id = %connection_id,
                                ws_url = %url,
                                "Timed out while connecting Twitch IRC WebSocket"
                            );
                            return Err(());
                        }
                        Ok(Err(e)) => {
                            tracing::error!(
                                target: "mcv::plugin-twitch",
                                connection_id = %connection_id,
                                error = %e,
                                "Failed to connect to Twitch IRC WebSocket"
                            );
                            return Err(());
                        }
                        Ok(Ok(v)) => v,
                    };

                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Twitch IRC WebSocket connected"
                    );

                    let (mut write, mut read) = ws_stream.split();

                    let nick = if password.is_some() {
                        username
                    } else {
                        // 匿名接続時は justinfan ユーザーを使う
                        format!("justinfan{}", (connection_id.as_u128() % 1_000_000) as u64)
                    };

                    let mut init_commands = vec![
                        "CAP REQ :twitch.tv/tags twitch.tv/commands twitch.tv/membership"
                            .to_string(),
                    ];
                    if let Some(password) = &password {
                        init_commands.push(format!("PASS {password}"));
                    }
                    init_commands.push(format!("NICK {nick}"));

                    for command in init_commands {
                        Connection::send_irc_line(&mut write, &command)
                            .await
                            .map_err(|e| {
                                tracing::error!(
                                    target: "mcv::plugin-twitch",
                                    connection_id = %connection_id,
                                    command = %command,
                                    error = %e,
                                    "Failed to send IRC init command"
                                );
                            })?;
                    }
                    Connection::send_irc_line(&mut write, &join_command)
                        .await
                        .map_err(|e| {
                            tracing::error!(
                                target: "mcv::plugin-twitch",
                                connection_id = %connection_id,
                                command = join_command,
                                error = %e,
                                "Failed to send IRC init command"
                            );
                        })?;

                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Connected to Twitch IRC and initialized session"
                    );

                    // 直近の過去コメント履歴を取得して送信
                    let client_id = twitch_lib::ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
                    match twitch_lib::fetch_recent_chat_messages(
                        &channel_login,
                        &client_id,
                        auth_token.as_ref(),
                    )
                    .await
                    {
                        Ok(history) => {
                            let provider_messages: Vec<ProviderMessage> = history
                                .into_iter()
                                .filter(|msg| msg.deleted_at.is_none())
                                .map(|msg| {
                                    let timestamp =
                                        chrono::DateTime::parse_from_rfc3339(&msg.sent_at)
                                            .map(|dt| dt.timestamp())
                                            .unwrap_or_else(|_| chrono::Utc::now().timestamp());
                                    ProviderMessage {
                                        id: Uuid::new_v4().to_string(),
                                        platform_message_id: Some(msg.message_id),
                                        service: ServiceId("twitch".to_string()),
                                        channel: ChannelId(channel_login.clone()),
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
                            if !provider_messages.is_empty() {
                                let envelope = McvEnvelope {
                                    event_id: Uuid::new_v4(),
                                    connection_id,
                                    messages: provider_messages,
                                    received_at: chrono::Utc::now().timestamp(),
                                    raw_message: None,
                                };
                                let history_msg = McvMessage::new_notification(
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
                                TwitchPlugin::send_message(ctx.clone(), history_msg).await;
                            }
                        }
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-twitch",
                                connection_id = %connection_id,
                                error = %e,
                                "直近コメント履歴の取得に失敗"
                            );
                        }
                    }

                    let client_id = twitch_lib::ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");

                    // グローバルバッジを GQL GlobalBadges で取得（認証不要）
                    let mut merged_badges = match fetch_global_badges_gql(
                        &client_id,
                        auth_token.as_ref(),
                    )
                    .await
                    {
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

                    // チャンネルバッジを GQL ChatList_Badges で取得し、グローバルに重ねる
                    match fetch_channel_badges_gql(
                        &channel_login,
                        &client_id,
                        auth_token.as_ref(),
                    )
                    .await
                    {
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
                                merged_badges.entry(set_id).or_default().extend(versions);
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
                        badge_set_count = merged_badges.len(),
                        total_badge_count = merged_badges.values().map(|v| v.len()).sum::<usize>(),
                        "バッジキャッシュ確定"
                    );
                    let badge_cache: Arc<BadgeCache> = Arc::new(merged_badges);

                    let mut ping_interval = interval(Duration::from_secs(300));

                    loop {
                        tracing::trace!(
                            target: "mcv::plugin-twitch",
                            connection_id = %connection_id,
                            "loop is ok"
                        );
                        tokio::select! {
                            _ = cancel_rx.changed() => {
                                if *cancel_rx.borrow() {
                                    tracing::info!(
                                        target: "mcv::plugin-twitch",
                                        connection_id = %connection_id,
                                        "Twitch IRC loop cancelled"
                                    );
                                    break;
                                }
                            }
                            _ = ping_interval.tick() => {
                                if let Err(e) = Connection::send_irc_line(
                                    &mut write,
                                    "PING :tmi.twitch.tv",
                                ).await {
                                    tracing::error!(
                                        target: "mcv::plugin-twitch",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "Failed to send periodic PING"
                                    );
                                    break;
                                }
                            }
                            recv = read.next() => {
                                match recv {
                                    Some(Ok(WsMessage::Text(text))) => {
                                        let should_continue = Self::handle_text_message(
                                            ctx.clone(),
                                            logical_plugin_id.clone(),
                                            connection_id,
                                            &mut write,
                                            text.to_string(),
                                            Arc::clone(&badge_cache),
                                        ).await;
                                        if !should_continue {
                                            break;
                                        }
                                    }
                                    Some(Ok(WsMessage::Close(frame))) => {
                                        tracing::info!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            close_frame = ?frame,
                                            "WebSocket closed by server"
                                        );
                                        break;
                                    }
                                    Some(Ok(WsMessage::Binary(_))) => {
                                        tracing::trace!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            "Received binary frame"
                                        );
                                    }
                                    Some(Ok(WsMessage::Ping(_))) => {
                                        tracing::trace!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            "Received websocket ping frame"
                                        );
                                    }
                                    Some(Ok(WsMessage::Pong(_))) => {
                                        tracing::trace!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            "Received websocket pong frame"
                                        );
                                    }
                                    Some(Ok(WsMessage::Frame(_))) => {
                                        tracing::trace!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            "Received websocket raw frame"
                                        );
                                    }
                                    Some(Err(e)) => {
                                        tracing::error!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "Error while receiving websocket message"
                                        );
                                        break;
                                    }
                                    None => {
                                        tracing::info!(
                                            target: "mcv::plugin-twitch",
                                            connection_id = %connection_id,
                                            "WebSocket stream ended"
                                        );
                                        break;
                                    }
                                }
                            }
                        }
                    }

                    Ok(())
                }
                .await;

                if run_result.is_err() {
                    tracing::debug!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        "Twitch IRC task terminated due to earlier error"
                    );
                }

                // 切断前にアカウント情報をクリア
                let clear_account = McvMessage::new_notification(
                    MessageType::UpdateConnectionAccount,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(UpdateConnectionAccountPayload {
                        connection_id,
                        account: None,
                    })
                    .unwrap(),
                );
                TwitchPlugin::send_message(ctx.clone(), clear_account).await;

                let message = McvMessage::new_notification(
                    MessageType::Disconnected,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                );
                TwitchPlugin::send_message(ctx.clone(), message).await;
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
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    panic = %panic_message,
                    "Twitch IRC task panicked"
                );
            }
        });
        Some(task)
    }

    fn start_metadata_polling_task(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        channel_login: String,
        auth_token: Option<AuthToken>,
        cancel_rx: watch::Receiver<bool>,
    ) {
        let runtime_handle = match tokio::runtime::Handle::try_current() {
            Ok(handle) => handle,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twitch",
                    connection_id = %connection_id,
                    error = %e,
                    "Tokio runtime is not available; cannot spawn metadata polling task"
                );
                return;
            }
        };

        runtime_handle.spawn(metadata_polling_loop(
            ctx,
            logical_plugin_id,
            connection_id,
            channel_login,
            auth_token,
            cancel_rx,
        ));
    }

    async fn send_irc_line(write: &mut WsWrite, line: &str) -> Result<(), WsError> {
        let payload = format!("{line}\r\n");
        write.send(WsMessage::Text(payload.into())).await
    }

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
                if n <= requested_n {
                    Some((n, entry.clone()))
                } else {
                    None
                }
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
        let parse_months = |key: &str| -> Option<u32> {
            badge_info.get(key).and_then(|v| v.parse().ok())
        };

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
                    return format!("cheer {}", Self::format_bits_number(n));
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
            // 3桁ごとにカンマ区切り
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
            let start = *start;
            let end = *end; // inclusive

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

    async fn handle_text_message(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        write: &mut WsWrite,
        raw_text: String,
        badge_cache: Arc<BadgeCache>,
    ) -> bool {
        // 1 WebSocket フレーム内の全 PrivMsg を収集して1つの McvEnvelope にまとめる
        let mut provider_messages = Vec::new();
        for line in raw_text.lines() {
            let irc = parse_irc_line(line);
            let mut text_log_file = OpenOptions::new()
                .create(true)
                .append(true)
                .open(format!("twitch_{}.txt", &irc.command))
                .await
                .unwrap();
            let _ = text_log_file
                .write_all(format!("{line}\n").as_bytes())
                .await;
            let event = to_twitch_event(irc);

            match event {
                TwitchEvent::PrivMsg {
                    channel,
                    user,
                    text,
                    tags,
                } => {
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

                    // IRC tags の badges フィールドからバッジ情報を解析
                    // 形式: "moderator/1,subscriber/36"
                    let badges: Vec<ProviderBadge> = if let Some(badges_str) = tags.get("badges") {
                        badges_str
                            .split(',')
                            .filter(|s| !s.is_empty())
                            .map(|badge| {
                                let mut parts = badge.splitn(2, '/');
                                let set_id = parts.next().unwrap_or("").to_string();
                                let version = parts.next().unwrap_or("1").to_string();
                                let resolved = badge_cache.get(&set_id).and_then(|versions| {
                                    Self::resolve_badge_version(versions, &version)
                                });
                                let (image_url, gql_title) = resolved
                                    .map(|(url, t)| (Some(url), t))
                                    .unwrap_or_else(|| (None, String::new()));
                                let name = Self::build_badge_name(
                                    &set_id,
                                    &version,
                                    &gql_title,
                                    &badge_info,
                                );
                                ProviderBadge {
                                    id: set_id,
                                    name,
                                    image_url,
                                }
                            })
                            .collect()
                    } else {
                        vec![]
                    };
                    let provider_msg = ProviderMessage {
                        id: Uuid::new_v4().to_string(),
                        platform_message_id: None,
                        service: ServiceId("twitch".to_string()),
                        channel: ChannelId(channel.clone()),
                        sender: ProviderSender {
                            id: user.clone(),
                            display_name: vec![MessagePart::Text { text: user.clone() }],
                            badges,
                            role: None,
                            avatar_url: None,
                        },
                        timestamp: chrono::Utc::now().timestamp(),
                        kind: ProviderMessageKind::Chat,
                        content: ProviderContent::Text {
                            text: Self::parse_emote_content(&text, tags.get("emotes")),
                        },
                        reply_to: None,
                        metadata: serde_json::Value::Null,
                    };
                    provider_messages.push(provider_msg);
                }
                TwitchEvent::GlobalUserState {
                    display_name: Some(name),
                    user_id,
                } => {
                    let account_msg = McvMessage::new_notification(
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
                    TwitchPlugin::send_message(ctx.clone(), account_msg).await;
                }
                TwitchEvent::GlobalUserState { .. } => {}
                TwitchEvent::Ping => {
                    if let Err(e) = Self::send_irc_line(write, "PONG").await {
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
            let comment_message = McvMessage::new_notification(
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
            TwitchPlugin::send_message(ctx, comment_message).await;
        }
        true
    }

    #[allow(dead_code)]
    async fn send_comment_if_privmsg(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        line: &str,
    ) {
        if let Some(comment_text) = Self::extract_privmsg_text(line) {
            let provider_msg = ProviderMessage {
                id: Uuid::new_v4().to_string(),
                platform_message_id: None,
                service: ServiceId("twitch".to_string()),
                channel: ChannelId("unknown".to_string()),
                sender: ProviderSender {
                    id: "unknown".to_string(),
                    display_name: vec![MessagePart::Text {
                        text: "unknown".to_string(),
                    }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp: chrono::Utc::now().timestamp(),
                kind: ProviderMessageKind::Chat,
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text {
                        text: comment_text.to_string(),
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
                raw_message: Some(line.to_owned()),
            };
            let comment_message = McvMessage::new_notification(
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
            TwitchPlugin::send_message(ctx, comment_message).await;
        }
    }

    pub(crate) fn extract_channel_id(url: &str) -> Option<String> {
        let trimmed = url.trim();
        let path = if let Some(rest) = trimmed.strip_prefix("https://www.twitch.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("http://www.twitch.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("https://twitch.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("http://twitch.tv/") {
            rest
        } else {
            return None;
        };

        let channel = path
            .split(['/', '?', '#'])
            .next()
            .unwrap_or_default()
            .trim();

        if channel.is_empty() {
            return None;
        }

        Some(channel.to_string())
    }

    #[allow(dead_code)]
    fn extract_privmsg_text(line: &str) -> Option<&str> {
        let privmsg_index = line.find(" PRIVMSG ")?;
        let after_privmsg = &line[privmsg_index..];
        let text_index = after_privmsg.find(" :")?;
        Some(after_privmsg[(text_index + 2)..].trim())
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        if let Some(tx) = &self.metadata_cancel_tx {
            let _ = tx.send(true);
        }
        // abort()を使わず、cancel_txでタスクを正常終了させる
        // これにより、loopを抜けた後のDisconnectedメッセージ送信が実行される
        self.cancel_tx = None;
        self.metadata_cancel_tx = None;
        self.task = None;
        self.running = false;
    }
}

/// Twitch ストリームメタデータ（視聴者数・配信開始時刻・タイトル）を定期的に取得して Core に送信する
async fn metadata_polling_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: String,
    auth_token: Option<AuthToken>,
    mut cancel_rx: watch::Receiver<bool>,
) {
    const POLL_INTERVAL: Duration = Duration::from_secs(60);
    const RETRY_INTERVAL: Duration = Duration::from_secs(30);

    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        channel = %channel_login,
        "メタデータポーリング開始"
    );

    let mut poll_count: u64 = 0;

    loop {
        if *cancel_rx.borrow() {
            tracing::info!(
                target: "mcv::plugin-twitch",
                connection_id = %connection_id,
                poll_count,
                "メタデータポーリング: キャンセル受信、終了"
            );
            break;
        }

        poll_count += 1;
        tracing::debug!(
            target: "mcv::plugin-twitch",
            connection_id = %connection_id,
            poll_count,
            "メタデータポーリング: fetch_stream_info 呼び出し"
        );

        let sleep_duration =
            match twitch_lib::fetch_stream_info(&channel_login, auth_token.as_ref()).await {
                Ok(info) => {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        poll_count,
                        title = ?info.title,
                        viewer_count = ?info.viewer_count,
                        start_time = ?info.start_time,
                        "メタデータポーリング: 取得成功、StreamMetadata 送信"
                    );
                    let payload = StreamMetadataPayload {
                        connection_id,
                        title: info.title,
                        viewer_count: info.viewer_count,
                        total_viewer_count: None,
                        start_time: info.start_time,
                        others: None,
                    };
                    let msg = McvMessage::new_notification(
                        MessageType::StreamMetadata,
                        MessageSource::Plugin {
                            plugin_id: logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(payload).unwrap(),
                    );
                    TwitchPlugin::send_message(ctx.clone(), msg).await;
                    POLL_INTERVAL
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        poll_count,
                        error = %e,
                        "メタデータポーリング: 取得失敗、{}秒後にリトライ",
                        RETRY_INTERVAL.as_secs()
                    );
                    RETRY_INTERVAL
                }
            };

        tokio::select! {
            _ = tokio::time::sleep(sleep_duration) => {}
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        poll_count,
                        "メタデータポーリング: 待機中にキャンセル受信、終了"
                    );
                    break;
                }
            }
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
        assert!(conn.metadata_cancel_tx.is_none());
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
        assert!(conn.metadata_cancel_tx.is_none());
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

    #[test]
    fn test_extract_channel_id() {
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/amal_3rd"),
            Some("amal_3rd".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/aoi_sakura3"),
            Some("aoi_sakura3".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/hatsukaneru"),
            Some("hatsukaneru".to_string())
        );
    }
}
