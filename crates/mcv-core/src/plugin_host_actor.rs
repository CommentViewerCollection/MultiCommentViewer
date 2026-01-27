use actix::prelude::*;
use mcv_messages::Message as McvMessage;
use mcv_plugin_loader::{MessageCallback, PluginLoader};
use once_cell::sync::OnceCell;
use std::ffi::CStr;
use std::os::raw::c_char;
use std::sync::Arc;
use uuid::Uuid;

/// グローバルなCoreActorアドレス（DLLプラグインのコールバック用）
static CORE_ADDR_FOR_CALLBACK: OnceCell<Addr<crate::core_actor::CoreActor>> = OnceCell::new();

/// Physical Plugin-Host Actor
///
/// プラグインを隔離して実行するActor（DLLプラグイン専用）
pub struct PhysicalPluginHostActor {
    physical_plugin_id: Uuid,
    plugin_loader: Arc<PluginLoader>,
    core_addr: Option<Addr<crate::core_actor::CoreActor>>,
}

impl PhysicalPluginHostActor {
    /// 新しいPlugin-Host Actorを作成（DLLプラグイン用）
    pub fn new_from_dll(physical_plugin_id: Uuid, plugin_loader: PluginLoader) -> Self {
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

        if let Some(plugin) = &self.plugin {
            // 静的リンクプラグインの場合
            tracing::debug!(target: "mcv::core::PluginHostActor", "Initializing static plugin");
            let plugin = plugin.clone();
            let host = self.host.clone() as Arc<dyn PluginHost>;

            let fut = async move {
                tracing::debug!(target: "mcv::core::PluginHostActor", "Calling plugin.on_loaded()");
                let mut plugin_guard = plugin.lock().await;
                let result = plugin_guard.on_loaded(host).await;
                tracing::debug!(target: "mcv::core::PluginHostActor",   result = ?result, "plugin.on_loaded() returned");
                result
            };

            ctx.spawn(
                fut.into_actor(self).map(|result, _act, ctx| {
                    if let Err(e) = result {
                        tracing::error!(
                            target: "mcv::core::PluginHostActor", 
                            error = %e,
                            "Plugin on_loaded failed"
                        );
                        ctx.stop();
                    } else {
                        tracing::info!(target: "mcv::core::PluginHostActor", "Plugin on_loaded completed successfully");
                    }
                }),
            );
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
            tracing::debug!(target: "mcv::core::PluginHostActor", "Initializing DLL plugin");

            // グローバルなcore_addrを設定
            if let Some(core_addr) = &self.core_addr {
                let _ = CORE_ADDR_FOR_CALLBACK.set(core_addr.clone());
            }

            // コールバックを設定
            let callback: MessageCallback = {
                extern "C" fn callback_fn(message_json: *const c_char) {
                    unsafe {
                        if !message_json.is_null() {
                            let message_cstr = CStr::from_ptr(message_json);
                            if let Ok(message_str) = message_cstr.to_str() {
                                tracing::debug!(
                                    target: "mcv::core::PluginHostActor",
                                    message = %message_str,
                                    "Received message from DLL plugin"
                                );
                                
                                // JSONをパースしてCoreActorに転送
                                match serde_json::from_str::<McvMessage>(message_str) {
                                    Ok(message) => {
                                        if let Some(core_addr) = CORE_ADDR_FOR_CALLBACK.get() {
                                            core_addr.do_send(
                                                crate::core_actor::SendMessageToCore { message },
                                            );
                                            tracing::debug!(target: "mcv::core::PluginHostActor", "Message forwarded to CoreActor");
                                        } else {
                                            tracing::error!(target: "mcv::core::PluginHostActor", "CORE_ADDR_FOR_CALLBACK not set");
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
                }
                callback_fn
            };

            if let Err(e) = plugin_loader.set_callback(callback) {
                tracing::error!(
                    target: "mcv::core::PluginHostActor",
                    error = %e,
                    "Failed to set callback"
                );
                ctx.stop();
                return;
            }

            // プラグインを初期化（host_contextはnull、プラグイン側でplugin_idを生成）
            tracing::debug!(target: "mcv::plugin_host_actor", "Initializing DLL plugin (plugin will generate its own plugin_id)");

            if let Err(e) = plugin_loader.init(std::ptr::null_mut()) {
                tracing::error!(
                target: "mcv::core::PluginHostActor",
                     error = %e,
                     "Plugin init failed"
                 );
                ctx.stop();
                return;
            }

            tracing::info!("DLL plugin initialized successfully");

            // on_loadedを呼び出し
            tracing::debug!(target: "mcv::plugin_host_actor", "Calling DLL plugin on_loaded");
            if let Err(e) = plugin_loader.on_loaded() {
                tracing::error!(
                    target: "mcv::core::PluginHostActor",
                    error = %e,
                    "Plugin on_loaded failed"
                );
                ctx.stop();
            } else {
                tracing::info!(target: "mcv::core::PluginHostActor",  "DLL plugin on_loaded completed (plugin_id will be received via plugin-hello)");
            }
        } else {
            tracing::error!(target: "mcv::core::PluginHostActor",  "Neither plugin nor plugin_loader is set");
            ctx.stop();
        }
    }
}

// ============================================================================
// メッセージハンドラ
// ============================================================================

/// プラグインへメッセージを送信
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendMessageToPlugin {
    pub message: McvMessage,
}

impl Handler<SendMessageToPlugin> for PhysicalPluginHostActor {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToPlugin, ctx: &mut Self::Context) {
        let message = msg.message;

        if let Some(plugin) = &self.plugin {
            // 静的リンクプラグインの場合
            let plugin = plugin.clone();
            let host = self.host.clone() as Arc<dyn PluginHost>;

            let fut = async move {
                let mut plugin_guard = plugin.lock().await;
                plugin_guard.on_message(message, host).await
            };

            ctx.spawn(fut.into_actor(self).map(|result, _act, _ctx| {
                if let Err(e) = result {
                    tracing::error!(
                        error = %e,
                        "Plugin on_message failed"
                    );
                }
            }));
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
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

            if let Err(e) = plugin_loader.send_message(&message_json) {
                tracing::error!(
                    target: "mcv::core::PluginHostActor",
                    error = %e,
                    "Failed to send message to DLL plugin"
                );
            }
        } else {
            tracing::error!(target: "mcv::core::PluginHostActor", "Neither plugin nor plugin_loader is set");
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
        if let Some(plugin) = &self.plugin {
            // 静的リンクプラグインの場合
            let plugin = plugin.clone();

            let fut = async move {
                let mut plugin_guard = plugin.lock().await;
                plugin_guard.on_shutdown().await
            };

            Box::pin(fut.into_actor(self).map(|result, _act, ctx| {
                if let Err(e) = result {
                    tracing::error!(
                        target: "mcv::core::PluginHostActor",
                        error = %e,
                        "Plugin on_shutdown failed"
                    );
                }
                ctx.stop();
            }))
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
            let plugin_loader = plugin_loader.clone();

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
        } else {
            tracing::error!("Neither plugin nor plugin_loader is set");
            Box::pin(actix::fut::ready(()).into_actor(self).map(|_, _, ctx| {
                ctx.stop();
            }))
        }
    }
}

// ============================================================================
// PluginHost実装
// ============================================================================

// Physical PluginHostインターフェースの実装
pub struct PhysicalPluginHostImpl {
    physical_plugin_id: Uuid,
    core_addr: Arc<std::sync::Mutex<Option<Addr<crate::core_actor::CoreActor>>>>,
}

#[async_trait::async_trait]
impl PluginHost for PhysicalPluginHostImpl {
    async fn send_message(
        &self,
        mut message: McvMessage,
    ) -> Result<(), mcv_plugin_interface::PluginError> {
        //panicにしても止まらない。つまり使われていない。
        panic!()
    }
}
