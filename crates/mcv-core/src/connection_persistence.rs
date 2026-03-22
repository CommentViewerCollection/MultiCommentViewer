use mcv_common::{BrowserId, SiteId};
use serde::{Deserialize, Serialize};
use std::path::Path;
use uuid::Uuid;

use crate::connection_manager::ConnectionManager;

/// 永続化用の接続データ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PersistedConnection {
    pub connection_id: Uuid,
    pub site_id: Option<SiteId>,
    pub url: Option<String>,
    /// ブラウザID。旧形式ファイルとの互換のため省略可。
    #[serde(default)]
    pub browser_id: Option<BrowserId>,
    pub advanced_settings: Option<serde_json::Value>,
    /// 接続時入力フォームの最後の値（URL以外のサイト固有入力も含む）
    #[serde(default)]
    pub input_state: Option<serde_json::Value>,
    /// コメント投稿フォームの最後の値（text 以外のサイト固有入力を想定）
    #[serde(default)]
    pub comment_state: Option<serde_json::Value>,
    pub name: String,
}

/// 接続ストレージのルート構造
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionsStorage {
    pub schema_version: String,
    pub connections: Vec<PersistedConnection>,
}

impl ConnectionsStorage {
    /// 空のストレージを作成
    pub fn new() -> Self {
        Self {
            schema_version: "1.1".to_string(),
            connections: Vec::new(),
        }
    }

    /// ファイルから読み込み（ファイルが存在しない場合は空のストレージを返す）
    pub fn load_from_file(path: &Path) -> Result<Self, String> {
        tracing::debug!(
            target: "mcv::core::connection_persistence",
            path = ?path,
            "Loading connections from file"
        );

        // ファイルが存在しない場合は空のストレージを返す
        if !path.exists() {
            tracing::info!(
                target: "mcv::core::connection_persistence",
                path = ?path,
                "Connections file does not exist, returning empty storage"
            );
            return Ok(Self::new());
        }

        // ファイルを読み込み
        let file_content = std::fs::read_to_string(path)
            .map_err(|e| format!("Failed to read connections file: {}", e))?;

        // JSONをパース
        let storage: ConnectionsStorage = serde_json::from_str(&file_content).map_err(|e| {
            // パースエラー時はファイル内容をログに含める
            tracing::error!(
                target: "mcv::core::connection_persistence",
                path = ?path,
                file_content = %file_content,
                error = %e,
                "Failed to parse connections.json (file content included for debugging)"
            );
            format!("Failed to parse connections.json: {}", e)
        })?;

        tracing::info!(
            target: "mcv::core::connection_persistence",
            path = ?path,
            connection_count = storage.connections.len(),
            schema_version = %storage.schema_version,
            "Connections loaded successfully"
        );

        Ok(storage)
    }

    /// ファイルに保存
    pub fn save_to_file(&self, path: &Path) -> Result<(), String> {
        tracing::debug!(
            target: "mcv::core::connection_persistence",
            path = ?path,
            connection_count = self.connections.len(),
            "Saving connections to file"
        );

        // 親ディレクトリを作成
        if let Some(parent) = path.parent() {
            std::fs::create_dir_all(parent)
                .map_err(|e| format!("Failed to create parent directory: {}", e))?;
        }

        // JSONにシリアライズ（pretty print）
        let json = serde_json::to_string_pretty(self)
            .map_err(|e| format!("Failed to serialize connections: {}", e))?;

        // ファイルに書き込み
        std::fs::write(path, json)
            .map_err(|e| format!("Failed to write connections file: {}", e))?;

        tracing::info!(
            target: "mcv::core::connection_persistence",
            path = ?path,
            connection_count = self.connections.len(),
            "Connections saved successfully"
        );

        Ok(())
    }

    /// ConnectionManagerの現在状態から作成
    pub fn from_connection_manager(manager: &ConnectionManager) -> Self {
        let connections = manager.export_for_persistence();
        Self {
            schema_version: "1.1".to_string(),
            connections,
        }
    }
}

impl Default for ConnectionsStorage {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::io::Write;
    use tempfile::NamedTempFile;

    #[test]
    fn test_save_and_load_round_trip() {
        let temp_file = NamedTempFile::new().unwrap();
        let path = temp_file.path();

        // 保存
        let storage = ConnectionsStorage {
            schema_version: "1.1".to_string(),
            connections: vec![PersistedConnection {
                connection_id: Uuid::new_v4(),
                site_id: Some(SiteId::new(
                    "TestSite",
                    "00000000-0000-0000-0000-000000000001",
                )),
                url: Some("https://example.com".to_string()),
                browser_id: Some(BrowserId::new(
                    "Chrome",
                    "00000000-0000-0000-0000-000000000002",
                )),
                advanced_settings: Some(serde_json::json!({"key": "value"})),
                input_state: Some(serde_json::json!({"url": "https://example.com/live"})),
                comment_state: Some(serde_json::json!({"emote": "smile"})),
                name: "Test Connection".to_string(),
            }],
        };

        storage.save_to_file(path).unwrap();

        // 読み込み
        let loaded = ConnectionsStorage::load_from_file(path).unwrap();

        assert_eq!(loaded.schema_version, "1.1");
        assert_eq!(loaded.connections.len(), 1);
        assert_eq!(loaded.connections[0].name, "Test Connection");
        assert_eq!(
            loaded.connections[0].site_id,
            Some(SiteId::new(
                "TestSite",
                "00000000-0000-0000-0000-000000000001"
            ))
        );
    }

    #[test]
    fn test_load_nonexistent_file() {
        let temp_dir = tempfile::tempdir().unwrap();
        let path = temp_dir.path().join("nonexistent.json");

        let result = ConnectionsStorage::load_from_file(&path);

        assert!(result.is_ok());
        let storage = result.unwrap();
        assert_eq!(storage.connections.len(), 0);
    }

    #[test]
    fn test_load_invalid_json() {
        let mut temp_file = NamedTempFile::new().unwrap();
        writeln!(temp_file, "{{invalid json").unwrap();

        let result = ConnectionsStorage::load_from_file(temp_file.path());

        assert!(result.is_err());
    }

    #[test]
    fn test_default() {
        let storage = ConnectionsStorage::default();
        assert_eq!(storage.schema_version, "1.1");
        assert_eq!(storage.connections.len(), 0);
    }
}
