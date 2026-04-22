//! Chromium ブラウザのプロファイル検出
//!
//! Chrome・Edge など Chromium ベースのブラウザの User Data ディレクトリを走査し、
//! クッキーファイルを持つプロファイルを列挙する。

use std::collections::HashMap;
use std::path::{Path, PathBuf};

use mcv_messages::BrowserId;

/// Chromium プロファイル情報
pub struct ChromiumProfile {
    /// プロファイルの表示名（Local State の info_cache から取得、失敗時はディレクトリ名）
    pub display_name: String,
    /// 呼び出し側が生成した BrowserId
    pub browser_id: BrowserId,
    /// プロファイルディレクトリ
    pub profile_dir: PathBuf,
}

/// Local State ファイルから全プロファイルの表示名マップを読み込む
///
/// 戻り値: `HashMap<ディレクトリ名, 表示名>`
/// 優先: `gaia_name`（アカウント表示名）→ `name`（ローカル名）
pub fn load_profile_names_from_local_state(
    user_data_dir: &Path,
    log_target: &str,
) -> HashMap<String, String> {
    let mut map = HashMap::new();

    let local_state_path = user_data_dir.join("Local State");
    let content = match std::fs::read_to_string(&local_state_path) {
        Ok(c) => c,
        Err(e) => {
            tracing::warn!(
                target: "chromium_cookie_lib",
                log_target = log_target,
                error = %e,
                "Failed to read Local State"
            );
            return map;
        }
    };

    let json: serde_json::Value = match serde_json::from_str(&content) {
        Ok(v) => v,
        Err(e) => {
            tracing::warn!(
                target: "chromium_cookie_lib",
                log_target = log_target,
                error = %e,
                "Failed to parse Local State JSON"
            );
            return map;
        }
    };

    let info_cache = match json["profile"]["info_cache"].as_object() {
        Some(cache) => cache,
        None => {
            tracing::warn!(
                target: "chromium_cookie_lib",
                log_target = log_target,
                "profile.info_cache not found in Local State"
            );
            return map;
        }
    };

    for (dir_name, info) in info_cache {
        // 優先 1: gaia_name（アカウントの表示名）
        if let Some(gaia_name) = info["gaia_name"].as_str() {
            if !gaia_name.is_empty() {
                map.insert(dir_name.clone(), gaia_name.to_string());
                continue;
            }
        }
        // 優先 2: name（ブラウザが付けたローカルプロファイル名）
        if let Some(name) = info["name"].as_str() {
            if !name.is_empty() {
                map.insert(dir_name.clone(), name.to_string());
                continue;
            }
        }
        // フォールバック: ディレクトリ名をそのまま使う（呼び出し側で行う）
    }

    map
}

/// プロファイルディレクトリにクッキーファイルが存在するか確認する
pub fn has_cookies_file(profile_dir: &Path) -> bool {
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

/// Chromium ブラウザのプロファイルを列挙して返す。
///
/// - `user_data_dir`: User Data ディレクトリ（例: `%LOCALAPPDATA%\Google\Chrome\User Data`）
/// - `make_browser_id`: ディレクトリ名から `BrowserId` を生成するクロージャ
/// - `log_target`: ログ出力先の識別子（呼び出し側のクレート名など）
pub fn get_chromium_profiles(
    user_data_dir: &Path,
    make_browser_id: impl Fn(&str) -> BrowserId,
    log_target: &str,
) -> Vec<ChromiumProfile> {
    let mut profiles = Vec::new();

    if !user_data_dir.exists() {
        tracing::info!(
            target: "chromium_cookie_lib",
            log_target = log_target,
            path = %user_data_dir.display(),
            "User Data directory not found"
        );
        return profiles;
    }

    // Local State からプロファイル名マップを一括取得
    let profile_names = load_profile_names_from_local_state(user_data_dir, log_target);

    // Default プロファイルをチェック
    let default_dir_name = "Default";
    let default_profile_dir = user_data_dir.join(default_dir_name);
    if default_profile_dir.exists() && has_cookies_file(&default_profile_dir) {
        let display_name = profile_names
            .get(default_dir_name)
            .cloned()
            .unwrap_or_else(|| default_dir_name.to_string());
        let browser_id = make_browser_id(default_dir_name);
        profiles.push(ChromiumProfile {
            display_name,
            browser_id,
            profile_dir: default_profile_dir,
        });
    }

    // Profile N ディレクトリを列挙
    let dir_entries = match std::fs::read_dir(user_data_dir) {
        Ok(entries) => entries,
        Err(e) => {
            tracing::error!(
                target: "chromium_cookie_lib",
                log_target = log_target,
                error = %e,
                "Failed to read User Data directory"
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

        let display_name = profile_names
            .get(&dir_name)
            .cloned()
            .unwrap_or_else(|| dir_name.clone());
        let browser_id = make_browser_id(&dir_name);
        profiles.push(ChromiumProfile {
            display_name,
            browser_id,
            profile_dir: path,
        });
    }

    profiles
}
