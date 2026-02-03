use actix::prelude::*;
use mcv_common::PhysicalPluginId;
use mcv_plugin_loader_v3::PluginLoaderV3;
use plugin_abi_helper::abi::v3::HostRuntimeV3;
use std::sync::Arc;
use uuid::Uuid;

use crate::plugin_host_actor::SendMessageToPlugin;

/// Physical Plugin-Host Actor for v3 ABI
///
/// v3プラグインを隔離して実行するActor
pub struct PhysicalPluginHostActorV3 {
    physical_plugin_id: PhysicalPluginId,
    plugin_id: Uuid,
    plugin_loader: Arc<PluginLoaderV3>,
    core_addr: Option<Addr<crate::core_actor::CoreActor>>,
    host_runtime: Box<HostRuntimeV3>,
}

impl PhysicalPluginHostActorV3 {
    /// 新しいv3 Plugin-Host Actorを作成
    pub fn new(
        physical_plugin_id: PhysicalPluginId,
        plugin_id: Uuid,
        plugin_loader: Arc<PluginLoaderV3>,
        core_addr: Option<Addr<crate::core_actor::CoreActor>>,
    ) -> Self {
        // HostRuntimeV3を作成
        let host_runtime = Box::new(HostRuntimeV3 {
            send_message: Self::host_send_message,
        });

        Self {
            physical_plugin_id,
            plugin_id,
            plugin_loader,
            core_addr,
            host_runtime,
        }
    }

    /// Host側のsend_message実装
    ///
    /// プラグインからのメッセージをCoreに転送する
    unsafe extern "C" fn host_send_message(json_ptr: *const u8, json_len: usize) {
        if json_ptr.is_null() || json_len == 0 {
            tracing::error!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                "Invalid message: null pointer or zero length"
            );
            return;
        }

        let msg_slice = unsafe { std::slice::from_raw_parts(json_ptr, json_len) };
        let msg_str = match std::str::from_utf8(msg_slice) {
            Ok(s) => s,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::PhysicalPluginHostActorV3",
                    error = %e,
                    "Failed to parse message as UTF-8"
                );
                return;
            }
        };

        tracing::trace!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            message = %msg_str,
            "Received message from plugin"
        );

        // JSONをパースしてCoreに送信
        // Note: 現在はcore_addrにアクセスできないため、ログ出力のみ
        // 実際の実装では、userdataなどを使ってcore_addrを取得する必要がある
    }

    /// Core Actorのアドレスを設定
    pub fn set_core_addr(&mut self, addr: Addr<crate::core_actor::CoreActor>) {
        self.core_addr = Some(addr);
    }

    /// Core Actorのアドレスを取得
    pub fn get_core_addr(&self) -> Option<Addr<crate::core_actor::CoreActor>> {
        self.core_addr.clone()
    }
}

impl Actor for PhysicalPluginHostActorV3 {
    type Context = Context<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        tracing::debug!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %self.physical_plugin_id.inner(),
            plugin_id = %self.plugin_id,
            "v3 Plugin host actor started"
        );

        let plugin = self.plugin_loader.get_plugin_mut();

        // plugin_idを設定
        plugin.plugin_id = *self.plugin_id.as_bytes();

        // hostポインタを設定
        plugin.host = &*self.host_runtime as *const HostRuntimeV3;

        // on_loaded呼び出し
        let result = unsafe { (plugin.on_loaded)(plugin) };

        if result != 0 {
            tracing::error!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                result = result,
                "Plugin on_loaded returned error"
            );
            ctx.stop();
        }
    }

    fn stopped(&mut self, _ctx: &mut Self::Context) {
        tracing::debug!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %self.physical_plugin_id.inner(),
            "v3 Plugin host actor stopped"
        );

        let plugin = self.plugin_loader.get_plugin_mut();

        // on_shutdown呼び出し
        let result = unsafe { (plugin.on_shutdown)(plugin) };

        if result != 0 {
            tracing::warn!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                result = result,
                "Plugin on_shutdown returned error"
            );
        }
    }
}

impl Handler<SendMessageToPlugin> for PhysicalPluginHostActorV3 {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToPlugin, _ctx: &mut Self::Context) {
        tracing::trace!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %self.physical_plugin_id.inner(),
            "Sending message to v3 plugin"
        );

        let json = match serde_json::to_vec(&msg.message) {
            Ok(json) => json,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::PhysicalPluginHostActorV3",
                    error = %e,
                    "Failed to serialize message"
                );
                return;
            }
        };

        let plugin = self.plugin_loader.get_plugin_mut();

        // on_message呼び出し
        let result = unsafe { (plugin.on_message)(plugin, json.as_ptr(), json.len()) };

        if result != 0 {
            tracing::warn!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                result = result,
                "Plugin on_message returned error"
            );
        }
    }
}

impl Handler<crate::plugin_host_actor::ShutdownPlugin> for PhysicalPluginHostActorV3 {
    type Result = ();

    fn handle(&mut self, _msg: crate::plugin_host_actor::ShutdownPlugin, ctx: &mut Self::Context) {
        tracing::info!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %self.physical_plugin_id.inner(),
            "Shutting down v3 plugin"
        );
        ctx.stop();
    }
}
