use actix::prelude::*;
use mcv_core::{CoreActor, PluginManager};
use std::path::PathBuf;
use std::time::Duration;

#[actix::test]
async fn test_register_plugin_from_dll() {
    // DLLパスを構築
    let dll_path = PathBuf::from(env!("CARGO_MANIFEST_DIR"))
        .parent()
        .unwrap()
        .parent()
        .unwrap()
        .join("target")
        .join("debug")
        .join("plugin_dummy.dll");

    println!("DLL path: {:?}", dll_path);

    // DLLが存在することを確認
    assert!(
        dll_path.exists(),
        "plugin_dummy.dll not found at {:?}. Please run 'cargo build -p plugin-dummy' first.",
        dll_path
    );

    // Core Actorを起動
    let core_actor = CoreActor::new();
    let core_addr = core_actor.start();

    // Plugin Managerを作成
    let mut plugin_manager = PluginManager::new();
    plugin_manager.set_core_addr(core_addr.clone());

    // DLLからプラグインを登録
    let result = plugin_manager.register_plugin_from_dll(&dll_path).await;

    assert!(
        result.is_ok(),
        "Failed to register plugin from DLL: {:?}",
        result.err()
    );

    let (plugin_id, _plugin_host_addr) = result.unwrap();
    println!("Plugin registered successfully with ID: {}", plugin_id);

    // 少し待ってプラグインが初期化されるのを待つ
    tokio::time::sleep(Duration::from_millis(500)).await;

    println!("Test completed successfully");
}
