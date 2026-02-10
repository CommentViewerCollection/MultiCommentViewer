use crate::export_plugin_v3_async;
use plugin_abi_helper::v3::prelude::*;

#[derive(Default)]
struct MyPlugin;
#[async_trait]
impl PluginImplV3Async for MyPlugin {
    async fn on_loaded(&mut self, _ctx: PluginContext) {
    }

    async fn on_message(&mut self, _ctx: PluginContext, _msg: &[u8]) {
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
    }
}
export_plugin_v3_async!(MyPlugin);

fn main(){}