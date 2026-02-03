use std::ffi::c_void;

use crate::{
    abi::v4::{HostRuntimeV4, PLUGIN_ABI_VERSION, PluginV4},
    v4::plugin_async::PluginImplV4Async,
};

pub mod context;
pub mod factory;
pub mod host;
pub mod macros;
pub mod plugin;
pub mod plugin_async;
pub mod plugin_runtime;
pub mod runtime;
pub mod runtime_event;
pub mod trampoline;

pub fn validate_plugin_v4(p: *const PluginV4) -> Result<(), AbiError> {
    if p.is_null() {
        return Err(AbiError::NullPlugin);
    }

    let plugin = unsafe { &*p };

    if plugin.abi_version != PLUGIN_ABI_VERSION {
        return Err(AbiError::VersionMismatch {
            expected: PLUGIN_ABI_VERSION,
            found: plugin.abi_version,
        });
    }

    Ok(())
}
pub enum AbiError {
    NullPlugin,
    VersionMismatch { expected: u32, found: u32 },
}
