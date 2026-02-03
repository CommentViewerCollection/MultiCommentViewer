use crate::abi::v4::PluginV4;
use crate::v4::host::Host;
use crate::v4::runtime_event::RuntimeEvent;

unsafe extern "C" fn on_loaded_trampoline(plugin: *mut PluginV4) -> i32 {
    let plugin = &mut *plugin;

    let ctx = &*(plugin.userdata as *mut PluginContext);
    ctx.attach_host(plugin.host);

    let impl_ = &mut *(plugin.userdata as *mut Self);

    let host = Host::from_raw(plugin.host);
    impl_.on_loaded(host);

    0
}

pub unsafe extern "C" fn on_message_trampoline(
    p: *mut PluginV4,
    msg_ptr: *const u8,
    msg_len: usize,
) -> i32 {
    let plugin = &mut *p;
    let runtime = &*(plugin.userdata as *mut crate::v4::plugin_runtime::PluginRuntimeV4);

    let msg = std::slice::from_raw_parts(msg_ptr, msg_len).to_vec();
    runtime.send(RuntimeEvent::Message(msg));
    0
}

pub unsafe extern "C" fn on_shutdown_trampoline(p: *mut PluginV4) -> i32 {
    let plugin = &mut *p;
    let runtime = &*(plugin.userdata as *mut crate::v4::plugin_runtime::PluginRuntimeV4);

    runtime.send(RuntimeEvent::Shutdown);
    0
}