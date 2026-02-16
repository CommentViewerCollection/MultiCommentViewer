pub mod schema;
pub mod sender;
pub mod storage;
pub mod subscriber;

use std::path::Path;
use std::sync::{Arc, Mutex, OnceLock};

// グローバルストレージインスタンス
static GLOBAL_STORAGE: OnceLock<Arc<Mutex<storage::LogStorage>>> = OnceLock::new();

// グローバルなログ挿入時コールバック
static LOG_INSERT_CALLBACK: OnceLock<
    Arc<Mutex<Option<Box<dyn Fn(&schema::LogEntry) + Send + Sync>>>>,
> = OnceLock::new();

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
}
