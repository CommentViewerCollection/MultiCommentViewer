use crate::{abi::v4::PluginV4, v4::{AbiError, validate_plugin_v4}};

// destroy_plugin_v4 は factory.rs に移動済み
pub use crate::v4::factory::destroy_plugin_v4;

pub struct PluginHandle {
    raw: *mut PluginV4,
}

impl PluginHandle {
    /// # Safety
    ///
    /// `p`は有効な`PluginV4`ポインタでなければならない
    pub unsafe fn from_raw(p: *mut PluginV4) -> Result<Self, AbiError> {
        validate_plugin_v4(p)?;
        Ok(Self { raw: p })
    }

    pub fn plugin_id(&self) -> u64 {
        unsafe { (*self.raw).plugin_id }
    }
}
