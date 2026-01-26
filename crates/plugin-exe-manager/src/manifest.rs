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

/// manifest.jsonのスキーマ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginManifest {
    pub schema_version: String,
    pub plugin: PluginInfo,
    pub executable: ExecutableInfo,
    #[serde(default)]
    pub websocket: WebSocketConfig,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginInfo {
    pub id: String,
    pub name: String,
    pub version: String,
    pub api_version: String,
    #[serde(default)]
    pub description: String,
    #[serde(default)]
    pub author: String,
    pub roles: Vec<String>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ExecutableInfo {
    pub path: PathBuf,
    #[serde(default)]
    pub args: Vec<String>,
    #[serde(default = "default_working_directory")]
    pub working_directory: PathBuf,
}

fn default_working_directory() -> PathBuf {
    PathBuf::from(".")
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct WebSocketConfig {
    #[serde(default = "default_auto_reconnect")]
    pub auto_reconnect: bool,
    #[serde(default = "default_reconnect_interval_ms")]
    pub reconnect_interval_ms: u64,
    #[serde(default = "default_timeout_ms")]
    pub timeout_ms: u64,
}

fn default_auto_reconnect() -> bool {
    true
}

fn default_reconnect_interval_ms() -> u64 {
    5000
}

fn default_timeout_ms() -> u64 {
    30000
}

impl Default for WebSocketConfig {
    fn default() -> Self {
        Self {
            auto_reconnect: default_auto_reconnect(),
            reconnect_interval_ms: default_reconnect_interval_ms(),
            timeout_ms: default_timeout_ms(),
        }
    }
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
        if self.schema_version.is_empty() {
            return Err(ManifestError::Invalid("schema_version is empty".to_string()));
        }

        if self.plugin.id.is_empty() {
            return Err(ManifestError::Invalid("plugin.id is empty".to_string()));
        }

        if self.plugin.name.is_empty() {
            return Err(ManifestError::Invalid("plugin.name is empty".to_string()));
        }

        if self.plugin.version.is_empty() {
            return Err(ManifestError::Invalid("plugin.version is empty".to_string()));
        }

        if self.plugin.api_version.is_empty() {
            return Err(ManifestError::Invalid("plugin.api_version is empty".to_string()));
        }

        if self.plugin.roles.is_empty() {
            return Err(ManifestError::Invalid("plugin.roles is empty".to_string()));
        }

        if self.executable.path.as_os_str().is_empty() {
            return Err(ManifestError::Invalid("executable.path is empty".to_string()));
        }

        Ok(())
    }

    /// 実行ファイルの絶対パスを取得
    ///
    /// # Arguments
    /// * `manifest_dir` - manifest.jsonが配置されているディレクトリ
    pub fn get_executable_path<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        let manifest_dir = manifest_dir.as_ref();
        manifest_dir.join(&self.executable.path)
    }

    /// 作業ディレクトリの絶対パスを取得
    ///
    /// # Arguments
    /// * `manifest_dir` - manifest.jsonが配置されているディレクトリ
    pub fn get_working_directory<P: AsRef<Path>>(&self, manifest_dir: P) -> PathBuf {
        let manifest_dir = manifest_dir.as_ref();
        manifest_dir.join(&self.executable.working_directory)
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
            "schema_version": "1.0",
            "plugin": {
                "id": "com.example.test",
                "name": "Test Plugin",
                "version": "1.0.0",
                "api_version": "v2",
                "description": "Test",
                "author": "Test Author",
                "roles": ["comment-provider"]
            },
            "executable": {
                "path": "bin/test.exe",
                "args": [],
                "working_directory": "."
            },
            "websocket": {
                "auto_reconnect": true,
                "reconnect_interval_ms": 5000,
                "timeout_ms": 30000
            }
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert_eq!(manifest.schema_version, "1.0");
        assert_eq!(manifest.plugin.id, "com.example.test");
        assert_eq!(manifest.plugin.name, "Test Plugin");
        assert_eq!(manifest.executable.path, PathBuf::from("bin/test.exe"));
    }

    #[test]
    fn test_manifest_validation() {
        let json = r#"{
            "schema_version": "1.0",
            "plugin": {
                "id": "",
                "name": "Test Plugin",
                "version": "1.0.0",
                "api_version": "v2",
                "roles": ["comment-provider"]
            },
            "executable": {
                "path": "bin/test.exe"
            }
        }"#;

        let manifest: PluginManifest = serde_json::from_str(json).unwrap();
        assert!(manifest.validate().is_err());
    }

    #[test]
    fn test_manifest_load() {
        let temp_dir = TempDir::new().unwrap();
        let manifest_path = temp_dir.path().join("manifest.json");

        let json = r#"{
            "schema_version": "1.0",
            "plugin": {
                "id": "com.example.test",
                "name": "Test Plugin",
                "version": "1.0.0",
                "api_version": "v2",
                "roles": ["comment-provider"]
            },
            "executable": {
                "path": "bin/test.exe"
            }
        }"#;

        let mut file = fs::File::create(&manifest_path).unwrap();
        file.write_all(json.as_bytes()).unwrap();

        let manifest = PluginManifest::load(&manifest_path).unwrap();
        assert_eq!(manifest.plugin.id, "com.example.test");
    }
}
