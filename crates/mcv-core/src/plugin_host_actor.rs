use actix::prelude::*;
use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::{Plugin, PluginHost};
use mcv_plugin_loader::{PluginLoader, MessageCallback};
use std::ffi::CStr;
use std::os::raw::c_char;
use std::sync::Arc;
use tokio::sync::Mutex;
use uuid::Uuid;

/// Plugin-Host Actor
///
/// プラグインを隔離して実行するActor
pub struct PluginHostActor {
    plugin_id: Uuid,
    plugin: Option<Arc<Mutex<Box<dyn Plugin>>>>,
    plugin_loader: Option<Arc<PluginLoader>>,
    core_addr: Option<Addr<crate::core_actor::CoreActor>>,
    host: Arc<PluginHostImpl>,
}

impl PluginHostActor {
    /// 新しいPlugin-Host Actorを作成（静的リンクプラグイン用）
    pub fn new(plugin_id: Uuid, plugin: Box<dyn Plugin>) -> Self {
        Self {
            plugin_id,
            plugin: Some(Arc::new(Mutex::new(plugin))),
            plugin_loader: None,
            core_addr: None,
            host: Arc::new(PluginHostImpl {
                core_addr: Arc::new(Mutex::new(None)),
            }),
        }
    }

    /// 新しいPlugin-Host Actorを作成（DLLプラグイン用）
    pub fn new_from_dll(plugin_id: Uuid, plugin_loader: PluginLoader) -> Self {
        Self {
            plugin_id,
            plugin: None,
            plugin_loader: Some(Arc::new(plugin_loader)),
            core_addr: None,
            host: Arc::new(PluginHostImpl {
                core_addr: Arc::new(Mutex::new(None)),
            }),
        }
    }

    /// Core Actorのアドレスを設定
    pub fn set_core_addr(&mut self, addr: Addr<crate::core_actor::CoreActor>) {
        self.core_addr = Some(addr.clone());
        // Hostの共有インスタンスにも設定
        let core_addr_clone = addr.clone();
        let host = self.host.clone();
        actix::spawn(async move {
            *host.core_addr.lock().await = Some(core_addr_clone);
        });
    }

    /// Core Actorのアドレスを取得
    pub fn get_core_addr(&self) -> Option<Addr<crate::core_actor::CoreActor>> {
        self.core_addr.clone()
    }
}

impl Actor for PluginHostActor {
    type Context = Context<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        println!("PluginHostActor started");

        if let Some(plugin) = &self.plugin {
            // 静的リンクプラグインの場合
            println!("Initializing static plugin...");
            let plugin = plugin.clone();
            let host = self.host.clone() as Arc<dyn PluginHost>;

            let fut = async move {
                println!("About to call plugin.on_loaded()");
                let mut plugin_guard = plugin.lock().await;
                let result = plugin_guard.on_loaded(host).await;
                println!("plugin.on_loaded() returned: {:?}", result);
                result
            };

            ctx.spawn(
                fut.into_actor(self).map(|result, _act, ctx| {
                    if let Err(e) = result {
                        eprintln!("Plugin on_loaded failed: {}", e);
                        ctx.stop();
                    } else {
                        println!("Plugin on_loaded completed successfully");
                    }
                }),
            );
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
            println!("Initializing DLL plugin...");

            // コールバックを設定
            let _core_addr = self.core_addr.clone();
            let callback: MessageCallback = {
                extern "C" fn callback_fn(message_json: *const c_char) {
                    // グローバルなコンテキストからcore_addrを取得する必要がある
                    // ここでは単純化のため、ログ出力のみ
                    unsafe {
                        if !message_json.is_null() {
                            let message_cstr = CStr::from_ptr(message_json);
                            if let Ok(message_str) = message_cstr.to_str() {
                                println!("Received message from DLL plugin: {}", message_str);
                                // TODO: JSONをパースしてCoreActorに転送
                            }
                        }
                    }
                }
                callback_fn
            };

            if let Err(e) = plugin_loader.set_callback(callback) {
                eprintln!("Failed to set callback: {}", e);
                ctx.stop();
                return;
            }

            // プラグインを初期化
            if let Err(e) = plugin_loader.init(std::ptr::null_mut()) {
                eprintln!("Plugin init failed: {}", e);
                ctx.stop();
            } else {
                println!("DLL plugin initialized successfully");
            }
        } else {
            eprintln!("ERROR: Neither plugin nor plugin_loader is set");
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

impl Handler<SendMessageToPlugin> for PluginHostActor {
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

            ctx.spawn(
                fut.into_actor(self).map(|result, _act, _ctx| {
                    if let Err(e) = result {
                        eprintln!("Plugin on_message failed: {}", e);
                    }
                }),
            );
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
            let message_json = match serde_json::to_string(&message) {
                Ok(json) => json,
                Err(e) => {
                    eprintln!("Failed to serialize message: {}", e);
                    return;
                }
            };

            if let Err(e) = plugin_loader.send_message(&message_json) {
                eprintln!("Failed to send message to DLL plugin: {}", e);
            }
        } else {
            eprintln!("ERROR: Neither plugin nor plugin_loader is set");
        }
    }
}

/// プラグインをシャットダウン
#[derive(Message)]
#[rtype(result = "()")]
pub struct ShutdownPlugin;

impl Handler<ShutdownPlugin> for PluginHostActor {
    type Result = ResponseActFuture<Self, ()>;

    fn handle(&mut self, _msg: ShutdownPlugin, _ctx: &mut Self::Context) -> Self::Result {
        if let Some(plugin) = &self.plugin {
            // 静的リンクプラグインの場合
            let plugin = plugin.clone();

            let fut = async move {
                let mut plugin_guard = plugin.lock().await;
                plugin_guard.on_shutdown().await
            };

            Box::pin(
                fut.into_actor(self).map(|result, _act, ctx| {
                    if let Err(e) = result {
                        eprintln!("Plugin on_shutdown failed: {}", e);
                    }
                    ctx.stop();
                }),
            )
        } else if let Some(plugin_loader) = &self.plugin_loader {
            // DLLプラグインの場合
            let plugin_loader = plugin_loader.clone();

            let fut = async move {
                plugin_loader.shutdown()
            };

            Box::pin(
                fut.into_actor(self).map(|result, _act, ctx| {
                    if let Err(e) = result {
                        eprintln!("DLL plugin shutdown failed: {}", e);
                    }
                    ctx.stop();
                }),
            )
        } else {
            eprintln!("ERROR: Neither plugin nor plugin_loader is set");
            Box::pin(actix::fut::ready(()).into_actor(self).map(|_, _, ctx| {
                ctx.stop();
            }))
        }
    }
}

// ============================================================================
// PluginHost実装
// ============================================================================

/// PluginHostインターフェースの実装
pub struct PluginHostImpl {
    core_addr: Arc<Mutex<Option<Addr<crate::core_actor::CoreActor>>>>,
}

#[async_trait::async_trait]
impl PluginHost for PluginHostImpl {
    async fn send_message(
        &self,
        message: McvMessage,
    ) -> Result<(), mcv_plugin_interface::PluginError> {
        println!("PluginHostImpl::send_message called, message_type: {:?}", message.message_type);
        let core_addr_guard = self.core_addr.lock().await;
        if let Some(core_addr) = core_addr_guard.as_ref() {
            println!("Sending message to CoreActor: {:?}", message.message_type);
            core_addr.do_send(crate::core_actor::SendMessageToCore { message });
            println!("Message sent to CoreActor");
            Ok(())
        } else {
            eprintln!("ERROR: Core actor not set in PluginHostImpl");
            Err(mcv_plugin_interface::PluginError::ConnectionError(
                "Core actor not set".to_string(),
            ))
        }
    }
}
