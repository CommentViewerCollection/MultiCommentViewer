use std::path::Path;

use tauri::WebviewWindow;

/// 指定ディレクトリへの書き込み可否を確認する。
/// テスト用ファイルを作成・削除することで確認し、成功した場合 true を返す。
pub(crate) fn is_dir_writable(dir: &Path) -> bool {
    let test_path = dir.join(".mcv_write_test");
    match std::fs::File::create(&test_path) {
        Ok(_) => {
            let _ = std::fs::remove_file(&test_path);
            true
        }
        Err(_) => false,
    }
}

/// 指定ディレクトリ以下の `.old` ファイルをサブフォルダも含めて全て削除する。
/// `.old` ファイルしか存在しなかったディレクトリは空になるため合わせて削除する。
/// 元から空だったディレクトリは削除しない。
/// 戻り値: このディレクトリ内（再帰含む）で `.old` ファイルを1件以上削除した場合 true。
pub(crate) fn cleanup_old_files(dir: &Path) -> bool {
    let Ok(entries) = std::fs::read_dir(dir) else {
        return false;
    };
    let mut deleted_something = false;
    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_dir() {
            let deleted_in_sub = cleanup_old_files(&path);
            if deleted_in_sub {
                // .old ファイルが削除された結果、空になっていれば削除する
                if std::fs::read_dir(&path).is_ok_and(|mut e| e.next().is_none()) {
                    let _ = std::fs::remove_dir(&path);
                }
                deleted_something = true;
            }
        } else if path.extension().is_some_and(|ext| ext == "old") {
            let _ = std::fs::remove_file(&path);
            deleted_something = true;
        }
    }
    deleted_something
}

/// タイトルバー領域（上部 50px）がいずれかのモニターと重なっているか確認する
pub(crate) fn is_title_bar_visible(
    monitors: &[tauri::Monitor],
    x: i32,
    y: i32,
    width: u32,
) -> bool {
    let title_bar_h = 50i32;
    for m in monitors {
        let mp = m.position();
        let ms = m.size();
        let m_right = mp.x + ms.width as i32;
        let m_bottom = mp.y + ms.height as i32;
        if x < m_right && (x + width as i32) > mp.x && y < m_bottom && (y + title_bar_h) > mp.y {
            return true;
        }
    }
    false
}

/// ウィンドウ状態を core.json にマージ保存する
///
/// 最大化中はサイズ・位置を保存しない（最大化解除後のサイズを維持するため）。
pub(crate) fn save_window_state(window: &WebviewWindow, settings_dir: &Path) {
    let core_json = settings_dir.join("core.json");
    let mut data: serde_json::Value = std::fs::read_to_string(&core_json)
        .ok()
        .and_then(|s| serde_json::from_str(&s).ok())
        .unwrap_or(serde_json::json!({}));

    let is_maximized = window.is_maximized().unwrap_or(false);
    data["window_maximized"] = serde_json::json!(is_maximized);

    if !is_maximized {
        if let (Ok(pos), Ok(size)) = (window.outer_position(), window.outer_size()) {
            data["window_x"] = serde_json::json!(pos.x);
            data["window_y"] = serde_json::json!(pos.y);
            data["window_width"] = serde_json::json!(size.width);
            data["window_height"] = serde_json::json!(size.height);
        }
    }

    if let Ok(json) = serde_json::to_string_pretty(&data) {
        let _ = std::fs::write(&core_json, json);
    }
}

/// core.json からウィンドウ状態を復元する（マルチモニター対応）
pub(crate) fn restore_window_state(window: &WebviewWindow, settings_dir: &Path) {
    let core_json = settings_dir.join("core.json");
    let Ok(json) = std::fs::read_to_string(&core_json) else {
        return;
    };
    let Ok(data): Result<serde_json::Value, _> = serde_json::from_str(&json) else {
        return;
    };

    let maximized = data
        .get("window_maximized")
        .and_then(|v| v.as_bool())
        .unwrap_or(false);
    if maximized {
        let _ = window.maximize();
        return;
    }

    let x = data
        .get("window_x")
        .and_then(|v| v.as_i64())
        .map(|v| v as i32);
    let y = data
        .get("window_y")
        .and_then(|v| v.as_i64())
        .map(|v| v as i32);
    let w = data
        .get("window_width")
        .and_then(|v| v.as_u64())
        .map(|v| v as u32);
    let h = data
        .get("window_height")
        .and_then(|v| v.as_u64())
        .map(|v| v as u32);

    if let (Some(x), Some(y), Some(w), Some(h)) = (x, y, w, h) {
        let monitors = window.available_monitors().unwrap_or_default();
        if monitors.is_empty() || is_title_bar_visible(&monitors, x, y, w) {
            let _ = window.set_size(tauri::Size::Physical(tauri::PhysicalSize {
                width: w,
                height: h,
            }));
            let _ =
                window.set_position(tauri::Position::Physical(tauri::PhysicalPosition { x, y }));
        }
        // タイトルバーが見えない場合（モニター切断等）はデフォルト位置のまま
    }
}

/// 書き込み制限エラーをネイティブのメッセージボックスで表示する（Windows 専用）。
#[cfg(windows)]
pub(crate) fn show_write_restricted_error(dir: &Path) {
    use windows_sys::Win32::UI::WindowsAndMessaging::{MessageBoxW, MB_ICONERROR, MB_OK};

    let title: Vec<u16> = "MultiCommentViewer - 起動エラー\0".encode_utf16().collect();
    let message = format!(
        "アプリケーションが書き込み制限のあるフォルダーに配置されています。\n\n\
        フォルダー: {}\n\n\
        デスクトップや「ドキュメント」など書き込みが可能なフォルダーへ移動してから再起動してください。",
        dir.display()
    );
    let message: Vec<u16> = message.encode_utf16().chain(std::iter::once(0)).collect();

    unsafe {
        MessageBoxW(
            std::ptr::null_mut(),
            message.as_ptr(),
            title.as_ptr(),
            MB_OK | MB_ICONERROR,
        );
    }
}
