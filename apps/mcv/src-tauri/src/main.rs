#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use actix::prelude::*;
use mcv_core::*;
use mcv_messages::{self, Message as McvMessage, MessageSource, MessageDestination, MessageType, *};
use plugin_dummy::DummyPlugin;
use std::sync::Arc;
use tauri::{AppHandle, Emitter};
use uuid::Uuid;

/// アプリケーションの状態
struct AppState {
    core_addr: Addr<CoreActor>,
    plugin_manager: Arc<tokio::sync::Mutex<PluginManager>>,
    dummy_plugin_id: Uuid,
}

/// 接続を開始（簡易版: add_connection + connect）
#[tauri::command]
async fn start_connection(
    state: tauri::State<'_, AppState>,
) -> Result<String, String> {
    println!("=== start_connection called ===");
    let plugin_id = state.dummy_plugin_id;
    let connection_id = Uuid::new_v4();

    println!("Sending add-connection message, plugin_id: {}", plugin_id);
    // add-connectionメッセージを送信
    let add_message = McvMessage::new(
        MessageType::AddConnection,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(AddConnectionPayload {
            site: SiteInfo {
                name: "Dummy".to_string(),
                id: plugin_id,
            },
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendMessageToCore { message: add_message })
        .await
        .map_err(|e| e.to_string())?;

    // 少し待つ
    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    println!("Sending connect message, connection_id: {}", connection_id);
    // connectメッセージを送信
    let connect_message = McvMessage::new(
        MessageType::Connect,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(ConnectPayload {
            connection_id,
            site: SiteInfo {
                name: "Dummy".to_string(),
                id: plugin_id,
            },
            input: InputInfo {
                input_type: "dummy".to_string(),
                extra: serde_json::json!({}),
            },
            browser: BrowserInfo {
                name: "None".to_string(),
                id: Uuid::nil(),
            },
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendMessageToCore { message: connect_message })
        .await
        .map_err(|e| e.to_string())?;

    println!("start_connection completed, returning connection_id: {}", connection_id);
    Ok(connection_id.to_string())
}

/// 接続を追加
#[tauri::command]
async fn add_connection(
    state: tauri::State<'_, AppState>,
) -> Result<String, String> {
    let plugin_id = state.dummy_plugin_id;

    // add-connectionメッセージを送信
    let message = McvMessage::new(
        MessageType::AddConnection,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(AddConnectionPayload {
            site: SiteInfo {
                name: "Dummy".to_string(),
                id: plugin_id,
            },
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendMessageToCore { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok("Connection add request sent".to_string())
}

/// 接続を開始
#[tauri::command]
async fn connect(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;
    let plugin_id = state.dummy_plugin_id;

    // connectメッセージを送信
    let message = McvMessage::new(
        MessageType::Connect,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(ConnectPayload {
            connection_id: conn_id,
            site: SiteInfo {
                name: "Dummy".to_string(),
                id: plugin_id,
            },
            input: InputInfo {
                input_type: "dummy".to_string(),
                extra: serde_json::json!({}),
            },
            browser: BrowserInfo {
                name: "None".to_string(),
                id: Uuid::nil(),
            },
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendMessageToCore { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// 切断
#[tauri::command]
async fn disconnect(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    // disconnectメッセージを送信
    let message = McvMessage::new(
        MessageType::Disconnect,
        MessageSource::Core,
        MessageDestination::Core,
        serde_json::to_value(DisconnectPayload {
            connection_id: conn_id,
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendMessageToCore { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// 接続一覧を取得
#[tauri::command]
async fn get_connections(
    state: tauri::State<'_, AppState>,
) -> Result<Vec<ConnectionInfo>, String> {
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    Ok(connections)
}

fn main() {
    // actixのシステムをセットアップするためのチャネル
    let (tx, rx) = std::sync::mpsc::channel();

    // Actixシステムを別スレッドで実行
    std::thread::spawn(move || {
        let actix_system = System::new();

        actix_system.block_on(async {
            println!("Actix system thread started");

            // Core Actorを起動
            let mut core_actor = CoreActor::new();

            // Plugin Managerを作成
            let mut plugin_manager = PluginManager::new();

            // AppHandleを保持するためのプレースホルダー
            let app_handle: Arc<tokio::sync::Mutex<Option<AppHandle>>> =
                Arc::new(tokio::sync::Mutex::new(None));

            // イベントコールバックを設定
            let app_handle_clone = app_handle.clone();
            let event_callback = Arc::new(move |message: McvMessage| {
                let app_handle_clone2 = app_handle_clone.clone();
                actix::spawn(async move {
                    if let Some(app_handle) = app_handle_clone2.lock().await.as_ref() {
                        match message.message_type {
                            MessageType::CommentReceived => {
                                let payload: CommentReceivedPayload =
                                    serde_json::from_value(message.payload).unwrap();
                                println!("Emitting comment-received event: {:?}", payload.comment);
                                // connection_idを含めたコメントオブジェクトを作成
                                let mut comment_with_conn = serde_json::to_value(&payload.comment).unwrap();
                                if let Some(obj) = comment_with_conn.as_object_mut() {
                                    obj.insert("connection_id".to_string(), serde_json::Value::String(payload.connection_id.to_string()));
                                }
                                if let Err(e) = app_handle.emit("comment-received", comment_with_conn) {
                                    eprintln!("Failed to emit comment-received event: {}", e);
                                }
                            }
                            MessageType::Connected => {
                                println!("Emitting connected event");
                                if let Err(e) = app_handle.emit("connected", message.payload) {
                                    eprintln!("Failed to emit connected event: {}", e);
                                }
                            }
                            MessageType::Disconnected => {
                                println!("Emitting disconnected event");
                                if let Err(e) = app_handle.emit("disconnected", message.payload) {
                                    eprintln!("Failed to emit disconnected event: {}", e);
                                }
                            }
                            _ => {}
                        }
                    }
                });
            });

            core_actor.set_event_callback(event_callback);

            println!("Starting CoreActor...");
            let core_addr = core_actor.start();
            println!("CoreActor started");

            println!("Setting core_addr to PluginManager...");
            plugin_manager.set_core_addr(core_addr.clone());
            println!("core_addr set to PluginManager");

            // ダミープラグインを登録
            println!("Creating DummyPlugin...");
            let dummy_plugin = Box::new(DummyPlugin::new());
            println!("Calling plugin_manager.register_plugin...");
            let (plugin_id, plugin_host_addr) = plugin_manager
                .register_plugin(dummy_plugin)
                .await
                .expect("Failed to register dummy plugin");
            println!("plugin_manager.register_plugin returned successfully");

            // Core ActorにPluginInfoを登録
            let plugin_info = PluginInfo {
                name: "Dummy Plugin".to_string(),
                plugin_id,
                role: vec!["dummy".to_string()],
                api_version: "v2".to_string(),
                host_addr: plugin_host_addr,
            };

            println!("Sending RegisterPlugin to CoreActor...");
            core_addr.do_send(mcv_core::core_actor::RegisterPlugin {
                plugin_id,
                plugin_info,
            });

            println!("Dummy plugin registered: {}", plugin_id);

            // AppStateを作成してメインスレッドに送信
            let app_state = AppState {
                core_addr,
                plugin_manager: Arc::new(tokio::sync::Mutex::new(plugin_manager)),
                dummy_plugin_id: plugin_id,
            };

            println!("Sending AppState to main thread...");
            tx.send((app_state, app_handle)).expect("Failed to send AppState");

            println!("Actix system setup complete, keeping system alive...");
        });

        // actixシステムを実行し続ける
        actix_system.run().expect("Actix system failed");
    });

    // メインスレッドでAppStateを受信
    println!("Waiting for AppState from actix thread...");
    let (app_state, app_handle) = rx.recv().expect("Failed to receive AppState");
    println!("Received AppState, starting Tauri...");

    // Tauriアプリを起動
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup(move |app| {
            // AppHandleを保存（ブロッキング操作）
            let handle = app.handle().clone();
            let app_handle_clone = app_handle.clone();
            std::thread::spawn(move || {
                let rt = tokio::runtime::Runtime::new().unwrap();
                rt.block_on(async move {
                    *app_handle_clone.lock().await = Some(handle);
                    println!("AppHandle set successfully");
                });
            });
            Ok(())
        })
        .manage(app_state)
        .invoke_handler(tauri::generate_handler![
            start_connection,
            add_connection,
            connect,
            disconnect,
            get_connections
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
