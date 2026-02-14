use serde::{Deserialize, Serialize};

/// ブラウザID（静的識別子 "{browser_name}_{static_uuid}"形式）
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct BrowserId(String);

impl BrowserId {
    /// 新しいブラウザIDを作成
    ///
    /// # Arguments
    /// * `browser_name` - ブラウザ名（例: "Chrome", "FireFox"）
    /// * `uuid` - 静的UUID（実装時に生成したUUID文字列）
    pub fn new(browser_name: &str, uuid: &str) -> Self {
        Self(format!("{}_{}", browser_name, uuid))
    }

    /// 文字列スライスとして取得
    pub fn as_str(&self) -> &str {
        &self.0
    }

    /// 内部文字列を取得
    pub fn into_string(self) -> String {
        self.0
    }

    /// 文字列から直接作成（既に "{browser_name}_{uuid}"形式の場合）
    pub fn from_string(s: String) -> Self {
        Self(s)
    }
}

impl std::fmt::Display for BrowserId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

impl From<String> for BrowserId {
    fn from(s: String) -> Self {
        Self::from_string(s)
    }
}

impl From<BrowserId> for String {
    fn from(browser_id: BrowserId) -> Self {
        browser_id.into_string()
    }
}

impl AsRef<str> for BrowserId {
    fn as_ref(&self) -> &str {
        &self.0
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_browser_id_creation() {
        let browser_id = BrowserId::new("dummy", "00000000-0000-0000-0000-000000000001");
        assert_eq!(browser_id.as_str(), "dummy_00000000-0000-0000-0000-000000000001");
    }

    #[test]
    fn test_browser_id_from_string() {
        let browser_id = BrowserId::from_string("YouTubeLive_12345678-1234-1234-1234-123456789012".to_string());
        assert_eq!(browser_id.as_str(), "YouTubeLive_12345678-1234-1234-1234-123456789012");
    }

    #[test]
    fn test_browser_id_display() {
        let browser_id = BrowserId::new("test", "abc123");
        assert_eq!(format!("{}", browser_id), "test_abc123");
    }

    #[test]
    fn test_browser_id_serialization() {
        let browser_id = BrowserId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let json = serde_json::to_string(&browser_id).unwrap();
        let deserialized: BrowserId = serde_json::from_str(&json).unwrap();
        assert_eq!(browser_id, deserialized);
    }

    #[test]
    fn test_browser_id_equality() {
        let id1 = BrowserId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let id2 = BrowserId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let id3 = BrowserId::new("other", "00000000-0000-0000-0000-000000000001");

        assert_eq!(id1, id2);
        assert_ne!(id1, id3);
    }

    #[test]
    fn test_browser_id_from_into() {
        let original = "dummy_00000000-0000-0000-0000-000000000001".to_string();
        let browser_id: BrowserId = original.clone().into();
        let back: String = browser_id.into();
        assert_eq!(original, back);
    }
}
