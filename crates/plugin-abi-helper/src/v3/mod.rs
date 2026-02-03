use std::ffi::c_void;

use crate::{abi::v3::{HostRuntimeV3, PLUGIN_ABI_VERSION, PluginV3}, v3::plugin::PluginImplV3};

pub mod trampoline;
pub mod host;
pub mod macros;
pub mod plugin;

pub fn validate_plugin_v3(p: *const PluginV3) -> Result<(), AbiError> {
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

