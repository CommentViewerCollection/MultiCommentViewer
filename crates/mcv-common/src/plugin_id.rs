use serde::{Deserialize, Serialize};

/// 物理プラグインID（plugin.json の id フィールドに対応する文字列）
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct PhysicalPluginId(String);

impl PhysicalPluginId {
    pub fn from_id(id: impl AsRef<str>) -> Self {
        Self(id.as_ref().to_string())
    }

    pub fn as_str(&self) -> &str {
        &self.0
    }
}

impl std::fmt::Display for PhysicalPluginId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// 論理プラグインID（plugin-hello で送信するランタイム識別子）
///
/// 値は一意であれば何でも良い。推奨形式: "{PluginName}_logical_{uuid}"
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize, Default)]
pub struct PluginId(String);

impl PluginId {
    pub fn new(value: impl Into<String>) -> Self {
        Self(value.into())
    }

    pub fn as_str(&self) -> &str {
        &self.0
    }
}

impl std::fmt::Display for PluginId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_physical_plugin_id() {
        let id1 = PhysicalPluginId::from_id("chrome-cookie");
        let id2 = PhysicalPluginId::from_id("cookies-txt");
        assert_ne!(id1, id2);

        let id3 = PhysicalPluginId::from_id("chrome-cookie");
        assert_eq!(id1, id3);

        assert_eq!(id1.as_str(), "chrome-cookie");
        assert_eq!(id1.to_string(), "chrome-cookie");
    }

    #[test]
    fn test_plugin_id() {
        let id1 = PluginId::new("DummyPlugin_logical_550e8400-e29b-41d4-a716-446655440000");
        let id2 = PluginId::new("DummyPlugin_logical_661f9511-f3ac-52e5-b827-557766551111");
        assert_ne!(id1, id2);

        let id3 = PluginId::new("DummyPlugin_logical_550e8400-e29b-41d4-a716-446655440000");
        assert_eq!(id1, id3);

        assert_eq!(
            id1.as_str(),
            "DummyPlugin_logical_550e8400-e29b-41d4-a716-446655440000"
        );
    }

    #[test]
    fn test_serialization() {
        let physical_id = PhysicalPluginId::from_id("chrome-cookie");
        let json = serde_json::to_string(&physical_id).unwrap();
        let deserialized: PhysicalPluginId = serde_json::from_str(&json).unwrap();
        assert_eq!(physical_id, deserialized);

        let plugin_id = PluginId::new("DummyPlugin_logical_test-uuid");
        let json = serde_json::to_string(&plugin_id).unwrap();
        let deserialized: PluginId = serde_json::from_str(&json).unwrap();
        assert_eq!(plugin_id, deserialized);
    }
}
