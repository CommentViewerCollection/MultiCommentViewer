use std::ffi::c_void;

use crate::abi::v3::{PLUGIN_ABI_VERSION, PluginV3};
use crate::v3::context::PluginContext;
use crate::v3::plugin_async::PluginImplV3Async;
use crate::v3::plugin_runtime::PluginRuntimeV3;

/// プラグイン状態（userdataに格納）
/// context と runtime を一緒に保持する
pub struct PluginState {
    pub(crate) context: PluginContext,
    pub(crate) runtime: PluginRuntimeV3,
}

pub struct PluginFactoryV3;

impl PluginFactoryV3 {
    pub fn new<T: PluginImplV3Async + Default>() -> *mut PluginV3 {
        // 1. Contextを作成
        let context = PluginContext::new();

        // 2. Plugin実装を作成
        let plugin_impl = T::default();

        // 3. Runtimeを起動（contextをclone）
        let runtime = PluginRuntimeV3::start(plugin_impl, context.clone());

        // 3. 状態をまとめる
        let state = PluginState { context, runtime };
        let userdata = Box::into_raw(Box::new(state)) as *mut c_void;

        // 5. PluginV3を作成
        let plugin = Box::new(PluginV3 {
            abi_version: PLUGIN_ABI_VERSION,
            plugin_id: [0u8; 16], // nil UUID
            host: core::ptr::null(),
            on_loaded: crate::v3::trampoline::on_loaded_trampoline,
            on_message: crate::v3::trampoline::on_message_trampoline,
            on_shutdown: crate::v3::trampoline::on_shutdown_trampoline,
            userdata,
        });

        Box::into_raw(plugin)
    }
}

/// プラグイン破棄
///
/// # Safety
///
/// `p`は`PluginFactoryV3::new`で作成された有効なポインタでなければならない
pub unsafe fn destroy_plugin_v3(p: *mut PluginV3) {
    if p.is_null() {
        return;
    }

    unsafe {
        let plugin = Box::from_raw(p);
        // userdataも解放
        if !plugin.userdata.is_null() {
            let _ = Box::from_raw(plugin.userdata as *mut PluginState);
        }
    }
}
