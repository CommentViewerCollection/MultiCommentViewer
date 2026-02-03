use std::ffi::c_void;

use async_trait::async_trait;

use crate::{abi::v4::{PLUGIN_ABI_VERSION, PluginV4}, v4::{AbiError, context::PluginContext, host::Host,  validate_plugin_v4}};

// #[async_trait]
// pub trait PluginImplV4Async: Send + 'static {
//     async fn on_loaded(&mut self, ctx: PluginContext);
//     async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]);
//     async fn on_shutdown(&mut self, ctx: PluginContext);
// }
// pub struct PluginFactoryv4;

// impl PluginFactoryv4 {
//     pub fn new<T: PluginImplv4 + Default>() -> *mut PluginV4 {
//         let boxed = Box::new(T::default());
//         let userdata = Box::into_raw(boxed) as *mut c_void;

//         let plugin = Box::new(PluginV4 {
//             abi_version: PLUGIN_ABI_VERSION,
//             plugin_id: 0,
//             host: core::ptr::null(),
//             on_loaded: Trampolinev4::<T>::on_loaded,
//             on_message: Trampolinev4::<T>::on_message,
//             on_shutdown: Trampolinev4::<T>::on_shutdown,
//             userdata,
//         });

//         Box::into_raw(plugin)
//     }
// }


pub unsafe fn destroy_plugin_v4(p: *mut PluginV4) {
    if p.is_null() {
        return;
    }

    let plugin = Box::from_raw(p);
    drop(Box::from_raw(plugin.userdata));
}

pub struct PluginHandle {
    raw: *mut PluginV4,
}

impl PluginHandle {
    pub unsafe fn from_raw(p: *mut PluginV4) -> Result<Self, AbiError> {
        validate_plugin_v4(p)?;
        Ok(Self { raw: p })
    }

    pub fn plugin_id(&self) -> u64 {
        unsafe { (*self.raw).plugin_id }
    }
}
