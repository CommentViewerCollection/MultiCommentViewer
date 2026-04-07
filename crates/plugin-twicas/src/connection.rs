use crate::TwicasPlugin;
use futures_util::{FutureExt, StreamExt};
use mcv_messages::{
    ChannelId, CommentReceivedPayload, DisconnectedPayload, McvEnvelope, Message as McvMessage,
    MessageDestination, MessagePart, MessageSource, MessageType, PluginId, ProviderContent,
    ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId, StreamMetadataPayload,
};
use plugin_abi_helper::v3::prelude::*;
use reqwest::cookie::CookieStore as _;
use serde::Deserialize;
use serde_json::Value;
use std::sync::{
    atomic::{AtomicBool, Ordering},
    Arc,
};
use tokio::sync::{watch, RwLock};
use tokio::task::JoinHandle;
use tokio::time::{timeout, Duration, Instant};
use tokio_tungstenite::{connect_async, tungstenite::Message as WsMessage};
use twicas_lib::{
    fetch_event_pubsub_url, fetch_latest_movie, fetch_movie_info, fetch_movie_token,
    fetch_session_ids, fetch_viewer_status, MovieInfo,
};

/// JS の PlayerPage から抽出したシークレット（extract-secret ツールで取得: 2026-04-05 時点）
/// SECRET は JS 更新のたびに変わるため、`cargo run -p extract-secret` で最新値を確認すること
const SECRET: &str = "b0k5hdsh1bob1iog";
use uuid::Uuid;

/// コメント投稿に必要な接続状態
pub(crate) struct CommentPostState {
    pub(crate) movie_id: i64,
    pub(crate) screen_name: String,
    pub(crate) cs_session_id: String,
    pub(crate) client: Arc<reqwest::Client>,
    pub(crate) jar: Arc<reqwest::cookie::Jar>,
}

pub(crate) struct Connection {
    pub(crate) id: Uuid,
    pub(crate) cancel_tx: Option<watch::Sender<bool>>,
    pub(crate) task: Option<JoinHandle<()>>,
    pub(crate) running: Arc<AtomicBool>,
    /// 接続確立後にセットされるコメント投稿用状態
    pub(crate) comment_post: Arc<RwLock<Option<CommentPostState>>>,
}

impl Connection {
    pub(crate) fn new(id: &Uuid) -> Self {
        Self {
            id: *id,
            cancel_tx: None,
            task: None,
            running: Arc::new(AtomicBool::new(false)),
            comment_post: Arc::new(RwLock::new(None)),
        }
    }

    #[allow(clippy::too_many_arguments)]
    pub(crate) fn connect(
        &mut self,
        ctx: PluginContext,
        logical_plugin_id: PluginId,
        user_name: &str,
        wpass: Option<&str>,
        // resolve_wpass で取得した cs_session_id（プライベート配信用フォールバック）
        cs_session_id_from_wpass: Option<String>,
        session_client: Arc<reqwest::Client>,
        session_jar: Arc<reqwest::cookie::Jar>,
    ) {
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
        let wpass = wpass.map(|s| s.to_string());
        let running_flag = Arc::clone(&self.running);
        let comment_post = Arc::clone(&self.comment_post);

        let task = tokio::spawn(async move {
            let task_result =
                std::panic::AssertUnwindSafe(async {
                    let run_result: Result<(), ()> = async {
                    let client = session_client;
                    let mut sent_waiting_metadata = false;

                    // 配信開始/終了に対応するリトライループ。
                    // 配信中でない場合は待機し、配信開始を検知したら自動接続する。
                    // 配信終了時は切断せず、次の配信開始まで待機状態に戻る。
                    'retry: loop {
                        // キャンセルチェック
                        if *cancel_rx.borrow() {
                            break 'retry;
                        }

                        let (session_id, cs_session_id, movie_id_from_html) =
                            match fetch_session_ids(&client, &user_name).await {
                                Ok(v) => v,
                                Err(e) => {
                                    tracing::error!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connection_id,
                                        user_name = %user_name,
                                        error = %e,
                                        "Failed to fetch session IDs. Retrying in 5s"
                                    );
                                    if wait_or_cancel(&mut cancel_rx, 5000).await {
                                        break 'retry;
                                    }
                                    continue 'retry;
                                }
                            };

                        // twitcasting.tv で得たクッキー（did 等）を
                        // frontendapi.twitcasting.tv 用にコピーする。
                        // did はホスト専用クッキーの可能性があるため、明示的に登録する。
                        {
                            let src: reqwest::Url = "https://twitcasting.tv/".parse().unwrap();
                            let dst: reqwest::Url =
                                "https://frontendapi.twitcasting.tv/".parse().unwrap();

                            let src_cookies = session_jar
                                .cookies(&src)
                                .and_then(|h| h.to_str().ok().map(|s| s.to_string()))
                                .unwrap_or_else(|| "(なし)".to_string());
                            tracing::debug!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                cookies = %src_cookies,
                                "コピー前 twitcasting.tv jar"
                            );

                            if let Some(header) = session_jar.cookies(&src) {
                                if let Ok(s) = header.to_str() {
                                    for part in s.split(';') {
                                        let part = part.trim();
                                        if !part.is_empty() {
                                            session_jar.add_cookie_str(part, &dst);
                                        }
                                    }
                                }
                            }

                            let dst_cookies = session_jar
                                .cookies(&dst)
                                .and_then(|h| h.to_str().ok().map(|s| s.to_string()))
                                .unwrap_or_else(|| "(なし)".to_string());
                            tracing::debug!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                cookies = %dst_cookies,
                                "コピー後 frontendapi.twitcasting.tv jar"
                            );
                        }

                        // movie_id の取得: HTML の data-movie-id を優先する。
                        // ログイン済み・非ログインともに配信中であれば HTML に含まれる。
                        // HTML にない場合のみ fetch_latest_movie（非ログインユーザー向け）にフォールバックする。
                        let movie = match movie_id_from_html {
                            Some(mid) => {
                                tracing::info!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connection_id,
                                    user_name = %user_name,
                                    movie_id = mid,
                                    "HTML から movie_id を取得しました"
                                );
                                MovieInfo { id: mid, is_on_live: true }
                            }
                            None => {
                                if session_id.is_empty() {
                                    // ログイン済みユーザー、かつ HTML に movie_id がない
                                    // → 配信中でないため待機
                                    tracing::info!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connection_id,
                                        user_name = %user_name,
                                        "movie_id が HTML から取得できません。配信開始を待機します"
                                    );
                                    if !sent_waiting_metadata {
                                        send_stream_metadata(
                                            ctx.clone(),
                                            logical_plugin_id.clone(),
                                            StreamMetadataPayload {
                                                connection_id,
                                                title: Some(
                                                    "（次の配信が始まるまで待機中...）"
                                                        .to_string(),
                                                ),
                                                viewer_count: None,
                                                total_viewer_count: None,
                                                start_time: None,
                                                others: None,
                                                clear: Some(true),
                                            },
                                        )
                                        .await;
                                        sent_waiting_metadata = true;
                                    }
                                    if wait_or_cancel(&mut cancel_rx, 5000).await {
                                        break 'retry;
                                    }
                                    continue 'retry;
                                }

                                // 非ログインユーザー向け: fetch_latest_movie で movie_id と配信状態を確認
                                let latest_movie = match fetch_latest_movie(
                                    &client,
                                    &session_id,
                                    SECRET,
                                    &user_name,
                                    wpass.as_deref(),
                                )
                                .await
                                {
                                    Ok(v) => v,
                                    Err(e) => {
                                        tracing::error!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            user_name = %user_name,
                                            error = %e,
                                            "Failed to fetch latest movie. Retrying in 5s"
                                        );
                                        if wait_or_cancel(&mut cancel_rx, 5000).await {
                                            break 'retry;
                                        }
                                        continue 'retry;
                                    }
                                };

                                match latest_movie.movie {
                                    Some(v) if v.is_on_live => {
                                        tracing::info!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            user_name = %user_name,
                                            movie_id = v.id,
                                            "fetch_latest_movie: 配信中を確認"
                                        );
                                        v
                                    }
                                    _ => {
                                        // 配信中でない → 待機
                                        tracing::info!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            user_name = %user_name,
                                            "配信中でないため配信開始を待機します"
                                        );
                                        if !sent_waiting_metadata {
                                            send_stream_metadata(
                                                ctx.clone(),
                                                logical_plugin_id.clone(),
                                                StreamMetadataPayload {
                                                    connection_id,
                                                    title: Some(
                                                        "（次の配信が始まるまで待機中...）"
                                                            .to_string(),
                                                    ),
                                                    viewer_count: None,
                                                    total_viewer_count: None,
                                                    start_time: None,
                                                    others: None,
                                                    clear: Some(true),
                                                },
                                            )
                                            .await;
                                            sent_waiting_metadata = true;
                                        }
                                        if wait_or_cancel(&mut cancel_rx, 5000).await {
                                            break 'retry;
                                        }
                                        continue 'retry;
                                    }
                                }
                            }
                        };

                        // ── 以下は配信中 ──

                        // コメント投稿用の状態を保存
                        // cs_session_id: 認証済みページでは None になる場合があるため、
                        // resolve_wpass 時に取得したものをフォールバックとして使用する
                        let effective_cs_session_id = cs_session_id
                            .or_else(|| cs_session_id_from_wpass.clone())
                            .unwrap_or_default();
                        tracing::debug!(
                            target: "mcv::plugin-twicas",
                            connection_id = %connection_id,
                            cs_session_id = %effective_cs_session_id,
                            "cs_session_id 確定"
                        );
                        {
                            let mut state = comment_post.write().await;
                            *state = Some(CommentPostState {
                                movie_id: movie.id,
                                screen_name: user_name.clone(),
                                cs_session_id: effective_cs_session_id,
                                client: Arc::clone(&client),
                                jar: Arc::clone(&session_jar),
                            });
                        }

                        // wpass が未解決の場合、ブラウザ Cookie の jar から wpass を抽出する。
                        // jar の Cookie はリクエスト時に自動送信されるが、
                        // password パラメータにも明示的に渡すことで確実に認証する。
                        let twicas_url: reqwest::Url = "https://twitcasting.tv/".parse().unwrap();
                        let wpass_from_jar: Option<String> = if wpass.is_none() {
                            session_jar
                                .cookies(&twicas_url)
                                .and_then(|h| h.to_str().ok().map(|s| s.to_string()))
                                .and_then(|s| {
                                    s.split(';')
                                        .map(|c| c.trim())
                                        .find(|c| c.starts_with("wpass="))
                                        .and_then(|c| {
                                            c.strip_prefix("wpass=").map(|v| v.to_string())
                                        })
                                })
                        } else {
                            None
                        };
                        let effective_wpass = wpass.as_deref().or(wpass_from_jar.as_deref());
                        let effective_wpass_owned: Option<String> =
                            effective_wpass.map(|s| s.to_string());

                        {
                            let jar_cookies = session_jar
                                .cookies(&twicas_url)
                                .and_then(|h| h.to_str().ok().map(|s| s.to_string()))
                                .unwrap_or_else(|| "(なし)".to_string());
                            tracing::debug!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                cookies = %jar_cookies,
                                has_wpass = wpass.is_some(),
                                has_wpass_from_jar = wpass_from_jar.is_some(),
                                effective_has_wpass = effective_wpass.is_some(),
                                "fetch_event_pubsub_url 呼び出し前の twitcasting.tv jar cookies"
                            );
                        }

                        let ws_url =
                            match fetch_event_pubsub_url(&client, movie.id, effective_wpass).await
                            {
                                Ok(v) => v,
                                Err(e) => {
                                    tracing::error!(
                                        target: "mcv::plugin-twicas",
                                        connection_id = %connection_id,
                                        movie_id = movie.id,
                                        error = %e,
                                        "Failed to fetch event pubsub URL. Retrying in 5s"
                                    );
                                    if wait_or_cancel(&mut cancel_rx, 5000).await {
                                        break 'retry;
                                    }
                                    continue 'retry;
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
                                    "Timed out while connecting websocket. Retrying in 5s"
                                );
                                if wait_or_cancel(&mut cancel_rx, 5000).await {
                                    break 'retry;
                                }
                                continue 'retry;
                            }
                            Ok(Err(e)) => {
                                tracing::error!(
                                    target: "mcv::plugin-twicas",
                                    connection_id = %connection_id,
                                    error = %e,
                                    "Failed to connect websocket. Retrying in 5s"
                                );
                                if wait_or_cancel(&mut cancel_rx, 5000).await {
                                    break 'retry;
                                }
                                continue 'retry;
                            }
                            Ok(Ok(v)) => v,
                        };

                        // メタデータポーリングタスクを起動
                        let metadata_cancel_rx = cancel_rx.clone();
                        let metadata_handle = tokio::spawn(metadata_polling_loop(
                            ctx.clone(),
                            logical_plugin_id.clone(),
                            connection_id,
                            Arc::clone(&client),
                            movie.id,
                            user_name.clone(),
                            effective_wpass_owned,
                            session_id.clone(),
                            metadata_cancel_rx,
                        ));

                        let (_write, mut read) = ws_stream.split();
                        // true: サーバー側からの終了（配信終了 or 受信エラー）
                        // false: ユーザーによるキャンセル
                        let mut ws_recv_error = false;
                        let stream_ended_by_server = loop {
                            tokio::select! {
                                _ = cancel_rx.changed() => {
                                    if *cancel_rx.borrow() {
                                        tracing::info!(
                                            target: "mcv::plugin-twicas",
                                            connection_id = %connection_id,
                                            "Twicas websocket loop cancelled"
                                        );
                                        break false;
                                    }
                                }
                                recv = read.next() => {
                                    match recv {
                                        Some(Ok(WsMessage::Text(text))) => {
                                            handle_text_message(
                                                ctx.clone(),
                                                logical_plugin_id.clone(),
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
                                            break true;
                                        }
                                        Some(Ok(_)) => {}
                                        Some(Err(e)) => {
                                            tracing::error!(
                                                target: "mcv::plugin-twicas",
                                                connection_id = %connection_id,
                                                error = %e,
                                                "Error while receiving websocket message. Will retry"
                                            );
                                            ws_recv_error = true;
                                            break true;
                                        }
                                        None => {
                                            tracing::info!(
                                                target: "mcv::plugin-twicas",
                                                connection_id = %connection_id,
                                                "WebSocket stream ended"
                                            );
                                            break true;
                                        }
                                    }
                                }
                            }
                        };

                        metadata_handle.abort();

                        // コメント投稿状態をクリア
                        {
                            let mut state = comment_post.write().await;
                            *state = None;
                        }

                        if !stream_ended_by_server {
                            // ユーザーキャンセル → ループを抜けて切断
                            break 'retry;
                        }

                        // 配信終了 or 受信エラー → 待機状態へ遷移し、次の配信開始を待つ
                        if ws_recv_error {
                            tracing::warn!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                "WebSocket受信エラーにより切断されました。再接続を試みます"
                            );
                        } else {
                            tracing::info!(
                                target: "mcv::plugin-twicas",
                                connection_id = %connection_id,
                                "配信終了。次の配信開始を待機します"
                            );
                        }
                        send_stream_metadata(
                            ctx.clone(),
                            logical_plugin_id.clone(),
                            StreamMetadataPayload {
                                connection_id,
                                title: Some("（次の配信が始まるまで待機中...）".to_string()),
                                viewer_count: None,
                                total_viewer_count: None,
                                start_time: None,
                                others: None,
                                clear: Some(true),
                            },
                        )
                        .await;
                        sent_waiting_metadata = true;

                        if wait_or_cancel(&mut cancel_rx, 5000).await {
                            break 'retry;
                        }
                        // continue 'retry: 次の配信を待つ
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

                    // コメント投稿状態をクリア
                    {
                        let mut state = comment_post.write().await;
                        *state = None;
                    }

                    let message = McvMessage::new_notification(
                        MessageType::Disconnected,
                        MessageSource::Plugin {
                            plugin_id: logical_plugin_id.clone(),
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

    /// コメントを投稿する
    ///
    /// 接続確立前や切断後は `Err` を返す。
    pub(crate) async fn post_comment(&self, text: &str, anonymous: bool) -> Result<(), String> {
        let state = self.comment_post.read().await;
        let state = match &*state {
            Some(s) => s,
            None => return Err("接続中ではありません".to_string()),
        };
        let resp = twicas_lib::post_comment(
            &state.client,
            &state.jar,
            &state.screen_name,
            state.movie_id,
            text,
            &state.cs_session_id,
            anonymous,
        )
        .await
        .map_err(|e| e.to_string())?;

        if let Some(error) = &resp.error {
            if !error.is_null() {
                return Err(format!("コメント投稿APIエラー: {}", error));
            }
        }
        Ok(())
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

/// キャンセルを待つか、指定ミリ秒タイムアウトする。
/// Returns `true` if cancelled, `false` if timed out.
async fn wait_or_cancel(cancel_rx: &mut watch::Receiver<bool>, ms: u64) -> bool {
    tokio::select! {
        _ = tokio::time::sleep(Duration::from_millis(ms)) => false,
        result = cancel_rx.changed() => {
            result.is_err() || *cancel_rx.borrow()
        }
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
    logical_plugin_id: PluginId,
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

async fn send_stream_metadata(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    payload: StreamMetadataPayload,
) {
    let message = McvMessage::new_notification(
        MessageType::StreamMetadata,
        MessageSource::Plugin {
            plugin_id: logical_plugin_id,
        },
        MessageDestination::Core,
        serde_json::to_value(payload).unwrap(),
    );
    TwicasPlugin::send_message(ctx, message).await;
}

#[allow(clippy::too_many_arguments)]
async fn metadata_polling_loop(
    ctx: PluginContext,
    logical_plugin_id: PluginId,
    connection_id: Uuid,
    client: Arc<reqwest::Client>,
    movie_id: i64,
    user_name: String,
    wpass: Option<String>,
    session_id: String,
    mut cancel_rx: watch::Receiver<bool>,
) {
    tracing::info!(
        target: "mcv::plugin-twicas",
        connection_id = %connection_id,
        movie_id,
        has_session_id = !session_id.is_empty(),
        "メタデータポーリング開始"
    );

    // 接続時点で実際に配信中かどうかを確認する。
    // movie_id_from_html はアーカイブページでも存在するため、
    // is_on_live をハードコードせず fetch_latest_movie で確認する。
    match fetch_latest_movie(&client, &session_id, SECRET, &user_name, wpass.as_deref()).await {
        Ok(resp) => {
            let is_live = resp.movie.map(|m| m.is_on_live).unwrap_or(false);
            if !is_live {
                tracing::info!(
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    "配信中でないため待機中メタデータを送信してポーリングを終了"
                );
                send_stream_metadata(
                    ctx,
                    logical_plugin_id,
                    StreamMetadataPayload {
                        connection_id,
                        title: Some("（次の配信が始まるまで待機中...）".to_string()),
                        viewer_count: None,
                        total_viewer_count: None,
                        start_time: None,
                        others: None,
                        clear: Some(true),
                    },
                )
                .await;
                return;
            }
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twicas",
                connection_id = %connection_id,
                error = %e,
                "fetch_latest_movie 失敗、配信中と仮定してポーリングを続行"
            );
        }
    }

    let mut poll_count: u64 = 0;

    // /movies/{id}/token エンドポイントから視聴用トークンを取得する。
    // session_id が空（ログイン済みユーザー）の場合はトークンなしで続行。
    let token: Option<String> = if session_id.is_empty() {
        tracing::info!(
            target: "mcv::plugin-twicas",
            connection_id = %connection_id,
            "session_id が空のためトークンなしで続行（ログイン済み cookie で認証）"
        );
        None
    } else {
        match fetch_movie_token(
            &client,
            movie_id,
            wpass.as_deref().unwrap_or(""),
            &session_id,
            SECRET,
        )
        .await
        {
            Ok(t) => {
                tracing::info!(
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    "fetch_movie_token 成功"
                );
                Some(t.token)
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    error = %e,
                    "fetch_movie_token 失敗、トークンなしで続行"
                );
                None
            }
        }
    };

    // 配信開始時刻を取得して即時送信
    // ブラウザは /info に token を URL パラメータとして渡すだけで認証ヘッダーを送らないため
    // session_id は渡さない。
    match fetch_movie_info(&client, movie_id, token.as_deref(), "", SECRET).await {
        Ok(info) => {
            tracing::info!(
                target: "mcv::plugin-twicas",
                connection_id = %connection_id,
                started_at = info.started_at,
                "fetch_movie_info 成功、start_time を送信"
            );
            send_stream_metadata(
                ctx.clone(),
                logical_plugin_id.clone(),
                StreamMetadataPayload {
                    connection_id,
                    title: None,
                    viewer_count: None,
                    total_viewer_count: None,
                    start_time: Some(info.started_at),
                    others: None,
                    clear: None,
                },
            )
            .await;
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-twicas",
                connection_id = %connection_id,
                error = %e,
                "fetch_movie_info 失敗"
            );
        }
    }

    // 視聴者数ポーリングループ
    loop {
        if *cancel_rx.borrow() {
            tracing::info!(
                target: "mcv::plugin-twicas",
                connection_id = %connection_id,
                poll_count,
                "メタデータポーリング: キャンセル受信、終了"
            );
            break;
        }

        poll_count += 1;
        let loop_start = Instant::now();

        // ブラウザは /status/viewer にも token を URL パラメータとして渡すだけで
        // 認証ヘッダーを送らないため session_id は渡さない。
        let interval_secs = match fetch_viewer_status(
            &client,
            movie_id,
            token.as_deref(),
            "ja",
            "",
            SECRET,
        )
        .await
        {
            Ok(status) => {
                tracing::info!(
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    poll_count,
                    title = %status.movie.title,
                    viewer_count = status.movie.viewers.current,
                    total_viewer_count = status.movie.viewers.total,
                    next_poll_secs = status.update_interval_sec,
                    "メタデータポーリング: 取得成功、StreamMetadata 送信"
                );
                send_stream_metadata(
                    ctx.clone(),
                    logical_plugin_id.clone(),
                    StreamMetadataPayload {
                        connection_id,
                        title: Some(status.movie.title),
                        viewer_count: Some(status.movie.viewers.current),
                        total_viewer_count: Some(status.movie.viewers.total),
                        start_time: None,
                        others: None,
                        clear: None,
                    },
                )
                .await;
                status.update_interval_sec as u64
            }
            Err(e) => {
                tracing::warn!(
                    target: "mcv::plugin-twicas",
                    connection_id = %connection_id,
                    poll_count,
                    error = %e,
                    "メタデータポーリング: fetch_viewer_status 失敗、30秒後にリトライ"
                );
                30
            }
        };

        // サーバーが 0 を返した場合のビジーループを防ぐため最低 5 秒を保証する
        let interval_secs = interval_secs.max(5);
        let wait = Duration::from_secs(interval_secs).saturating_sub(loop_start.elapsed());
        tokio::select! {
            _ = tokio::time::sleep(wait) => {}
            result = cancel_rx.changed() => {
                if result.is_err() || *cancel_rx.borrow() {
                    tracing::info!(
                        target: "mcv::plugin-twicas",
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
