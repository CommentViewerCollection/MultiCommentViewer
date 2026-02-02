use std::sync::Arc;

use mcv_messages::{LogEntryPayload, Message};
use mcv_plugin_exe_interface::ExePluginClient;
use std::fmt::Write;
use tracing::field::{Field, Visit};
use tracing::{Event, Subscriber};
use tracing_subscriber::registry::LookupSpan;
use tracing_subscriber::Layer;
use uuid::Uuid;

struct StringVisitor {
    buf: String,
}

impl StringVisitor {
    fn new() -> Self {
        Self { buf: String::new() }
    }
}

impl Visit for StringVisitor {
    fn record_debug(&mut self, field: &Field, value: &dyn std::fmt::Debug) {
        let _ = write!(self.buf, "{}={:?} ", field.name(), value);
    }

    fn record_str(&mut self, field: &Field, value: &str) {
        let _ = write!(self.buf, "{}={} ", field.name(), value);
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

        let mut visitor = StringVisitor::new();
        event.record(&mut visitor);
        let message = visitor.buf;

        let metadata = event.metadata();

        // ログレベルを文字列に変換
        let level = match *metadata.level() {
            tracing::Level::ERROR => "ERROR",
            tracing::Level::WARN => "WARN",
            tracing::Level::INFO => "INFO",
            tracing::Level::DEBUG => "DEBUG",
            tracing::Level::TRACE => "TRACE",
        };

        // ビルドプロファイルを取得
        let build_profile = if cfg!(debug_assertions) {
            "debug"
        } else {
            "release"
        };

        let log_entry_payload = LogEntryPayload {
            level: level.to_owned(),
            message,
            context: Some(serde_json::json!({
                "file": metadata.file().unwrap_or("unknown"),
                "line": metadata.line().unwrap_or(0),
                "target": metadata.target(),
            })),
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
