//! # plugin-abi-helper v4
//!
//! async/awaitを完全サポートする最新のプラグインABI。
//!
//! ## アーキテクチャ
//!
//! ```text
//! C ABI (extern "C" fn)
//!     ↓ trampoline
//! イベント駆動ランタイム (専用スレッド)
//!     ↓ tokio runtime
//! async Rust (PluginImplV4Async)
//! ```
//!
//! ## 使用例
//!
//! ```rust,ignore
//! use plugin_abi_helper::v4::prelude::*;
//!
//! #[derive(Default)]
//! struct MyPlugin;
//!
//! #[async_trait]
//! impl PluginImplV4Async for MyPlugin {
//!     async fn on_loaded(&mut self, ctx: PluginContext) {
//!         ctx.host().log(2, "Plugin loaded!");
//!     }
//!     async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
//!         // handle message
//!     }
//!     async fn on_shutdown(&mut self, ctx: PluginContext) {
//!         // cleanup
//!     }
//! }
//!
//! export_plugin_v4_async!(MyPlugin);
//! ```

use crate::abi::v4::{PLUGIN_ABI_VERSION, PluginV4};

pub mod context;
pub mod factory;
pub mod host;
pub mod macros;
pub mod plugin;
pub mod plugin_async;
pub mod plugin_runtime;
pub mod runtime;
pub mod runtime_event;
pub mod trampoline;

/// v4の主要な型を一括インポートするためのprelude
pub mod prelude {
    pub use crate::abi::v4::PluginV4;
    pub use export_plugin_v3_macros::export_plugin_v4_async;
    pub use crate::v4::context::PluginContext;
    pub use crate::v4::host::Host;
    pub use crate::v4::plugin_async::PluginImplV4Async;
    pub use async_trait::async_trait;    
}

pub fn validate_plugin_v4(p: *const PluginV4) -> Result<(), AbiError> {
    if p.is_null() {
        return Err(AbiError::NullPlugin);
    }

    let plugin = unsafe { &*p };

    if plugin.abi_version != PLUGIN_ABI_VERSION {
        return Err(AbiError::VersionMismatch {
            expected: PLUGIN_ABI_VERSION,
            found: plugin.abi_version,
        });
    }

    Ok(())
}

pub enum AbiError {
    NullPlugin,
    VersionMismatch { expected: u32, found: u32 },
}
