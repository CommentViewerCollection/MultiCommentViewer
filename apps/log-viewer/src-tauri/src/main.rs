#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use mcv_log_core::schema::LogEntry;
use rusqlite::{Connection, Result as SqliteResult};
use serde::{Deserialize, Serialize};

#[derive(Debug, Serialize, Deserialize)]
struct SearchFields {
    message: bool,
    #[serde(rename = "sourceLocation")]
    source_location: bool,
    context: bool,
    stacktrace: bool,
}

enum SearchMode {
    And,
    Or,
}

struct ParsedSearch {
    terms: Vec<String>,
    mode: SearchMode,
}

fn parse_search(search: &str) -> ParsedSearch {
    if search.contains(" OR ") {
        let terms = search
            .split(" OR ")
            .map(|t| t.trim().to_string())
            .filter(|t| !t.is_empty())
            .collect();
        ParsedSearch {
            terms,
            mode: SearchMode::Or,
        }
    } else {
        // 全角スペースを半角に正規化してから分割
        let normalized = search.replace('\u{3000}', " ");
        let terms = normalized
            .split(' ')
            .map(|t| t.trim().to_string())
            .filter(|t| !t.is_empty())
            .collect();
        ParsedSearch {
            terms,
            mode: SearchMode::And,
        }
    }
}

fn build_search_sql(
    parsed: &ParsedSearch,
    fields: &SearchFields,
    params_vec: &mut Vec<Box<dyn rusqlite::ToSql>>,
) -> Option<String> {
    if parsed.terms.is_empty() {
        return None;
    }

    let term_groups: Vec<String> = parsed
        .terms
        .iter()
        .map(|term| {
            let pattern = format!("%{}%", term);
            let mut field_clauses = Vec::new();

            if fields.message {
                field_clauses.push("message LIKE ?".to_string());
                params_vec.push(Box::new(pattern.clone()));
            }
            if fields.source_location {
                field_clauses.push("(file LIKE ? OR module_path LIKE ?)".to_string());
                params_vec.push(Box::new(pattern.clone()));
                params_vec.push(Box::new(pattern.clone()));
            }
            if fields.context {
                field_clauses.push("context LIKE ?".to_string());
                params_vec.push(Box::new(pattern.clone()));
            }
            if fields.stacktrace {
                field_clauses.push("stacktrace LIKE ?".to_string());
                params_vec.push(Box::new(pattern.clone()));
            }

            if field_clauses.is_empty() {
                "1=0".to_string()
            } else {
                format!("({})", field_clauses.join(" OR "))
            }
        })
        .collect();

    let joiner = match parsed.mode {
        SearchMode::And => " AND ",
        SearchMode::Or => " OR ",
    };
    Some(format!("({})", term_groups.join(joiner)))
}

#[derive(Debug, Serialize, Deserialize)]
struct LogQueryFilters {
    level: Option<String>,
    from: Option<i64>,
    to: Option<i64>,
    search: Option<String>,
    #[serde(rename = "searchFields")]
    search_fields: Option<SearchFields>,
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

    let conn =
        Connection::open(&log_db_path).map_err(|e| format!("Failed to open database: {}", e))?;

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
        let default_fields = SearchFields {
            message: true,
            source_location: true,
            context: true,
            stacktrace: true,
        };
        let fields = filters.search_fields.as_ref().unwrap_or(&default_fields);
        let parsed = parse_search(search);
        if let Some(sql) = build_search_sql(&parsed, fields, &mut params_vec) {
            where_clauses.push(sql);
        }
    }

    let where_clause = if where_clauses.is_empty() {
        String::new()
    } else {
        format!("WHERE {}", where_clauses.join(" AND "))
    };

    // Get total count
    let count_query = format!("SELECT COUNT(*) FROM logs {}", where_clause);
    let total: usize = {
        let mut stmt = conn
            .prepare(&count_query)
            .map_err(|e| format!("Failed to prepare count query: {}", e))?;

        let params_refs: Vec<&dyn rusqlite::ToSql> =
            params_vec.iter().map(|b| b.as_ref()).collect();
        let count: i64 = stmt
            .query_row(params_refs.as_slice(), |row| row.get(0))
            .map_err(|e| format!("Failed to get count: {}", e))?;
        count as usize
    };

    // Get logs
    let query = format!(
        "SELECT id, level, timestamp, message, file, line, column, module_path, \
         stacktrace, context, mcv_version, platform, arch, build_profile, plugin_version \
         FROM logs {} ORDER BY timestamp DESC LIMIT ? OFFSET ?",
        where_clause
    );

    let mut stmt = conn
        .prepare(&query)
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    params_vec.push(Box::new(limit as i64));
    params_vec.push(Box::new(offset as i64));

    let params_refs: Vec<&dyn rusqlite::ToSql> = params_vec.iter().map(|b| b.as_ref()).collect();

    let logs = stmt
        .query_map(params_refs.as_slice(), |row| {
            let stacktrace_json: Option<String> = row.get(8)?;
            let stacktrace = stacktrace_json.and_then(|s| serde_json::from_str(&s).ok());

            let context_json: Option<String> = row.get(9)?;
            let context = context_json.and_then(|s| serde_json::from_str(&s).ok());

            Ok(LogEntry {
                id: row.get(0)?,
                level: serde_json::from_str(&format!("\"{}\"", row.get::<_, String>(1)?)).unwrap(),
                timestamp: row.get(2)?,
                message: row.get(3)?,
                source: mcv_log_core::schema::SourceLocation {
                    file: row.get(4)?,
                    line: row.get(5)?,
                    column: row.get(6)?,
                    module_path: row.get(7)?,
                },
                stacktrace,
                context,
                system_info: mcv_log_core::schema::SystemInfo {
                    mcv_version: row.get(10)?,
                    platform: row.get(11)?,
                    arch: row.get(12)?,
                    build_profile: row.get(13)?,
                    plugin_version: row.get(14)?,
                },
            })
        })
        .map_err(|e| format!("Failed to query logs: {}", e))?
        .collect::<SqliteResult<Vec<_>>>()
        .map_err(|e| format!("Failed to collect logs: {}", e))?;

    Ok(LogQueryResult { logs, total })
}

/// デフォルトのログDBパスを返す（LOCALAPPDATA\MultiCommentViewer\logs\logs.db）
#[tauri::command]
fn get_log_db_path() -> Result<String, String> {
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;
    let db_path = std::path::PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("logs")
        .join("logs.db");
    Ok(db_path.to_string_lossy().to_string())
}

/// ファイルピッカーで任意のログDBファイルを選択し、パスを返す
#[tauri::command]
async fn select_log_db_path(app: tauri::AppHandle) -> Result<Option<String>, String> {
    use tauri_plugin_dialog::DialogExt;

    let default_dir = std::env::var("LOCALAPPDATA").ok().map(|d| {
        std::path::PathBuf::from(d)
            .join("MultiCommentViewer")
            .join("logs")
    });

    let (tx, rx) = tokio::sync::oneshot::channel();

    let mut builder = app
        .dialog()
        .file()
        .set_title("ログDBファイルを選択")
        .add_filter("SQLite Database", &["db"]);

    if let Some(dir) = default_dir {
        builder = builder.set_directory(dir);
    }

    builder.pick_file(move |path| {
        let _ = tx.send(path);
    });

    let path = rx.await.map_err(|e| format!("Dialog error: {}", e))?;

    Ok(path
        .and_then(|p| p.into_path().ok())
        .map(|p| p.to_string_lossy().to_string()))
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

    let response = client
        .get(&url)
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

    let server_response: ServerResponse = response
        .json()
        .await
        .map_err(|e| format!("Failed to parse server response: {}", e))?;

    // Convert server logs to LogEntry
    let logs: Vec<LogEntry> = server_response
        .logs
        .into_iter()
        .filter_map(|log| match serde_json::from_value(log.clone()) {
            Ok(entry) => Some(entry),
            Err(e) => {
                eprintln!("Failed to deserialize log entry: {}", e);
                eprintln!(
                    "Log data: {}",
                    serde_json::to_string_pretty(&log).unwrap_or_default()
                );
                None
            }
        })
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

    let conn =
        Connection::open(&log_db_path).map_err(|e| format!("Failed to open database: {}", e))?;

    let placeholders = ids.iter().map(|_| "?").collect::<Vec<_>>().join(",");
    let query = format!("DELETE FROM logs WHERE id IN ({})", placeholders);

    let mut stmt = conn
        .prepare(&query)
        .map_err(|e| format!("Failed to prepare delete query: {}", e))?;

    let params: Vec<&dyn rusqlite::ToSql> =
        ids.iter().map(|id| id as &dyn rusqlite::ToSql).collect();

    let deleted = stmt
        .execute(params.as_slice())
        .map_err(|e| format!("Failed to delete logs: {}", e))?;

    Ok(deleted)
}

/// ローカルDBからログをJSON形式でエクスポート
#[tauri::command]
async fn export_local_logs(
    app: tauri::AppHandle,
    filters: LogQueryFilters,
) -> Result<Option<String>, String> {
    use tauri_plugin_dialog::DialogExt;

    let today = chrono::Utc::now().format("%Y-%m-%d");
    let default_filename = format!("mcv-logs-{}.json", today);

    let (tx, rx) = tokio::sync::oneshot::channel();
    app.dialog()
        .file()
        .set_title("Export Logs")
        .set_file_name(&default_filename)
        .add_filter("JSON Files", &["json"])
        .save_file(move |path| {
            let _ = tx.send(path);
        });

    let path = rx.await.map_err(|e| format!("Dialog error: {}", e))?;

    let save_path = match path {
        Some(file_path) => file_path
            .into_path()
            .map_err(|_| "Invalid save path".to_string())?,
        None => return Ok(None),
    };

    let log_db_path = get_log_db_path()?;
    let conn =
        Connection::open(&log_db_path).map_err(|e| format!("Failed to open database: {}", e))?;

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
        let default_fields = SearchFields {
            message: true,
            source_location: true,
            context: true,
            stacktrace: true,
        };
        let fields = filters.search_fields.as_ref().unwrap_or(&default_fields);
        let parsed = parse_search(search);
        if let Some(sql) = build_search_sql(&parsed, fields, &mut params_vec) {
            where_clauses.push(sql);
        }
    }

    let where_clause = if where_clauses.is_empty() {
        String::new()
    } else {
        format!("WHERE {}", where_clauses.join(" AND "))
    };

    let query = format!(
        "SELECT id, level, timestamp, message, file, line, column, module_path, \
         stacktrace, context, mcv_version, platform, arch, build_profile, plugin_version \
         FROM logs {} ORDER BY timestamp DESC",
        where_clause
    );

    let mut stmt = conn
        .prepare(&query)
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    let params_refs: Vec<&dyn rusqlite::ToSql> = params_vec.iter().map(|b| b.as_ref()).collect();

    let logs = stmt
        .query_map(params_refs.as_slice(), |row| {
            let stacktrace_json: Option<String> = row.get(8)?;
            let stacktrace = stacktrace_json.and_then(|s| serde_json::from_str(&s).ok());

            let context_json: Option<String> = row.get(9)?;
            let context = context_json.and_then(|s| serde_json::from_str(&s).ok());

            Ok(LogEntry {
                id: row.get(0)?,
                level: serde_json::from_str(&format!("\"{}\"", row.get::<_, String>(1)?)).unwrap(),
                timestamp: row.get(2)?,
                message: row.get(3)?,
                source: mcv_log_core::schema::SourceLocation {
                    file: row.get(4)?,
                    line: row.get(5)?,
                    column: row.get(6)?,
                    module_path: row.get(7)?,
                },
                stacktrace,
                context,
                system_info: mcv_log_core::schema::SystemInfo {
                    mcv_version: row.get(10)?,
                    platform: row.get(11)?,
                    arch: row.get(12)?,
                    build_profile: row.get(13)?,
                    plugin_version: row.get(14)?,
                },
            })
        })
        .map_err(|e| format!("Failed to query logs: {}", e))?
        .collect::<SqliteResult<Vec<_>>>()
        .map_err(|e| format!("Failed to collect logs: {}", e))?;

    let json = serde_json::to_string_pretty(&logs)
        .map_err(|e| format!("Failed to serialize logs: {}", e))?;

    std::fs::write(&save_path, json).map_err(|e| format!("Failed to write file: {}", e))?;

    Ok(Some(save_path.to_string_lossy().to_string()))
}

/// サーバーAPIからログをJSON形式でエクスポート
#[tauri::command]
async fn export_server_logs(
    app: tauri::AppHandle,
    api_url: String,
    filters: LogQueryFilters,
) -> Result<Option<String>, String> {
    use tauri_plugin_dialog::DialogExt;

    let today = chrono::Utc::now().format("%Y-%m-%d");
    let default_filename = format!("mcv-logs-server-{}.json", today);

    let (tx, rx) = tokio::sync::oneshot::channel();
    app.dialog()
        .file()
        .set_title("Export Logs")
        .set_file_name(&default_filename)
        .add_filter("JSON Files", &["json"])
        .save_file(move |path| {
            let _ = tx.send(path);
        });

    let path = rx.await.map_err(|e| format!("Dialog error: {}", e))?;

    let save_path = match path {
        Some(file_path) => file_path
            .into_path()
            .map_err(|_| "Invalid save path".to_string())?,
        None => return Ok(None),
    };

    let client = reqwest::Client::new();
    let mut url = format!("{}/api/mcv/logs?limit=1000000&offset=0", api_url);

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

    let response = client
        .get(&url)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch logs from server: {}", e))?;

    if !response.status().is_success() {
        return Err(format!("Server returned error: {}", response.status()));
    }

    let logs: Vec<serde_json::Value> = response
        .json::<serde_json::Value>()
        .await
        .map_err(|e| format!("Failed to parse server response: {}", e))?
        .get("logs")
        .and_then(|v| v.as_array())
        .cloned()
        .unwrap_or_default();

    let json = serde_json::to_string_pretty(&logs)
        .map_err(|e| format!("Failed to serialize logs: {}", e))?;

    std::fs::write(&save_path, json).map_err(|e| format!("Failed to write file: {}", e))?;

    Ok(Some(save_path.to_string_lossy().to_string()))
}

/// ローカルDBからログをJSON文字列として返す（クリップボードコピー用）
#[tauri::command]
async fn get_local_logs_json(filters: LogQueryFilters) -> Result<String, String> {
    let log_db_path = get_log_db_path()?;
    let conn =
        Connection::open(&log_db_path).map_err(|e| format!("Failed to open database: {}", e))?;

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
        let default_fields = SearchFields {
            message: true,
            source_location: true,
            context: true,
            stacktrace: true,
        };
        let fields = filters.search_fields.as_ref().unwrap_or(&default_fields);
        let parsed = parse_search(search);
        if let Some(sql) = build_search_sql(&parsed, fields, &mut params_vec) {
            where_clauses.push(sql);
        }
    }

    let where_clause = if where_clauses.is_empty() {
        String::new()
    } else {
        format!("WHERE {}", where_clauses.join(" AND "))
    };

    let query = format!(
        "SELECT id, level, timestamp, message, file, line, column, module_path, \
         stacktrace, context, mcv_version, platform, arch, build_profile, plugin_version \
         FROM logs {} ORDER BY timestamp DESC",
        where_clause
    );

    let mut stmt = conn
        .prepare(&query)
        .map_err(|e| format!("Failed to prepare query: {}", e))?;

    let params_refs: Vec<&dyn rusqlite::ToSql> = params_vec.iter().map(|b| b.as_ref()).collect();

    let logs = stmt
        .query_map(params_refs.as_slice(), |row| {
            let stacktrace_json: Option<String> = row.get(8)?;
            let stacktrace = stacktrace_json.and_then(|s| serde_json::from_str(&s).ok());

            let context_json: Option<String> = row.get(9)?;
            let context = context_json.and_then(|s| serde_json::from_str(&s).ok());

            Ok(LogEntry {
                id: row.get(0)?,
                level: serde_json::from_str(&format!("\"{}\"", row.get::<_, String>(1)?)).unwrap(),
                timestamp: row.get(2)?,
                message: row.get(3)?,
                source: mcv_log_core::schema::SourceLocation {
                    file: row.get(4)?,
                    line: row.get(5)?,
                    column: row.get(6)?,
                    module_path: row.get(7)?,
                },
                stacktrace,
                context,
                system_info: mcv_log_core::schema::SystemInfo {
                    mcv_version: row.get(10)?,
                    platform: row.get(11)?,
                    arch: row.get(12)?,
                    build_profile: row.get(13)?,
                    plugin_version: row.get(14)?,
                },
            })
        })
        .map_err(|e| format!("Failed to query logs: {}", e))?
        .collect::<SqliteResult<Vec<_>>>()
        .map_err(|e| format!("Failed to collect logs: {}", e))?;

    serde_json::to_string_pretty(&logs).map_err(|e| format!("Failed to serialize logs: {}", e))
}

/// サーバーAPIからログをJSON文字列として返す（クリップボードコピー用）
#[tauri::command]
async fn get_server_logs_json(api_url: String, filters: LogQueryFilters) -> Result<String, String> {
    let client = reqwest::Client::new();
    let mut url = format!("{}/api/mcv/logs?limit=1000000&offset=0", api_url);

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

    let response = client
        .get(&url)
        .send()
        .await
        .map_err(|e| format!("Failed to fetch logs from server: {}", e))?;

    if !response.status().is_success() {
        return Err(format!("Server returned error: {}", response.status()));
    }

    let logs: Vec<serde_json::Value> = response
        .json::<serde_json::Value>()
        .await
        .map_err(|e| format!("Failed to parse server response: {}", e))?
        .get("logs")
        .and_then(|v| v.as_array())
        .cloned()
        .unwrap_or_default();

    serde_json::to_string_pretty(&logs).map_err(|e| format!("Failed to serialize logs: {}", e))
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

    let response = client
        .delete(&url)
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

    let delete_response: DeleteResponse = response
        .json()
        .await
        .map_err(|e| format!("Failed to parse delete response: {}", e))?;

    Ok(delete_response.deleted)
}

fn main() {
    tauri::Builder::default()
        .plugin(tauri_plugin_dialog::init())
        .invoke_handler(tauri::generate_handler![
            get_local_logs,
            get_log_db_path,
            select_log_db_path,
            get_server_logs,
            delete_local_logs,
            delete_server_logs,
            export_local_logs,
            export_server_logs,
            get_local_logs_json,
            get_server_logs_json,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
