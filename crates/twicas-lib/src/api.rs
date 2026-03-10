//! TwitCasting フロントエンド API クライアント

use reqwest::cookie::CookieStore as _;
use reqwest::multipart::Form;
use serde::Deserialize;

use crate::auth::generate_authorize_key;
use crate::html::{extract_cs_session_id, extract_movie_id_from_html, get_tc_variable};

const TWICAS_ORIGIN: &str = "https://twitcasting.tv";
const FRONTEND_API: &str = "https://frontendapi.twitcasting.tv";

fn user_agent() -> &'static str {
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36"
}

/// ヘッダーマップを改行区切りの文字列にフォーマットする
fn fmt_headers(headers: &reqwest::header::HeaderMap) -> String {
    headers
        .iter()
        .map(|(name, value)| format!("{}: {}", name, value.to_str().unwrap_or("(invalid utf-8)")))
        .collect::<Vec<_>>()
        .join("\n")
}

/// レスポンスの Set-Cookie ヘッダーをログ出力する
fn log_response_cookies(label: &str, response: &reqwest::Response) {
    let cookies: Vec<String> = response
        .cookies()
        .map(|c| format!("{}={}", c.name(), c.value()))
        .collect();
    if cookies.is_empty() {
        tracing::debug!(target: "mcv::twicas-lib", label, "Set-Cookie: (なし)");
    } else {
        tracing::debug!(target: "mcv::twicas-lib", label, set_cookie = %cookies.join("; "), "Set-Cookie");
    }
}

// ---------------------------------------------------------------------------
// Cookie / RequestBody
// ---------------------------------------------------------------------------

/// HTTP Cookie (name=value)
pub struct Cookie {
    pub name: String,
    pub value: String,
}

/// リクエストボディ
pub enum RequestBody {
    /// multipart/form-data（フィールド名→値のペアリスト）
    Multipart(Vec<(String, String)>),
    /// application/x-www-form-urlencoded（エンコード済み文字列）
    UrlEncoded(String),
    /// ボディなし（GET など）
    None,
}

/// API 呼び出し時のエラー
#[derive(Debug, thiserror::Error)]
pub enum TwicasApiError {
    #[error("HTTP error: {0}")]
    Http(#[from] reqwest::Error),
    #[error("JSON decode error: {source}\nBody: {body}")]
    Json {
        #[source]
        source: serde_json::Error,
        body: String,
    },
}

/// URL から `twitcasting.tv` 以降のパス部分を切り出す
fn extract_path(url: &str) -> &str {
    if let Some(pos) = url.find("twitcasting.tv") {
        &url[pos + "twitcasting.tv".len()..]
    } else {
        "/"
    }
}

/// ツイキャスへのリクエストを送信する共通関数
///
/// 共通ヘッダー（Accept, Accept-Language, Origin, Referer, User-Agent）を自動付与する。
/// `session_id` と `secret` が両方 `Some` の場合、`x-web-authorizekey` / `x-web-sessionid` を付与する。
async fn send_request(
    client: &reqwest::Client,
    method: &str,
    url: &str,
    body: RequestBody,
    cookies: Vec<Cookie>,
    session_id: Option<&str>,
    secret: Option<&str>,
) -> Result<reqwest::Response, reqwest::Error> {
    let (response, _) =
        send_request_inner(client, method, url, body, cookies, session_id, secret).await?;
    Ok(response)
}

/// `send_request` と同じだがリクエストヘッダーも返す（デバッグログ用）
async fn send_request_inner(
    client: &reqwest::Client,
    method: &str,
    url: &str,
    body: RequestBody,
    cookies: Vec<Cookie>,
    session_id: Option<&str>,
    secret: Option<&str>,
) -> Result<(reqwest::Response, reqwest::header::HeaderMap), reqwest::Error> {
    let path = extract_path(url);

    let mut builder = match method {
        "POST" => client.post(url),
        _ => client.get(url),
    };

    builder = builder
        .header(reqwest::header::ACCEPT, "*/*")
        .header(
            reqwest::header::ACCEPT_LANGUAGE,
            "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7",
        )
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", TWICAS_ORIGIN))
        .header(reqwest::header::USER_AGENT, user_agent());

    if let (Some(sid), Some(sec)) = (session_id, secret) {
        let key = generate_authorize_key(method, path, sid, "", sec);
        builder = builder
            .header("x-web-authorizekey", key)
            .header("x-web-sessionid", sid);
    }

    if !cookies.is_empty() {
        let cookie_str = cookies
            .iter()
            .map(|c| format!("{}={}", c.name, c.value))
            .collect::<Vec<_>>()
            .join("; ");
        builder = builder.header(reqwest::header::COOKIE, cookie_str);
    }

    builder = match body {
        RequestBody::Multipart(fields) => {
            let mut form = Form::new();
            for (k, v) in fields {
                form = form.text(k, v);
            }
            builder.multipart(form)
        }
        RequestBody::UrlEncoded(s) => builder
            .header(
                reqwest::header::CONTENT_TYPE,
                "application/x-www-form-urlencoded",
            )
            .body(s),
        RequestBody::None => builder,
    };

    let request = builder.build()?;
    let req_headers = request.headers().clone();
    let response = client.execute(request).await?;
    Ok((response, req_headers))
}

// ---------------------------------------------------------------------------
// fetch_session_id
// ---------------------------------------------------------------------------

/// ライブページHTMLからセッションIDを取得する
///
/// wpass や did 等のクッキーは呼び出し元が `client` の cookie jar に
/// 事前に登録しておくこと（`resolve_wpass` の後、jar に wpass を追加する）。
pub async fn fetch_session_id(
    client: &reqwest::Client,
    user_name: &str,
) -> Result<String, reqwest::Error> {
    let live_page_url = format!("{}/{}", TWICAS_ORIGIN, user_name);
    tracing::debug!(target: "mcv::twicas-lib", "fetch_session_id GET: {}", live_page_url);
    let response = send_request(
        client,
        "GET",
        &live_page_url,
        RequestBody::None,
        vec![],
        None,
        None,
    )
    .await?;
    log_response_cookies("fetch_session_id GET レスポンス", &response);
    let html = response.text().await?;
    let session_id = get_tc_variable(&html, "web-authorize-session-id").unwrap_or_default();
    tracing::debug!(target: "mcv::twicas-lib", session_id = %session_id, "fetch_session_id セッションID取得");
    Ok(session_id)
}

// ---------------------------------------------------------------------------
// fetch_session_ids (session_id + cs_session_id)
// ---------------------------------------------------------------------------

/// ライブページHTMLからセッションID・cs_session_id・movie_id を同時に取得する
///
/// - `session_id`: 非ログインユーザー向け。ログイン済みHTMLには存在せず空文字になる。
/// - `cs_session_id`: コメント投稿用セッションID（パスワードフォームのhiddenフィールド）。
/// - `movie_id_from_html`: `twitter:image` の `twimage/{id}` から抽出した movie_id。
///   ログイン済みで session_id が空のとき、`fetch_latest_movie` の代替として使用する。
pub async fn fetch_session_ids(
    client: &reqwest::Client,
    user_name: &str,
) -> Result<(String, Option<String>, Option<i64>), reqwest::Error> {
    let live_page_url = format!("{}/{}", TWICAS_ORIGIN, user_name);
    tracing::debug!(target: "mcv::twicas-lib", "fetch_session_ids GET: {}", live_page_url);
    let response = send_request(
        client,
        "GET",
        &live_page_url,
        RequestBody::None,
        vec![],
        None,
        None,
    )
    .await?;
    log_response_cookies("fetch_session_ids GET レスポンス", &response);
    let html = response.text().await?;
    let session_id = get_tc_variable(&html, "web-authorize-session-id").unwrap_or_default();
    // cs_session_id はパスワードフォームの hidden input から取得する（非ログインユーザー向け）。
    // ログイン済みユーザーの場合はパスワードフォームが表示されないため、
    // tc-page-variables の csrf_token をフォールバックとして使用する。
    let cs_session_id =
        extract_cs_session_id(&html).or_else(|| get_tc_variable(&html, "csrf_token"));
    let movie_id_from_html = extract_movie_id_from_html(&html);
    tracing::debug!(
        target: "mcv::twicas-lib",
        session_id = %session_id,
        has_cs_session_id = cs_session_id.is_some(),
        movie_id_from_html = ?movie_id_from_html,
        "fetch_session_ids 取得"
    );
    Ok((session_id, cs_session_id, movie_id_from_html))
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
/// * `session_id` - `fetch_session_id` で取得したセッションID
/// * `secret`     - 認証キー生成用シークレット
/// * `pass`       - プライベート配信のパスワード（公開配信なら `None`）
pub async fn fetch_latest_movie(
    client: &reqwest::Client,
    session_id: &str,
    secret: &str,
    user_name: &str,
    pass: Option<&str>,
) -> Result<LatestMovieResponse, TwicasApiError> {
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

    // session_id が空の場合はブラウザcookieで認証済みと判断し、
    // 認証ヘッダー（x-web-authorizekey / x-web-sessionid）を省略する。
    // 空のsession_idで認証ヘッダーを付けると 400 Bad Request になる。
    let (sid_opt, sec_opt) = if session_id.is_empty() {
        (None, None)
    } else {
        (Some(session_id), Some(secret))
    };

    let (response, req_headers) = send_request_inner(
        client,
        "GET",
        &url,
        RequestBody::None,
        vec![],
        sid_opt,
        sec_opt,
    )
    .await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        user_name = %user_name,
        has_session_id = !session_id.is_empty(),
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_latest_movie"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    serde_json::from_str::<LatestMovieResponse>(&body)
        .map_err(|e| TwicasApiError::Json { source: e, body })
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
) -> Result<String, TwicasApiError> {
    let now_ms = chrono::Utc::now().timestamp_millis();
    let fields = vec![
        ("movie_id".to_string(), movie_id.to_string()),
        ("__n".to_string(), now_ms.to_string()),
        ("password".to_string(), pass.unwrap_or("").to_string()),
    ];
    let req_body_log = format!(
        "movie_id={}&password={}",
        movie_id,
        urlencoding::encode(pass.unwrap_or(""))
    );

    let url = format!("{}/eventpubsuburl.php", TWICAS_ORIGIN);
    let (response, req_headers) = send_request_inner(
        client,
        "POST",
        &url,
        RequestBody::Multipart(fields),
        vec![],
        None,
        None,
    )
    .await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        movie_id = movie_id,
        has_pass = pass.is_some(),
        request_body = %req_body_log,
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_event_pubsub_url"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    let resp: EventPubSubUrlResponse =
        serde_json::from_str(&body).map_err(|e| TwicasApiError::Json { source: e, body })?;
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
/// * `pass`       - 配信パスワード（空文字でも可）
/// * `session_id` - `fetch_session_ids` で取得したセッションID（空文字の場合は認証ヘッダー省略）
/// * `secret`     - 認証キー生成用シークレット
pub async fn fetch_movie_token(
    client: &reqwest::Client,
    movie_id: i64,
    pass: &str,
    session_id: &str,
    secret: &str,
) -> Result<MovieTokenResponse, TwicasApiError> {
    let url = format!("{}/movies/{}/token", FRONTEND_API, movie_id);
    let fields = vec![("password".to_string(), pass.to_string())];
    let req_body_log = format!("password={}", urlencoding::encode(pass));

    let (sid_opt, sec_opt) = if session_id.is_empty() {
        (None, None)
    } else {
        (Some(session_id), Some(secret))
    };

    let (response, req_headers) = send_request_inner(
        client,
        "POST",
        &url,
        RequestBody::Multipart(fields),
        vec![],
        sid_opt,
        sec_opt,
    )
    .await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        movie_id = movie_id,
        request_body = %req_body_log,
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_movie_token"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    serde_json::from_str::<MovieTokenResponse>(&body)
        .map_err(|e| TwicasApiError::Json { source: e, body })
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
/// * `token`      - `fetch_movie_token` で取得したトークン（公開配信なら `None`）
/// * `session_id` - `fetch_session_ids` で取得したセッションID（空文字の場合は認証ヘッダー省略）
/// * `secret`     - 認証キー生成用シークレット
pub async fn fetch_movie_info(
    client: &reqwest::Client,
    movie_id: i64,
    token: Option<&str>,
    session_id: &str,
    secret: &str,
) -> Result<MovieInfoResponse, TwicasApiError> {
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

    let (sid_opt, sec_opt) = if session_id.is_empty() {
        (None, None)
    } else {
        (Some(session_id), Some(secret))
    };

    let (response, req_headers) = send_request_inner(
        client,
        "GET",
        &url,
        RequestBody::None,
        vec![],
        sid_opt,
        sec_opt,
    )
    .await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        movie_id = movie_id,
        has_token = token.is_some(),
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_movie_info"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    serde_json::from_str::<MovieInfoResponse>(&body)
        .map_err(|e| TwicasApiError::Json { source: e, body })
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
/// * `token`      - `fetch_movie_token` で取得したトークン（公開配信なら `None`）
/// * `hl`         - 言語コード（例: "ja"）
/// * `session_id` - `fetch_session_ids` で取得したセッションID（空文字の場合は認証ヘッダー省略）
/// * `secret`     - 認証キー生成用シークレット
pub async fn fetch_viewer_status(
    client: &reqwest::Client,
    movie_id: i64,
    token: Option<&str>,
    hl: &str,
    session_id: &str,
    secret: &str,
) -> Result<ViewerStatusResponse, TwicasApiError> {
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

    let (sid_opt, sec_opt) = if session_id.is_empty() {
        (None, None)
    } else {
        (Some(session_id), Some(secret))
    };

    let (response, req_headers) = send_request_inner(
        client,
        "GET",
        &url,
        RequestBody::None,
        vec![],
        sid_opt,
        sec_opt,
    )
    .await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        movie_id = movie_id,
        has_token = token.is_some(),
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_viewer_status"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    serde_json::from_str::<ViewerStatusResponse>(&body)
        .map_err(|e| TwicasApiError::Json { source: e, body })
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
) -> Result<ItemBoxResponse, TwicasApiError> {
    let url = format!("{}/item_box/{}", FRONTEND_API, user_name);

    let (response, req_headers) =
        send_request_inner(client, "GET", &url, RequestBody::None, vec![], None, None).await?;

    let status = response.status();
    let req_headers_str = fmt_headers(&req_headers);
    let resp_headers_str = fmt_headers(response.headers());
    let http_error = response.error_for_status_ref().err();
    let body = response.text().await?;

    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        user_name = %user_name,
        request_headers = %req_headers_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "fetch_item_box"
    );

    if let Some(e) = http_error {
        return Err(TwicasApiError::Http(e));
    }

    serde_json::from_str::<ItemBoxResponse>(&body)
        .map_err(|e| TwicasApiError::Json { source: e, body })
}

// ---------------------------------------------------------------------------
// 合言葉 → wpass クッキー
// ---------------------------------------------------------------------------

/// 合言葉認証エラーの種別
#[derive(Debug, thiserror::Error)]
pub enum WpassError {
    #[error("HTTP error: {0}")]
    Http(#[from] reqwest::Error),
    #[error(
        "cs_session_id が見つかりません（合言葉不要な配信か、すでに認証済みの可能性があります）"
    )]
    CsSessionIdNotFound,
    #[error("Set-Cookie に wpass が含まれていません（合言葉が間違っている可能性があります）")]
    WpassNotFound,
}

/// レスポンスヘッダーの Set-Cookie から wpass の値を取り出す
fn extract_wpass_cookie(headers: &reqwest::header::HeaderMap) -> Option<String> {
    for value in headers.get_all(reqwest::header::SET_COOKIE) {
        let cookie_str = match value.to_str() {
            Ok(s) => s,
            Err(_) => continue,
        };
        if let Some(rest) = cookie_str.strip_prefix("wpass=") {
            let wpass = rest.split(';').next().unwrap_or(rest).trim().to_string();
            if !wpass.is_empty() {
                return Some(wpass);
            }
        }
    }
    None
}

/// 合言葉（password）を `wpass` クッキー値に変換する
///
/// # 処理の流れ
/// 1. `client` で `GET https://twitcasting.tv/{user_name}` を実行。
///    `did` 等のセッションクッキーが `client` の jar に保存される。
/// 2. `POST https://twitcasting.tv/{user_name}` に合言葉を送信
/// 3. レスポンスの `Set-Cookie: wpass=...` の値を返す
///
/// # 引数
/// `client` は `cookie_store(true)` で生成した共有クライアントを渡すこと。
/// この関数の呼び出し後、同じ `client` を後続の API 呼び出しに使用すれば
/// `did` 等のセッションクッキーが自動送信される。
///
/// # 戻り値
/// `(wpass, cs_session_id)` のタプル。
/// `wpass` はプライベート配信用クッキー値（例: `"c1ebb4933e06ce5617483f665e26627c"`）。
/// `cs_session_id` はこの GET で取得したセッション ID（コメント投稿に利用可能）。
pub async fn resolve_wpass(
    client: &reqwest::Client,
    user_name: &str,
    password: &str,
) -> Result<(String, String), WpassError> {
    let live_page_url = format!("{}/{}", TWICAS_ORIGIN, user_name);

    // 1. GET → did 等のセッションクッキーが client の jar に保存される
    tracing::debug!(target: "mcv::twicas-lib", url = %live_page_url, "resolve_wpass GET");
    let get_response = send_request(
        client,
        "GET",
        &live_page_url,
        RequestBody::None,
        vec![],
        None,
        None,
    )
    .await?;
    log_response_cookies("resolve_wpass GET レスポンス", &get_response);

    let session_cookies: String = get_response
        .cookies()
        .map(|c| format!("{}={}", c.name(), c.value()))
        .collect::<Vec<_>>()
        .join("; ");
    tracing::debug!(target: "mcv::twicas-lib", session_cookies = %session_cookies, "resolve_wpass POST に引き継ぐクッキー");

    let html = get_response.text().await?;
    let cs_session_id = extract_cs_session_id(&html).ok_or(WpassError::CsSessionIdNotFound)?;

    // 2. POST: リダイレクト追跡なし（302 の Set-Cookie: wpass を直接読むため）
    //    GET で得たセッションクッキーを Cookie ヘッダーとして引き継ぐ
    let post_client = reqwest::Client::builder()
        .redirect(reqwest::redirect::Policy::none())
        .build()
        .unwrap();

    let body = format!(
        "password={}&cs_session_id={}",
        urlencoding::encode(password),
        urlencoding::encode(&cs_session_id)
    );

    tracing::debug!(target: "mcv::twicas-lib", url = %live_page_url, "resolve_wpass POST");
    let mut response: reqwest::Response = post_client
        .post(&live_page_url)
        .header(reqwest::header::USER_AGENT, user_agent())
        .header(
            reqwest::header::ACCEPT_LANGUAGE,
            "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7",
        )
        .header(
            reqwest::header::CONTENT_TYPE,
            "application/x-www-form-urlencoded",
        )
        .header(reqwest::header::ORIGIN, TWICAS_ORIGIN)
        .header(reqwest::header::REFERER, format!("{}/", live_page_url))
        .header(reqwest::header::COOKIE, session_cookies)
        .body(body)
        .send()
        .await?;

    // 各リダイレクトホップの Set-Cookie から wpass を探す（最大 5 ホップ）
    let mut hops = 0usize;
    loop {
        tracing::debug!(
            target: "mcv::twicas-lib",
            hop = hops,
            status = %response.status(),
            "resolve_wpass レスポンス"
        );
        log_response_cookies(&format!("resolve_wpass hop={} レスポンス", hops), &response);

        if let Some(wpass) = extract_wpass_cookie(response.headers()) {
            return Ok((wpass, cs_session_id));
        }

        if !response.status().is_redirection() || hops >= 5 {
            break;
        }

        let location = match response.headers().get(reqwest::header::LOCATION) {
            Some(v) => match v.to_str() {
                Ok(s) => s.to_string(),
                Err(_) => break,
            },
            None => break,
        };

        let next_url = if location.starts_with("http") {
            location
        } else {
            format!("{}{}", TWICAS_ORIGIN, location)
        };

        tracing::debug!(target: "mcv::twicas-lib", next_url = %next_url, "resolve_wpass リダイレクト追跡");
        response = post_client
            .get(&next_url)
            .header(reqwest::header::USER_AGENT, user_agent())
            .header(
                reqwest::header::ACCEPT_LANGUAGE,
                "ja-JP,ja;q=0.9,en-US;q=0.8,en;q=0.7",
            )
            .header(reqwest::header::REFERER, &live_page_url)
            .send()
            .await?;

        hops += 1;
    }

    Err(WpassError::WpassNotFound)
}

// ---------------------------------------------------------------------------
// post_comment
// ---------------------------------------------------------------------------

/// コメント投稿レスポンス
#[derive(Debug, Deserialize)]
pub struct PostCommentResponse {
    pub error: Option<serde_json::Value>,
}

/// コメント投稿エラー
#[derive(Debug, thiserror::Error)]
pub enum PostCommentError {
    #[error("HTTP エラー: {0}")]
    Http(#[from] reqwest::Error),
    /// サーバーが `[]` を返した場合。セッション切れ・cs_session_id 無効など。
    #[error("コメント投稿が拒否されました（セッションが無効か cs_session_id が期限切れの可能性があります）")]
    Rejected,
    #[error("レスポンスボディのJSONデコードに失敗: {source}\nBody: {body}")]
    JsonDecode {
        #[source]
        source: serde_json::Error,
        body: String,
    },
}

/// コメントを投稿する
///
/// * `client`        - reqwest クライアント（wpass 等は jar に登録済みであること）
/// * `jar`           - クッキージャー（ログ出力用・リクエスト送信時に自動付与される）
/// * `screen_name`   - 配信者のスクリーンネーム（例: "komachik1221"）
/// * `movie_id`      - ライブ動画ID（`fetch_latest_movie` で取得）
/// * `text`          - 投稿するコメント本文
/// * `cs_session_id` - ライブページHTMLから取得したセッションID（`fetch_session_ids` で取得）
/// * `anonymous`     - `true` なら匿名コメント（a=1）、`false` なら通常コメント（a=0）
pub async fn post_comment(
    client: &reqwest::Client,
    jar: &reqwest::cookie::Jar,
    screen_name: &str,
    movie_id: i64,
    text: &str,
    cs_session_id: &str,
    anonymous: bool,
) -> Result<PostCommentResponse, PostCommentError> {
    let url = format!(
        "{}/{}/userajax.php?c=post&format=json",
        TWICAS_ORIGIN, screen_name
    );
    let fields = vec![
        ("m".to_string(), movie_id.to_string()),
        ("s".to_string(), text.to_string()),
        ("o".to_string(), screen_name.to_string()),
        (
            "a".to_string(),
            if anonymous { "1" } else { "0" }.to_string(),
        ),
        ("nt".to_string(), "2".to_string()), //xに投稿をonにすると0だった
        ("cs_session_id".to_string(), cs_session_id.to_string()),
    ];

    // ログ用: jar に登録済みの twitcasting.tv クッキーを事前に取得する
    // （reqwest の cookie_provider はリクエスト送信時に自動付与するため
    //   req_headers には含まれないが、jar から直接取得することで記録できる）
    let jar_url: reqwest::Url = "https://twitcasting.tv/".parse().unwrap();
    let jar_cookies_str = jar
        .cookies(&jar_url)
        .and_then(|h: reqwest::header::HeaderValue| h.to_str().ok().map(|s: &str| s.to_string()))
        .unwrap_or_else(|| "(なし)".to_string());

    let req_body_str = fields
        .iter()
        .map(|(k, v)| format!("{}={}", k, v))
        .collect::<Vec<_>>()
        .join("&");

    let (response, req_headers) = send_request_inner(
        client,
        "POST",
        &url,
        RequestBody::Multipart(fields),
        vec![],
        None,
        None,
    )
    .await?;

    let req_headers_str: String = req_headers
        .iter()
        .map(|(name, value)| format!("{}: {}", name, value.to_str().unwrap_or("(invalid utf-8)")))
        .collect::<Vec<_>>()
        .join("\n");

    // error_for_status_ref は Response を消費しないため、ボディ読み取り前に確認できる
    let http_error = response.error_for_status_ref().err();

    let status = response.status();
    let resp_headers_str = response
        .headers()
        .iter()
        .map(|(name, value)| format!("{}: {}", name, value.to_str().unwrap_or("(invalid utf-8)")))
        .collect::<Vec<String>>()
        .join("\n");
    let body = response.text().await?;

    // リクエスト〜レスポンスの全コンテキストを1回のログにまとめる
    tracing::debug!(
        target: "mcv::twicas-lib",
        url = %url,
        screen_name = %screen_name,
        movie_id = movie_id,
        anonymous = anonymous,
        cs_session_id = %cs_session_id,
        jar_cookies = %jar_cookies_str,
        request_headers = %req_headers_str,
        request_body = %req_body_str,
        response_status = %status,
        response_headers = %resp_headers_str,
        response_body = %body,
        "post_comment"
    );

    if let Some(e) = http_error {
        return Err(PostCommentError::Http(e));
    }

    // [] はサーバー側の拒否（セッション切れ・cs_session_id 無効・wpass 不正など）を示す
    if body.trim() == "[]" {
        return Err(PostCommentError::Rejected);
    }

    serde_json::from_str::<PostCommentResponse>(&body)
        .map_err(|e| PostCommentError::JsonDecode { source: e, body })
}
