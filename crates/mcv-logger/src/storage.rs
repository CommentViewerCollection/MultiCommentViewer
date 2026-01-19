use crate::schema::{LogEntry, SourceLocation, SystemInfo};
#[cfg(test)]
use crate::schema::LogLevel;
use rusqlite::{params, Connection, Result as SqliteResult};
use std::path::Path;

/// ログストレージ（SQLite）
pub struct LogStorage {
    conn: Connection,
}

impl LogStorage {
    /// 新しいログストレージを作成
    pub fn new<P: AsRef<Path>>(db_path: P) -> SqliteResult<Self> {
        let conn = Connection::open(db_path)?;

        // テーブル作成
        conn.execute(
            "CREATE TABLE IF NOT EXISTS logs (
                id TEXT PRIMARY KEY,
                level TEXT NOT NULL,
                timestamp INTEGER NOT NULL,
                message TEXT NOT NULL,
                file TEXT NOT NULL,
                line INTEGER NOT NULL,
                column INTEGER,
                module_path TEXT NOT NULL,
                stacktrace TEXT,
                context TEXT,
                mcv_version TEXT NOT NULL,
                platform TEXT NOT NULL,
                arch TEXT NOT NULL,
                build_profile TEXT NOT NULL,
                sent INTEGER DEFAULT 0,
                created_at INTEGER DEFAULT (strftime('%s','now'))
            )",
            [],
        )?;

        // インデックス作成
        conn.execute(
            "CREATE INDEX IF NOT EXISTS idx_timestamp ON logs(timestamp DESC)",
            [],
        )?;

        conn.execute(
            "CREATE INDEX IF NOT EXISTS idx_sent ON logs(sent)",
            [],
        )?;

        conn.execute(
            "CREATE INDEX IF NOT EXISTS idx_level ON logs(level)",
            [],
        )?;

        Ok(Self { conn })
    }

    /// ログエントリを挿入
    pub fn insert(&self, entry: &LogEntry) -> SqliteResult<()> {
        self.conn.execute(
            "INSERT INTO logs (
                id, level, timestamp, message, file, line, column, module_path,
                stacktrace, context, mcv_version, platform, arch, build_profile
            ) VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11, ?12, ?13, ?14)",
            params![
                entry.id,
                entry.level.to_string(),
                entry.timestamp,
                entry.message,
                entry.source.file,
                entry.source.line,
                entry.source.column,
                entry.source.module_path,
                entry
                    .stacktrace
                    .as_ref()
                    .map(|st| serde_json::to_string(st).unwrap()),
                entry
                    .context
                    .as_ref()
                    .map(|c| serde_json::to_string(c).unwrap()),
                entry.system_info.mcv_version,
                entry.system_info.platform,
                entry.system_info.arch,
                entry.system_info.build_profile,
            ],
        )?;

        Ok(())
    }

    /// 未送信のログエントリを取得
    pub fn get_unsent(&self, limit: usize) -> SqliteResult<Vec<LogEntry>> {
        let mut stmt = self.conn.prepare(
            "SELECT id, level, timestamp, message, file, line, column, module_path,
                    stacktrace, context, mcv_version, platform, arch, build_profile
             FROM logs
             WHERE sent = 0
             ORDER BY timestamp ASC
             LIMIT ?1",
        )?;

        let entries = stmt
            .query_map([limit], |row| {
                Ok(LogEntry {
                    id: row.get(0)?,
                    level: row.get::<_, String>(1)?.parse().unwrap(),
                    timestamp: row.get(2)?,
                    message: row.get(3)?,
                    source: SourceLocation {
                        file: row.get(4)?,
                        line: row.get(5)?,
                        column: row.get(6)?,
                        module_path: row.get(7)?,
                    },
                    stacktrace: row
                        .get::<_, Option<String>>(8)?
                        .and_then(|s| serde_json::from_str(&s).ok()),
                    context: row
                        .get::<_, Option<String>>(9)?
                        .and_then(|s| serde_json::from_str(&s).ok()),
                    system_info: SystemInfo {
                        mcv_version: row.get(10)?,
                        platform: row.get(11)?,
                        arch: row.get(12)?,
                        build_profile: row.get(13)?,
                    },
                })
            })?
            .collect::<SqliteResult<Vec<_>>>()?;

        Ok(entries)
    }

    /// ログを送信済みとしてマーク
    pub fn mark_as_sent(&self, ids: &[String]) -> SqliteResult<()> {
        if ids.is_empty() {
            return Ok(());
        }

        let placeholders = ids.iter().map(|_| "?").collect::<Vec<_>>().join(",");
        let query = format!("UPDATE logs SET sent = 1 WHERE id IN ({})", placeholders);

        self.conn
            .execute(&query, rusqlite::params_from_iter(ids))?;

        Ok(())
    }

    /// 古いログを削除（最大件数を超えた分）
    pub fn cleanup_old_logs(&self, max_count: usize) -> SqliteResult<usize> {
        self.conn.execute(
            "DELETE FROM logs WHERE id NOT IN (
                SELECT id FROM logs ORDER BY timestamp DESC LIMIT ?1
            )",
            [max_count],
        )
    }

    /// ログの総数を取得
    pub fn count(&self) -> SqliteResult<usize> {
        let count: usize = self
            .conn
            .query_row("SELECT COUNT(*) FROM logs", [], |row| row.get(0))?;
        Ok(count)
    }

    /// 未送信ログの数を取得
    pub fn count_unsent(&self) -> SqliteResult<usize> {
        let count: usize = self
            .conn
            .query_row("SELECT COUNT(*) FROM logs WHERE sent = 0", [], |row| {
                row.get(0)
            })?;
        Ok(count)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn create_test_entry(id: &str, level: LogLevel, message: &str) -> LogEntry {
        LogEntry {
            id: id.to_string(),
            level,
            timestamp: chrono::Utc::now().timestamp_millis(),
            message: message.to_string(),
            source: SourceLocation {
                file: "test.rs".to_string(),
                line: 42,
                column: Some(10),
                module_path: "test::module".to_string(),
            },
            stacktrace: None,
            context: Some(serde_json::json!({"key": "value"})),
            system_info: SystemInfo {
                mcv_version: "0.1.0".to_string(),
                platform: "windows".to_string(),
                arch: "x86_64".to_string(),
                build_profile: "alpha".to_string(),
            },
        }
    }

    #[test]
    fn test_storage_create() {
        let storage = LogStorage::new(":memory:").unwrap();
        assert_eq!(storage.count().unwrap(), 0);
    }

    #[test]
    fn test_storage_insert_and_retrieve() {
        let storage = LogStorage::new(":memory:").unwrap();

        let entry = create_test_entry("test-1", LogLevel::Error, "Test error");
        storage.insert(&entry).unwrap();

        assert_eq!(storage.count().unwrap(), 1);

        let unsent = storage.get_unsent(10).unwrap();
        assert_eq!(unsent.len(), 1);
        assert_eq!(unsent[0].id, "test-1");
        assert_eq!(unsent[0].message, "Test error");
    }

    #[test]
    fn test_storage_mark_as_sent() {
        let storage = LogStorage::new(":memory:").unwrap();

        let entry1 = create_test_entry("test-1", LogLevel::Error, "Error 1");
        let entry2 = create_test_entry("test-2", LogLevel::Warn, "Warning 1");

        storage.insert(&entry1).unwrap();
        storage.insert(&entry2).unwrap();

        assert_eq!(storage.count_unsent().unwrap(), 2);

        storage.mark_as_sent(&["test-1".to_string()]).unwrap();

        assert_eq!(storage.count_unsent().unwrap(), 1);

        let unsent = storage.get_unsent(10).unwrap();
        assert_eq!(unsent.len(), 1);
        assert_eq!(unsent[0].id, "test-2");
    }

    #[test]
    fn test_storage_cleanup() {
        let storage = LogStorage::new(":memory:").unwrap();

        // 5件挿入
        for i in 0..5 {
            let entry =
                create_test_entry(&format!("test-{}", i), LogLevel::Info, &format!("Message {}", i));
            storage.insert(&entry).unwrap();
        }

        assert_eq!(storage.count().unwrap(), 5);

        // 最新3件のみ残す
        let deleted = storage.cleanup_old_logs(3).unwrap();
        assert_eq!(deleted, 2);
        assert_eq!(storage.count().unwrap(), 3);
    }
}
