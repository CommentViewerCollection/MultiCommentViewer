#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use mcv_updater::{InstallerUpdateInfo, McvUpdateInfo, PluginListItem, UpdateChecker};
use std::path::PathBuf;
use sysinfo::System;
use tauri::{AppHandle, Emitter, Manager, State};

#[cfg(windows)]
use winreg::enums::*;
#[cfg(windows)]
use winreg::RegKey;

use once_cell::sync::Lazy;
use std::sync::Mutex;

/// グローバル変数: アンインストール対象を保持
static UNINSTALL_TARGET: Lazy<Mutex<Option<String>>> = Lazy::new(|| Mutex::new(None));

/// アプリケーションの状態
struct AppState {
    update_checker: UpdateChecker,
}

/// インストーラ自身の更新をチェック
#[tauri::command]
async fn check_installer_update(
    state: State<'_, AppState>,
) -> Result<Option<InstallerUpdateInfo>, String> {
    const CURRENT_VERSION: &str = env!("CARGO_PKG_VERSION");

    println!(
        "Checking for installer updates... Current version: {}",
        CURRENT_VERSION
    );

    match state
        .update_checker
        .check_installer_update(CURRENT_VERSION)
        .await
    {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                println!(
                    "Installer update available: {} -> {}",
                    CURRENT_VERSION, info.version
                );
            } else {
                println!("No installer update available");
            }
            Ok(update_info)
        }
        Err(e) => {
            eprintln!("Failed to check for installer updates: {}", e);
            Err(format!("Failed to check for installer updates: {}", e))
        }
    }
}

/// mcv本体の更新をチェック
#[tauri::command]
async fn check_mcv_update(
    state: State<'_, AppState>,
    current_version: String,
) -> Result<Option<McvUpdateInfo>, String> {
    println!(
        "Checking for mcv updates... Current version: {}",
        current_version
    );

    match state
        .update_checker
        .check_mcv_update(&current_version)
        .await
    {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                println!(
                    "Mcv update available: {} -> {}",
                    current_version, info.version
                );
            } else {
                println!("No mcv update available");
            }
            Ok(update_info)
        }
        Err(e) => {
            eprintln!("Failed to check for mcv updates: {}", e);
            Err(format!("Failed to check for mcv updates: {}", e))
        }
    }
}

/// プラグイン一覧を取得
#[tauri::command]
async fn list_plugins(state: State<'_, AppState>) -> Result<Vec<PluginListItem>, String> {
    println!("Fetching plugin list...");

    match state.update_checker.list_plugins().await {
        Ok(plugins) => {
            println!("Found {} plugins", plugins.len());
            Ok(plugins)
        }
        Err(e) => {
            eprintln!("Failed to fetch plugin list: {}", e);
            Err(format!("Failed to fetch plugin list: {}", e))
        }
    }
}

/// ファイルをダウンロード
#[tauri::command]
async fn download_file(
    url: String,
    dest: String,
    app_handle: AppHandle,
    state: State<'_, AppState>,
) -> Result<(), String> {
    println!("Downloading from: {}", url);
    println!("Saving to: {}", dest);

    let dest_path = PathBuf::from(&dest);

    state
        .update_checker
        .download(&url, &dest_path, |downloaded, total| {
            let progress = if total > 0 {
                (downloaded as f64 / total as f64 * 100.0) as u64
            } else {
                0
            };

            // 進捗イベントを発行
            let _ = app_handle.emit(
                "download-progress",
                serde_json::json!({
                    "url": &url,
                    "downloaded": downloaded,
                    "total": total,
                    "progress": progress
                }),
            );
        })
        .await
        .map_err(|e| format!("Download failed: {}", e))?;

    println!("Download completed: {}", dest);
    Ok(())
}

/// チェックサムを検証
#[tauri::command]
async fn verify_checksum(
    file_path: String,
    expected: String,
    state: State<'_, AppState>,
) -> Result<bool, String> {
    println!("Verifying checksum for: {}", file_path);

    let path = PathBuf::from(&file_path);

    state
        .update_checker
        .verify_checksum(&path, &expected)
        .await
        .map_err(|e| format!("Checksum verification failed: {}", e))
}

/// mcvをインストール（ZIPファイル展開）
#[tauri::command]
async fn install_mcv(zip_path: String, dest_dir: String) -> Result<(), String> {
    println!("Installing mcv from: {}", zip_path);
    println!("Destination: {}", dest_dir);

    let zip_file =
        std::fs::File::open(&zip_path).map_err(|e| format!("Failed to open ZIP file: {}", e))?;

    let mut archive =
        zip::ZipArchive::new(zip_file).map_err(|e| format!("Failed to read ZIP archive: {}", e))?;

    let dest_path = PathBuf::from(&dest_dir);

    // ディレクトリが存在しない場合は作成
    std::fs::create_dir_all(&dest_path)
        .map_err(|e| format!("Failed to create destination directory: {}", e))?;

    // ZIPファイルを展開
    for i in 0..archive.len() {
        let mut file = archive
            .by_index(i)
            .map_err(|e| format!("Failed to access file in ZIP: {}", e))?;

        let outpath = match file.enclosed_name() {
            Some(path) => dest_path.join(path),
            None => continue,
        };

        if file.name().ends_with('/') {
            // ディレクトリ
            std::fs::create_dir_all(&outpath)
                .map_err(|e| format!("Failed to create directory: {}", e))?;
        } else {
            // ファイル
            if let Some(parent) = outpath.parent() {
                std::fs::create_dir_all(parent)
                    .map_err(|e| format!("Failed to create parent directory: {}", e))?;
            }

            let mut outfile = std::fs::File::create(&outpath)
                .map_err(|e| format!("Failed to create file: {}", e))?;

            std::io::copy(&mut file, &mut outfile)
                .map_err(|e| format!("Failed to extract file: {}", e))?;
        }
    }

    println!("Mcv installation completed");

    // ZIPファイルを削除
    std::fs::remove_file(&zip_path)
        .unwrap_or_else(|e| eprintln!("Failed to delete temp file: {}", e));

    Ok(())
}

/// プラグインをインストール（DLLコピー）
#[tauri::command]
async fn install_plugin(dll_path: String, dest_dir: String) -> Result<(), String> {
    println!("Installing plugin from: {}", dll_path);
    println!("Destination: {}", dest_dir);

    let src_path = PathBuf::from(&dll_path);
    let dest_path = PathBuf::from(&dest_dir);

    // プラグインディレクトリが存在しない場合は作成
    std::fs::create_dir_all(&dest_path)
        .map_err(|e| format!("Failed to create plugin directory: {}", e))?;

    // ファイル名を取得
    let file_name = src_path
        .file_name()
        .ok_or_else(|| "Invalid file path".to_string())?;

    let dest_file = dest_path.join(file_name);

    // DLLをコピー
    std::fs::copy(&src_path, &dest_file)
        .map_err(|e| format!("Failed to copy plugin DLL: {}", e))?;

    println!("Plugin installation completed: {:?}", dest_file);

    // DLLファイルを削除
    std::fs::remove_file(&src_path)
        .unwrap_or_else(|e| eprintln!("Failed to delete temp file: {}", e));

    Ok(())
}

/// mcvが既にインストールされているかチェック
#[tauri::command]
async fn check_existing_installation() -> Result<Option<String>, String> {
    // %LOCALAPPDATA%\Programs\MultiCommentViewer\mcv.exe の存在をチェック
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let mcv_path = PathBuf::from(local_app_data)
        .join("Programs")
        .join("MultiCommentViewer")
        .join("mcv.exe");

    if mcv_path.exists() {
        // PowerShellでファイルバージョンを取得
        let version_cmd = format!(
            "(Get-Item '{}').VersionInfo.FileVersion",
            mcv_path.display()
        );

        let output = std::process::Command::new("powershell")
            .args(&["-Command", &version_cmd])
            .output()
            .map_err(|e| format!("Failed to get version: {}", e))?;

        let version = String::from_utf8_lossy(&output.stdout).trim().to_string();

        // バージョン取得に成功した場合はそれを返す、失敗した場合は "Unknown"
        if version.is_empty() || !output.status.success() {
            println!("Could not determine mcv.exe version, using 'Unknown'");
            Ok(Some("Unknown".to_string()))
        } else {
            println!("Found existing mcv installation: version {}", version);
            Ok(Some(version))
        }
    } else {
        Ok(None)
    }
}

/// LOCALAPPDATAパスを取得
#[tauri::command]
async fn get_local_app_data() -> Result<String, String> {
    std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())
}

/// mcvを起動
#[tauri::command]
async fn launch_mcv() -> Result<(), String> {
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let mcv_path = PathBuf::from(local_app_data)
        .join("Programs")
        .join("MultiCommentViewer")
        .join("mcv.exe");

    println!("Launching mcv from: {:?}", mcv_path);

    if !mcv_path.exists() {
        return Err(format!("mcv.exe not found at: {:?}", mcv_path));
    }

    // mcv.exeを起動（バックグラウンドで実行）
    std::process::Command::new(&mcv_path)
        .spawn()
        .map_err(|e| format!("Failed to launch mcv: {}", e))?;

    Ok(())
}

/// mcvのダウンロードURLを取得
#[tauri::command]
async fn get_mcv_download_url(
    state: State<'_, AppState>,
    version: String,
    channel: String,
) -> Result<String, String> {
    Ok(state
        .update_checker
        .build_mcv_download_url(&version, &channel))
}

/// プラグインのダウンロードURLを取得
#[tauri::command]
async fn get_plugin_download_url(
    state: State<'_, AppState>,
    plugin_id: String,
    version: String,
    channel: String,
) -> Result<String, String> {
    Ok(state
        .update_checker
        .build_plugin_download_url(&plugin_id, &version, &channel))
}

/// 一時ディレクトリのパスを取得
#[tauri::command]
async fn get_temp_dir() -> Result<String, String> {
    std::env::temp_dir()
        .to_str()
        .ok_or_else(|| "Failed to get temp directory".to_string())
        .map(|s| s.to_string())
}

/// デスクトップショートカットを作成
#[tauri::command]
async fn create_desktop_shortcut(target_path: String, shortcut_name: String) -> Result<(), String> {
    println!("Creating desktop shortcut for: {}", target_path);

    let desktop =
        std::env::var("USERPROFILE").map_err(|_| "Failed to get user profile".to_string())?;
    let desktop_path = format!("{}\\Desktop", desktop);

    let ps_command = format!(
        "$ws = New-Object -ComObject WScript.Shell; \
         $s = $ws.CreateShortcut('{}\\{}.lnk'); \
         $s.TargetPath = '{}'; \
         $s.Save()",
        desktop_path, shortcut_name, target_path
    );

    let output = std::process::Command::new("powershell")
        .args(&["-Command", &ps_command])
        .output()
        .map_err(|e| format!("Failed to execute PowerShell command: {}", e))?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        return Err(format!("Failed to create desktop shortcut: {}", stderr));
    }

    println!("Desktop shortcut created successfully");
    Ok(())
}

/// スタートメニューエントリを作成
#[tauri::command]
async fn create_start_menu_entry(target_path: String, app_name: String) -> Result<(), String> {
    println!("Creating Start Menu entry for: {}", target_path);

    let start_menu = std::env::var("APPDATA").map_err(|_| "Failed to get APPDATA".to_string())?;
    let programs_path = format!("{}\\Microsoft\\Windows\\Start Menu\\Programs", start_menu);
    let app_folder = format!("{}\\{}", programs_path, app_name);

    // アプリフォルダ作成
    std::fs::create_dir_all(&app_folder)
        .map_err(|e| format!("Failed to create Start Menu folder: {}", e))?;

    let shortcut_path = format!("{}\\{}.lnk", app_folder, app_name);

    let ps_command = format!(
        "$ws = New-Object -ComObject WScript.Shell; \
         $s = $ws.CreateShortcut('{}'); \
         $s.TargetPath = '{}'; \
         $s.Save()",
        shortcut_path, target_path
    );

    let output = std::process::Command::new("powershell")
        .args(&["-Command", &ps_command])
        .output()
        .map_err(|e| format!("Failed to execute PowerShell command: {}", e))?;

    if !output.status.success() {
        let stderr = String::from_utf8_lossy(&output.stderr);
        return Err(format!("Failed to create Start Menu entry: {}", stderr));
    }

    println!("Start Menu entry created successfully");
    Ok(())
}

/// mcvをWindowsアプリとして登録
#[cfg(windows)]
#[tauri::command]
async fn register_mcv_to_windows_apps(
    mcv_version: String,
    install_location: String,
    installer_path: String,
) -> Result<(), String> {
    println!("Registering mcv to Windows apps...");

    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MultiCommentViewer";

    let (key, _) = hklm.create_subkey(path).map_err(|e| {
        format!(
            "Failed to create registry key: {}. Administrator privileges may be required.",
            e
        )
    })?;

    key.set_value("DisplayName", &"MultiCommentViewer")
        .map_err(|e| format!("Failed to set DisplayName: {}", e))?;

    key.set_value("DisplayVersion", &mcv_version)
        .map_err(|e| format!("Failed to set DisplayVersion: {}", e))?;

    key.set_value("Publisher", &"ryu-s")
        .map_err(|e| format!("Failed to set Publisher: {}", e))?;

    key.set_value("InstallLocation", &install_location)
        .map_err(|e| format!("Failed to set InstallLocation: {}", e))?;

    let uninstall_string = format!("\"{}\" --uninstall mcv", installer_path);
    key.set_value("UninstallString", &uninstall_string)
        .map_err(|e| format!("Failed to set UninstallString: {}", e))?;

    let display_icon = format!("{}\\mcv.exe", install_location);
    key.set_value("DisplayIcon", &display_icon)
        .map_err(|e| format!("Failed to set DisplayIcon: {}", e))?;

    key.set_value("NoModify", &1u32)
        .map_err(|e| format!("Failed to set NoModify: {}", e))?;

    key.set_value("NoRepair", &1u32)
        .map_err(|e| format!("Failed to set NoRepair: {}", e))?;

    // ファイルサイズを計算（KB単位）
    let install_path = PathBuf::from(&install_location);
    if let Ok(size) = calculate_install_size(&install_path) {
        key.set_value("EstimatedSize", &(size as u32))
            .unwrap_or_else(|e| eprintln!("Failed to set EstimatedSize: {}", e));
    }

    println!("mcv successfully registered to Windows apps");
    Ok(())
}

/// インストーラーをWindowsアプリとして登録
#[cfg(windows)]
#[tauri::command]
async fn register_installer_to_windows_apps(installer_path: String) -> Result<(), String> {
    println!("Registering installer to Windows apps...");

    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MultiCommentViewerInstaller";

    let (key, _) = hklm.create_subkey(path).map_err(|e| {
        format!(
            "Failed to create registry key: {}. Administrator privileges may be required.",
            e
        )
    })?;

    key.set_value("DisplayName", &"MultiCommentViewer Installer & Updater")
        .map_err(|e| format!("Failed to set DisplayName: {}", e))?;

    let installer_version = env!("CARGO_PKG_VERSION");
    key.set_value("DisplayVersion", &installer_version)
        .map_err(|e| format!("Failed to set DisplayVersion: {}", e))?;

    key.set_value("Publisher", &"ryu-s")
        .map_err(|e| format!("Failed to set Publisher: {}", e))?;

    let install_location = PathBuf::from(&installer_path)
        .parent()
        .and_then(|p| p.to_str())
        .ok_or_else(|| "Invalid installer path".to_string())?
        .to_string();

    key.set_value("InstallLocation", &install_location)
        .map_err(|e| format!("Failed to set InstallLocation: {}", e))?;

    let uninstall_string = format!("\"{}\" --uninstall self", installer_path);
    key.set_value("UninstallString", &uninstall_string)
        .map_err(|e| format!("Failed to set UninstallString: {}", e))?;

    key.set_value("DisplayIcon", &installer_path)
        .map_err(|e| format!("Failed to set DisplayIcon: {}", e))?;

    key.set_value("NoModify", &1u32)
        .map_err(|e| format!("Failed to set NoModify: {}", e))?;

    key.set_value("NoRepair", &1u32)
        .map_err(|e| format!("Failed to set NoRepair: {}", e))?;

    println!("Installer successfully registered to Windows apps");
    Ok(())
}

/// Windowsアプリ登録を解除
#[cfg(windows)]
#[tauri::command]
async fn unregister_from_windows_apps(
    app_name: String, // "MultiCommentViewer" or "MultiCommentViewerInstaller"
) -> Result<(), String> {
    println!("Unregistering {} from Windows apps...", app_name);

    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let uninstall_path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    let uninstall_key = hklm.open_subkey_with_flags(uninstall_path, KEY_WRITE)
        .map_err(|e| format!("Failed to open Uninstall registry key: {}. Administrator privileges may be required.", e))?;

    uninstall_key
        .delete_subkey(&app_name)
        .map_err(|e| format!("Failed to delete registry key {}: {}", app_name, e))?;

    println!("{} successfully unregistered from Windows apps", app_name);
    Ok(())
}

/// インストールディレクトリのサイズを計算（KB単位）
#[cfg(windows)]
fn calculate_install_size(path: &PathBuf) -> Result<u64, std::io::Error> {
    let mut total_size = 0u64;

    if path.is_dir() {
        for entry in std::fs::read_dir(path)? {
            let entry = entry?;
            let metadata = entry.metadata()?;

            if metadata.is_file() {
                total_size += metadata.len();
            } else if metadata.is_dir() {
                if let Ok(size) = calculate_install_size(&entry.path()) {
                    total_size += size;
                }
            }
        }
    }

    // バイトからKBに変換
    Ok(total_size / 1024)
}

/// 管理者権限で実行されているかチェック
#[cfg(windows)]
#[tauri::command]
async fn is_elevated() -> Result<bool, String> {
    // Windowsで管理者権限チェック
    // 簡易実装: HKEY_LOCAL_MACHINEへの書き込みテストで確認
    let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
    let test_path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

    match hklm.open_subkey_with_flags(test_path, KEY_WRITE) {
        Ok(_) => Ok(true),
        Err(_) => Ok(false),
    }
}

/// インストーラーを固定場所にコピー
#[tauri::command]
async fn copy_installer_to_persistent_location() -> Result<String, String> {
    println!("Copying installer to persistent location...");

    // 現在の実行ファイルパスを取得
    let current_exe =
        std::env::current_exe().map_err(|e| format!("Failed to get current exe path: {}", e))?;

    // 固定場所のパスを構築
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let dest_dir = PathBuf::from(local_app_data)
        .join("Programs")
        .join("mcv-installer");

    let dest_path = dest_dir.join("mcv-installer.exe");

    // 既に固定場所にある場合はスキップ
    if current_exe == dest_path {
        println!("Already running from persistent location");
        return Ok(dest_path.to_string_lossy().to_string());
    }

    // ディレクトリを作成
    std::fs::create_dir_all(&dest_dir)
        .map_err(|e| format!("Failed to create installer directory: {}", e))?;

    // ファイルをコピー
    std::fs::copy(&current_exe, &dest_path)
        .map_err(|e| format!("Failed to copy installer: {}", e))?;

    println!("Installer copied to: {:?}", dest_path);

    Ok(dest_path.to_string_lossy().to_string())
}

/// インストーラーが固定場所にあるか確認
#[tauri::command]
async fn is_installer_in_persistent_location() -> Result<bool, String> {
    // 現在の実行ファイルパスを取得
    let current_exe =
        std::env::current_exe().map_err(|e| format!("Failed to get current exe path: {}", e))?;

    // 固定場所のパスを構築
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let persistent_path = PathBuf::from(local_app_data)
        .join("Programs")
        .join("mcv-installer")
        .join("mcv-installer.exe");

    let is_installed = current_exe == persistent_path;

    println!("=== Installer Location Check ===");
    println!("Current exe: {:?}", current_exe);
    println!("Persistent path: {:?}", persistent_path);
    println!("Is installed: {}", is_installed);
    println!("================================");

    Ok(is_installed)
}

/// インストーラーのデフォルトインストール先パスを取得
#[tauri::command]
async fn get_installer_default_install_path() -> Result<String, String> {
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let path = PathBuf::from(local_app_data)
        .join("Programs")
        .join("mcv-installer");

    Ok(path.to_string_lossy().to_string())
}

/// mcv.exeが実行中かチェック
#[tauri::command]
async fn is_mcv_running() -> Result<bool, String> {
    let mut system = System::new_all();
    system.refresh_processes();

    let is_running = system
        .processes()
        .values()
        .any(|process| process.name().eq_ignore_ascii_case("mcv.exe"));

    Ok(is_running)
}

/// アンインストールモードを取得
#[tauri::command]
async fn get_uninstall_mode() -> Result<Option<String>, String> {
    Ok(UNINSTALL_TARGET.lock().unwrap().clone())
}

/// ユーザーデータ識別用のパターン
const USER_DATA_PATTERNS: &[&str] = &["config.json", "logs.db", "user_data", "settings"];

/// パスがユーザーデータかどうか判定
fn is_user_data(path: &std::path::Path) -> bool {
    let file_name = path.file_name().and_then(|n| n.to_str()).unwrap_or("");

    USER_DATA_PATTERNS
        .iter()
        .any(|pattern| file_name.contains(pattern))
}

/// mcvをアンインストール
#[tauri::command]
async fn uninstall_mcv(keep_user_data: bool) -> Result<(), String> {
    println!("Uninstalling mcv... (keep_user_data: {})", keep_user_data);

    // mcvインストールディレクトリを取得
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let mcv_dir = PathBuf::from(&local_app_data)
        .join("Programs")
        .join("MultiCommentViewer");

    if !mcv_dir.exists() {
        return Err("mcv is not installed".to_string());
    }

    // 1. ファイル削除
    if keep_user_data {
        // ユーザーデータを保持する場合、選択的に削除
        println!("Deleting mcv files (keeping user data)...");

        for entry in std::fs::read_dir(&mcv_dir)
            .map_err(|e| format!("Failed to read mcv directory: {}", e))?
        {
            let entry = entry.map_err(|e| format!("Failed to read entry: {}", e))?;
            let path = entry.path();

            // ユーザーデータはスキップ
            if is_user_data(&path) {
                println!("Keeping user data: {:?}", path);
                continue;
            }

            // それ以外を削除
            if path.is_file() {
                std::fs::remove_file(&path)
                    .unwrap_or_else(|e| eprintln!("Failed to delete file {:?}: {}", path, e));
                println!("Deleted file: {:?}", path);
            } else if path.is_dir() {
                std::fs::remove_dir_all(&path)
                    .unwrap_or_else(|e| eprintln!("Failed to delete directory {:?}: {}", path, e));
                println!("Deleted directory: {:?}", path);
            }
        }
    } else {
        // 全削除
        println!("Deleting mcv directory completely...");
        std::fs::remove_dir_all(&mcv_dir)
            .map_err(|e| format!("Failed to delete mcv directory: {}", e))?;
    }

    // 2. デスクトップショートカット削除
    let desktop =
        std::env::var("USERPROFILE").map_err(|_| "Failed to get USERPROFILE".to_string())?;
    let desktop_shortcut = PathBuf::from(desktop)
        .join("Desktop")
        .join("MultiCommentViewer.lnk");

    if desktop_shortcut.exists() {
        std::fs::remove_file(&desktop_shortcut)
            .unwrap_or_else(|e| eprintln!("Failed to delete desktop shortcut: {}", e));
        println!("Deleted desktop shortcut");
    }

    // 3. スタートメニューエントリ削除
    let start_menu = std::env::var("APPDATA").map_err(|_| "Failed to get APPDATA".to_string())?;
    let start_menu_entry = PathBuf::from(start_menu)
        .join("Microsoft")
        .join("Windows")
        .join("Start Menu")
        .join("Programs")
        .join("MultiCommentViewer");

    if start_menu_entry.exists() {
        std::fs::remove_dir_all(&start_menu_entry)
            .unwrap_or_else(|e| eprintln!("Failed to delete start menu entry: {}", e));
        println!("Deleted start menu entry");
    }

    // 4. レジストリエントリ削除
    #[cfg(windows)]
    {
        unregister_from_windows_apps("MultiCommentViewer".to_string())
            .await
            .unwrap_or_else(|e| eprintln!("Failed to unregister from Windows apps: {}", e));
    }

    println!("mcv uninstallation completed");
    Ok(())
}

/// mcv-installerをアンインストール
#[tauri::command]
async fn uninstall_installer() -> Result<(), String> {
    println!("Uninstalling mcv-installer...");

    // インストーラーディレクトリを取得
    let local_app_data =
        std::env::var("LOCALAPPDATA").map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let installer_dir = PathBuf::from(&local_app_data)
        .join("Programs")
        .join("mcv-installer");

    if !installer_dir.exists() {
        return Err("Installer is not installed".to_string());
    }

    // 1. レジストリエントリ削除（先に実行）
    #[cfg(windows)]
    {
        unregister_from_windows_apps("MultiCommentViewerInstaller".to_string())
            .await
            .unwrap_or_else(|e| {
                eprintln!("Failed to unregister installer from Windows apps: {}", e)
            });
    }

    // 2. PowerShellで遅延削除を実行（リトライロジック付き）
    // 実行中のEXEは自身を削除できないため、PowerShellで遅延削除
    let installer_dir_str = installer_dir.to_string_lossy().to_string();

    // リトライロジック付きの削除スクリプト
    let ps_command = format!(
        r#"Start-Sleep -Seconds 5; $maxRetries = 10; $retryCount = 0; while ($retryCount -lt $maxRetries) {{ try {{ Remove-Item -Path '{}' -Recurse -Force -ErrorAction Stop; exit 0 }} catch {{ $retryCount++; if ($retryCount -lt $maxRetries) {{ Start-Sleep -Seconds 1 }} }} }}"#,
        installer_dir_str
    );

    println!(
        "Scheduling installer directory deletion: {}",
        installer_dir_str
    );

    std::process::Command::new("powershell")
        .args(&["-WindowStyle", "Hidden", "-Command", &ps_command])
        .spawn()
        .map_err(|e| format!("Failed to schedule installer deletion: {}", e))?;

    println!("Installer uninstallation scheduled (will complete after exit)");
    Ok(())
}

// Windows以外のプラットフォームでのスタブ実装
#[cfg(not(windows))]
#[tauri::command]
async fn register_mcv_to_windows_apps(
    _mcv_version: String,
    _install_location: String,
    _installer_path: String,
) -> Result<(), String> {
    Err("This command is only available on Windows".to_string())
}

#[cfg(not(windows))]
#[tauri::command]
async fn register_installer_to_windows_apps(_installer_path: String) -> Result<(), String> {
    Err("This command is only available on Windows".to_string())
}

#[cfg(not(windows))]
#[tauri::command]
async fn unregister_from_windows_apps(_app_name: String) -> Result<(), String> {
    Err("This command is only available on Windows".to_string())
}

#[cfg(not(windows))]
#[tauri::command]
async fn is_elevated() -> Result<bool, String> {
    Err("This command is only available on Windows".to_string())
}

fn main() {
    const API_BASE_URL: &str = "http://localhost"; // TODO: 実際のAPIエンドポイントに変更

    let update_checker = UpdateChecker::new(API_BASE_URL);

    // コマンドライン引数をパース
    let args: Vec<String> = std::env::args().collect();
    let uninstall_target = if args.len() > 2 && args[1] == "--uninstall" {
        match args[2].as_str() {
            "mcv" => Some("mcv".to_string()),
            "self" => Some("installer".to_string()),
            _ => None,
        }
    } else {
        None
    };

    // グローバル変数に保存
    *UNINSTALL_TARGET.lock().unwrap() = uninstall_target.clone();

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup(move |app| {
            println!("Installer started");

            // アンインストールモードの場合、イベントを発行
            if let Some(ref target) = uninstall_target {
                println!("Uninstall mode detected: {}", target);
                if let Some(window) = app.get_webview_window("main") {
                    let _ = window.emit("uninstall-mode", target);
                }
            }

            // 管理者権限チェック（Windows）
            #[cfg(windows)]
            {
                let hklm = RegKey::predef(HKEY_LOCAL_MACHINE);
                let test_path = r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";

                match hklm.open_subkey_with_flags(test_path, KEY_WRITE) {
                    Ok(_) => {
                        println!("Running with administrator privileges");
                    }
                    Err(_) => {
                        eprintln!("Warning: Running without administrator privileges.");
                        eprintln!("Some operations (Windows app registration) may fail.");
                        eprintln!(
                            "Please run the installer as administrator if you encounter issues."
                        );
                    }
                }
            }

            Ok(())
        })
        .manage(AppState { update_checker })
        .invoke_handler(tauri::generate_handler![
            check_installer_update,
            check_mcv_update,
            list_plugins,
            download_file,
            verify_checksum,
            install_mcv,
            install_plugin,
            check_existing_installation,
            get_local_app_data,
            launch_mcv,
            get_mcv_download_url,
            get_plugin_download_url,
            get_temp_dir,
            create_desktop_shortcut,
            create_start_menu_entry,
            register_mcv_to_windows_apps,
            register_installer_to_windows_apps,
            unregister_from_windows_apps,
            is_elevated,
            copy_installer_to_persistent_location,
            is_installer_in_persistent_location,
            get_installer_default_install_path,
            is_mcv_running,
            uninstall_mcv,
            uninstall_installer,
            get_uninstall_mode,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
