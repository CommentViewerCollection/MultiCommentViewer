use actix::prelude::*;
use mcv_common::PhysicalPluginId;
use mcv_messages::Message as McvMessage;
use mcv_plugin_loader_v3::PluginLoaderV3;
use plugin_abi_helper::abi::v3::{HostRuntimeV3, PLUGIN_ABI_VERSION};
use std::ffi::c_void;
use std::sync::Arc;
use uuid::Uuid;

use crate::internal_message::InternalMessage;
use crate::plugin_host_actor::SendMessageToPlugin;

/// Callback用のデータ（userdataに格納）
struct CallbackData {
    physical_plugin_id: PhysicalPluginId,
    actor_addr: Addr<PhysicalPluginHostActorV3>,
}

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
        // HostRuntimeV3を作成（userdataはstarted()で設定）
        let host_runtime = Box::new(HostRuntimeV3 {
            abi_version: PLUGIN_ABI_VERSION,
            send_message: Self::host_send_message,
            userdata: std::ptr::null_mut(),
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
    unsafe extern "C" fn host_send_message(
        host: *const HostRuntimeV3,
        json_ptr: *const u8,
        json_len: usize,
    ) -> i32 {
        if host.is_null() || json_ptr.is_null() {
            tracing::error!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                "Invalid arguments: null pointer"
            );
            return -1;
        }

        // HostRuntimeV3からuserdataを取得
        let host_ref = &*host;
        if host_ref.userdata.is_null() {
            tracing::error!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                "userdata is null"
            );
            return -1;
        }

        // CallbackDataを取得
        let callback_data = &*(host_ref.userdata as *const CallbackData);
        let physical_plugin_id = callback_data.physical_plugin_id;

        // JSONをパース
        let msg_slice = std::slice::from_raw_parts(json_ptr, json_len);
        let message = match serde_json::from_slice::<McvMessage>(msg_slice) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::PhysicalPluginHostActorV3",
                    error = %e,
                    "Failed to parse message from v3 plugin"
                );
                return -1;
            }
        };

        // InternalMessageを作成
        let internal_message = InternalMessage {
            physical_plugin_id,
            message,
        };

        // ReceiveMessageFromDllをactor_addrに送信
        callback_data
            .actor_addr
            .do_send(ReceiveMessageFromDll { internal_message });

        tracing::debug!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %physical_plugin_id,
            "Message forwarded to CoreActor"
        );

        0
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
        let physical_plugin_id = self.physical_plugin_id;
        let self_addr = ctx.address();

        tracing::debug!(
            target: "mcv::core::PhysicalPluginHostActorV3",
            physical_plugin_id = %physical_plugin_id.inner(),
            plugin_id = %self.plugin_id,
            "v3 Plugin host actor started"
        );

        // CallbackDataをヒープに確保
        let callback_data = CallbackData {
            physical_plugin_id,
            actor_addr: self_addr,
        };
        let userdata = Box::into_raw(Box::new(callback_data)) as *mut c_void;

        // HostRuntimeV3のuserdataに設定
        self.host_runtime.userdata = userdata;

        let plugin = self.plugin_loader.get_plugin_mut();

        // plugin_idを設定
        plugin.plugin_id = *self.plugin_id.as_bytes();

        // PluginV3のuserdataにも設定
        plugin.userdata = userdata;

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

// ============================================================================
// メッセージハンドラ
// ============================================================================

/// DLLプラグインからメッセージを受信
#[derive(Message)]
#[rtype(result = "()")]
pub struct ReceiveMessageFromDll {
    pub internal_message: InternalMessage,
}

impl Handler<ReceiveMessageFromDll> for PhysicalPluginHostActorV3 {
    type Result = ();

    fn handle(&mut self, msg: ReceiveMessageFromDll, _ctx: &mut Self::Context) {
        // CoreActorに転送
        if let Some(core_addr) = &self.core_addr {
            core_addr.do_send(crate::core_actor::SendMessageToCore {
                internal_message: msg.internal_message,
            });
        } else {
            tracing::error!(
                target: "mcv::core::PhysicalPluginHostActorV3",
                "CoreActor address not set"
            );
        }
    }
}
