use crate::types::StackFrame;

/// スタックトレースをキャプチャする
///
/// ロギングシステム自身のフレームをスキップし、最大10フレームまでキャプチャします。
pub fn capture_stacktrace() -> Vec<StackFrame> {
    let bt = backtrace::Backtrace::new();
    bt.frames()
        .iter()
        .skip(3) // ロギングシステム自身のフレームをスキップ
        .take(10) // 最大10フレームまで
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
    fn test_stacktrace_capture() {
        let frames = capture_stacktrace();
        assert!(!frames.is_empty());
        // 最大10フレームまで
        assert!(frames.len() <= 10);
    }
}
