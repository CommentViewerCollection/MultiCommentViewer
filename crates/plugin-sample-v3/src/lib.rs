//! plugin-sample-v3 - v3 ABIのサンプルプラグイン
//!
//! async/awaitを使用した非同期プラグインの実装例

use plugin_abi_helper::v3::prelude::*;
#[derive(Default)]
struct SamplePlugin {
    message_count: u32,
}

#[async_trait]
impl PluginImplV3Async for SamplePlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        let plugin_id = ctx.plugin_uuid();
        ctx.host().log(2, &format!("SamplePlugin v3 loaded, ID: {}", plugin_id));

        // plugin-helloを送信
        let hello = serde_json::json!({
            "message_type": "plugin-hello",
            "src": { "Plugin": { "plugin_id": plugin_id.to_string() } },
            "dst": "Core",
            "timestamp": 0,
            "payload": {
                "name": "Sample Plugin v3",
                "plugin_id": plugin_id.to_string(),
                "role": ["sample"],
                "api_version": "v3"
            }
        });

        let json = serde_json::to_vec(&hello).unwrap();
        ctx.send_message(&json).await;

        ctx.host().log(2, "plugin-hello sent");
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        self.message_count += 1;

        // メッセージをパース
        match serde_json::from_slice::<serde_json::Value>(msg) {
            Ok(message) => {
                let msg_type = message["message_type"].as_str().unwrap_or("unknown");
                ctx.host().log(
                    1,
                    &format!("Message #{}: {}", self.message_count, msg_type),
                );

                // 非同期処理のデモ
                if msg_type == "connect" {
                    ctx.host().log(2, "Processing connect request...");
                    // 実際のプラグインではここでWebSocket接続などの非同期処理を行う
                }
            }
            Err(e) => {
                ctx.host().log(4, &format!("Failed to parse message: {}", e));
            }
        }
    }

    async fn on_shutdown(&mut self, ctx: PluginContext) {
        ctx.host().log(
            2,
            &format!(
                "SamplePlugin shutting down, processed {} messages",
                self.message_count
            ),
        );
    }
}

export_plugin_v3_async!(SamplePlugin);
