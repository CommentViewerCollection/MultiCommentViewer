use crate::TwicasPlugin;
use futures_util::{FutureExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, ProviderContent,
    ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
};
use plugin_abi_helper::v3::prelude::*;
use serde::Deserialize;
use serde_json::Value;
use std::sync::{
    Arc,
    atomic::{AtomicBool, Ordering},
};
use tokio::sync::watch;
use tokio::task::JoinHandle;
use tokio::time::{timeout, Duration};
use tokio_tungstenite::{connect_async, tungstenite::Message as WsMessage};
use twicas_lib::{TwicasSession, fetch_event_pubsub_url, fetch_latest_movie};
use uuid::Uuid;

pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: Arc<AtomicBool>,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            task: None,
            running: Arc::new(AtomicBool::new(false)),
        }
    }

    pub(crate) fn connect(&mut self, ctx: PluginContext, logical_plugin_id: Uuid, user_name: &str) {
        if self.running.load(Ordering::Relaxed) {
            tracing::debug!(
                target: "mcv::plugin-twicas",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }

        let (cancel_tx, mut cancel_rx) = watch::channel(false);
        let connection_id = self.id;
        let user_name = user_name.to_string();
        let running_flag = Arc::clone(&self.running);

        let task = tokio::spawn(async move {
            let task_result = std::panic::AssertUnwindSafe(async {
                let run_result: Result<(), ()> = async {
                    let client = reqwest::Client::new();

                    let session = match TwicasSession::from_user(&client, &user_name).await {
                        Ok(v) => v,
                        Err(e) => {
                            tracing::error!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                user_name = %user_name,
                                error = %e,
                                "Failed to fetch TwicasSession"
                            );
                            return Err(());
                        }
                    };

                    let latest_movie =
                        match fetch_latest_movie(&client, &session, &user_name, None).await {
                            Ok(v) => v,
                            Err(e) => {
                                tracing::error!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connection_id,
                                    user_name = %user_name,
                                    error = %e,
                                    "Failed to fetch latest movie"
                                );
                                return Err(());
                            }
                        };

                    let movie = match latest_movie.movie {
                        Some(v) if v.is_on_live => v,
                        Some(_) => {
                            tracing::info!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                user_name = %user_name,
                                "User is not on live"
                            );
                            return Ok(());
                        }
                        None => {
                            tracing::info!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                user_name = %user_name,
                                "No movie found"
                            );
                            return Ok(());
                        }
                    };

                    let ws_url = match fetch_event_pubsub_url(&client, movie.id, None).await {
                        Ok(v) => v,
                        Err(e) => {
                            tracing::error!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                movie_id = movie.id,
                                error = %e,
                                "Failed to fetch event pubsub URL"
                            );
                            return Err(());
                        }
                    };

                    tracing::info!(
                        target: "mcv::plugin-twicas",
                        connection_id = %connection_id,
                        movie_id = movie.id,
                        "Connecting to Twicas event pubsub websocket"
                    );

                    let connect_result =
                        timeout(Duration::from_secs(15), connect_async(&ws_url)).await;
                    let (ws_stream, _response) = match connect_result {
                        Err(_) => {
                            tracing::error!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                "Timed out while connecting websocket"
                            );
                            return Err(());
                        }
                        Ok(Err(e)) => {
                            tracing::error!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                error = %e,
                                "Failed to connect websocket"
                            );
                            return Err(());
                        }
                        Ok(Ok(v)) => v,
                    };

                    let (_write, mut read) = ws_stream.split();
                    loop {
                        tokio::select! {
                            _ = cancel_rx.changed() => {
                                if *cancel_rx.borrow() {
                                    tracing::info!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connection_id,
                                        "Twicas websocket loop cancelled"
                                    );
                                    break;
                                }
                            }
                            recv = read.next() => {
                                match recv {
                                    Some(Ok(WsMessage::Text(text))) => {
                                        handle_text_message(
                                            ctx.clone(),
                                            logical_plugin_id,
                                            connection_id,
                                            &text,
                                        ).await;
                                    }
                                    Some(Ok(WsMessage::Close(frame))) => {
                                        tracing::info!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            close_frame = ?frame,
                                            "WebSocket closed by server"
                                        );
                                        break;
                                    }
                                    Some(Ok(_)) => {}
                                    Some(Err(e)) => {
                                        tracing::error!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "Error while receiving websocket message"
                                        );
                                        break;
                                    }
                                    None => {
                                        tracing::info!(
                                            target: "mcv::plugin-twicas",
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
                        target: "mcv::plugin-twicas",
                        connection_id = %connection_id,
                        "Twicas connection task terminated due to earlier error"
                    );
                }

                // running フラグを先にクリアすることで、Disconnected 受信後の
                // 即時再接続要求が connect() で弾かれないようにする
                running_flag.store(false, Ordering::Relaxed);

                let message = McvMessage::new_notification(
                    MessageType::Disconnected,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                );
                TwicasPlugin::send_message(ctx, message).await;
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
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    panic = %panic_message,
                    "Twicas task panicked"
                );
            }
        });

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running.store(true, Ordering::Relaxed);
    }

    pub(crate) fn extract_user_name(url: &str) -> Option<String> {
        let trimmed = url.trim();
        let path = if let Some(rest) = trimmed.strip_prefix("https://twitcasting.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("http://twitcasting.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("https://www.twitcasting.tv/") {
            rest
        } else if let Some(rest) = trimmed.strip_prefix("http://www.twitcasting.tv/") {
            rest
        } else {
            return None;
        };

        let user = path
            .split(['/', '?', '#'])
            .next()
            .unwrap_or_default()
            .trim();
        if user.is_empty() {
            return None;
        }
        Some(user.to_string())
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        self.cancel_tx = None;
        self.task = None;
        self.running.store(false, Ordering::Relaxed);
    }
}

#[derive(Deserialize)]
struct TwicasCommentEvent {
    #[serde(rename = "type")]
    event_type: String,
    id: Option<Value>,
    message: Option<String>,
    #[serde(rename = "createdAt")]
    created_at: Option<i64>,
    author: Option<TwicasAuthor>,
}

#[derive(Deserialize)]
struct TwicasAuthor {
    id: Option<String>,
    name: Option<String>,
}

async fn handle_text_message(
    ctx: PluginContext,
    logical_plugin_id: Uuid,
    connection_id: Uuid,
    raw_text: &str,
) {
    let events: Vec<TwicasCommentEvent> = match serde_json::from_str(raw_text) {
        Ok(v) => v,
        Err(_) => return,
    };

    let mut provider_messages = Vec::new();
    for event in events {
        if event.event_type != "comment" {
            continue;
        }
        let Some(text) = event.message else {
            continue;
        };

        let user_name = event
            .author
            .as_ref()
            .and_then(|a| a.name.clone())
            .unwrap_or_else(|| "unknown".to_string());
        let user_id = event
            .author
            .as_ref()
            .and_then(|a| a.id.clone())
            .unwrap_or_else(|| "unknown".to_string());
        let id = stringify_comment_id(event.id).unwrap_or_else(|| Uuid::new_v4().to_string());
        let timestamp = event
            .created_at
            .map(|ms| ms / 1000)
            .unwrap_or_else(|| chrono::Utc::now().timestamp());

        let provider_msg = ProviderMessage {
            id: id.clone(),
            platform_message_id: Some(id),
            service: ServiceId("twicas".to_string()),
            channel: ChannelId("".to_string()),
            sender: ProviderSender {
                id: user_id,
                display_name: vec![MessagePart::Text { text: user_name }],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp,
            kind: ProviderMessageKind::Chat,
            content: ProviderContent::Text {
                text: vec![MessagePart::Text { text }],
            },
            reply_to: None,
            metadata: serde_json::Value::Null,
        };
        provider_messages.push(provider_msg);
    }

    if !provider_messages.is_empty() {
        let envelope = McvEnvelope {
            event_id: Uuid::new_v4(),
            connection_id,
            messages: provider_messages,
            received_at: chrono::Utc::now().timestamp(),
            raw_message: Some(raw_text.to_owned()),
        };
        let message = McvMessage::new_notification(
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
        TwicasPlugin::send_message(ctx, message).await;
    }
}

fn stringify_comment_id(id: Option<Value>) -> Option<String> {
    match id {
        Some(Value::String(v)) => Some(v),
        Some(Value::Number(v)) => Some(v.to_string()),
        _ => None,
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
        assert!(!conn.running.load(Ordering::Relaxed));
        assert!(conn.cancel_tx.is_none());
        assert!(conn.task.is_none());
    }

    #[test]
    fn test_connection_stop_when_not_running() {
        let id = Uuid::new_v4();
        let mut conn = Connection::new(&id);
        conn.stop();

        assert!(!conn.running.load(Ordering::Relaxed));
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

        assert!(!conn.running.load(Ordering::Relaxed));
    }

    #[test]
    fn test_extract_user_name() {
        assert_eq!(
            Connection::extract_user_name("https://twitcasting.tv/lanlanutau"),
            Some("lanlanutau".to_string())
        );
        assert_eq!(
            Connection::extract_user_name("https://www.twitcasting.tv/abc123/"),
            Some("abc123".to_string())
        );
        assert_eq!(
            Connection::extract_user_name("https://twitcasting.tv/c:amam_frfr_"),
            Some("c:amam_frfr_".to_string())
        );
        assert_eq!(
            Connection::extract_user_name("https://example.com/user"),
            None
        );
    }
}
