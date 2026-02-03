// use core::ffi::c_void;
// use crate::abi::v3::PluginV3;
// use crate::v3::host::Host;
// use crate::v3::plugin::PluginImplV3;

// /// on_loaded trampoline
// pub unsafe extern "C" fn on_loaded_trampoline<T: PluginImplV3>(
//     plugin: *mut PluginV3,
// ) -> i32 {
//     let plugin = &mut *plugin;

//     let state = &mut *(plugin.userdata as *mut T);
//     let host = Host::from_raw(plugin.host, plugin.plugin_id);

//     state.on_loaded(host);
//     0
// }

// /// on_message trampoline
// pub unsafe extern "C" fn on_message_trampoline<T: PluginImplV3>(
//     plugin: *mut PluginV3,
//     msg_ptr: *const u8,
//     msg_len: usize,
// ) -> i32 {
//     let plugin = &mut *plugin;

//     let state = &mut *(plugin.userdata as *mut T);
//     let host = Host::from_raw(plugin.host, plugin.plugin_id);

//     let msg = core::slice::from_raw_parts(msg_ptr, msg_len);
//     state.on_message(host, msg);

//     0
// }

// /// on_shutdown trampoline
// pub unsafe extern "C" fn on_shutdown_trampoline<T: PluginImplV3>(
//     plugin: *mut PluginV3,
// ) -> i32 {
//     let plugin = &mut *plugin;

//     let state = &mut *(plugin.userdata as *mut T);
//     let host = Host::from_raw(plugin.host, plugin.plugin_id);

//     state.on_shutdown(host);

//     // state 解放
//     drop(Box::from_raw(plugin.userdata as *mut T));
//     plugin.userdata = core::ptr::null_mut();

//     0
// }

use core::ffi::c_void;
use crate::abi::v3::{PluginV3, PLUGIN_ABI_VERSION};
use crate::v3::host::Host;
use crate::v3::plugin::PluginImplV3;

unsafe fn with_state<T, F>(p: *mut PluginV3, f: F) -> i32
where
    T: PluginImplV3,
    F: FnOnce(&mut T, Host),
{
    let plugin = &mut *p;
    let state = &mut *(plugin.userdata as *mut T);
    let host = Host { raw: &*plugin.host };
    f(state, host);
    0
}

unsafe fn with_state_msg<T, F>(
    p: *mut PluginV3,
    msg_ptr: *const u8,
    msg_len: usize,
    f: F,
) -> i32
where
    T: PluginImplV3,
    F: FnOnce(&mut T, Host, &[u8]),
{
    let plugin = &mut *p;
    let state = &mut *(plugin.userdata as *mut T);
    let host = Host { raw: &*plugin.host };
    let msg = core::slice::from_raw_parts(msg_ptr, msg_len);
    f(state, host, msg);
    0
}

pub struct TrampolineV3<T>(core::marker::PhantomData<T>);

impl<T: PluginImplV3> TrampolineV3<T> {
    pub extern "C" fn on_loaded(p: *mut PluginV3) -> i32 {
        unsafe { with_state::<T, _>(p, |s, h| s.on_loaded(h)) }
    }

    pub extern "C" fn on_message(
        p: *mut PluginV3,
        msg_ptr: *const u8,
        msg_len: usize,
    ) -> i32 {
        unsafe {
            with_state_msg::<T, _>(p, msg_ptr, msg_len, |s, h, m| {
                s.on_message(h, m)
            })
        }
    }

    pub extern "C" fn on_shutdown(p: *mut PluginV3) -> i32 {
        unsafe { with_state::<T, _>(p, |s, h| s.on_shutdown(h)) }
    }
}