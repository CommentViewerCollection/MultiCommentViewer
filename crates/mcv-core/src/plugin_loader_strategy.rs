use actix::prelude::*;
use async_trait::async_trait;
use mcv_common::PhysicalPluginId;
use mcv_plugin_loader::{PluginLoader, PluginLoaderError};
use std::path::Path;

use crate::core_actor::CoreActor;
use crate::plugin_host_actor::PhysicalPluginHostActor;

/// プラグインロード後の情報
#[derive(Debug)]
pub struct LoadedPluginInfo {
    pub physical_plugin_id: PhysicalPluginId,
    pub abi_version: u32,
    pub host_addr: Addr<PhysicalPluginHostActor>,
    pub plugin_name: Option<String>,
}

/// プラグインローダーの戦略インターフェース
#[async_trait]
pub trait PluginLoaderStrategy: Send + Sync {
    /// ABI バージョン番号
    fn abi_version(&self) -> u32;

    /// DLL がこのバージョンに対応しているか確認
    fn can_load(&self, dll_path: &Path) -> bool;

    /// プラグインをロード
    async fn load_plugin(
        &self,
        dll_path: &Path,
        core_addr: Addr<CoreActor>,
    ) -> Result<LoadedPluginInfo, PluginLoaderError>;
}

/// プラグインローダーのレジストリ
pub struct PluginLoaderRegistry {
    strategies: Vec<Box<dyn PluginLoaderStrategy>>,
}

impl PluginLoaderRegistry {
    /// 新しいレジストリを作成
    pub fn new() -> Self {
        Self {
            strategies: Vec::new(),
        }
    }

    /// 戦略を登録
    pub fn register(&mut self, strategy: Box<dyn PluginLoaderStrategy>) {
        self.strategies.push(strategy);
    }

    /// プラグインをロード（バージョン自動検出）
    pub async fn load_plugin(
        &self,
        dll_path: &Path,
        core_addr: Addr<CoreActor>,
    ) -> Result<LoadedPluginInfo, PluginLoaderError> {
        // 各戦略を試す（最後に登録されたものから = 新しいバージョン優先）
        for strategy in self.strategies.iter().rev() {
            if strategy.can_load(dll_path) {
                tracing::debug!(
                    target: "mcv::core::PluginLoaderRegistry",
                    abi_version = strategy.abi_version(),
                    dll_path = %dll_path.display(),
                    "Detected plugin ABI version"
                );
                return strategy.load_plugin(dll_path, core_addr).await;
            }
        }

        tracing::error!(
            target: "mcv::core::PluginLoaderRegistry",
            dll_path = %dll_path.display(),
            "No compatible loader strategy found for plugin"
        );
        Err(PluginLoaderError::UnsupportedAbiVersion)
    }
}

impl Default for PluginLoaderRegistry {
    fn default() -> Self {
        Self::new()
    }
}

/// V2プラグインローダー戦略
pub struct V2LoaderStrategy;

#[async_trait]
impl PluginLoaderStrategy for V2LoaderStrategy {
    fn abi_version(&self) -> u32 {
        2
    }

    fn can_load(&self, dll_path: &Path) -> bool {
        // v2のシンボル（plugin_init）をチェック
        use libloading::{Library, Symbol};

        let lib: Library = match unsafe { Library::new(dll_path) } {
            Ok(lib) => lib,
            Err(_) => return false,
        };

        // plugin_init シンボルが存在するか確認
        unsafe {
            lib.get::<Symbol<'_, unsafe extern "C" fn() -> i32>>(b"plugin_init\0")
                .is_ok()
        }
    }

    async fn load_plugin(
        &self,
        dll_path: &Path,
        core_addr: Addr<CoreActor>,
    ) -> Result<LoadedPluginInfo, PluginLoaderError> {
        // 既存のコードを再利用
        let plugin_loader = PluginLoader::load(dll_path)?;
        plugin_loader.validate_plugin_exports()?;

        let physical_plugin_id = PhysicalPluginId::new();

        // プラグイン名をDLLファイル名から取得
        let plugin_name = dll_path
            .file_stem()
            .and_then(|s| s.to_str())
            .map(|s| s.to_string());

        tracing::info!(
            target: "mcv::core::V2LoaderStrategy",
            physical_plugin_id = %physical_plugin_id,
            plugin_name = ?plugin_name,
            "Loading v2 plugin"
        );

        let mut host_actor =
            PhysicalPluginHostActor::new_from_dll(physical_plugin_id, plugin_loader);
        host_actor.set_core_addr(core_addr);

        let host_addr = host_actor.start();

        Ok(LoadedPluginInfo {
            physical_plugin_id,
            abi_version: 2,
            host_addr,
            plugin_name,
        })
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_registry_creation() {
        let registry = PluginLoaderRegistry::new();
        assert_eq!(registry.strategies.len(), 0);
    }

    #[test]
    fn test_registry_registration() {
        let mut registry = PluginLoaderRegistry::new();
        registry.register(Box::new(V2LoaderStrategy));
        assert_eq!(registry.strategies.len(), 1);
    }

    #[test]
    fn test_v2_strategy_abi_version() {
        let strategy = V2LoaderStrategy;
        assert_eq!(strategy.abi_version(), 2);
    }
}
