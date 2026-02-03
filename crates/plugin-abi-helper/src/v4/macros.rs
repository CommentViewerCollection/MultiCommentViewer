//! v4用のエクスポートマクロ

/// 非同期プラグインをエクスポートするマクロ
///
/// # Example
///
/// ```ignore
/// use plugin_abi_helper::v4::prelude::*;
///
/// #[derive(Default)]
/// struct MyPlugin;
///
/// #[async_trait]
/// impl PluginImplV4Async for MyPlugin {
///     async fn on_loaded(&mut self, ctx: PluginContext) {
///         ctx.host().log(2, "Plugin loaded!");
///     }
///     async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
///         // handle message
///     }
///     async fn on_shutdown(&mut self, ctx: PluginContext) {
///         // cleanup
///     }
/// }
///
/// export_plugin_v4_async!(MyPlugin);
/// ```
#[macro_export]
macro_rules! export_plugin_v4_async {
    ($impl:ty) => {
        #[unsafe(no_mangle)]
        pub extern "C" fn create_plugin_v4() -> *mut $crate::abi::v4::PluginV4 {
            $crate::v4::factory::PluginFactoryV4::new::<$impl>()
        }
    };
}

/// v4マクロを再エクスポート
pub use crate::export_plugin_v4_async;
