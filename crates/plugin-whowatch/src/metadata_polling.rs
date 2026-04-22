//! ふわっちストリームメタデータポーリング
//!
//! `GET /lives/{live_id}?last_updated_at=0` を定期的に呼び出し、
//! タイトル・視聴者数などを Core へ送信する。

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginId,
    StreamMetadataPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::time::{Duration, Instant};
use uuid::Uuid;
use whowatch_lib::WhoWatchClient;

use crate::WhoWatchPlugin;

/// 通常のポーリング間隔
const POLL_INTERVAL: Duration = Duration::from_secs(60);
/// エラー時のリトライ間隔
const RETRY_INTERVAL: Duration = Duration::from_secs(30);

/// ふわっちストリームメタデータを定期的に取得して Core に送信する。
pub(crate) async fn metadata_polling_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    live_id: u64,
    whowatch_token: String,
    mut cancel_rx: watch::Receiver<bool>,
) {
    tracing::info!(
        target: "mcv::plugin-whowatch",
        connection_id = %connection_id,
        live_id = %live_id,
        "メタデータポーリング開始"
    );

    let client = match WhoWatchClient::new(&whowatch_token, None) {
        Ok(c) => c,
        Err(e) => {
            tracing::error!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                error = %e,
                "メタデータポーリング: WhoWatchClient 生成失敗、終了"
            );
            return;
        }
    };

    let mut poll_count: u64 = 0;
    // 前回レスポンスの updated_at。次回呼び出しの last_updated_at に使う。
    let mut last_updated_at: u64 = 0;

    loop {
        let loop_start = Instant::now();

        if *cancel_rx.borrow() {
            tracing::info!(
                target: "mcv::plugin-whowatch",
                connection_id = %connection_id,
                poll_count,
                "メタデータポーリング: キャンセル受信、終了"
            );
            break;
        }

        poll_count += 1;
        tracing::debug!(
            target: "mcv::plugin-whowatch",
            connection_id = %connection_id,
            poll_count,
            live_id = %live_id,
            last_updated_at,
            "メタデータポーリング: GET /lives/{live_id}"
        );

        let desired_interval = match client.get_live(live_id, last_updated_at).await {
            Ok(resp) => {
                // 次回ポーリング用に updated_at を更新（トップレベルフィールド）
                if let Some(ua) = resp.updated_at {
                    last_updated_at = ua;
                }

                let live = &resp.live;
                let is_live = live.is_publishing();
                let title = live.title.clone();
                let viewer_count = live.view_count;
                let total_viewer_count = live.total_view_count;
                // started_at が取得できない場合は running_time から逆算する
                let start_time = live.started_at.map(|ms| (ms / 1000) as i64).or_else(|| {
                    live.running_time.map(|secs| {
                        let now = std::time::SystemTime::now()
                            .duration_since(std::time::UNIX_EPOCH)
                            .unwrap_or_default()
                            .as_secs();
                        (now.saturating_sub(secs)) as i64
                    })
                });

                tracing::info!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    poll_count,
                    is_live,
                    title = ?title,
                    viewer_count = ?viewer_count,
                    "メタデータポーリング: 取得成功"
                );

                if !resp.deleted_comment_ids.is_empty() {
                    tracing::info!(
                        target: "mcv::plugin-whowatch",
                        connection_id = %connection_id,
                        live_id = %live_id,
                        deleted_comment_ids = ?resp.deleted_comment_ids,
                        "メタデータポーリング: deleted_comment_ids を検出"
                    );
                }

                let payload = if is_live {
                    StreamMetadataPayload {
                        connection_id,
                        title,
                        viewer_count,
                        total_viewer_count,
                        start_time,
                        others: None,
                        clear: None,
                    }
                } else {
                    StreamMetadataPayload {
                        connection_id,
                        title: Some("（次の配信が始まるまで待機中...）".to_string()),
                        viewer_count: None,
                        total_viewer_count: None,
                        start_time: None,
                        others: None,
                        clear: Some(true),
                    }
                };

                let msg = McvMessage::new_notification(
                    MessageType::StreamMetadata,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(payload).unwrap(),
                );
                WhoWatchPlugin::send_message(ctx.clone(), msg).await;
                POLL_INTERVAL
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-whowatch",
                    connection_id = %connection_id,
                    poll_count,
                    error = %e,
                    "メタデータポーリング: 取得失敗、{}秒後にリトライ",
                    RETRY_INTERVAL.as_secs()
                );
                RETRY_INTERVAL
            }
        };

        // API 呼び出し時間を差し引いた残り時間だけ待機
        let wait = desired_interval.saturating_sub(loop_start.elapsed());
        tokio::select! {
            _ = tokio::time::sleep(wait) => {}
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-whowatch",
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
