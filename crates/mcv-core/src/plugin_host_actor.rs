use actix::prelude::*;
use mcv_common::PhysicalPluginId;
use mcv_messages::Message as McvMessage;
use mcv_plugin_loader::{MessageCallbackWithUserdata, PluginLoader};
use once_cell::sync::OnceCell;
use std::collections::HashMap;
use std::ffi::{c_void, CStr};
use std::os::raw::c_char;
use std::sync::{Arc, RwLock};

use crate::internal_message::InternalMessage;

/// グローバルなPhysicalPluginHostActorアドレスマップ（callback_fn用）
static PLUGIN_HOST_ADDRS: OnceCell<
    RwLock<HashMap<PhysicalPluginId, Addr<PhysicalPluginHostActor>>>,
> = OnceCell::new();

/// Physical Plugin-Host Actor
///
/// プラグインを隔離して実行するActor（DLLプラグイン専用）
pub struct PhysicalPluginHostActor {
    physical_plugin_id: PhysicalPluginId,
    plugin_loader: Arc<PluginLoader>,
    core_addr: Option<Addr<crate::core_actor::CoreActor>>,
}

impl PhysicalPluginHostActor {
    /// 新しいPlugin-Host Actorを作成（DLLプラグイン用）
    pub fn new_from_dll(physical_plugin_id: PhysicalPluginId, plugin_loader: PluginLoader) -> Self {
        Self {
            physical_plugin_id,
            plugin_loader: Arc::new(plugin_loader),
            core_addr: None,
        }
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

impl Actor for PhysicalPluginHostActor {
    type Context = Context<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        tracing::debug!(target: "mcv::core::PluginHostActor", "PluginHostActor started");

        let physical_plugin_id = self.physical_plugin_id;
        let plugin_loader = self.plugin_loader.clone();
        let self_addr = ctx.address();

        // グローバルHashMapにPhysicalPluginHostActorのAddrを登録
        PLUGIN_HOST_ADDRS
            .get_or_init(|| RwLock::new(HashMap::new()))
            .write()
            .unwrap()
            .insert(physical_plugin_id, self_addr);

        // userdataを作成（PhysicalPluginIdをヒープに確保）
        let userdata = Box::into_raw(Box::new(physical_plugin_id)) as *mut c_void;

        // コールバック関数を定義（userdata対応）
        extern "C" fn callback_fn(message_json: *const c_char, userdata: *mut c_void) {
            unsafe {
                if message_json.is_null() || userdata.is_null() {
                    return;
                }

                // userdataからphysical_plugin_idを取得
                let physical_plugin_id = *(userdata as *const PhysicalPluginId);

                let message_cstr = CStr::from_ptr(message_json);
                if let Ok(message_str) = message_cstr.to_str() {
                    tracing::debug!(
                        target: "mcv::core::PluginHostActor",
                        physical_plugin_id = %physical_plugin_id,
                        message = %message_str,
                        "Received message from DLL plugin"
                    );

                    // JSONをパース
                    match serde_json::from_str::<McvMessage>(message_str) {
                        Ok(message) => {
                            // InternalMessageを作成
                            let internal_message = InternalMessage {
                                physical_plugin_id,
                                message,
                            };

                            // グローバルHashMapからPhysicalPluginHostActorのAddrを取得
                            if let Some(addrs) = PLUGIN_HOST_ADDRS.get() {
                                let addrs_guard = addrs.read().unwrap();
                                if let Some(actor_addr) = addrs_guard.get(&physical_plugin_id) {
                                    tracing::trace!(
                                        target: "mcv::core::PluginHostActor",
                                        "callbak_fn physical_plugin_id: {}, message_type: {:?}", physical_plugin_id, internal_message.message.message_type
                                    );
                                    actor_addr.do_send(ReceiveMessageFromDll { internal_message });
                                    tracing::debug!(
                                        target: "mcv::core::PluginHostActor",
                                        "Message forwarded to PhysicalPluginHostActor"
                                    );
                                } else {
                                    tracing::error!(
                                        target: "mcv::core::PluginHostActor",
                                        physical_plugin_id = %physical_plugin_id,
                                        "PhysicalPluginHostActor not found in global map"
                                    );
                                }
                            } else {
                                tracing::error!(
                                    target: "mcv::core::PluginHostActor",
                                    "PLUGIN_HOST_ADDRS not initialized"
                                );
                            }
                        }
                        Err(e) => {
                            tracing::error!(
                                target: "mcv::core::PluginHostActor",
                                error = %e,
                                message_json = %message_str,
                                "Failed to parse message from DLL plugin"
                            );
                        }
                    }
                }
            }
        }

        // コールバックを設定（userdata対応）
        if let Err(e) = plugin_loader.set_callback_with_userdata(callback_fn, userdata) {
            tracing::error!(
                target: "mcv::core::PluginHostActor",
                error = %e,
                "Failed to set callback"
            );
            ctx.stop();
            return;
        }

        // プラグインを初期化
        tracing::debug!(
            target: "mcv::plugin_host_actor",
            physical_plugin_id = %physical_plugin_id,
            "Initializing DLL plugin"
        );

        if let Err(e) = plugin_loader.init(std::ptr::null_mut()) {
            tracing::error!(
                target: "mcv::core::PluginHostActor",
                error = %e,
                "Plugin init failed"
            );
            ctx.stop();
            return;
        }

        tracing::info!(
            physical_plugin_id = %physical_plugin_id,
            "DLL plugin initialized successfully"
        );

        // on_loadedを呼び出し
        tracing::debug!(
            target: "mcv::plugin_host_actor",
            physical_plugin_id = %physical_plugin_id,
            "Calling DLL plugin on_loaded"
        );

        if let Err(e) = plugin_loader.on_loaded() {
            tracing::error!(
                target: "mcv::core::PluginHostActor",
                error = %e,
                "Plugin on_loaded failed"
            );
            ctx.stop();
        } else {
            tracing::info!(
                target: "mcv::core::PluginHostActor",
                physical_plugin_id = %physical_plugin_id,
                "DLL plugin on_loaded completed"
            );
        }
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

impl Handler<ReceiveMessageFromDll> for PhysicalPluginHostActor {
    type Result = ();

    fn handle(&mut self, msg: ReceiveMessageFromDll, _ctx: &mut Self::Context) {
        // CoreActorに転送
        if let Some(core_addr) = &self.core_addr {
            core_addr.do_send(crate::core_actor::SendMessageToCore {
                internal_message: msg.internal_message,
            });
        } else {
            tracing::error!(
                target: "mcv::core::PluginHostActor",
                "CoreActor address not set"
            );
        }
    }
}

/// プラグインへメッセージを送信
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendMessageToPlugin {
    pub message: McvMessage,
}

impl Handler<SendMessageToPlugin> for PhysicalPluginHostActor {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToPlugin, _ctx: &mut Self::Context) {
        let message = msg.message;

        // DLLプラグインの場合のみ
        let message_json = match serde_json::to_string(&message) {
            Ok(json) => json,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::PluginHostActor",
                    error = %e,
                    "Failed to serialize message"
                );
                return;
            }
        };

        if let Err(e) = self.plugin_loader.send_message(&message_json) {
            tracing::error!(
                target: "mcv::core::PluginHostActor",
                error = %e,
                "Failed to send message to DLL plugin"
            );
        }
    }
}

/// プラグインをシャットダウン
#[derive(Message)]
#[rtype(result = "()")]
pub struct ShutdownPlugin;

impl Handler<ShutdownPlugin> for PhysicalPluginHostActor {
    type Result = ResponseActFuture<Self, ()>;

    fn handle(&mut self, _msg: ShutdownPlugin, _ctx: &mut Self::Context) -> Self::Result {
        // DLLプラグインの場合のみ
        let plugin_loader = self.plugin_loader.clone();

        let fut = async move { plugin_loader.shutdown() };

        Box::pin(fut.into_actor(self).map(|result, _act, ctx| {
            if let Err(e) = result {
                tracing::error!(
                    target: "mcv::core::PluginHostActor",
                    error = %e,
                    "DLL plugin shutdown failed"
                );
            }
            ctx.stop();
        }))
    }
}
