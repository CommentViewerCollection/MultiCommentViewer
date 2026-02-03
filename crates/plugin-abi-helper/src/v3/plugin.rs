use crate::{abi::v3::PluginV3, v3::{AbiError, validate_plugin_v3}};

// destroy_plugin_v3 は factory.rs に移動済み
pub use crate::v3::factory::destroy_plugin_v3;

pub struct PluginHandle {
    raw: *mut PluginV3,
}

impl PluginHandle {
    /// # Safety
    ///
    /// `p`は有効な`PluginV3`ポインタでなければならない
    pub unsafe fn from_raw(p: *mut PluginV3) -> Result<Self, AbiError> {
        validate_plugin_v3(p)?;
        Ok(Self { raw: p })
    }

    pub fn plugin_id(&self) -> u64 {
        unsafe { (*self.raw).plugin_id }
    }
}
