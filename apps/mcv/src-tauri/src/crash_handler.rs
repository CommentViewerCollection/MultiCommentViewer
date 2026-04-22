/// Windows SEH ハンドラと診断ユーティリティ
///
/// - `install()`: `SetUnhandledExceptionFilter` で SEH ハンドラを設置し、
///   Rust の `panic!` では捕捉できない例外（0xe0000008 等）を
///   `crash_seh_<unix_ts>.txt` として `app_data_dir` に保存する。
///
/// - `get_process_memory_mb()`: Working Set サイズ (MB) を返す。
///   ロギング用。
use std::path::{Path, PathBuf};

static SEH_CRASH_DIR: std::sync::OnceLock<PathBuf> = std::sync::OnceLock::new();

/// SEH ハンドラを設置する。`main()` の最初期（パニックフック直後）に呼ぶこと。
pub fn install(app_data_dir: &Path) {
    #[cfg(windows)]
    {
        SEH_CRASH_DIR.set(app_data_dir.to_path_buf()).ok();
        unsafe {
            windows_sys::Win32::System::Diagnostics::Debug::SetUnhandledExceptionFilter(Some(
                seh_handler,
            ));
        }
    }
    let _ = app_data_dir; // cfg(not(windows)) で警告を抑制
}

/// 現在のプロセスの Working Set サイズを MB 単位で返す。取得失敗時は `None`。
pub fn get_process_memory_mb() -> Option<u64> {
    #[cfg(windows)]
    {
        use windows_sys::Win32::System::ProcessStatus::{
            K32GetProcessMemoryInfo, PROCESS_MEMORY_COUNTERS,
        };
        use windows_sys::Win32::System::Threading::GetCurrentProcess;
        unsafe {
            let mut pmc: PROCESS_MEMORY_COUNTERS = std::mem::zeroed();
            pmc.cb = std::mem::size_of::<PROCESS_MEMORY_COUNTERS>() as u32;
            let proc = GetCurrentProcess();
            if K32GetProcessMemoryInfo(proc, &mut pmc, pmc.cb) != 0 {
                return Some(pmc.WorkingSetSize as u64 / (1024 * 1024));
            }
        }
    }
    None
}

#[cfg(windows)]
unsafe extern "system" fn seh_handler(
    info: *const windows_sys::Win32::System::Diagnostics::Debug::EXCEPTION_POINTERS,
) -> i32 {
    use windows_sys::Win32::System::Threading::GetCurrentThreadId;

    let crash_dir = match SEH_CRASH_DIR.get() {
        Some(d) => d,
        None => return 0, // EXCEPTION_CONTINUE_SEARCH
    };

    let exception_code: u32 = if !info.is_null() {
        let record = (*info).ExceptionRecord;
        if !record.is_null() {
            (*record).ExceptionCode as u32
        } else {
            0
        }
    } else {
        0
    };

    let ts = std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .unwrap_or_default()
        .as_secs();

    let path = crash_dir.join(format!("crash_seh_{}.txt", ts));

    let mem_mb = get_process_memory_mb()
        .map(|m| format!("{} MB", m))
        .unwrap_or_else(|| "unknown".to_string());

    let thread_id = GetCurrentThreadId();

    let content = format!(
        "SEH unhandled exception\nexception_code: {:#010x}\nworking_set: {}\nthread_id: {}\n",
        exception_code, mem_mb, thread_id,
    );

    let _ = std::fs::write(&path, &content);

    // tracing が動いていれば SQLite にも残す（クラッシュ直前なので best-effort）
    tracing::error!(
        target: "mcv::crash",
        exception_code = format!("{:#010x}", exception_code),
        working_set = %mem_mb,
        thread_id = thread_id,
        crash_file = %path.display(),
        "SEH unhandled exception"
    );

    0 // EXCEPTION_CONTINUE_SEARCH — WER に引き渡してダンプを生成させる
}
