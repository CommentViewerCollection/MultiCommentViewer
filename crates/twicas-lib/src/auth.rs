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
/// * `secret`      - 固定値（PlayerPage2.js の r(333) = "ngg71ob7okuk3ngk"）
///
/// # アルゴリズム（PlayerPage2.js モジュール 5012 から逆算）
/// hash_input = secret + timestamp_sec + METHOD + pathname_with_query + session_id + body
/// result = timestamp_sec + "." + sha256(hash_input).hex()
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
    use hex;
    use sha2::{Digest, Sha256};

    /// ブラウザ実測値との一致確認（データポイント1）
    /// 実測: 1773050557.ce26b76dd38ffcb2d02ff73e3348ac6ba81760be5eff45a22550a2d992f56ef9
    #[test]
    fn test_authorize_key_matches_browser_1() {
        let secret = "ngg71ob7okuk3ngk";
        let timestamp = 1773050557u64;
        let path =
            "/users/kv510k/latest-movie?pass=c1ebb4933e06ce5617483f665e26627c&__n=1773050557526";
        let session_id = "MjlmNDgwY2UxZGY4ZDM3NGJiN2IxYTVmNTU0NWJkNDI=:1775642556:c55da94f20a826df";
        let expected_hash = "ce26b76dd38ffcb2d02ff73e3348ac6ba81760be5eff45a22550a2d992f56ef9";

        let input = format!("{}{}{}{}{}", secret, timestamp, "GET", path, session_id);
        let mut hasher = Sha256::new();
        hasher.update(input.as_bytes());
        let hash = hex::encode(hasher.finalize());
        assert_eq!(hash, expected_hash);
    }

    /// ブラウザ実測値との一致確認（データポイント2）
    /// 実測: 1773054717.7e867bc9eac61a4e28f73078c6af9c72afa9930b4abe823d26953e4f418af1d3
    #[test]
    fn test_authorize_key_matches_browser_2() {
        let secret = "ngg71ob7okuk3ngk";
        let timestamp = 1773054717u64;
        let path =
            "/users/kv510k/latest-movie?pass=c1ebb4933e06ce5617483f665e26627c&__n=1773054717137";
        let session_id = "MjlmNDgwY2UxZGY4ZDM3NGJiN2IxYTVmNTU0NWJkNDI=:1775642556:c55da94f20a826df";
        let expected_hash = "7e867bc9eac61a4e28f73078c6af9c72afa9930b4abe823d26953e4f418af1d3";

        let input = format!("{}{}{}{}{}", secret, timestamp, "GET", path, session_id);
        let mut hasher = Sha256::new();
        hasher.update(input.as_bytes());
        let hash = hex::encode(hasher.finalize());
        assert_eq!(hash, expected_hash);
    }

    /// 出力フォーマットの確認
    #[test]
    fn test_generate_authorize_key_format() {
        let key = generate_authorize_key(
            "GET",
            "https://frontendapi.twitcasting.tv/users/test/latest-movie?__n=123",
            "session-id-abc",
            "",
            "ngg71ob7okuk3ngk",
        );
        let parts: Vec<&str> = key.splitn(2, '.').collect();
        assert_eq!(parts.len(), 2);
        assert!(parts[0].parse::<u64>().is_ok());
        assert_eq!(parts[1].len(), 64); // SHA256 hex
    }
}
