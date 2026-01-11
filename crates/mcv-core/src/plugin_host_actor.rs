use actix::prelude::*;
use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::{Plugin, PluginHost};
use std::sync::Arc;
use tokio::sync::Mutex;
use uuid::Uuid;

/// Plugin-Host Actor
///
/// プラグインを隔離して実行するActor
pub struct PluginHostActor {
    plugin_id: Uuid,
    plugin: Arc<Mutex<Box<dyn Plugin>>>,
    core_addr: Option<Addr<crate::core_actor::CoreActor>>,
    host: Arc<PluginHostImpl>,
}

impl PluginHostActor {
    /// 新しいPlugin-Host Actorを作成
    pub fn new(plugin_id: Uuid, plugin: Box<dyn Plugin>) -> Self {
        Self {
            plugin_id,
            plugin: Arc::new(Mutex::new(plugin)),
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
        println!("PluginHostActor started, calling plugin.on_loaded()...");
        // プラグインのon_loadedを呼び出す
        let plugin = self.plugin.clone();
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
        let plugin = self.plugin.clone();
        let message = msg.message;
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
    }
}

/// プラグインをシャットダウン
#[derive(Message)]
#[rtype(result = "()")]
pub struct ShutdownPlugin;

impl Handler<ShutdownPlugin> for PluginHostActor {
    type Result = ResponseActFuture<Self, ()>;

    fn handle(&mut self, _msg: ShutdownPlugin, _ctx: &mut Self::Context) -> Self::Result {
        let plugin = self.plugin.clone();

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
