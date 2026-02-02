use std::ffi::{CStr, CString, c_void};
use std::os::raw::c_char;
use std::sync::atomic::{AtomicUsize, Ordering};
use std::sync::Arc;

use async_trait::async_trait;
use once_cell::sync::OnceCell;
use tokio::runtime::Runtime;

use mcv_messages::Message as McvMessage;
use mcv_plugin_interface::{Plugin, PluginError, PluginHost};

/// ===== グローバル（static mut 禁止対応） =====

static PLUGIN_INSTANCE: OnceCell<
    Arc<tokio::sync::Mutex<Box<dyn Plugin + Send>>>
> = OnceCell::new();

static MESSAGE_CALLBACK: OnceCell<
    extern "C" fn(*const c_char, *mut c_void)
> = OnceCell::new();

static USERDATA: AtomicUsize = AtomicUsize::new(0);

static RUNTIME: OnceCell<Runtime> = OnceCell::new();

/// ===== PluginHost 実装 =====

pub struct CApiPluginHost;

#[async_trait]
impl PluginHost for CApiPluginHost {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        let json = serde_json::to_string(&message)
            .map_err(|e| PluginError::MessageHandlingFailed(e.to_string()))?;

        let cstr = CString::new(json).unwrap();

        if let Some(cb) = MESSAGE_CALLBACK.get() {
            let userdata = USERDATA.load(Ordering::SeqCst) as *mut c_void;
            cb(cstr.as_ptr(), userdata);
        }

        Ok(())
    }
}

/// ===== ABI 公開関数 =====

pub fn init_plugin(plugin: Box<dyn Plugin + Send>) -> i32 {
    let runtime = match Runtime::new() {
        Ok(rt) => rt,
        Err(_) => return -1,
    };

    if RUNTIME.set(runtime).is_err() {
        return -1;
    }

    let plugin = Arc::new(tokio::sync::Mutex::new(plugin));

    if PLUGIN_INSTANCE.set(plugin).is_err() {
        return -1;
    }

    0
}

pub fn call_on_loaded() -> i32 {
    let plugin = match PLUGIN_INSTANCE.get() {
        Some(p) => p,
        None => return -1,
    };

    let rt = match RUNTIME.get() {
        Some(rt) => rt,
        None => return -1,
    };

    let host = Arc::new(CApiPluginHost);

    rt.block_on(async {
        let mut p = plugin.lock().await;
        p.on_loaded(host).await
    })
    .map(|_| 0)
    .unwrap_or(-1)
}

pub fn call_on_message(json: *const c_char) -> i32 {
    if json.is_null() {
        return -1;
    }

    let cstr = unsafe { CStr::from_ptr(json) };

    let msg: McvMessage = match serde_json::from_str(cstr.to_str().unwrap()) {
        Ok(m) => m,
        Err(_) => return -1,
    };

    let plugin = match PLUGIN_INSTANCE.get() {
        Some(p) => p,
        None => return -1,
    };

    let rt = match RUNTIME.get() {
        Some(rt) => rt,
        None => return -1,
    };

    let host = Arc::new(CApiPluginHost);

    rt.block_on(async {
        let mut p = plugin.lock().await;
        p.on_message(msg, host).await
    })
    .map(|_| 0)
    .unwrap_or(-1)
}

pub fn set_callback(
    cb: extern "C" fn(*const c_char, *mut c_void),
    userdata: *mut c_void,
) {
    let _ = MESSAGE_CALLBACK.set(cb);
    USERDATA.store(userdata as usize, Ordering::SeqCst);
}

pub fn shutdown() {
    // 明示的 drop が必要ならここで
}