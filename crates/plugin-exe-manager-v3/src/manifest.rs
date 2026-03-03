use serde::{Deserialize, Serialize};
use std::path::{Path, PathBuf};
use thiserror::Error;

#[derive(Debug, Error)]
pub enum ManifestError {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("JSON parse error: {0}")]
    Json(#[from] serde_json::Error),

    #[error("Invalid manifest: {0}")]
    Invalid(String),
}

/// plugin.jsonのシンプルなスキーマ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginManifest {
    pub id: String,
    pub name: String,
    pub description: String,
    #[serde(default)]
    pub path: String,
    pub entry: String,
    #[serde(default)]
    pub has_channel_feature: bool,
}

impl PluginManifest {
    /// plugin.jsonを読み込む
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, ManifestError> {
        let content = std::fs::read_to_string(&path)?;
        //BOM付きの場合があるかもしれないので除去
        let content = content.trim_start_matches('\u{feff}');
        let manifest: PluginManifest = serde_json::from_str(&content)?;

        // バリデーション
        manifest.validate()?;

        Ok(manifest)
    }

    /// plugin.jsonのバリデーション
    fn validate(&self) -> Result<(), ManifestError> {
        if self.id.trim().is_empty() {
            return Err(ManifestError::Invalid("id is empty".to_string()));
        }

        if self.name.trim().is_empty() {
            return Err(ManifestError::Invalid("name is empty".to_string()));
        }

        if self.description.trim().is_empty() {
            return Err(ManifestError::Invalid("description is empty".to_string()));
        }

        if self.entry.trim().is_empty() {
            return Err(ManifestError::Invalid("entry is empty".to_string()));
        }

        if self.path.contains("..") {
            return Err(ManifestError::Invalid("path traversal is not allowed".to_string()));
        }

        Ok(())
    }

    /// 実行ファイルの絶対パスを取得
    ///
    /// # Arguments
    /// * `manifest_dir` - plugin.jsonが配置されているディレクトリ
    pub fn get_executable_path<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        self.get_working_directory(manifest_dir).join(&self.entry)
    }

    /// 作業ディレクトリの絶対パスを取得（plugin.jsonと同じディレクトリ）
    ///
    /// # Arguments
    /// * `manifest_dir` - plugin.jsonが配置されているディレクトリ
    pub fn get_working_directory<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        let manifest_dir = manifest_dir.as_ref();
        if self.path.is_empty() {
            manifest_dir.to_path_buf()
        } else {
            manifest_dir.join(&self.path)
        }
    }

    /// プラグインIDを取得
    pub fn get_plugin_id<P: AsRef<Path>>(&self, manifest_dir: P) -> String {
        let _ = manifest_dir;
        self.id.clone()
    }

    /// プラグイン名を取得
    pub fn get_plugin_name(&self) -> String {
        self.name.clone()
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use std::fs;
    use std::io::Write;
    use tempfile::TempDir;

    #[test]
    fn test_manifest_deserialization() {
        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "path": "apps/exe-plugin-sample/src-tauri",
            "entry": "exe-plugin-sample.exe",
            "has_channel_feature": false
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.id, "exe-plugin-sample");
        assert_eq!(manifest.entry, "exe-plugin-sample.exe");
    }

    #[test]
    fn test_manifest_validation() {
        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "entry": ""
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert!(manifest.validate().is_err());
    }

    #[test]
    fn test_manifest_load() {
        let temp_dir = TempDir::new().unwrap();
        let manifest_path = temp_dir.path().join("plugin.json");

        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "entry": "test.exe"
        }"#;

        let mut file = fs::File::create(&manifest_path).unwrap();
        file.write_all(json.as_bytes()).unwrap();

        let manifest = PluginManifest::load(&manifest_path).unwrap();
        assert_eq!(manifest.entry, "test.exe");
    }

    #[test]
    fn test_get_plugin_name() {
        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "entry": "exe-plugin-sample.exe"
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.get_plugin_name(), "EXEプラグインサンプル");
    }

    #[test]
    fn test_path_optional() {
        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "entry": "exe-plugin-sample.exe",
            "has_channel_feature": false
        }"#;
        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.path, "");
    }

    #[test]
    fn test_get_executable_path_uses_entry() {
        let json = r#"{
            "id": "exe-plugin-sample",
            "name": "EXEプラグインサンプル",
            "description": "EXEプラグインのデバッグ・開発用サンプルツールです",
            "path": "bin",
            "entry": "exe-plugin-sample.exe",
            "has_channel_feature": false
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        let dir = PathBuf::from("C:/plugins/sample");
        assert_eq!(
            manifest.get_executable_path(&dir),
            PathBuf::from("C:/plugins/sample/bin/exe-plugin-sample.exe")
        );
    }
}
