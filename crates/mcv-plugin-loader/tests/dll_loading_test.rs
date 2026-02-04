use mcv_plugin_loader::PluginLoader;
use std::path::PathBuf;

#[test]
fn test_load_plugin_dummy_dll() {
    // DLLのパスを構築
    let manifest_dir = env!("CARGO_MANIFEST_DIR");
    let dll_path = PathBuf::from(manifest_dir)
        .parent()
        .unwrap()
        .parent()
        .unwrap()
        .join("target")
        .join("debug")
        .join("plugin_dummy.dll");

    println!("DLL path: {:?}", dll_path);

    // DLLをロード
    let _loader = PluginLoader::load(&dll_path).expect("Failed to load plugin DLL");
}

#[test]
fn test_plugin_init() {
    let manifest_dir = env!("CARGO_MANIFEST_DIR");
    let dll_path = PathBuf::from(manifest_dir)
        .parent()
        .unwrap()
        .parent()
        .unwrap()
        .join("target")
        .join("debug")
        .join("plugin_dummy.dll");

    let loader = PluginLoader::load(&dll_path).expect("Failed to load plugin DLL");

    // プラグイン初期化
    let result = loader.init(std::ptr::null_mut());
    assert!(result.is_ok(), "Plugin initialization failed");

    println!("Plugin initialized successfully");

    // シャットダウン
    let result = loader.shutdown();
    assert!(result.is_ok(), "Plugin shutdown failed");

    println!("Plugin shutdown successfully");
}
