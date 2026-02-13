use serde::{Deserialize, Serialize};

/// サイトID（静的識別子 "{site_name}_{static_uuid}"形式）
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct SiteId(String);

impl SiteId {
    /// 新しいサイトIDを作成
    ///
    /// # Arguments
    /// * `site_name` - サイト名（例: "dummy", "YouTubeLive"）
    /// * `uuid` - 静的UUID（実装時に生成したUUID文字列）
    pub fn new(site_name: &str, uuid: &str) -> Self {
        Self(format!("{}_{}", site_name, uuid))
    }

    /// 文字列スライスとして取得
    pub fn as_str(&self) -> &str {
        &self.0
    }

    /// 内部文字列を取得
    pub fn into_string(self) -> String {
        self.0
    }

    /// 文字列から直接作成（既に "{site_name}_{uuid}"形式の場合）
    pub fn from_string(s: String) -> Self {
        Self(s)
    }
}

impl std::fmt::Display for SiteId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

impl From<String> for SiteId {
    fn from(s: String) -> Self {
        Self::from_string(s)
    }
}

impl From<SiteId> for String {
    fn from(site_id: SiteId) -> Self {
        site_id.into_string()
    }
}

impl AsRef<str> for SiteId {
    fn as_ref(&self) -> &str {
        &self.0
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_site_id_creation() {
        let site_id = SiteId::new("dummy", "00000000-0000-0000-0000-000000000001");
        assert_eq!(site_id.as_str(), "dummy_00000000-0000-0000-0000-000000000001");
    }

    #[test]
    fn test_site_id_from_string() {
        let site_id = SiteId::from_string("YouTubeLive_12345678-1234-1234-1234-123456789012".to_string());
        assert_eq!(site_id.as_str(), "YouTubeLive_12345678-1234-1234-1234-123456789012");
    }

    #[test]
    fn test_site_id_display() {
        let site_id = SiteId::new("test", "abc123");
        assert_eq!(format!("{}", site_id), "test_abc123");
    }

    #[test]
    fn test_site_id_serialization() {
        let site_id = SiteId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let json = serde_json::to_string(&site_id).unwrap();
        let deserialized: SiteId = serde_json::from_str(&json).unwrap();
        assert_eq!(site_id, deserialized);
    }

    #[test]
    fn test_site_id_equality() {
        let id1 = SiteId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let id2 = SiteId::new("dummy", "00000000-0000-0000-0000-000000000001");
        let id3 = SiteId::new("other", "00000000-0000-0000-0000-000000000001");

        assert_eq!(id1, id2);
        assert_ne!(id1, id3);
    }

    #[test]
    fn test_site_id_from_into() {
        let original = "dummy_00000000-0000-0000-0000-000000000001".to_string();
        let site_id: SiteId = original.clone().into();
        let back: String = site_id.into();
        assert_eq!(original, back);
    }
}
