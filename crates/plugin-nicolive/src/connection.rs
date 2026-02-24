//! ニコニコ生放送 接続管理

use futures_util::{stream::SplitSink, FutureExt, SinkExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, ProviderContent,
    ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
};
use nicolive_lib::{
    decode_chunked_messages, decode_segment_messages, decode_view_entries, extract_live_id,
    fetch_websocket_url, ServerTimeCache, ViewEntry,
};
use plugin_abi_helper::v3::prelude::*;
use std::{io::Write, sync::Arc};
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

// ─── MessageAction ───────────────────────────────────────────────────────────────

/// `handle_text_message` の処理結果
enum MessageAction {
    Continue,
    Stop,
    Reconnect {
        audience_token: String,
        wait_secs: u64,
    },
    StartViewPolling {
        view_uri: String,
    },
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

    pub(crate) fn connect(&mut self, ctx: PluginContext, logical_plugin_id: Uuid, ws_url: &str) {
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
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        ws_url: String,
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
                    logical_plugin_id,
                    connection_id,
                    &ws_url,
                    &mut cancel_rx,
                )
                .await;

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
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        initial_ws_url: &str,
        cancel_rx: &mut watch::Receiver<bool>,
    ) {
        let mut current_url = initial_ws_url.to_string();

        'outer: loop {
            let reconnect = Self::run_single_connection(
                ctx.clone(),
                logical_plugin_id,
                connection_id,
                &current_url,
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
        logical_plugin_id: Uuid,
        connection_id: Uuid,
        ws_url: &str,
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

        let mut keep_seat_interval: Option<Interval> = None;
        let mut reconnect_result: Option<(String, u64)> = None;
        let mut view_poll_guard: Option<AbortGuard> = None;
        let time_cache = Arc::new(ServerTimeCache::new());

        loop {
            tokio::select! {
                _ = cancel_rx.changed() => {
                    if *cancel_rx.borrow() {
                        tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "切断要求を受信");
                        break;
                    }
                }
                _ = async {
                    match keep_seat_interval {
                        Some(ref mut interval) => { interval.tick().await; }
                        None => std::future::pending::<()>().await,
                    }
                } => {
                    let keep_seat = serde_json::json!({"type": "keepSeat"});
                    if let Err(e) = write.send(WsMessage::Text(keep_seat.to_string().into())).await {
                        tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "keepSeat 送信失敗");
                        break;
                    }
                    tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "keepSeat 送信");
                }
                recv = read.next() => {
                    match recv {
                        Some(Ok(WsMessage::Text(text))) => {
                            match Self::handle_text_message(&mut write, &mut keep_seat_interval, connection_id, &text, &time_cache).await {
                                MessageAction::Continue => {}
                                MessageAction::Stop => break,
                                MessageAction::Reconnect { audience_token, wait_secs } => {
                                    reconnect_result = Some((audience_token, wait_secs));
                                    break;
                                }
                                MessageAction::StartViewPolling { view_uri } => {
                                    tracing::info!(
                                        target: "mcv::plugin-nicolive",
                                        connection_id = %connection_id,
                                        view_uri = %view_uri,
                                        "viewUri polling タスクを spawn"
                                    );
                                    let ctx_poll = ctx.clone();
                                    view_poll_guard = Some(AbortGuard(
                                        tokio::spawn(Self::poll_view_uri(
                                            view_uri, connection_id,
                                            ctx_poll, logical_plugin_id,
                                        ))
                                    ));
                                }
                            }
                        }
                        Some(Ok(WsMessage::Close(_))) => {
                            tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket Close フレーム受信");
                            break;
                        }
                        Some(Ok(WsMessage::Ping(data))) => {
                            if let Err(e) = write.send(WsMessage::Pong(data)).await {
                                tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "Pong 送信失敗");
                                break;
                            }
                        }
                        Some(Ok(_)) => {}
                        Some(Err(e)) => {
                            tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "WebSocket 受信エラー");
                            break;
                        }
                        None => {
                            tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "WebSocket ストリーム終了");
                            break;
                        }
                    }
                }
            }
        }

        drop(view_poll_guard);
        reconnect_result
    }

    // ── viewUri ポーリングタスク ──────────────────────────────────────────────

    async fn poll_view_uri(
        view_uri: String,
        connection_id: Uuid,
        ctx: PluginContext,
        logical_plugin_id: Uuid,
    ) {
        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            view_uri = %view_uri,
            "viewUri polling 開始"
        );

        let _end_guard = PollEndGuard { connection_id };

        let client = reqwest::Client::builder()
            .build()
            .unwrap_or_else(|_| reqwest::Client::new());

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
                logical_plugin_id,
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

    /// viewUri を 1 回 GET し、レスポンス内の全 ChunkedEntry をデコードする。
    ///
    /// `Next` エントリがあれば `Some((待機時間, 次回 at))` を返す。
    async fn fetch_and_decode_view(
        client: &reqwest::Client,
        view_uri: &str,
        at_param: &str,
        connection_id: Uuid,
        ctx: &PluginContext,
        logical_plugin_id: Uuid,
        fetched_uris: &mut std::collections::HashSet<String>,
    ) -> Result<Option<(Duration, String)>, String> {
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
            .header("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36")
            .send()
            .await
            .map_err(|e| format!("GET 失敗: {e}"))?;

        let status = response.status();
        let bytes = response
            .bytes()
            .await
            .map_err(|e| format!("レスポンス読み取り失敗: {e}"))?;

        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            status = %status,
            bytes_len = bytes.len(),
            "viewUri レスポンス受信"
        );

        if !status.is_success() {
            return Err(format!("HTTP エラー: {status}"));
        }

        let entries = decode_view_entries(&bytes);
        let mut file = std::fs::OpenOptions::new()
            .create(true) // ファイルがなければ作成
            .append(true) // 追記モード
            .open("a.txt")
            .unwrap();

        file.write_all(format!("{:?}", entries).as_bytes()).unwrap();
        file.write_all(b"\n").unwrap(); // 改行を追加したい場合
        let mut next_info: Option<(Duration, String)> = None;
        let mut entry_count = 0u32;
        // 今回取得すべき新規セグメント URI を収集する（重複チェック済み）
        let mut segment_uris: Vec<String> = Vec::new();

        for entry in &entries {
            entry_count += 1;
            match entry {
                ViewEntry::Segment { uri } | ViewEntry::Previous { uri } => {
                    if fetched_uris.contains(uri) {
                        tracing::debug!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            uri = %uri,
                            "セグメント重複スキップ"
                        );
                    } else {
                        tracing::info!(
                            target: "mcv::plugin-nicolive",
                            connection_id = %connection_id,
                            uri = %uri,
                            "Segment/Previous 受信: コメント取得対象"
                        );
                        fetched_uris.insert(uri.clone());
                        segment_uris.push(uri.clone());
                    }
                }
                ViewEntry::Next { at } => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        at = at,
                        "ReadyForNext 受信"
                    );
                    // Duration はダミー（呼び出し元では使用しない）
                    next_info = Some((Duration::ZERO, at.to_string()));
                }
                ViewEntry::Backward { segment_uri, .. } => {
                    // BackwardSegment.segment.uri は /data/segment/v4/ 形式 → 通常通り取得可能
                    // snapshot_uri (/data/backward/v4/) は異なるフォーマットのためスキップ
                    if let Some(uri) = segment_uri {
                        if fetched_uris.contains(uri) {
                            tracing::debug!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                uri = %uri,
                                "Backward セグメント重複スキップ"
                            );
                        } else {
                            tracing::info!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                uri = %uri,
                                "Backward segment 受信: コメント取得対象"
                            );
                            fetched_uris.insert(uri.clone());
                            segment_uris.push(uri.clone());
                        }
                    }
                }
            }
        }

        // 複数セグメントを並列で取得（直列だと後ろのセグメントほど遅延が増える）
        if !segment_uris.is_empty() {
            let fetch_futures = segment_uris.iter().map(|uri| {
                let client = client.clone();
                let ctx = ctx.clone();
                let uri = uri.clone();
                async move {
                    let result = Self::fetch_segment_messages(
                        &client,
                        &uri,
                        connection_id,
                        &ctx,
                        logical_plugin_id,
                    )
                    .await;
                    (uri, result)
                }
            });
            let results = futures_util::future::join_all(fetch_futures).await;
            for (uri, result) in results {
                if let Err(e) = result {
                    tracing::warn!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connection_id,
                        error = %e,
                        uri = %uri,
                        "セグメントメッセージ取得失敗"
                    );
                }
            }
        }

        tracing::info!(
            target: "mcv::plugin-nicolive",
            connection_id = %connection_id,
            entry_count = entry_count,
            has_next = next_info.is_some(),
            "viewUri エントリ処理完了"
        );

        Ok(next_info)
    }

    /// セグメント URI からコメント一覧を取得し CommentReceived を送信する。
    async fn fetch_segment_messages(
        client: &reqwest::Client,
        segment_uri: &str,
        connection_id: Uuid,
        ctx: &PluginContext,
        logical_plugin_id: Uuid,
    ) -> Result<(), String> {
        let response = client
            .get(segment_uri)
            .header("Accept", "*/*")
            .header("Origin", "https://live.nicovideo.jp")
            .header("Referer", "https://live.nicovideo.jp/")
            .header("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36")
            .send()
            .await
            .map_err(|e| format!("セグメント GET 失敗: {e}"))?;

        if !response.status().is_success() {
            return Err(format!("セグメント HTTP エラー: {}", response.status()));
        }

        let bytes = response
            .bytes()
            .await
            .map_err(|e| format!("セグメント読み取り失敗: {e}"))?;

        // 調査用: 生の ChunkedMessage をパースして nico_segment_data.txt に追記
        {
            let raw = decode_chunked_messages(&bytes);
            if let Ok(mut f) = std::fs::OpenOptions::new()
                .create(true)
                .append(true)
                .open("nico_segment_data.txt")
            {
                for msg in &raw {
                    let _ = writeln!(f, "{:#?}", msg);
                    let _ = writeln!(f, "---");
                }
            }
        }

        let messages = decode_segment_messages(&bytes);
        let mut file = std::fs::OpenOptions::new()
            .create(true) // ファイルがなければ作成
            .append(true) // 追記モード
            .open("b.txt")
            .unwrap();

        file.write_all(format!("{:?}", messages).as_bytes())
            .unwrap();
        file.write_all(b"\n").unwrap(); // 改行を追加したい場合

        // 1セグメント HTTP レスポンス内の全コメントを収集して1つの McvEnvelope にまとめる
        let mut provider_messages = Vec::new();
        for chat in messages {
            let timestamp = chat
                .at_secs
                .unwrap_or_else(|| chrono::Utc::now().timestamp());
            let provider_msg = ProviderMessage {
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
                },
                timestamp,
                kind: ProviderMessageKind::Chat,
                content: ProviderContent::Text {
                    text: vec![MessagePart::Text { text: chat.content }],
                },
                reply_to: None,
                metadata: serde_json::Value::Null,
            };
            provider_messages.push(provider_msg);
        }
        if !provider_messages.is_empty() {
            tracing::info!(
                target: "mcv::plugin-nicolive",
                connection_id = %connection_id,
                comment_count = provider_messages.len(),
                "コメント送信完了"
            );
            let envelope = McvEnvelope {
                event_id: Uuid::new_v4(),
                connection_id,
                messages: provider_messages,
                received_at: chrono::Utc::now().timestamp(),
                // データ形式が protobuf バイナリのため文字列として保存しない
                raw_message: None,
            };
            let payload = CommentReceivedPayload {
                connection_id,
                envelope,
            };
            let mcv_msg = McvMessage::new_notification(
                MessageType::CommentReceived,
                MessageSource::Plugin {
                    plugin_id: logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(&payload).unwrap_or_default(),
            );
            NicoLivePlugin::send_message(ctx.clone(), mcv_msg).await;
        }

        Ok(())
    }

    // ── アプリレベルメッセージ処理 ────────────────────────────────────────────
    async fn handle_text_message(
        write: &mut WsSink,
        keep_seat_interval: &mut Option<Interval>,
        connection_id: Uuid,
        text: &str,
        time_cache: &Arc<ServerTimeCache>,
    ) -> MessageAction {
        let json: serde_json::Value = match serde_json::from_str(text) {
            Ok(v) => v,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    error = %e,
                    raw = %text,
                    "JSON パース失敗"
                );
                return MessageAction::Continue;
            }
        };

        let msg_type = match json["type"].as_str() {
            Some(t) => t,
            None => return MessageAction::Continue,
        };

        match msg_type {
            "ping" => {
                let pong = serde_json::json!({"type": "pong"});
                if let Err(e) = write.send(WsMessage::Text(pong.to_string().into())).await {
                    tracing::error!(target: "mcv::plugin-nicolive", connection_id = %connection_id, error = %e, "pong 送信失敗");
                    return MessageAction::Stop;
                }
                tracing::trace!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "pong 送信");
            }
            "seat" => {
                let keep_interval_sec = json["data"]["keepIntervalSec"].as_u64().unwrap_or(30);
                let duration = Duration::from_secs(keep_interval_sec);
                *keep_seat_interval = Some(interval_at(Instant::now() + duration, duration));
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    keep_interval_sec = keep_interval_sec,
                    "seat 受信: keepSeat インターバル設定"
                );
            }
            "messageServer" => {
                let view_uri = json["data"]["viewUri"]
                    .as_str()
                    .unwrap_or_default()
                    .to_string();
                let vpos_base_time = json["data"]["vposBaseTime"].as_str().unwrap_or_default();
                // data の全フィールドをログ出力（未知フィールドの発見用）
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    view_uri = %view_uri,
                    vpos_base_time = %vpos_base_time,
                    raw_data = %json["data"],
                    "messageServer 受信"
                );
                if !view_uri.is_empty() {
                    return MessageAction::StartViewPolling { view_uri };
                }
            }
            "reconnect" => {
                let audience_token = json["data"]["audienceToken"]
                    .as_str()
                    .unwrap_or_default()
                    .to_string();
                let wait_secs = json["data"]["waitTimeSec"].as_u64().unwrap_or(0);
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    audience_token = %audience_token,
                    wait_secs = wait_secs,
                    "reconnect 受信"
                );
                return MessageAction::Reconnect {
                    audience_token,
                    wait_secs,
                };
            }
            "disconnect" => {
                tracing::info!(target: "mcv::plugin-nicolive", connection_id = %connection_id, "サーバーから disconnect を受信");
                return MessageAction::Stop;
            }
            "serverTime" => {
                if let Some(current_ms) = json["data"]["currentMs"].as_str() {
                    match chrono::DateTime::parse_from_rfc3339(current_ms) {
                        Ok(dt) => {
                            time_cache.update(dt.timestamp());
                            tracing::info!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                server_secs = dt.timestamp(),
                                "serverTime 受信: 時刻キャッシュ更新"
                            );
                        }
                        Err(e) => {
                            tracing::warn!(
                                target: "mcv::plugin-nicolive",
                                connection_id = %connection_id,
                                error = %e,
                                raw = current_ms,
                                "serverTime パース失敗"
                            );
                        }
                    }
                }
            }
            _ => {
                tracing::info!(
                    target: "mcv::plugin-nicolive",
                    connection_id = %connection_id,
                    msg_type = msg_type,
                    raw = %text,
                    "未処理メッセージ受信"
                );
            }
        }

        MessageAction::Continue
    }

    // ── URL / WebSocket ユーティリティ ────────────────────────────────────────

    /// URL から live_id を抽出する。`nicolive_lib::extract_live_id` に委譲。
    pub(crate) fn extract_live_id(url: &str) -> Option<String> {
        extract_live_id(url)
    }

    /// ニコ生ページから WebSocket URL を取得する。`nicolive_lib::fetch_websocket_url` に委譲。
    pub(crate) async fn fetch_websocket_url(live_id: &str) -> Result<String, String> {
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
