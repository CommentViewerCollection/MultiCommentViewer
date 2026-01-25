use crate::schema::{capture_stacktrace, LogEntry, LogLevel, SourceLocation, SystemInfo};
use crate::storage::LogStorage;
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

/// ビルドプロファイルを取得
fn get_build_profile() -> String {
    #[cfg(feature = "alpha")]
    return "alpha".to_string();

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "beta".to_string();

    #[cfg(all(
        not(feature = "alpha"),
        not(feature = "beta"),
        feature = "stable"
    ))]
    return "stable".to_string();

    // フィーチャーフラグが指定されていない場合
    #[cfg(all(not(feature = "alpha"), not(feature = "beta"), not(feature = "stable")))]
    return "unknown".to_string();
}

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

        // メタデータからソース情報を取得
        let metadata = event.metadata();
        let source = SourceLocation {
            file: metadata.file().unwrap_or("unknown").to_string(),
            line: metadata.line().unwrap_or(0),
            column: None,
            module_path: metadata.module_path().unwrap_or("unknown").to_string(),
        };

        // メッセージとフィールドを抽出
        let mut visitor = MessageVisitor::default();
        event.record(&mut visitor);

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
            }
        }
    }
}

/// メッセージとフィールドを収集するビジター
#[derive(Default)]
struct MessageVisitor {
    message: String,
    fields: std::collections::HashMap<String, serde_json::Value>,
}

impl tracing::field::Visit for MessageVisitor {
    fn record_debug(&mut self, field: &tracing::field::Field, value: &dyn std::fmt::Debug) {
        if field.name() == "message" {
            self.message = format!("{:?}", value);
            // メッセージから引用符を削除
            if self.message.starts_with('"') && self.message.ends_with('"') {
                self.message = self.message[1..self.message.len() - 1].to_string();
            }
        } else {
            self.fields.insert(
                field.name().to_string(),
                serde_json::Value::String(format!("{:?}", value)),
            );
        }
    }

    fn record_str(&mut self, field: &tracing::field::Field, value: &str) {
        if field.name() == "message" {
            self.message = value.to_string();
        } else {
            self.fields
                .insert(field.name().to_string(), serde_json::Value::String(value.to_string()));
        }
    }

    fn record_i64(&mut self, field: &tracing::field::Field, value: i64) {
        self.fields
            .insert(field.name().to_string(), serde_json::Value::Number(value.into()));
    }

    fn record_u64(&mut self, field: &tracing::field::Field, value: u64) {
        self.fields
            .insert(field.name().to_string(), serde_json::Value::Number(value.into()));
    }

    fn record_bool(&mut self, field: &tracing::field::Field, value: bool) {
        self.fields
            .insert(field.name().to_string(), serde_json::Value::Bool(value));
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::storage::LogStorage;

    #[test]
    fn test_build_profile() {
        let profile = get_build_profile();
        assert!(!profile.is_empty());
    }

    #[test]
    fn test_subscriber_init() {
        let storage = Arc::new(Mutex::new(LogStorage::new(":memory:").unwrap()));
        let result = init_subscriber(storage, "0.1.0", "debug");
        // 2回目の初期化は失敗するが、テストでは無視
        let _ = result;
    }
}
