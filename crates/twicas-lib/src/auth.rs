//! TwitCasting API の認証キー生成

use sha2::{Digest, Sha256};
use std::time::{SystemTime, UNIX_EPOCH};
use url::Url;

/// JS版と同等の X-Web-AuthorizeKey を生成
///
/// # Arguments
/// * `method`      - HTTPメソッド（例: "GET"）
/// * `path_or_url` - "/users/..." 形式またはフルURL
/// * `session_id`  - X-Web-SessionId
/// * `body`        - リクエストボディ（GETなら ""）
/// * `secret`      - 固定値（例: "16nkbjlus302qi23"）
pub fn generate_authorize_key(
    method: &str,
    path_or_url: &str,
    session_id: &str,
    body: &str,
    secret: &str,
) -> String {
    let full_url = if path_or_url.starts_with('/') {
        format!("https://cas.st{}", path_or_url)
    } else {
        path_or_url.to_string()
    };

    let url = Url::parse(&full_url).expect("Invalid URL");

    let mut path_with_query = url.path().to_string();
    if let Some(q) = url.query() {
        path_with_query.push('?');
        path_with_query.push_str(q);
    }

    let timestamp = SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("Time error")
        .as_secs();

    let string_to_hash = format!(
        "{}{}{}{}{}{}",
        secret, timestamp, method, path_with_query, session_id, body
    );

    let mut hasher = Sha256::new();
    hasher.update(string_to_hash.as_bytes());
    let hash = hasher.finalize();

    format!("{}.{}", timestamp, hex::encode(hash))
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_generate_authorize_key_format() {
        let key = generate_authorize_key(
            "GET",
            "https://frontendapi.twitcasting.tv/users/test/latest-movie?__n=123",
            "session-id-abc",
            "",
            "16nkbjlus302qi23",
        );
        // "timestamp.hex64" の形式であること
        let parts: Vec<&str> = key.splitn(2, '.').collect();
        assert_eq!(parts.len(), 2);
        assert!(parts[0].parse::<u64>().is_ok());
        assert_eq!(parts[1].len(), 64); // SHA256 hex
    }
}
