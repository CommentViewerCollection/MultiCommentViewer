//! ニコニコ生放送 URL ユーティリティ・ページフェッチ

use regex::Regex;

#[derive(Debug, Clone)]
pub struct Cookie {
    pub name: String,
    pub value: String,
}

/// URL から live_id を抽出する。
///
/// # 例
/// ```
/// use nicolive_lib::extract_live_id;
/// assert_eq!(
///     extract_live_id("https://live.nicovideo.jp/watch/lv349907621"),
///     Some("lv349907621".to_string())
/// );
/// ```
pub fn extract_live_id(url: &str) -> Option<String> {
    let trimmed = url.trim();
    let after_host = trimmed
        .strip_prefix("https://live.nicovideo.jp/")
        .or_else(|| trimmed.strip_prefix("http://live.nicovideo.jp/"))?;
    let after_watch = after_host.strip_prefix("watch/")?;
    let live_id = after_watch
        .split(['/', '?', '#'])
        .next()
        .unwrap_or_default()
        .trim();
    if live_id.is_empty() {
        return None;
    }
    Some(live_id.to_string())
}

/// ニコ生視聴ページから取得した接続データ
pub struct NicoLiveConnectionData {
    pub ws_url: String,
    pub viewer_id: Option<String>,
    pub viewer_name: Option<String>,
    pub viewer_icon_url: Option<String>,
}

pub struct NicoLiveAccountInfo {
    pub user_id: String,
    pub display_name: String,
    pub avatar_url: Option<String>,
}

/// ニコ生視聴ページから embedded-data を取得し、WebSocket URL と視聴者情報を返す。
pub async fn fetch_websocket_url(live_id: &str) -> Result<NicoLiveConnectionData, String> {
    let page_url = format!("https://live.nicovideo.jp/watch/{live_id}");

    let html = reqwest::get(&page_url)
        .await
        .map_err(|e| format!("HTTP request failed: {e}"))?
        .text()
        .await
        .map_err(|e| format!("Failed to read response body: {e}"))?;

    let raw_props = extract_data_props(&html)
        .ok_or_else(|| "embedded-data の data-props が見つからない".to_string())?;

    let decoded = raw_props
        .replace("&quot;", "\"")
        .replace("&amp;", "&")
        .replace("&#39;", "'")
        .replace("&#x27;", "'");

    let json: serde_json::Value = serde_json::from_str(&decoded)
        .map_err(|e| format!("data-props の JSON パースに失敗: {e}"))?;

    let ws_url = json["site"]["relive"]["webSocketUrl"]
        .as_str()
        .ok_or_else(|| "site.relive.webSocketUrl が見つからない".to_string())?
        .to_string();

    // 視聴者情報を抽出（ログイン済みの場合のみ存在する）
    let viewer_id = json
        .pointer("/user/id")
        .and_then(|v| v.as_u64())
        .map(|id| id.to_string());
    let viewer_name = json
        .pointer("/user/nickname")
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());
    let viewer_icon_url = json
        .pointer("/user/iconImage/url")
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());

    Ok(NicoLiveConnectionData {
        ws_url,
        viewer_id,
        viewer_name,
        viewer_icon_url,
    })
}

/// ニコ生トップページからログイン中ユーザー情報を取得する。
///
/// 取得できない場合（未ログイン、ページ構造変更など）は `Ok(None)` を返す。
pub async fn fetch_account_info_from_top(
    cookies: &[Cookie],
) -> Result<Option<NicoLiveAccountInfo>, String> {
    let cookie_header = cookies
        .iter()
        .map(|c| format!("{}={}", c.name, c.value))
        .collect::<Vec<_>>()
        .join("; ");

    let client = reqwest::Client::new();
    let mut req = client
        .get("https://live.nicovideo.jp/")
        .header(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:146.0) Gecko/20100101 Firefox/146.0",
        )
        .header(
            "Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        );
    if !cookie_header.is_empty() {
        req = req.header("Cookie", cookie_header);
    }

    let html = req
        .send()
        .await
        .map_err(|e| format!("HTTP request failed: {e}"))?
        .text()
        .await
        .map_err(|e| format!("Failed to read response body: {e}"))?;

    let raw_props = match extract_data_props(&html) {
        Some(v) => v,
        None => return Ok(None),
    };

    let decoded = raw_props
        .replace("&quot;", "\"")
        .replace("&amp;", "&")
        .replace("&#39;", "'")
        .replace("&#x27;", "'");

    let json: serde_json::Value = match serde_json::from_str(&decoded) {
        Ok(v) => v,
        Err(_) => return Ok(None),
    };

    let viewer_name = json
        .pointer("/user/nickname")
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());
    let Some(display_name) = viewer_name else {
        return Ok(None);
    };

    let user_id = json
        .pointer("/user/id")
        .and_then(|v| v.as_u64())
        .map(|id| id.to_string())
        .unwrap_or_else(|| display_name.clone());
    let avatar_url = json
        .pointer("/user/iconImage/url")
        .and_then(|v| v.as_str())
        .map(|s| s.to_string());

    Ok(Some(NicoLiveAccountInfo {
        user_id,
        display_name,
        avatar_url,
    }))
}

/// HTML から `<script id="embedded-data" data-props="...">` の値を取り出す。
pub fn extract_data_props(html: &str) -> Option<String> {
    let re = Regex::new(r#"<script\b[^>]*\bid="embedded-data"[^>]*>"#).ok()?;
    let tag = re.find(html)?.as_str();
    let props_re = Regex::new(r#"data-props="([^"]*)""#).ok()?;
    let caps = props_re.captures(tag)?;
    Some(caps[1].to_string())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_extract_live_id_standard() {
        assert_eq!(
            extract_live_id("https://live.nicovideo.jp/watch/lv349907621"),
            Some("lv349907621".to_string())
        );
    }

    #[test]
    fn test_extract_live_id_with_query() {
        assert_eq!(
            extract_live_id("https://live.nicovideo.jp/watch/lv349907621?ref=top"),
            Some("lv349907621".to_string())
        );
    }

    #[test]
    fn test_extract_live_id_with_fragment() {
        assert_eq!(
            extract_live_id("https://live.nicovideo.jp/watch/lv349907621#anchor"),
            Some("lv349907621".to_string())
        );
    }

    #[test]
    fn test_extract_live_id_invalid() {
        assert_eq!(extract_live_id("https://www.nicovideo.jp/watch/sm12345"), None);
        assert_eq!(extract_live_id("https://example.com/"), None);
        assert_eq!(extract_live_id(""), None);
    }

    #[test]
    fn test_extract_data_props() {
        let html = r#"<html><head>
            <script id="embedded-data" data-props="{&quot;foo&quot;:1}"></script>
            </head></html>"#;
        assert_eq!(
            extract_data_props(html),
            Some("{&quot;foo&quot;:1}".to_string())
        );
    }
}
