#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use mcv_updater::{UpdateChecker, InstallerUpdateInfo, McvUpdateInfo, PluginListItem};
use std::path::PathBuf;
use tauri::{AppHandle, Emitter, State};

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

    match state.update_checker.check_installer_update(CURRENT_VERSION).await {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                println!("Installer update available: {} -> {}", CURRENT_VERSION, info.version);
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
    println!("Checking for mcv updates... Current version: {}", current_version);

    match state.update_checker.check_mcv_update(&current_version).await {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                println!("Mcv update available: {} -> {}", current_version, info.version);
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

    let zip_file = std::fs::File::open(&zip_path)
        .map_err(|e| format!("Failed to open ZIP file: {}", e))?;

    let mut archive = zip::ZipArchive::new(zip_file)
        .map_err(|e| format!("Failed to read ZIP archive: {}", e))?;

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
    // %LOCALAPPDATA%\MultiCommentViewer\mcv.exe の存在をチェック
    let local_app_data = std::env::var("LOCALAPPDATA")
        .map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let mcv_path = PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("mcv.exe");

    if mcv_path.exists() {
        // TODO: mcv.exeのバージョンを取得
        // 現時点では簡易的に存在チェックのみ
        Ok(Some("0.1.0".to_string()))
    } else {
        Ok(None)
    }
}

/// LOCALAPPDATAパスを取得
#[tauri::command]
async fn get_local_app_data() -> Result<String, String> {
    std::env::var("LOCALAPPDATA")
        .map_err(|_| "Failed to get LOCALAPPDATA".to_string())
}

/// mcvを起動
#[tauri::command]
async fn launch_mcv() -> Result<(), String> {
    let local_app_data = std::env::var("LOCALAPPDATA")
        .map_err(|_| "Failed to get LOCALAPPDATA".to_string())?;

    let mcv_path = PathBuf::from(local_app_data)
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
    Ok(state.update_checker.build_mcv_download_url(&version, &channel))
}

/// プラグインのダウンロードURLを取得
#[tauri::command]
async fn get_plugin_download_url(
    state: State<'_, AppState>,
    plugin_id: String,
    version: String,
    channel: String,
) -> Result<String, String> {
    Ok(state.update_checker.build_plugin_download_url(&plugin_id, &version, &channel))
}

/// 一時ディレクトリのパスを取得
#[tauri::command]
async fn get_temp_dir() -> Result<String, String> {
    std::env::temp_dir()
        .to_str()
        .ok_or_else(|| "Failed to get temp directory".to_string())
        .map(|s| s.to_string())
}

fn main() {
    const API_BASE_URL: &str = "http://localhost"; // TODO: 実際のAPIエンドポイントに変更

    let update_checker = UpdateChecker::new(API_BASE_URL);

    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup(|_app| {
            println!("Installer started");
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
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
