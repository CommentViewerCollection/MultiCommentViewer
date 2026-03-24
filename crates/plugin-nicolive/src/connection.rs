//! ニコニコ生放送 接続管理

use futures_util::{stream::SplitSink, FutureExt, SinkExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, MonetaryInfo, Money, PluginId,
    ProviderContent, ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
    StreamMetadataPayload, SystemKind, UpdateConnectionAccountPayload,
};
use nicolive_lib::{
    domain_state_machine::{DomainCommand, DomainEvent, NicoLiveStateMachine},
    extract_live_id, fetch_websocket_url, try_pop_segment_event, try_pop_view_entry, SegmentEvent,
    ServerTimeCache, ViewEntry,
};
use plugin_abi_helper::v3::prelude::*;
use std::sync::Arc;
use tokio::{
    net::TcpStream,
    sync::watch,
    task::JoinHandle,
    time::{interval_at, sleep, timeout, Duration, Instant, Interval},
};
use tokio_tungstenite::{
    connect_async, tungstenite::Message as WsMessage, MaybeTlsStream, WebSocketStream,
};
use uuid::Uuid;

use crate::NicoLivePlugin;

type WsSink = SplitSink<WebSocketStream<MaybeTlsStream<TcpStream>>, WsMessage>;

// ─── RAII ガード ────────────────────────────────────────────────────────────────

/// Drop 時に JoinHandle を abort() するガード。
struct AbortGuard(JoinHandle<()>);

impl Drop for AbortGuard {
    fn drop(&mut self) {
        self.0.abort();
    }
}

/// poll_view_uri タスクの終了（abort 含む）をログに記録するガード。
struct PollEndGuard {
    connection_id: Uuid,
}

impl Drop for PollEndGuard {
    fn drop(&mut self) {
        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %self.connection_id,
            "viewUri polling 終了"
        );
    }
}

// ─── Connection ──────────────────────────────────────────────────────────────────

/// ニコニコ生放送配信への接続を表す構造体
pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: bool,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            task: None,
            running: false,
        }
    }

    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        ws_url: &str,
        title: Option<String>,
        start_time: Option<i64>,
    ) {
        if self.running {
            tracing::debug!(
                target: "mcv::plugin-nicolive",
                connection_id = %self.id,
                "connect() ignored: already running"
            );
            return;
        }
        let (cancel_tx, cancel_rx) = watch::channel(false);
        let connection_id = self.id;

        let task = match Self::start_connection_task(
            ctx,
            logical_plugin_id,
            connection_id,
            ws_url.to_string(),
            title,
            start_time,
            cancel_rx,
        ) {
            Some(t) => t,
            None => return,
        };

        self.cancel_tx = Some(cancel_tx);
        self.task = Some(task);
        self.running = true;
    }

    fn start_connection_task(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        ws_url: String,
        title: Option<String>,
        start_time: Option<i64>,
        mut cancel_rx: watch::Receiver<bool>,
    ) -> Option<JoinHandle<()>> {
        let runtime_handle = match tokio::runtime::Handle::try_current() {
            Ok(h) => h,
            Err(e) => {
                tracing::error!(
                    target: "mcv::plugin-nicolive",
                    error = %e,
                    "Tokio runtime が利用できない"
                );
                return None;
            }
        };

        let task = runtime_handle.spawn(async move {
            let task_result = std::panic::AssertUnwindSafe(async {
                let ctx_loop = ctx.clone();
                Self::run_websocket_loop(
                    ctx_loop,
                    logical_plugin_id.clone(),
                    connection_id,
                    &ws_url,
                    title,
                    start_time,
                    &mut cancel_rx,
                )
                .await;

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
                NicoLivePlugin::send_message(ctx.clone(), clear_account).await;

                let msg = McvMessage::new_notification(
                    MessageType::Disconnected,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id,
                    },
                    MessageDestination::Core,
                    serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
                );
                NicoLivePlugin::send_message(ctx, msg).await;
            })
            .catch_unwind()
            .await;

            if let Err(payload) = task_result {
                let msg = if let Some(s) = payload.downcast_ref::<&str>() {
                    s.to_string()
                } else if let Some(s) = payload.downcast_ref::<String>() {
                    s.clone()
                } else {
                    "unknown panic".to_string()
                };
                tracing::error!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    panic = %msg,
                    "接続タスクがパニック"
                );
            }
        });

        Some(task)
    }

    // ── 外側ループ: reconnect メッセージで新 URL に切り替える ──────────────────
    async fn run_websocket_loop(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        initial_ws_url: &str,
        title: Option<String>,
        start_time: Option<i64>,
        cancel_rx: &mut watch::Receiver<bool>,
    ) {
        let mut current_url = initial_ws_url.to_string();

        'outer: loop {
            let reconnect = Self::run_single_connection(
                ctx.clone(),
                logical_plugin_id.clone(),
                connection_id,
                &current_url,
                title.clone(),
                start_time,
                cancel_rx,
            )
            .await;

            match reconnect {
                None => break 'outer,
                Some((audience_token, wait_secs)) => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        wait_secs = wait_secs,
                        "reconnect 受信: {}秒後に再接続",
                        wait_secs
                    );

                    tokio::select! {
                        _ = cancel_rx.changed() => {
                            if *cancel_rx.borrow() { break 'outer; }
                        }
                        _ = sleep(Duration::from_secs(wait_secs)) => {}
                    }

                    current_url =
                        format!("wss://a.live2.nicovideo.jp/unama/wsapi/v2/watch/{audience_token}");
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        new_url = %current_url,
                        "再接続 URL に切り替え"
                    );
                }
            }
        }
    }

    // ── 内側ループ: 単一 WebSocket セッション ────────────────────────────────
    async fn run_single_connection(
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        connection_id: Uuid,
        ws_url: &str,
        title: Option<String>,
        start_time: Option<i64>,
        cancel_rx: &mut watch::Receiver<bool>,
    ) -> Option<(String, u64)> {
        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            ws_url = %ws_url,
            "WebSocket 接続中"
        );

        let connect_result = timeout(Duration::from_secs(15), connect_async(ws_url)).await;
        let (ws_stream, _) = match connect_result {
            Err(_) => {
                tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket 接続タイムアウト");
                return None;
            }
            Ok(Err(e)) => {
                tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "WebSocket 接続失敗");
                return None;
            }
            Ok(Ok(v)) => v,
        };

        tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket 接続成功");

        let (mut write, mut read) = ws_stream.split();

        // startWatching 送信
        let start_watching = serde_json::json!({
            "type": "startWatching",
            "data": {
                "stream": { "quality": "abr", "protocol": "hls", "latency": "low", "accessRightMethod": "single_cookie", "chasePlay": false },
                "room": { "protocol": "webSocket", "commentable": true },
                "reconnect": false
            }
        });
        if let Err(e) = write
            .send(WsMessage::Text(start_watching.to_string().into()))
            .await
        {
            tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "startWatching 送信失敗");
            return None;
        }
        tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "startWatching 送信完了");

        // 初回 StreamMetadata 送信（視聴者数なし、タイトル・開始時刻のみ）
        if title.is_some() || start_time.is_some() {
            let payload = StreamMetadataPayload {
                connection_id,
                title: title.clone(),
                viewer_count: None,
                total_viewer_count: None,
                start_time,
                others: None,
                clear: None,
            };
            let msg = McvMessage::new_notification(
                MessageType::StreamMetadata,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id.clone(),
                },
                MessageDestination::Core,
                serde_json::to_value(payload).unwrap(),
            );
            NicoLivePlugin::send_message(ctx.clone(), msg).await;
        }

        let mut machine = NicoLiveStateMachine::new();
        let mut keep_seat_interval: Option<Interval> = None;
        let mut reconnect_result: Option<(String, u64)> = None;
        let mut view_poll_guard: Option<AbortGuard> = None;
        let time_cache = Arc::new(ServerTimeCache::new());

        'session: loop {
            tokio::select! {
                _ = cancel_rx.changed() => {
                    if *cancel_rx.borrow() {
                        tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "切断要求を受信");
                        break 'session;
                    }
                }
                _ = async {
                    match keep_seat_interval {
                        Some(ref mut interval) => { interval.tick().await; }
                        None => std::future::pending::<()>().await,
                    }
                } => {
                    // KeepSeatTick は Active 状態でのみ有効で、常に [SendKeepSeat] を返す。
                    // keep_seat_interval の借用を手放してから execute_commands を呼ぶことを
                    // 避けるため、この分岐ではコマンド実行をインラインで行う。
                    match machine.on_event(DomainEvent::KeepSeatTick) {
                        Ok(cmds) => {
                            for cmd in cmds {
                                if let DomainCommand::SendKeepSeat = cmd {
                                    let payload = serde_json::json!({"type": "keepSeat"});
                                    if let Err(e) = write.send(WsMessage::Text(payload.to_string().into())).await {
                                        tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "keepSeat 送信失敗");
                                        break 'session;
                                    }
                                    tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "keepSeat 送信");
                                }
                            }
                        }
                        Err(e) => {
                            tracing::warn!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "KeepSeatTick: 無効な状態遷移");
                        }
                    }
                }
                recv = read.next() => {
                    match recv {
                        Some(Ok(WsMessage::Text(text))) => {
                            tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, raw = %text, "WS テキスト受信");
                            let cmds = match machine.on_event(DomainEvent::WsText(text.to_string())) {
                                Ok(c) => c,
                                Err(e) => {
                                    tracing::warn!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "WsText: 無効な状態遷移");
                                    continue;
                                }
                            };
                            if Self::execute_commands(
                                cmds,
                                &mut write,
                                &mut keep_seat_interval,
                                &mut view_poll_guard,
                                &mut reconnect_result,
                                &time_cache,
                                connection_id,
                                &ctx,
                                &logical_plugin_id,
                                title.as_deref(),
                                start_time,
                            )
                            .await
                            {
                                break 'session;
                            }
                        }
                        Some(Ok(WsMessage::Close(_))) => {
                            tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket Close フレーム受信");
                            break 'session;
                        }
                        Some(Ok(WsMessage::Ping(data))) => {
                            if let Err(e) = write.send(WsMessage::Pong(data)).await {
                                tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "Pong 送信失敗");
                                break 'session;
                            }
                        }
                        Some(Ok(_)) => {}
                        Some(Err(e)) => {
                            tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "WebSocket 受信エラー");
                            break 'session;
                        }
                        None => {
                            tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket ストリーム終了");
                            break 'session;
                        }
                    }
                }
            }
        }

        drop(view_poll_guard);
        reconnect_result
    }

    // ── DomainCommand 実行ランナー ────────────────────────────────────────────

    /// 状態機械から返されたコマンドリストを実行する。
    ///
    /// セッションを終了すべき場合（EmitDisconnected / ReconnectTo / 送信エラー）は
    /// `true` を返す。呼び出し元はこの戻り値で 'session ループを抜けること。
    #[allow(clippy::too_many_arguments)]
    async fn execute_commands(
        cmds: Vec<DomainCommand>,
        write: &mut WsSink,
        keep_seat_interval: &mut Option<Interval>,
        view_poll_guard: &mut Option<AbortGuard>,
        reconnect_result: &mut Option<(String, u64)>,
        time_cache: &Arc<ServerTimeCache>,
        connection_id: Uuid,
        ctx: &PluginContext,
        logical_plugin_id: &PluginId,
        title: Option<&str>,
        start_time: Option<i64>,
    ) -> bool {
        for cmd in cmds {
            match cmd {
                DomainCommand::SendPong => {
                    let pong = serde_json::json!({"type": "pong"});
                    if let Err(e) = write.send(WsMessage::Text(pong.to_string().into())).await {
                        tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "pong 送信失敗");
                        return true;
                    }
                    tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "pong 送信");
                }
                DomainCommand::SendKeepSeat => {
                    // KeepSeatTick 分岐ではインライン処理するためここには到達しない想定。
                    // 念のため実装しておく。
                    let payload = serde_json::json!({"type": "keepSeat"});
                    if let Err(e) = write
                        .send(WsMessage::Text(payload.to_string().into()))
                        .await
                    {
                        tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "keepSeat 送信失敗");
                        return true;
                    }
                    tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "keepSeat 送信");
                }
                DomainCommand::SetKeepSeatInterval { interval_secs } => {
                    let duration = Duration::from_secs(interval_secs);
                    *keep_seat_interval = Some(interval_at(Instant::now() + duration, duration));
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        keep_interval_secs = interval_secs,
                        "seat 受信: keepSeat インターバル設定"
                    );
                }
                DomainCommand::SpawnViewPolling { view_uri } => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        view_uri = %view_uri,
                        "viewUri polling タスクを spawn"
                    );
                    *view_poll_guard = Some(AbortGuard(tokio::spawn(Self::poll_view_uri(
                        view_uri,
                        connection_id,
                        ctx.clone(),
                        logical_plugin_id.clone(),
                    ))));
                }
                DomainCommand::UpdateServerTime { server_secs } => {
                    time_cache.update(server_secs);
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        server_secs,
                        "serverTime 受信: 時刻キャッシュ更新"
                    );
                }
                DomainCommand::EmitStatistics { viewers } => {
                    tracing::debug!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        viewers,
                        "statistics 受信: StreamMetadata 送信"
                    );
                    let payload = StreamMetadataPayload {
                        connection_id,
                        title: title.map(str::to_string),
                        viewer_count: Some(viewers),
                        total_viewer_count: None,
                        start_time,
                        others: None,
                        clear: None,
                    };
                    let msg = McvMessage::new_notification(
                        MessageType::StreamMetadata,
                        MessageSource::Plugin {
                            plugin_id: logical_plugin_id.clone(),
                        },
                        MessageDestination::Core,
                        serde_json::to_value(payload).unwrap(),
                    );
                    NicoLivePlugin::send_message(ctx.clone(), msg).await;
                }
                DomainCommand::ReconnectTo {
                    audience_token,
                    wait_secs,
                } => {
                    *reconnect_result = Some((audience_token, wait_secs));
                    return true;
                }
                DomainCommand::EmitDisconnected => {
                    return true;
                }
            }
        }
        false
    }

    // ── viewUri ポーリングタスク ──────────────────────────────────────────────

    async fn poll_view_uri(
        view_uri: String,
        connection_id: Uuid,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
    ) {
        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            view_uri = %view_uri,
            "viewUri polling 開始"
        );

        let _end_guard = PollEndGuard { connection_id };

        // Chrome 131 の TLS/HTTP2 フィンガープリントでリクエストし、
        // サーバー側のブラウザ検出・意図的な遅延を回避する。
        let client = rquest::Client::builder()
            .impersonate(rquest::Impersonate::Chrome131)
            .build()
            .unwrap_or_else(|_| rquest::Client::new());

        let mut at_param = "now".to_string();
        // ポーリングをまたいで同じセグメント URI を重複取得しないよう管理する
        let mut fetched_uris = std::collections::HashSet::<String>::new();

        loop {
            // viewUri はロングポーリング: サーバーが次のセグメント準備まで接続を保持するため
            // クライアント側では sleep せず即座に次のリクエストを送信する
            match Self::fetch_and_decode_view(
                &client,
                &view_uri,
                &at_param,
                connection_id,
                &ctx,
                logical_plugin_id.clone(),
                &mut fetched_uris,
            )
            .await
            {
                Ok(Some((_, next_at))) => {
                    at_param = next_at;
                }
                Ok(None) => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        "待ち時間情報なし: viewUri polling 終了"
                    );
                    break;
                }
                Err(e) => {
                    tracing::warn!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        error = %e,
                        "viewUri polling エラー: 終了"
                    );
                    break;
                }
            }
        }
    }

    /// viewUri を 1 回 GET し、ストリームを読みながら ChunkedEntry をリアルタイムにデコードする。
    ///
    /// 従来の `bytes().await` では全レスポンス（約30秒）が届くまでブロックしていた。
    /// `bytes_stream()` を使い、Segment エントリが届いた瞬間に `fetch_segment_messages` を
    /// tokio::spawn でスポーンすることで遅延を解消する。
    ///
    /// `Next` エントリがあれば `Some((待機時間, 次回 at))` を返す。
    async fn fetch_and_decode_view(
        client: &rquest::Client,
        view_uri: &str,
        at_param: &str,
        connection_id: Uuid,
        ctx: &PluginContext,
        logical_plugin_id: PluginId,
        fetched_uris: &mut std::collections::HashSet<String>,
    ) -> Result<Option<(Duration, String)>, String> {
        use prost::bytes::{Buf, BufMut, BytesMut};

        let url = if view_uri.contains('?') {
            format!("{view_uri}&at={at_param}")
        } else {
            format!("{view_uri}?at={at_param}")
        };

        let response = client
            .get(&url)
            .header("Accept", "*/*")
            .header("Origin", "https://live.nicovideo.jp")
            .header("Referer", "https://live.nicovideo.jp/")
            .send()
            .await
            .map_err(|e| format!("GET 失敗: {e}"))?;

        let status = response.status();
        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            status = %status,
            "viewUri レスポンス受信開始"
        );

        if !status.is_success() {
            return Err(format!("HTTP エラー: {status}"));
        }

        // ストリーミング読み取り: バイト列が届き次第デコードし Segment を即時スポーン
        let mut buf = BytesMut::new();
        let mut stream = response.bytes_stream();
        let mut next_info: Option<(Duration, String)> = None;
        let mut entry_count = 0u32;

        while let Some(chunk_result) = stream.next().await {
            let chunk = chunk_result.map_err(|e| format!("ストリーム読み取りエラー: {e}"))?;
            buf.put(chunk.as_ref());

            // バッファから完全なエントリが取り出せる限りループ
            loop {
                let (entry_opt, consumed) = try_pop_view_entry(&buf);
                if consumed == 0 {
                    break; // データ不足、次のチャンクを待つ
                }
                buf.advance(consumed);
                entry_count += 1;

                let entry = match entry_opt {
                    Some(e) => e,
                    None => continue, // 認識外エントリをスキップ
                };

                match entry {
                    ViewEntry::Previous { uri } => {
                        if !fetched_uris.contains(&uri) {
                            fetched_uris.insert(uri.clone());
                            tracing::info!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                uri = %uri,
                                "Previous 受信: 過去コメントを取得"
                            );
                            let client = client.clone();
                            let ctx = ctx.clone();
                            let logical_plugin_id = logical_plugin_id.clone();
                            tokio::spawn(async move {
                                if let Err(e) = Self::fetch_segment_messages(
                                    &client,
                                    &uri,
                                    connection_id,
                                    &ctx,
                                    logical_plugin_id,
                                )
                                .await
                                {
                                    tracing::warn!(
                                        target: "mcv::plugin-nicolive",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "Previous セグメント取得失敗"
                                    );
                                }
                            });
                        }
                    }
                    ViewEntry::Segment { uri } => {
                        if fetched_uris.contains(&uri) {
                            tracing::debug!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                uri = %uri,
                                "セグメント重複スキップ"
                            );
                        } else {
                            fetched_uris.insert(uri.clone());
                            tracing::info!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                uri = %uri,
                                "Segment 受信: セグメント取得を即時スポーン"
                            );
                            // 受け取った瞬間にスポーン（full レスポンスを待たない）
                            let client = client.clone();
                            let ctx = ctx.clone();
                            let logical_plugin_id = logical_plugin_id.clone();
                            tokio::spawn(async move {
                                if let Err(e) = Self::fetch_segment_messages(
                                    &client,
                                    &uri,
                                    connection_id,
                                    &ctx,
                                    logical_plugin_id,
                                )
                                .await
                                {
                                    tracing::warn!(
                                        target: "mcv::plugin-nicolive",
                                        connection_id = %connection_id,
                                        error = %e,
                                        "セグメントメッセージ取得失敗"
                                    );
                                }
                            });
                        }
                    }
                    ViewEntry::Next { at } => {
                        tracing::info!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            at = at,
                            "ReadyForNext 受信"
                        );
                        next_info = Some((Duration::ZERO, at.to_string()));
                    }
                    ViewEntry::Backward {
                        segment_uri,
                        snapshot_uri,
                    } => {
                        // snapshot_uri: 調査用に取得・ログ出力
                        if let Some(snap_uri) = snapshot_uri {
                            if !fetched_uris.contains(&snap_uri) {
                                fetched_uris.insert(snap_uri.clone());
                                tracing::info!(
                                    target: "mcv::plugin-nicolive",
                                    connection_id = %connection_id,
                                    uri = %snap_uri,
                                    "Backward snapshot 受信: 調査のため取得"
                                );
                                let client = client.clone();
                                tokio::spawn(async move {
                                    Self::investigate_backward_snapshot(
                                        &client,
                                        &snap_uri,
                                        connection_id,
                                    )
                                    .await;
                                });
                            }
                        }
                        if let Some(uri) = segment_uri {
                            if !fetched_uris.contains(&uri) {
                                fetched_uris.insert(uri.clone());
                                tracing::info!(
                                    target: "mcv::plugin-nicolive",
                                    connection_id = %connection_id,
                                    uri = %uri,
                                    "Backward segment 受信: 即時スポーン"
                                );
                                let client = client.clone();
                                let ctx = ctx.clone();
                                let logical_plugin_id = logical_plugin_id.clone();
                                tokio::spawn(async move {
                                    if let Err(e) = Self::fetch_segment_messages(
                                        &client,
                                        &uri,
                                        connection_id,
                                        &ctx,
                                        logical_plugin_id,
                                    )
                                    .await
                                    {
                                        tracing::warn!(
                                            target: "mcv::plugin-nicolive",
                                            connection_id = %connection_id,
                                            error = %e,
                                            "Backward セグメント取得失敗"
                                        );
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }

        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            entry_count = entry_count,
            has_next = next_info.is_some(),
            "viewUri ストリーミング処理完了"
        );

        Ok(next_info)
    }

    /// Backward snapshot URI の内容を調査してログに出力する（フロントへは送信しない）。
    async fn investigate_backward_snapshot(
        client: &rquest::Client,
        uri: &str,
        connection_id: Uuid,
    ) {
        use nicolive_lib::decode_segment_events;

        let response = match client
            .get(uri)
            .header("Accept", "*/*")
            .header("Origin", "https://live.nicovideo.jp")
            .header("Referer", "https://live.nicovideo.jp/")
            .send()
            .await
        {
            Ok(r) => r,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    uri = %uri,
                    error = %e,
                    "Backward snapshot GET 失敗"
                );
                return;
            }
        };

        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            status = %response.status(),
            content_type = ?response.headers().get("content-type"),
            "Backward snapshot レスポンス"
        );

        if !response.status().is_success() {
            return;
        }

        match timeout(Duration::from_secs(15), response.bytes()).await {
            Ok(Ok(bytes)) => {
                use nicolive_lib::decode_chunked_messages;

                // 先頭40バイトを hex でログ（フォーマット特定用）
                let hex_head: String = bytes
                    .iter()
                    .take(40)
                    .map(|b| format!("{:02X}", b))
                    .collect::<Vec<_>>()
                    .join(" ");
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    bytes_len = bytes.len(),
                    hex_head = %hex_head,
                    "Backward snapshot バイト列"
                );

                // 試み1: そのまま decode_segment_events（length-delimited ChunkedMessage 列）
                let events_full = decode_segment_events(&bytes);
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    count = events_full.len(),
                    "snapshot 試み1: decode_segment_events(full) → {} 件",
                    events_full.len()
                );

                // 試み2: 先頭の varint 長さプレフィックスを読み飛ばして再試行
                {
                    use prost::bytes::Buf;
                    let mut cursor = prost::bytes::Bytes::copy_from_slice(&bytes);
                    if let Ok(inner_len) = prost::decode_length_delimiter(&mut cursor) {
                        let remaining = cursor.remaining();
                        tracing::info!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            inner_len = inner_len,
                            remaining_after_prefix = remaining,
                            "snapshot varint prefix 読み飛ばし"
                        );
                        let inner: Vec<u8> = cursor.chunk()[..inner_len.min(remaining)].to_vec();
                        let events_inner = decode_segment_events(&inner);
                        tracing::info!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            count = events_inner.len(),
                            "snapshot 試み2: decode_segment_events(inner) → {} 件",
                            events_inner.len()
                        );
                        for (i, event) in events_inner.iter().take(3).enumerate() {
                            tracing::info!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                index = i,
                                event = ?event,
                                "snapshot サンプルイベント"
                            );
                        }
                        // ChunkedMessage (raw) も試す
                        let raw = decode_chunked_messages(&inner);
                        tracing::info!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            raw_count = raw.len(),
                            "snapshot 試み2: decode_chunked_messages(inner) → {} 件",
                            raw.len()
                        );
                    }
                }
            }
            Ok(Err(e)) => {
                tracing::warn!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    error = %e,
                    "Backward snapshot 読み取りエラー"
                );
            }
            Err(_) => {
                tracing::warn!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    "Backward snapshot タイムアウト（15秒）"
                );
            }
        }
    }

    /// セグメント URI からコメント一覧を取得し CommentReceived を送信する。
    ///
    /// ストリーミング読み取りにより、HTTP チャンクが届くたびにデコードして即時送信する。
    /// これにより、セグメント全体のダウンロード完了を待たずにコメントが表示される。
    async fn fetch_segment_messages(
        client: &rquest::Client,
        segment_uri: &str,
        connection_id: Uuid,
        ctx: &PluginContext,
        logical_plugin_id: PluginId,
    ) -> Result<(), String> {
        use prost::bytes::{Buf, BufMut, BytesMut};

        let response = client
            .get(segment_uri)
            .header("Accept", "*/*")
            .header("Origin", "https://live.nicovideo.jp")
            .header("Referer", "https://live.nicovideo.jp/")
            .send()
            .await
            .map_err(|e| format!("セグメント GET 失敗: {e}"))?;

        if !response.status().is_success() {
            return Err(format!("セグメント HTTP エラー: {}", response.status()));
        }

        // ストリーミング読み取り: チャンクが届くたびに ChunkedMessage をデコードして即時送信
        let mut buf = BytesMut::new();
        let mut stream = response.bytes_stream();

        while let Some(chunk_result) = stream.next().await {
            let chunk = chunk_result.map_err(|e| format!("セグメント読み取りエラー: {e}"))?;
            buf.put(chunk.as_ref());

            // バッファから完全なメッセージが取り出せる限りデコード
            let mut batch = Vec::new();
            loop {
                let (event_opt, consumed) = try_pop_segment_event(&buf);
                if consumed == 0 {
                    break; // データ不足、次のチャンクを待つ
                }
                buf.advance(consumed);
                if let Some(event) = event_opt {
                    if let Some(msg) = segment_event_to_provider_msg(event) {
                        batch.push(msg);
                    }
                }
            }

            // チャンクごとに溜まったメッセージをまとめて送信
            if !batch.is_empty() {
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    comment_count = batch.len(),
                    "コメントバッチ送信"
                );
                let envelope = McvEnvelope {
                    event_id: Uuid::new_v4(),
                    connection_id,
                    messages: batch,
                    received_at: chrono::Utc::now().timestamp(),
                    raw_message: None,
                };
                let payload = CommentReceivedPayload {
                    connection_id,
                    envelope,
                };
                let mcv_msg = McvMessage::new_notification(
                    MessageType::CommentReceived,
                    MessageSource::Plugin {
                        plugin_id: logical_plugin_id.clone(),
                    },
                    MessageDestination::Core,
                    serde_json::to_value(&payload).unwrap_or_default(),
                );
                NicoLivePlugin::send_message(ctx.clone(), mcv_msg).await;
            }
        }

        Ok(())
    }

    // ── URL / WebSocket ユーティリティ ────────────────────────────────────────

    /// URL から live_id を抽出する。`nicolive_lib::extract_live_id` に委譲。
    pub(crate) fn extract_live_id(url: &str) -> Option<String> {
        extract_live_id(url)
    }

    /// ニコ生ページから WebSocket URL と視聴者情報を取得する。`nicolive_lib::fetch_websocket_url` に委譲。
    pub(crate) async fn fetch_websocket_url(
        live_id: &str,
    ) -> Result<nicolive_lib::NicoLiveConnectionData, String> {
        fetch_websocket_url(live_id).await
    }

    pub(crate) fn stop(&mut self) {
        if let Some(tx) = &self.cancel_tx {
            let _ = tx.send(true);
        }
        self.cancel_tx = None;
        self.task = None;
        self.running = false;
    }
}

// ─── 変換ヘルパー ─────────────────────────────────────────────────────────────────

/// `SegmentEvent` を `ProviderMessage` に変換する。
/// 表示不要なイベントは `None` を返す。
fn segment_event_to_provider_msg(event: SegmentEvent) -> Option<ProviderMessage> {
    let now = || chrono::Utc::now().timestamp();

    match event {
        SegmentEvent::Chat(chat) | SegmentEvent::ForwardedChat { chat, .. } => {
            let timestamp = chat.at_secs.unwrap_or_else(now);
            Some(ProviderMessage {
                id: chat.id.clone(),
                platform_message_id: Some(chat.id),
                service: ServiceId("nicolive".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: chat.user_id,
                    display_name: vec![MessagePart::Text {
                        text: chat.name.unwrap_or_default(),
                    }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::Chat,
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: chat.content }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }

        SegmentEvent::SimpleNotification { id, at_secs, text }
        | SegmentEvent::SimpleNotificationV2 { id, at_secs, text } => {
            let timestamp = at_secs.unwrap_or_else(now);
            Some(ProviderMessage {
                id: id.clone(),
                platform_message_id: Some(id),
                service: ServiceId("nicolive".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: "".to_string(),
                    display_name: vec![],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::System(SystemKind::Notice),
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }

        SegmentEvent::Gift {
            id,
            at_secs,
            advertiser_name,
            advertiser_user_id,
            point,
            item_name,
            message,
        } => {
            let timestamp = at_secs.unwrap_or_else(now);
            let display_text = if message.is_empty() {
                item_name.clone()
            } else {
                format!("{}: {}", item_name, message)
            };
            Some(ProviderMessage {
                id: id.clone(),
                platform_message_id: Some(id),
                service: ServiceId("nicolive".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: advertiser_user_id
                        .map(|u| u.to_string())
                        .unwrap_or_default(),
                    display_name: vec![MessagePart::Text {
                        text: advertiser_name,
                    }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::Monetary(MonetaryInfo {
                    amount: Money {
                        currency: "NCP".to_string(),
                        value_minor: point,
                    },
                    tier: None,
                    recurring: false,
                }),
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: display_text }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }

        SegmentEvent::Nicoad {
            id,
            at_secs,
            advertiser,
            point,
            message,
        } => {
            let timestamp = at_secs.unwrap_or_else(now);
            let display_text = message.unwrap_or_default();
            Some(ProviderMessage {
                id: id.clone(),
                platform_message_id: Some(id),
                service: ServiceId("nicolive".to_string()),
                channel: ChannelId("".to_string()),
                sender: ProviderSender {
                    id: "".to_string(),
                    display_name: vec![MessagePart::Text { text: advertiser }],
                    badges: vec![],
                    role: None,
                    avatar_url: None,
                },
                timestamp,
                kind: ProviderMessageKind::Monetary(MonetaryInfo {
                    amount: Money {
                        currency: "NCP".to_string(),
                        value_minor: point,
                    },
                    tier: None,
                    recurring: false,
                }),
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: display_text }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            })
        }
    }
}

// ─── テスト ──────────────────────────────────────────────────────────────────────
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
        conn.stop();
        assert!(!conn.running);
        assert!(conn.cancel_tx.is_none());
        assert!(conn.task.is_none());
    }
}
