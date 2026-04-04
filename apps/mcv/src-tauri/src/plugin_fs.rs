use std::collections::HashMap;
use std::path::{Path, PathBuf};

/// インストール済みプラグインのメタ情報（ID + バージョン + チャンネル）
#[derive(serde::Serialize)]
pub(crate) struct InstalledPluginMeta {
    pub(crate) id: String,
    pub(crate) version: Option<String>,
    pub(crate) channel: Option<String>,
}

/// pluginsディレクトリのパスを返す
pub(crate) fn get_plugin_dir() -> PathBuf {
    mcv_common::get_base_dir().join("plugins")
}

/// plugin.json を読んで (id, version, channel) を返す（BOM 対応）
/// id が空文字列の場合は None を返す
pub(crate) fn read_plugin_manifest(
    path: &Path,
) -> Option<(String, Option<String>, Option<String>)> {
    #[derive(serde::Deserialize)]
    struct PluginJsonFull {
        #[serde(default)]
        id: Option<String>,
        #[serde(default)]
        version: Option<String>,
        #[serde(default)]
        channel: Option<String>,
    }
    let content = std::fs::read(path).ok()?;
    let content = if content.starts_with(b"\xEF\xBB\xBF") {
        &content[3..]
    } else {
        &content[..]
    };
    let meta: PluginJsonFull = serde_json::from_slice(content).ok()?;
    let id = meta.id.filter(|s| !s.is_empty())?;
    Some((id, meta.version, meta.channel))
}

/// plugin.json から id フィールドを読み取る（BOM 対応）
pub(crate) fn read_plugin_id_from_manifest(manifest_path: &Path) -> Option<String> {
    #[derive(serde::Deserialize)]
    struct Manifest {
        #[serde(default)]
        id: Option<String>,
    }
    let content = std::fs::read(manifest_path).ok()?;
    let content = if content.starts_with(b"\xEF\xBB\xBF") {
        &content[3..]
    } else {
        &content
    };
    let manifest: Manifest = serde_json::from_slice(content).ok()?;
    manifest.id.filter(|s| !s.is_empty())
}

/// pluginsディレクトリをスキャンしてインストール済みプラグイン一覧を返す
///
/// - ディレクトリ形式（{id}/plugin.json）: manifest の `id`・`version`・`channel` を使用
/// - ZIP 形式（*.zip）: `.cache/{stem}/plugin.json` から `id`・`version`・`channel` を取得
///   （ZIP ファイル名は `{id}-{version}-{channel}.zip` などで変わりうるため、
///   ファイル名ステムではなく plugin.json の `id` フィールドを使用する）
pub(crate) fn scan_installed_plugins(
    plugin_dir: &Path,
) -> Result<Vec<InstalledPluginMeta>, String> {
    if !plugin_dir.exists() {
        return Ok(vec![]);
    }

    // id → InstalledPluginMeta（重複時はディレクトリ形式を優先）
    let mut result: HashMap<String, InstalledPluginMeta> = HashMap::new();

    for entry in std::fs::read_dir(plugin_dir)
        .map_err(|e| format!("Failed to read plugin directory: {}", e))?
        .flatten()
    {
        let path = entry.path();
        if path.is_dir() {
            let dir_name = match path.file_name().and_then(|n| n.to_str()) {
                Some(n) => n.to_string(),
                None => continue,
            };
            // 隠しディレクトリ（.cache 等）はスキップ
            if dir_name.starts_with('.') {
                continue;
            }
            // .deleted.plugin.json が存在する → plugin.json がリネームされた削除待ち状態
            // plugin.json が存在しないためこの後の exists() チェックで自然にスキップされるが、
            // 明示的にここでスキップして意図を示す
            if path.join(".deleted.plugin.json").exists() {
                continue;
            }
            let manifest_path = path.join("plugin.json");
            if !manifest_path.exists() {
                continue;
            }
            let (id, version, channel) = match read_plugin_manifest(&manifest_path) {
                Some(m) => m,
                None => continue,
            };
            let marker = plugin_dir.join(format!(".uninstall-{}", &id));
            if marker.exists() {
                continue;
            }
            // ディレクトリ形式を優先して挿入
            result.insert(
                id.clone(),
                InstalledPluginMeta {
                    id,
                    version,
                    channel,
                },
            );
        } else if path
            .extension()
            .and_then(|e| e.to_str())
            .map(|e| e.eq_ignore_ascii_case("zip"))
            .unwrap_or(false)
        {
            let stem = match path.file_stem().and_then(|s| s.to_str()) {
                Some(s) => s.to_string(),
                None => continue,
            };
            // キャッシュから実際の id・version・channel を取得
            // （起動時に展開済みのため .cache/{stem}/plugin.json が存在する）
            let cache_manifest = plugin_dir.join(".cache").join(&stem).join("plugin.json");
            let (id, version, channel) = match read_plugin_manifest(&cache_manifest) {
                Some(m) => m,
                // キャッシュ未展開（初回起動前）はスキップ
                None => continue,
            };
            let marker = plugin_dir.join(format!(".uninstall-{}", &id));
            if marker.exists() {
                continue;
            }
            // ディレクトリ形式がまだ挿入されていない場合のみ追加
            result.entry(id.clone()).or_insert(InstalledPluginMeta {
                id,
                version,
                channel,
            });
        }
    }

    Ok(result.into_values().collect())
}

/// .cache/ 以下を走査して plugin_id に対応する ZIP ステム（キャッシュサブディレクトリ名）を返す
///
/// ZIP ファイル名は `{id}-{version}-{channel}.zip` のように id 以外の情報を含む場合があるため、
/// ファイル名ではなく .cache/{stem}/plugin.json の `id` フィールドで照合する。
pub(crate) fn find_zip_stem_for_id(plugin_dir: &Path, plugin_id: &str) -> Option<String> {
    let cache_root = plugin_dir.join(".cache");
    if !cache_root.exists() {
        return None;
    }
    for entry in std::fs::read_dir(&cache_root).ok()?.flatten() {
        let path = entry.path();
        if !path.is_dir() {
            continue;
        }
        let stem = path.file_name()?.to_str()?.to_string();
        if stem.starts_with('.') {
            continue;
        }
        let manifest = path.join("plugin.json");
        if read_plugin_id_from_manifest(&manifest)
            .as_deref()
            .unwrap_or("")
            == plugin_id
        {
            return Some(stem);
        }
    }
    None
}

/// 前回セッションでアンインストール待ちになったエントリを削除する
///
/// `.uninstall-{id}` がディレクトリの場合はそのまま削除（リネーム成功済み）。
/// ファイルの場合はマーカーファイルで、対応する `{id}/` ディレクトリの削除を試みてからマーカーを消す。
pub(crate) fn cleanup_pending_uninstalls(plugin_dir: &Path) {
    // plugin_dir 直下の .uninstall-* エントリを処理
    if let Ok(entries) = std::fs::read_dir(plugin_dir) {
        for entry in entries.flatten() {
            let path = entry.path();
            let name = match path.file_name().and_then(|n| n.to_str()) {
                Some(n) => n.to_string(),
                None => continue,
            };
            if !name.starts_with(".uninstall-") {
                continue;
            }
            if path.is_dir() {
                // リネーム済みのディレクトリ → 削除
                tracing::info!(target: "mcv::main", path = %path.display(), "Cleaning up pending uninstall directory");
                let _ = std::fs::remove_dir_all(&path);
            } else if path.is_file() {
                // マーカーファイル → 実際のディレクトリ削除を試みてからマーカーを消す
                let plugin_id = &name[".uninstall-".len()..];
                let target_dir = plugin_dir.join(plugin_id);
                tracing::info!(target: "mcv::main", id = %plugin_id, "Cleaning up plugin directory from uninstall marker");
                if target_dir.exists() {
                    if std::fs::remove_dir_all(&target_dir).is_ok() {
                        let _ = std::fs::remove_file(&path);
                    }
                    // まだ失敗する場合はマーカーを残して次回再試行
                } else {
                    let _ = std::fs::remove_file(&path);
                }
            }
        }
    }
    // .cache/ 下の .uninstall-* ディレクトリも削除
    let cache_root = plugin_dir.join(".cache");
    if let Ok(entries) = std::fs::read_dir(&cache_root) {
        for entry in entries.flatten() {
            let path = entry.path();
            if path.is_dir()
                && path
                    .file_name()
                    .and_then(|n| n.to_str())
                    .map(|n| n.starts_with(".uninstall-"))
                    .unwrap_or(false)
            {
                tracing::info!(target: "mcv::main", path = %path.display(), "Cleaning up pending uninstall cache directory");
                let _ = std::fs::remove_dir_all(&path);
            }
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;

    fn write_plugin_json(dir: &Path, id: &str, version: &str, channel: &str) {
        let json = serde_json::json!({
            "id": id,
            "version": version,
            "channel": channel,
        });
        fs::write(dir.join("plugin.json"), json.to_string()).unwrap();
    }

    #[test]
    fn scan_returns_empty_for_nonexistent_dir() {
        let dir = PathBuf::from("/nonexistent/path/that/does/not/exist");
        let result = scan_installed_plugins(&dir).unwrap();
        assert!(result.is_empty());
    }

    #[test]
    fn scan_finds_directory_format_plugin() {
        let tmp = tempfile::tempdir().unwrap();
        let plugin_dir = tmp.path().join("plugins");
        fs::create_dir_all(&plugin_dir).unwrap();

        let plugin_subdir = plugin_dir.join("my-plugin");
        fs::create_dir_all(&plugin_subdir).unwrap();
        write_plugin_json(&plugin_subdir, "my-plugin", "1.0.0", "stable");

        let result = scan_installed_plugins(&plugin_dir).unwrap();
        assert_eq!(result.len(), 1);
        assert_eq!(result[0].id, "my-plugin");
        assert_eq!(result[0].version.as_deref(), Some("1.0.0"));
        assert_eq!(result[0].channel.as_deref(), Some("stable"));
    }

    #[test]
    fn scan_skips_deleted_plugin() {
        let tmp = tempfile::tempdir().unwrap();
        let plugin_dir = tmp.path().join("plugins");
        fs::create_dir_all(&plugin_dir).unwrap();

        let plugin_subdir = plugin_dir.join("deleted-plugin");
        fs::create_dir_all(&plugin_subdir).unwrap();
        // plugin.json の代わりに .deleted.plugin.json が存在 → 削除待ち
        fs::write(plugin_subdir.join(".deleted.plugin.json"), "{}").unwrap();

        let result = scan_installed_plugins(&plugin_dir).unwrap();
        assert!(result.is_empty());
    }

    #[test]
    fn scan_skips_uninstall_marker() {
        let tmp = tempfile::tempdir().unwrap();
        let plugin_dir = tmp.path().join("plugins");
        fs::create_dir_all(&plugin_dir).unwrap();

        let plugin_subdir = plugin_dir.join("marked-plugin");
        fs::create_dir_all(&plugin_subdir).unwrap();
        write_plugin_json(&plugin_subdir, "marked-plugin", "1.0.0", "stable");
        // アンインストールマーカーが存在する
        fs::write(plugin_dir.join(".uninstall-marked-plugin"), b"").unwrap();

        let result = scan_installed_plugins(&plugin_dir).unwrap();
        assert!(result.is_empty());
    }

    #[test]
    fn scan_directory_takes_priority_over_zip() {
        let tmp = tempfile::tempdir().unwrap();
        let plugin_dir = tmp.path().join("plugins");
        fs::create_dir_all(&plugin_dir).unwrap();

        // ディレクトリ形式
        let plugin_subdir = plugin_dir.join("dup-plugin");
        fs::create_dir_all(&plugin_subdir).unwrap();
        write_plugin_json(&plugin_subdir, "dup-plugin", "2.0.0", "stable");

        // ZIP キャッシュ形式（同じ id）
        let cache_dir = plugin_dir.join(".cache").join("dup-plugin");
        fs::create_dir_all(&cache_dir).unwrap();
        write_plugin_json(&cache_dir, "dup-plugin", "1.0.0", "stable");
        // ダミー ZIP ファイルを配置
        fs::write(plugin_dir.join("dup-plugin.zip"), b"PK").unwrap();

        let result = scan_installed_plugins(&plugin_dir).unwrap();
        assert_eq!(result.len(), 1);
        // ディレクトリ形式のバージョン（2.0.0）が優先される
        assert_eq!(result[0].version.as_deref(), Some("2.0.0"));
    }

    #[test]
    fn read_plugin_manifest_bom() {
        let tmp = tempfile::tempdir().unwrap();
        let path = tmp.path().join("plugin.json");
        // UTF-8 BOM 付き JSON
        let json =
            b"\xEF\xBB\xBF{\"id\":\"bom-plugin\",\"version\":\"0.1.0\",\"channel\":\"alpha\"}";
        fs::write(&path, json).unwrap();

        let result = read_plugin_manifest(&path);
        assert!(result.is_some());
        let (id, version, channel) = result.unwrap();
        assert_eq!(id, "bom-plugin");
        assert_eq!(version.as_deref(), Some("0.1.0"));
        assert_eq!(channel.as_deref(), Some("alpha"));
    }
}
