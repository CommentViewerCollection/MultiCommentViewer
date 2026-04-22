use mcv_updater::{McvUpdateInfo, PluginListItem, UpdateChecker};
use std::path::PathBuf;
use tauri::AppHandle;

use crate::types::{AppState, CoreUpdatePayload};
use crate::updater::{
    can_write_to_dir, extract_zip_file, find_file_recursive, ps_escape_single_quoted,
};

/// mcv本体の更新をチェック
#[tauri::command]
pub(crate) async fn check_for_updates() -> Result<Option<McvUpdateInfo>, String> {
    const CURRENT_VERSION: &str = env!("CARGO_PKG_VERSION");
    tracing::info!(current_version = CURRENT_VERSION, "Checking for updates");

    let updater = UpdateChecker::new(crate::API_BASE_URL, crate::get_user_agent());
    match updater.check_mcv_update(CURRENT_VERSION).await {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                tracing::info!(
                    current_version = CURRENT_VERSION,
                    new_version = %info.version,
                    "Update available"
                );
            } else {
                tracing::info!("No update available");
            }
            Ok(update_info)
        }
        Err(e) => {
            tracing::error!(error = %e, "Failed to check for updates");
            Err(format!("Failed to check for updates: {}", e))
        }
    }
}

/// 現在のバージョンを返す
#[tauri::command]
pub(crate) async fn get_current_version() -> Result<String, String> {
    Ok(env!("CARGO_PKG_VERSION").to_string())
}

/// 配布サーバーのプラグイン一覧を取得
#[tauri::command]
pub(crate) async fn list_registry_plugins() -> Result<Vec<PluginListItem>, String> {
    let updater = UpdateChecker::new(crate::API_BASE_URL, crate::get_user_agent());
    updater
        .list_plugins()
        .await
        .map_err(|e| format!("Failed to list registry plugins: {}", e))
}

/// mcv本体アップデートZIPをダウンロードしてチェックサム検証
#[tauri::command]
pub(crate) async fn download_core_update(
    version: String,
    channel: String,
    sha256: String,
) -> Result<String, String> {
    let updater = UpdateChecker::new(crate::API_BASE_URL, crate::get_user_agent());
    let temp_dir = std::env::temp_dir().join("mcv-updater");
    std::fs::create_dir_all(&temp_dir)
        .map_err(|e| format!("Failed to create temp directory: {}", e))?;
    let zip_path = temp_dir.join(format!("mcv-core-{}-{}.zip", version, channel));

    let download_url = updater.build_mcv_download_url(&version, &channel);
    updater
        .download(&download_url, &zip_path, |_downloaded, _total| {})
        .await
        .map_err(|e| format!("Failed to download update package: {}", e))?;

    updater
        .verify_checksum(&zip_path, &sha256)
        .await
        .map_err(|e| format!("Failed to verify update package checksum: {}", e))?;

    Ok(zip_path.to_string_lossy().to_string())
}

/// ダウンロード済みアップデートを適用し、再起動
#[tauri::command]
pub(crate) async fn apply_core_update(
    zip_path: String,
    app_handle: AppHandle,
) -> Result<(), String> {
    let current_exe =
        std::env::current_exe().map_err(|e| format!("Failed to get current exe path: {}", e))?;
    let exe_dir = current_exe
        .parent()
        .ok_or_else(|| "Failed to resolve executable directory".to_string())?
        .to_path_buf();

    can_write_to_dir(&exe_dir)?;

    let update_zip = PathBuf::from(&zip_path);
    if !update_zip.exists() {
        return Err(format!("Update ZIP not found: {}", update_zip.display()));
    }

    let extracted_dir =
        std::env::temp_dir().join(format!("mcv-update-extracted-{}", std::process::id()));
    extract_zip_file(&update_zip, &extracted_dir)?;

    let exe_name = current_exe
        .file_name()
        .and_then(|n| n.to_str())
        .ok_or_else(|| "Failed to resolve executable name".to_string())?;
    let new_exe = find_file_recursive(&extracted_dir, exe_name)
        .ok_or_else(|| format!("{} not found in update package", exe_name))?;

    let ps_command = format!(
        "Start-Sleep -Milliseconds 700; \
         Copy-Item -Path '{src}' -Destination '{dst}' -Force; \
         Start-Process -FilePath '{dst}'; \
         Remove-Item -Path '{zip}' -Force -ErrorAction SilentlyContinue; \
         Remove-Item -Path '{extract}' -Recurse -Force -ErrorAction SilentlyContinue",
        src = ps_escape_single_quoted(&new_exe.to_string_lossy()),
        dst = ps_escape_single_quoted(&current_exe.to_string_lossy()),
        zip = ps_escape_single_quoted(&update_zip.to_string_lossy()),
        extract = ps_escape_single_quoted(&extracted_dir.to_string_lossy()),
    );

    std::process::Command::new("powershell")
        .args(["-WindowStyle", "Hidden", "-Command", &ps_command])
        .spawn()
        .map_err(|e| format!("Failed to launch update script: {}", e))?;

    tracing::info!("Core update script launched, exiting mcv");
    app_handle.exit(0);
    Ok(())
}

/// フロントエンド起動時に pending な core アップデートを取得する
/// イベントを受け取る前に UI が表示された場合のフォールバック
#[tauri::command]
pub(crate) async fn get_pending_core_update(
    state: tauri::State<'_, AppState>,
) -> Result<Option<CoreUpdatePayload>, String> {
    Ok(state.pending_core_update.lock().await.clone())
}
