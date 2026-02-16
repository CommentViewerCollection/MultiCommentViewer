use proc_macro::TokenStream;
use quote::quote;
use syn::{Type, parse_macro_input};

/// v3非同期プラグインをエクスポートするproc-macro
///
/// # Example
///
/// ```ignore
/// use plugin_abi_helper::v3::prelude::*;
///
/// #[derive(Default)]
/// struct MyPlugin;
///
/// #[async_trait]
/// impl PluginImplV3Async for MyPlugin {
///     // ...
/// }
///
/// export_plugin_v3_async!(MyPlugin);
/// ```
#[proc_macro]
pub fn export_plugin_v3_async(input: TokenStream) -> TokenStream {
    let impl_ty = parse_macro_input!(input as Type);

    let expanded = quote! {
        #[unsafe(no_mangle)]
        pub extern "C" fn create_plugin_v3()
            -> *mut ::plugin_abi_helper::abi::v3::PluginV3
        {
            ::plugin_abi_helper::v3::factory::PluginFactoryV3::new::<#impl_ty>()
        }
    };

    expanded.into()
}
