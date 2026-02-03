
#[macro_export]
macro_rules! export_plugin_v3 {
    ($impl:ty) => {
        #[unsafe(no_mangle)]
        pub extern "C" fn create_plugin_v3() -> *mut PluginV3 {
            PluginFactoryV3::new::<$impl>()
        }
    };
}