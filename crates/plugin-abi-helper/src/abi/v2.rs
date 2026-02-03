//! ABI v2 – function-based ABI
//!
//! ⚠️ 新規実装では使用非推奨
//! v3 (struct pointer) への移行用として保持

#![allow(clippy::missing_safety_doc)]

use core::ffi::c_char;
use core::ffi::c_void;

pub const PLUGIN_ABI_VERSION: u32 = 2;

/// プラグインメタデータ取得
pub type PluginGetMetadata = unsafe extern "C" fn() -> *const c_char;

/// プラグイン初期化
pub type PluginInit = unsafe extern "C" fn(host_context: *mut c_void) -> i32;

/// on_loaded
pub type PluginOnLoaded = unsafe extern "C" fn() -> i32;

/// メッセージ送信
pub type PluginSendMessage = unsafe extern "C" fn(message_json: *const c_char) -> i32;

/// シャットダウン
pub type PluginShutdown = unsafe extern "C" fn() -> i32;

/// Core → Plugin コールバック設定
pub type PluginSetCallback = unsafe extern "C" fn(
    callback: extern "C" fn(*const c_char, *mut c_void),
    userdata: *mut c_void,
) -> i32;
