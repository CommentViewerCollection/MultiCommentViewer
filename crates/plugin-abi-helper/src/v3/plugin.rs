use std::ffi::c_void;

use crate::{abi::v3::{PLUGIN_ABI_VERSION, PluginV3}, v3::{AbiError, host::Host, trampoline::TrampolineV3, validate_plugin_v3}};

pub trait PluginImplV3 {
    fn on_loaded(&mut self, host: Host);
    fn on_message(&mut self, host: Host, msg: &[u8]);
    fn on_shutdown(&mut self, host: Host);
}
pub struct PluginFactoryV3;

impl PluginFactoryV3 {
    pub fn new<T: PluginImplV3 + Default>() -> *mut PluginV3 {
        let boxed = Box::new(T::default());
        let userdata = Box::into_raw(boxed) as *mut c_void;

        let plugin = Box::new(PluginV3 {
            abi_version: PLUGIN_ABI_VERSION,
            plugin_id: 0,
            host: core::ptr::null(),
            on_loaded: TrampolineV3::<T>::on_loaded,
            on_message: TrampolineV3::<T>::on_message,
            on_shutdown: TrampolineV3::<T>::on_shutdown,
            userdata,
        });

        Box::into_raw(plugin)
    }
}


pub unsafe fn destroy_plugin_v3(p: *mut PluginV3) {
    if p.is_null() {
        return;
    }

    let plugin = Box::from_raw(p);
    drop(Box::from_raw(plugin.userdata));
}

pub struct PluginHandle {
    raw: *mut PluginV3,
}

impl PluginHandle {
    pub unsafe fn from_raw(p: *mut PluginV3) -> Result<Self, AbiError> {
        validate_plugin_v3(p)?;
        Ok(Self { raw: p })
    }

    pub fn plugin_id(&self) -> u64 {
        unsafe { (*self.raw).plugin_id }
    }
}
