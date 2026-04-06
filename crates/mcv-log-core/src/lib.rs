pub mod schema;
pub mod sender;
pub mod storage;
pub mod subscriber;

use std::path::{Path, PathBuf};
use std::sync::{Arc, Mutex, OnceLock};

// グローバルストレージインスタンス
static GLOBAL_STORAGE: OnceLock<Arc<Mutex<storage::LogStorage>>> = OnceLock::new();

// グローバルなログ挿入時コールバック
type LogInsertCallback = Arc<Mutex<Option<Box<dyn Fn(&schema::LogEntry) + Send + Sync>>>>;
static LOG_INSERT_CALLBACK: OnceLock<LogInsertCallback> = OnceLock::new();

/// ロガーを初期化
///
/// # Arguments
///
/// * `db_path` - ログデータベースのパス
/// * `mcv_version` - mcvのバージョン
///
/// # Returns
///
/// 成功時は`Ok(())`、失敗時はエラーメッセージ
pub fn init_logger<P: AsRef<Path>>(
    db_path: P,
    mcv_version: &str,
) -> Result<(), Box<dyn std::error::Error>> {
    // ログレベルを取得（feature フラグから）
    let log_level = get_log_level();

    // ストレージを作成
    let storage = Arc::new(Mutex::new(storage::LogStorage::new(db_path)?));

    // グローバルストレージに設定
    GLOBAL_STORAGE
        .set(storage.clone())
        .map_err(|_| "Storage already initialized")?;

    // サブスクライバーを初期化
    subscriber::init_subscriber(storage, mcv_version, log_level)?;

    println!("mcv-logger initialized with log level: {}", log_level);

    Ok(())
}

/// グローバルストレージを取得
pub fn get_storage() -> Arc<Mutex<storage::LogStorage>> {
    GLOBAL_STORAGE
        .get()
        .expect("Storage not initialized. Call init_logger first.")
        .clone()
}

/// ログ挿入時のコールバックを設定
pub fn set_log_insert_callback<F>(callback: F)
where
    F: Fn(&schema::LogEntry) + Send + Sync + 'static,
{
    let callback_holder = LOG_INSERT_CALLBACK.get_or_init(|| Arc::new(Mutex::new(None)));

    if let Ok(mut holder) = callback_holder.lock() {
        *holder = Some(Box::new(callback));
    }
}

/// ログ挿入時のコールバックを呼び出す（内部使用）
pub(crate) fn invoke_log_insert_callback(entry: &schema::LogEntry) {
    if let Some(callback_holder) = LOG_INSERT_CALLBACK.get() {
        if let Ok(holder) = callback_holder.lock() {
            if let Some(callback) = holder.as_ref() {
                callback(entry);
            }
        }
    }
}

/// ログレベルを取得（feature フラグから）
fn get_log_level() -> &'static str {
    #[cfg(feature = "alpha")]
    return "trace";

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "info";

    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), feature = "stable"))]
    return "error";

    // フィーチャーフラグが指定されていない場合
    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
    return "error";
}

/// パニックフックをインストールする。
///
/// パニック発生時に SQLite への書き込みを試み、失敗した場合は
/// `panic_log_path` に JSON 形式で書き出す。
pub fn install_panic_hook(panic_log_path: PathBuf, mcv_version: String) {
    let default_hook = std::panic::take_hook();
    std::panic::set_hook(Box::new(move |panic_info| {
        let entry = build_panic_log_entry(panic_info, &mcv_version);
        if !write_panic_to_sqlite(&entry) {
            if let Ok(json) = serde_json::to_string(&entry) {
                let _ = std::fs::write(&panic_log_path, json.as_bytes());
            }
        }
        default_hook(panic_info);
    }));
}

/// 前回クラッシュ時に書き出された `panic_log_path` を SQLite にインポートする。
///
/// ファイルが存在し、インポートに成功した場合は true を返してファイルを削除する。
/// `init_logger` を呼んだ後に実行すること。
pub fn recover_panic_log(panic_log_path: &Path) -> bool {
    if !panic_log_path.exists() {
        return false;
    }
    let content = match std::fs::read(panic_log_path) {
        Ok(c) => c,
        Err(e) => {
            tracing::warn!(
                target: "mcv::panic",
                path = %panic_log_path.display(),
                error = %e,
                "panic.log の読み込みに失敗しました"
            );
            return false;
        }
    };
    let entry: schema::LogEntry = match serde_json::from_slice(&content) {
        Ok(e) => e,
        Err(e) => {
            let raw = String::from_utf8_lossy(&content);
            tracing::warn!(
                target: "mcv::panic",
                path = %panic_log_path.display(),
                error = %e,
                raw_content = %raw,
                "panic.log のデシリアライズに失敗しました。破損ファイルを削除します"
            );
            let _ = std::fs::remove_file(panic_log_path);
            return false;
        }
    };
    let storage = match GLOBAL_STORAGE.get() {
        Some(s) => s,
        None => {
            tracing::error!(
                target: "mcv::panic",
                path = %panic_log_path.display(),
                "GLOBAL_STORAGE が未初期化です。init_logger を先に呼んでください"
            );
            return false;
        }
    };
    let storage = match storage.lock() {
        Ok(s) => s,
        Err(e) => {
            tracing::error!(
                target: "mcv::panic",
                path = %panic_log_path.display(),
                error = %e,
                "SQLite ミューテックスのロック取得に失敗しました"
            );
            return false;
        }
    };
    match storage.insert(&entry) {
        Ok(()) => {
            let _ = std::fs::remove_file(panic_log_path);
            true
        }
        Err(e) => {
            tracing::error!(
                target: "mcv::panic",
                path = %panic_log_path.display(),
                error = %e,
                "panic.log の SQLite への書き込みに失敗しました"
            );
            false
        }
    }
}

fn write_panic_to_sqlite(entry: &schema::LogEntry) -> bool {
    if let Some(storage) = GLOBAL_STORAGE.get() {
        // パニック中はミューテックスが毒化している可能性があるため try_lock を使用
        if let Ok(storage) = storage.try_lock() {
            return storage.insert(entry).is_ok();
        }
    }
    false
}

fn build_panic_log_entry(
    panic_info: &std::panic::PanicHookInfo<'_>,
    mcv_version: &str,
) -> schema::LogEntry {
    let message = if let Some(s) = panic_info.payload().downcast_ref::<&str>() {
        format!("panic: {}", s)
    } else if let Some(s) = panic_info.payload().downcast_ref::<String>() {
        format!("panic: {}", s)
    } else {
        "panic occurred (non-string payload)".to_string()
    };

    let (file, line, column) = if let Some(loc) = panic_info.location() {
        (loc.file().to_string(), loc.line(), Some(loc.column()))
    } else {
        ("unknown".to_string(), 0, None)
    };

    let stacktrace = capture_stacktrace();

    schema::LogEntry {
        id: uuid::Uuid::new_v4().to_string(),
        level: schema::LogLevel::Error,
        timestamp: chrono::Utc::now().timestamp_millis(),
        message,
        source: SourceLocation {
            file,
            line,
            column,
            module_path: "mcv::panic".to_string(),
        },
        stacktrace: Some(stacktrace),
        context: Some(serde_json::json!({ "panic": true })),
        system_info: schema::SystemInfo {
            mcv_version: mcv_version.to_string(),
            platform: std::env::consts::OS.to_string(),
            arch: std::env::consts::ARCH.to_string(),
            build_profile: get_build_profile().to_string(),
            plugin_version: None,
        },
    }
}

// 公開API

// Re-export from mcv-log-schema (for backward compatibility)
pub use mcv_log_schema::{
    capture_stacktrace, get_build_profile, LogLevel, MessageVisitor, SourceLocation, StackFrame,
};

// Re-export from internal modules
pub use schema::{LogEntry, SystemInfo};
pub use sender::{LogSenderActor, SendImmediately, SendUnsentLogs};
pub use storage::LogStorage;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_log_level() {
        let level = get_log_level();
        assert!(!level.is_empty());
    }

    #[test]
    fn test_init_logger() {
        let temp_dir = std::env::temp_dir();
        let db_path = temp_dir.join("test_logs.db");

        // 既存のDBを削除
        let _ = std::fs::remove_file(&db_path);

        let result = init_logger(&db_path, "0.1.0");
        // 2回目の初期化は失敗するが、テストでは許容
        let _ = result;

        // クリーンアップ
        let _ = std::fs::remove_file(&db_path);
    }

    #[test]
    fn test_capture_stacktrace() {
        let frames = capture_stacktrace();
        assert!(!frames.is_empty());
    }

    #[test]
    fn test_recover_panic_log_no_file() {
        let temp_dir = std::env::temp_dir();
        let path = temp_dir.join("mcv_test_recover_nonexistent.log");
        let _ = std::fs::remove_file(&path);

        let result = recover_panic_log(&path);

        assert!(!result);
    }

    #[test]
    fn test_recover_panic_log_invalid_json_deletes_file() {
        let temp_dir = std::env::temp_dir();
        let path = temp_dir.join("mcv_test_recover_invalid.log");

        // 破損したファイルを書き出す
        std::fs::write(&path, b"this is not valid json").unwrap();
        assert!(path.exists());

        let result = recover_panic_log(&path);

        // デシリアライズ失敗 → false かつファイルが削除される
        assert!(!result);
        assert!(!path.exists(), "破損した panic.log は削除されるべき");
    }

    // recover_panic_log の成功パスは GLOBAL_STORAGE がグローバル OnceLock のため
    // 単体テストでは注入できない。
    // ロジックの主要部分（parse → insert → delete）は storage::tests でカバー済み。
}
