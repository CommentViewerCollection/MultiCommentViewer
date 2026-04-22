use serde::{Deserialize, Serialize};

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
}
