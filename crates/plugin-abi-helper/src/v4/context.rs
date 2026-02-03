// crates/plugin-abi-helper/src/context.rs

use crate::abi::v4::HostRuntimeV4;
use std::sync::{Arc, OnceLock};

/// プラグイン実装者向けの安全なコンテキスト
#[derive(Clone)]
pub struct PluginContext {
    inner: Arc<PluginContextInner>,
}

struct PluginContextInner {
    host: OnceLock<*const HostRuntimeV4>,
    runtime: tokio::runtime::Runtime,
}

impl PluginContext {
   pub(crate) fn new() -> Self {
        let runtime = tokio::runtime::Builder::new_multi_thread()
            .enable_all()
            .build()
            .expect("failed to create runtime");

        Self {
            inner: Arc::new(PluginContextInner {
                host: OnceLock::new(),
                runtime,
            }),
        }
    }

    pub(crate) fn attach_host(&self, host: *const HostRuntimeV4) {
        let _ = self.inner.host.set(host);
    }
    pub fn host(&self) -> Host {
        let raw = *self.inner.host.get().expect("host not initialized");
        unsafe { Host::from_raw(raw) }
    }

    pub fn spawn<F>(&self, fut: F)
    where
        F: std::future::Future<Output = ()> + Send + 'static,
    {
        self.inner.runtime.spawn(fut);
    }
    /// Core へメッセージ送信（非同期）
    pub async fn send_message(&self, bytes: &[u8]) {
        // 非同期だが、ABI 自体は同期
        // helper runtime 上で await される
        unsafe {
            (self.inner.host.raw.send_message)(self.inner.host.raw, bytes.as_ptr(), bytes.len());
        }
    }

    /// 同期送信（軽量用途）
    pub fn send_message_sync(&self, bytes: &[u8]) {
        unsafe {
            (self.inner.host.raw.send_message)(self.inner.host.raw, bytes.as_ptr(), bytes.len());
        }
    }

    /// Core が割り当てた plugin_id
    pub fn plugin_id(&self) -> u64 {
        unsafe { (*self.host.raw).plugin_id }
    }
}

/// 内部用 Host ラッパ
struct Host {
    raw: *const HostRuntimev4,
}
