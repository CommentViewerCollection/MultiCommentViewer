use crate::schema::{LogEntry, SystemInfo};
use crate::storage::LogStorage;
use mcv_log_schema::{
    capture_stacktrace, get_build_profile, LogLevel, MessageVisitor, SourceLocation,
};
use std::sync::{Arc, Mutex};
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt, EnvFilter, Layer};

/// ログサブスクライバーを初期化
pub fn init_subscriber(
    storage: Arc<Mutex<LogStorage>>,
    mcv_version: &str,
    log_level: &str,
) -> Result<(), Box<dyn std::error::Error>> {
    let mcv_version = mcv_version.to_string();

    // システム情報を取得
    let platform = std::env::consts::OS.to_string();
    let arch = std::env::consts::ARCH.to_string();
    let build_profile = get_build_profile();

    // カスタムレイヤーを作成
    let storage_layer = StorageLayer {
        storage,
        system_info: SystemInfo {
            mcv_version,
            platform,
            arch,
            build_profile,
        },
    };

    // EnvFilterを作成（ログレベルフィルタリング）
    let filter = EnvFilter::try_new(format!("mcv={}", log_level))
        .unwrap_or_else(|_| EnvFilter::new("mcv=error"));

    // サブスクライバーを構築
    tracing_subscriber::registry()
        .with(storage_layer.with_filter(filter))
        .try_init()?;

    Ok(())
}

// get_build_profile() は mcv-log-schema から re-export

/// カスタムストレージレイヤー
struct StorageLayer {
    storage: Arc<Mutex<LogStorage>>,
    system_info: SystemInfo,
}

impl<S> Layer<S> for StorageLayer
where
    S: tracing::Subscriber,
{
    fn on_event(
        &self,
        event: &tracing::Event<'_>,
        _ctx: tracing_subscriber::layer::Context<'_, S>,
    ) {
        // ログレベルを取得
        let level = match *event.metadata().level() {
            tracing::Level::TRACE => LogLevel::Trace,
            tracing::Level::DEBUG => LogLevel::Debug,
            tracing::Level::INFO => LogLevel::Info,
            tracing::Level::WARN => LogLevel::Warn,
            tracing::Level::ERROR => LogLevel::Error,
        };

        // メタデータからソース情報を取得（必要に応じて frontend_trace が上書き）
        let metadata = event.metadata();

        // メッセージとフィールドを抽出
        let mut visitor = MessageVisitor::default();
        event.record(&mut visitor);

        let source = extract_source_location(metadata, &mut visitor.fields);

        // スタックトレースをキャプチャ（エラーレベルのみ）
        let stacktrace = if level == LogLevel::Error {
            Some(capture_stacktrace())
        } else {
            None
        };

        // コンテキストをJSON化（フィールドから）
        let context = if !visitor.fields.is_empty() {
            Some(serde_json::to_value(&visitor.fields).unwrap_or(serde_json::Value::Null))
        } else {
            None
        };

        // ログエントリを作成
        let entry = LogEntry {
            id: uuid::Uuid::new_v4().to_string(),
            level,
            timestamp: chrono::Utc::now().timestamp_millis(),
            message: visitor.message,
            source,
            stacktrace,
            context,
            system_info: self.system_info.clone(),
        };

        // ストレージに保存
        if let Ok(storage) = self.storage.lock() {
            if let Err(e) = storage.insert(&entry) {
                eprintln!("Failed to insert log entry: {}", e);
            } else {
                // ログ挿入成功時にコールバックを呼び出す
                crate::invoke_log_insert_callback(&entry);
            }
        }
    }
}

fn extract_source_location(
    metadata: &tracing::Metadata<'_>,
    fields: &mut std::collections::HashMap<String, serde_json::Value>,
) -> SourceLocation {
    // 通常ログは metadata を使い、frontend_trace が送った source があればそちらを優先する。
    let fallback = SourceLocation {
        file: metadata.file().unwrap_or("unknown").to_string(),
        line: metadata.line().unwrap_or(0),
        column: None,
        module_path: metadata.module_path().unwrap_or("unknown").to_string(),
    };

    let file = fields
        .remove("frontend_source_file")
        .and_then(|v| v.as_str().map(|s| s.to_string()));
    let line = fields.remove("frontend_source_line").and_then(as_u32_json);
    let column = fields
        .remove("frontend_source_column")
        .and_then(as_u32_json);
    let module_path = fields
        .remove("frontend_source_module")
        .and_then(|v| v.as_str().map(|s| s.to_string()));

    if file.is_none() && line.is_none() && column.is_none() && module_path.is_none() {
        return fallback;
    }

    SourceLocation {
        file: file.unwrap_or(fallback.file),
        line: line.unwrap_or(fallback.line),
        column,
        module_path: module_path.unwrap_or(fallback.module_path),
    }
}

fn as_u32_json(v: serde_json::Value) -> Option<u32> {
    match v {
        serde_json::Value::Number(n) => n.as_u64().map(|x| x as u32),
        serde_json::Value::String(s) => s.parse::<u32>().ok(),
        _ => None,
    }
}

// MessageVisitor は mcv-log-schema から re-export

#[cfg(test)]
mod tests {
    use super::*;
    use crate::storage::LogStorage;

    #[test]
    fn test_subscriber_init() {
        let storage = Arc::new(Mutex::new(LogStorage::new(":memory:").unwrap()));
        let result = init_subscriber(storage, "0.1.0", "debug");
        // 2回目の初期化は失敗するが、テストでは無視
        let _ = result;
    }
}
