use crate::error::WhoWatchError;
use crate::live::{
    JoinCommentsResponse, LivePlayResponse, LiveResponse, PlayItemsResponse, SendCommentResponse,
};
use crate::user::{MyUser, UserProfile};

const API_BASE: &str = "https://api.whowatch.tv/";

/// ふわっち内部 API クライアント。
///
/// `whowatch_token` は `WHOWATCH` Cookie の値（JWT 文字列）。
/// `device_id` は `x-whowatch-device-id` ヘッダーに使う端末 ID（例: `"1773735937981-90461387"`）。
/// 省略した場合は起動時刻ベースの値を自動生成する。
#[derive(Debug, Clone)]
pub struct WhoWatchClient {
    http: reqwest::Client,
    device_id: String,
}

impl WhoWatchClient {
    /// 新しいクライアントを生成する。
    ///
    /// # 引数
    /// * `whowatch_token` - `WHOWATCH` Cookie の値（JWT）
    /// * `device_id`      - `x-whowatch-device-id` の値。`None` を渡すと自動生成する。
    pub fn new(whowatch_token: &str, device_id: Option<&str>) -> Result<Self, WhoWatchError> {
        let device_id = device_id
            .map(|s| s.to_string())
            .unwrap_or_else(generate_device_id);

        let cookie = format!("WHOWATCH={}", whowatch_token);

        let http = reqwest::Client::builder()
            .user_agent(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) \
                 AppleWebKit/537.36 (KHTML, like Gecko) \
                 Chrome/147.0.0.0 Safari/537.36",
            )
            .default_headers({
                let mut headers = reqwest::header::HeaderMap::new();
                headers.insert(
                    reqwest::header::COOKIE,
                    reqwest::header::HeaderValue::from_str(&cookie).expect("invalid cookie value"),
                );
                headers.insert(
                    reqwest::header::ACCEPT,
                    reqwest::header::HeaderValue::from_static("application/json, text/plain, */*"),
                );
                headers.insert(
                    reqwest::header::HeaderName::from_static("origin"),
                    reqwest::header::HeaderValue::from_static("https://whowatch.tv"),
                );
                headers.insert(
                    reqwest::header::HeaderName::from_static("referer"),
                    reqwest::header::HeaderValue::from_static("https://whowatch.tv/"),
                );
                headers
            })
            .gzip(true)
            .build()?;

        Ok(Self { http, device_id })
    }

    // -----------------------------------------------------------------------
    // ユーザー API
    // -----------------------------------------------------------------------

    /// `GET /users/me` — ログイン中のユーザー情報を取得する。
    pub async fn get_my_user(&self) -> Result<MyUser, WhoWatchError> {
        let url = format!("{}users/me", API_BASE);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "GET /users/me");
        let resp = resp.error_for_status()?.json::<MyUser>().await?;
        Ok(resp)
    }

    /// `GET /users/{user_id}/profile` — ユーザープロフィールと配信情報を取得する。
    pub async fn get_user_profile(&self, user_id: u64) -> Result<UserProfile, WhoWatchError> {
        let url = format!("{}users/{}/profile", API_BASE, user_id);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "GET /users/{user_id}/profile");
        let resp = resp.error_for_status()?.json::<UserProfile>().await?;
        Ok(resp)
    }

    // -----------------------------------------------------------------------
    // 配信 API
    // -----------------------------------------------------------------------

    /// `POST /lives/{live_id}/join_comments` — コメントストリームに参加する。
    ///
    /// ポーリングを開始する前に一度呼ぶ必要がある。
    /// 返り値の `last_updated_at` を最初の [`get_live`] の引数に使う。
    pub async fn join_comments(&self, live_id: u64) -> Result<JoinCommentsResponse, WhoWatchError> {
        let url = format!("{}lives/{}/join_comments", API_BASE, live_id);
        let resp = self
            .http
            .post(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .header(reqwest::header::CONTENT_LENGTH, "0")
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "POST /lives/{live_id}/join_comments");
        let resp = resp
            .error_for_status()?
            .json::<JoinCommentsResponse>()
            .await?;
        Ok(resp)
    }

    /// `GET /lives/{live_id}?last_updated_at={timestamp}` — 配信情報とコメントをポーリングする。
    ///
    /// `last_updated_at` には前回のレスポンスの `live.updated_at` を渡す。
    /// 初回は [`join_comments`] の返り値か、現在時刻のミリ秒を渡す。
    pub async fn get_live(
        &self,
        live_id: u64,
        last_updated_at: u64,
    ) -> Result<LiveResponse, WhoWatchError> {
        let url = format!("{}lives/{}", API_BASE, live_id);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .query(&[("last_updated_at", last_updated_at.to_string())])
            .send()
            .await?;
        let status = resp.status();
        let raw = resp.error_for_status()?.text().await?;

        tracing::debug!(
            target: "mcv::whowatch-lib",
            %url,
            %status,
            response_body = %raw,
            "GET /lives/{live_id}"
        );

        let resp = serde_json::from_str::<LiveResponse>(&raw)?;
        Ok(resp)
    }

    /// `POST /lives/{live_id}/comments` — コメントを投稿する。
    ///
    /// `anonymous` を `true` にすると匿名投稿（たぬきの葉っぱ使用時など）。
    pub async fn send_comment(
        &self,
        live_id: u64,
        message: &str,
        anonymous: bool,
    ) -> Result<SendCommentResponse, WhoWatchError> {
        let url = format!("{}lives/{}/comments", API_BASE, live_id);
        let mut body = serde_json::json!({ "message": message });
        if anonymous {
            body["anonymous"] = serde_json::Value::Bool(true);
        }
        let resp = self
            .http
            .post(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .json(&body)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "POST /lives/{live_id}/comments");
        let resp = resp
            .error_for_status()?
            .json::<SendCommentResponse>()
            .await?;
        Ok(resp)
    }

    /// `GET /users/{user_path}/profile` — ユーザーパス（例: `w:Rabbit320`）でプロフィールを取得する。
    ///
    /// ふわっち URL（`https://whowatch.tv/w:Rabbit320`）から取り出したパスをそのまま渡せる。
    pub async fn get_user_profile_by_path(
        &self,
        user_path: &str,
    ) -> Result<UserProfile, WhoWatchError> {
        let encoded = user_path.replace(':', "%3A");
        let url = format!("{}users/{}/profile", API_BASE, encoded);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(
            target: "mcv::whowatch-lib",
            %url,
            %status,
            "GET /users/{user_path}/profile"
        );
        let resp = resp.error_for_status()?.json::<UserProfile>().await?;
        Ok(resp)
    }

    /// `GET /lives/{live_id}/play` — WebSocket 接続トークンを取得する。
    ///
    /// 返り値の [`LivePlayResponse::ws_token`] が Phoenix の phx_join `p` フィールドに使うトークン。
    pub async fn get_live_play(&self, live_id: u64) -> Result<LivePlayResponse, WhoWatchError> {
        let url = format!("{}lives/{}/play", API_BASE, live_id);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "GET /lives/{live_id}/play");
        let resp = resp.error_for_status()?.json::<LivePlayResponse>().await?;
        Ok(resp)
    }

    /// `GET /lives/{live_id}/playitems3` — 利用可能なプレイアイテム一覧を取得する。
    pub async fn get_playitems3(&self, live_id: u64) -> Result<PlayItemsResponse, WhoWatchError> {
        let url = format!("{}lives/{}/playitems3", API_BASE, live_id);
        let resp = self
            .http
            .get(&url)
            .header("x-whowatch-device-id", &self.device_id)
            .send()
            .await?;
        let status = resp.status();
        tracing::debug!(target: "mcv::whowatch-lib", %url, %status, "GET /lives/{live_id}/playitems3");
        let resp = resp.error_for_status()?.json::<PlayItemsResponse>().await?;
        Ok(resp)
    }
}

// ---------------------------------------------------------------------------
// ヘルパー
// ---------------------------------------------------------------------------

/// `x-whowatch-device-id` の自動生成（例: `"1773735937981-90461387"`）
fn generate_device_id() -> String {
    use std::time::{SystemTime, UNIX_EPOCH};
    let millis = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .map(|d| d.as_millis())
        .unwrap_or(0);
    // 8 桁の疑似ランダム数（環境依存でよい）
    let suffix = (millis % 100_000_000) as u32;
    format!("{}-{:08}", millis, suffix)
}
