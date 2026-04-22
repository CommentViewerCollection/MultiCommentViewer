//! OPENREC.tv 内部 API クライアント
//!
//! - 配信ページ HTML から movieId を抽出する HTTP API クライアント
//! - `apiv5.openrec.tv` REST API クライアント
//! - socket.io v2（EIO=3）テキストフレームのパーサー

use chrono::DateTime;
use regex::Regex;
use serde::Deserialize;
use serde_json::Value;
use tokio::time::{timeout, Duration};

// ---------------------------------------------------------------------------
// 公開型定義
// ---------------------------------------------------------------------------

/// 配信ページから取得したストリーム情報
#[derive(Debug, Clone)]
pub struct StreamInfo {
    /// WebSocket 接続用の数値 ID（例: 3289482）
    pub movie_id: u64,
    /// REST チャット履歴 API 用の文字列スラッグ（例: "olryvq9qlr2"）
    pub movie_slug: Option<String>,
    pub title: Option<String>,
    pub start_time: Option<i64>,
    pub is_live: bool,
}

/// CHAT_ADD イベントのチャットデータ
#[derive(Debug, Clone)]
pub struct OpenrecChatData {
    pub chat_id: String,
    pub message: String,
    pub timestamp: i64,
    pub user_key: String,
    pub user_name: String,
    pub user_icon: Option<String>,
    pub is_official: bool,
    pub is_moderator: bool,
    pub is_premium: bool,
    /// スタンプ画像 URL（スタンプ送信時のみ）
    pub stamp_url: Option<String>,
    pub yell: Option<OpenrecYell>,
}

/// イェル（マネタイズイベント）データ
#[derive(Debug, Clone)]
pub struct OpenrecYell {
    pub points: i64,
    pub label: Option<String>,
    pub image_url: Option<String>,
}

/// socket.io "message" イベントから解析されたイベント
#[derive(Debug, Clone)]
pub enum OpenrecEvent {
    Chat(OpenrecChatData),
    SystemMessage {
        chat_id: Option<u64>,
        message: String,
    },
    StreamStart,
    StreamEnd,
    Subscribed,
    ChatlistMode,
    /// 視聴者数更新（type: 1）
    ///
    /// - `viewers`: 累計視聴者数
    /// - `live_viewers`: 現在のライブ視聴者数
    ViewerCount {
        viewers: u64,
        live_viewers: u64,
    },
}

/// チャット履歴取得のカーソル
///
/// - `BeforeTime`: 指定日時より前のチャットを取得（接続直後の初期ロードに使用）
/// - `BeforeId`: 指定チャット ID より前のチャットを取得（ページング）
pub enum ChatHistoryCursor {
    /// `to_created_at` + キャッシュバスティング用 `c`（ミリ秒タイムスタンプ）
    BeforeTime {
        to_created_at: String,
        c: Option<u64>,
    },
    /// `to_chat_id` で指定した ID より前のアイテムを返す
    BeforeId { to_chat_id: u64 },
}

/// チャット履歴内のユーザー情報（公開 API 用、フィールドに null が多い）
#[derive(Debug, Clone, Deserialize)]
pub struct OpenrecHistoryUser {
    pub id: String,
    pub nickname: String,
    pub openrec_user_id: u64,
    pub icon_image_url: String,
    pub l_icon_image_url: String,
    #[serde(default)]
    pub is_premium: bool,
    #[serde(default)]
    pub is_official: bool,
    #[serde(default)]
    pub is_partner: Option<bool>,
}

/// チャット履歴の 1 件（`/external/api/v5/movies/{id}/chats` レスポンス要素）
#[derive(Debug, Clone, Deserialize)]
pub struct OpenrecHistoryChat {
    pub id: u64,
    /// 0: 通常チャット
    pub chat_type: u32,
    pub message: String,
    /// サーバーが受理した日時
    pub posted_at: String,
    /// クライアントが送信した日時
    pub messaged_at: String,
    pub user: OpenrecHistoryUser,
    pub is_top_supporter: bool,
    /// スタンプ情報（任意）
    pub stamp: Option<serde_json::Value>,
    /// イェル情報（任意）
    pub yell: Option<serde_json::Value>,
    /// システムメッセージ（任意）
    pub system_message: Option<serde_json::Value>,
}

/// `fetch_chat_history` のエラー型
#[derive(Debug)]
pub enum FetchChatHistoryError {
    Http(rquest::Error),
    Parse {
        status: u16,
        body: String,
        error: String,
    },
}

impl std::fmt::Display for FetchChatHistoryError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Http(e) => write!(f, "HTTP error: {e}"),
            Self::Parse {
                status,
                error,
                body,
            } => {
                write!(
                    f,
                    "JSON parse error (status={status}): {error}, body={body}"
                )
            }
        }
    }
}

impl std::error::Error for FetchChatHistoryError {}

/// socket.io テキストフレームのパース結果
#[derive(Debug)]
pub enum SocketIoFrame {
    /// ループ継続、処理するイベントあり
    Events(Vec<OpenrecEvent>),
    /// ループ継続、処理なし
    Continue,
    /// ループ終了（socket.io namespace disconnect）
    Disconnect,
}

// ---------------------------------------------------------------------------
// socket.io メッセージタイプ定数（OpenREC フロントエンドの `Ye` オブジェクトに対応）
// ---------------------------------------------------------------------------

const CHAT_ADD: u32 = 0;
const VIEWER_COUNT: u32 = 1;
const STREAM_END: u32 = 3;
const STREAM_START: u32 = 5;
const ROOM_CONFIG: u32 = 10;
const SYSTEM_MESSAGE_V2: u32 = 11;
const SUBSCRIBED: u32 = 27;
const CHATLIST_MODE: u32 = 34;

/// `GET /api/v5/users/me` で取得した自分のユーザー情報
#[derive(Debug, Clone, Deserialize)]
pub struct OpenrecCurrentUser {
    /// チャンネル ID（例: "kv510k"）
    pub id: String,
    /// 表示名
    pub nickname: String,
    /// 数値ユーザー ID
    pub openrec_user_id: u64,
    /// アイコン画像 URL（90px）
    pub icon_image_url: String,
    /// 大きいアイコン画像 URL（320px）
    pub l_icon_image_url: String,
    pub is_premium: bool,
    pub is_partner: bool,
    pub is_official: bool,
}

/// `/api/v5/users/me` 取得エラー
#[derive(Debug)]
pub enum FetchCurrentUserError {
    Http(rquest::Error),
    Parse {
        status: u16,
        body: String,
        error: String,
    },
    NotFound {
        status: u16,
        body: String,
    },
}

impl std::fmt::Display for FetchCurrentUserError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Http(e) => write!(f, "HTTP error: {e}"),
            Self::Parse {
                status,
                error,
                body,
            } => {
                write!(
                    f,
                    "JSON parse error (status={status}): {error}, body={body}"
                )
            }
            Self::NotFound { status, .. } => write!(f, "user not found (status={status})"),
        }
    }
}

impl std::error::Error for FetchCurrentUserError {}

/// `apiv5.openrec.tv` で自分のユーザー情報を取得する
///
/// - `access_token`: ブラウザの `access-token` ヘッダー値
/// - `uuid`: ブラウザの `uuid` ヘッダー値（クライアント識別子）
pub async fn fetch_current_user(
    access_token: &str,
    uuid: &str,
) -> Result<OpenrecCurrentUser, FetchCurrentUserError> {
    let client = rquest::Client::builder()
        .impersonate(rquest::Impersonate::Chrome131)
        .build()
        .map_err(FetchCurrentUserError::Http)?;

    let response = client
        .get("https://apiv5.openrec.tv/api/v5/users/me?include_hidden_channels=true")
        .header("access-token", access_token)
        .header("uuid", uuid)
        .header("origin", "https://www.openrec.tv")
        .header("referer", "https://www.openrec.tv/")
        .header("x-lang", "ja")
        .header("content-type", "application/json")
        .send()
        .await
        .map_err(FetchCurrentUserError::Http)?;

    let status = response.status().as_u16();
    let body = response.text().await.map_err(FetchCurrentUserError::Http)?;

    let root: Value = serde_json::from_str(&body).map_err(|e| FetchCurrentUserError::Parse {
        status,
        body: body.chars().take(500).collect(),
        error: e.to_string(),
    })?;

    let item = root
        .pointer("/data/items/0")
        .ok_or_else(|| FetchCurrentUserError::NotFound {
            status,
            body: body.chars().take(500).collect(),
        })?;

    serde_json::from_value::<OpenrecCurrentUser>(item.clone()).map_err(|e| {
        FetchCurrentUserError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: e.to_string(),
        }
    })
}

// ---------------------------------------------------------------------------
// HTTP API
// ---------------------------------------------------------------------------

/// 過去チャットを取得する
///
/// `public.openrec.tv/external/api/v5/movies/{movie_id}/chats` を呼び出す。
/// 認証不要（公開 API）。
///
/// - `movie_id`: 配信 ID（例: `"lv81x2p27z9"`）
/// - `cursor`: ページングカーソル
/// - `include_system_messages`: システムメッセージを含むか
pub async fn fetch_chat_history(
    movie_id: &str,
    cursor: &ChatHistoryCursor,
    include_system_messages: bool,
) -> Result<Vec<OpenrecHistoryChat>, FetchChatHistoryError> {
    let base = format!("https://public.openrec.tv/external/api/v5/movies/{movie_id}/chats");

    let mut params: Vec<(&str, String)> = Vec::new();
    match cursor {
        ChatHistoryCursor::BeforeTime { to_created_at, c } => {
            params.push(("to_created_at", to_created_at.clone()));
            if let Some(c) = c {
                params.push(("c", c.to_string()));
            }
        }
        ChatHistoryCursor::BeforeId { to_chat_id } => {
            params.push(("to_chat_id", to_chat_id.to_string()));
        }
    }
    params.push((
        "is_including_system_message",
        include_system_messages.to_string(),
    ));

    let client = rquest::Client::builder()
        .impersonate(rquest::Impersonate::Chrome131)
        .build()
        .map_err(FetchChatHistoryError::Http)?;

    let response = client
        .get(&base)
        .query(&params)
        .header("origin", "https://www.openrec.tv")
        .header("referer", "https://www.openrec.tv/")
        .send()
        .await
        .map_err(FetchChatHistoryError::Http)?;

    let status = response.status().as_u16();
    let body = response.text().await.map_err(FetchChatHistoryError::Http)?;

    // まず汎用 JSON としてパース
    let root: serde_json::Value =
        serde_json::from_str(&body).map_err(|e| FetchChatHistoryError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: e.to_string(),
        })?;

    // API エラーレスポンス（{"message":"...", "status": 非ゼロ}）の場合は空リストを返す
    // ライブ配信中は movie_id に対応するビデオレコードが存在しないことがある
    if root.is_object() {
        let api_status = root["status"].as_i64().unwrap_or(-1);
        let api_message = root["message"].as_str().unwrap_or("unknown");
        tracing::debug!(
            target: "mcv::openrec-lib",
            url = %base,
            %movie_id,
            api_status,
            api_message,
            "チャット履歴 API: 動画が見つからないため履歴をスキップ"
        );
        return Ok(vec![]);
    }

    serde_json::from_value::<Vec<OpenrecHistoryChat>>(root).map_err(|e| {
        FetchChatHistoryError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: e.to_string(),
        }
    })
}

/// 配信ページ HTML を取得して接続に必要なメタデータを抽出する
///
/// ページには `"movieId":3286144` のような形式でIDが埋め込まれている。
///
/// 返値: `StreamInfo` または `None`（配信中でない・取得失敗時）
pub async fn fetch_stream_info(page_url: &str) -> Option<StreamInfo> {
    let client = rquest::Client::builder()
        .impersonate(rquest::Impersonate::Chrome131)
        .build()
        .ok()?;

    let resp = timeout(Duration::from_secs(15), client.get(page_url).send())
        .await
        .ok()?
        .ok()?;

    let html = resp.text().await.ok()?;

    let re = Regex::new(r#""movieId":(\d+)"#).ok()?;

    // タイトル: "movieId":DIGITS,"title":"..." の形式で movieId の直後に存在する
    let title_re = Regex::new(r#""movieId":\d+,"title":"([^"]+)""#).ok()?;
    let title = title_re
        .captures(&html)
        .and_then(|cap| cap.get(1))
        .map(|m| m.as_str().to_string());

    // 配信開始時刻: 空でない ISO 8601 日時文字列（例: "2026-03-09T16:46:15+09:00"）
    let created_re = Regex::new(r#""createdAt":"(\d{4}-[^"]+)""#).ok()?;
    let start_time = created_re
        .captures(&html)
        .and_then(|cap| cap.get(1))
        .and_then(|m| DateTime::parse_from_rfc3339(m.as_str()).ok())
        .map(|dt| dt.timestamp());

    // 配信中フラグ: "isLive":true/false
    let is_live_re = Regex::new(r#""isLive":(true|false)"#).ok()?;
    let is_live = is_live_re
        .captures(&html)
        .and_then(|cap| cap.get(1))
        .map(|m| m.as_str() == "true")
        .unwrap_or(false);

    // movie_slug: "id":"<slug>","movieId":\d+ のパターンで取得する
    // JS の _updateMovie が `this.id = e.id` → `this.movieId = e.movieId` の順に設定するため
    // HTML の JSON でも "id":"..." と "movieId":\d+ が隣接して現れる
    let slug_re = Regex::new(r#""id":"([a-z0-9]{6,15})","movieId":\d+"#).ok()?;
    let movie_slug = slug_re
        .captures(&html)
        .and_then(|cap| cap.get(1))
        .map(|m| m.as_str().to_string());

    tracing::debug!(
        target: "mcv::openrec-lib",
        url = page_url,
        ?title,
        ?start_time,
        ?movie_slug,
        is_live,
        "配信メタデータ抽出完了"
    );

    // まず "status":"OPENED" 付近の movieId を優先（現在ライブ中の配信）
    if let Some(opened_pos) = html.find("\"status\":\"OPENED\"") {
        let window_start = opened_pos.saturating_sub(2000);
        let window_end = (opened_pos + 500).min(html.len());
        let window = &html[window_start..window_end];
        for cap in re.captures_iter(window) {
            if let Some(m) = cap.get(1) {
                if let Ok(id) = m.as_str().parse::<u64>() {
                    if id > 0 {
                        tracing::info!(
                            target: "mcv::openrec-lib",
                            url = page_url,
                            movie_id = id,
                            ?movie_slug,
                            "OPENED ステータス付近から movieId を取得"
                        );
                        return Some(StreamInfo {
                            movie_id: id,
                            movie_slug,
                            title,
                            start_time,
                            is_live,
                        });
                    }
                }
            }
        }
        tracing::debug!(
            target: "mcv::openrec-lib",
            "\"status\":\"OPENED\" はあるが付近に movieId がない"
        );
    } else {
        tracing::debug!(
            target: "mcv::openrec-lib",
            "HTML に \"status\":\"OPENED\" が見つからない（配信中でない可能性）"
        );
    }

    // フォールバック: 最初の非ゼロ movieId
    for cap in re.captures_iter(&html) {
        if let Some(m) = cap.get(1) {
            if let Ok(id) = m.as_str().parse::<u64>() {
                if id > 0 {
                    tracing::info!(
                        target: "mcv::openrec-lib",
                        url = page_url,
                        movie_id = id,
                        "フォールバック: 最初の movieId を使用"
                    );
                    return Some(StreamInfo {
                        movie_id: id,
                        movie_slug,
                        title,
                        start_time,
                        is_live,
                    });
                }
            }
        }
    }

    tracing::debug!(
        target: "mcv::openrec-lib",
        url = page_url,
        "HTML 中に有効な movieId が見つからなかった"
    );
    None
}

/// URL から OPENREC チャンネル ID を抽出する
///
/// 例: `https://www.openrec.tv/live/jpml0306` → `"jpml0306"`
pub fn extract_channel_id(url: &str) -> Option<String> {
    let trimmed = url.trim().trim_end_matches('/');
    let path = trimmed.strip_prefix("https://www.openrec.tv/live/")?;
    let channel_id = path.split(['/', '?', '#']).next().unwrap_or("").trim();
    if channel_id.is_empty() {
        return None;
    }
    Some(channel_id.to_string())
}

// ---------------------------------------------------------------------------
// socket.io パーサー
// ---------------------------------------------------------------------------

/// WebSocket テキストフレームを解析する
///
/// OPENREC の実際の動作（ブラウザ観測）:
/// - `0{...}` : EIO OPEN → 無視
/// - `40`     : socket.io namespace connect ack → 無視
/// - `3`      : PONG → 無視
/// - `41`     : socket.io namespace disconnect → `Disconnect` を返す
/// - `42[...]`: socket.io event → イベント名が "message" のものを処理
pub fn parse_socketio_text(text: &str) -> SocketIoFrame {
    let first = text.chars().next().unwrap_or_default();
    match first {
        '0' => SocketIoFrame::Continue, // EIO OPEN
        '3' => SocketIoFrame::Continue, // PONG
        '4' => {
            if text.starts_with("42") {
                parse_socketio_event(text)
            } else if text.starts_with("41") {
                SocketIoFrame::Disconnect
            } else {
                // "40" = サーバー側の namespace connect ack → 無視
                SocketIoFrame::Continue
            }
        }
        _ => SocketIoFrame::Continue,
    }
}

fn parse_socketio_event(text: &str) -> SocketIoFrame {
    // "42" の後ろが JSON 配列: ["event_name", payload]
    let arr_str = &text[2..];
    let arr: Value = match serde_json::from_str(arr_str) {
        Ok(v) => v,
        Err(e) => {
            tracing::warn!(
                target: "mcv::openrec-lib",
                error = %e,
                raw = text,
                "socket.io イベント配列のパース失敗"
            );
            return SocketIoFrame::Continue;
        }
    };

    let arr = match arr.as_array() {
        Some(a) if !a.is_empty() => a,
        _ => return SocketIoFrame::Continue,
    };

    let event_name = arr[0].as_str().unwrap_or("");
    if event_name != "message" {
        tracing::trace!(target: "mcv::openrec-lib", event = event_name, "未処理イベント");
        return SocketIoFrame::Continue;
    }

    // payload はサーバーが JSON 文字列として emit している
    // → JSON.parse(e) が必要な文字列 or 既にオブジェクト
    let msg: Value = match arr.get(1) {
        Some(Value::String(s)) => match serde_json::from_str(s) {
            Ok(v) => v,
            Err(e) => {
                tracing::warn!(
                    target: "mcv::openrec-lib",
                    error = %e,
                    payload = s,
                    "socket message（文字列）のパース失敗"
                );
                return SocketIoFrame::Continue;
            }
        },
        Some(v @ Value::Object(_)) => v.clone(),
        _ => return SocketIoFrame::Continue,
    };

    let msg_type = match msg["type"].as_u64() {
        Some(t) => t as u32,
        None => return SocketIoFrame::Continue,
    };

    let data = &msg["data"];

    match msg_type {
        CHAT_ADD => {
            if let Some(chat_data) = parse_chat_data(data) {
                SocketIoFrame::Events(vec![OpenrecEvent::Chat(chat_data)])
            } else {
                SocketIoFrame::Continue
            }
        }
        VIEWER_COUNT => {
            let viewers = data["viewers"].as_u64().unwrap_or(0);
            let live_viewers = data["live_viewers"].as_u64().unwrap_or(0);
            SocketIoFrame::Events(vec![OpenrecEvent::ViewerCount {
                viewers,
                live_viewers,
            }])
        }
        STREAM_START => SocketIoFrame::Events(vec![OpenrecEvent::StreamStart]),
        STREAM_END => SocketIoFrame::Events(vec![OpenrecEvent::StreamEnd]),
        SYSTEM_MESSAGE_V2 => {
            let message = data["message"].as_str().unwrap_or("").to_string();
            if message.is_empty() {
                SocketIoFrame::Continue
            } else {
                let chat_id = data["chat_id"].as_u64();
                SocketIoFrame::Events(vec![OpenrecEvent::SystemMessage { chat_id, message }])
            }
        }
        ROOM_CONFIG => SocketIoFrame::Continue,
        SUBSCRIBED => SocketIoFrame::Events(vec![OpenrecEvent::Subscribed]),
        CHATLIST_MODE => SocketIoFrame::Events(vec![OpenrecEvent::ChatlistMode]),
        other => {
            tracing::error!(
                target: "mcv::openrec-lib",
                msg_type = other,
                data = %data,
                "未処理メッセージタイプ"
            );
            SocketIoFrame::Continue
        }
    }
}

// ---------------------------------------------------------------------------
// CHAT_ADD データ解析
// ---------------------------------------------------------------------------

/// socket.io CHAT_ADD データを `OpenrecChatData` に変換する
///
/// フィールド（`_toChatFromSocketChat` に対応）:
/// - `chat_id`      : メッセージ ID
/// - `message`      : テキスト本文
/// - `cre_dt`       : 投稿日時（ISO 8601 または Unix ms）
/// - `user_key`     : ユーザー ID 文字列
/// - `user_name`    : 表示名
/// - `user_icon`    : アイコン URL
/// - `user_type`    : "1" = 公式スタッフ
/// - `is_moderator` : 0/1
/// - `is_premium`   : 有料会員
/// - `stamp`        : スタンプ（任意）
/// - `yell`         : イェル・マネタイズイベント（任意）
fn parse_chat_data(data: &Value) -> Option<OpenrecChatData> {
    let chat_id = value_to_string_id(data.get("chat_id")?);
    if chat_id.is_empty() || chat_id == "0" {
        return None;
    }

    let message = data["message"].as_str().unwrap_or("").to_string();
    let user_key = data["user_key"].as_str().unwrap_or("").to_string();
    let user_name = data["user_name"].as_str().unwrap_or("").to_string();
    let user_icon = data["user_icon"]
        .as_str()
        .filter(|s| !s.is_empty())
        .map(str::to_string);
    let timestamp = parse_cre_dt(&data["cre_dt"]);

    let is_official = match &data["user_type"] {
        Value::String(s) => s == "1",
        Value::Number(n) => n.as_u64() == Some(1),
        _ => false,
    };
    let is_moderator =
        data["is_moderator"].as_u64() == Some(1) || data["is_moderator"].as_bool() == Some(true);
    let is_premium =
        data["is_premium"].as_bool() == Some(true) || data["is_premium"].as_u64() == Some(1);

    let stamp_url = data
        .get("stamp")
        .filter(|v| v.is_object())
        .and_then(|stamp| stamp["image_url"].as_str())
        .filter(|s| !s.is_empty())
        .map(str::to_string);

    let yell = data
        .get("yell")
        .filter(|v| v.is_object())
        .map(|yell| OpenrecYell {
            points: yell["points"].as_i64().unwrap_or(0),
            label: yell["label"].as_str().map(str::to_string),
            image_url: yell["image_url"]
                .as_str()
                .filter(|s| !s.is_empty())
                .map(str::to_string),
        });

    Some(OpenrecChatData {
        chat_id,
        message,
        timestamp,
        user_key,
        user_name,
        user_icon,
        is_official,
        is_moderator,
        is_premium,
        stamp_url,
        yell,
    })
}

// ---------------------------------------------------------------------------
// ヘルパー関数（非公開）
// ---------------------------------------------------------------------------

/// `serde_json::Value` を文字列 ID に変換する（数値・文字列を正規化）
fn value_to_string_id(v: &Value) -> String {
    match v {
        Value::Number(n) => n.to_string(),
        Value::String(s) => s.clone(),
        _ => String::new(),
    }
}

/// `cre_dt` フィールドを Unix タイムスタンプ（秒）に変換する
///
/// - 文字列: RFC 3339 としてパース
/// - 数値  : ミリ秒なら秒に変換、秒ならそのまま使用
fn parse_cre_dt(v: &Value) -> i64 {
    match v {
        Value::String(s) => DateTime::parse_from_rfc3339(s)
            .map(|dt| dt.timestamp())
            .unwrap_or_else(|_| chrono::Utc::now().timestamp()),
        Value::Number(n) => {
            let val = n.as_i64().unwrap_or(0);
            if val > 1_000_000_000_000 {
                val / 1000
            } else {
                val
            }
        }
        _ => chrono::Utc::now().timestamp(),
    }
}
