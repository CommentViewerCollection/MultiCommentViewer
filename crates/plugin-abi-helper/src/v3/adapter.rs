//! PluginContext → PluginHost アダプタ
//!
//! mcv_plugin_telemetry で PluginHost トレイトが必要なため、
//! PluginContext をラップして PluginHost を実装する。

use crate::v3::context::PluginContext;
use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::{PluginError, PluginHost};

/// PluginContext → PluginHost adapter for mcv_tracing
pub struct PluginContextAdapter {
    ctx: PluginContext,
}

impl PluginContextAdapter {
    pub fn new(ctx: PluginContext) -> Self {
        Self { ctx }
    }
}

#[async_trait::async_trait]
impl PluginHost for PluginContextAdapter {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        self.ctx
            .send_notification(message)
            .await
            .map_err(|e| PluginError::MessageHandlingFailed(e.to_string()))
    }
}
