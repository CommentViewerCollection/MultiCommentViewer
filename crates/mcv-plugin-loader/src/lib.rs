use libloading::{Library, Symbol};
use serde::{Deserialize, Serialize};
use std::ffi::{c_char, c_void, CString};
use std::path::Path;
use thiserror::Error;

/// プラグインメタデータ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginMetadata {
    pub id: String,
    pub name: String,
    pub version: String,
    pub api_version: String,
    pub roles: Vec<String>,
}

/// プラグインローダーエラー
#[derive(Error, Debug)]
pub enum PluginLoaderError {
    #[error("Failed to load plugin DLL: {0}")]
    LoadFailed(String),

    #[error("Failed to get metadata: {0}")]
    MetadataError(String),

    #[error("Failed to initialize plugin: {0}")]
    InitFailed(String),

    #[error("Failed to send message: {0}")]
    MessageSendFailed(String),

    #[error("Failed to set callback: {0}")]
    CallbackSetFailed(String),

    #[error("Failed to shutdown plugin: {0}")]
    ShutdownFailed(String),

    #[error("Unsupported ABI version")]
    UnsupportedAbiVersion,
}

/// メッセージコールバック関数の型（userdata対応）
pub type MessageCallbackWithUserdata = extern "C" fn(*const c_char, *mut c_void);

/// プラグインローダー
///
/// プラグインDLLを動的にロードし、C ABI関数を呼び出す
pub struct PluginLoader {
    library: Library,
}

impl PluginLoader {
    /// DLLをロードしてメタデータを取得
    ///
    /// # Arguments
    /// * `path` - プラグインDLLのパス
    ///
    /// # Returns
    /// ロード成功時は `PluginLoader` インスタンス、失敗時はエラー
    pub fn load<P: AsRef<Path>>(path: P) -> Result<Self, PluginLoaderError> {
        // libloadingでDLLをロード
        let library = unsafe {
            Library::new(path.as_ref()).map_err(|e| {
                PluginLoaderError::LoadFailed(format!(
                    "Failed to load library from {:?}: {}",
                    path.as_ref(),
                    e
                ))
            })?
        };

        Ok(Self { library })
    }

    /// プラグイン初期化
    ///
    /// # Arguments
    /// * `host_context` - mcv側から渡されるコンテキスト（将来の拡張用、現在は未使用）
    ///
    /// # Returns
    /// 成功時は `Ok(())`、失敗時はエラー
    pub fn init(&self, host_context: *mut c_void) -> Result<(), PluginLoaderError> {
        unsafe {
            let init_fn: Symbol<unsafe extern "C" fn(*mut c_void) -> i32> = self
                .library
                .get(b"plugin_init\0")
                .map_err(|e| PluginLoaderError::InitFailed(format!("Symbol not found: {}", e)))?;

            let result = init_fn(host_context);

            if result == 0 {
                Ok(())
            } else {
                Err(PluginLoaderError::InitFailed(format!(
                    "plugin_init returned error code: {}",
                    result
                )))
            }
        }
    }

    /// メッセージ送信（mcv→プラグイン）
    ///
    /// # Arguments
    /// * `message` - JSON形式のメッセージ文字列
    ///
    /// # Returns
    /// 成功時は `Ok(())`、失敗時はエラー
    pub fn send_message(&self, message: &str) -> Result<(), PluginLoaderError> {
        tracing::trace!(
            target: "mcv::plugin_loader::PluginLoader",
            "Sending message to plugin: {}",
            message
        );
        unsafe {
            let send_fn: Symbol<unsafe extern "C" fn(*const c_char) -> i32> =
                self.library.get(b"plugin_send_message\0").map_err(|e| {
                    PluginLoaderError::MessageSendFailed(format!("Symbol not found: {}", e))
                })?;

            let message_cstr = CString::new(message).map_err(|e| {
                PluginLoaderError::MessageSendFailed(format!("Invalid string: {}", e))
            })?;

            let result = send_fn(message_cstr.as_ptr());

            if result == 0 {
                Ok(())
            } else {
                Err(PluginLoaderError::MessageSendFailed(format!(
                    "plugin_send_message returned error code: {}",
                    result
                )))
            }
        }
    }
    /// プラグインdllとして適切な形式であるか
    pub fn validate_plugin_exports(&self) -> Result<(), PluginLoaderError> {
        unsafe {
            let _: Symbol<unsafe extern "C" fn(MessageCallbackWithUserdata, *mut c_void) -> i32> =
                self.library.get(b"plugin_set_callback\0").map_err(|e| {
                    PluginLoaderError::CallbackSetFailed(format!("Symbol not found: {}", e))
                })?;
            let _: Symbol<unsafe extern "C" fn() -> i32> = self
                .library
                .get(b"plugin_on_loaded\0")
                .map_err(|e| PluginLoaderError::InitFailed(format!("Symbol not found: {}", e)))?;
            let _: Symbol<unsafe extern "C" fn() -> i32> =
                self.library.get(b"plugin_shutdown\0").map_err(|e| {
                    PluginLoaderError::ShutdownFailed(format!("Symbol not found: {}", e))
                })?;
            let _: Symbol<unsafe extern "C" fn(*const c_char) -> i32> =
                self.library.get(b"plugin_send_message\0").map_err(|e| {
                    PluginLoaderError::MessageSendFailed(format!("Symbol not found: {}", e))
                })?;
            let _: Symbol<unsafe extern "C" fn(*mut c_void) -> i32> = self
                .library
                .get(b"plugin_init\0")
                .map_err(|e| PluginLoaderError::InitFailed(format!("Symbol not found: {}", e)))?;
        }
        Ok(())
    }
    /// コールバック設定（プラグイン→mcv、userdata対応）
    ///
    /// # Arguments
    /// * `callback` - mcv側から呼ばれるコールバック関数
    /// * `userdata` - コールバック時に渡されるユーザーデータポインタ
    ///
    /// # Returns
    /// 成功時は `Ok(())`、失敗時はエラー
    pub fn set_callback_with_userdata(
        &self,
        callback: MessageCallbackWithUserdata,
        userdata: *mut c_void,
    ) -> Result<(), PluginLoaderError> {
        unsafe {
            let set_callback_fn: Symbol<
                unsafe extern "C" fn(MessageCallbackWithUserdata, *mut c_void) -> i32,
            > = self.library.get(b"plugin_set_callback\0").map_err(|e| {
                PluginLoaderError::CallbackSetFailed(format!("Symbol not found: {}", e))
            })?;

            let result = set_callback_fn(callback, userdata);

            if result == 0 {
                Ok(())
            } else {
                Err(PluginLoaderError::CallbackSetFailed(format!(
                    "plugin_set_callback returned error code: {}",
                    result
                )))
            }
        }
    }

    /// プラグインon_loaded呼び出し
    ///
    /// # Returns
    /// 成功時は `Ok(())`、失敗時はエラー
    pub fn on_loaded(&self) -> Result<(), PluginLoaderError> {
        unsafe {
            let on_loaded_fn: Symbol<unsafe extern "C" fn() -> i32> = self
                .library
                .get(b"plugin_on_loaded\0")
                .map_err(|e| PluginLoaderError::InitFailed(format!("Symbol not found: {}", e)))?;

            let result = on_loaded_fn();

            if result == 0 {
                Ok(())
            } else {
                Err(PluginLoaderError::InitFailed(format!(
                    "plugin_on_loaded returned error code: {}",
                    result
                )))
            }
        }
    }

    /// プラグイン終了
    ///
    /// # Returns
    /// 成功時は `Ok(())`、失敗時はエラー
    pub fn shutdown(&self) -> Result<(), PluginLoaderError> {
        unsafe {
            let shutdown_fn: Symbol<unsafe extern "C" fn() -> i32> =
                self.library.get(b"plugin_shutdown\0").map_err(|e| {
                    PluginLoaderError::ShutdownFailed(format!("Symbol not found: {}", e))
                })?;

            let result = shutdown_fn();

            if result == 0 {
                Ok(())
            } else {
                Err(PluginLoaderError::ShutdownFailed(format!(
                    "plugin_shutdown returned error code: {}",
                    result
                )))
            }
        }
    }
    pub fn clear_callback() -> Result<(), PluginLoaderError> {
        Ok(())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_plugin_metadata_deserialization() {
        let json = r#"{
            "id": "plugin-test",
            "name": "Test Plugin",
            "version": "1.0.0",
            "api_version": "v2",
            "roles": ["test"]
        }"#;

        let metadata: PluginMetadata = serde_json::from_str(json).unwrap();
        assert_eq!(metadata.id, "plugin-test");
        assert_eq!(metadata.name, "Test Plugin");
        assert_eq!(metadata.version, "1.0.0");
        assert_eq!(metadata.api_version, "v2");
        assert_eq!(metadata.roles, vec!["test"]);
    }
}
