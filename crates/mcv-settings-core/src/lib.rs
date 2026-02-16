use rusqlite::{Connection, Result as SqliteResult};
use serde::{Deserialize, Serialize};
use std::path::Path;

/// 設定エントリ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SettingsEntry {
    pub target: String,            // "core" または plugin_id
    pub schema: serde_json::Value, // JSON Schema
    pub data: serde_json::Value,   // 設定値
    pub updated_at: i64,           // Unix timestamp
}

/// 設定ストレージ
pub struct SettingsStorage {
    conn: Connection,
}

impl SettingsStorage {
    /// 新しいストレージを作成または開く
    pub fn new<P: AsRef<Path>>(db_path: P) -> SqliteResult<Self> {
        let conn = Connection::open(db_path)?;

        // テーブル作成
        conn.execute(
            "CREATE TABLE IF NOT EXISTS settings (
                target TEXT PRIMARY KEY,
                schema TEXT NOT NULL,
                data TEXT NOT NULL,
                updated_at INTEGER NOT NULL
            )",
            [],
        )?;

        Ok(Self { conn })
    }

    /// 設定を保存（INSERT OR REPLACE）
    pub fn save_settings(&self, entry: &SettingsEntry) -> SqliteResult<()> {
        let schema_str = serde_json::to_string(&entry.schema)
            .map_err(|e| rusqlite::Error::ToSqlConversionFailure(Box::new(e)))?;
        let data_str = serde_json::to_string(&entry.data)
            .map_err(|e| rusqlite::Error::ToSqlConversionFailure(Box::new(e)))?;

        self.conn.execute(
            "INSERT OR REPLACE INTO settings (target, schema, data, updated_at)
             VALUES (?1, ?2, ?3, ?4)",
            rusqlite::params![&entry.target, schema_str, data_str, entry.updated_at],
        )?;

        Ok(())
    }

    /// 設定を取得
    pub fn get_settings(&self, target: &str) -> SqliteResult<Option<SettingsEntry>> {
        let mut stmt = self
            .conn
            .prepare("SELECT target, schema, data, updated_at FROM settings WHERE target = ?1")?;

        let mut rows = stmt.query(rusqlite::params![target])?;

        if let Some(row) = rows.next()? {
            let target: String = row.get(0)?;
            let schema_str: String = row.get(1)?;
            let data_str: String = row.get(2)?;
            let updated_at: i64 = row.get(3)?;

            let schema = serde_json::from_str(&schema_str).map_err(|e| {
                rusqlite::Error::FromSqlConversionFailure(
                    1,
                    rusqlite::types::Type::Text,
                    Box::new(e),
                )
            })?;
            let data = serde_json::from_str(&data_str).map_err(|e| {
                rusqlite::Error::FromSqlConversionFailure(
                    2,
                    rusqlite::types::Type::Text,
                    Box::new(e),
                )
            })?;

            Ok(Some(SettingsEntry {
                target,
                schema,
                data,
                updated_at,
            }))
        } else {
            Ok(None)
        }
    }

    /// すべての設定を取得
    pub fn list_all_settings(&self) -> SqliteResult<Vec<SettingsEntry>> {
        let mut stmt = self
            .conn
            .prepare("SELECT target, schema, data, updated_at FROM settings")?;

        let rows = stmt.query_map([], |row| {
            let target: String = row.get(0)?;
            let schema_str: String = row.get(1)?;
            let data_str: String = row.get(2)?;
            let updated_at: i64 = row.get(3)?;

            let schema = serde_json::from_str(&schema_str).map_err(|e| {
                rusqlite::Error::FromSqlConversionFailure(
                    1,
                    rusqlite::types::Type::Text,
                    Box::new(e),
                )
            })?;
            let data = serde_json::from_str(&data_str).map_err(|e| {
                rusqlite::Error::FromSqlConversionFailure(
                    2,
                    rusqlite::types::Type::Text,
                    Box::new(e),
                )
            })?;

            Ok(SettingsEntry {
                target,
                schema,
                data,
                updated_at,
            })
        })?;

        rows.collect()
    }

    /// 設定を削除
    pub fn delete_settings(&self, target: &str) -> SqliteResult<()> {
        self.conn.execute(
            "DELETE FROM settings WHERE target = ?1",
            rusqlite::params![target],
        )?;

        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_settings_storage_crud() {
        // テスト用の一時DBを作成
        let storage = SettingsStorage::new(":memory:").unwrap();

        // 保存
        let entry = SettingsEntry {
            target: "core".to_string(),
            schema: serde_json::json!({
                "type": "object",
                "properties": {
                    "theme": {
                        "type": "string",
                        "default": "dark"
                    }
                }
            }),
            data: serde_json::json!({
                "theme": "dark"
            }),
            updated_at: 1234567890,
        };

        storage.save_settings(&entry).unwrap();

        // 取得
        let retrieved = storage.get_settings("core").unwrap().unwrap();
        assert_eq!(retrieved.target, "core");
        assert_eq!(retrieved.data["theme"], "dark");

        // 更新
        let updated_entry = SettingsEntry {
            target: "core".to_string(),
            schema: entry.schema.clone(),
            data: serde_json::json!({
                "theme": "light"
            }),
            updated_at: 1234567900,
        };

        storage.save_settings(&updated_entry).unwrap();

        let retrieved = storage.get_settings("core").unwrap().unwrap();
        assert_eq!(retrieved.data["theme"], "light");
        assert_eq!(retrieved.updated_at, 1234567900);

        // 全件取得
        let all = storage.list_all_settings().unwrap();
        assert_eq!(all.len(), 1);

        // 削除
        storage.delete_settings("core").unwrap();
        let retrieved = storage.get_settings("core").unwrap();
        assert!(retrieved.is_none());
    }
}
