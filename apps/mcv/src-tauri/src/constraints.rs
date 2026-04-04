use std::cmp::Ordering;
use std::path::Path;

use actix::Addr;
use mcv_core::CoreActor;
use mcv_updater::{BlockedVersion, PluginMinVersion};
use tauri::{AppHandle, Emitter};

use crate::plugin_fs::{get_plugin_dir, scan_installed_plugins};
use crate::types::CoreUpdatePayload;
use crate::updater::{get_current_channel, install_registry_plugin_internal};

/// キャッシュファイル内の1プラグイン分の制約情報
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct PluginConstraintEntry {
    pub(crate) id: String,
    #[serde(default)]
    pub(crate) min_version: Option<PluginMinVersion>,
    #[serde(default)]
    pub(crate) blocked_versions: Vec<BlockedVersion>,
    /// このチャンネルの最新バージョン（強制アップデート先として使用）
    #[serde(default)]
    pub(crate) channels: Option<mcv_updater::PluginChannels>,
}

/// settings/plugin-constraints.json に保存するキャッシュ全体
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct PluginConstraintsCache {
    pub(crate) fetched_at: String,
    pub(crate) constraints: Vec<PluginConstraintEntry>,
}

/// フロントエンドへ返す制約ステータス（インストール済みプラグインごと）
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct PluginConstraintStatus {
    pub(crate) id: String,
    /// "ok" | "blocked" | "below_min_version"
    pub(crate) status: String,
    pub(crate) reason: Option<String>,
}

/// settings/plugin-constraints.json を読み込む
pub(crate) fn load_plugin_constraints(settings_dir: &Path) -> Option<PluginConstraintsCache> {
    let path = settings_dir.join("plugin-constraints.json");
    let content = std::fs::read(&path).ok()?;
    serde_json::from_slice(&content).ok()
}

/// settings/plugin-constraints.json に書き込む
pub(crate) fn save_plugin_constraints(settings_dir: &Path, cache: &PluginConstraintsCache) {
    let path = settings_dir.join("plugin-constraints.json");
    if let Ok(json) = serde_json::to_string_pretty(cache) {
        let _ = std::fs::write(&path, json);
    }
}

/// "X.Y.Z" 形式のバージョン文字列を比較する
pub(crate) fn compare_semver(a: &str, b: &str) -> Ordering {
    let parse = |s: &str| -> (u64, u64, u64) {
        let parts: Vec<u64> = s.split('.').filter_map(|p| p.parse().ok()).collect();
        (
            parts.first().copied().unwrap_or(0),
            parts.get(1).copied().unwrap_or(0),
            parts.get(2).copied().unwrap_or(0),
        )
    };
    parse(a).cmp(&parse(b))
}

/// core 本体の強制アップデートチェックを実行する（バックグラウンド用）
///
/// 1. レジストリから core 制約情報を取得
/// 2. 現在バージョンがブロック済みまたは min_version 未満の場合に
///    結果を pending_core_update に格納し "core-update-required" イベントも emit する
pub(crate) async fn run_core_constraint_check(
    app_handle: &AppHandle,
    pending: std::sync::Arc<tokio::sync::Mutex<Option<CoreUpdatePayload>>>,
) {
    let channel = get_current_channel();
    let current_version = env!("CARGO_PKG_VERSION");
    let updater = mcv_updater::UpdateChecker::new(crate::API_BASE_URL);

    let constraints = match updater.get_core_constraints(channel).await {
        Ok(c) => c,
        Err(e) => {
            tracing::warn!(
                target: "mcv::main",
                error = %e,
                "core 制約チェック: レジストリへの接続に失敗"
            );
            return;
        }
    };

    // ブロックチェック
    let is_blocked = constraints
        .blocked_versions
        .iter()
        .any(|b| b.version == current_version);

    // min_version チェック
    let below_min = constraints
        .min_version
        .as_deref()
        .filter(|min| compare_semver(current_version, min) == Ordering::Less)
        .is_some();

    if !is_blocked && !below_min {
        tracing::info!(
            target: "mcv::main",
            version = current_version,
            channel = channel,
            "core 制約チェック: 問題なし"
        );
        return;
    }

    let Some(target_version) = constraints.latest else {
        tracing::warn!(
            target: "mcv::main",
            version = current_version,
            channel = channel,
            "core 制約違反だがアップデート先バージョンが存在しません"
        );
        return;
    };

    tracing::warn!(
        target: "mcv::main",
        current = current_version,
        target = %target_version,
        channel = channel,
        is_blocked = is_blocked,
        below_min = below_min,
        "core 強制アップデートが必要です"
    );

    let payload = CoreUpdatePayload {
        target_version,
        channel: channel.to_string(),
        sha256: constraints.latest_sha256.unwrap_or_default(),
    };

    // フロントエンドが未ロードでも取得できるようキャッシュに保存
    *pending.lock().await = Some(payload.clone());

    // フロントエンドがすでにリスナーを登録済みであれば即時受信できる
    let _ = app_handle.emit("core-update-required", payload);
}

/// 制約違反プラグインの強制アップデートチェックを実行する（バックグラウンド用）
///
/// 1. レジストリからプラグイン一覧（制約情報込み）を取得してキャッシュに保存
/// 2. インストール済みプラグインと照合
/// 3. ブロック済み・min_version 未満のプラグインを強制アップデート
/// 4. 結果を "plugin-constraints-applied" イベントでフロントエンドへ通知
pub(crate) async fn run_plugin_constraint_check(
    settings_dir: &Path,
    app_handle: &AppHandle,
    core_addr: Addr<CoreActor>,
    plugin_manager: std::sync::Arc<tokio::sync::Mutex<mcv_core::PluginManager>>,
) {
    let updater = mcv_updater::UpdateChecker::new(crate::API_BASE_URL);

    // レジストリからプラグイン一覧を取得
    let plugins = match updater.list_plugins().await {
        Ok(p) => {
            // キャッシュに保存
            let cache = PluginConstraintsCache {
                fetched_at: {
                    use std::time::{SystemTime, UNIX_EPOCH};
                    let secs = SystemTime::now()
                        .duration_since(UNIX_EPOCH)
                        .unwrap_or_default()
                        .as_secs();
                    format!("{}", secs)
                },
                constraints: p
                    .iter()
                    .map(|item| PluginConstraintEntry {
                        id: item.id.clone(),
                        min_version: item.min_version.clone(),
                        blocked_versions: item.blocked_versions.clone(),
                        channels: Some(item.channels.clone()),
                    })
                    .collect(),
            };
            save_plugin_constraints(settings_dir, &cache);
            p
        }
        Err(e) => {
            tracing::warn!(
                target: "mcv::main",
                error = %e,
                "制約チェック: レジストリへの接続に失敗。キャッシュを使用します"
            );
            // オフライン時はキャッシュから読み込むが、ダウンロードが必要な強制アップデートは実行不可
            return;
        }
    };

    // インストール済みプラグインを取得
    let plugin_dir = get_plugin_dir();
    let installed = match scan_installed_plugins(&plugin_dir) {
        Ok(p) => p,
        Err(e) => {
            tracing::warn!(target: "mcv::main", error = %e, "制約チェック: インストール済みプラグインの取得に失敗");
            return;
        }
    };

    #[derive(serde::Serialize, Clone)]
    struct ConstraintResult {
        id: String,
        /// "updated" | "update_failed" | "no_update_available"
        action: String,
        reason: Option<String>,
    }

    let mut results: Vec<ConstraintResult> = Vec::new();

    for inst in &installed {
        let Some(registry) = plugins.iter().find(|p| p.id == inst.id) else {
            continue;
        };

        let channel = inst.channel.as_deref().unwrap_or("stable");
        let version = inst.version.as_deref().unwrap_or("0.0.0");

        // ブロックチェック
        let blocked = registry
            .blocked_versions
            .iter()
            .find(|b| b.version == version && b.channel == channel);

        // min_version チェック
        let below_min = registry.min_version.as_ref().and_then(|mv| {
            let min = match channel {
                "stable" => mv.stable.as_deref(),
                "beta" => mv.beta.as_deref(),
                "alpha" => mv.alpha.as_deref(),
                _ => None,
            };
            min.filter(|min_str| compare_semver(version, min_str) == Ordering::Less)
        });

        if blocked.is_none() && below_min.is_none() {
            continue;
        }

        let reason = blocked
            .and_then(|b| b.reason.as_deref())
            .or(below_min.map(|_| "最小バージョン要件を満たしていません"))
            .map(|s| s.to_string());

        tracing::warn!(
            target: "mcv::main",
            id = %inst.id,
            version = %version,
            channel = %channel,
            reason = ?reason,
            "制約違反プラグインを強制アップデートします"
        );

        // アップデート先バージョンを決定（そのチャンネルの最新版）
        let latest = match channel {
            "stable" => registry.channels.stable.as_deref(),
            "beta" => registry.channels.beta.as_deref(),
            "alpha" => registry.channels.alpha.as_deref(),
            _ => None,
        };

        match latest {
            Some(new_version) => {
                match install_registry_plugin_internal(
                    inst.id.clone(),
                    new_version.to_string(),
                    channel.to_string(),
                    core_addr.clone(),
                    std::sync::Arc::clone(&plugin_manager),
                )
                .await
                {
                    Ok(()) => {
                        tracing::info!(
                            target: "mcv::main",
                            id = %inst.id,
                            from = %version,
                            to = %new_version,
                            "強制アップデート完了"
                        );
                        results.push(ConstraintResult {
                            id: inst.id.clone(),
                            action: "updated".to_string(),
                            reason,
                        });
                    }
                    Err(e) => {
                        tracing::error!(
                            target: "mcv::main",
                            id = %inst.id,
                            error = %e,
                            "強制アップデート失敗"
                        );
                        results.push(ConstraintResult {
                            id: inst.id.clone(),
                            action: "update_failed".to_string(),
                            reason: Some(e),
                        });
                    }
                }
            }
            None => {
                tracing::warn!(
                    target: "mcv::main",
                    id = %inst.id,
                    channel = %channel,
                    "制約違反だがアップデート先バージョンが存在しません"
                );
                results.push(ConstraintResult {
                    id: inst.id.clone(),
                    action: "no_update_available".to_string(),
                    reason,
                });
            }
        }
    }

    if !results.is_empty() {
        let _ = app_handle.emit("plugin-constraints-applied", &results);
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn compare_semver_equal() {
        assert_eq!(compare_semver("1.2.3", "1.2.3"), Ordering::Equal);
        assert_eq!(compare_semver("0.0.0", "0.0.0"), Ordering::Equal);
    }

    #[test]
    fn compare_semver_major() {
        assert_eq!(compare_semver("2.0.0", "1.0.0"), Ordering::Greater);
        assert_eq!(compare_semver("1.0.0", "2.0.0"), Ordering::Less);
    }

    #[test]
    fn compare_semver_minor() {
        assert_eq!(compare_semver("1.3.0", "1.2.0"), Ordering::Greater);
        assert_eq!(compare_semver("1.2.0", "1.3.0"), Ordering::Less);
    }

    #[test]
    fn compare_semver_patch() {
        assert_eq!(compare_semver("1.2.4", "1.2.3"), Ordering::Greater);
        assert_eq!(compare_semver("1.2.3", "1.2.4"), Ordering::Less);
    }

    #[test]
    fn compare_semver_partial_version() {
        // パッチなし → 0 として扱う
        assert_eq!(compare_semver("1.2", "1.2.0"), Ordering::Equal);
        // メジャーのみ → 1.0.0 として扱う
        assert_eq!(compare_semver("1", "1.0.0"), Ordering::Equal);
    }

    #[test]
    fn compare_semver_empty_or_invalid() {
        // 空文字列は 0.0.0
        assert_eq!(compare_semver("", "0.0.0"), Ordering::Equal);
        // 非数値は 0 として扱う
        assert_eq!(compare_semver("abc", "0.0.0"), Ordering::Equal);
        assert_eq!(compare_semver("1.0.0", "abc"), Ordering::Greater);
    }
}
