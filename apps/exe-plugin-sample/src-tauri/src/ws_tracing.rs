use std::sync::Arc;

use mcv_messages::{LogEntryPayload, Message};
use mcv_plugin_exe_interface::ExePluginClient;
use tracing::field::{Field, Visit};
use tracing::{Event, Subscriber};
use tracing_subscriber::registry::LookupSpan;
use tracing_subscriber::Layer;
use uuid::Uuid;

struct MessageVisitor {
    message: Option<String>,
    fields: Vec<(String, String)>,
}

impl MessageVisitor {
    fn new() -> Self {
        Self {
            message: None,
            fields: Vec::new(),
        }
    }
}

impl Visit for MessageVisitor {
    fn record_debug(&mut self, field: &Field, value: &dyn std::fmt::Debug) {
        if field.name() == "message" {
            self.message = Some(format!("{:?}", value));
        } else {
            self.fields.push((field.name().to_string(), format!("{:?}", value)));
        }
    }

    fn record_str(&mut self, field: &Field, value: &str) {
        if field.name() == "message" {
            self.message = Some(value.to_string());
        } else {
            self.fields.push((field.name().to_string(), value.to_string()));
        }
    }
}
pub struct WsLayer {
    client: Option<Arc<ExePluginClient>>,
    plugin_id: Option<Uuid>,
}

impl WsLayer {
    pub fn new(client: Option<Arc<ExePluginClient>>) -> Self {
        let plugin_id = client.as_ref().map(|c| c.plugin_id());
        Self {
            client,
            plugin_id,
        }
    }
}

impl<S> Layer<S> for WsLayer
where
    S: Subscriber + for<'a> LookupSpan<'a>,
{
    fn on_event(&self, event: &Event<'_>, _ctx: tracing_subscriber::layer::Context<'_, S>) {
        // clientとplugin_idが設定されているか確認
        let client = match &self.client {
            Some(c) => c,
            None => return,
        };

        let plugin_id = match self.plugin_id {
            Some(id) => id,
            None => return,
        };

        let mut visitor = MessageVisitor::new();
        event.record(&mut visitor);

        // メッセージを取得（messageフィールドがない場合はターゲットを使用）
        let message = visitor.message.unwrap_or_else(|| {
            event.metadata().target().to_string()
        });

        let metadata = event.metadata();

        // ログレベルを文字列に変換（小文字）
        let level = match *metadata.level() {
            tracing::Level::ERROR => "error",
            tracing::Level::WARN => "warn",
            tracing::Level::INFO => "info",
            tracing::Level::DEBUG => "debug",
            tracing::Level::TRACE => "trace",
        };

        // ビルドプロファイルを取得
        let build_profile = if cfg!(debug_assertions) {
            "debug"
        } else {
            "release"
        };

        // コンテキスト情報を構築（coreが期待する構造に合わせる）
        // coreは context.source.file, context.source.line, context.source.module_path を参照する
        let mut context_obj = serde_json::Map::new();

        // sourceオブジェクトをネスト構造で作成
        context_obj.insert(
            "source".to_string(),
            serde_json::json!({
                "file": metadata.file().unwrap_or("unknown"),
                "line": metadata.line().unwrap_or(0),
                "module_path": metadata.module_path().unwrap_or("unknown"),
            }),
        );

        // 追加フィールドがあればcontextのトップレベルに追加
        if !visitor.fields.is_empty() {
            for (key, value) in visitor.fields {
                context_obj.insert(key, serde_json::Value::String(value));
            }
        }

        let context = serde_json::Value::Object(context_obj);

        let log_entry_payload = LogEntryPayload {
            level: level.to_owned(),
            message,
            context: Some(context),
            connection_id: None,
            plugin_version: Some(env!("CARGO_PKG_VERSION").to_owned()),
            plugin_build_profile: Some(build_profile.to_owned()),
        };
        let payload = match serde_json::to_value(log_entry_payload) {
            Ok(g) => g,
            Err(_) => return,
        };
        let log = Message {
            message_type: mcv_messages::MessageType::LogEntry,
            src: mcv_messages::MessageSource::Plugin { plugin_id },
            dst: mcv_messages::MessageDestination::Core,
            request_id: None,
            timestamp: 0,
            payload,
        };

        let _ = client.send_message(log);
    }
}
