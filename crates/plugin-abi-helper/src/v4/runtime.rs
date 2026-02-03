use once_cell::sync::OnceCell;
use tokio::runtime::{Builder, Runtime};

static RUNTIME: OnceCell<Runtime> = OnceCell::new();

//process-wide で 1 個
//plugin ごとに runtime を作らない
//Core スレッドと完全分離
pub fn runtime() -> &'static Runtime {
    RUNTIME.get_or_init(|| {
        Builder::new_multi_thread()
            .worker_threads(2)
            .enable_all()
            .build()
            .expect("failed to create tokio runtime")
    })
}
use std::sync::mpsc::{channel, Sender};
use std::thread;

use crate::v4::runtime_event::RuntimeEvent;
use crate::v4::plugin_async::PluginImplV4Async;
use crate::v4::context::PluginContext;
// use crate::v4::runtime::runtime;

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