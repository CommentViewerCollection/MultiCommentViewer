//! サーバー時刻キャッシュ

use std::sync::atomic::{AtomicBool, AtomicI64, Ordering};

/// serverTime メッセージで受け取ったサーバー時刻を保持し、
/// 経過時間を加算して現在のサーバー推定時刻を返す。
pub struct ServerTimeCache {
    server_secs_at_recv: AtomicI64,
    local_secs_at_recv: AtomicI64,
    received: AtomicBool,
}

impl ServerTimeCache {
    pub fn new() -> Self {
        Self {
            server_secs_at_recv: AtomicI64::new(0),
            local_secs_at_recv: AtomicI64::new(0),
            received: AtomicBool::new(false),
        }
    }

    /// serverTime を受信したときに呼ぶ。
    pub fn update(&self, server_secs: i64) {
        let local_secs = chrono::Utc::now().timestamp();
        self.server_secs_at_recv
            .store(server_secs, Ordering::Relaxed);
        self.local_secs_at_recv.store(local_secs, Ordering::Relaxed);
        self.received.store(true, Ordering::Relaxed);
    }

    /// 現在のサーバー推定時刻 (Unix秒) を返す。
    /// 未受信の場合はローカル時刻をそのまま返す。
    pub fn now_secs(&self) -> i64 {
        if !self.received.load(Ordering::Relaxed) {
            return chrono::Utc::now().timestamp();
        }
        let server = self.server_secs_at_recv.load(Ordering::Relaxed);
        let local = self.local_secs_at_recv.load(Ordering::Relaxed);
        server + (chrono::Utc::now().timestamp() - local)
    }
}

impl Default for ServerTimeCache {
    fn default() -> Self {
        Self::new()
    }
}
