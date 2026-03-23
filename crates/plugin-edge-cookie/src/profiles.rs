//! Edge プロファイル検出
//!
//! %LOCALAPPDATA%\Microsoft\Edge\User Data\ 以下のプロファイルを検出する。

use chromium_cookie_lib::profiles::{get_chromium_profiles, ChromiumProfile};
use mcv_messages::BrowserId;
use uuid::Uuid;

/// Edge Cookie プラグイン固有の namespace UUID (UUID v5 の名前空間として使用)
const EDGE_COOKIE_NS: &str = "b5d8e3f1-c4a6-4f9b-8d1e-c2f3a5b6d7e8";

/// プロファイルディレクトリ名から決定論的な BrowserId を生成する
fn profile_browser_id(dir_name: &str) -> BrowserId {
    let ns = Uuid::parse_str(EDGE_COOKIE_NS).expect("valid namespace UUID");
    let uuid = Uuid::new_v5(&ns, format!("edge_{}", dir_name).as_bytes());
    BrowserId::new("Edge", &uuid.to_string())
}

/// インストール済みの Edge プロファイルを検出して返す
pub(crate) fn get_edge_profiles() -> Vec<ChromiumProfile> {
    let local_app_data = match std::env::var("LOCALAPPDATA") {
        Ok(p) => p,
        Err(_) => {
            tracing::warn!(target: "mcv::plugin-edge-cookie", "LOCALAPPDATA not set");
            return vec![];
        }
    };

    let user_data_dir = std::path::PathBuf::from(&local_app_data)
        .join("Microsoft")
        .join("Edge")
        .join("User Data");

    get_chromium_profiles(&user_data_dir, profile_browser_id, "mcv::plugin-edge-cookie")
}
