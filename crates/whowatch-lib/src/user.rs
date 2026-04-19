use serde::{Deserialize, Serialize};

/// `GET /users/me` のレスポンス
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct MyUser {
    pub id: u64,
    pub name: String,
    pub account_name: String,
    pub icon_url: String,
    pub user_code: String,
    pub user_path: String,
    pub verified: bool,
    pub is_admin: bool,
}

/// `GET /users/{user_id}/profile` のレスポンス
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct UserProfile {
    pub user_id: u64,
    pub name: String,
    pub account_name: String,
    pub icon_url: String,
    pub background_url: Option<String>,
    pub description: Option<String>,
    pub user_path: String,
    pub gender: Option<String>,
    pub area: Option<String>,
    pub follower_count: u64,
    pub follow_count: u64,
    /// 現在配信中の場合に設定される
    pub live: Option<ProfileLiveInfo>,
    pub publish_grade_name: Option<String>,
    pub publish_grade_icon_url: Option<String>,
    pub is_vliver: bool,
    pub is_follow: bool,
    pub is_blocked: bool,
}

/// ユーザープロフィール内の配信情報（配信中の場合のみ）
#[derive(Debug, Clone, Deserialize, Serialize)]
pub struct ProfileLiveInfo {
    pub id: u64,
    pub title: String,
    /// Unix ミリ秒
    pub started_at: u64,
    pub client_type: String,
    pub is_comment_disallowed: bool,
    pub is_drive_mode: bool,
    pub is_portrait: bool,
    pub hide_category_default_badge: bool,
}
