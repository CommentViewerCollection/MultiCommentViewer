use std::sync::Arc;

use crate::constraints::{load_plugin_constraints, PluginConstraintStatus};
use crate::plugin_fs::{
    find_zip_stem_for_id, get_plugin_dir, scan_installed_plugins, InstalledPluginMeta,
};
use crate::types::{AppState, SettingsDirState};
use crate::updater::install_registry_plugin_internal;

/// レジストリからプラグインZIPをダウンロードしてインストール
#[tauri::command]
pub(crate) async fn install_registry_plugin(
    plugin_id: String,
    version: String,
    channel: String,
    state: tauri::State<'_, AppState>,
) -> Result<(), String> {
    install_registry_plugin_internal(
        plugin_id,
        version,
        channel,
        state.core_addr.clone(),
        Arc::clone(&state.plugin_manager),
    )
    .await
}

/// インストール済みプラグイン一覧をバージョン情報付きで取得
#[tauri::command]
pub(crate) async fn list_installed_plugins() -> Result<Vec<InstalledPluginMeta>, String> {
    let plugin_dir = get_plugin_dir();
    scan_installed_plugins(&plugin_dir)
}

/// インストール済みプラグインを削除する（ZIP形式・ディレクトリ形式の両方に対応）
///
/// DLL がプロセスにロードされているため即時削除できない場合は、
/// `.uninstall-{id}` にリネームして次回起動時にクリーンアップする。
#[tauri::command]
pub(crate) async fn uninstall_registry_plugin(plugin_id: String) -> Result<(), String> {
    let plugin_dir = get_plugin_dir();
    let dir_path = plugin_dir.join(&plugin_id);

    // ZIP ファイルの探索
    let zip_stem = find_zip_stem_for_id(&plugin_dir, &plugin_id);
    let zip_path = if let Some(ref stem) = zip_stem {
        let versioned = plugin_dir.join(format!("{}.zip", stem));
        if versioned.exists() {
            Some(versioned)
        } else {
            let simple = plugin_dir.join(format!("{}.zip", plugin_id));
            if simple.exists() {
                Some(simple)
            } else {
                None
            }
        }
    } else {
        let simple = plugin_dir.join(format!("{}.zip", plugin_id));
        if simple.exists() {
            Some(simple)
        } else {
            None
        }
    };
    let cache_dir = plugin_dir
        .join(".cache")
        .join(zip_stem.unwrap_or_else(|| plugin_id.clone()));

    let mut found = false;

    if let Some(ref zp) = zip_path {
        std::fs::remove_file(zp)
            .map_err(|e| format!("Failed to remove plugin '{}': {}", plugin_id, e))?;
        found = true;
    }

    if dir_path.exists() {
        if std::fs::remove_dir_all(&dir_path).is_err() {
            let pending_path = plugin_dir.join(format!(".uninstall-{}", plugin_id));
            if std::fs::rename(&dir_path, &pending_path).is_err() {
                tracing::warn!(
                    target: "mcv::main",
                    id = %plugin_id,
                    "Cannot rename locked plugin directory; creating uninstall marker for next startup"
                );
                std::fs::write(&pending_path, b"").map_err(|e| {
                    format!("Failed to queue plugin '{}' for removal: {}", plugin_id, e)
                })?;
            }
        }
        found = true;
    }

    // キャッシュも同様に処理（best-effort）
    if cache_dir.exists() && std::fs::remove_dir_all(&cache_dir).is_err() {
        let pending_cache = plugin_dir
            .join(".cache")
            .join(format!(".uninstall-{}", plugin_id));
        let _ = std::fs::rename(&cache_dir, &pending_cache);
    }

    if !found {
        return Err(format!("Plugin '{}' is not installed.", plugin_id));
    }

    Ok(())
}

/// キャッシュを元にインストール済みプラグインの制約ステータスを返す
#[tauri::command]
pub(crate) async fn get_plugin_constraint_status(
    state: tauri::State<'_, SettingsDirState>,
) -> Result<Vec<PluginConstraintStatus>, String> {
    use crate::constraints::compare_semver;
    use std::cmp::Ordering;

    let settings_dir = state.settings_dir.clone();
    let cache = match load_plugin_constraints(&settings_dir) {
        Some(c) => c,
        None => return Ok(vec![]),
    };

    let plugin_dir = get_plugin_dir();
    let installed = match scan_installed_plugins(&plugin_dir) {
        Ok(p) => p,
        Err(_) => return Ok(vec![]),
    };

    let mut results = Vec::new();
    for inst in &installed {
        let Some(entry) = cache.constraints.iter().find(|c| c.id == inst.id) else {
            continue;
        };

        let channel = inst.channel.as_deref().unwrap_or("stable");
        let version = inst.version.as_deref().unwrap_or("0.0.0");

        if let Some(b) = entry
            .blocked_versions
            .iter()
            .find(|b| b.version == version && b.channel == channel)
        {
            results.push(PluginConstraintStatus {
                id: inst.id.clone(),
                status: "blocked".to_string(),
                reason: b.reason.clone(),
            });
            continue;
        }

        let below_min = entry.min_version.as_ref().and_then(|mv| {
            let min = match channel {
                "stable" => mv.stable.as_deref(),
                "beta" => mv.beta.as_deref(),
                "alpha" => mv.alpha.as_deref(),
                _ => None,
            };
            min.filter(|min_str| compare_semver(version, min_str) == Ordering::Less)
        });

        if let Some(min_str) = below_min {
            results.push(PluginConstraintStatus {
                id: inst.id.clone(),
                status: "below_min_version".to_string(),
                reason: Some(format!("v{} 以上が必要です", min_str)),
            });
            continue;
        }

        results.push(PluginConstraintStatus {
            id: inst.id.clone(),
            status: "ok".to_string(),
            reason: None,
        });
    }

    Ok(results)
}
