//! Twitch 接続ライフサイクル管理
//!
//! `Connection` 構造体が接続の開始・停止を管理する。
//! IRC セッションの実体は [`crate::irc_session`]、
//! メタデータポーリングは [`crate::metadata_polling`] モジュールに委譲する。

use futures_util::FutureExt;
use mcv_messages::{
    DisconnectedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginId, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::task::JoinHandle;
use twitch_lib::auth_token::AuthToken;
use uuid::Uuid;

use crate::{hermes, irc_session, metadata_polling, TwitchPlugin};

/// Twitch チャンネルへの接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    cancel_tx: Option<watch::Sender<bool>>,
    hermes_cancel_tx: Option<watch::Sender<bool>>,
    metadata_cancel_tx: Option<watch::Sender<bool>>,
    task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            hermes_cancel_tx: None,
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
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-twitch",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }
        let channel_login = match Self::extract_channel_id(url) {
            Some(ch) => ch,
            None => {
                tracing::debug!(
                    target: "mcv::plugin-twitch",
                    url = url,
                    "channel_idの抽出に失敗"
                );
                return;
            }
        };

        let rt = match tokio::runtime::Handle::try_current() {
            Ok(h) => h,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-twitch",
                    connection_id = %self.id,
                    error = %e,
                    "Tokio runtime is not available; cannot spawn tasks"
                );
                return;
            }
        };

        let connection_id = self.id;
        let ws_url = "wss://irc-ws.chat.twitch.tv/".to_string();
        let username = username.to_string();
        let password = auth_token.as_ref().map(|a| format!("oauth:{}", a.value()));
        let join_command = format!("JOIN #{channel_login}");

        let (cancel_tx, cancel_rx) = watch::channel(false);
        let (hermes_cancel_tx, hermes_cancel_rx) = watch::channel(false);
        let (metadata_cancel_tx, metadata_cancel_rx) = watch::channel(false);

        // IRC セッションタスク（パニックハンドリング付き）
        let task = rt.spawn({
            let ctx = ctx.clone();
            let logical_plugin_id = logical_plugin_id.clone();
            let channel_login = channel_login.clone();
            let auth_token = auth_token.clone();
            async move {
                let task_result = std::panic::AssertUnwindSafe(async {
                    let run_result = irc_session::run_irc_session(
                        ctx.clone(),
                        logical_plugin_id.clone(),
                        connection_id,
                        ws_url,
                        username,
                        password,
                        join_command,
                        channel_login,
                        auth_token,
                        cancel_rx,
                    )
                    .await;

                    if run_result.is_err() {
                        tracing::debug!(
                            target: "mcv::plugin-twitch",
                            connection_id = %connection_id,
                            "Twitch IRC task terminated due to earlier error"
                        );
                    }

                    // 切断時にアカウント情報をクリアして Disconnected を通知
                    TwitchPlugin::send_message(
                        ctx.clone(),
                        McvMessage::new_notification(
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
                        ),
                    )
                    .await;

                    TwitchPlugin::send_message(
                        ctx,
                        McvMessage::new_notification(
                            MessageType::Disconnected,
                            MessageSource::Plugin {
                                plugin_id: logical_plugin_id,
                            },
                            MessageDestination::Core,
                            serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                        ),
                    )
                    .await;
                })
                .catch_unwind()
                .await;

                if let Err(payload) = task_result {
                    let msg = if let Some(s) = payload.downcast_ref::<&str>() {
                        s.to_string()
                    } else if let Some(s) = payload.downcast_ref::<String>() {
                        s.clone()
                    } else {
                        "unknown panic payload".to_string()
                    };
                    tracing::error!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        panic = %msg,
                        "Twitch IRC task panicked"
                    );
                }
            }
        });

        // Hermes WebSocket タスク
        rt.spawn(hermes::hermes_loop(
            ctx.clone(),
            logical_plugin_id.clone(),
            connection_id,
            channel_login.clone(),
            auth_token.clone(),
            hermes_cancel_rx,
        ));

        // メタデータポーリングタスク（タイトル・開始時刻のみ。視聴者数は Hermes から取得）
        rt.spawn(metadata_polling::metadata_polling_loop(
            ctx,
            logical_plugin_id,
            connection_id,
            channel_login,
            metadata_cancel_rx,
        ));

        self.cancel_tx = Some(cancel_tx);
        self.hermes_cancel_tx = Some(hermes_cancel_tx);
        self.metadata_cancel_tx = Some(metadata_cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        if let Some(tx) = &self.hermes_cancel_tx {
            let _ = tx.send(true);
        }
        if let Some(tx) = &self.metadata_cancel_tx {
            let _ = tx.send(true);
        }
        // abort() を使わず cancel_tx でタスクを正常終了させる
        // これにより、loop を抜けた後の Disconnected メッセージ送信が実行される
        self.cancel_tx = None;
        self.hermes_cancel_tx = None;
        self.metadata_cancel_tx = None;
        self.task = None;
        self.running = false;
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
        assert!(conn.hermes_cancel_tx.is_none());
        assert!(conn.metadata_cancel_tx.is_none());
        assert!(conn.task.is_none());
    }

    #[test]
    fn test_connection_stop_when_not_running() {
        let id = Uuid::new_v4();
        let mut conn = Connection::new(&id);
        conn.stop(); // パニックしないことを確認
        assert!(!conn.running);
    }

    #[test]
    fn test_extract_channel_id_valid() {
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/example"),
            Some("example".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://twitch.tv/example"),
            Some("example".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("http://twitch.tv/example"),
            Some("example".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/example/extra"),
            Some("example".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/example?query=1"),
            Some("example".to_string())
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/example#fragment"),
            Some("example".to_string())
        );
    }

    #[test]
    fn test_extract_channel_id_invalid() {
        assert_eq!(
            Connection::extract_channel_id("https://youtube.com/watch?v=abc"),
            None
        );
        assert_eq!(
            Connection::extract_channel_id("https://www.twitch.tv/"),
            None
        );
        assert_eq!(Connection::extract_channel_id(""), None);
    }
}
