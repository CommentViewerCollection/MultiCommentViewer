use proc_macro::TokenStream;
use quote::quote;
use syn::{parse_macro_input, Type};

/// v4非同期プラグインをエクスポートするproc-macro
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
///     // ...
/// }
///
/// export_plugin_v4_async!(MyPlugin);
/// ```
#[proc_macro]
pub fn export_plugin_v4_async(input: TokenStream) -> TokenStream {
    let impl_ty = parse_macro_input!(input as Type);

    let expanded = quote! {
        #[unsafe(no_mangle)]
        pub extern "C" fn create_plugin_v4()
            -> *mut ::plugin_abi_helper::abi::v4::PluginV4
        {
            ::plugin_abi_helper::v4::factory::PluginFactoryV4::new::<#impl_ty>()
        }
    };

    expanded.into()
}