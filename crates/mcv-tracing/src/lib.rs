use mcv_messages::{LogEntryPayload, Message, MessageDestination, MessageSource, MessageType};
use mcv_plugin_interface::PluginHost;
use std::collections::HashMap;
use std::sync::Arc;
use tracing_subscriber::{layer::SubscriberExt, util::SubscriberInitExt, EnvFilter, Layer};
use uuid::Uuid;

// エラーコンテキスト機能用の型定義

/// ソース位置情報
#[derive(Debug, Clone, serde::Serialize)]
pub struct SourceLocation {
    pub file: String,
    pub line: u32,
    pub module_path: String,
}

/// スタックフレーム
#[derive(Debug, Clone, serde::Serialize)]
pub struct StackFrame {
    pub symbol: Option<String>,
    pub filename: Option<String>,
    pub lineno: Option<u32>,
}

/// エラーコンテキスト情報
#[derive(Debug, Clone)]
pub struct ErrorContext {
    /// エラーメッセージ
    pub message: String,

    /// ソース位置情報
    pub source: SourceLocation,

    /// スタックトレース
    pub stacktrace: Vec<StackFrame>,

    /// カスタムフィールド
    pub fields: HashMap<String, serde_json::Value>,

    /// タイムスタンプ
    pub timestamp: i64,
}

/// mcv-tracing のエラー型
#[derive(Debug, thiserror::Error)]
pub struct TracingError {
    context: ErrorContext,
}

impl TracingError {
    pub fn new(context: ErrorContext) -> Self {
        Self { context }
    }

    pub fn context(&self) -> &ErrorContext {
        &self.context
    }
}

impl std::fmt::Display for TracingError {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        write!(
            f,
            "{} (at {}:{})",
            self.context.message, self.context.source.file, self.context.source.line
        )
    }
}

impl From<ErrorContext> for TracingError {
    fn from(context: ErrorContext) -> Self {
        Self::new(context)
    }
}

impl ErrorContext {
    /// 新しいErrorContextを作成
    pub fn new(message: String, file: &str, line: u32, module_path: &str) -> Self {
        Self {
            message,
            source: SourceLocation {
                file: file.to_string(),
                line,
                module_path: module_path.to_string(),
            },
            stacktrace: Self::capture_stacktrace(),
            fields: HashMap::new(),
            timestamp: chrono::Utc::now().timestamp_millis(),
        }
    }

    /// カスタムフィールドを追加
    pub fn add_field<V: Into<serde_json::Value>>(&mut self, key: &str, value: V) {
        self.fields.insert(key.to_string(), value.into());
    }

    /// スタックトレースをキャプチャ
    fn capture_stacktrace() -> Vec<StackFrame> {
        let bt = backtrace::Backtrace::new();
        bt.frames()
            .iter()
            .skip(3) // capture_context 自身のフレームをスキップ
            .take(10) // 最大10フレームまで
            .map(|frame| {
                let symbol = frame.symbols().first();
                StackFrame {
                    symbol: symbol.and_then(|s| s.name().map(|n| n.to_string())),
                    filename: symbol
                        .and_then(|s| s.filename().map(|p| p.display().to_string())),
                    lineno: symbol.and_then(|s| s.lineno()),
                }
            })
            .collect()
    }

    /// LogEntryPayloadに変換
    pub fn to_log_entry_payload(&self) -> LogEntryPayload {
        LogEntryPayload {
            level: "error".to_string(),
            message: self.message.clone(),
            context: Some(self.to_json_context()),
            connection_id: None,
            plugin_version: None,
            plugin_build_profile: None,
        }
    }

    /// JSONコンテキストに変換
    fn to_json_context(&self) -> serde_json::Value {
        let mut context = serde_json::Map::new();

        // ソース情報
        context.insert(
            "source".to_string(),
            serde_json::json!({
                "file": self.source.file,
                "line": self.source.line,
                "module_path": self.source.module_path,
            }),
        );

        // スタックトレース
        if !self.stacktrace.is_empty() {
            let frames: Vec<serde_json::Value> = self
                .stacktrace
                .iter()
                .map(|frame| {
                    serde_json::json!({
                        "symbol": frame.symbol,
                        "filename": frame.filename,
                        "lineno": frame.lineno,
                    })
                })
                .collect();
            context.insert("stacktrace".to_string(), serde_json::Value::Array(frames));
        }

        // カスタムフィールド
        for (key, value) in &self.fields {
            context.insert(key.clone(), value.clone());
        }

        serde_json::Value::Object(context)
    }
}

impl serde::Serialize for ErrorContext {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        use serde::ser::SerializeStruct;
        let mut state = serializer.serialize_struct("ErrorContext", 5)?;
        state.serialize_field("message", &self.message)?;
        state.serialize_field("source", &self.source)?;
        state.serialize_field("stacktrace", &self.stacktrace)?;
        state.serialize_field("fields", &self.fields)?;
        state.serialize_field("timestamp", &self.timestamp)?;
        state.end()
    }
}

/// 現在のコンテキストをキャプチャ
///
/// # 例
/// ```rust,ignore
/// use mcv_tracing::capture_context;
///
/// let ctx = capture_context!("Invalid value");
/// return Err(ctx.into());
/// ```
#[macro_export]
macro_rules! capture_context {
    ($msg:expr) => {
        $crate::ErrorContext::new($msg.to_string(), file!(), line!(), module_path!())
    };
    ($msg:expr, $($key:tt = $value:expr),+ $(,)?) => {{
        let mut ctx = $crate::ErrorContext::new($msg.to_string(), file!(), line!(), module_path!());
        $(
            ctx.add_field(stringify!($key), $value);
        )+
        ctx
    }};
}

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

    #[test]
    fn test_error_context_new() {
        let ctx = ErrorContext::new("Test error".to_string(), "test.rs", 42, "test::module");
        assert_eq!(ctx.message, "Test error");
        assert_eq!(ctx.source.file, "test.rs");
        assert_eq!(ctx.source.line, 42);
        assert_eq!(ctx.source.module_path, "test::module");
        assert!(ctx.stacktrace.len() > 0);
        assert!(ctx.fields.is_empty());
    }

    #[test]
    fn test_error_context_add_field() {
        let mut ctx = ErrorContext::new("Test error".to_string(), "test.rs", 42, "test::module");
        ctx.add_field("key1", "value1");
        ctx.add_field("key2", 123);
        ctx.add_field("key3", true);

        assert_eq!(ctx.fields.len(), 3);
        assert_eq!(ctx.fields.get("key1"), Some(&serde_json::Value::String("value1".to_string())));
        assert_eq!(ctx.fields.get("key2"), Some(&serde_json::Value::Number(123.into())));
        assert_eq!(ctx.fields.get("key3"), Some(&serde_json::Value::Bool(true)));
    }

    #[test]
    fn test_capture_context_macro() {
        let ctx = capture_context!("Test error");
        assert_eq!(ctx.message, "Test error");
        assert!(ctx.source.file.contains("lib.rs"));
    }

    #[test]
    fn test_capture_context_macro_with_fields() {
        let ctx = capture_context!("Test error", value = 42, name = "test");
        assert_eq!(ctx.message, "Test error");
        assert_eq!(ctx.fields.len(), 2);
        assert_eq!(ctx.fields.get("value"), Some(&serde_json::Value::Number(42.into())));
        assert_eq!(ctx.fields.get("name"), Some(&serde_json::Value::String("test".to_string())));
    }

    #[test]
    fn test_tracing_error_from_context() {
        let ctx = capture_context!("Test error");
        let err: TracingError = ctx.into();
        assert_eq!(err.context().message, "Test error");
    }

    #[test]
    fn test_error_context_to_log_entry_payload() {
        let ctx = capture_context!("Test error", field1 = "value1");
        let payload = ctx.to_log_entry_payload();

        assert_eq!(payload.level, "error");
        assert_eq!(payload.message, "Test error");
        assert!(payload.context.is_some());

        let context = payload.context.unwrap();
        assert!(context.get("source").is_some());
        assert!(context.get("stacktrace").is_some());
        assert!(context.get("field1").is_some());
    }

    #[test]
    fn test_error_context_serialize() {
        let ctx = capture_context!("Test error");
        let json = serde_json::to_value(&ctx).unwrap();

        assert!(json.get("message").is_some());
        assert!(json.get("source").is_some());
        assert!(json.get("stacktrace").is_some());
        assert!(json.get("fields").is_some());
        assert!(json.get("timestamp").is_some());
    }
}
