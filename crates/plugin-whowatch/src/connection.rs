//! ふわっち接続ライフサイクル管理
//!
//! `Connection` 構造体が接続の開始・停止を管理する。
//! WebSocket セッションの実体は [`crate::ws_session`]、
//! メタデータポーリングは [`crate::metadata_polling`] モジュールに委譲する。

use futures_util::FutureExt;
use mcv_messages::{
    DisconnectedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginId, UpdateConnectionAccountPayload,
};
use plugin_abi_helper::v3::prelude::*;
use std::sync::{
    atomic::{AtomicBool, Ordering},
    Arc,
};
use tokio::sync::watch;
use tokio::task::JoinHandle;
use uuid::Uuid;

use crate::{metadata_polling, ws_session, WhoWatchPlugin};

/// キャンセルを待つか、指定ミリ秒タイムアウトする。
/// `true` = キャンセル済み、`false` = タイムアウト
async fn wait_or_cancel(cancel_rx: &mut watch::Receiver<bool>, ms: u64) -> bool {
    tokio::select! {
        _ = tokio::time::sleep(std::time::Duration::from_millis(ms)) => false,
        result = cancel_rx.changed() => {
            result.is_err() || *cancel_rx.borrow()
        }
    }
}

/// ふわっちチャンネルへの接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    cancel_tx: Option<watch::Sender<bool>>,
    metadata_cancel_tx: Option<watch::Sender<bool>>,
    task: Option<JoinHandle<()>>,
    pub(crate) running: Arc<AtomicBool>,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            metadata_cancel_tx: None,
            task: None,
            running: Arc::new(AtomicBool::new(false)),
        }
    }

    /// 接続を開始する。
    ///
    /// # 引数
    /// * `url`             - ふわっちのURL（例: `https://whowatch.tv/viewer/71923893`）
    /// * `whowatch_token`  - WHOWATCH Cookie の値（JWT 文字列）
    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        url: &str,
        whowatch_token: &str,
    ) {
        if self.running.load(Ordering::Relaxed) {
            tracing::debug!(
                target: "mcv::plugin-whowatch",
                connection_id = %self.id,
                "connect() ignored because already running"
            );
            return;
        }

        let live_id = match Self::extract_live_id(url) {
            Some(id) => id,
            None => {
                tracing::warn!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %self.id,
                    url = %url,
                    "URL から live_id を抽出できなかった（対応フォーマット: https://whowatch.tv/viewer/{{live_id}}）"
                );
                return;
            }
        };

        let rt = match tokio::runtime::Handle::try_current() {
            Ok(h) => h,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %self.id,
                    error = %e,
                    "Tokio runtime が利用できない"
                );
                return;
            }
        };

        let connection_id = self.id;
        let whowatch_token = whowatch_token.to_string();
        let (cancel_tx, cancel_rx) = watch::channel(false);
        let (metadata_cancel_tx, metadata_cancel_rx) = watch::channel(false);
        let running_flag = Arc::clone(&self.running);

        // WebSocket セッションタスク（パニックハンドリング付き）
        let task = rt.spawn({
            let ctx = ctx.clone();
            let logical_plugin_id = logical_plugin_id.clone();
            let whowatch_token = whowatch_token.clone();

            async move {
                let ctx_panic = ctx.clone();
                let plugin_id_panic = logical_plugin_id.clone();
                let running_flag_panic = Arc::clone(&running_flag);

                let task_result = std::panic::AssertUnwindSafe(async {
                    let mut cancel_rx = cancel_rx;

                    'retry: loop {
                        if *cancel_rx.borrow() {
                            break 'retry;
                        }

                        let result = ws_session::run_ws_session(
                            ctx.clone(),
                            logical_plugin_id.clone(),
                            connection_id,
                            live_id,
                            whowatch_token.clone(),
                            &mut cancel_rx,
                        )
                        .await;

                        match result {
                            ws_session::SessionResult::Cancelled => {
                                tracing::info!(
                                    target: "mcv::plugin-whowatch",
                                    connection_id = %connection_id,
                                    "WhoWatch WS セッション: ユーザーキャンセル"
                                );
                                break 'retry;
                            }
                            ws_session::SessionResult::Error(reason) => {
                                tracing::warn!(
                                    target: "mcv::plugin-whowatch",
                                    connection_id = %connection_id,
                                    reason = %reason,
                                    "WhoWatch WS セッションがエラーで終了。5秒後に再接続します"
                                );
                                if wait_or_cancel(&mut cancel_rx, 5_000).await {
                                    break 'retry;
                                }
                            }
                            ws_session::SessionResult::NotLive => {
                                tracing::info!(
                                    target: "mcv::plugin-whowatch",
                                    connection_id = %connection_id,
                                    "配信中ではないため 30 秒後に再確認します"
                                );
                                if wait_or_cancel(&mut cancel_rx, 30_000).await {
                                    break 'retry;
                                }
                            }
                        }
                    }

                    running_flag.store(false, Ordering::Relaxed);

                    // アカウント情報をクリアして Disconnected を通知
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
                                account: None,
                            })
                            .unwrap(),
                        ),
                    )
                    .await;

                    WhoWatchPlugin::send_message(
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
                        target: "mcv::plugin-whowatch",
                        connection_id = %connection_id,
                        panic = %msg,
                        "WhoWatch WS タスクがパニック"
                    );
                    running_flag_panic.store(false, Ordering::Relaxed);
                    WhoWatchPlugin::send_message(
                        ctx_panic.clone(),
                        McvMessage::new_notification(
                            MessageType::UpdateConnectionAccount,
                            MessageSource::Plugin {
                                plugin_id: plugin_id_panic.clone(),
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
                    WhoWatchPlugin::send_message(
                        ctx_panic,
                        McvMessage::new_notification(
                            MessageType::Disconnected,
                            MessageSource::Plugin {
                                plugin_id: plugin_id_panic,
                            },
                            MessageDestination::Core,
                            serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                        ),
                    )
                    .await;
                }
            }
        });

        // メタデータポーリングタスク（タイトル・視聴者数）
        rt.spawn(metadata_polling::metadata_polling_loop(
            ctx,
            logical_plugin_id,
            connection_id,
            live_id,
            whowatch_token,
            metadata_cancel_rx,
        ));

        self.cancel_tx = Some(cancel_tx);
        self.metadata_cancel_tx = Some(metadata_cancel_tx);
        self.task = Some(task);
        self.running.store(true, Ordering::Relaxed);
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        if let Some(tx) = &self.metadata_cancel_tx {
            let _ = tx.send(true);
        }
        self.cancel_tx = None;
        self.metadata_cancel_tx = None;
        self.task = None;
        self.running.store(false, Ordering::Relaxed);
    }

    /// ふわっち URL から live_id を抽出する。
    ///
    /// 対応フォーマット:
    /// - `https://whowatch.tv/viewer/71923893` → `71923893`
    pub(crate) fn extract_live_id(url: &str) -> Option<u64> {
        let trimmed = url.trim();
        for prefix in &[
            "https://whowatch.tv/viewer/",
            "http://whowatch.tv/viewer/",
            "https://www.whowatch.tv/viewer/",
            "http://www.whowatch.tv/viewer/",
        ] {
            if let Some(rest) = trimmed.strip_prefix(prefix) {
                let id_str = rest.split(['?', '#']).next().unwrap_or("").trim();
                if let Ok(id) = id_str.parse::<u64>() {
                    return Some(id);
                }
            }
        }
        None
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_extract_live_id_valid() {
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/viewer/71923893"),
            Some(71923893)
        );
        assert_eq!(
            Connection::extract_live_id("https://www.whowatch.tv/viewer/12345"),
            Some(12345)
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/viewer/71923893?foo=1"),
            Some(71923893)
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/viewer/71923893#section"),
            Some(71923893)
        );
    }

    #[test]
    fn test_extract_live_id_invalid() {
        assert_eq!(
            Connection::extract_live_id("https://twitch.tv/someone"),
            None
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/viewer/"),
            None
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/viewer/abc"),
            None
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/w:Rabbit320"),
            None
        );
        assert_eq!(
            Connection::extract_live_id("https://whowatch.tv/user/49276237"),
            None
        );
        assert_eq!(Connection::extract_live_id(""), None);
    }

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
        conn.stop(); // パニックしないことを確認
        assert!(!conn.running.load(Ordering::Relaxed));
    }
}
