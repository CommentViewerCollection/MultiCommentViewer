use std::sync::{Arc, Mutex, RwLock};

use mcv_messages::{LogEntryPayload, Message};
use mcv_plugin_exe_interface::ExePluginClient;
use std::fmt::Write;
use tracing::field::{Field, Visit};
use tracing::{Event, Subscriber};
use tracing_subscriber::layer::Context;
use tracing_subscriber::registry::{LookupSpan, SpanRef};
use tracing_subscriber::Layer;
use uuid::Uuid;

pub fn extract_plugin_id<S>(event: &Event<'_>, ctx: &Context<'_, S>) -> Option<Uuid>
where
    S: Subscriber + for<'a> LookupSpan<'a>,
{
    let scope = ctx.event_scope(event)?;

    for span in scope.from_root() {
        if let Some(plugin_id) = span.extensions().get::<Uuid>() {
            return Some(plugin_id.clone());
        }
    }

    None
}
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
    sender: Arc<Mutex<Option<Arc<ExePluginClient>>>>,
}

impl WsLayer {
    pub fn new(sender: Option<Arc<ExePluginClient>>) -> Self {
        Self {
            sender: Arc::new(Mutex::new(sender)),
        }
    }
}

impl<S> Layer<S> for WsLayer
where
    S: Subscriber + for<'a> LookupSpan<'a>,
{
    fn on_event(&self, event: &Event<'_>, ctx: tracing_subscriber::layer::Context<'_, S>) {

        let client = {
            let guard = self.sender.lock().unwrap();
            guard.clone()
        };
        let client = match client{
            Some(a)=>a,
            None=>return,
        };

        let mut visitor = StringVisitor::new();
        event.record(&mut visitor);
        let msg = visitor.buf;

        let plugin_id = extract_plugin_id(event, &ctx);
        if plugin_id.is_none() {
            return;
        }
        let plugin_id = plugin_id.unwrap();

        let metadata = event.metadata();

        let log_entry_payload = LogEntryPayload {
            level: "".to_owned(),
            message: "".to_owned(),
            context: None,
            connection_id: None,
            plugin_version: None,
            plugin_build_profile: None,
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
        let _ = std::fs::write("C:\\Users\\ryu\\Downloads\\akkdkd.txt", "=====????DIDS Log created");
        println!("=====????DIDS Log created");

        let _ = client.send_message(log);
    }
}
fn get_log_level() -> String {
    "debug".to_owned()
}
