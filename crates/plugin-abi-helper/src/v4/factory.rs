// use crate::v4::plugin_async::PluginImplV4Async;
// use crate::v4::runtime::PluginRuntimev4;
// use crate::v4::context::PluginContext;
// use crate::abi::v4::{PluginV4, PLUGIN_ABI_VERSION};

// pub struct PluginFactory4;

// impl PluginFactoryV4 {
//     pub fn new_async<P: PluginImplV4Async + Default>() -> *mut PluginV4 {
//         let plugin_impl = P::default();
//         let ctx = PluginContext::new();

//         let runtime = PluginRuntimev4::start(plugin_impl, ctx);

//         let boxed_runtime = Box::new(runtime);

//         let plugin = Box::new(PluginV4 {
//             abi_version: PLUGIN_ABI_VERSION,
//             plugin_id: 0,
//             host: std::ptr::null(),
//             on_loaded: crate::v4::trampoline::on_loaded_trampoline,
//             on_message: crate::v4::trampoline::on_message_trampoline,
//             on_shutdown: crate::v4::trampoline::on_shutdown_trampoline,
//             userdata: Box::into_raw(boxed_runtime) as *mut _,
//         });

//         Box::into_raw(plugin)
//     }
// }



use std::ffi::c_void;

use crate::v4::plugin_async::PluginImplV4Async;
use crate::v4::runtime::PluginRuntimev4;
use crate::v4::context::PluginContext;
use crate::abi::v4::{PluginV4, PLUGIN_ABI_VERSION};

pub struct PluginFactoryV4;

impl PluginFactoryV4 {
    pub fn new<T: PluginImplV4Async + Default>() -> *mut PluginV4 {
        let ctx = Box::new(PluginContext::new());
        let ctx_ptr = Box::into_raw(ctx);

        let boxed = Box::new(T::default());
        let userdata = Box::into_raw(boxed) as *mut c_void;

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