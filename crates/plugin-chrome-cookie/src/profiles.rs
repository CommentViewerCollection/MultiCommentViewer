//! Chrome プロファイル検出
//!
//! %LOCALAPPDATA%\Google\Chrome\User Data\ 以下のプロファイルを検出する。

use chromium_cookie_lib::profiles::{get_chromium_profiles, ChromiumProfile};
use mcv_messages::BrowserId;
use uuid::Uuid;

/// Chrome Cookie プラグイン固有の namespace UUID (UUID v5 の名前空間として使用)
const CHROME_COOKIE_NS: &str = "a4c7d1e2-f3b5-4e8a-9c0d-b1e2f3a4c5d6";

/// プロファイルディレクトリ名から決定論的な BrowserId を生成する
fn profile_browser_id(dir_name: &str) -> BrowserId {
    let ns = Uuid::parse_str(CHROME_COOKIE_NS).expect("valid namespace UUID");
    let uuid = Uuid::new_v5(&ns, format!("chrome_{}", dir_name).as_bytes());
    BrowserId::new("Chrome", &uuid.to_string())
}

/// インストール済みの Chrome プロファイルを検出して返す
pub(crate) fn get_chrome_profiles() -> Vec<ChromiumProfile> {
    let local_app_data = match std::env::var("LOCALAPPDATA") {
        Ok(p) => p,
        Err(_) => {
            tracing::warn!(target: "mcv::plugin-chrome-cookie", "LOCALAPPDATA not set");
            return vec![];
        }
    };

    let user_data_dir = std::path::PathBuf::from(&local_app_data)
        .join("Google")
        .join("Chrome")
        .join("User Data");

    get_chromium_profiles(&user_data_dir, profile_browser_id, "mcv::plugin-chrome-cookie")
}
