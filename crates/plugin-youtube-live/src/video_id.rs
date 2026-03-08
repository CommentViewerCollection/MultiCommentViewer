//! YouTube動画ID抽出ユーティリティ
//!
//! YouTube動画URLまたは動画IDから11文字の動画IDを抽出します。

use once_cell::sync::Lazy;
use regex::Regex;

/// YouTube動画ID抽出
/// 対応:
/// - watch?v=ID(順不同)
/// — youtu.be/ID
/// - studio.youtube.com/video/ID/...
/// - ID単体
pub fn extract_video_id(input: &str) -> Option<String> {
    // 1. ID単体(11文字)
    static ID_ONLY: Lazy<Regex> = Lazy::new(|| Regex::new(r"^[A-Za-z0-9_-]{11}$").unwrap());

    if ID_ONLY.is_match(input) {
        return Some(input.to_string());
    }

    extract_video_id_from_url(input)
}

/// URLからのみYouTube動画IDを抽出する（ID単体は不可）
///
/// URL自動検出用。ドメインが含まれないと意図しない文字列に誤反応するため、
/// `extract_video_id` とは異なりID単体の入力は受け付けない。
pub fn extract_video_id_from_url(input: &str) -> Option<String> {
    static URL_RE: Lazy<Regex> = Lazy::new(|| {
        Regex::new(r"(?:v=|youtu\.be/|studio\.youtube\.com/video/)([A-Za-z0-9_-]{11})").unwrap()
    });

    URL_RE
        .captures(input)
        .and_then(|caps| caps.get(1))
        .map(|m| m.as_str().to_string())
}

#[cfg(test)]
mod tests {
    use super::extract_video_id;

    #[test]
    fn watch_url_with_extra_params() {
        let url = "https://www.youtube.com/watch?v=oPgxDDRw8DE&n=0";
        assert_eq!(extract_video_id(url), Some("oPgxDDRw8DE".to_string()));
    }

    #[test]
    fn watch_url_simple() {
        let url = "https://www.youtube.com/watch?v=oPgxDDRw8DE";
        assert_eq!(extract_video_id(url), Some("oPgxDDRw8DE".to_string()));
    }

    #[test]
    fn watch_url_param_order_changed() {
        let url = "https://www.youtube.com/watch?s=adg&v=oPgxDDRw8DE";
        assert_eq!(extract_video_id(url), Some("oPgxDDRw8DE".to_string()));
    }

    #[test]
    fn short_url() {
        let url = "https://youtu.be/oPgxDDRw8DE?si=LyR2FIsXDJ2217Ql";
        assert_eq!(extract_video_id(url), Some("oPgxDDRw8DE".to_string()));
    }

    #[test]
    fn id_only() {
        let id = "oPgxDDRw8DE";
        assert_eq!(extract_video_id(id), Some("oPgxDDRw8DE".to_string()));
    }

    #[test]
    fn invalid_id_length_short() {
        let input = "abc";
        assert_eq!(extract_video_id(input), None);
    }

    #[test]
    fn invalid_id_length_long() {
        let input = "oPgxDDRw8DE123";
        assert_eq!(extract_video_id(input), None);
    }

    #[test]
    fn no_video_id_in_url() {
        let url = "https://www.youtube.com/watch?x=123";
        assert_eq!(extract_video_id(url), None);
    }

    #[test]
    fn empty_string() {
        let input = "";
        assert_eq!(extract_video_id(input), None);
    }

    #[test]
    fn studio_url() {
        let url = "https://studio.youtube.com/video/bp_xG58XtLQ/livestreaming";
        assert_eq!(extract_video_id(url), Some("bp_xG58XtLQ".to_string()));
    }
}
