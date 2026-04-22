use std::sync::mpsc::{Sender, channel};
use std::sync::{
    Arc, Mutex,
    atomic::{AtomicBool, Ordering},
};
use std::thread;

use crate::v3::context::PluginContext;
use crate::v3::plugin_async::PluginImplV3Async;
use crate::v3::runtime::runtime;
use crate::v3::runtime_event::RuntimeEvent;

#[derive(Debug, Clone, Copy)]
pub enum RuntimeSendError {
    Panicked,
    ChannelClosed,
}

pub struct PluginRuntimeV3 {
    tx: Sender<RuntimeEvent>,
    panic_flag: Arc<AtomicBool>,
    panic_msg: Arc<Mutex<Option<String>>>,
}

impl PluginRuntimeV3 {
    pub fn start<P: PluginImplV3Async>(mut plugin: P, ctx: PluginContext) -> Self {
        let (tx, rx) = channel::<RuntimeEvent>();
        let panic_flag = Arc::new(AtomicBool::new(false));
        let panic_msg = Arc::new(Mutex::new(None));
        let thread_panic_flag = Arc::clone(&panic_flag);
        let thread_panic_msg = Arc::clone(&panic_msg);

        thread::spawn(move || {
            for event in rx {
                let is_shutdown = matches!(event, RuntimeEvent::Shutdown);
                let call_result =
                    std::panic::catch_unwind(std::panic::AssertUnwindSafe(|| match event {
                        RuntimeEvent::Loaded => {
                            runtime().block_on(plugin.on_loaded(ctx.clone()));
                        }
                        RuntimeEvent::Message(msg) => {
                            runtime().block_on(plugin.on_message(ctx.clone(), &msg));
                        }
                        RuntimeEvent::Shutdown => {
                            runtime().block_on(plugin.on_shutdown(ctx.clone()));
                        }
                    }));

                if let Err(panic_payload) = call_result {
                    thread_panic_flag.store(true, Ordering::Release);
                    let panic_message = if let Some(s) = panic_payload.downcast_ref::<&str>() {
                        s.to_string()
                    } else if let Some(s) = panic_payload.downcast_ref::<String>() {
                        s.clone()
                    } else {
                        "unknown panic payload".to_string()
                    };
                    if let Ok(mut panic_slot) = thread_panic_msg.lock() {
                        *panic_slot = Some(panic_message);
                    }
                    break;
                }

                if is_shutdown {
                    break;
                }
            }
        });

        Self {
            tx,
            panic_flag,
            panic_msg,
        }
    }

    pub fn send(&self, ev: RuntimeEvent) -> Result<(), RuntimeSendError> {
        if self.has_panicked() {
            return Err(RuntimeSendError::Panicked);
        }

        self.tx
            .send(ev)
            .map_err(|_| RuntimeSendError::ChannelClosed)
    }

    pub fn has_panicked(&self) -> bool {
        self.panic_flag.load(Ordering::Acquire)
    }

    pub fn panic_summary(&self) -> Option<String> {
        self.panic_msg.lock().ok().and_then(|slot| slot.clone())
    }
}
