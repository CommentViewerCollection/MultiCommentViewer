use serde::{Deserialize, Serialize};

// ---------------------------------------------------------------------------
// GET /lives/{live_id}/play  （WebSocket 接続トークン取得）
// ---------------------------------------------------------------------------

/// `GET /lives/{live_id}/play` のレスポンス。
/// Phoenix WebSocket への接続に必要なトークンを含む。
/// フィールド名はバージョンによって変わる可能性があるため `serde_json::Value` で受け取り、
/// [`extract_ws_token`] でトークンを取り出す。
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct LivePlayResponse {
    /// WebSocket 接続用トークン（phx_join の `p` フィールドに使う）
    pub room_token: Option<String>,
    /// エラーコード（"L-017" は視聴不可）
    pub error_code: Option<String>,
    /// レスポンス全体（不明フィールドの保持用）
    #[serde(flatten)]
    pub extra: std::collections::HashMap<String, serde_json::Value>,
}

impl LivePlayResponse {
    /// WebSocket トークンを取り出す。
    /// `room_token` → `token` → `p` → `live_token` → `ws_token` の順で探す。
    pub fn ws_token(&self) -> Option<&str> {
        if let Some(t) = &self.room_token {
            return Some(t.as_str());
        }
        for field in &["token", "p", "live_token", "ws_token", "join_token"] {
            if let Some(v) = self.extra.get(*field) {
                if let Some(s) = v.as_str() {
                    return Some(s);
                }
            }
        }
        None
    }
}

// ---------------------------------------------------------------------------
// GET /lives/{live_id}?last_updated_at={timestamp}
// ---------------------------------------------------------------------------

/// `GET /lives/{live_id}?last_updated_at={timestamp}` のレスポンス。
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct LiveResponse {
    pub live: LiveInfo,
    #[serde(default)]
    pub comments: Vec<Comment>,
    /// 削除されたコメントの ID 一覧。通常は空配列。
    #[serde(default)]
    pub deleted_comment_ids: Vec<u64>,
    /// Phoenix WebSocket 接続用 JWT トークン（phx_join の `p` フィールドに使う）
    pub jwt: Option<String>,
    /// WebSocket サーバー URL（例: `wss://ws.whowatch.tv/socket`）
    pub comment_server_url: Option<String>,
    /// ポーリング推奨間隔（ミリ秒）
    pub polling_interval: Option<u64>,
    /// コメントポーリング用更新タイムスタンプ（Unix ミリ秒）。
    /// 次回の `last_updated_at` に渡す。
    pub updated_at: Option<u64>,
}

/// 配信情報
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct LiveInfo {
    pub id: u64,
    pub title: Option<String>,
    /// 配信ステータス（"PUBLISHING" = 配信中）
    pub live_status: Option<String>,
    /// 現在の視聴者数
    pub view_count: Option<u64>,
    /// 累計視聴者数
    pub total_view_count: Option<u64>,
    /// Unix ミリ秒
    pub started_at: Option<u64>,
    /// 配信開始からの経過秒数（API が返す running_time フィールド）
    pub running_time: Option<u64>,
    pub is_comment_disallowed: Option<bool>,
    pub is_drive_mode: Option<bool>,
    pub is_portrait: Option<bool>,
    pub client_type: Option<String>,
    pub user: Option<LiveUser>,
}

impl LiveInfo {
    /// 配信中かどうかを返す。
    pub fn is_publishing(&self) -> bool {
        self.live_status
            .as_deref()
            .map(|s| s == "PUBLISHING")
            .unwrap_or(false)
    }
}

/// 配信者情報（live オブジェクト内）
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct LiveUser {
    pub id: u64,
    pub name: Option<String>,
    pub icon_url: Option<String>,
    pub user_path: Option<String>,
}

/// コメント
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct Comment {
    pub id: u64,
    pub message: String,
    /// コメント投稿時刻（Unix ミリ秒）
    pub posted_at: u64,
    pub comment_type: Option<String>,
    pub anonymized: Option<bool>,
    pub enabled: Option<bool>,
    pub is_tanuki: Option<bool>,
    pub is_first_comment: Option<bool>,
    pub is_silent_comment: Option<bool>,
    pub user: Option<CommentUser>,
}

/// コメント投稿者情報
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct CommentUser {
    pub id: Option<u64>,
    pub name: Option<String>,
    pub icon_url: Option<String>,
    pub user_path: Option<String>,
    pub is_admin: Option<bool>,
}

// ---------------------------------------------------------------------------
// POST /lives/{live_id}/join_comments
// ---------------------------------------------------------------------------

/// `POST /lives/{live_id}/join_comments` のレスポンス
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct JoinCommentsResponse {
    /// 参加成功時の初期 last_updated_at（Unix ミリ秒）
    pub last_updated_at: Option<u64>,
    pub message: Option<String>,
}

// ---------------------------------------------------------------------------
// POST /lives/{live_id}/comments  （コメント投稿）
// ---------------------------------------------------------------------------

/// `POST /lives/{live_id}/comments` のレスポンス
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct SendCommentResponse {
    pub comment: Option<Comment>,
    pub message: Option<String>,
}

// ---------------------------------------------------------------------------
// GET /lives/{live_id}/playitems3
// ---------------------------------------------------------------------------

/// `GET /lives/{live_id}/playitems3` のレスポンス
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct PlayItemsResponse {
    #[serde(default)]
    pub user_retain_items: Vec<PlayItem>,
}

/// プレイアイテム（ギフトアイテム等）
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct PlayItem {
    pub play_item_id: u64,
    pub description: Option<String>,
    pub count: Option<u64>,
    pub count_label: Option<String>,
    pub is_enabled: bool,
    pub is_infinity: bool,
    pub is_lock: bool,
    #[serde(default)]
    pub patterns: Vec<PlayItemPattern>,
    pub badge_text: Option<String>,
}

/// プレイアイテムのパターン（送信時に使うパターン ID 等）
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct PlayItemPattern {
    pub play_item_pattern_id: u64,
    pub item_quantity: u64,
    pub name: String,
    pub image_url: String,
    pub send_comment: bool,
    pub anonymous_post: bool,
    pub required_comment: bool,
    pub pattern_limit: Option<u64>,
}
