use mcv_messages::{
    AddSitePayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload,
};
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;
#[derive(Default)]
struct YouTubeLivePlugin {
    logical_plugin_id: Uuid,//現在の実装ではplugin-helloは1回しか送らないから1つで良い。
    is_initialized: bool,
}
impl YouTubeLivePlugin {
    pub fn initialize(&mut self) {
        if self.is_initialized {
            return;
        }
        self.is_initialized = true;
        self.logical_plugin_id = Uuid::new_v4();
    }
    async fn send_message(ctx: PluginContext, message: McvMessage) {
        let json = serde_json::to_vec(&message).unwrap();
        ctx.send_message(&json).await;
    }
    async fn send_plugin_hello(
        &self,
        ctx: PluginContext,
        payload: PluginHelloPayload,
        plugin_id: Uuid,
    ) {
        let message = McvMessage::new(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let json = serde_json::to_vec(&message).unwrap();
        ctx.send_message(&json).await;
    }
    async fn send_add_site(&self, ctx: PluginContext, payload: AddSitePayload, plugin_id: Uuid) {
        let message = McvMessage::new(
            MessageType::AddSite,
            MessageSource::Plugin {
                plugin_id: plugin_id,
            },
            MessageDestination::Core,
            serde_json::to_value(&payload).unwrap(),
        );
        let json = serde_json::to_vec(&message).unwrap();
        ctx.send_message(&json).await;
    }
}

#[async_trait::async_trait]
impl PluginImplV3Async for YouTubeLivePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {        
        self.initialize();
        // plugin-hello送信
        let hello_payload = PluginHelloPayload {
            name: "YouTubeLive".to_string(),
            plugin_id: self.logical_plugin_id,
            role: vec!["youtubelive".to_string()],
            api_version: "v3".to_string(),
        };
        self.send_plugin_hello(ctx.clone(), hello_payload, self.logical_plugin_id)
            .await;

        let site_id = Uuid::new_v4();

        let add_site = AddSitePayload {
            site_id,
            site_name: "YouTubeLive".to_owned(),
            display_name: "なんでじゃー".to_owned(),
            options_schema: serde_json::from_str("{}").unwrap(),
        };
        self.send_add_site(ctx.clone(), add_site, self.logical_plugin_id)
            .await;
        // let message = McvMessage::new(
        //     MessageType::PluginHello,
        //     MessageSource::Plugin {
        //         plugin_id: self.plugin_id,
        //     },
        //     MessageDestination::Core,
        //     serde_json::to_value(&hello_payload).unwrap(),
        // );

        // let json = serde_json::to_vec(&message).unwrap();
        // ctx.send_message(&json).await;
    }
    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {}

    async fn on_shutdown(&mut self, ctx: PluginContext) {}
}

export_plugin_v3_async!(YouTubeLivePlugin);
