use rusqlite::{Connection, Result as SqliteResult, params};
use serde::{Deserialize, Serialize};

use crate::CommentRow;

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UserInfo {
    pub user_id: String,
    pub display_name: Vec<mcv_messages::MessagePart>,
    pub avatar_url: Option<String>,
    pub badges: Vec<mcv_messages::ProviderBadge>,
    pub connection_id: String,
    pub comment_count: i64,
    pub last_seen: i64,
    pub is_site_ng: bool,
}

pub struct CommentStore {
    conn: Connection,
}

impl CommentStore {
    pub fn new() -> SqliteResult<Self> {
        let conn = Connection::open_in_memory()?;
        conn.execute_batch("
            CREATE TABLE IF NOT EXISTS comments (
                id TEXT PRIMARY KEY,
                user_id TEXT NOT NULL,
                user_name_json TEXT NOT NULL,
                text_json TEXT NOT NULL,
                timestamp INTEGER NOT NULL,
                connection_id TEXT NOT NULL,
                is_visible INTEGER NOT NULL DEFAULT 1,
                kind TEXT NOT NULL,
                avatar_url TEXT,
                badges_json TEXT NOT NULL DEFAULT '[]',
                replaces_id TEXT,
                amount_text TEXT
            );
            CREATE INDEX IF NOT EXISTS idx_comments_timestamp ON comments(timestamp DESC);
            CREATE INDEX IF NOT EXISTS idx_comments_user_id ON comments(user_id);
            CREATE INDEX IF NOT EXISTS idx_comments_connection_id ON comments(connection_id);

            CREATE TABLE IF NOT EXISTS users (
                user_id TEXT PRIMARY KEY,
                display_name_json TEXT NOT NULL,
                avatar_url TEXT,
                badges_json TEXT NOT NULL DEFAULT '[]',
                connection_id TEXT NOT NULL,
                comment_count INTEGER NOT NULL DEFAULT 0,
                last_seen INTEGER NOT NULL DEFAULT 0,
                is_site_ng INTEGER NOT NULL DEFAULT 0
            );
        ")?;
        Ok(Self { conn })
    }

    /// コメントを保存し、ユーザー情報をUPSERT
    pub fn insert_comment(&self, row: &CommentRow) -> SqliteResult<()> {
        let user_name_json = serde_json::to_string(&row.user_name).unwrap_or_default();
        let text_json = serde_json::to_string(&row.text).unwrap_or_default();
        let badges_json = serde_json::to_string(&row.badges).unwrap_or_else(|_| "[]".to_string());

        self.conn.execute(
            "INSERT OR REPLACE INTO comments
                (id, user_id, user_name_json, text_json, timestamp, connection_id,
                 is_visible, kind, avatar_url, badges_json, replaces_id, amount_text)
             VALUES (?1, ?2, ?3, ?4, ?5, ?6, ?7, ?8, ?9, ?10, ?11, ?12)",
            params![
                row.id,
                row.user_id,
                user_name_json,
                text_json,
                row.timestamp,
                row.connection_id,
                row.is_visible as i64,
                row.kind,
                row.avatar_url,
                badges_json,
                row.replaces_id,
                row.amount_text,
            ],
        )?;

        // ユーザー情報をUPSERT（コメント数と最終コメント時刻を更新）
        self.conn.execute(
            "INSERT INTO users (user_id, display_name_json, avatar_url, badges_json, connection_id, comment_count, last_seen)
             VALUES (?1, ?2, ?3, ?4, ?5, 1, ?6)
             ON CONFLICT(user_id) DO UPDATE SET
                display_name_json = excluded.display_name_json,
                avatar_url = excluded.avatar_url,
                badges_json = excluded.badges_json,
                connection_id = excluded.connection_id,
                comment_count = comment_count + 1,
                last_seen = excluded.last_seen",
            params![
                row.user_id,
                user_name_json,
                row.avatar_url,
                badges_json,
                row.connection_id,
                row.timestamp,
            ],
        )?;

        Ok(())
    }

    /// サイトNGフラグを立てる（delete-all-by-userイベント時）
    pub fn set_site_ng(&self, user_id: &str) -> SqliteResult<()> {
        self.conn.execute(
            "UPDATE users SET is_site_ng = 1 WHERE user_id = ?1",
            params![user_id],
        )?;
        Ok(())
    }

    /// コメント検索（全フィールドOR検索、大文字小文字を区別しない）
    pub fn search_comments(
        &self,
        query: &str,
        limit: usize,
        offset: usize,
    ) -> SqliteResult<Vec<CommentRow>> {
        let pattern = format!("%{}%", query);
        let mut stmt = self.conn.prepare(
            "SELECT id, user_id, user_name_json, text_json, timestamp, connection_id,
                    is_visible, kind, avatar_url, badges_json, replaces_id, amount_text
             FROM comments
             WHERE user_name_json LIKE ?1
                OR text_json LIKE ?1
                OR user_id LIKE ?1
                OR connection_id LIKE ?1
                OR amount_text LIKE ?1
             ORDER BY timestamp ASC
             LIMIT ?2 OFFSET ?3",
        )?;

        let rows = stmt.query_map(params![pattern, limit as i64, offset as i64], |row| {
            let user_name_json: String = row.get(2)?;
            let text_json: String = row.get(3)?;
            let badges_json: String = row.get(9)?;
            let is_visible: i64 = row.get(6)?;

            Ok(CommentRow {
                id: row.get(0)?,
                user_id: row.get(1)?,
                user_name: serde_json::from_str(&user_name_json).unwrap_or_default(),
                text: serde_json::from_str(&text_json).unwrap_or_default(),
                timestamp: row.get(4)?,
                connection_id: row.get(5)?,
                is_visible: is_visible != 0,
                kind: row.get(7)?,
                avatar_url: row.get(8)?,
                badges: serde_json::from_str(&badges_json).unwrap_or_default(),
                replaces_id: row.get(10)?,
                amount_text: row.get(11)?,
            })
        })?
        .collect::<SqliteResult<Vec<_>>>()?;

        Ok(rows)
    }

    /// ユーザー一覧を last_seen 降順で取得
    pub fn get_users(&self, limit: usize, offset: usize) -> SqliteResult<Vec<UserInfo>> {
        let mut stmt = self.conn.prepare(
            "SELECT user_id, display_name_json, avatar_url, badges_json,
                    connection_id, comment_count, last_seen, is_site_ng
             FROM users
             ORDER BY last_seen DESC
             LIMIT ?1 OFFSET ?2",
        )?;

        let users = stmt.query_map(params![limit as i64, offset as i64], |row| {
            let display_name_json: String = row.get(1)?;
            let badges_json: String = row.get(3)?;
            let is_site_ng: i64 = row.get(7)?;

            Ok(UserInfo {
                user_id: row.get(0)?,
                display_name: serde_json::from_str(&display_name_json).unwrap_or_default(),
                avatar_url: row.get(2)?,
                badges: serde_json::from_str(&badges_json).unwrap_or_default(),
                connection_id: row.get(4)?,
                comment_count: row.get(5)?,
                last_seen: row.get(6)?,
                is_site_ng: is_site_ng != 0,
            })
        })?
        .collect::<SqliteResult<Vec<_>>>()?;

        Ok(users)
    }
}
