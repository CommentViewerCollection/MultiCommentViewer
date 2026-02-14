//! Chrome プロファイル検出
//!
//! %LOCALAPPDATA%\Google\Chrome\User Data\ 以下のプロファイルを検出する。
//! 各プロファイルに対して決定論的な BrowserId を生成する。

use mcv_common::BrowserId;
use uuid::Uuid;

/// Chrome プロファイル情報
pub(crate) struct ChromeProfile {
    /// プロファイルの表示名（Preferences から取得、失敗時はディレクトリ名）
    pub display_name: String,
    /// 決定論的に生成された BrowserId
    pub browser_id: BrowserId,
}

/// Chrome Cookie プラグイン固有の namespace UUID (UUID v5 の名前空間として使用)
const CHROME_COOKIE_NS: &str = "a4c7d1e2-f3b5-4e8a-9c0d-b1e2f3a4c5d6";

/// プロファイルディレクトリ名から決定論的な BrowserId を生成する
fn profile_browser_id(dir_name: &str) -> BrowserId {
    let ns = Uuid::parse_str(CHROME_COOKIE_NS).expect("valid namespace UUID");
    let uuid = Uuid::new_v5(&ns, format!("chrome_{}", dir_name).as_bytes());
    BrowserId::new("Chrome", &uuid.to_string())
}

/// Preferences JSON からプロファイルの表示名を読み取る
/// 失敗した場合は dir_name をそのまま返す
fn read_profile_display_name(profile_dir: &std::path::Path, dir_name: &str) -> String {
    let prefs_path = profile_dir.join("Preferences");
    let content = match std::fs::read_to_string(&prefs_path) {
        Ok(c) => c,
        Err(_) => return dir_name.to_string(),
    };
    let json: serde_json::Value = match serde_json::from_str(&content) {
        Ok(v) => v,
        Err(_) => return dir_name.to_string(),
    };
    json["profile"]["name"]
        .as_str()
        .filter(|s| !s.is_empty())
        .unwrap_or(dir_name)
        .to_string()
}

/// Cookies ファイルが存在するか確認する（新形式・旧形式の両方をチェック）
fn has_cookies_file(profile_dir: &std::path::Path) -> bool {
    // 新形式: <profile>/Network/Cookies
    if profile_dir.join("Network").join("Cookies").exists() {
        return true;
    }
    // 旧形式: <profile>/Cookies
    if profile_dir.join("Cookies").exists() {
        return true;
    }
    false
}

/// インストール済みの Chrome プロファイルを検出して返す
pub(crate) fn get_chrome_profiles() -> Vec<ChromeProfile> {
    let mut profiles = Vec::new();

    // %LOCALAPPDATA%\Google\Chrome\User Data\
    let local_app_data = match std::env::var("LOCALAPPDATA") {
        Ok(p) => p,
        Err(_) => {
            tracing::warn!(target: "mcv::plugin-chrome-cookie", "LOCALAPPDATA not set");
            return profiles;
        }
    };

    let user_data_dir = std::path::PathBuf::from(&local_app_data)
        .join("Google")
        .join("Chrome")
        .join("User Data");

    if !user_data_dir.exists() {
        tracing::info!(
            target: "mcv::plugin-chrome-cookie",
            path = %user_data_dir.display(),
            "Chrome User Data directory not found"
        );
        return profiles;
    }

    // Default プロファイルをチェック
    let default_dir_name = "Default";
    let default_profile_dir = user_data_dir.join(default_dir_name);
    if default_profile_dir.exists() && has_cookies_file(&default_profile_dir) {
        let display_name = read_profile_display_name(&default_profile_dir, default_dir_name);
        let browser_id = profile_browser_id(default_dir_name);
        profiles.push(ChromeProfile {
            display_name,
            browser_id,
        });
    }

    // Profile N ディレクトリを列挙
    let dir_entries = match std::fs::read_dir(&user_data_dir) {
        Ok(entries) => entries,
        Err(e) => {
            tracing::error!(
                target: "mcv::plugin-chrome-cookie",
                error = %e,
                "Failed to read Chrome User Data directory"
            );
            return profiles;
        }
    };

    for entry in dir_entries.flatten() {
        let path = entry.path();
        if !path.is_dir() {
            continue;
        }
        let dir_name = match path.file_name().and_then(|n| n.to_str()) {
            Some(n) => n.to_string(),
            None => continue,
        };

        // "Profile" で始まるディレクトリのみ対象（大文字小文字区別なし）
        if !dir_name.to_lowercase().starts_with("profile") {
            continue;
        }

        if !has_cookies_file(&path) {
            continue;
        }

        let display_name = read_profile_display_name(&path, &dir_name);
        let browser_id = profile_browser_id(&dir_name);
        profiles.push(ChromeProfile {
            display_name,
            browser_id,
        });
    }

    profiles
}
