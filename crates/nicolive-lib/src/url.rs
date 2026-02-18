//! ニコニコ生放送 URL ユーティリティ・ページフェッチ

use regex::Regex;

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

/// ニコ生視聴ページから embedded-data を取得し、WebSocket URL を返す。
pub async fn fetch_websocket_url(live_id: &str) -> Result<String, String> {
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

    Ok(ws_url)
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
