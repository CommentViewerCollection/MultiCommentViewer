use mcv_messages::{LogEntryPayload, Message, MessageDestination, MessageSource, MessageType};
use mcv_plugin_interface::PluginHost;
use std::sync::Arc;
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt, EnvFilter, Layer};
use uuid::Uuid;

/// プラグイン用の tracing を初期化
///
/// この関数を呼び出すと、プラグインおよびその依存クレートで使用される tracing マクロが
/// 自動的に LogEntry メッセージとしてコアに送信されるようになります。
///
/// # 引数
/// * `plugin_id` - プラグインの UUID
/// * `plugin_host` - PluginHost の Arc 参照
/// * `plugin_version` - プラグインのバージョン文字列
/// * `log_level` - ログレベル ("trace", "debug", "info", "warn", "error")
///
/// # 例
/// ```rust,ignore
/// mcv_tracing::init_tracing(
///     self.plugin_id,
///     Arc::new(host),
///     env!("CARGO_PKG_VERSION"),
///     "info",
/// )?;
/// ```
pub fn init_tracing(
    plugin_id: Uuid,
    plugin_host: Arc<dyn PluginHost>,
    plugin_version: &str,
    log_level: &str,
) -> Result<(), Box<dyn std::error::Error>> {
    let plugin_version = plugin_version.to_string();
    let plugin_build_profile = get_build_profile();

    // カスタムレイヤーを作成
    let mcv_layer = McvTracingLayer {
        plugin_id,
        plugin_host,
        plugin_version,
        plugin_build_profile,
    };

    // EnvFilter を作成（ログレベルフィルタリング）
    let filter = EnvFilter::try_new(log_level).unwrap_or_else(|_| EnvFilter::new("error"));

    // サブスクライバーを構築
    tracing_subscriber::registry()
        .with(mcv_layer.with_filter(filter))
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
    "unknown".to_string()
}

/// カスタム tracing Layer
struct McvTracingLayer {
    plugin_id: Uuid,
    plugin_host: Arc<dyn PluginHost>,
    plugin_version: String,
    plugin_build_profile: String,
}

impl<S> Layer<S> for McvTracingLayer
where
    S: tracing::Subscriber,
{
    fn on_event(
        &self,
        event: &tracing::Event<'_>,
        _ctx: tracing_subscriber::layer::Context<'_, S>,
    ) {
        // ログレベルを文字列に変換
        let level = match *event.metadata().level() {
            tracing::Level::TRACE => "trace",
            tracing::Level::DEBUG => "debug",
            tracing::Level::INFO => "info",
            tracing::Level::WARN => "warn",
            tracing::Level::ERROR => "error",
        }
        .to_string();

        // メタデータからソース情報を取得
        let metadata = event.metadata();
        let source_info = serde_json::json!({
            "file": metadata.file().unwrap_or("unknown"),
            "line": metadata.line().unwrap_or(0),
            "module_path": metadata.module_path().unwrap_or("unknown"),
        });

        // メッセージとフィールドを抽出
        let mut visitor = MessageVisitor::default();
        event.record(&mut visitor);

        // コンテキストを構築
        let mut context_map = visitor.fields;
        context_map.insert("source".to_string(), source_info);

        // エラーレベルの場合はスタックトレースを追加
        if level == "error" {
            let stacktrace = capture_stacktrace_json();
            context_map.insert("stacktrace".to_string(), stacktrace);
        }

        let context = if !context_map.is_empty() {
            Some(serde_json::to_value(&context_map).unwrap_or(serde_json::Value::Null))
        } else {
            None
        };

        // LogEntryPayload を構築
        let payload = LogEntryPayload {
            level,
            message: visitor.message,
            context,
            connection_id: None, // スパンから取得する実装は今後追加可能
            plugin_version: Some(self.plugin_version.clone()),
            plugin_build_profile: Some(self.plugin_build_profile.clone()),
        };

        // メッセージを構築
        let message = Message::new_notification(
            MessageType::LogEntry,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(payload).unwrap_or(serde_json::Value::Null),
        );

        // 非同期でメッセージ送信（tokio::spawn）
        let host = Arc::clone(&self.plugin_host);
        tokio::spawn(async move {
            if let Err(e) = host.send_message(message).await {
                eprintln!("Failed to send log entry: {}", e);
            }
        });
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

/// スタックトレースを JSON として取得
fn capture_stacktrace_json() -> serde_json::Value {
    let bt = backtrace::Backtrace::new();
    let frames: Vec<serde_json::Value> = bt
        .frames()
        .iter()
        .skip(5) // tracing システム自身のフレームをスキップ
        .map(|frame| {
            let symbol = frame.symbols().first();
            serde_json::json!({
                "symbol": symbol.and_then(|s| s.name().map(|n| n.to_string())),
                "filename": symbol.and_then(|s| s.filename().map(|p| p.display().to_string())),
                "lineno": symbol.and_then(|s| s.lineno()),
                "addr": format!("{:p}", frame.ip()),
            })
        })
        .collect();

    serde_json::Value::Array(frames)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_build_profile() {
        let profile = get_build_profile();
        assert!(!profile.is_empty());
    }

    #[test]
    fn test_capture_stacktrace_json() {
        let stacktrace = capture_stacktrace_json();
        assert!(stacktrace.is_array());
    }
}
