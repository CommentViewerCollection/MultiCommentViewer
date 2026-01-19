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

/// ログレベル
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "lowercase")]
pub enum LogLevel {
    Trace,
    Debug,
    Info,
    Warn,
    Error,
}

impl std::fmt::Display for LogLevel {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            LogLevel::Trace => write!(f, "trace"),
            LogLevel::Debug => write!(f, "debug"),
            LogLevel::Info => write!(f, "info"),
            LogLevel::Warn => write!(f, "warn"),
            LogLevel::Error => write!(f, "error"),
        }
    }
}

impl std::str::FromStr for LogLevel {
    type Err = String;

    fn from_str(s: &str) -> Result<Self, Self::Err> {
        match s.to_lowercase().as_str() {
            "trace" => Ok(LogLevel::Trace),
            "debug" => Ok(LogLevel::Debug),
            "info" => Ok(LogLevel::Info),
            "warn" => Ok(LogLevel::Warn),
            "error" => Ok(LogLevel::Error),
            _ => Err(format!("Invalid log level: {}", s)),
        }
    }
}

/// ソースコード位置情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SourceLocation {
    /// ファイル名
    pub file: String,

    /// 行番号
    pub line: u32,

    /// 列番号（オプション）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub column: Option<u32>,

    /// モジュールパス
    pub module_path: String,
}

/// スタックフレーム
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StackFrame {
    /// シンボル名
    #[serde(skip_serializing_if = "Option::is_none")]
    pub symbol: Option<String>,

    /// ファイル名
    #[serde(skip_serializing_if = "Option::is_none")]
    pub filename: Option<String>,

    /// 行番号
    #[serde(skip_serializing_if = "Option::is_none")]
    pub lineno: Option<u32>,

    /// アドレス
    pub addr: String,
}

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
}

/// スタックトレースをキャプチャする
pub fn capture_stacktrace() -> Vec<StackFrame> {
    let bt = backtrace::Backtrace::new();
    bt.frames()
        .iter()
        .skip(3) // ロギングシステム自身のフレームをスキップ
        .map(|frame| {
            let symbol = frame.symbols().first();
            StackFrame {
                symbol: symbol.and_then(|s| s.name().map(|n| n.to_string())),
                filename: symbol.and_then(|s| s.filename().map(|p| p.display().to_string())),
                lineno: symbol.and_then(|s| s.lineno()),
                addr: format!("{:p}", frame.ip()),
            }
        })
        .collect()
}

#[cfg(test)]
mod tests {
    use super::*;

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
            },
        };

        let json = serde_json::to_string(&entry).unwrap();
        let deserialized: LogEntry = serde_json::from_str(&json).unwrap();

        assert_eq!(deserialized.id, entry.id);
        assert_eq!(deserialized.level, entry.level);
        assert_eq!(deserialized.message, entry.message);
    }
}
