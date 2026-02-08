//! YouTube Live接続管理
//!
//! 個々のYouTube Live配信への接続を管理し、
//! ライブチャットメッセージを定期的に取得します。

use mcv_messages::{DisconnectedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::task::JoinHandle;
use tokio::time::{sleep, Duration};
use uuid::Uuid;
use youtube_live_lib::{
    extract_ytcfg, get_live_chat, get_live_chat_messages, get_yt_initial_data, Action,
    Continuation, Vid,
};

use crate::video_id::extract_video_id;
use crate::YouTubeLivePlugin;

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

    pub(crate) fn connect(&mut self, ctx: PluginContext, logical_plugin_id: Uuid, url: &str) {
        tracing::trace!(
            target: "mcv::plugin-youtube-live",
            url = url,
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

        let task = tokio::spawn(async move {
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

            for action in yt_initial_data.actions() {
                if let Action::ParseError(raw) = action {
                    tracing::error!(
                        target: "mcv::plugin-youtube-live",
                        connection_id = %connection_id,
                        raw = raw,
                        "Failed to parse action"
                    );
                }
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
                    Ok((maybe_cont, actions)) => {
                        for action in actions {
                            if let Action::ParseError(raw) = action {
                                tracing::error!(
                                    target: "mcv::plugin-youtube-live",
                                    connection_id = %connection_id,
                                    raw = raw,
                                    "Failed to parse action"
                                );
                            }
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
            let message = McvMessage::new(
                MessageType::Disconnected,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(DisconnectedPayload {
                    connection_id,
                })
                .unwrap(),
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
