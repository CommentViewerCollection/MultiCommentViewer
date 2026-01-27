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

/// manifest.jsonのシンプルなスキーマ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginManifest {
    pub path: String,
}

impl PluginManifest {
    /// manifest.jsonを読み込む
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, ManifestError> {
        let content = std::fs::read_to_string(&path)?;
        let manifest: PluginManifest = serde_json::from_str(&content)?;

        // バリデーション
        manifest.validate()?;

        Ok(manifest)
    }

    /// manifest.jsonのバリデーション
    fn validate(&self) -> Result<(), ManifestError> {
        if self.path.is_empty() {
            return Err(ManifestError::Invalid("path is empty".to_string()));
        }

        Ok(())
    }

    /// 実行ファイルの絶対パスを取得
    ///
    /// # Arguments
    /// * `manifest_dir` - manifest.jsonが配置されているディレクトリ
    pub fn get_executable_path<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        let manifest_dir = manifest_dir.as_ref();
        manifest_dir.join(&self.path)
    }

    /// 作業ディレクトリの絶対パスを取得（manifest.jsonと同じディレクトリ）
    ///
    /// # Arguments
    /// * `manifest_dir` - manifest.jsonが配置されているディレクトリ
    pub fn get_working_directory<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        manifest_dir.as_ref().to_path_buf()
    }

    /// プラグインIDを生成（ディレクトリ名から）
    pub fn get_plugin_id<P: AsRef<Path>>(&self, manifest_dir: P) -> String {
        manifest_dir
            .as_ref()
            .file_name()
            .and_then(|s| s.to_str())
            .unwrap_or("unknown")
            .to_string()
    }

    /// プラグイン名を生成（実行ファイル名から）
    pub fn get_plugin_name(&self) -> String {
        PathBuf::from(&self.path)
            .file_stem()
            .and_then(|s| s.to_str())
            .unwrap_or("Unknown Plugin")
            .to_string()
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
            "path": "exe-plugin-sample.exe"
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.path, "exe-plugin-sample.exe");
    }

    #[test]
    fn test_manifest_validation() {
        let json = r#"{
            "path": ""
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert!(manifest.validate().is_err());
    }

    #[test]
    fn test_manifest_load() {
        let temp_dir = TempDir::new().unwrap();
        let manifest_path = temp_dir.path().join("manifest.json");

        let json = r#"{
            "path": "test.exe"
        }"#;

        let mut file = fs::File::create(&manifest_path).unwrap();
        file.write_all(json.as_bytes()).unwrap();

        let manifest = PluginManifest::load(&manifest_path).unwrap();
        assert_eq!(manifest.path, "test.exe");
    }

    #[test]
    fn test_get_plugin_name() {
        let json = r#"{
            "path": "exe-plugin-sample.exe"
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.get_plugin_name(), "exe-plugin-sample");
    }
}
