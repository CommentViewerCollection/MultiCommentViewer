
#[macro_export]
macro_rules! export_plugin_v4 {
    ($impl:ty) => {
        #[unsafe(no_mangle)]
        pub extern "C" fn create_plugin_v4() -> *mut Pluginv4 {
            PluginFactoryv4::new::<$impl>()
        }
    };
}