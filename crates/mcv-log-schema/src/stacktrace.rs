use crate::types::StackFrame;

/// ロギング・パニック基盤に属するシンボルプレフィックス。
/// これらに一致する先頭フレームをスキップし、ユーザーコードのフレームのみを残す。
const INTERNAL_PREFIXES: &[&str] = &[
    "mcv_log_schema::",
    "mcv_log_core::",
    "backtrace::",
    // パニックフック経由の場合に混入する標準ライブラリのパニック処理フレーム
    "std::panicking::",
    "core::panicking::",
];

/// フレームがロギング・パニック基盤に属するか判定する。
///
/// フレーム内のシンボルが全て内部プレフィックスに一致する場合 true を返す。
/// シンボルが空またはシンボル名が取得できない場合は false（スキップしない）を返す。
fn is_internal_frame(frame: &&backtrace::BacktraceFrame) -> bool {
    let symbols = frame.symbols();
    if symbols.is_empty() {
        // シンボルが解決できないフレームは内部フレームとみなさない
        return false;
    }
    symbols.iter().all(|sym| {
        sym.name()
            .map(|name| {
                let name_str = name.to_string();
                INTERNAL_PREFIXES
                    .iter()
                    .any(|prefix| name_str.starts_with(prefix))
            })
            // シンボル名が取得できない場合は内部フレームとみなさない
            .unwrap_or(false)
    })
}

/// スタックトレースをキャプチャする
///
/// ロギング・パニック基盤のフレーム（`mcv_log_schema`, `mcv_log_core`,
/// `backtrace`, `std::panicking`, `core::panicking`）を先頭からスキップし、
/// ユーザーコードのフレームを最大10件返す。
/// トレーシング経由・パニックフック経由のどちらの呼び出しパスでも
/// 正しくフィルタリングされる。
pub fn capture_stacktrace() -> Vec<StackFrame> {
    let bt = backtrace::Backtrace::new();
    bt.frames()
        .iter()
        .skip_while(is_internal_frame)
        .take(10)
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

    #[test]
    fn test_stacktrace_no_internal_frames_at_start() {
        let frames = capture_stacktrace();

        // 最初のフレームが内部モジュールでないことを確認
        if let Some(first) = frames.first() {
            if let Some(symbol) = &first.symbol {
                for prefix in INTERNAL_PREFIXES {
                    assert!(
                        !symbol.starts_with(prefix),
                        "先頭フレームが内部モジュール '{prefix}' から始まっている: {symbol}"
                    );
                }
            }
        }
    }
}
