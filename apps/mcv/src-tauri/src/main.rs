#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

use actix::prelude::*;
use mcv_core::{
    BrowserInfo as CoreBrowserInfo, // mcv-coreから明示的にインポート
    ConnectionInfo,
    CoreActor,
    CreateConnection,
    GetBrowsers,
    GetConnections,
    GetSites,
    PluginManager,
    RemoveConnection,
    RenameConnection,
    SendRequest,
    SetConnectionSite,
    SiteInfo as CoreSiteInfo,
    UpdateConnectionSettings,
};
use mcv_messages::{
    self,
    BrowserInfo as MsgBrowserInfo,
    CommentReceivedPayload,
    ConnectPayload,
    DisconnectPayload,
    InputInfo, // mcv-messagesから明示的にインポート
    Message as McvMessage,
    MessageDestination,
    MessageSource,
    MessageType,
    SendCommentPayload,
    SiteInfo as MsgSiteInfo,
};
use mcv_updater::{McvUpdateInfo, UpdateChecker};
use std::path::PathBuf;
use std::sync::Arc;
use tauri::{AppHandle, Emitter, Manager, State};
use uuid::Uuid;

/// アプリケーションの状態
struct AppState {
    core_addr: Addr<CoreActor>,
    plugin_manager: Arc<tokio::sync::Mutex<PluginManager>>,
}

/// 接続を追加
#[tauri::command]
async fn add_connection(state: State<'_, AppState>) -> Result<String, String> {
    tracing::debug!("add_connection called");

    // 現在の接続を取得してデフォルト名を生成
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    // 既存の接続名から#N形式の番号を抽出
    let mut used_numbers = std::collections::HashSet::new();
    for conn in &connections {
        if let Some(stripped) = conn.name.strip_prefix('#') {
            if let Ok(num) = stripped.parse::<u32>() {
                used_numbers.insert(num);
            }
        }
    }

    // #1から順に空いている番号を探す
    let mut next_number = 1;
    while used_numbers.contains(&next_number) {
        next_number += 1;
    }

    let default_name = format!("#{}", next_number);
    tracing::debug!(name = %default_name, "Generated default connection name");

    // 接続を作成（plugin_idはNone、サイト未選択状態）
    let connection_id = state
        .core_addr
        .send(CreateConnection {
            plugin_id: None, // 変更: サイト未選択状態で作成
            site_name: "未選択".to_string(),
            input_info: "{}".to_string(),
            name: default_name,
        })
        .await
        .map_err(|e| e.to_string())?;

    tracing::info!(connection_id = %connection_id, "Connection created");
    Ok(connection_id.to_string())
}

/// 接続を削除
#[tauri::command]
async fn remove_connection(
    state: tauri::State<'_, AppState>,
    connection_id: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    state
        .core_addr
        .send(RemoveConnection {
            connection_id: conn_id,
        })
        .await
        .map_err(|e| e.to_string())?
}

/// 接続名を変更
#[tauri::command]
async fn rename_connection(
    state: State<'_, AppState>,
    connection_id: String,
    new_name: String,
) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    state
        .core_addr
        .send(RenameConnection {
            connection_id: conn_id,
            new_name,
        })
        .await
        .map_err(|e| e.to_string())?
}

/// 接続を開始
#[tauri::command]
async fn connect(state: tauri::State<'_, AppState>, connection_id: String) -> Result<(), String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    // 接続情報を取得
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    let conn_info = connections
        .iter()
        .find(|c| c.connection_id == conn_id)
        .ok_or("Connection not found")?;

    // サイトが選択されていない場合はエラー
    let plugin_id = conn_info.plugin_id.ok_or("サイトが選択されていません")?;
    let site_id = conn_info.site_id.ok_or("サイトが選択されていません")?;
    let url = conn_info.url.clone().ok_or("URLが入力されていません")?;

    tracing::debug!(
        connection_id = %conn_id,
        plugin_id = %plugin_id,
        site_id = %site_id,
        "Connecting to site"
    );

    // connectメッセージを送信
    let message = McvMessage::new(
        MessageType::Connect,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(ConnectPayload {
            connection_id: conn_id,
            site: MsgSiteInfo {
                name: conn_info.site_name.clone(),
                id: site_id,
            },
            input: InputInfo {
                input_type: conn_info.site_name.clone(),
                extra: serde_json::json!({
                    "url": url,
                    "advanced_settings": conn_info.advanced_settings,
                }),
            },
            browser: MsgBrowserInfo {
                name: conn_info.browser_name.clone().unwrap_or("None".to_string()),
                id: conn_info.browser_id.unwrap_or(Uuid::nil()),
            },
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendRequest { message })
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
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok(())
}

/// 接続一覧を取得
#[tauri::command]
async fn get_connections(state: tauri::State<'_, AppState>) -> Result<Vec<ConnectionInfo>, String> {
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    Ok(connections)
}

/// サイト一覧を取得
#[tauri::command]
async fn get_sites(state: tauri::State<'_, AppState>) -> Result<Vec<CoreSiteInfo>, String> {
    let sites = state
        .core_addr
        .send(GetSites)
        .await
        .map_err(|e| e.to_string())?;

    Ok(sites)
}

/// ブラウザ一覧を取得
#[tauri::command]
async fn get_browsers(state: tauri::State<'_, AppState>) -> Result<Vec<CoreBrowserInfo>, String> {
    let browsers = state
        .core_addr
        .send(GetBrowsers)
        .await
        .map_err(|e| e.to_string())?;

    Ok(browsers)
}

/// 接続にサイトを設定
#[tauri::command]
async fn set_connection_site(
    state: tauri::State<'_, AppState>,
    connection_id: String,
    site_id: String,
) -> Result<(), String> {
    let conn_id =
        Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid connection_id: {}", e))?;
    let s_id = Uuid::parse_str(&site_id).map_err(|e| format!("Invalid site_id: {}", e))?;

    tracing::debug!(
        connection_id = %conn_id,
        site_id = %s_id,
        "Setting connection site"
    );

    state
        .core_addr
        .send(SetConnectionSite {
            connection_id: conn_id,
            site_id: s_id,
        })
        .await
        .map_err(|e| format!("Failed to set connection site: {}", e))?
        .map_err(|e| e)?;

    Ok(())
}

/// 接続設定を更新
#[tauri::command]
async fn update_connection_settings(
    state: tauri::State<'_, AppState>,
    connection_id: String,
    url: Option<String>,
    browser_id: Option<String>,
    advanced_settings: Option<serde_json::Value>,
) -> Result<(), String> {
    let conn_id =
        Uuid::parse_str(&connection_id).map_err(|e| format!("Invalid connection_id: {}", e))?;

    let b_id = if let Some(bid) = browser_id {
        Some(Uuid::parse_str(&bid).map_err(|e| format!("Invalid browser_id: {}", e))?)
    } else {
        None
    };

    tracing::debug!(
        connection_id = %conn_id,
        has_url = url.is_some(),
        has_browser = b_id.is_some(),
        has_settings = advanced_settings.is_some(),
        "Updating connection settings"
    );

    state
        .core_addr
        .send(UpdateConnectionSettings {
            connection_id: conn_id,
            url,
            browser_id: b_id,
            advanced_settings,
        })
        .await
        .map_err(|e| format!("Failed to update connection settings: {}", e))?
        .map_err(|e| e)?;

    Ok(())
}

/// コメントを送信
#[tauri::command]
async fn send_comment(
    state: State<'_, AppState>,
    connection_id: String,
    text: String,
) -> Result<String, String> {
    let conn_id = Uuid::parse_str(&connection_id).map_err(|e| e.to_string())?;

    // 接続情報を取得してplugin_idを取得
    let connections = state
        .core_addr
        .send(GetConnections)
        .await
        .map_err(|e| e.to_string())?;

    let conn_info = connections
        .iter()
        .find(|c| c.connection_id == conn_id)
        .ok_or("Connection not found")?;

    let plugin_id = conn_info
        .plugin_id
        .ok_or("Plugin not assigned to this connection")?;

    // send-commentメッセージを送信
    let message = McvMessage::new(
        MessageType::SendComment,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(SendCommentPayload {
            connection_id: conn_id,
            text,
        })
        .unwrap(),
    );

    state
        .core_addr
        .send(SendRequest { message })
        .await
        .map_err(|e| e.to_string())?;

    Ok("Comment sent".to_string())
}

/// mcv本体の更新をチェック
#[tauri::command]
async fn check_for_updates() -> Result<Option<McvUpdateInfo>, String> {
    const CURRENT_VERSION: &str = env!("CARGO_PKG_VERSION");
    const API_BASE_URL: &str = "http://localhost"; // TODO: 運用環境では実際のAPIサーバーURLに変更

    tracing::info!(current_version = CURRENT_VERSION, "Checking for updates");

    let updater = UpdateChecker::new(API_BASE_URL);

    match updater.check_mcv_update(CURRENT_VERSION).await {
        Ok(update_info) => {
            if let Some(ref info) = update_info {
                tracing::info!(
                    current_version = CURRENT_VERSION,
                    new_version = %info.version,
                    "Update available"
                );
            } else {
                tracing::info!("No update available");
            }
            Ok(update_info)
        }
        Err(e) => {
            tracing::error!(
                error = %e,
                "Failed to check for updates"
            );
            Err(format!("Failed to check for updates: {}", e))
        }
    }
}

/// インストーラを起動してmcvを終了
#[tauri::command]
async fn launch_installer(app_handle: AppHandle) -> Result<(), String> {
    tracing::info!("Launching installer");

    // インストーラのパスを構築
    let installer_path = std::env::current_exe()
        .map_err(|e| format!("Failed to get current exe path: {}", e))?
        .parent()
        .ok_or_else(|| "Failed to get parent directory".to_string())?
        .join("installer.exe");

    tracing::debug!(installer_path = ?installer_path, "Installer path");

    // インストーラが存在するか確認
    if !installer_path.exists() {
        return Err(format!(
            "Installer not found at {:?}. Please download the installer manually.",
            installer_path
        ));
    }

    // インストーラを起動（--update-mcv フラグ付き）
    std::process::Command::new(&installer_path)
        .arg("--update-mcv")
        .spawn()
        .map_err(|e| format!("Failed to launch installer: {}", e))?;

    // mcvを終了
    tracing::info!("Exiting mcv for update");
    app_handle.exit(0);

    Ok(())
}

fn main() {
    // ロガーを初期化
    let local_app_data = std::env::var("LOCALAPPDATA").expect("Failed to get LOCALAPPDATA");
    let log_db_path = PathBuf::from(local_app_data)
        .join("MultiCommentViewer")
        .join("logs.db");

    // ログディレクトリを作成
    if let Some(parent) = log_db_path.parent() {
        std::fs::create_dir_all(parent).expect("Failed to create log directory");
    }

    mcv_logger::init_logger(&log_db_path, env!("CARGO_PKG_VERSION"))
        .expect("Failed to initialize logger");

    tracing::info!(
        target: "mcv::main",
        version = env!("CARGO_PKG_VERSION"),
        log_db_path = %log_db_path.display(),
        "mcv started"
    );

    // LogSenderActorを起動するためのストレージを取得
    let log_storage = mcv_logger::get_storage();

    // actixのシステムをセットアップするためのチャネル
    let (tx, rx) = std::sync::mpsc::channel();

    // Actixシステムを別スレッドで実行
    std::thread::spawn(move || {
        let actix_system = System::new();

        actix_system.block_on(async {
            tracing::info!(target: "mcv::main","Actix system thread started");

            // LogSenderActorを起動
            const API_BASE_URL: &str = "http://localhost"; // TODO: 運用環境では実際のAPIサーバーURLに変更
            let _log_sender_addr = mcv_logger::LogSenderActor::new(
                log_storage,
                API_BASE_URL.to_string(),
            ).start();
            tracing::info!(target: "mcv::main", api_base_url = API_BASE_URL, "LogSenderActor started");

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
                                tracing::debug!(
                                    target: "mcv::main",
                                    comment_id = %payload.comment.id,
                                    connection_id = %payload.connection_id,
                                    "Emitting comment-received event"
                                );
                                // connection_idを含めたコメントオブジェクトを作成
                                let mut comment_with_conn = serde_json::to_value(&payload.comment).unwrap();
                                if let Some(obj) = comment_with_conn.as_object_mut() {
                                    obj.insert("connection_id".to_string(), serde_json::Value::String(payload.connection_id.to_string()));
                                }
                                if let Err(e) = app_handle.emit("comment-received", comment_with_conn) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit comment-received event"
                                    );
                                }
                            }
                            MessageType::Connected => {
                                tracing::debug!(target: "mcv::main","Emitting connected event");
                                if let Err(e) = app_handle.emit("connected", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit connected event"
                                    );
                                }
                            }
                            MessageType::Disconnected => {
                                tracing::debug!(target: "mcv::main","Emitting disconnected event");
                                if let Err(e) = app_handle.emit("disconnected", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit disconnected event"
                                    );
                                }
                            }
                            MessageType::AddSite => {
                                tracing::trace!(target: "mcv::main", "Emitting site-added event");
                                if let Err(e) = app_handle.emit("site-added", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit site-added event"
                                    );
                                }
                            }
                            MessageType::AddBrowser => {
                                tracing::trace!(target: "mcv::main", "Emitting browser-added event");
                                if let Err(e) = app_handle.emit("browser-added", message.payload) {
                                    tracing::error!(
                                        target: "mcv::main",
                                        error = %e,
                                        "Failed to emit browser-added event"
                                    );
                                }
                            }
                            _ => {}
                        }
                    }
                });
            });

            core_actor.set_event_callback(event_callback);
            core_actor.set_log_storage(mcv_logger::get_storage());

            tracing::debug!(target: "mcv::main","Starting CoreActor");
            let core_addr = core_actor.start();
            tracing::info!(target: "mcv::main","CoreActor started");

            tracing::debug!(target: "mcv::main","Setting core_addr to PluginManager");
            plugin_manager.set_core_addr(core_addr.clone());
            tracing::debug!(target: "mcv::main","core_addr set to PluginManager");

            // プラグインディレクトリを決定
            // %LOCALAPPDATA%\MultiCommentViewer\plugins\
            let plugins_dir = {
                // 本番環境: %LOCALAPPDATA%\MultiCommentViewer\plugins\
                let local_app_data = std::env::var("LOCALAPPDATA")
                    .expect("Failed to get LOCALAPPDATA");
                PathBuf::from(local_app_data)
                    .join("MultiCommentViewer")
                    .join("plugins")
            };

            tracing::info!(target: "mcv::main", plugins_dir = %plugins_dir.display(), "Loading DLL plugins from directory");

            // プラグインディレクトリをスキャンして自動ロード
            let loaded_plugins = plugin_manager.scan_and_load_plugins(&plugins_dir).await;

            tracing::info!(target: "mcv::main", count = loaded_plugins.len(), "Loaded DLL plugins");

            // ロードされた物理プラグインをCoreActorに登録
            for (physical_plugin_id, plugin_host_addr, _plugin_name) in loaded_plugins {
                tracing::debug!(
                    target: "mcv::main",
                    physical_plugin_id = %physical_plugin_id,
                    "Registering physical plugin with CoreActor"
                );

                core_addr.do_send(mcv_core::core_actor::RegisterPhysicalPlugin {
                    physical_plugin_id,
                    host_addr: plugin_host_addr,
                });
            }

            // AppStateを作成してメインスレッドに送信
            let app_state = AppState {
                core_addr,
                plugin_manager: Arc::new(tokio::sync::Mutex::new(plugin_manager)),
            };

            tracing::debug!(target: "mcv::main", "Sending AppState to main thread");
            tx.send((app_state, app_handle)).expect("Failed to send AppState");

            tracing::info!(target: "mcv::main", "Actix system setup complete, keeping system alive");
        });

        // actixシステムを実行し続ける
        actix_system.run().expect("Actix system failed");
    });

    // メインスレッドでAppStateを受信
    tracing::debug!(target: "mcv::main", "Waiting for AppState from actix thread");
    let (app_state, app_handle) = rx.recv().expect("Failed to receive AppState");
    tracing::info!(target: "mcv::main", "Received AppState, starting Tauri");

    // Tauriアプリを起動
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .setup(move |app| {
            // ウィンドウタイトルにバージョン番号とチャンネルを設定
            if let Some(window) = app.get_webview_window("main") {
                let title = get_title();

                let _ = window.set_title(&title);
                tracing::debug!(target: "mcv::main", title = %title, "Window title set");
            }

            // AppHandleを保存（ブロッキング操作）
            let handle = app.handle().clone();
            let app_handle_clone = app_handle.clone();
            std::thread::spawn(move || {
                let rt = tokio::runtime::Runtime::new().unwrap();
                rt.block_on(async move {
                    *app_handle_clone.lock().await = Some(handle);
                    tracing::debug!(target: "mcv::main", "AppHandle set successfully");
                });
            });
            Ok(())
        })
        .manage(app_state)
        .invoke_handler(tauri::generate_handler![
            add_connection,
            remove_connection,
            rename_connection,
            connect,
            disconnect,
            get_connections,
            get_sites,
            get_browsers,
            set_connection_site,
            update_connection_settings,
            send_comment,
            check_for_updates,
            launch_installer
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
fn get_title() -> String {
    let version = env!("CARGO_PKG_VERSION");

    #[cfg(feature = "alpha")]
    let title = format!("MultiCommentViewer v{} (アルファ版)", version);

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    let title = format!("MultiCommentViewer v{} (ベータ版)", version);

    #[cfg(all(not(feature = "alpha"), not(feature = "beta")))]
    let title = format!("MultiCommentViewer v{}", version);
    title
}
