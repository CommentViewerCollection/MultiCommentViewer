use crate::manifest::{PluginManifest, ManifestError};
use std::collections::HashMap;
use std::path::{Path, PathBuf};
use std::process::Child;
use thiserror::Error;

#[derive(Debug, Error)]
pub enum ProcessManagerError {
    #[error("Manifest error: {0}")]
    Manifest(#[from] ManifestError),

    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),

    #[error("Process spawn error: {0}")]
    SpawnError(String),

    #[error("Plugin directory not found")]
    PluginDirectoryNotFound,
}

/// プロセス管理
pub struct ProcessManager {
    websocket_port: u16,
    processes: HashMap<String, PluginProcess>,  // plugin_id -> PluginProcess
    manifests: Vec<(PathBuf, PluginManifest)>,  // (manifest_dir, manifest)
}

/// EXEプラグインのプロセス情報
struct PluginProcess {
    manifest: PluginManifest,
    manifest_dir: PathBuf,
    child: Option<Child>,
    restart_count: u32,
    auto_started: bool,  // 自動起動されたプラグインかどうか
}

impl ProcessManager {
    /// 新しいプロセスマネージャーを作成
    pub async fn new(websocket_port: u16) -> Result<Self, ProcessManagerError> {
        tracing::info!(websocket_port = websocket_port, "ProcessManager::new called");

        let mut manager = Self {
            websocket_port,
            processes: HashMap::new(),
            manifests: Vec::new(),
        };

        // pluginsディレクトリをスキャン
        manager.scan_plugins_directory().await?;

        // 検出したプラグインを自動起動
        manager.start_auto_plugins().await?;

        Ok(manager)
    }

    /// pluginsディレクトリをスキャンしてmanifest.jsonを検出
    async fn scan_plugins_directory(&mut self) -> Result<(), ProcessManagerError> {
        let plugins_dir = Self::get_plugins_directory()?;

        tracing::info!(plugins_dir = %plugins_dir.display(), "Scanning plugins directory");

        if !plugins_dir.exists() {
            tracing::warn!("Plugins directory does not exist, creating it");
            std::fs::create_dir_all(&plugins_dir)?;
            return Ok(());
        }

        // pluginsディレクトリ内のサブディレクトリを走査
        for entry in std::fs::read_dir(&plugins_dir)? {
            let entry = entry?;
            let path = entry.path();

            if !path.is_dir() {
                continue;
            }

            // manifest.jsonを検索
            let manifest_path = path.join("manifest.json");
            if manifest_path.exists() {
                match PluginManifest::load(&manifest_path) {
                    Ok(manifest) => {
                        tracing::info!(
                            plugin_name = %manifest.plugin.name,
                            manifest_path = %manifest_path.display(),
                            "Found plugin manifest"
                        );
                        self.manifests.push((path, manifest));
                    }
                    Err(e) => {
                        tracing::error!(
                            manifest_path = %manifest_path.display(),
                            error = %e,
                            "Failed to load manifest"
                        );
                    }
                }
            }
        }

        tracing::info!(count = self.manifests.len(), "Plugins scanned");

        Ok(())
    }

    /// 検出したプラグインを自動起動
    async fn start_auto_plugins(&mut self) -> Result<(), ProcessManagerError> {
        // 借用エラーを避けるためにclone
        let manifests_clone = self.manifests.clone();

        for (manifest_dir, manifest) in manifests_clone {
            tracing::info!(
                plugin_name = %manifest.plugin.name,
                "Starting auto plugin"
            );

            match self.start_plugin(&manifest_dir, &manifest, true).await {
                Ok(_) => {
                    tracing::info!(plugin_name = %manifest.plugin.name, "Plugin started");
                }
                Err(e) => {
                    tracing::error!(
                        plugin_name = %manifest.plugin.name,
                        error = %e,
                        "Failed to start plugin"
                    );
                }
            }
        }

        Ok(())
    }

    /// プラグインを起動
    async fn start_plugin(
        &mut self,
        manifest_dir: &Path,
        manifest: &PluginManifest,
        auto_started: bool,
    ) -> Result<(), ProcessManagerError> {
        let exe_path = manifest.get_executable_path(manifest_dir);
        let working_dir = manifest.get_working_directory(manifest_dir);

        tracing::debug!(
            exe_path = %exe_path.display(),
            working_dir = %working_dir.display(),
            "Starting plugin process"
        );

        // TODO: 実際にプロセスを起動
        // let child = std::process::Command::new(&exe_path)
        //     .current_dir(&working_dir)
        //     .args(&manifest.executable.args)
        //     .env("MCV_WEBSOCKET_PORT", self.websocket_port.to_string())
        //     .spawn()?;

        let plugin_process = PluginProcess {
            manifest: manifest.clone(),
            manifest_dir: manifest_dir.to_path_buf(),
            child: None,  // TODO: 実際のChildを設定
            restart_count: 0,
            auto_started,
        };

        self.processes.insert(manifest.plugin.id.clone(), plugin_process);

        Ok(())
    }

    /// プラグインをシャットダウン
    pub async fn shutdown(&mut self) -> Result<(), ProcessManagerError> {
        tracing::info!("ProcessManager::shutdown called");

        // すべてのプロセスを終了
        for (plugin_id, mut process) in self.processes.drain() {
            tracing::info!(plugin_id = %plugin_id, "Stopping plugin process");

            if let Some(mut child) = process.child.take() {
                // TODO: グレースフルシャットダウン
                match child.kill() {
                    Ok(_) => tracing::info!(plugin_id = %plugin_id, "Plugin process killed"),
                    Err(e) => tracing::error!(plugin_id = %plugin_id, error = %e, "Failed to kill plugin process"),
                }
            }
        }

        Ok(())
    }

    /// pluginsディレクトリのパスを取得
    fn get_plugins_directory() -> Result<PathBuf, ProcessManagerError> {
        // %APPDATA%\MultiCommentViewer\plugins\
        let appdata = std::env::var("APPDATA")
            .map_err(|_| ProcessManagerError::PluginDirectoryNotFound)?;

        let plugins_dir = PathBuf::from(appdata)
            .join("MultiCommentViewer")
            .join("plugins");

        Ok(plugins_dir)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_get_plugins_directory() {
        let result = ProcessManager::get_plugins_directory();
        // APPDATA環境変数が設定されている場合のみ成功
        if std::env::var("APPDATA").is_ok() {
            assert!(result.is_ok());
        }
    }
}
