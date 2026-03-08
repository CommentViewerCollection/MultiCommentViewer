//! TwitCasting フロントエンド API クライアント

use reqwest::multipart::Form;
use serde::Deserialize;

use crate::auth::generate_authorize_key;
use crate::html::{extract_cs_session_id, get_tc_variable};

const TWICAS_ORIGIN: &str = "https://twitcasting.tv";
const FRONTEND_API: &str = "https://frontendapi.twitcasting.tv";
const SECRET: &str = "16nkbjlus302qi23";

fn user_agent() -> &'static str {
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36"
}

// ---------------------------------------------------------------------------
// TwicasSession
// ---------------------------------------------------------------------------

/// ライブページHTML取得時に得られる認証情報
pub struct TwicasSession {
    pub session_id: String,
}

impl TwicasSession {
    /// ライブページHTMLからセッションIDを取得する
    pub async fn from_user(
        client: &reqwest::Client,
        user_name: &str,
    ) -> Result<Self, reqwest::Error> {
        let live_page_url = format!("{}/{}", TWICAS_ORIGIN, user_name);
        let html = client
            .get(&live_page_url)
            .header(reqwest::header::USER_AGENT, user_agent())
            .send()
            .await?
            .text()
            .await?;

        let session_id = get_tc_variable(&html, "web-authorize-session-id").unwrap_or_default();

        Ok(Self { session_id })
    }

    fn auth_headers(&self, method: &str, url: &str) -> [(&'static str, String); 2] {
        let key = generate_authorize_key(method, url, &self.session_id, "", SECRET);
        [
            ("x-web-authorizekey", key),
            ("x-web-sessionid", self.session_id.clone()),
        ]
    }
}

// ---------------------------------------------------------------------------
// latest-movie
// ---------------------------------------------------------------------------

/// `GET /users/{user}/latest-movie` レスポンス
#[derive(Debug, Deserialize)]
pub struct LatestMovieResponse {
    pub movie: Option<MovieInfo>,
}

#[derive(Debug, Deserialize)]
pub struct MovieInfo {
    pub id: i64,
    pub is_on_live: bool,
}

/// ユーザーの最新ライブ情報を取得する
///
/// * `pass` - プライベート配信のパスワード（公開配信なら `None`）
pub async fn fetch_latest_movie(
    client: &reqwest::Client,
    session: &TwicasSession,
    user_name: &str,
    pass: Option<&str>,
) -> Result<LatestMovieResponse, reqwest::Error> {
    let now_ms = chrono::Utc::now().timestamp_millis();
    let url = match pass {
        Some(p) => format!(
            "{}/users/{}/latest-movie?pass={}&__n={}",
            FRONTEND_API, user_name, p, now_ms
        ),
        None => format!(
            "{}/users/{}/latest-movie?__n={}",
            FRONTEND_API, user_name, now_ms
        ),
    };

    let [auth_key_header, session_id_header] = session.auth_headers("GET", &url);

    client
        .get(&url)
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .header(auth_key_header.0, auth_key_header.1)
        .header(session_id_header.0, session_id_header.1)
        .send()
        .await?
        .error_for_status()?
        .json()
        .await
}

// ---------------------------------------------------------------------------
// event pubsub URL
// ---------------------------------------------------------------------------

#[derive(Deserialize)]
struct EventPubSubUrlResponse {
    url: String,
}

/// WebSocket接続用イベントpubsub URLを取得する
pub async fn fetch_event_pubsub_url(
    client: &reqwest::Client,
    movie_id: i64,
    pass: Option<&str>,
) -> Result<String, reqwest::Error> {
    let now_ms = chrono::Utc::now().timestamp_millis();
    let form = Form::new()
        .text("movie_id", movie_id.to_string())
        .text("__n", now_ms.to_string())
        .text("password", pass.unwrap_or("").to_string());

    let resp: EventPubSubUrlResponse = client
        .post(format!("{}/eventpubsuburl.php", TWICAS_ORIGIN))
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .multipart(form)
        .send()
        .await?
        .error_for_status()?
        .json()
        .await?;

    Ok(resp.url)
}

// ---------------------------------------------------------------------------
// movie token (POST /movies/{id}/token)
// ---------------------------------------------------------------------------

/// `POST /movies/{id}/token` レスポンス
#[derive(Debug, Deserialize)]
pub struct MovieTokenResponse {
    pub token: String,
}

/// プライベート配信用トークンを取得する
///
/// * `pass` - 配信パスワード（空文字でも可）
pub async fn fetch_movie_token(
    client: &reqwest::Client,
    movie_id: i64,
    pass: &str,
) -> Result<MovieTokenResponse, reqwest::Error> {
    let url = format!("{}/movies/{}/token", FRONTEND_API, movie_id);
    let form = Form::new().text("password", pass.to_string());

    client
        .post(&url)
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .multipart(form)
        .send()
        .await?
        .error_for_status()?
        .json()
        .await
}

// ---------------------------------------------------------------------------
// movie info (GET /movies/{id}/info)
// ---------------------------------------------------------------------------

/// `GET /movies/{id}/info` レスポンス
#[derive(Debug, Deserialize)]
pub struct MovieInfoResponse {
    pub broadcaster: BroadcasterInfo,
    pub requested_at: i64,
    pub id: i64,
    pub started_at: i64,
    pub visibility: VisibilityInfo,
    pub has_secrecy_of_communication: Option<bool>,
}

#[derive(Debug, Deserialize)]
pub struct BroadcasterInfo {
    pub id: String,
    pub social_id: String,
}

#[derive(Debug, Deserialize)]
pub struct VisibilityInfo {
    #[serde(rename = "type")]
    pub visibility_type: String,
}

/// ライブ情報を取得する
///
/// * `token` - `fetch_movie_token` で取得したトークン（公開配信なら `None`）
pub async fn fetch_movie_info(
    client: &reqwest::Client,
    movie_id: i64,
    token: Option<&str>,
) -> Result<MovieInfoResponse, reqwest::Error> {
    let now_ms = chrono::Utc::now().timestamp_millis();
    let url = match token {
        Some(t) => format!(
            "{}/movies/{}/info?token={}&__n={}",
            FRONTEND_API,
            movie_id,
            urlencoding::encode(t),
            now_ms
        ),
        None => format!("{}/movies/{}/info?__n={}", FRONTEND_API, movie_id, now_ms),
    };

    client
        .get(&url)
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .send()
        .await?
        .error_for_status()?
        .json()
        .await
}

// ---------------------------------------------------------------------------
// viewer status (GET /movies/{id}/status/viewer)
// ---------------------------------------------------------------------------

/// `GET /movies/{id}/status/viewer` レスポンス
#[derive(Debug, Deserialize)]
pub struct ViewerStatusResponse {
    pub update_interval_sec: u32,
    pub movie: ViewerMovieInfo,
}

#[derive(Debug, Deserialize)]
pub struct ViewerMovieInfo {
    pub id: i64,
    pub title: String,
    pub viewers: ViewerCount,
    pub comment_count: u64,
}

#[derive(Debug, Deserialize)]
pub struct ViewerCount {
    pub current: u64,
    pub total: u64,
}

/// 視聴者ステータスを取得する
///
/// * `token` - `fetch_movie_token` で取得したトークン（公開配信なら `None`）
/// * `hl`    - 言語コード（例: "ja"）
pub async fn fetch_viewer_status(
    client: &reqwest::Client,
    movie_id: i64,
    token: Option<&str>,
    hl: &str,
) -> Result<ViewerStatusResponse, reqwest::Error> {
    let now_ms = chrono::Utc::now().timestamp_millis();
    let url = match token {
        Some(t) => format!(
            "{}/movies/{}/status/viewer?token={}&hl={}&__n={}",
            FRONTEND_API,
            movie_id,
            urlencoding::encode(t),
            hl,
            now_ms
        ),
        None => format!(
            "{}/movies/{}/status/viewer?hl={}&__n={}",
            FRONTEND_API, movie_id, hl, now_ms
        ),
    };

    client
        .get(&url)
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .send()
        .await?
        .error_for_status()?
        .json()
        .await
}

// ---------------------------------------------------------------------------
// item box (GET /item_box/{user})
// ---------------------------------------------------------------------------

/// アイテム情報
#[derive(Debug, Deserialize)]
pub struct Item {
    pub item_id: String,
    pub name: String,
    pub explain: String,
    pub image_url: String,
    pub mp: u64,
    pub point: u64,
    pub expire: u64,
    pub count: u64,
    pub gem: u64,
}

/// コンティニュー情報
#[derive(Debug, Deserialize)]
pub struct ContinueInfo {
    pub current_coin: u64,
    pub min_coins_to_continue: u64,
    pub max_streaming_time_in_minutes: u64,
}

/// ステータス情報
#[derive(Debug, Deserialize)]
pub struct ItemBoxStatus {
    pub mp: u64,
    pub star: u64,
}

/// `GET /item_box/{user}` レスポンス
#[derive(Debug, Deserialize)]
pub struct ItemBoxResponse {
    pub items: Vec<Item>,
    #[serde(rename = "continue")]
    pub continue_info: ContinueInfo,
    pub status: ItemBoxStatus,
}

/// アイテムボックス（投げ銭アイテム一覧）を取得する
pub async fn fetch_item_box(
    client: &reqwest::Client,
    user_name: &str,
) -> Result<ItemBoxResponse, reqwest::Error> {
    let url = format!("{}/item_box/{}", FRONTEND_API, user_name);

    client
        .get(&url)
        .header(reqwest::header::ACCEPT, "*/*")
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent())
        .send()
        .await?
        .error_for_status()?
        .json()
        .await
}

// ---------------------------------------------------------------------------
// 合言葉 → wpass クッキー
// ---------------------------------------------------------------------------

/// 合言葉認証エラーの種別
#[derive(Debug, thiserror::Error)]
pub enum WpassError {
    #[error("HTTP error: {0}")]
    Http(#[from] reqwest::Error),
    #[error("cs_session_id が見つかりません（合言葉不要な配信か、すでに認証済みの可能性があります）")]
    CsSessionIdNotFound,
    #[error("Set-Cookie に wpass が含まれていません（合言葉が間違っている可能性があります）")]
    WpassNotFound,
}

/// 合言葉（password）を `wpass` クッキー値に変換する
///
/// # 処理の流れ
/// 1. `GET https://twitcasting.tv/{user_name}` でHTMLを取得し `cs_session_id` を抽出
/// 2. `POST https://twitcasting.tv/{user_name}` に合言葉を送信
/// 3. レスポンスの `Set-Cookie: wpass=...` の値を返す
///
/// # 戻り値
/// `wpass` クッキーの値（例: `"c1ebb4933e06ce5617483f665e26627c"`）
pub async fn resolve_wpass(
    client: &reqwest::Client,
    user_name: &str,
    password: &str,
) -> Result<String, WpassError> {
    // 1. ライブページHTMLを取得して cs_session_id を抽出
    let live_page_url = format!("{}/{}", TWICAS_ORIGIN, user_name);
    let html = client
        .get(&live_page_url)
        .header(reqwest::header::USER_AGENT, user_agent())
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .send()
        .await?
        .text()
        .await?;

    let cs_session_id =
        extract_cs_session_id(&html).ok_or(WpassError::CsSessionIdNotFound)?;

    // 2. 合言葉を POST して Set-Cookie を受け取る
    let body = format!(
        "password={}&cs_session_id={}",
        urlencoding::encode(password),
        urlencoding::encode(&cs_session_id)
    );

    let response = client
        .post(&live_page_url)
        .header(reqwest::header::CONTENT_TYPE, "application/x-www-form-urlencoded")
        .header(reqwest::header::USER_AGENT, user_agent())
        .header(reqwest::header::ACCEPT_LANGUAGE, "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7")
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", live_page_url))
        .body(body)
        .send()
        .await?;

    // 3. Set-Cookie から wpass を取り出す
    //    reqwest はリダイレクト時に Set-Cookie を保持しないため、
    //    ヘッダーを直接解析する
    for value in response.headers().get_all(reqwest::header::SET_COOKIE) {
        let cookie_str = match value.to_str() {
            Ok(s) => s,
            Err(_) => continue,
        };
        // "wpass=<value>; path=..." の形式
        if let Some(rest) = cookie_str.strip_prefix("wpass=") {
            let wpass = rest.split(';').next().unwrap_or(rest).trim().to_string();
            return Ok(wpass);
        }
    }

    Err(WpassError::WpassNotFound)
}
