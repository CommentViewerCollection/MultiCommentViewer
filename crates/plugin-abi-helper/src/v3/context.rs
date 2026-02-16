// crates/plugin-abi-helper/src/context.rs

use crate::abi::v3::HostRuntimeV3;
use crate::v3::host::Host;
use mcv_messages::Message as McvMessage;
use std::collections::HashMap;
use std::sync::{Arc, Mutex, OnceLock};
use std::time::Duration;
use thiserror::Error;
use tokio::sync::oneshot;
use uuid::Uuid;

/// プラグイン実装者向けの安全なコンテキスト
#[derive(Clone)]
pub struct PluginContext {
    inner: Arc<PluginContextInner>,
}

struct PluginContextInner {
    host: OnceLock<*const HostRuntimeV3>,
    plugin_id: OnceLock<[u8; 16]>,
    runtime: tokio::runtime::Runtime,
    // on_message本体を待たずにレスポンスを解決するため、request_idで待機チャネルを管理する。
    pending_requests: Mutex<HashMap<Uuid, oneshot::Sender<McvMessage>>>,
}

#[derive(Debug, Error)]
pub enum RequestError {
    #[error("failed to serialize request: {0}")]
    Serialize(#[from] serde_json::Error),
    #[error("request timed out (request_id={0})")]
    Timeout(Uuid),
    #[error("response channel closed (request_id={0})")]
    ResponseChannelClosed(Uuid),
    #[error("internal error: {0}")]
    Internal(String),
    #[error("request message must have request_id")]
    InvalidRequestMessage,
}

#[derive(Debug, Error)]
pub enum SendError {
    #[error("failed to serialize message: {0}")]
    Serialize(#[from] serde_json::Error),
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
                pending_requests: Mutex::new(HashMap::new()),
            }),
        }
    }

    pub(crate) fn attach_host(&self, host: *const HostRuntimeV3, plugin_id: [u8; 16]) {
        let _ = self.inner.host.set(host);
        let _ = self.inner.plugin_id.set(plugin_id);
    }

    pub fn host(&self) -> Host {
        let raw = *self.inner.host.get().expect("host not initialized");
        let plugin_id = *self
            .inner
            .plugin_id
            .get()
            .expect("plugin_id not initialized");
        unsafe { Host::from_raw(raw, plugin_id) }
    }

    pub fn spawn<F>(&self, fut: F)
    where
        F: std::future::Future<Output = ()> + Send + 'static,
    {
        self.inner.runtime.spawn(fut);
    }
    /// Host へバイト列を送信（内部用）
    pub(crate) async fn send_message(&self, bytes: &[u8]) {
        // hostを取得してから呼び出し
        let host = self.host();
        host.send_message(bytes);
    }

    /// 同期送信（軽量用途）
    pub fn send_message_sync(&self, bytes: &[u8]) {
        let host = self.host();
        host.send_message(bytes);
    }

    /// fire-and-forget通知を送信する
    pub async fn send_notification(&self, message: McvMessage) -> Result<(), SendError> {
        let bytes = serde_json::to_vec(&message)?;
        self.send_message(&bytes).await;
        Ok(())
    }

    pub async fn send_request(
        &self,
        mut message: McvMessage,
        timeout: Duration,
    ) -> Result<McvMessage, RequestError> {
        let request_id = match message.request_id {
            Some(id) => id,
            None => return Err(RequestError::InvalidRequestMessage),
        };
        message.request_id = Some(request_id);

        let (tx, rx) = oneshot::channel::<McvMessage>();
        {
            let mut pending = self
                .inner
                .pending_requests
                .lock()
                .map_err(|e| RequestError::Internal(format!("pending lock poisoned: {}", e)))?;
            pending.insert(request_id, tx);
        }

        let bytes = serde_json::to_vec(&message)?;
        self.send_message(&bytes).await;

        match tokio::time::timeout(timeout, rx).await {
            Ok(Ok(response)) => Ok(response),
            Ok(Err(_)) => {
                // リーク防止: 成否に関係なくpendingから削除する。
                if let Ok(mut pending) = self.inner.pending_requests.lock() {
                    pending.remove(&request_id);
                }
                Err(RequestError::ResponseChannelClosed(request_id))
            }
            Err(_) => {
                // リーク防止: 成否に関係なくpendingから削除する。
                if let Ok(mut pending) = self.inner.pending_requests.lock() {
                    pending.remove(&request_id);
                }
                Err(RequestError::Timeout(request_id))
            }
        }
    }

    pub(crate) fn try_resolve_pending_response(&self, message: &McvMessage) -> bool {
        let request_id = match message.request_id {
            Some(id) => id,
            None => return false,
        };

        let sender = match self.inner.pending_requests.lock() {
            Ok(mut pending) => pending.remove(&request_id),
            Err(_) => None,
        };

        match sender {
            Some(tx) => {
                let _ = tx.send(message.clone());
                true
            }
            None => false,
        }
    }
}
