use crate::abi::v3::HostRuntimeV3;

pub struct Host {
    raw: *const HostRuntimeV3,
    plugin_id: [u8; 16],
}

impl Host {
    /// 生ポインタからHostを構築（unsafeを隠蔽）
    pub(crate) unsafe fn from_raw(raw: *const HostRuntimeV3, plugin_id: [u8; 16]) -> Self {
        Self { raw, plugin_id }
    }

    /// plugin_idを取得（バイト配列）
    pub fn plugin_id(&self) -> [u8; 16] {
        self.plugin_id
    }

    /// ログ出力
    pub fn log(&self, level: i32, msg: &str) {
        unsafe {
            ((*self.raw).log)(level, msg.as_ptr(), msg.len());
        }
    }

    /// メッセージ送信（同期）
    pub fn send_message(&self, json: &[u8]) {
        unsafe {
            ((*self.raw).send_message)(json.as_ptr(), json.len());
        }
    }
}

// 必須：Sendを実装（生ポインタだがスレッド間移動を許可）
unsafe impl Send for Host {}