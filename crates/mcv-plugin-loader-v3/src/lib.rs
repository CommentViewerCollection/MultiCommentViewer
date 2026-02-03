use libloading::{Library, Symbol};
use plugin_abi_helper::abi::v3::PluginV3;
use std::path::Path;
use thiserror::Error;

/// v3プラグインローダーエラー
#[derive(Error, Debug)]
pub enum PluginLoaderV3Error {
    #[error("Failed to load plugin DLL: {0}")]
    LoadFailed(String),

    #[error("Symbol not found: {0}")]
    SymbolNotFound(String),

    #[error("Plugin creation failed")]
    CreationFailed,
}

/// v3プラグインローダー
///
/// v3 ABI (構造体ポインタベース) のプラグインをロードする
pub struct PluginLoaderV3 {
    library: Library,
    plugin_ptr: *mut PluginV3,
}

impl PluginLoaderV3 {
    /// DLLをロードしてPluginV3を作成
    ///
    /// # Arguments
    /// * `path` - プラグインDLLのパス
    ///
    /// # Returns
    /// ロード成功時は `PluginLoaderV3` インスタンス、失敗時はエラー
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, PluginLoaderV3Error> {
        // libloadingでDLLをロード
        let library = unsafe {
            Library::new(path.as_ref()).map_err(|e| {
                PluginLoaderV3Error::LoadFailed(format!(
                    "Failed to load library from {:?}: {}",
                    path.as_ref(),
                    e
                ))
            })?
        };

        // create_plugin_v3 シンボルを取得
        let create_fn: Symbol<unsafe extern "C" fn() -> *mut PluginV3> = unsafe {
            library
                .get(b"create_plugin_v3\0")
                .map_err(|e| PluginLoaderV3Error::SymbolNotFound(format!("create_plugin_v3: {}", e)))?
        };

        // プラグインを作成
        let plugin_ptr = unsafe { create_fn() };

        if plugin_ptr.is_null() {
            return Err(PluginLoaderV3Error::CreationFailed);
        }

        Ok(Self {
            library,
            plugin_ptr,
        })
    }

    /// PluginV3構造体への可変参照を取得
    ///
    /// # Safety
    /// プラグインが正常にロードされている場合のみ安全
    pub fn get_plugin_mut(&self) -> &mut PluginV3 {
        unsafe { &mut *self.plugin_ptr }
    }

    /// PluginV3構造体への不変参照を取得
    ///
    /// # Safety
    /// プラグインが正常にロードされている場合のみ安全
    pub fn get_plugin(&self) -> &PluginV3 {
        unsafe { &*self.plugin_ptr }
    }

    /// プラグインポインタを取得
    pub fn plugin_ptr(&self) -> *mut PluginV3 {
        self.plugin_ptr
    }

    /// v3プラグインとして有効なシンボルを持っているか検証
    pub fn validate(&self) -> Result<(), PluginLoaderV3Error> {
        let _: Symbol<unsafe extern "C" fn() -> *mut PluginV3> = unsafe {
            self.library
                .get(b"create_plugin_v3\0")
                .map_err(|e| PluginLoaderV3Error::SymbolNotFound(format!("create_plugin_v3: {}", e)))?
        };

        Ok(())
    }
}

// PluginLoaderV3はSendを実装（スレッド間移動を許可）
unsafe impl Send for PluginLoaderV3 {}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_error_display() {
        let err = PluginLoaderV3Error::LoadFailed("test error".to_string());
        assert!(err.to_string().contains("test error"));

        let err = PluginLoaderV3Error::SymbolNotFound("create_plugin_v3".to_string());
        assert!(err.to_string().contains("create_plugin_v3"));

        let err = PluginLoaderV3Error::CreationFailed;
        assert!(err.to_string().contains("creation failed"));
    }
}
