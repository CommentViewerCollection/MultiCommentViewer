use crate::abi::v3::PluginV3;
use crate::v3::factory::PluginState;
use crate::v3::plugin_runtime::RuntimeSendError;
use crate::v3::runtime_event::RuntimeEvent;

fn map_runtime_send_result(
    state: &PluginState,
    result: Result<(), RuntimeSendError>,
    phase: &str,
) -> i32 {
    match result {
        Ok(()) => 0,
        Err(RuntimeSendError::ChannelClosed) => {
            tracing::error!(
                target: "plugin_abi_helper::v3::trampoline",
                phase = phase,
                "plugin runtime channel is closed"
            );
            -1
        }
        Err(RuntimeSendError::Panicked) => {
            tracing::error!(
                target: "plugin_abi_helper::v3::trampoline",
                phase = phase,
                panic_summary = ?state.runtime.panic_summary(),
                "plugin runtime already panicked"
            );
            -2
        }
    }
}

/// on_loaded trampoline
///
/// # Safety
///
/// `plugin`は有効な`PluginV3`ポインタで、userdataは`PluginState`を指している必要がある
pub unsafe extern "C" fn on_loaded_trampoline(plugin: *mut PluginV3) -> i32 {
    unsafe {
        let plugin = &mut *plugin;
        let state = &mut *(plugin.userdata as *mut PluginState);

        // デバッグ: trampolineで受け取ったplugin_idを確認
        let plugin_id_uuid = uuid::Uuid::from_bytes(plugin.plugin_id);
        tracing::debug!(
            target: "plugin_abi_helper::v3::trampoline",
            plugin_id = %plugin_id_uuid,
            "on_loaded_trampoline received plugin_id"
        );

        // Hostを設定（plugin_idも一緒に渡す）
        state.context.attach_host(plugin.host, plugin.plugin_id);

        // Loadedイベントを送信
        map_runtime_send_result(state, state.runtime.send(RuntimeEvent::Loaded), "on_loaded")
    }
}

/// on_message trampoline
///
/// # Safety
///
/// `p`は有効な`PluginV3`ポインタで、userdataは`PluginState`を指している必要がある
pub unsafe extern "C" fn on_message_trampoline(
    p: *mut PluginV3,
    msg_ptr: *const u8,
    msg_len: usize,
) -> i32 {
    unsafe {
        let plugin = &mut *p;
        let state = &*(plugin.userdata as *mut PluginState);

        let msg = std::slice::from_raw_parts(msg_ptr, msg_len).to_vec();
        // requestの応答はここで先に消化し、逐次on_message実行待ちによる自己待機を避ける。
        if let Ok(parsed) = serde_json::from_slice::<mcv_messages::Message>(&msg)
            && state.context.try_resolve_pending_response(&parsed)
        {
            return 0;
        }
        // 互換維持: 解析不能/未解決メッセージは従来どおりon_messageへ渡す。
        map_runtime_send_result(
            state,
            state.runtime.send(RuntimeEvent::Message(msg)),
            "on_message",
        )
    }
}

/// on_shutdown trampoline
///
/// # Safety
///
/// `p`は有効な`PluginV3`ポインタで、userdataは`PluginState`を指している必要がある
pub unsafe extern "C" fn on_shutdown_trampoline(p: *mut PluginV3) -> i32 {
    unsafe {
        let plugin = &mut *p;
        let state = &*(plugin.userdata as *mut PluginState);

        map_runtime_send_result(
            state,
            state.runtime.send(RuntimeEvent::Shutdown),
            "on_shutdown",
        )
    }
}
