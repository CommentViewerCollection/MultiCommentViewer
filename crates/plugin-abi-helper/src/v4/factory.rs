use std::ffi::c_void;

use crate::v4::plugin_async::PluginImplV4Async;
use crate::v4::plugin_runtime::PluginRuntimeV4;
use crate::v4::context::PluginContext;
use crate::abi::v4::{PluginV4, PLUGIN_ABI_VERSION};

/// プラグイン状態（userdataに格納）
/// context と runtime を一緒に保持する
pub struct PluginState {
    pub(crate) context: PluginContext,
    pub(crate) runtime: PluginRuntimeV4,
}

pub struct PluginFactoryV4;

impl PluginFactoryV4 {
    pub fn new<T: PluginImplV4Async + Default>() -> *mut PluginV4 {
        // 1. Contextを作成
        let context = PluginContext::new();

        // 2. Plugin実装を作成
        let plugin_impl = T::default();

        // 3. Runtimeを起動（contextをclone）
        let runtime = PluginRuntimeV4::start(plugin_impl, context.clone());

        // 4. 状態をまとめる
        let state = PluginState { context, runtime };
        let userdata = Box::into_raw(Box::new(state)) as *mut c_void;

        // 5. PluginV4を作成
        let plugin = Box::new(PluginV4 {
            abi_version: PLUGIN_ABI_VERSION,
            plugin_id: 0,
            host: core::ptr::null(),
            on_loaded: crate::v4::trampoline::on_loaded_trampoline,
            on_message: crate::v4::trampoline::on_message_trampoline,
            on_shutdown: crate::v4::trampoline::on_shutdown_trampoline,
            userdata,
        });

        Box::into_raw(plugin)
    }
}

/// プラグイン破棄
///
/// # Safety
///
/// `p`は`PluginFactoryV4::new`で作成された有効なポインタでなければならない
pub unsafe fn destroy_plugin_v4(p: *mut PluginV4) {
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
