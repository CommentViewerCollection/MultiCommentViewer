#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use mcv_logger::schema::LogEntry;
use rusqlite::{params, Connection, Result as SqliteResult};
use serde::{Deserialize, Serialize};
use std::path::PathBuf;

#[derive(Debug, Serialize, Deserialize)]
struct LogQueryFilters {
    level: Option<String>,
    from: Option<i64>,
    to: Option<i64>,
    search: Option<String>,
}

#[derive(Debug, Serialize)]
struct LogQueryResult {
    logs: Vec<LogEntry>,
    total: usize,
}

/// ローカルDBからログを取得
#[tauri::command]
async fn get_local_logs(
    filters: LogQueryFilters,
    limit: usize,
    offset: usize,
) -> Result<LogQueryResult, String> {
    let log_db_path = get_log_db_path()?;

    let conn = Connection::open(&log_db_path)
        .map_err(|e| format!("Failed to open database: {}", e))?;

    // Build WHERE clause
    let mut where_clauses = Vec::new();
    let mut params_vec: Vec<Box<dyn rusqlite::ToSql>> = Vec::new();

    if let Some(level) = &filters.level {
        where_clauses.push("level = ?".to_string());
        params_vec.push(Box::new(level.clone()));
    }

    if let Some(from) = filters.from {
        where_clauses.push("timestamp >= ?".to_string());
        params_vec.push(Box::new(from));
    }

    if let Some(to) = filters.to {
        where_clauses.push("timestamp <= ?".to_string());
        params_vec.push(Box::new(to));
    }

    if let Some(search) = &filters.search {
        where_clauses.push("(message LIKE ? OR file LIKE ? OR module_path LIKE ?)".to_string());
        let search_pattern = format!("%{}%", search);
        params_vec.push(Box::new(search_pattern.clone()));
        params_vec.push(Box::new(search_pattern.clone()));
        params_vec.push(Box::new(search_pattern));
    }

    let where_clause = if where_clauses.is_empty() {
        String::new()
    } else {
        format!("WHERE {}", where_clauses.join(" AND "))
    };

    // Get total count
    let count_query = format!("SELECT COUNT(*) FROM logs {}", where_clause);
    let total: usize = {
        let mut stmt = conn.prepare(&count_query)
            .map_err(|e| format!("Failed to prepare count query: {}", e))?;

        let params_refs: Vec<&dyn rusqlite::ToSql> = params_vec.iter().map(|b| b.as_ref()).collect();
        stmt.query_row(params_refs.as_slice(), |row| row.get(0))
            .map_err(|e| format!("Failed to get count: {}", e))?
    };

    // Get logs
    let query = format!(
        "SELECT id, level, timestamp, message, file, line, column, module_path, \
         stacktrace, context, mcv_version, platform, arch, build_profile \
         FROM logs {} ORDER BY timestamp DESC LIMIT ? OFFSET ?",
        where_clause
    );

    let mut stmt = conn.prepare(&query)
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    params_vec.push(Box::new(limit as i64));
    params_vec.push(Box::new(offset as i64));

    let params_refs: Vec<&dyn rusqlite::ToSql> = params_vec.iter().map(|b| b.as_ref()).collect();

    let logs = stmt.query_map(params_refs.as_slice(), |row| {
        let stacktrace_json: Option<String> = row.get(8)?;
        let stacktrace = stacktrace_json.and_then(|s| serde_json::from_str(&s).ok());

        let context_json: Option<String> = row.get(9)?;
        let context = context_json.and_then(|s| serde_json::from_str(&s).ok());

        Ok(LogEntry {
            id: row.get(0)?,
            level: serde_json::from_str(&format!("\"{}\"", row.get::<_, String>(1)?)).unwrap(),
            timestamp: row.get(2)?,
            message: row.get(3)?,
            source: mcv_logger::schema::SourceLocation {
                file: row.get(4)?,
                line: row.get(5)?,
                column: row.get(6)?,
                module_path: row.get(7)?,
            },
            stacktrace,
            context,
            system_info: mcv_logger::schema::SystemInfo {
                mcv_version: row.get(10)?,
                platform: row.get(11)?,
                arch: row.get(12)?,
                build_profile: row.get(13)?,
            },
        })
    })
    .map_err(|e| format!("Failed to query logs: {}", e))?
    .collect::<SqliteResult<Vec<_>>>()
    .map_err(|e| format!("Failed to collect logs: {}", e))?;

    Ok(LogQueryResult { logs, total })
}

/// ログDBのパスを取得
#[tauri::command]
fn get_log_db_path() -> Result<String, String> {
    let local_app_data = std::env::var("LOCALAPPDATA")
        .map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let db_path = PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("logs.db");

    Ok(db_path.to_string_lossy().to_string())
}

/// サーバーAPIからログを取得
#[tauri::command]
async fn get_server_logs(
    api_url: String,
    filters: LogQueryFilters,
    limit: usize,
    offset: usize,
) -> Result<LogQueryResult, String> {
    let client = reqwest::Client::new();

    let mut url = format!("{}/api/mcv/logs?limit={}&offset={}", api_url, limit, offset);

    if let Some(level) = &filters.level {
        url.push_str(&format!("&level={}", level));
    }
    if let Some(from) = filters.from {
        url.push_str(&format!("&from={}", from));
    }
    if let Some(to) = filters.to {
        url.push_str(&format!("&to={}", to));
    }
    if let Some(search) = &filters.search {
        url.push_str(&format!("&search={}", urlencoding::encode(search)));
    }

    let response = client.get(&url)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch logs from server: {}", e))?;

    if !response.status().is_success() {
        return Err(format!("Server returned error: {}", response.status()));
    }

    #[derive(Deserialize)]
    struct ServerResponse {
        logs: Vec<serde_json::Value>,
        total: usize,
    }

    let server_response: ServerResponse = response.json()
        .await
        .map_err(|e| format!("Failed to parse server response: {}", e))?;

    // Convert server logs to LogEntry
    let logs: Vec<LogEntry> = server_response.logs.into_iter()
        .filter_map(|log| serde_json::from_value(log).ok())
        .collect();

    Ok(LogQueryResult {
        logs,
        total: server_response.total,
    })
}

/// ログを削除（ローカルDB）
#[tauri::command]
async fn delete_local_logs(ids: Vec<String>) -> Result<usize, String> {
    let log_db_path = get_log_db_path()?;

    let conn = Connection::open(&log_db_path)
        .map_err(|e| format!("Failed to open database: {}", e))?;

    let placeholders = ids.iter().map(|_| "?").collect::<Vec<_>>().join(",");
    let query = format!("DELETE FROM logs WHERE id IN ({})", placeholders);

    let mut stmt = conn.prepare(&query)
        .map_err(|e| format!("Failed to prepare delete query: {}", e))?;

    let params: Vec<&dyn rusqlite::ToSql> = ids.iter().map(|id| id as &dyn rusqlite::ToSql).collect();

    let deleted = stmt.execute(params.as_slice())
        .map_err(|e| format!("Failed to delete logs: {}", e))?;

    Ok(deleted)
}

/// サーバーAPIでログを削除
#[tauri::command]
async fn delete_server_logs(api_url: String, ids: Vec<String>) -> Result<usize, String> {
    let client = reqwest::Client::new();

    let url = format!("{}/api/mcv/logs", api_url);

    #[derive(Serialize)]
    struct DeleteRequest {
        ids: Vec<String>,
    }

    let response = client.delete(&url)
        .json(&DeleteRequest { ids })
        .send()
        .await
        .map_err(|e| format!("Failed to delete logs from server: {}", e))?;

    if !response.status().is_success() {
        return Err(format!("Server returned error: {}", response.status()));
    }

    #[derive(Deserialize)]
    struct DeleteResponse {
        deleted: usize,
    }

    let delete_response: DeleteResponse = response.json()
        .await
        .map_err(|e| format!("Failed to parse delete response: {}", e))?;

    Ok(delete_response.deleted)
}

fn main() {
    tauri::Builder::default()
        .invoke_handler(tauri::generate_handler![
            get_local_logs,
            get_log_db_path,
            get_server_logs,
            delete_local_logs,
            delete_server_logs,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
