//! PluginContext→PluginHostアダプタ
//!
//! mcv_tracingでPluginHostトレイトが必要なため、
//! PluginContextをラップしてPluginHostを実装します。

use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::{PluginError, PluginHost};
use plugin_abi_helper::v3::prelude::*;
use uuid::Uuid;

/// PluginContext → PluginHost adapter for mcv_tracing
pub(crate) struct PluginContextAdapter {
    ctx: PluginContext,
    _plugin_id: Uuid,
}

impl PluginContextAdapter {
    pub(crate) fn new(ctx: PluginContext, plugin_id: Uuid) -> Self {
        Self {
            ctx,
            _plugin_id: plugin_id,
        }
    }
}

#[async_trait::async_trait]
impl PluginHost for PluginContextAdapter {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        let json = serde_json::to_vec(&message)
            .map_err(|e| PluginError::MessageHandlingFailed(e.to_string()))?;
        self.ctx.send_message(&json).await;
        Ok(())
    }
}
