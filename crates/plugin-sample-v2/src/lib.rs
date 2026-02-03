use async_trait::async_trait;
use std::sync::Arc;
use uuid::Uuid;

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use mcv_plugin_interface::{Plugin, PluginError, PluginHost};

use plugin_abi_helper::v2 as abi;

/// ===== プラグイン本体 =====

pub struct SamplePlugin {
    plugin_id: Uuid,
}

impl SamplePlugin {
    pub fn new() -> Self {
        Self {
            plugin_id: Uuid::new_v4(),
        }
    }
}

#[async_trait]
impl Plugin for SamplePlugin {
    async fn on_loaded(&mut self, host: Arc<dyn PluginHost>) -> Result<(), PluginError> {
        let hello = PluginHelloPayload {
            name: "Sample Plugin".into(),
            plugin_id: self.plugin_id,
            role: vec!["sample".into()],
            api_version: "v2".into(),
        };

        let msg = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(hello).unwrap(),
        );

        host.send_message(msg).await?;
        Ok(())
    }

    async fn on_message(
        &mut self,
        message: McvMessage,
        _host: Arc<dyn PluginHost>,
    ) -> Result<(), PluginError> {
        println!("SamplePlugin received: {:?}", message.message_type);
        Ok(())
    }

    async fn on_shutdown(&mut self) -> Result<(), PluginError> {
        println!("SamplePlugin shutdown");
        Ok(())
    }
}

/// ===== C ABI =====

#[unsafe(no_mangle)]
pub extern "C" fn plugin_init(_: *mut libc::c_void) -> i32 {
    abi::init_plugin(Box::new(SamplePlugin::new()))
}

#[unsafe(no_mangle)]
pub extern "C" fn plugin_on_loaded() -> i32 {
    abi::call_on_loaded()
}

#[unsafe(no_mangle)]
pub extern "C" fn plugin_send_message(msg: *const std::os::raw::c_char) -> i32 {
    abi::call_on_message(msg)
}

#[unsafe(no_mangle)]
pub extern "C" fn plugin_set_callback(
    cb: extern "C" fn(*const std::os::raw::c_char, *mut std::ffi::c_void),
    userdata: *mut std::ffi::c_void,
) -> i32 {
    abi::set_callback(cb, userdata);
    0
}

#[unsafe(no_mangle)]
pub extern "C" fn plugin_shutdown() -> i32 {
    abi::shutdown();
    0
}