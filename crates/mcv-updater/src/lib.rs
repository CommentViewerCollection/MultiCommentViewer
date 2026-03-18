use reqwest::Client;
use semver::Version;
use serde::{Deserialize, Serialize};
use sha2::{Digest, Sha256};
use std::path::Path;
use thiserror::Error;
use tokio::io::AsyncWriteExt;

fn default_true() -> bool {
    true
}

/// アップデート情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UpdateInfo {
    pub version: String,
    pub download_url: String,
    pub sha256: String,
    pub release_notes: String,
    pub released_at: String,
}

/// インストーラアップデート情報（必須フラグ付き）
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct InstallerUpdateInfo {
    pub version: String,
    pub channel: String,
    #[serde(rename = "fileName")]
    pub file_name: String,
    pub sha256: String,
}

/// プラグインのチャンネル情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginChannels {
    pub stable: Option<String>,
    pub beta: Option<String>,
    pub alpha: Option<String>,
}

/// プラグイン一覧の各アイテム
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginListItem {
    pub id: String,
    pub name: String,
    pub description: String,
    pub channels: PluginChannels,
    #[serde(default)]
    pub download_count: u64,
}

/// プラグイン詳細レスポンス
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginDetail {
    pub id: String,
    pub name: String,
    pub description: Option<String>,
    pub versions: Vec<PluginVersionDetail>,
}

/// プラグインの各バージョン情報
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct PluginVersionDetail {
    pub version: String,
    pub channel: String,
    pub file_name: String,
    pub sha256: String,
    #[serde(default = "default_true")]
    pub is_public: bool,
    #[serde(default)]
    pub is_deleted: bool,
}

/// プラグイン詳細情報（将来の拡張用）
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginInfo {
    pub id: String,
    pub name: String,
    pub description: String,
    pub version: String,
    pub download_url: String,
    pub sha256: String,
    pub file_size: u64,
    pub author: String,
    pub license: String,
    pub released_at: String,
    pub min_mcv_version: String,
}

/// mcv本体アップデート情報（拡張版）
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "camelCase")]
pub struct McvUpdateInfo {
    pub version: String,
    pub channel: String,
    pub file_name: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub file_size: Option<u64>,
    pub sha256: String,
    pub uploaded_at: String,
}

/// アップデートエラー
#[derive(Error, Debug)]
pub enum UpdateError {
    #[error("HTTP request failed: {0}")]
    HttpError(#[from] reqwest::Error),

    #[error("Failed to parse version: {0}")]
    VersionParseError(#[from] semver::Error),

    #[error("Failed to parse JSON: {0}")]
    JsonParseError(String),

    #[error("IO error: {0}")]
    IoError(#[from] std::io::Error),

    #[error("Checksum mismatch: expected {expected}, got {actual}")]
    ChecksumMismatch { expected: String, actual: String },

    #[error("No update available")]
    NoUpdateAvailable,

    #[error("Invalid URL: {0}")]
    InvalidUrl(String),
}

/// アップデートチェッカー
pub struct UpdateChecker {
    api_base_url: String,
    client: Client,
}

impl UpdateChecker {
    /// 新しいUpdateCheckerを作成
    ///
    /// # Arguments
    /// * `api_base_url` - 配布APIのベースURL（例: "https://api.example.com"）
    pub fn new(api_base_url: impl Into<String>) -> Self {
        Self {
            api_base_url: api_base_url.into(),
            client: Client::builder()
                .user_agent("McvUpdater/1.0.0")
                .build()
                .expect("Failed to create HTTP client"),
        }
    }

    /// インストーラの更新をチェック
    ///
    /// # Arguments
    /// * `current_version` - 現在のインストーラバージョン
    ///
    /// # Returns
    /// 更新が利用可能な場合は `Some(InstallerUpdateInfo)`、それ以外は `None`
    pub async fn check_installer_update(
        &self,
        current_version: &str,
    ) -> Result<Option<InstallerUpdateInfo>, UpdateError> {
        #[cfg(feature = "alpha")]
        let url = format!("{}/api/mcv/installer/latest/alpha", self.api_base_url);
        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        let url = format!("{}/api/mcv/installer/latest/beta", self.api_base_url);
        #[cfg(not(any(feature = "alpha", feature = "beta")))]
        let url = format!("{}/api/mcv/installer/latest/stable", self.api_base_url);
        let response = self.client.get(&url).send().await?;

        if response.status() == reqwest::StatusCode::NOT_FOUND {
            // サーバー仕様上、チャンネルにバージョンが存在しない場合も 404 を返す。
            // ただし、エンドポイントのURL変更等による誤った 404 と区別するため、
            // Content-Type が application/json のときのみ「バージョン未登録」と判断する。
            // HTML 等が返った場合はエンドポイント不正とみなしてエラーにする。
            let is_json = response
                .headers()
                .get(reqwest::header::CONTENT_TYPE)
                .and_then(|v| v.to_str().ok())
                .map(|v| v.contains("application/json"))
                .unwrap_or(false);
            return if is_json { Ok(None) } else { Err(UpdateError::HttpError(response.error_for_status().unwrap_err())) };
        }
        if !response.status().is_success() {
            return Err(UpdateError::HttpError(
                response.error_for_status().unwrap_err(),
            ));
        }
        let info: InstallerUpdateInfo = response.json().await?;

        let current = Version::parse(current_version)?;
        let latest = Version::parse(&info.version)?;

        if latest > current {
            Ok(Some(info))
        } else {
            Ok(None)
        }
    }
    fn get_mcv_update_endpoint(&self, api_base_url: impl Into<String>) -> String {
        #[cfg(feature = "alpha")]
        { format!("{}/api/mcv/core/latest/alpha", api_base_url.into()) }

        #[cfg(all(feature = "beta", not(feature = "alpha")))]
        { format!("{}/api/mcv/core/latest/beta", api_base_url.into()) }

        #[cfg(not(any(feature = "alpha", feature = "beta")))]
        { format!("{}/api/mcv/core/latest/stable", api_base_url.into()) }
    }
    /// mcv本体の更新をチェック
    ///
    /// # Arguments
    /// * `current_version` - 現在のmcvバージョン
    ///
    /// # Returns
    /// 更新が利用可能な場合は `Some(McvUpdateInfo)`、それ以外は `None`
    pub async fn check_mcv_update(
        &self,
        current_version: &str,
    ) -> Result<Option<McvUpdateInfo>, UpdateError> {
        let url = self.get_mcv_update_endpoint(&self.api_base_url);
        let response = self
            .client
            .get(&url)
            .header("Current-Version", current_version)
            .send()
            .await?;

        if response.status() == reqwest::StatusCode::NOT_FOUND {
            // サーバー仕様上、チャンネルにバージョンが存在しない場合も 404 を返す。
            // ただし、エンドポイントのURL変更等による誤った 404 と区別するため、
            // Content-Type が application/json のときのみ「バージョン未登録」と判断する。
            // HTML 等が返った場合はエンドポイント不正とみなしてエラーにする。
            let is_json = response
                .headers()
                .get(reqwest::header::CONTENT_TYPE)
                .and_then(|v| v.to_str().ok())
                .map(|v| v.contains("application/json"))
                .unwrap_or(false);
            return if is_json { Ok(None) } else { Err(UpdateError::HttpError(response.error_for_status().unwrap_err())) };
        }
        if !response.status().is_success() {
            return Err(UpdateError::HttpError(
                response.error_for_status().unwrap_err(),
            ));
        }

        let info: McvUpdateInfo = response.json().await?;

        let current = Version::parse(current_version)?;
        let latest = Version::parse(&info.version)?;

        if latest > current {
            Ok(Some(info))
        } else {
            Ok(None)
        }
    }

    /// プラグイン一覧を取得
    ///
    /// # Returns
    /// 利用可能なプラグイン一覧
    pub async fn list_plugins(&self) -> Result<Vec<PluginListItem>, UpdateError> {
        let url = format!("{}/api/mcv/plugins", self.api_base_url);
        let response = self.client.get(&url).send().await?;

        if !response.status().is_success() {
            return Err(UpdateError::HttpError(
                response.error_for_status().unwrap_err(),
            ));
        }

        let plugins: Vec<PluginListItem> = response.json().await?;
        Ok(plugins)
    }

    /// 特定プラグインの詳細情報を取得
    pub async fn get_plugin_detail(&self, plugin_id: &str) -> Result<PluginDetail, UpdateError> {
        let url = format!("{}/api/mcv/plugins/{}", self.api_base_url, plugin_id);
        let response = self.client.get(&url).send().await?;

        if !response.status().is_success() {
            return Err(UpdateError::HttpError(
                response.error_for_status().unwrap_err(),
            ));
        }

        let detail: PluginDetail = response.json().await?;
        Ok(detail)
    }

    /// mcvのダウンロードURLを構築
    ///
    /// # Arguments
    /// * `version` - mcvのバージョン（例: "7.77.7777"）
    /// * `channel` - リリースチャンネル（"stable", "beta", "alpha"）
    ///
    /// # Returns
    /// ダウンロードURL
    pub fn build_mcv_download_url(&self, version: &str, channel: &str) -> String {
        format!(
            "{}/api/mcv/core/{}/{}/download",
            self.api_base_url, version, channel
        )
    }

    /// プラグインのダウンロードURLを構築
    ///
    /// # Arguments
    /// * `plugin_id` - プラグインID（例: "plugin-youtube"）
    /// * `version` - プラグインのバージョン
    /// * `channel` - リリースチャンネル（"stable", "beta", "alpha"）
    ///
    /// # Returns
    /// ダウンロードURL
    pub fn build_plugin_download_url(
        &self,
        plugin_id: &str,
        version: &str,
        channel: &str,
    ) -> String {
        format!(
            "{}/api/mcv/plugins/{}/{}/{}/download",
            self.api_base_url, plugin_id, version, channel
        )
    }

    /// ファイルをダウンロード
    ///
    /// # Arguments
    /// * `url` - ダウンロードURL
    /// * `dest` - 保存先パス
    /// * `progress_callback` - 進捗コールバック関数（downloaded_bytes, total_bytes）
    ///
    /// # Returns
    /// ダウンロード成功時は `Ok(())`
    pub async fn download<F>(
        &self,
        url: &str,
        dest: &Path,
        mut progress_callback: F,
    ) -> Result<(), UpdateError>
    where
        F: FnMut(u64, u64),
    {
        println!("Downloading from: {}", url);
        println!("Saving to: {:?}", dest);

        let response = self.client.get(url).send().await?;

        if !response.status().is_success() {
            return Err(UpdateError::HttpError(
                response.error_for_status().unwrap_err(),
            ));
        }

        let total_size = response.content_length().unwrap_or(0);
        println!("Total size: {} bytes", total_size);

        // 親ディレクトリが存在しない場合は作成
        if let Some(parent) = dest.parent() {
            tokio::fs::create_dir_all(parent).await?;
        }

        let mut file = tokio::fs::File::create(dest).await?;
        let mut downloaded: u64 = 0;

        let mut stream = response.bytes_stream();
        use futures_util::StreamExt;

        while let Some(chunk_result) = stream.next().await {
            let chunk = chunk_result?;
            file.write_all(&chunk).await?;
            downloaded += chunk.len() as u64;
            progress_callback(downloaded, total_size);
        }

        file.flush().await?;
        println!("Download completed: {} bytes", downloaded);

        Ok(())
    }

    /// SHA-256チェックサムを検証
    ///
    /// # Arguments
    /// * `file_path` - 検証するファイルのパス
    /// * `expected_sha256` - 期待されるSHA-256ハッシュ（16進数文字列）
    ///
    /// # Returns
    /// チェックサムが一致する場合は `Ok(true)`、不一致の場合は `Err(UpdateError::ChecksumMismatch)`
    pub async fn verify_checksum(
        &self,
        file_path: &Path,
        expected_sha256: &str,
    ) -> Result<bool, UpdateError> {
        println!("Verifying checksum for: {:?}", file_path);

        let mut file = tokio::fs::File::open(file_path).await?;
        let mut hasher = Sha256::new();

        use tokio::io::AsyncReadExt;
        let mut buffer = vec![0u8; 8192];

        loop {
            let n = file.read(&mut buffer).await?;
            if n == 0 {
                break;
            }
            hasher.update(&buffer[..n]);
        }

        let result = hasher.finalize();
        let actual_sha256 = format!("{:x}", result);

        println!("Expected SHA-256: {}", expected_sha256);
        println!("Actual SHA-256:   {}", actual_sha256);

        if actual_sha256.to_lowercase() == expected_sha256.to_lowercase() {
            println!("Checksum verification: OK");
            Ok(true)
        } else {
            Err(UpdateError::ChecksumMismatch {
                expected: expected_sha256.to_string(),
                actual: actual_sha256,
            })
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_update_info_serialization() {
        let info = UpdateInfo {
            version: "1.0.0".to_string(),
            download_url: "https://example.com/download".to_string(),
            sha256: "abc123".to_string(),
            release_notes: "Test release".to_string(),
            released_at: "2026-01-01T00:00:00Z".to_string(),
        };

        let json = serde_json::to_string(&info).unwrap();
        let deserialized: UpdateInfo = serde_json::from_str(&json).unwrap();

        assert_eq!(deserialized.version, "1.0.0");
        assert_eq!(deserialized.download_url, "https://example.com/download");
    }

    #[test]
    fn test_plugin_list_item_serialization() {
        let plugin = PluginListItem {
            id: "test-plugin1".to_string(),
            name: "一番最初のテストプラグイン".to_string(),
            description: "ここに説明を記述".to_string(),
            download_count: 0,
            channels: PluginChannels {
                stable: Some("0.1.0".to_string()),
                beta: None,
                alpha: None,
            },
        };

        let json = serde_json::to_string(&plugin).unwrap();
        let deserialized: PluginListItem = serde_json::from_str(&json).unwrap();

        assert_eq!(deserialized.id, "test-plugin1");
        assert_eq!(deserialized.name, "一番最初のテストプラグイン");
        assert_eq!(deserialized.channels.stable, Some("0.1.0".to_string()));
        assert_eq!(deserialized.channels.beta, None);
    }

    #[test]
    fn test_plugin_list_api_format() {
        let json = r#"[{"id":"test-plugin1","name":"一番最初のテストプラグイン","description":"ここに説明を記述","channels":{"stable":"0.1.0","beta":null,"alpha":null}}]"#;
        let plugins: Vec<PluginListItem> = serde_json::from_str(json).unwrap();

        assert_eq!(plugins.len(), 1);
        assert_eq!(plugins[0].id, "test-plugin1");
        assert_eq!(plugins[0].name, "一番最初のテストプラグイン");
        assert_eq!(plugins[0].description, "ここに説明を記述");
        assert_eq!(plugins[0].channels.stable, Some("0.1.0".to_string()));
        assert_eq!(plugins[0].channels.beta, None);
        assert_eq!(plugins[0].channels.alpha, None);
    }

    #[test]
    fn test_plugin_info_serialization() {
        let plugin = PluginInfo {
            id: "plugin-test".to_string(),
            name: "Test Plugin".to_string(),
            description: "A test plugin".to_string(),
            version: "1.0.0".to_string(),
            download_url: "https://example.com/plugin.dll".to_string(),
            sha256: "def456".to_string(),
            file_size: 1024,
            author: "Test Author".to_string(),
            license: "MIT".to_string(),
            released_at: "2026-01-01T00:00:00Z".to_string(),
            min_mcv_version: "0.1.0".to_string(),
        };

        let json = serde_json::to_string(&plugin).unwrap();
        let deserialized: PluginInfo = serde_json::from_str(&json).unwrap();

        assert_eq!(deserialized.id, "plugin-test");
        assert_eq!(deserialized.name, "Test Plugin");
    }

    #[tokio::test]
    async fn test_version_comparison() {
        let current = Version::parse("1.0.0").unwrap();
        let latest = Version::parse("1.1.0").unwrap();

        assert!(latest > current);
    }
}
