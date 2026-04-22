use std::sync::Arc;

use tokio::sync::watch;
use uuid::Uuid;

use irc_lib::{client, error::IrcError, profile::ProfileEvent, session::IrcSession};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, ConnectFailedPayload, ConnectedPayload, DisconnectedPayload,
    McvEnvelope, Message as McvMessage, MessageDestination, MessagePart, MessageSource,
    MessageType, ModerationAction, ProviderBadge, ProviderContent, ProviderMessage,
    ProviderMessageKind, ProviderSender, ServiceId,
};
use plugin_abi_helper::v3::prelude::*;

use crate::profiles;

/// 1 つの IRC チャンネルへの接続を管理する
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    cancel_tx: Option<watch::Sender<bool>>,
}

impl Connection {
    pub fn new(id: Uuid) -> Self {
        Self {
            id,
            cancel_tx: None,
        }
    }

    #[allow(clippy::too_many_arguments)]
    pub fn connect(
        &mut self,
        ctx: PluginContext,
        plugin_id: mcv_messages::PluginId,
        host: String,
        port: u16,
        nick: String,
        pass: Option<String>,
        channel: String,
        use_tls: bool,
        server_type: ServerType,
    ) {
        let (cancel_tx, cancel_rx) = watch::channel(false);
        let conn_id = self.id;

        tokio::spawn(async move {
            if let Err(e) = run_irc(
                ctx.clone(),
                plugin_id.clone(),
                conn_id,
                &host,
                port,
                &nick,
                pass.as_deref(),
                &channel,
                use_tls,
                server_type,
                cancel_rx,
            )
            .await
            {
                tracing::error!(
                    target: "mcv::plugin-irc",
                    conn_id = %conn_id,
                    error = %e,
                    "IRC接続エラー"
                );
                let fail_msg = McvMessage::new_notification(
                    MessageType::ConnectFailed,
                    MessageSource::Plugin { plugin_id },
                    MessageDestination::Core,
                    serde_json::to_value(ConnectFailedPayload {
                        connection_id: conn_id,
                        reason: e.to_string(),
                    })
                    .unwrap(),
                );
                ctx.send_notification(fail_msg).await.ok();
            }
        });

        self.cancel_tx = Some(cancel_tx);
    }

    pub fn stop(&mut self) {
        if let Some(tx) = self.cancel_tx.take() {
            let _ = tx.send(true);
        }
    }
}

/// サーバー種別（プロファイル選択に使用）
pub(crate) enum ServerType {
    Rfc,
    Twitch { anon: bool },
}

impl ServerType {
    pub fn from_str(s: &str) -> Self {
        match s {
            "twitch" => ServerType::Twitch { anon: true },
            _ => ServerType::Rfc,
        }
    }

    pub fn into_profile(self) -> Arc<dyn irc_lib::profile::ServerProfile> {
        match self {
            ServerType::Rfc => Arc::new(irc_lib::RfcProfile),
            ServerType::Twitch { anon: true } => {
                let n: u32 = rand::random::<u32>() % 80000 + 1000;
                Arc::new(profiles::TwitchIrcProfile::anonymous(n))
            }
            ServerType::Twitch { anon: false } => {
                Arc::new(profiles::TwitchIrcProfile::authenticated())
            }
        }
    }
}

#[allow(clippy::too_many_arguments)]
async fn run_irc(
    ctx: PluginContext,
    plugin_id: mcv_messages::PluginId,
    conn_id: Uuid,
    host: &str,
    port: u16,
    nick: &str,
    pass: Option<&str>,
    channel: &str,
    use_tls: bool,
    server_type: ServerType,
    cancel_rx: watch::Receiver<bool>,
) -> Result<(), IrcError> {
    tracing::info!(
        target: "mcv::plugin-irc",
        conn_id = %conn_id,
        host = %host,
        port = %port,
        channel = %channel,
        use_tls = %use_tls,
        "IRC接続開始"
    );

    let profile = server_type.into_profile();

    let (reader, writer) = if use_tls {
        client::connect_tls(host, port).await?
    } else {
        client::connect_tcp(host, port).await?
    };

    let mut session = IrcSession::new(reader, writer, profile);
    session.authenticate(nick, pass).await?;
    session.join_channel(channel).await?;

    tracing::info!(
        target: "mcv::plugin-irc",
        conn_id = %conn_id,
        channel = %channel,
        "IRC チャンネル参加完了"
    );

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
    if let Err(e) = ctx.send_notification(connected_msg).await {
        tracing::warn!(target: "mcv::plugin-irc", error = %e, "Connected 送信失敗");
    }

    let service_id = ServiceId("irc".to_string());
    let channel_id = ChannelId(channel.to_string());

    session
        .run_loop(cancel_rx, {
            let ctx = ctx.clone();
            let plugin_id = plugin_id.clone();
            move |event| {
                on_profile_event(
                    event,
                    ctx.clone(),
                    plugin_id.clone(),
                    conn_id,
                    service_id.clone(),
                    channel_id.clone(),
                );
            }
        })
        .await;

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

    Ok(())
}

fn on_profile_event(
    event: ProfileEvent,
    ctx: PluginContext,
    plugin_id: mcv_messages::PluginId,
    conn_id: Uuid,
    service_id: ServiceId,
    channel_id: ChannelId,
) {
    match event {
        ProfileEvent::Chat(chat) => {
            let msg_id = chat
                .message_id
                .clone()
                .unwrap_or_else(|| Uuid::new_v4().to_string());

            let display_name_parts = vec![MessagePart::Text {
                text: chat
                    .display_name
                    .clone()
                    .unwrap_or_else(|| chat.login_name.clone()),
            }];

            let provider_msg = ProviderMessage {
                id: msg_id,
                platform_message_id: chat.message_id,
                service: service_id,
                channel: channel_id,
                sender: ProviderSender {
                    id: chat.user_id.unwrap_or_else(|| chat.login_name.clone()),
                    display_name: display_name_parts,
                    role: None,
                    badges: chat
                        .badges
                        .iter()
                        .map(|b| ProviderBadge {
                            id: b.clone(),
                            name: b.clone(),
                            image_url: None,
                        })
                        .collect(),
                    avatar_url: None,
                },
                timestamp: chat
                    .timestamp
                    .unwrap_or_else(|| chrono::Utc::now().timestamp()),
                kind: ProviderMessageKind::Chat,
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: chat.body }],
                },
                reply_to: None,
                metadata: chat.extra,
            };

            let envelope = McvEnvelope {
                event_id: Uuid::new_v4(),
                connection_id: conn_id,
                messages: vec![provider_msg],
                received_at: chrono::Utc::now().timestamp(),
                raw_message: None,
            };

            let comment_msg = McvMessage::new_notification(
                MessageType::CommentReceived,
                MessageSource::Plugin { plugin_id },
                MessageDestination::Core,
                serde_json::to_value(CommentReceivedPayload {
                    connection_id: conn_id,
                    envelope,
                })
                .unwrap(),
            );
            tokio::spawn(async move {
                if let Err(e) = ctx.send_notification(comment_msg).await {
                    tracing::warn!(
                        target: "mcv::plugin-irc",
                        error = %e,
                        "CommentReceived 送信失敗"
                    );
                }
            });
        }

        ProfileEvent::ClearChat {
            target_user,
            target_user_id,
        } => {
            // target_user_id を優先し、なければ target_user（ログイン名）を使用
            if let Some(user_id) = target_user_id.or(target_user) {
                let msg = build_moderation_event(
                    conn_id,
                    plugin_id,
                    service_id,
                    channel_id,
                    ProviderMessageKind::Moderation(ModerationAction::Ban {
                        target_user_id: user_id,
                    }),
                );
                tokio::spawn(async move {
                    ctx.send_notification(msg).await.ok();
                });
            }
        }

        ProfileEvent::ClearMsg { message_id } => {
            let msg = build_moderation_event(
                conn_id,
                plugin_id,
                service_id,
                channel_id,
                ProviderMessageKind::Moderation(ModerationAction::Delete {
                    target_message_id: message_id,
                }),
            );
            tokio::spawn(async move {
                ctx.send_notification(msg).await.ok();
            });
        }

        ProfileEvent::UserNotice {
            event_type,
            user_login,
            system_msg,
        } => {
            tracing::debug!(
                target: "mcv::plugin-irc",
                event_type = %event_type,
                user_login = %user_login,
                system_msg = ?system_msg,
                "USERNOTICE 受信"
            );
        }

        ProfileEvent::SystemMessage(text) => {
            tracing::info!(
                target: "mcv::plugin-irc",
                message = %text,
                "IRC システムメッセージ"
            );
        }
    }
}

fn build_moderation_event(
    conn_id: Uuid,
    plugin_id: mcv_messages::PluginId,
    service_id: ServiceId,
    channel_id: ChannelId,
    kind: ProviderMessageKind,
) -> McvMessage {
    let envelope = McvEnvelope {
        event_id: Uuid::new_v4(),
        connection_id: conn_id,
        messages: vec![ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: service_id,
            channel: channel_id,
            sender: ProviderSender {
                id: String::new(),
                display_name: vec![],
                role: None,
                badges: vec![],
                avatar_url: None,
            },
            timestamp: chrono::Utc::now().timestamp(),
            kind,
            content: ProviderContent::Empty,
            reply_to: None,
            metadata: serde_json::Value::Null,
        }],
        received_at: chrono::Utc::now().timestamp(),
        raw_message: None,
    };
    McvMessage::new_notification(
        MessageType::CommentReceived,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(CommentReceivedPayload {
            connection_id: conn_id,
            envelope,
        })
        .unwrap(),
    )
}
