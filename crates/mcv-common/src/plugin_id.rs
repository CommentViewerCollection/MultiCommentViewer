use serde::{Deserialize, Serialize};
use uuid::Uuid;

/// 物理プラグインID（DLLファイル）
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct PhysicalPluginId(Uuid);

impl PhysicalPluginId {
    pub fn new() -> Self {
        Self(Uuid::new_v4())
    }

    pub fn inner(&self) -> Uuid {
        self.0
    }

    pub fn from_uuid(uuid: Uuid) -> Self {
        Self(uuid)
    }
}

impl std::fmt::Display for PhysicalPluginId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

/// 論理プラグインID（plugin-helloで登録）
#[derive(Debug, Clone, Copy, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct LogicalPluginId(Uuid);

impl LogicalPluginId {
    pub fn new() -> Self {
        Self(Uuid::new_v4())
    }

    pub fn inner(&self) -> Uuid {
        self.0
    }

    pub fn from_uuid(uuid: Uuid) -> Self {
        Self(uuid)
    }
}

impl std::fmt::Display for LogicalPluginId {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(f, "{}", self.0)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_physical_plugin_id() {
        let id1 = PhysicalPluginId::new();
        let id2 = PhysicalPluginId::new();
        assert_ne!(id1, id2);

        let uuid = Uuid::new_v4();
        let id3 = PhysicalPluginId::from_uuid(uuid);
        assert_eq!(id3.inner(), uuid);

        let id_str = id1.to_string();
        assert!(!id_str.is_empty());
    }

    #[test]
    fn test_logical_plugin_id() {
        let id1 = LogicalPluginId::new();
        let id2 = LogicalPluginId::new();
        assert_ne!(id1, id2);

        let uuid = Uuid::new_v4();
        let id3 = LogicalPluginId::from_uuid(uuid);
        assert_eq!(id3.inner(), uuid);

        let id_str = id1.to_string();
        assert!(!id_str.is_empty());
    }

    #[test]
    fn test_serialization() {
        let physical_id = PhysicalPluginId::new();
        let json = serde_json::to_string(&physical_id).unwrap();
        let deserialized: PhysicalPluginId = serde_json::from_str(&json).unwrap();
        assert_eq!(physical_id, deserialized);

        let logical_id = LogicalPluginId::new();
        let json = serde_json::to_string(&logical_id).unwrap();
        let deserialized: LogicalPluginId = serde_json::from_str(&json).unwrap();
        assert_eq!(logical_id, deserialized);
    }
}
