#![cfg_attr(not(debug_assertions), windows_subsystem = "windows")]

mod commands;
mod comment;
mod comment_store;
mod constraints;
mod crash_handler;
mod event_handler;
mod plugin_fs;
mod types;
mod updater;
mod window;

use actix::prelude::*;
use mcv_core::{CoreActor, PluginManager};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::time::Instant;
use tauri::{AppHandle, Manager};
use uuid::Uuid;

use types::{AppState, PluginsPhase, SettingsDirState};

pub(crate) const API_BASE_URL: &str = "https://int-main.net";

/// ビルドプロファイルを返す（ログレベルフィルタ等で使用）
pub(crate) fn get_build_profile() -> &'static str {
    #[cfg(feature = "alpha")]
    return "alpha";

    #[cfg(all(feature = "beta", not(feature = "alpha")))]
    return "beta";

    #[cfg(all(not(feature = "alpha"), not(feature = "beta")))]
    return "stable";
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

fn main() {
    // 実行ファイルのあるディレクトリへの書き込み可否を確認する
    let app_data_dir = mcv_common::get_base_dir();
    if !window::is_dir_writable(&app_data_dir) {
        #[cfg(windows)]
        window::show_write_restricted_error(&app_data_dir);
        std::process::exit(1);
    }

    window::cleanup_old_files(&app_data_dir);

    // ロガーを初期化
    let log_db_path = app_data_dir.join("logs.db");
    std::fs::create_dir_all(&app_data_dir).expect("Failed to create app data directory");
    mcv_log_core::init_logger(&log_db_path, env!("CARGO_PKG_VERSION"))
        .expect("Failed to initialize logger");

    // 前回クラッシュ時の panic.log を SQLite にリカバリ
    let panic_log_path = app_data_dir.join("panic.log");
    if mcv_log_core::recover_panic_log(&panic_log_path) {
        tracing::warn!(
            target: "mcv::main",
            panic_log_path = %panic_log_path.display(),
            "前回のクラッシュ情報を panic.log から SQLite へ復旧しました"
        );
    }
    mcv_log_core::install_panic_hook(panic_log_path, env!("CARGO_PKG_VERSION").to_string());
    crash_handler::install(&app_data_dir);

    tracing::info!(
        target: "mcv::main",
        version = env!("CARGO_PKG_VERSION"),
        log_db_path = %log_db_path.display(),
        "mcv started"
    );

    let log_storage = mcv_log_core::get_storage();

    // 設定ストレージを初期化（settings/ ディレクトリに JSON ファイルとして保存）
    let settings_dir = app_data_dir.join("settings");
    std::fs::create_dir_all(&settings_dir).expect("Failed to create settings directory");
    let settings_storage = Arc::new(std::sync::Mutex::new(
        mcv_settings_core::SettingsStorage::new(&settings_dir)
            .expect("Failed to initialize settings storage"),
    ));
    tracing::info!(
        target: "mcv::main",
        settings_dir = %settings_dir.display(),
        "Settings storage initialized"
    );

    let settings_dir_for_tauri = settings_dir.clone();
    let logs_dir = app_data_dir.join("logs");
    std::fs::create_dir_all(&logs_dir).expect("Failed to create logs directory");

    // プラグインロード完了を通知する watch チャンネル
    let (plugins_phase_tx, mut plugins_phase_rx) =
        tokio::sync::watch::channel(PluginsPhase::Loading);

    // actix のシステムをセットアップするためのチャネル
    let (tx, rx) =
        std::sync::mpsc::channel::<(AppState, Arc<tokio::sync::Mutex<Option<AppHandle>>>)>();

    // Actixシステムを別スレッドで実行
    std::thread::spawn(move || {
        let actix_system = System::new();

        actix_system.block_on(async {
            tracing::info!(target: "mcv::main", "Actix system thread started");

            let _log_sender_addr =
                mcv_log_core::LogSenderActor::new(log_storage, API_BASE_URL.to_string()).start();
            tracing::info!(target: "mcv::main", api_base_url = API_BASE_URL, "LogSenderActor started");

            let mut core_actor = CoreActor::new();
            let mut plugin_manager = PluginManager::new();

            let app_handle: Arc<tokio::sync::Mutex<Option<AppHandle>>> =
                Arc::new(tokio::sync::Mutex::new(None));

            // connection_id ごとの表示タイミング基準（接続内の最初のメッセージ到着時に初期化）
            let comment_timing: Arc<tokio::sync::Mutex<HashMap<Uuid, (i64, Instant)>>> =
                Arc::new(tokio::sync::Mutex::new(HashMap::new()));

            // コメントストアを作成（exe と同じディレクトリに session.db を配置）
            let session_db_path = std::env::current_exe()
                .expect("Failed to get current exe path")
                .parent()
                .expect("Failed to get exe directory")
                .join("session.db");
            let comment_store = Arc::new(Mutex::new(
                comment_store::CommentStore::new(&session_db_path)
                    .expect("Failed to create comment store"),
            ));

            // イベントコールバックを設定
            let event_callback = event_handler::build_event_callback(
                app_handle.clone(),
                comment_store.clone(),
                comment_timing.clone(),
            );

            core_actor.set_event_callback(event_callback);
            core_actor.set_log_storage(mcv_log_core::get_storage());
            core_actor.set_settings_storage(settings_storage.clone());

            let connections_file_path = settings_dir.join("connections.json");
            core_actor.set_connections_file_path(connections_file_path.clone());
            core_actor.set_logs_dir(logs_dir);
            core_actor.set_plugins_dir(plugin_fs::get_plugin_dir());
            core_actor.set_settings_dir(settings_dir.clone());

            tracing::info!(
                target: "mcv::main",
                path = ?connections_file_path,
                "Restoring connections from file"
            );
            if let Err(e) = core_actor.restore_connections() {
                tracing::warn!(
                    target: "mcv::main",
                    error = %e,
                    "Failed to restore connections"
                );
            }

            tracing::debug!(target: "mcv::main", "Starting CoreActor");
            let core_addr = core_actor.start();
            tracing::info!(target: "mcv::main", "CoreActor started");

            plugin_manager.set_core_addr(core_addr.clone());

            let plugins_dir = plugin_fs::get_plugin_dir();
            tracing::info!(target: "mcv::main", plugins_dir = %plugins_dir.display(), "Loading DLL plugins from directory");

            plugin_fs::cleanup_pending_uninstalls(&plugins_dir);

            let loaded_plugins = plugin_manager.scan_and_load_plugins(&plugins_dir).await;
            tracing::info!(target: "mcv::main", count = loaded_plugins.len(), "Loaded DLL plugins");

            let _ = plugins_phase_tx.send(PluginsPhase::Ready);

            for loaded_info in loaded_plugins {
                tracing::debug!(
                    target: "mcv::main",
                    physical_plugin_id = %loaded_info.physical_plugin_id,
                    abi_version = loaded_info.abi_version,
                    plugin_name = ?loaded_info.plugin_name,
                    "Registering physical plugin with CoreActor"
                );
                core_addr.do_send(mcv_core::core_actor::RegisterPhysicalPlugin {
                    physical_plugin_id: loaded_info.physical_plugin_id,
                    host_addr: loaded_info.host_addr,
                });
            }

            // 定期メモリ使用量ログ（60秒ごと）
            actix::spawn(async {
                let mut interval = tokio::time::interval(std::time::Duration::from_secs(60));
                loop {
                    interval.tick().await;
                    if let Some(mb) = crash_handler::get_process_memory_mb() {
                        tracing::info!(
                            target: "mcv::main",
                            working_set_mb = mb,
                            "Periodic memory usage"
                        );
                    }
                }
            });

            let app_state = AppState {
                core_addr,
                plugin_manager: Arc::new(tokio::sync::Mutex::new(plugin_manager)),
                comment_store,
                pending_core_update: Arc::new(tokio::sync::Mutex::new(None)),
            };

            tracing::debug!(target: "mcv::main", "Sending AppState to main thread");
            tx.send((app_state, app_handle)).expect("Failed to send AppState");
            tracing::info!(target: "mcv::main", "Actix system setup complete, keeping system alive");
        });

        actix_system.run().expect("Actix system failed");
    });

    tracing::debug!(target: "mcv::main", "Waiting for AppState from actix thread");
    let (app_state, app_handle) = rx.recv().expect("Failed to receive AppState");
    tracing::info!(target: "mcv::main", "Received AppState, starting Tauri");

    let settings_dir_for_manage = settings_dir_for_tauri.clone();
    tauri::Builder::default()
        .plugin(tauri_plugin_opener::init())
        .plugin(tauri_plugin_dialog::init())
        .setup(move |app| {
            // ウィンドウタイトル・状態の復元
            if let Some(win) = app.get_webview_window("main") {
                let title = get_title();
                let _ = win.set_title(&title);
                tracing::debug!(target: "mcv::main", title = %title, "Window title set");

                window::restore_window_state(&win, &settings_dir_for_tauri);
                tracing::debug!(target: "mcv::main", settings_dir = %settings_dir_for_tauri.display(), "Window state restore attempted");

                let win_for_close = win.clone();
                let settings_dir_for_close = settings_dir_for_tauri.clone();
                win.on_window_event(move |event| {
                    if let tauri::WindowEvent::CloseRequested { .. } = event {
                        window::save_window_state(&win_for_close, &settings_dir_for_close);
                    }
                });
            }

            // AppHandle を保存（ブロッキング操作）
            let handle = app.handle().clone();
            let app_handle_clone = app_handle.clone();
            std::thread::spawn(move || {
                let rt = tokio::runtime::Runtime::new().unwrap();
                rt.block_on(async move {
                    *app_handle_clone.lock().await = Some(handle);
                    tracing::debug!(target: "mcv::main", "AppHandle set successfully");
                });
            });

            // 起動時プラグイン制約チェック（バックグラウンド）
            {
                let constraint_handle = app.handle().clone();
                let constraint_settings_dir = settings_dir_for_tauri.clone();
                let (constraint_core_addr, constraint_plugin_manager, constraint_pending) = {
                    let s = app.state::<AppState>();
                    (
                        s.core_addr.clone(),
                        Arc::clone(&s.plugin_manager),
                        Arc::clone(&s.pending_core_update),
                    )
                };
                std::thread::spawn(move || {
                    let rt = tokio::runtime::Runtime::new().unwrap();
                    rt.block_on(async move {
                        let _ = plugins_phase_rx
                            .wait_for(|p| *p == PluginsPhase::Ready)
                            .await;
                        tracing::info!(target: "mcv::main", "core 制約チェックを開始");
                        constraints::run_core_constraint_check(
                            &constraint_handle,
                            constraint_pending,
                        )
                        .await;
                        tracing::info!(target: "mcv::main", "プラグイン制約チェックを開始");
                        constraints::run_plugin_constraint_check(
                            &constraint_settings_dir,
                            &constraint_handle,
                            constraint_core_addr,
                            constraint_plugin_manager,
                        )
                        .await;
                    });
                });
            }

            Ok(())
        })
        .manage(app_state)
        .manage(SettingsDirState {
            settings_dir: settings_dir_for_manage,
        })
        .invoke_handler(tauri::generate_handler![
            commands::connection::add_connection,
            commands::connection::remove_connection,
            commands::connection::rename_connection,
            commands::connection::connect,
            commands::connection::disconnect,
            commands::connection::get_connections,
            commands::connection::get_sites,
            commands::connection::get_browsers,
            commands::connection::set_connection_site,
            commands::connection::update_connection_settings,
            commands::connection::detect_url,
            commands::comment::send_comment,
            commands::comment::get_comment_schema,
            commands::comment::fetch_account_info,
            commands::comment::get_plugins,
            #[cfg(feature = "comment-search")]
            commands::comment::search_comments,
            commands::comment::get_users,
            commands::plugin::install_registry_plugin,
            commands::plugin::uninstall_registry_plugin,
            commands::plugin::list_installed_plugins,
            commands::plugin::get_plugin_constraint_status,
            commands::update::check_for_updates,
            commands::update::get_current_version,
            commands::update::list_registry_plugins,
            commands::update::download_core_update,
            commands::update::apply_core_update,
            commands::update::get_pending_core_update,
            commands::settings::get_settings_schema,
            commands::settings::get_settings,
            commands::settings::update_settings,
            commands::settings::get_column_settings,
            commands::settings::save_column_settings,
            commands::log::get_logs,
            commands::log::get_build_profile_info,
            commands::trace::frontend_trace,
        ])
        .run(tauri::generate_context!())
        .expect("error while running tauri application");
}
