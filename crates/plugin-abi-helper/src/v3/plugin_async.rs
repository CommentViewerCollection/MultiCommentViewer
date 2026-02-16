use crate::v3::context::PluginContext;
use async_trait::async_trait;

#[async_trait]
pub trait PluginImplV3Async: Send + 'static {
    async fn on_loaded(&mut self, ctx: PluginContext);
    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]);
    async fn on_shutdown(&mut self, ctx: PluginContext);
}
