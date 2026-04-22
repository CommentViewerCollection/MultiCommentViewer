//! Twitch ストリームメタデータポーリング
//!
//! タイトル・配信開始時刻を定期的に取得して Core へ送信する。
//! 視聴者数は Hermes WebSocket の `video-playback-by-id` トピックから取得するため、ここでは送信しない。

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginId,
    StreamMetadataPayload,
};
use plugin_abi_helper::v3::prelude::*;
use tokio::sync::watch;
use tokio::time::{Duration, Instant};
use uuid::Uuid;

use crate::TwitchPlugin;

const POLL_INTERVAL: Duration = Duration::from_secs(60);
const RETRY_INTERVAL: Duration = Duration::from_secs(30);

/// Twitch ストリームメタデータ（タイトル・配信開始時刻）を定期的に取得して Core に送信する。
pub(crate) async fn metadata_polling_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    channel_login: String,
    mut cancel_rx: watch::Receiver<bool>,
    user_agent: String,
) {
    tracing::info!(
        target: "mcv::plugin-twitch",
        connection_id = %connection_id,
        channel = %channel_login,
        "メタデータポーリング開始"
    );

    let mut poll_count: u64 = 0;

    loop {
        // ループ開始時刻を記録。API呼び出し時間を含めても規定間隔以上を保証するため
        let loop_start = Instant::now();

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

        let desired_interval =
            match twitch_lib::fetch_stream_info(&channel_login, &user_agent).await {
                Ok(info) => {
                    let is_live = info.start_time.is_some();
                    tracing::info!(
                        target: "mcv::plugin-twitch",
                        connection_id = %connection_id,
                        poll_count,
                        title = ?info.title,
                        start_time = ?info.start_time,
                        is_live,
                        "メタデータポーリング: 取得成功、StreamMetadata 送信"
                    );
                    let payload = if is_live {
                        StreamMetadataPayload {
                            connection_id,
                            title: info.title,
                            viewer_count: None,
                            total_viewer_count: None,
                            start_time: info.start_time,
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

        // API呼び出しにかかった時間を差し引いた残り時間だけ待機する。
        // changed() が想定外に即座に返っても 0 秒ループにならないよう規定間隔を保証する。
        let wait = desired_interval.saturating_sub(loop_start.elapsed());
        tokio::select! {
            _ = tokio::time::sleep(wait) => {}
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
