use serde::{Deserialize, Serialize};
use std::path::{Path, PathBuf};

type SettingsResult<T> = Result<T, Box<dyn std::error::Error>>;

/// 設定エントリ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SettingsEntry {
    pub target: String,            // "core" または plugin_id
    pub schema: serde_json::Value, // JSON Schema（保存には使用しない）
    pub data: serde_json::Value,   // 設定値
    pub updated_at: i64,           // Unix timestamp（保存には使用しない）
}

/// 設定ストレージ（JSON ファイルベース）
///
/// 各 target の設定値を `{settings_dir}/{target}.json` に保存する。
pub struct SettingsStorage {
    settings_dir: PathBuf,
}

impl SettingsStorage {
    /// 新しいストレージを作成する。`settings_dir` が存在しない場合は作成する。
    pub fn new<P: AsRef<Path>>(settings_dir: P) -> SettingsResult<Self> {
        let settings_dir = settings_dir.as_ref().to_path_buf();
        std::fs::create_dir_all(&settings_dir)?;
        Ok(Self { settings_dir })
    }

    fn file_path(&self, target: &str) -> PathBuf {
        self.settings_dir.join(format!("{}.json", target))
    }

    /// 設定を保存する（entry.data を JSON ファイルに書き込む）
    pub fn save_settings(&self, entry: &SettingsEntry) -> SettingsResult<()> {
        let path = self.file_path(&entry.target);
        let json = serde_json::to_string_pretty(&entry.data)?;
        std::fs::write(&path, json)?;
        Ok(())
    }

    /// 設定を取得する。ファイルが存在しない場合は `Ok(None)` を返す。
    pub fn get_settings(&self, target: &str) -> SettingsResult<Option<SettingsEntry>> {
        let path = self.file_path(target);
        if !path.exists() {
            return Ok(None);
        }
        let json = std::fs::read_to_string(&path)?;
        let data: serde_json::Value = serde_json::from_str(&json)?;
        Ok(Some(SettingsEntry {
            target: target.to_string(),
            schema: serde_json::Value::Null,
            data,
            updated_at: 0,
        }))
    }

    /// すべての設定を取得する（ディレクトリ内の *.json を列挙）
    pub fn list_all_settings(&self) -> SettingsResult<Vec<SettingsEntry>> {
        let mut entries = Vec::new();
        for entry in std::fs::read_dir(&self.settings_dir)? {
            let entry = entry?;
            let path = entry.path();
            if path.extension().and_then(|e| e.to_str()) != Some("json") {
                continue;
            }
            let target = match path.file_stem().and_then(|s| s.to_str()) {
                Some(s) => s.to_string(),
                None => continue,
            };
            let json = std::fs::read_to_string(&path)?;
            let data: serde_json::Value = serde_json::from_str(&json)?;
            entries.push(SettingsEntry {
                target,
                schema: serde_json::Value::Null,
                data,
                updated_at: 0,
            });
        }
        Ok(entries)
    }

    /// 設定を削除する
    pub fn delete_settings(&self, target: &str) -> SettingsResult<()> {
        let path = self.file_path(target);
        if path.exists() {
            std::fs::remove_file(&path)?;
        }
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    fn temp_dir() -> PathBuf {
        let dir = std::env::temp_dir().join(format!(
            "mcv-settings-test-{}",
            std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .unwrap()
                .subsec_nanos()
        ));
        std::fs::create_dir_all(&dir).unwrap();
        dir
    }

    #[test]
    fn test_settings_storage_crud() {
        let dir = temp_dir();
        let storage = SettingsStorage::new(&dir).unwrap();

        // 保存
        let entry = SettingsEntry {
            target: "core".to_string(),
            schema: serde_json::json!({}),
            data: serde_json::json!({
                "theme": "dark"
            }),
            updated_at: 1234567890,
        };

        storage.save_settings(&entry).unwrap();

        // 取得
        let retrieved = storage.get_settings("core").unwrap().unwrap();
        assert_eq!(retrieved.target, "core");
        assert_eq!(retrieved.data["theme"], "dark");

        // 更新
        let updated_entry = SettingsEntry {
            target: "core".to_string(),
            schema: serde_json::json!({}),
            data: serde_json::json!({
                "theme": "light"
            }),
            updated_at: 1234567900,
        };

        storage.save_settings(&updated_entry).unwrap();

        let retrieved = storage.get_settings("core").unwrap().unwrap();
        assert_eq!(retrieved.data["theme"], "light");

        // 全件取得
        let all = storage.list_all_settings().unwrap();
        assert_eq!(all.len(), 1);

        // 削除
        storage.delete_settings("core").unwrap();
        let retrieved = storage.get_settings("core").unwrap();
        assert!(retrieved.is_none());

        // クリーンアップ
        std::fs::remove_dir_all(&dir).ok();
    }
}
