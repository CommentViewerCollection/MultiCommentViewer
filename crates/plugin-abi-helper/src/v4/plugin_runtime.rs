use std::sync::mpsc::{channel, Sender};
use std::thread;

use crate::v4::runtime_event::RuntimeEvent;
use crate::v4::plugin_async::PluginImplV4Async;
use crate::v4::context::PluginContext;
use crate::v4::runtime::runtime;

pub struct PluginRuntimeV4 {
    tx: Sender<RuntimeEvent>,
}

impl PluginRuntimeV4 {
    pub fn start<P: PluginImplV4Async>(mut plugin: P, ctx: PluginContext) -> Self {
        let (tx, rx) = channel::<RuntimeEvent>();

        thread::spawn(move || {
            for event in rx {
                match event {
                    RuntimeEvent::Loaded => {
                        runtime().block_on(plugin.on_loaded(ctx.clone()));
                    }
                    RuntimeEvent::Message(msg) => {
                        runtime().block_on(plugin.on_message(ctx.clone(), &msg));
                    }
                    RuntimeEvent::Shutdown => {
                        runtime().block_on(plugin.on_shutdown(ctx));
                        break;
                    }
                }
            }
        });

        Self { tx }
    }

    pub fn send(&self, ev: RuntimeEvent) {
        let _ = self.tx.send(ev);
    }
}