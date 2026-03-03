//! Kick.com API クライアント

use rquest::Impersonate;
use serde::Deserialize;
use serde_json::Value;

/// `GET https://kick.com/api/v2/channels/{channelSlug}` のレスポンス
#[derive(Debug, Deserialize)]
#[allow(dead_code)]
pub struct KickChannelResponse {
    pub id: u64,
    pub slug: String,
    pub chatroom: KickChatroom,
    pub livestream: Option<KickLivestream>,
    pub user_id: Option<u64>,
    pub followers_count: Option<u64>,
}

#[derive(Debug, Deserialize)]
pub struct KickChatroom {
    pub id: u64,
}

/// livestream フィールドが存在する（null でない）場合は配信中とみなす。
/// is_live フィールドは存在しない場合を考慮してオプションにしてある。
#[derive(Debug, Deserialize)]
#[allow(dead_code)]
pub struct KickLivestream {
    pub id: u64,
    pub is_live: Option<bool>,
    pub session_title: Option<String>,
    pub viewers: Option<u64>,
}

impl KickLivestream {
    /// livestream オブジェクトが存在するなら配信中とみなす。
    /// is_live フィールドがある場合はその値も確認する。
    pub fn is_live(&self) -> bool {
        self.is_live.unwrap_or(true)
    }
}

#[derive(Debug)]
pub enum FetchChannelError {
    Http(rquest::Error),
    Parse { status: u16, body: String, error: String },
}

impl std::fmt::Display for FetchChannelError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Http(e) => write!(f, "HTTP error: {e}"),
            Self::Parse { status, error, .. } => {
                write!(f, "JSON parse error (status={status}): {error}")
            }
        }
    }
}

impl std::error::Error for FetchChannelError {}

#[derive(Debug)]
pub enum FetchChatHistoryError {
    Http(rquest::Error),
    Parse { status: u16, body: String, error: String },
}

impl std::fmt::Display for FetchChatHistoryError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Http(e) => write!(f, "HTTP error: {e}"),
            Self::Parse { status, error, .. } => {
                write!(f, "JSON parse error (status={status}): {error}")
            }
        }
    }
}

impl std::error::Error for FetchChatHistoryError {}

#[allow(dead_code)]
#[derive(Debug)]
pub enum FetchEmotesError {
    Http(rquest::Error),
    Parse { status: u16, body: String, error: String },
}

impl std::fmt::Display for FetchEmotesError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Http(e) => write!(f, "HTTP error: {e}"),
            Self::Parse { status, error, .. } => {
                write!(f, "JSON parse error (status={status}): {error}")
            }
        }
    }
}

impl std::error::Error for FetchEmotesError {}

#[derive(Debug, Clone, Deserialize)]
pub struct KickHistoryMessage {
    #[serde(deserialize_with = "deserialize_string")]
    pub id: String,
    pub content: String,
    pub created_at: String,
    pub sender: KickHistorySender,
}

#[derive(Debug, Clone, Deserialize)]
pub struct KickHistorySender {
    #[serde(deserialize_with = "deserialize_u64")]
    pub id: u64,
    pub username: String,
    #[allow(dead_code)]
    pub slug: Option<String>,
}

fn deserialize_string<'de, D>(deserializer: D) -> Result<String, D::Error>
where
    D: serde::Deserializer<'de>,
{
    let v = Value::deserialize(deserializer)?;
    match v {
        Value::String(s) => Ok(s),
        Value::Number(n) => Ok(n.to_string()),
        _ => Err(serde::de::Error::custom("expected string or number")),
    }
}

fn deserialize_u64<'de, D>(deserializer: D) -> Result<u64, D::Error>
where
    D: serde::Deserializer<'de>,
{
    let v = Value::deserialize(deserializer)?;
    match v {
        Value::Number(n) => n
            .as_u64()
            .ok_or_else(|| serde::de::Error::custom("number is not u64")),
        Value::String(s) => s
            .parse::<u64>()
            .map_err(|_| serde::de::Error::custom("string is not u64")),
        _ => Err(serde::de::Error::custom("expected number or string")),
    }
}

fn build_client() -> Result<rquest::Client, rquest::Error> {
    rquest::Client::builder()
        .impersonate(Impersonate::Chrome131)
        .build()
}

fn apply_cookie_header(
    req: rquest::RequestBuilder,
    cookie_header: &str,
) -> rquest::RequestBuilder {
    if cookie_header.is_empty() {
        req
    } else {
        req.header("Cookie", cookie_header)
    }
}

fn extract_history_array(root: &Value) -> Option<&Vec<Value>> {
    root.as_array()
        .or_else(|| root.get("messages").and_then(Value::as_array))
        .or_else(|| root.pointer("/data/messages").and_then(Value::as_array))
        .or_else(|| root.get("data").and_then(Value::as_array))
}

/// `GET https://kick.com/api/v1/user` のレスポンス（ログイン中のユーザー情報）
#[derive(Debug, Deserialize)]
pub struct KickCurrentUserResponse {
    pub id: u64,
    pub username: String,
    pub profile_pic: Option<String>,
}

/// 現在ログイン中の Kick ユーザー情報を取得する
///
/// `cookie_header` にブラウザ Cookie を渡すことでログイン状態で取得できる。
/// 未ログイン・取得失敗時は `Err` を返す。
pub async fn fetch_current_user(
    cookie_header: &str,
) -> Result<KickCurrentUserResponse, FetchChannelError> {
    let client = build_client().map_err(FetchChannelError::Http)?;

    let req = client
        .get("https://kick.com/api/v1/user")
        .header("Accept-Language", "ja")
        .header("Priority", "u=0, i");
    let req = apply_cookie_header(req, cookie_header);

    let response = req.send().await.map_err(FetchChannelError::Http)?;
    let status = response.status().as_u16();
    let body = response.text().await.map_err(FetchChannelError::Http)?;

    serde_json::from_str::<KickCurrentUserResponse>(&body).map_err(|e| {
        FetchChannelError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: e.to_string(),
        }
    })
}

/// Kick チャンネル情報を取得する
///
/// Chrome 131 の TLS/HTTP2 フィンガープリントで接続することで
/// Cloudflare Bot Management を回避する。
/// `cookie_header` にブラウザ Cookie 文字列（`name=value; name2=value2` 形式）を渡すと
/// Cookie ヘッダーも付与する。
pub async fn fetch_channel(
    channel_slug: &str,
    cookie_header: &str,
) -> Result<KickChannelResponse, FetchChannelError> {
    let api_url = format!("https://kick.com/api/v2/channels/{channel_slug}");

    // Chrome 131 の TLS フィンガープリント（JA3/JA4）および
    // HTTP/2 フィンガープリント（SETTINGS フレーム・疑似ヘッダー順序）を再現する。
    // reqwest と異なり、TLS ClientHello が実際の Chrome と同一になるため
    // Cloudflare の Bot Management を通過できる。
    let client = build_client().map_err(FetchChannelError::Http)?;

    let req = client
        .get(&api_url)
        .header("Accept-Language", "ja")
        .header("Priority", "u=0, i");
    let req = apply_cookie_header(req, cookie_header);

    let response = req.send().await.map_err(FetchChannelError::Http)?;

    let status = response.status().as_u16();
    let body = response.text().await.map_err(FetchChannelError::Http)?;

    serde_json::from_str::<KickChannelResponse>(&body).map_err(|e| FetchChannelError::Parse {
        status,
        body: body.chars().take(500).collect(), // 先頭500文字をログ用に保持
        error: e.to_string(),
    })
}

/// Kick チャット履歴を取得する
///
/// `GET https://web.kick.com/api/v1/chat/{channel_id}/history`
pub async fn fetch_chat_history(
    channel_id: u64,
    cookie_header: &str,
) -> Result<Vec<KickHistoryMessage>, FetchChatHistoryError> {
    let api_url = format!("https://web.kick.com/api/v1/chat/{channel_id}/history");
    let client = build_client().map_err(FetchChatHistoryError::Http)?;

    let req = client
        .get(&api_url)
        .header("Accept-Language", "ja")
        .header("Priority", "u=0, i");
    let req = apply_cookie_header(req, cookie_header);

    let response = req.send().await.map_err(FetchChatHistoryError::Http)?;
    let status = response.status().as_u16();
    let body = response
        .text()
        .await
        .map_err(FetchChatHistoryError::Http)?;

    let root: Value =
        serde_json::from_str(&body).map_err(|e| FetchChatHistoryError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: format!("root parse failed: {e}"),
        })?;

    let raw_items = extract_history_array(&root).ok_or_else(|| FetchChatHistoryError::Parse {
        status,
        body: body.chars().take(500).collect(),
        error: "history array was not found in response".to_string(),
    })?;

    let mut items = Vec::with_capacity(raw_items.len());
    for item in raw_items {
        if let Ok(parsed) = serde_json::from_value::<KickHistoryMessage>(item.clone()) {
            items.push(parsed);
        }
    }

    if raw_items.is_empty() || !items.is_empty() {
        Ok(items)
    } else {
        Err(FetchChatHistoryError::Parse {
            status,
            body: body.chars().take(500).collect(),
            error: "history items parse failed".to_string(),
        })
    }
}

/// Kick チャンネルの emote 情報を取得する
///
/// `GET https://kick.com/emotes/{channel_slug}`
#[allow(dead_code)]
pub async fn fetch_emotes(
    channel_slug: &str,
    cookie_header: &str,
) -> Result<Value, FetchEmotesError> {
    let api_url = format!("https://kick.com/emotes/{channel_slug}");
    let client = build_client().map_err(FetchEmotesError::Http)?;

    let req = client
        .get(&api_url)
        .header("Accept-Language", "ja")
        .header("Priority", "u=0, i");
    let req = apply_cookie_header(req, cookie_header);

    let response = req.send().await.map_err(FetchEmotesError::Http)?;
    let status = response.status().as_u16();
    let body = response.text().await.map_err(FetchEmotesError::Http)?;

    serde_json::from_str::<Value>(&body).map_err(|e| FetchEmotesError::Parse {
        status,
        body: body.chars().take(500).collect(),
        error: e.to_string(),
    })
}
