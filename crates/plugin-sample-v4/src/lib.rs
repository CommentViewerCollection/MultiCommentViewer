use plugin_abi_helper::v4::plugin::PluginImplV4Async;

#[derive(Default)]
struct SamplePlugin;

#[async_trait::async_trait]
impl PluginImplV4Async for SamplePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        ctx.send_message(b"plugin-hello").await;
    }

    async fn on_message(&mut self, _ctx: PluginContext, msg: &[u8]) {
        do_heavy_io(msg).await;
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {}
}

export_plugin_v4_async!(SamplePlugin);
