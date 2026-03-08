//! TwitCastingページのHTML解析ユーティリティ

use scraper::{Html, Selector};
use serde_json::Value;

/// HTMLから tc-page-variables を取得してJSONとして返す
pub fn extract_tc_page_variables(html: &str) -> Option<Value> {
    let document = Html::parse_document(html);
    let selector = Selector::parse(r#"meta[name="tc-page-variables"]"#).ok()?;
    let element = document.select(&selector).next()?;
    let content = element.value().attr("content")?;
    let decoded = html_escape::decode_html_entities(content);
    serde_json::from_str(&decoded).ok()
}

/// tc-page-variables から指定キーの値を取得
pub fn get_tc_variable(html: &str, key: &str) -> Option<String> {
    let json = extract_tc_page_variables(html)?;
    match json.get(key) {
        Some(Value::String(s)) => Some(s.clone()),
        Some(v) => Some(v.to_string()),
        None => None,
    }
}

/// HTMLのパスワードフォームから `cs_session_id` を取得する
///
/// `<input type="hidden" name="cs_session_id" value="...">` を探す
pub fn extract_cs_session_id(html: &str) -> Option<String> {
    let document = Html::parse_document(html);
    let selector =
        Selector::parse(r#"input[type="hidden"][name="cs_session_id"]"#).ok()?;
    let element = document.select(&selector).next()?;
    element.value().attr("value").map(|s| s.to_string())
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_extract_tc_page_variables_missing() {
        assert!(extract_tc_page_variables("<html></html>").is_none());
    }

    #[test]
    fn test_get_tc_variable_missing() {
        assert!(get_tc_variable("<html></html>", "web-authorize-session-id").is_none());
    }

    #[test]
    fn test_extract_cs_session_id() {
        let html = r#"<form method="POST">
            <input type="text" name="password" value="">
            <input type="hidden" name="cs_session_id" value="46d71594ad2713f2a6a9d9cc06e7fa4b">
        </form>"#;
        assert_eq!(
            extract_cs_session_id(html),
            Some("46d71594ad2713f2a6a9d9cc06e7fa4b".to_string())
        );
    }

    #[test]
    fn test_extract_cs_session_id_missing() {
        assert!(extract_cs_session_id("<html></html>").is_none());
    }
}
