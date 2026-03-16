//! plugin-sample-v3 - v3 ABIのサンプルプラグイン
//!
//! async/awaitを使用した非同期プラグインの実装例

use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
    PluginId,
};
use plugin_abi_helper::v3::prelude::*;
use std::time::Duration;
use uuid::Uuid;
#[derive(Default)]
struct SamplePlugin {
    message_count: u32,
    logical_plugin_id: PluginId,
}

#[async_trait]
impl PluginImplV3Async for SamplePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let uuid = Uuid::new_v4();
        self.logical_plugin_id = PluginId::new(format!("SamplePluginV3_logical_{}", uuid));
        tracing::info!("SamplePlugin v3 loaded, ID: {}", self.logical_plugin_id);

        // plugin-helloを送信
        let hello_payload = PluginHelloPayload {
            name: "Sample Plugin v3".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec![],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );
        let _ = ctx.send_request(message, Duration::from_secs(10)).await;

        tracing::info!("plugin-hello sent");
    }

    async fn on_message(&mut self, _ctx: PluginContext, msg: &[u8]) {
        self.message_count += 1;

        // メッセージをパース
        match serde_json::from_slice::<serde_json::Value>(msg) {
            Ok(message) => {
                let msg_type = message["message_type"].as_str().unwrap_or("unknown");
                tracing::debug!("Message #{}: {}", self.message_count, msg_type);

                // 非同期処理のデモ
                if msg_type == "connect" {
                    tracing::info!("Processing connect request...");
                    // 実際のプラグインではここでWebSocket接続などの非同期処理を行う
                }
            }
            Err(e) => {
                tracing::error!("Failed to parse message: {}", e);
            }
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!(
            "SamplePlugin shutting down, processed {} messages",
            self.message_count
        );
    }
}

export_plugin_v3_async!(SamplePlugin);
