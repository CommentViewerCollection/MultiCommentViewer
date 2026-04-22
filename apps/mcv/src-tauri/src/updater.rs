use std::path::{Path, PathBuf};

use actix::Addr;
use mcv_core::{CoreActor, PluginManager, ScanAndLoadNewPlugins};
use mcv_updater::{PluginVersionDetail, UpdateChecker};
use zip::ZipArchive;

use crate::plugin_fs::{get_plugin_dir, read_plugin_manifest};

/// ビルドチャンネルを返す
pub(crate) fn get_current_channel() -> &'static str {
    #[cfg(feature = "alpha")]
    {
        "alpha"
    }
    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    {
        "beta"
    }
    #[cfg(not(any(feature = "alpha", feature = "beta")))]
    {
        "stable"
    }
}

/// PowerShell の単一引用符内で特殊文字をエスケープする
pub(crate) fn ps_escape_single_quoted(input: &str) -> String {
    input.replace('\'', "''")
}

/// 指定ディレクトリへの書き込み可否を確認する（プローブファイルの作成・削除で確認）
pub(crate) fn can_write_to_dir(dir: &Path) -> Result<(), String> {
    let probe = dir.join(".mcv_write_probe.tmp");
    std::fs::write(&probe, b"probe").map_err(|e| {
        format!(
            "Update requires write permission to install directory ({}): {}",
            dir.display(),
            e
        )
    })?;
    let _ = std::fs::remove_file(&probe);
    Ok(())
}

/// ZIP ファイルを指定ディレクトリに展開する
pub(crate) fn extract_zip_file(zip_path: &PathBuf, dest_dir: &PathBuf) -> Result<(), String> {
    if dest_dir.exists() {
        std::fs::remove_dir_all(dest_dir)
            .map_err(|e| format!("Failed to clean temporary directory: {}", e))?;
    }
    std::fs::create_dir_all(dest_dir)
        .map_err(|e| format!("Failed to create temporary directory: {}", e))?;

    let zip_file =
        std::fs::File::open(zip_path).map_err(|e| format!("Failed to open ZIP file: {}", e))?;
    let mut archive =
        ZipArchive::new(zip_file).map_err(|e| format!("Failed to read ZIP archive: {}", e))?;

    for i in 0..archive.len() {
        let mut file = archive
            .by_index(i)
            .map_err(|e| format!("Failed to access ZIP entry: {}", e))?;
        let out_path = match file.enclosed_name() {
            Some(path) => dest_dir.join(path),
            None => continue,
        };

        if file.name().ends_with('/') {
            std::fs::create_dir_all(&out_path)
                .map_err(|e| format!("Failed to create directory: {}", e))?;
        } else {
            if let Some(parent) = out_path.parent() {
                std::fs::create_dir_all(parent)
                    .map_err(|e| format!("Failed to create parent directory: {}", e))?;
            }
            let mut output = std::fs::File::create(&out_path)
                .map_err(|e| format!("Failed to create extracted file: {}", e))?;
            std::io::copy(&mut file, &mut output)
                .map_err(|e| format!("Failed to extract ZIP entry: {}", e))?;
        }
    }

    Ok(())
}

/// ディレクトリを再帰的に走査して指定ファイル名を持つファイルを検索する
pub(crate) fn find_file_recursive(root: &PathBuf, file_name: &str) -> Option<PathBuf> {
    if !root.is_dir() {
        return None;
    }

    let entries = std::fs::read_dir(root).ok()?;
    for entry in entries.flatten() {
        let path = entry.path();
        if path.is_file() {
            if path.file_name().and_then(|n| n.to_str()) == Some(file_name) {
                return Some(path);
            }
        } else if path.is_dir() {
            if let Some(found) = find_file_recursive(&path, file_name) {
                return Some(found);
            }
        }
    }
    None
}

/// プラグインインストールの共通実装
pub(crate) async fn install_registry_plugin_internal(
    plugin_id: String,
    version: String,
    channel: String,
    core_addr: Addr<CoreActor>,
    plugin_manager: std::sync::Arc<tokio::sync::Mutex<PluginManager>>,
) -> Result<(), String> {
    let updater = UpdateChecker::new(crate::API_BASE_URL, crate::get_user_agent());
    let plugin_detail = updater
        .get_plugin_detail(&plugin_id)
        .await
        .map_err(|e| format!("Failed to fetch plugin detail: {}", e))?;

    let target_version: &PluginVersionDetail = plugin_detail
        .versions
        .iter()
        .find(|v| v.version == version && v.channel == channel && !v.is_deleted && v.is_public)
        .ok_or_else(|| format!("Plugin version not found: {plugin_id} {version} {channel}"))?;

    let temp_dir = std::env::temp_dir().join("mcv-plugin-install");
    std::fs::create_dir_all(&temp_dir)
        .map_err(|e| format!("Failed to create temp directory: {}", e))?;
    let zip_path = temp_dir.join(format!("{}-{}-{}.zip", plugin_id, version, channel));

    let download_url = updater.build_plugin_download_url(&plugin_id, &version, &channel);
    updater
        .download(&download_url, &zip_path, |_downloaded, _total| {})
        .await
        .map_err(|e| format!("Failed to download plugin package: {}", e))?;

    updater
        .verify_checksum(&zip_path, &target_version.sha256)
        .await
        .map_err(|e| format!("Failed to verify plugin package checksum: {}", e))?;

    // ZIPをそのままpluginsディレクトリに配置する（依存ファイルを含めて保持するため）
    let plugin_dir = get_plugin_dir();
    std::fs::create_dir_all(&plugin_dir)
        .map_err(|e| format!("Failed to create plugin directory: {}", e))?;

    let dest_path = plugin_dir.join(format!("{}.zip", plugin_id));
    std::fs::copy(&zip_path, &dest_path).map_err(|e| format!("Failed to install plugin: {}", e))?;

    // 再インストール時に古いキャッシュが残らないよう削除する
    let cache_dir = plugin_dir.join(".cache").join(&plugin_id);
    let _ = std::fs::remove_dir_all(&cache_dir);

    // ディレクトリ形式で同じ plugin_id のプラグインが存在する場合は
    // plugin.json を .deleted.plugin.json にリネームして削除待ちとしてマーク
    if let Ok(entries) = std::fs::read_dir(&plugin_dir) {
        for entry in entries.flatten() {
            let path = entry.path();
            if !path.is_dir() {
                continue;
            }
            let manifest = path.join("plugin.json");
            if let Some((id, _, _)) = read_plugin_manifest(&manifest) {
                if id == plugin_id {
                    let deleted = path.join(".deleted.plugin.json");
                    if let Err(e) = std::fs::rename(&manifest, &deleted) {
                        tracing::warn!(
                            target: "mcv::main",
                            error = %e,
                            path = %manifest.display(),
                            "Failed to rename plugin.json to .deleted.plugin.json"
                        );
                    }
                    break;
                }
            }
        }
    }

    // ZIP 形式でインストール済みの場合にキャッシュ強制再展開を促すマーカーを作成
    let marker = plugin_dir.join(format!(".force-update-{}", plugin_id));
    let _ = std::fs::write(&marker, version.as_bytes());

    let _ = std::fs::remove_file(&zip_path);

    // actix コンテキスト内で新プラグインをスキャン・ロードする
    core_addr
        .send(ScanAndLoadNewPlugins {
            plugin_manager,
            plugins_dir: plugin_dir,
        })
        .await
        .map_err(|e| format!("Failed to load plugin after install: {}", e))?;

    Ok(())
}
