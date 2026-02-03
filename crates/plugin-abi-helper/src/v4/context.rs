// crates/plugin-abi-helper/src/context.rs

use crate::abi::v4::HostRuntimeV4;
use crate::v4::host::Host;
use std::sync::{Arc, OnceLock};

/// プラグイン実装者向けの安全なコンテキスト
#[derive(Clone)]
pub struct PluginContext {
    inner: Arc<PluginContextInner>,
}

struct PluginContextInner {
    host: OnceLock<*const HostRuntimeV4>,
    plugin_id: OnceLock<u64>,
    runtime: tokio::runtime::Runtime,
}

// PluginContextInnerはスレッド間で安全に共有できる
// host ポインタは一度設定されたら変更されず、複数スレッドからの読み取りのみ
unsafe impl Send for PluginContextInner {}
unsafe impl Sync for PluginContextInner {}

impl PluginContext {
   pub(crate) fn new() -> Self {
        let runtime = tokio::runtime::Builder::new_multi_thread()
            .enable_all()
            .build()
            .expect("failed to create runtime");

        Self {
            inner: Arc::new(PluginContextInner {
                host: OnceLock::new(),
                plugin_id: OnceLock::new(),
                runtime,
            }),
        }
    }

    pub(crate) fn attach_host(&self, host: *const HostRuntimeV4, plugin_id: u64) {
        let _ = self.inner.host.set(host);
        let _ = self.inner.plugin_id.set(plugin_id);
    }

    pub fn host(&self) -> Host {
        let raw = *self.inner.host.get().expect("host not initialized");
        let plugin_id = *self.inner.plugin_id.get().expect("plugin_id not initialized");
        unsafe { Host::from_raw(raw, plugin_id) }
    }

    pub fn spawn<F>(&self, fut: F)
    where
        F: std::future::Future<Output = ()> + Send + 'static,
    {
        self.inner.runtime.spawn(fut);
    }
    /// Core へメッセージ送信（非同期）
    pub async fn send_message(&self, bytes: &[u8]) {
        // hostを取得してから呼び出し
        let host = self.host();
        host.send_message(bytes);
    }

    /// 同期送信（軽量用途）
    pub fn send_message_sync(&self, bytes: &[u8]) {
        let host = self.host();
        host.send_message(bytes);
    }

    /// Core が割り当てた plugin_id
    pub fn plugin_id(&self) -> u64 {
        self.host().plugin_id()
    }
}
