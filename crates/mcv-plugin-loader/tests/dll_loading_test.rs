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
    let loader = PluginLoader::load(&dll_path).expect("Failed to load plugin DLL");

    // メタデータを確認
    let metadata = loader.metadata();
    assert_eq!(metadata.id, "plugin-dummy");
    assert_eq!(metadata.name, "Dummy Plugin");
    assert_eq!(metadata.version, "0.1.0");
    assert_eq!(metadata.api_version, "v2");
    assert_eq!(metadata.roles, vec!["dummy"]);

    println!("Plugin metadata: {:?}", metadata);
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
