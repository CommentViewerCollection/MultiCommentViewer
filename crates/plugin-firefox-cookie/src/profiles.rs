//! Firefox プロファイル検出
//!
//! %APPDATA%\Mozilla\Firefox\ 以下のプロファイルを profiles.ini から検出する。

use std::collections::HashMap;
use std::path::PathBuf;

use mcv_common::BrowserId;
use uuid::Uuid;

/// Firefox プロファイル情報
pub(crate) struct FirefoxProfile {
    /// プロファイルの表示名（profiles.ini の Name フィールド）
    pub display_name: String,
    /// 決定論的に生成された BrowserId
    pub browser_id: BrowserId,
    /// プロファイルディレクトリ
    pub profile_dir: PathBuf,
}

/// Firefox Cookie プラグイン固有の namespace UUID (UUID v5 の名前空間として使用)
const FIREFOX_COOKIE_NS: &str = "f5d8e1a2-b3c4-5f6a-8b9c-0d1e2f3a4b5c";

/// プロファイルディレクトリ名から決定論的な BrowserId を生成する
fn profile_browser_id(dir_name: &str) -> BrowserId {
    let ns = Uuid::parse_str(FIREFOX_COOKIE_NS).expect("valid namespace UUID");
    let uuid = Uuid::new_v5(&ns, format!("firefox_{}", dir_name).as_bytes());
    BrowserId::new("Firefox", &uuid.to_string())
}

/// profiles.ini の内容を解析し、セクションごとのキーバリューマップを返す
///
/// 戻り値: Vec<(セクション名, HashMap<キー, 値>)>
fn parse_profiles_ini(content: &str) -> Vec<(String, HashMap<String, String>)> {
    let mut sections: Vec<(String, HashMap<String, String>)> = Vec::new();
    let mut current_section: Option<String> = None;
    let mut current_entries: HashMap<String, String> = HashMap::new();

    for line in content.lines() {
        let line = line.trim();
        if line.starts_with('[') && line.ends_with(']') {
            if let Some(section) = current_section.take() {
                sections.push((section, current_entries.clone()));
            }
            current_entries.clear();
            current_section = Some(line[1..line.len() - 1].to_string());
        } else if let Some(pos) = line.find('=') {
            let key = line[..pos].trim().to_lowercase();
            let value = line[pos + 1..].trim().to_string();
            current_entries.insert(key, value);
        }
    }
    if let Some(section) = current_section {
        sections.push((section, current_entries));
    }
    sections
}

/// cookies.sqlite が存在するか確認する
fn has_cookies_file(profile_dir: &std::path::Path) -> bool {
    profile_dir.join("cookies.sqlite").exists()
}

/// インストール済みの Firefox プロファイルを検出して返す
pub(crate) fn get_firefox_profiles() -> Vec<FirefoxProfile> {
    let mut profiles = Vec::new();

    // %APPDATA%\Mozilla\Firefox\
    let appdata = match std::env::var("APPDATA") {
        Ok(p) => p,
        Err(_) => {
            tracing::warn!(target: "mcv::plugin-firefox-cookie", "APPDATA not set");
            return profiles;
        }
    };

    let firefox_dir = std::path::PathBuf::from(&appdata)
        .join("Mozilla")
        .join("Firefox");

    if !firefox_dir.exists() {
        tracing::info!(
            target: "mcv::plugin-firefox-cookie",
            path = %firefox_dir.display(),
            "Firefox directory not found"
        );
        return profiles;
    }

    let ini_path = firefox_dir.join("profiles.ini");
    let ini_content = match std::fs::read_to_string(&ini_path) {
        Ok(c) => c,
        Err(e) => {
            tracing::warn!(
                target: "mcv::plugin-firefox-cookie",
                error = %e,
                "Failed to read profiles.ini"
            );
            return profiles;
        }
    };

    let sections = parse_profiles_ini(&ini_content);

    for (section_name, entries) in &sections {
        // [ProfileN] セクションのみ処理（[Install...] などは除外）
        if !section_name.to_lowercase().starts_with("profile") {
            continue;
        }

        let path_val = match entries.get("path") {
            Some(v) => v.clone(),
            None => continue,
        };

        let is_relative = entries.get("isrelative").map(|v| v == "1").unwrap_or(false);

        let profile_dir = if is_relative {
            // Windows では profiles.ini の Path は '/' 区切りで記述されている
            let rel: PathBuf = path_val.replace('/', std::path::MAIN_SEPARATOR_STR).into();
            firefox_dir.join(rel)
        } else {
            PathBuf::from(&path_val)
        };

        if !profile_dir.exists() || !has_cookies_file(&profile_dir) {
            continue;
        }

        let dir_name = match profile_dir.file_name().and_then(|n| n.to_str()) {
            Some(n) => n.to_string(),
            None => continue,
        };

        let display_name = entries
            .get("name")
            .cloned()
            .unwrap_or_else(|| dir_name.clone());

        let browser_id = profile_browser_id(&dir_name);

        profiles.push(FirefoxProfile {
            display_name,
            browser_id,
            profile_dir,
        });
    }

    profiles
}
