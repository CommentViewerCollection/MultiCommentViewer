//! YouTube Live接続管理
//!
//! 個々のYouTube Live配信への接続を管理し、
//! ライブチャットメッセージを定期的に取得します。

use futures_util::{stream::SplitSink, FutureExt, SinkExt, StreamExt};
use mcv_messages::{
    AccountInfo, ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope,
    Message as McvMessage, MessageDestination, MessagePart, MessageSource, MessageType,
    ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
    UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::fs::{File, OpenOptions};
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
use twitch_lib::irc::{parse_irc_line, to_twitch_event, TwitchEvent};
use uuid::Uuid;

use crate::TwitchPlugin;

/// YouTube Live配信への接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

type WsWrite = SplitSink<WebSocketStream<MaybeTlsStream<TcpStream>>, WsMessage>;

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
        let channel_id = match Self::extract_channel_id(url) {
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
            channel_id = channel_id,
            "channel_idの抽出に成功"
        );

        let (cancel_tx, cancel_rx) = watch::channel(false);
        let connection_id = self.id;
        let url = "wss://irc-ws.chat.twitch.tv/";
        let username = username.to_string();
        let password = auth_token.map(|a| format!("oauth:{}", a.value()));
        let join_command = format!("JOIN #{channel_id}");
        let task = match Self::start_connection_task(
            ctx,
            logical_plugin_id,
            connection_id,
            url.to_string(),
            username,
            password,
            join_command,
            cancel_rx,
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
        url: String,
        username: String,
        password: Option<String>,
        join_command: String,
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
                                            logical_plugin_id,
                                            connection_id,
                                            &mut write,
                                            text.to_string(),
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
                        plugin_id: logical_plugin_id,
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
                        plugin_id: logical_plugin_id,
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

    async fn send_irc_line(write: &mut WsWrite, line: &str) -> Result<(), WsError> {
        let payload = format!("{line}\r\n");
        write.send(WsMessage::Text(payload.into())).await
    }

    async fn handle_text_message(
        ctx: PluginContext,
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        write: &mut WsWrite,
        raw_text: String,
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
                    let provider_msg = ProviderMessage {
                        id: Uuid::new_v4().to_string(),
                        platform_message_id: None,
                        service: ServiceId("twitch".to_string()),
                        channel: ChannelId(channel.clone()),
                        sender: ProviderSender {
                            id: user.clone(),
                            display_name: vec![MessagePart::Text { text: user.clone() }],
                            badges: vec![],
                            role: None,
                            avatar_url: None,
                        },
                        timestamp: chrono::Utc::now().timestamp(),
                        kind: ProviderMessageKind::Chat,
                        content: ProviderContent::Text {
                            text: vec![MessagePart::Text {
                                text: text.to_string(),
                            }],
                        },
                        reply_to: None,
                        metadata: serde_json::Value::Null,
                    };
                    provider_messages.push(provider_msg);
                }
                TwitchEvent::GlobalUserState {
                    display_name,
                    user_id,
                } => {
                    if let Some(name) = display_name {
                        let account_msg = McvMessage::new_notification(
                            MessageType::UpdateConnectionAccount,
                            MessageSource::Plugin {
                                plugin_id: logical_plugin_id,
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
                }
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
                    plugin_id: logical_plugin_id,
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

    async fn send_comment_if_privmsg(
        ctx: PluginContext,
        logical_plugin_id: Uuid,
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
                    plugin_id: logical_plugin_id,
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
