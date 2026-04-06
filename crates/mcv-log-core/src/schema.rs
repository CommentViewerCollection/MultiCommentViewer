pub use mcv_log_schema::{LogLevel, SourceLocation, StackFrame};
use serde::{Deserialize, Serialize};

/// ログエントリ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LogEntry {
    /// ログID（UUID）
    pub id: String,

    /// ログレベル
    pub level: LogLevel,

    /// タイムスタンプ（Unix timestamp、ミリ秒）
    pub timestamp: i64,

    /// メッセージ
    pub message: String,

    /// ソースコード情報
    pub source: SourceLocation,

    /// スタックトレース（エラー時のみ）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub stacktrace: Option<Vec<StackFrame>>,

    /// コンテキスト情報（構造化データ）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub context: Option<serde_json::Value>,

    /// システム情報
    pub system_info: SystemInfo,
}

// SourceLocation, StackFrame, LogLevel, capture_stacktrace は mcv-log-schema から re-export

/// システム情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SystemInfo {
    /// mcvバージョン
    pub mcv_version: String,

    /// プラットフォーム（"windows", "linux", "macos"）
    pub platform: String,

    /// アーキテクチャ（"x86_64", "aarch64"）
    pub arch: String,

    /// ビルドプロファイル（"alpha", "beta", "stable"）
    pub build_profile: String,

    /// プラグインバージョン（プラグイン起源のログのみ）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub plugin_version: Option<String>,
}

// capture_stacktrace() は mcv-log-schema から re-export

#[cfg(test)]
mod tests {
    use super::*;
    use mcv_log_schema::capture_stacktrace;

    #[test]
    fn test_log_level_serialization() {
        let level = LogLevel::Error;
        let json = serde_json::to_string(&level).unwrap();
        assert_eq!(json, r#""error""#);

        let deserialized: LogLevel = serde_json::from_str(&json).unwrap();
        assert_eq!(deserialized, LogLevel::Error);
    }

    #[test]
    fn test_log_level_from_str() {
        assert_eq!("error".parse::<LogLevel>().unwrap(), LogLevel::Error);
        assert_eq!("ERROR".parse::<LogLevel>().unwrap(), LogLevel::Error);
        assert_eq!("warn".parse::<LogLevel>().unwrap(), LogLevel::Warn);
        assert!("invalid".parse::<LogLevel>().is_err());
    }

    #[test]
    fn test_stacktrace_capture() {
        let frames = capture_stacktrace();
        assert!(!frames.is_empty());
    }

    #[test]
    fn test_log_entry_serialization() {
        let entry = LogEntry {
            id: "test-id".to_string(),
            level: LogLevel::Error,
            timestamp: 1705766400000,
            message: "Test error".to_string(),
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
                plugin_version: None,
            },
        };

        let json = serde_json::to_string(&entry).unwrap();
        let deserialized: LogEntry = serde_json::from_str(&json).unwrap();

        assert_eq!(deserialized.id, entry.id);
        assert_eq!(deserialized.level, entry.level);
        assert_eq!(deserialized.message, entry.message);
    }
}
