use actix::prelude::*;
use mcv_common::{LogicalPluginId, PhysicalPluginId};
use mcv_logger::{
    LogEntry as LoggerEntry, LogLevel, LogStorage, SourceLocation as LoggerSourceLocation,
    StackFrame as LoggerStackFrame, SystemInfo as LoggerSystemInfo,
};
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
use crate::internal_message::InternalMessage;
use crate::plugin_host_actor::{PhysicalPluginHostActor, SendMessageToPlugin};
use crate::site_browser_manager::{BrowserInfo, SiteAndBrowserManager, SiteInfo};

/// 論理プラグイン情報（ユーザーから見えるプラグイン単位）
#[derive(Debug, Clone)]
pub struct LogicalPluginInfo {
    pub logical_plugin_id: LogicalPluginId,
    pub physical_plugin_id: PhysicalPluginId,
    pub name: String,
    pub role: Vec<String>,
    pub api_version: String,
    pub host_addr: Addr<PhysicalPluginHostActor>,
}

/// 後方互換性のため
pub type PluginInfo = LogicalPluginInfo;

/// Core Actor
///
/// システムの中心となるActor
/// メッセージルーティングとビジネスロジックを担当
pub struct CoreActor {
    connection_manager: ConnectionManager,
    site_browser_manager: SiteAndBrowserManager,
    /// 論理プラグイン（ユーザーから見えるプラグイン）
    logical_plugins: HashMap<LogicalPluginId, LogicalPluginInfo>,
    /// 物理プラグイン（DLLファイル）
    physical_plugin_hosts: HashMap<PhysicalPluginId, Addr<PhysicalPluginHostActor>>,
    /// UIへのイベント送信用コールバック
    event_callback: Option<Arc<dyn Fn(McvMessage) + Send + Sync>>,
    /// プラグインログの直接ストレージ保存用
    log_storage: Option<Arc<Mutex<LogStorage>>>,
}

impl CoreActor {
    /// 新しいCore Actorを作成
    pub fn new() -> Self {
        Self {
            connection_manager: ConnectionManager::new(),
            site_browser_manager: SiteAndBrowserManager::new(),
            logical_plugins: HashMap::new(),
            physical_plugin_hosts: HashMap::new(),
            event_callback: None,
            log_storage: None,
        }
    }

    /// イベントコールバックを設定
    pub fn set_event_callback(&mut self, callback: Arc<dyn Fn(McvMessage) + Send + Sync>) {
        self.event_callback = Some(callback);
    }

    /// プラグインログの直接保存用ストレージを設定
    pub fn set_log_storage(&mut self, storage: Arc<Mutex<LogStorage>>) {
        self.log_storage = Some(storage);
    }

    /// plugin-helloを処理（InternalMessage対応）
    fn handle_plugin_hello(
        &mut self,
        physical_plugin_id: PhysicalPluginId,
        message: &McvMessage,
        _ctx: &mut Context<Self>,
    ) {
        let payload: PluginHelloPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    "Failed to parse plugin-hello payload"
                );
                return;
            }
        };

        // payload.plugin_id（Uuid）をLogicalPluginIdに変換
        let logical_plugin_id = LogicalPluginId::from_uuid(payload.plugin_id);

        // 既に論理プラグインとして登録済みか確認
        if self.logical_plugins.contains_key(&logical_plugin_id) {
            tracing::debug!(
                target: "mcv::core::CoreActor",
                logical_plugin_id = %logical_plugin_id,
                "Logical plugin already registered, ignoring duplicate plugin-hello"
            );
            return;
        }

        // 物理プラグインのPluginHostActorを取得
        let physical_plugin_host_addr =
            if let Some(addr) = self.physical_plugin_hosts.get(&physical_plugin_id) {
                // DLL物理プラグインの場合
                tracing::trace!(
                    target: "mcv::core::CoreActor",
                    physical_plugin_id = %physical_plugin_id,
                    logical_plugin_id = %logical_plugin_id,
                    "Found physical plugin host for DLL logical plugin"
                );
                addr.clone()
            } else {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    physical_plugin_id = %physical_plugin_id,
                    "Physical plugin host not found"
                );
                return;
            };

        // LogicalPluginInfoを作成
        let logical_plugin_info = LogicalPluginInfo {
            logical_plugin_id,
            physical_plugin_id,
            name: payload.name.clone(),
            role: payload.role.clone(),
            api_version: payload.api_version.clone(),
            host_addr: physical_plugin_host_addr,
        };

        // 論理プラグインとして登録
        self.logical_plugins
            .insert(logical_plugin_id, logical_plugin_info);

        tracing::info!(
            target: "mcv::core::CoreActor",
            physical_plugin_id = %physical_plugin_id,
            logical_plugin_id = %logical_plugin_id,
            logical_plugin_name = %payload.name,
            "Logical plugin registered (physical_plugin_id → logical_plugin_id mapping created)"
        );

        // plugin-addedを全論理プラグインにブロードキャスト
        let response = McvMessage::new(
            MessageType::PluginAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::to_value(PluginAddedPayload {
                name: payload.name.clone(),
                plugin_id: logical_plugin_id.inner(),
                role: payload.role.clone(),
                api_version: payload.api_version.clone(),
            })
            .unwrap(),
        );

        // 全論理プラグインにブロードキャスト
        for (_, logical_plugin_info) in &self.logical_plugins {
            logical_plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: response.clone(),
            });
        }

        tracing::info!(
            target: "mcv::core::CoreActor",
            logical_plugin_id = %logical_plugin_id,
            logical_plugins_count = self.logical_plugins.len(),
            "Logical plugin registered and plugin-added broadcasted to all logical plugins"
        );
    }

    /// 全論理プラグインにメッセージをブロードキャスト
    fn broadcast_to_all_logical_plugins(&self, message: McvMessage) {
        for (_, logical_plugin_info) in &self.logical_plugins {
            logical_plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: message.clone(),
            });
        }
    }

    /// get-pluginsを処理（InternalMessage対応）
    fn handle_get_plugins(
        &mut self,
        physical_plugin_id: PhysicalPluginId,
        _message: &McvMessage,
        _ctx: &mut Context<Self>,
    ) {
        // リクエスト元の論理プラグインを探す
        // （物理plugin_idから論理plugin_idを特定）
        let requester_logical_plugin_info =
            self.logical_plugins.values().find(|logical_plugin_info| {
                logical_plugin_info.physical_plugin_id == physical_plugin_id
            });

        let requester_logical_plugin_info = match requester_logical_plugin_info {
            Some(info) => info,
            None => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    physical_plugin_id = %physical_plugin_id,
                    "Logical plugin not found for get-plugins request"
                );
                return;
            }
        };

        tracing::info!(
            target: "mcv::core::CoreActor",
            physical_plugin_id = %physical_plugin_id,
            logical_plugin_id = %requester_logical_plugin_info.logical_plugin_id,
            logical_plugins_count = self.logical_plugins.len(),
            "Processing get-plugins request from logical plugin"
        );

        // 全論理プラグインの情報をplugin-addedメッセージとして送信
        for (logical_plugin_id, logical_plugin_info) in &self.logical_plugins {
            let plugin_added_message = McvMessage::new(
                MessageType::PluginAdded,
                MessageSource::Core,
                MessageDestination::Plugin {
                    plugin_id: requester_logical_plugin_info.logical_plugin_id.inner(),
                },
                serde_json::to_value(PluginAddedPayload {
                    name: logical_plugin_info.name.clone(),
                    plugin_id: logical_plugin_id.inner(),
                    role: logical_plugin_info.role.clone(),
                    api_version: logical_plugin_info.api_version.clone(),
                })
                .unwrap(),
            );

            // リクエスト元の物理プラグインのPluginHostActorに送信
            requester_logical_plugin_info
                .host_addr
                .do_send(SendMessageToPlugin {
                    message: plugin_added_message,
                });

            tracing::debug!(
                target: "mcv::core::CoreActor",
                logical_plugin_id = %logical_plugin_id,
                logical_plugin_name = %logical_plugin_info.name,
                "Sent plugin-added for logical plugin in response to get-plugins"
            );
        }

        tracing::info!(
            target: "mcv::core::CoreActor",
            requester_logical_plugin_id = %requester_logical_plugin_info.logical_plugin_id,
            "get-plugins request completed, sent all logical plugin info"
        );
    }

    fn generate_connection_default_name(&self) -> String {
        // 現在の接続を取得してデフォルト名を生成
        let connections = self.connection_manager.get_connections();

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
        default_name
    }
    /// add-connectionを処理
    fn handle_add_connection(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        tracing::trace!(
            target: "mcv::core::CoreActor",
            "handle_add_connection()"
        );
        println!("handle_add_connection()");
        // let payload: AddConnectionPayload = match serde_json::from_value(message.payload.clone()) {
        //     Ok(p) => p,
        //     Err(e) => {
        //         tracing::error!(
        //             target: "mcv::core::CoreActor",
        //             error = %e,
        //             message_type = "add-connection",
        //             "Failed to parse message payload"
        //         );
        //         return;
        //     }
        // };

        let connection_id = Uuid::new_v4();

        // プラグインIDを取得（Uuid）
        // let plugin_id_uuid = match &message.src {
        //     MessageSource::Plugin { plugin_id } => *plugin_id,
        //     _ => {
        //         tracing::error!(
        //             target: "mcv::core::CoreActor",
        //             message_type = "add-connection",
        //             message_source = ?message.src,
        //             "Message must come from a plugin"
        //         );
        //         return;
        //     }
        // };

        // // プラグイン名を取得（LogicalPluginIdに変換）
        // let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);

        // Connection Managerに登録
        let response_message = message.clone();
        let conn_name = self.generate_connection_default_name();

        self.connection_manager
            .add_connection(connection_id, conn_name.clone());
        println!("connectionを追加 id={}, name={}", connection_id,&conn_name);

        // connection-addedを返信
        let response = McvMessage::create_response(
            &response_message,
            MessageType::ConnectionAdded,
            serde_json::to_value(ConnectionAddedPayload { connection_id }).unwrap(),
        );

        // プラグインへ返信
        if let MessageSource::Plugin { plugin_id } = message.src {
            let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id);
            if let Some(plugin_info) = self.logical_plugins.get(&logical_plugin_id) {
                plugin_info
                    .host_addr
                    .do_send(SendMessageToPlugin { message: response });
            }
        }
        println!("ABCDEFEJLKFJLKFJDKFJ");
        // 全論理プラグインにConnectionAddedをブロードキャスト
        let broadcast_msg = McvMessage::new_notification(
            MessageType::ConnectionAdded,
            MessageSource::Core,
            MessageDestination::Broadcast,
            serde_json::to_value(ConnectionAddedPayload { connection_id }).unwrap(),
        );
        self.broadcast_to_all_logical_plugins(broadcast_msg);
        println!("mcv::core connection-addedをブロードキャストした");
    }

    /// connectを処理
    fn handle_connect(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: ConnectPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "connect",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        let connection_id = payload.connection_id;

        // Connection Managerから接続情報を取得してplugin_idを取得
        // let connection_manager = self.connection_manager.clone();
        let plugins = self.logical_plugins.clone();
        let msg = message.clone();

        // actix::spawn(async move {
        //     let mut manager = connection_manager.write().await;
        self.connection_manager
            .update_status(&connection_id, ConnectionStatus::Connecting);

        // plugin_idを取得
        if let Some(conn_info) = self.connection_manager.get_connection(&connection_id) {
            if let Some(plugin_id_uuid) = conn_info.plugin_id {
                // drop(manager); // ロックを解放

                let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);

                // デバッグ: 登録されている全plugin_idをログ出力
                let registered_plugin_ids: Vec<String> =
                    plugins.keys().map(|id| id.to_string()).collect();
                tracing::debug!(
                    target: "mcv::core::CoreActor",
                    connection_id = %connection_id,
                    plugin_id_from_connection = %plugin_id_uuid,
                    registered_plugin_ids = ?registered_plugin_ids,
                    "Attempting to find plugin for connection"
                );

                // プラグインへconnectメッセージを転送
                if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                    tracing::debug!(
                        target: "mcv::core::CoreActor",
                        plugin_id = %plugin_id_uuid,
                        plugin_name = %plugin_info.name,
                        connection_id = %connection_id,
                        "Found plugin, forwarding connect message"
                    );
                    plugin_info
                        .host_addr
                        .do_send(SendMessageToPlugin { message: msg });
                } else {
                    tracing::error!(
                        target: "mcv::core::CoreActor",
                        plugin_id = %plugin_id_uuid,
                        connection_id = %connection_id,
                        registered_plugin_count = plugins.len(),
                        "Plugin not found - plugin_id mismatch detected"
                    );
                }
            } else {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    connection_id = %connection_id,
                    "Connection has no plugin_id set"
                );
            }
        } else {
            tracing::error!(
                target: "mcv::core::CoreActor",
                connection_id = %connection_id,
                "Connection not found"
            );
        }
        // });
    }

    /// connectedを処理
    fn handle_connected(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: ConnectedPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "connected",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        tracing::info!(
            connection_id = %payload.connection_id,
            "Connection established"
        );

        // Connection Managerのステータスを更新
        // let connection_manager = self.connection_manager.clone();
        let connection_id = payload.connection_id;
        self.connection_manager
            .update_status(&connection_id, ConnectionStatus::Connected);

        // 全論理プラグインにConnectedをブロードキャスト
        let broadcast_msg = McvMessage::new_notification(
            MessageType::Connected,
            MessageSource::Core,
            MessageDestination::Broadcast,
            message.payload.clone(),
        );
        self.broadcast_to_all_logical_plugins(broadcast_msg);

        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message.clone());
        }
    }

    /// disconnectを処理
    fn handle_disconnect(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: DisconnectPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "disconnect",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        // プラグインへdisconnectメッセージを転送
        if let MessageSource::Core = message.src {
            // UIからのリクエストの場合、該当するプラグインへ転送
            let connection_id = payload.connection_id;
            // let connection_manager = self.connection_manager.clone();
            let plugins = self.logical_plugins.clone();
            let msg = message.clone();

            if let Some(conn_info) = self.connection_manager.get_connection(&connection_id) {
                if let Some(plugin_id_uuid) = conn_info.plugin_id {
                    let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);
                    if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                        plugin_info
                            .host_addr
                            .do_send(SendMessageToPlugin { message: msg });
                    }
                }
            }
        }
    }

    /// disconnectedを処理
    fn handle_disconnected(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: DisconnectedPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "disconnected",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        tracing::info!(
            connection_id = %payload.connection_id,
            "Connection disconnected"
        );

        // Connection Managerのステータスを更新
        let connection_id = payload.connection_id;
        self.connection_manager
            .update_status(&connection_id, ConnectionStatus::Disconnected);

        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message.clone());
        }
    }

    /// comment-receivedを処理
    fn handle_comment_received(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message.clone());
        }
    }

    /// send-commentを処理
    fn handle_send_comment(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: SendCommentPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "send-comment",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        // 該当する接続のプラグインへコメントを転送
        let connection_id = payload.connection_id;
        //let connection_manager = self.connection_manager.clone();
        let plugins = self.logical_plugins.clone();
        let msg = message.clone();

        if let Some(conn_info) = self.connection_manager.get_connection(&connection_id) {
            if let Some(plugin_id_uuid) = conn_info.plugin_id {
                let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);
                if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                    plugin_info
                        .host_addr
                        .do_send(SendMessageToPlugin { message: msg });
                }
            }
        }
    }

    /// log-entryを処理（プラグインからのログメッセージ）
    fn handle_log_entry(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: LogEntryPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    message_type = "log-entry",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        // プラグインIDを取得
        let plugin_id = match message.src {
            MessageSource::Plugin { plugin_id } => plugin_id,
            _ => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    message_type = "log-entry",
                    message_source = ?message.src,
                    "Received log-entry from non-plugin source"
                );
                return;
            }
        };

        // ログレベルを変換
        let level = match payload.level.as_str() {
            "error" => LogLevel::Error,
            "warn" => LogLevel::Warn,
            "info" => LogLevel::Info,
            "debug" => LogLevel::Debug,
            _ => LogLevel::Trace,
        };

        // context から元のソース位置情報を抽出
        let (source_file, source_line, source_module) = match &payload.context {
            Some(ctx) => {
                let file = ctx
                    .get("source")
                    .and_then(|s| s.get("file"))
                    .and_then(|f| f.as_str())
                    .unwrap_or("unknown")
                    .to_string();
                let line = ctx
                    .get("source")
                    .and_then(|s| s.get("line"))
                    .and_then(|l| l.as_u64())
                    .unwrap_or(0) as u32;
                let module = ctx
                    .get("source")
                    .and_then(|s| s.get("module_path"))
                    .and_then(|m| m.as_str())
                    .unwrap_or("unknown")
                    .to_string();
                (file, line, module)
            }
            None => ("unknown".to_string(), 0, "unknown".to_string()),
        };

        // context から スタックトレースを抽出
        let stacktrace = payload
            .context
            .as_ref()
            .and_then(|ctx| ctx.get("stacktrace"))
            .and_then(|st| st.as_array())
            .map(|frames| {
                frames
                    .iter()
                    .map(|f| LoggerStackFrame {
                        symbol: f
                            .get("symbol")
                            .and_then(|s| s.as_str())
                            .map(|s| s.to_string()),
                        filename: f
                            .get("filename")
                            .and_then(|s| s.as_str())
                            .map(|s| s.to_string()),
                        lineno: f.get("lineno").and_then(|n| n.as_u64()).map(|n| n as u32),
                        addr: f
                            .get("addr")
                            .and_then(|s| s.as_str())
                            .unwrap_or("0x0")
                            .to_string(),
                    })
                    .collect()
            });

        // context から source・stacktrace を除いた残りを保持
        let context = payload.context.as_ref().and_then(|ctx| {
            if let Some(obj) = ctx.as_object() {
                let filtered: serde_json::Map<String, serde_json::Value> = obj
                    .iter()
                    .filter(|(k, _)| k.as_str() != "source" && k.as_str() != "stacktrace")
                    .map(|(k, v)| (k.clone(), v.clone()))
                    .collect();
                if filtered.is_empty() {
                    None
                } else {
                    // plugin_id を構造化データとして追加
                    let mut with_plugin = filtered;
                    with_plugin.insert(
                        "plugin_id".to_string(),
                        serde_json::Value::String(plugin_id.to_string()),
                    );
                    Some(serde_json::Value::Object(with_plugin))
                }
            } else {
                Some(ctx.clone())
            }
        });

        // LogEntry を構築して直接ストレージに保存
        let entry = LoggerEntry {
            id: Uuid::new_v4().to_string(),
            level,
            timestamp: chrono::Utc::now().timestamp_millis(),
            message: payload.message,
            source: LoggerSourceLocation {
                file: source_file,
                line: source_line,
                column: None,
                module_path: source_module,
            },
            stacktrace,
            context,
            system_info: LoggerSystemInfo {
                mcv_version: env!("CARGO_PKG_VERSION").to_string(),
                platform: std::env::consts::OS.to_string(),
                arch: std::env::consts::ARCH.to_string(),
                build_profile: payload
                    .plugin_build_profile
                    .unwrap_or_else(|| "unknown".to_string()),
            },
        };

        if let Some(ref storage) = self.log_storage {
            if let Ok(storage) = storage.lock() {
                if let Err(e) = storage.insert(&entry) {
                    tracing::error!(
                        target: "mcv::core::CoreActor",
                        error = %e,
                        plugin_id = %plugin_id,
                        "Failed to insert plugin log entry into storage"
                    );
                }
            }
        }
    }

    /// add-siteを処理
    fn handle_add_site(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: AddSitePayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse AddSitePayload");
                return;
            }
        };

        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                tracing::error!(target: "mcv::core::CoreActor","AddSite must come from a plugin");
                return;
            }
        };

        let site_info = SiteInfo {
            site_id: payload.site_id,
            site_name: payload.site_name.clone(),
            display_name: payload.display_name.clone(),
            plugin_id,
            options_schema: payload.options_schema,
        };

        tracing::info!(
            target: "mcv::core::CoreActor",
            site_id = %payload.site_id,
            site_name = %payload.site_name,
            plugin_id_from_message_src = %plugin_id,
            "Registering site (plugin_id is from message.src)"
        );

        // let manager = self.site_browser_manager.clone();
        let site_info_clone = site_info.clone();
        self.site_browser_manager.add_site(site_info_clone);

        // UIにイベント通知
        if let Some(callback) = &self.event_callback {
            let event = McvMessage::new_notification(
                MessageType::AddSite,
                MessageSource::Core,
                MessageDestination::Core,
                serde_json::to_value(&site_info).unwrap(),
            );
            callback(event);
        }

        tracing::info!(
            target: "mcv::core::CoreActor",
            site_name = %payload.site_name,
            plugin_id = %plugin_id,
            "Site registered"
        );
    }

    /// add-browserを処理
    fn handle_add_browser(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: AddBrowserPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse AddBrowserPayload");
                return;
            }
        };

        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                tracing::error!(target: "mcv::core::CoreActor","AddBrowser must come from a plugin");
                return;
            }
        };

        let browser_info = BrowserInfo {
            browser_id: payload.browser_id,
            browser_name: payload.browser_name.clone(),
            display_name: payload.display_name.clone(),
            plugin_id,
        };

        let browser_info_clone = browser_info.clone();
        self.site_browser_manager.add_browser(browser_info_clone);

        // UIにイベント通知
        if let Some(callback) = &self.event_callback {
            let event = McvMessage::new_notification(
                MessageType::AddBrowser,
                MessageSource::Core,
                MessageDestination::Core,
                serde_json::to_value(&browser_info).unwrap(),
            );
            callback(event);
        }

        tracing::info!(
            target: "mcv::core::CoreActor",
            browser_name = %payload.browser_name,
            plugin_id = %plugin_id,
            "Browser registered"
        );
    }

    /// set-connection-siteを処理（UIから呼ばれる）
    fn handle_set_connection_site(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: SetConnectionSitePayload = match serde_json::from_value(
            message.payload.clone(),
        ) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(target: "mcv::core::CoreActor",error = %e, "Failed to parse SetConnectionSitePayload");
                return;
            }
        };

        let connection_id = payload.connection_id;
        let site_id = payload.site_id;

        tracing::debug!(
            target: "mcv::core::CoreActor",
            connection_id = %connection_id,
            site_id = %site_id,
            "Setting connection site"
        );

        // 前のサイトを取得してDiscardConnectionSiteを送信
        // let conn_mgr = self.connection_manager.clone();
        // let site_mgr = self.site_browser_manager.clone();
        let plugins = self.logical_plugins.clone();

        // actix::spawn(async move {
        //     let mut conn_manager = conn_mgr.write().await;
        //     let site_manager = site_mgr.read().await;

        // 前のplugin_idを取得
        let old_plugin_id = self
            .connection_manager
            .get_connection(&connection_id)
            .and_then(|c| c.plugin_id);

        // 新しいサイト情報を取得
        if let Some(site_info) = self.site_browser_manager.get_site(&site_id) {
            self.connection_manager.set_site(
                &connection_id,
                site_id,
                site_info.display_name.clone(),
                site_info.plugin_id,
            );

            // 前のプラグインにDiscardConnectionSiteを送信
            if let Some(old_pid) = old_plugin_id {
                if old_pid != site_info.plugin_id {
                    let old_logical_plugin_id = LogicalPluginId::from_uuid(old_pid);
                    if let Some(old_plugin) = plugins.get(&old_logical_plugin_id) {
                        let discard_msg = McvMessage::new(
                            MessageType::DiscardConnectionSite,
                            MessageSource::Core,
                            MessageDestination::Plugin { plugin_id: old_pid },
                            serde_json::to_value(DiscardConnectionSitePayload {
                                connection_id,
                                site_id,
                            })
                            .unwrap(),
                        );
                        old_plugin.host_addr.do_send(SendMessageToPlugin {
                            message: discard_msg,
                        });
                    }
                }
            }

            // 新しいプラグインにSetConnectionSiteを送信
            let new_logical_plugin_id = LogicalPluginId::from_uuid(site_info.plugin_id);
            if let Some(new_plugin) = plugins.get(&new_logical_plugin_id) {
                let set_msg = McvMessage::new(
                    MessageType::SetConnectionSite,
                    MessageSource::Core,
                    MessageDestination::Plugin {
                        plugin_id: site_info.plugin_id,
                    },
                    serde_json::to_value(SetConnectionSitePayload {
                        connection_id,
                        site_id,
                    })
                    .unwrap(),
                );
                new_plugin
                    .host_addr
                    .do_send(SendMessageToPlugin { message: set_msg });
            }
        }
        // });
    }

    /// update-connection-settingsを処理
    fn handle_update_connection_settings(&mut self, message: &McvMessage, _ctx: &mut Context<Self>) {
        let payload: UpdateConnectionSettingsPayload =
            match serde_json::from_value(message.payload.clone()) {
                Ok(p) => p,
                Err(e) => {
                    tracing::error!(error = %e, "Failed to parse UpdateConnectionSettingsPayload");
                    return;
                }
            };

        tracing::debug!(
            target: "mcv::core::CoreActor",
            connection_id = %payload.connection_id,
            has_url = payload.url.is_some(),
            has_browser = payload.browser_id.is_some(),
            has_settings = payload.advanced_settings.is_some(),
            "Updating connection settings"
        );

        if let Some(url) = payload.url {
            self.connection_manager
                .update_url(&payload.connection_id, Some(url));
        }

        if let Some(browser_id) = payload.browser_id {
            let browser_name = self
                .site_browser_manager
                .get_browser(&browser_id)
                .map(|b| b.display_name.clone());
            self.connection_manager.update_browser(
                &payload.connection_id,
                Some(browser_id),
                browser_name,
            );
        }

        if let Some(settings) = payload.advanced_settings {
            self.connection_manager
                .update_advanced_settings(&payload.connection_id, Some(settings));
        }
    }
}

impl Actor for CoreActor {
    type Context = Context<Self>;
}

impl Default for CoreActor {
    fn default() -> Self {
        Self::new()
    }
}

// ============================================================================
// メッセージハンドラ
// ============================================================================

/// プラグインからcoreへのメッセージ（InternalMessage対応）
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendMessageToCore {
    pub internal_message: InternalMessage,
}

impl Handler<SendMessageToCore> for CoreActor {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToCore, ctx: &mut Self::Context) {
        let InternalMessage {
            physical_plugin_id,
            message,
        } = msg.internal_message;

        tracing::trace!(
            target: "mcv::core::CoreActor",
            physical_plugin_id = %physical_plugin_id,
            message_type = ?message.message_type,
            "CoreActor received message from plugin"
        );

        match message.message_type {
            MessageType::PluginHello => self.handle_plugin_hello(physical_plugin_id, &message, ctx),
            MessageType::GetPlugins => self.handle_get_plugins(physical_plugin_id, &message, ctx),
            MessageType::AddConnection => self.handle_add_connection(&message, ctx),
            MessageType::Connect => self.handle_connect(&message, ctx),
            MessageType::Connected => self.handle_connected(&message, ctx),
            MessageType::Disconnect => self.handle_disconnect(&message, ctx),
            MessageType::Disconnected => self.handle_disconnected(&message, ctx),
            MessageType::CommentReceived => self.handle_comment_received(&message, ctx),
            MessageType::SendComment => self.handle_send_comment(&message, ctx),
            MessageType::LogEntry => self.handle_log_entry(&message, ctx),
            MessageType::AddSite => self.handle_add_site(&message, ctx),
            MessageType::AddBrowser => self.handle_add_browser(&message, ctx),
            MessageType::SetConnectionSite => self.handle_set_connection_site(&message, ctx),
            MessageType::UpdateConnectionSettings => {
                self.handle_update_connection_settings(&message, ctx)
            }
            _ => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    message_type = ?message.message_type,
                    "Unhandled message type"
                );
            }
        }
    }
}

/// UIからcoreへのメッセージ
pub struct SendRequest {
    pub message: McvMessage,
}
impl actix::Message for SendRequest {
    type Result = Result<McvMessage, String>;
}

impl Handler<SendRequest> for CoreActor {
    type Result = Result<McvMessage, String>;

    fn handle(&mut self, msg: SendRequest, ctx: &mut Self::Context) -> Self::Result {
        let message = msg.message;
        match message.message_type {
            MessageType::AddConnection => self.handle_add_connection(&message, ctx),
            MessageType::Connect => self.handle_connect(&message, ctx),
            MessageType::Disconnect => self.handle_disconnect(&message, ctx),
            MessageType::SendComment => self.handle_send_comment(&message, ctx),
            MessageType::AddSite => self.handle_add_site(&message, ctx),
            MessageType::AddBrowser => self.handle_add_browser(&message, ctx),
            MessageType::SetConnectionSite => self.handle_set_connection_site(&message, ctx),
            MessageType::UpdateConnectionSettings => {
                self.handle_update_connection_settings(&message, ctx)
            }
            _ => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    message_type = ?message.message_type,
                    "Unhandled UI message type"
                );
            }
        }
        Ok(message)
    }
}

/// 物理プラグイン（DLL）を登録
#[derive(Message)]
#[rtype(result = "()")]
pub struct RegisterPhysicalPlugin {
    pub physical_plugin_id: PhysicalPluginId,
    pub host_addr: Addr<PhysicalPluginHostActor>,
}

impl Handler<RegisterPhysicalPlugin> for CoreActor {
    type Result = ();

    fn handle(&mut self, msg: RegisterPhysicalPlugin, _ctx: &mut Self::Context) {
        tracing::info!(
            target: "mcv::core::CoreActor",
            physical_plugin_id = %msg.physical_plugin_id,
            "Registering physical plugin"
        );

        self.physical_plugin_hosts
            .insert(msg.physical_plugin_id, msg.host_addr);

        tracing::debug!(
            target: "mcv::core::CoreActor",
            physical_plugin_id = %msg.physical_plugin_id,
            physical_plugins_count = self.physical_plugin_hosts.len(),
            "Physical plugin registered"
        );
    }
}

/// 接続一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<ConnectionInfo>")]
pub struct GetConnections;

impl Handler<GetConnections> for CoreActor {
    type Result = Vec<ConnectionInfo>;

    fn handle(&mut self, _msg: GetConnections, _ctx: &mut Self::Context) -> Self::Result {
        self.connection_manager
            .list_connections()
            .into_iter()
            .cloned()
            .collect()
    }
}

/// 接続を作成（UI主導）
#[derive(Message)]
#[rtype(result = "Uuid")]
pub struct CreateConnection {
    pub plugin_id: Option<Uuid>, // 変更: Option<Uuid>に
    pub site_name: String,
    pub input_info: String,
    pub name: String,
}
impl Handler<CreateConnection> for CoreActor {
    type Result = MessageResult<CreateConnection>;

    fn handle(&mut self, msg: CreateConnection, _ctx: &mut Self::Context) -> Self::Result {
        let connection_id = Uuid::new_v4();
        self.connection_manager
            .add_connection(connection_id, msg.name);
        MessageResult(connection_id)
    }
}

/// 接続を削除
#[derive(Message)]
#[rtype(result = "Result<(), String>")]
pub struct RemoveConnection {
    pub connection_id: Uuid,
}

impl Handler<RemoveConnection> for CoreActor {
    type Result = Result<(), String>;

    fn handle(&mut self, msg: RemoveConnection, _ctx: &mut Self::Context) -> Self::Result {
        if let Some(status) = self.connection_manager.get_status(&msg.connection_id) {
            if status == ConnectionStatus::Connected || status == ConnectionStatus::Connecting {
                return Err("Cannot remove connected connection".to_string());
            }
        }

        self.connection_manager
            .remove_connection(&msg.connection_id);
        Ok(())
    }
}

/// 接続名を変更
#[derive(Message)]
#[rtype(result = "Result<(), String>")]
pub struct RenameConnection {
    pub connection_id: Uuid,
    pub new_name: String,
}

impl Handler<RenameConnection> for CoreActor {
    type Result = Result<(), String>;

    fn handle(&mut self, msg: RenameConnection, _ctx: &mut Self::Context) -> Self::Result {
        self.connection_manager
            .rename_connection(&msg.connection_id, msg.new_name);
        Ok(())
    }
}

/// サイト一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<SiteInfo>")]
pub struct GetSites;

impl Handler<GetSites> for CoreActor {
    type Result = Vec<SiteInfo>;

    fn handle(&mut self, _msg: GetSites, _ctx: &mut Context<Self>) -> Self::Result {
        self.site_browser_manager.list_sites()
    }
}

/// ブラウザ一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<BrowserInfo>")]
pub struct GetBrowsers;

impl Handler<GetBrowsers> for CoreActor {
    type Result = Vec<BrowserInfo>;

    fn handle(&mut self, _msg: GetBrowsers, _ctx: &mut Context<Self>) -> Self::Result {
        self.site_browser_manager.list_browsers()
    }
}

/// 接続にサイトを設定
#[derive(Message)]
#[rtype(result = "Result<(), String>")]
pub struct SetConnectionSite {
    pub connection_id: Uuid,
    pub site_id: Uuid,
}

impl Handler<SetConnectionSite> for CoreActor {
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: SetConnectionSite, ctx: &mut Context<Self>) -> Self::Result {
        let message = McvMessage::new(
            MessageType::SetConnectionSite,
            MessageSource::Core,
            MessageDestination::Core,
            serde_json::to_value(SetConnectionSitePayload {
                connection_id: msg.connection_id,
                site_id: msg.site_id,
            })
            .unwrap(),
        );
        self.handle_set_connection_site(&message, ctx);
        Box::pin(async { Ok(()) }.into_actor(self))
    }
}

/// 接続設定を更新
#[derive(Message)]
#[rtype(result = "Result<(), String>")]
pub struct UpdateConnectionSettings {
    pub connection_id: Uuid,
    pub url: Option<String>,
    pub browser_id: Option<Uuid>,
    pub advanced_settings: Option<serde_json::Value>,
}

impl Handler<UpdateConnectionSettings> for CoreActor {
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: UpdateConnectionSettings, ctx: &mut Context<Self>) -> Self::Result {
        let message = McvMessage::new(
            MessageType::UpdateConnectionSettings,
            MessageSource::Core,
            MessageDestination::Core,
            serde_json::to_value(UpdateConnectionSettingsPayload {
                connection_id: msg.connection_id,
                url: msg.url,
                browser_id: msg.browser_id,
                advanced_settings: msg.advanced_settings,
            })
            .unwrap(),
        );
        self.handle_update_connection_settings(&message, ctx);
        Box::pin(async { Ok(()) }.into_actor(self))
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[actix::test]
    async fn test_core_actor_creation() {
        let core = CoreActor::new();
        assert!(core.event_callback.is_none());
        assert_eq!(core.logical_plugins.len(), 0);
        assert_eq!(core.physical_plugin_hosts.len(), 0);
    }

    #[actix::test]
    async fn test_get_connections_empty() {
        let core = CoreActor::new();
        let addr = core.start();

        let connections = addr.send(GetConnections).await.unwrap();
        assert_eq!(connections.len(), 0);
    }

    #[actix::test]
    async fn test_create_connection() {
        let core = CoreActor::new();
        let addr = core.start();

        let conn_id = addr
            .send(CreateConnection {
                plugin_id: None,
                site_name: "Test".to_string(),
                input_info: "{}".to_string(),
                name: "#1".to_string(),
            })
            .await
            .unwrap();

        assert!(!conn_id.is_nil());

        let connections = addr.send(GetConnections).await.unwrap();
        assert_eq!(connections.len(), 1);
        assert_eq!(connections[0].name, "#1");
    }

    #[actix::test]
    async fn test_remove_connection() {
        let core = CoreActor::new();
        let addr = core.start();

        let conn_id = addr
            .send(CreateConnection {
                plugin_id: None,
                site_name: "Test".to_string(),
                input_info: "{}".to_string(),
                name: "#1".to_string(),
            })
            .await
            .unwrap();

        let result = addr
            .send(RemoveConnection {
                connection_id: conn_id,
            })
            .await
            .unwrap();
        assert!(result.is_ok());

        let connections = addr.send(GetConnections).await.unwrap();
        assert_eq!(connections.len(), 0);
    }

    #[actix::test]
    async fn test_rename_connection() {
        let core = CoreActor::new();
        let addr = core.start();

        let conn_id = addr
            .send(CreateConnection {
                plugin_id: None,
                site_name: "Test".to_string(),
                input_info: "{}".to_string(),
                name: "#1".to_string(),
            })
            .await
            .unwrap();

        addr.send(RenameConnection {
            connection_id: conn_id,
            new_name: "My Stream".to_string(),
        })
        .await
        .unwrap()
        .unwrap();

        let connections = addr.send(GetConnections).await.unwrap();
        assert_eq!(connections[0].name, "My Stream");
    }
}
