//! ABI v3 – struct pointer based ABI
//!
//! 推奨方式（将来拡張可能）

#![allow(clippy::missing_safety_doc)]

use core::ffi::c_void;

pub const PLUGIN_ABI_VERSION: u32 = 3;

/// Host 側が提供する関数群
#[repr(C)]
pub struct HostRuntimeV3 {
    /// ログ出力
    pub log: unsafe extern "C" fn(
        level: i32,
        msg_ptr: *const u8,
        msg_len: usize,
    ),

    /// Core へメッセージ送信
    pub send_message: unsafe extern "C" fn(
        json_ptr: *const u8,
        json_len: usize,
    ),
}

/// Plugin インスタンス（opaque handle）
#[repr(C)]
pub struct PluginV3 {
    /// ABI バージョン（必ず PLUGIN_ABI_VERSION）
    pub abi_version: u32,

    /// Core が割り当てる plugin_id
    pub plugin_id: u64,

    /// HostRuntime へのポインタ
    pub host: *const HostRuntimeV3,

    /// ライフサイクル
    pub on_loaded: unsafe extern "C" fn(*mut PluginV3) -> i32,
    pub on_message: unsafe extern "C" fn(
        *mut PluginV3,
        msg_ptr: *const u8,
        msg_len: usize,
    ) -> i32,
    pub on_shutdown: unsafe extern "C" fn(*mut PluginV3) -> i32,

    /// プラグイン実装が保持する state
    pub userdata: *mut c_void,
}