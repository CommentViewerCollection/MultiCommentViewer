use actix::prelude::*;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use crate::connection_manager::{ConnectionManager, ConnectionStatus, ConnectionInfo};
use crate::plugin_host_actor::{PluginHostActor, SendMessageToPlugin};
use crate::site_browser_manager::{SiteAndBrowserManager, SiteInfo, BrowserInfo};

/// プラグイン情報
#[derive(Debug, Clone)]
pub struct PluginInfo {
    pub name: String,
    pub plugin_id: Uuid,
    pub role: Vec<String>,
    pub api_version: String,
    pub host_addr: Addr<PluginHostActor>,
}

/// Core Actor
///
/// システムの中心となるActor
/// メッセージルーティングとビジネスロジックを担当
pub struct CoreActor {
    connection_manager: Arc<RwLock<ConnectionManager>>,
    site_browser_manager: Arc<RwLock<SiteAndBrowserManager>>,  // 新規
    plugins: HashMap<Uuid, PluginInfo>,
    /// UIへのイベント送信用コールバック
    event_callback: Option<Arc<dyn Fn(McvMessage) + Send + Sync>>,
}

impl CoreActor {
    /// 新しいCore Actorを作成
    pub fn new() -> Self {
        Self {
            connection_manager: Arc::new(RwLock::new(ConnectionManager::new())),
            site_browser_manager: Arc::new(RwLock::new(SiteAndBrowserManager::new())),  // 新規
            plugins: HashMap::new(),
            event_callback: None,
        }
    }

    /// イベントコールバックを設定
    pub fn set_event_callback(&mut self, callback: Arc<dyn Fn(McvMessage) + Send + Sync>) {
        self.event_callback = Some(callback);
    }

    /// プラグインを登録
    pub fn register_plugin(&mut self, plugin_id: Uuid, plugin_info: PluginInfo) {
        self.plugins.insert(plugin_id, plugin_info);
    }

    /// plugin-helloを処理
    fn handle_plugin_hello(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: PluginHelloPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    error = %e,
                    message_type = "plugin-hello",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        tracing::info!(
            plugin_name = %payload.name,
            plugin_id = %payload.plugin_id,
            "Plugin registered"
        );

        // plugin-addedを返信
        let response = McvMessage::create_response(
            &message,
            MessageType::PluginAdded,
            serde_json::to_value(PluginAddedPayload {
                name: payload.name.clone(),
                plugin_id: payload.plugin_id,
                role: payload.role.clone(),
                api_version: payload.api_version.clone(),
            })
            .unwrap(),
        );

        // プラグインへ返信
        if let Some(plugin_info) = self.plugins.get(&payload.plugin_id) {
            plugin_info.host_addr.do_send(SendMessageToPlugin {
                message: response,
            });
        }
    }

    /// add-connectionを処理
    fn handle_add_connection(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: AddConnectionPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    error = %e,
                    message_type = "add-connection",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        let connection_id = Uuid::new_v4();

        // プラグインIDを取得
        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                tracing::error!(
                    message_type = "add-connection",
                    message_source = ?message.src,
                    "Message must come from a plugin"
                );
                return;
            }
        };

        // プラグイン名を取得
        let site_name = self.plugins.get(&plugin_id)
            .map(|p| p.name.clone())
            .unwrap_or_else(|| "Unknown".to_string());

        let input_info = format!("{:?}", payload.site);

        // Connection Managerに登録
        let connection_manager = self.connection_manager.clone();
        let response_message = message.clone();

        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.add_connection(connection_id, Some(plugin_id), site_name, input_info, format!("Connection {}", connection_id));
        });

        // connection-addedを返信
        let response = McvMessage::create_response(
            &response_message,
            MessageType::ConnectionAdded,
            serde_json::to_value(ConnectionAddedPayload { connection_id }).unwrap(),
        );

        // プラグインへ返信
        if let MessageSource::Plugin { plugin_id } = message.src {
            if let Some(plugin_info) = self.plugins.get(&plugin_id) {
                plugin_info.host_addr.do_send(SendMessageToPlugin {
                    message: response,
                });
            }
        }
    }

    /// connectを処理
    fn handle_connect(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: ConnectPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    error = %e,
                    message_type = "connect",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        let connection_id = payload.connection_id;

        // Connection Managerから接続情報を取得してplugin_idを取得
        let connection_manager = self.connection_manager.clone();
        let plugins = self.plugins.clone();
        let msg = message.clone();

        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.update_status(&connection_id, ConnectionStatus::Connecting);

            // plugin_idを取得
            if let Some(conn_info) = manager.get_connection(&connection_id) {
                if let Some(plugin_id) = conn_info.plugin_id {
                    drop(manager); // ロックを解放

                    // プラグインへconnectメッセージを転送
                    if let Some(plugin_info) = plugins.get(&plugin_id) {
                        plugin_info.host_addr.do_send(SendMessageToPlugin { message: msg });
                    } else {
                        tracing::error!(
                            plugin_id = %plugin_id,
                            connection_id = %connection_id,
                            "Plugin not found"
                        );
                    }
                } else {
                    tracing::error!(
                        connection_id = %connection_id,
                        "Connection has no plugin_id set"
                    );
                }
            } else {
                tracing::error!(
                    connection_id = %connection_id,
                    "Connection not found"
                );
            }
        });
    }

    /// connectedを処理
    fn handle_connected(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: ConnectedPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
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
        let connection_manager = self.connection_manager.clone();
        let connection_id = payload.connection_id;
        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.update_status(&connection_id, ConnectionStatus::Connected);
        });

        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message);
        }
    }

    /// disconnectを処理
    fn handle_disconnect(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: DisconnectPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
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
            let connection_manager = self.connection_manager.clone();
            let plugins = self.plugins.clone();
            let msg = message.clone();

            actix::spawn(async move {
                let manager = connection_manager.read().await;
                if let Some(conn_info) = manager.get_connection(&connection_id) {
                    if let Some(plugin_id) = conn_info.plugin_id {
                        if let Some(plugin_info) = plugins.get(&plugin_id) {
                            plugin_info.host_addr.do_send(SendMessageToPlugin { message: msg });
                        }
                    }
                }
            });
        }
    }

    /// disconnectedを処理
    fn handle_disconnected(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: DisconnectedPayload = match serde_json::from_value(message.payload.clone())
        {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
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
        let connection_manager = self.connection_manager.clone();
        let connection_id = payload.connection_id;
        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.update_status(&connection_id, ConnectionStatus::Disconnected);
        });

        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message);
        }
    }

    /// comment-receivedを処理
    fn handle_comment_received(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        // UIへイベント通知
        if let Some(callback) = &self.event_callback {
            callback(message);
        }
    }

    /// send-commentを処理
    fn handle_send_comment(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: SendCommentPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
                    error = %e,
                    message_type = "send-comment",
                    "Failed to parse message payload"
                );
                return;
            }
        };

        // 該当する接続のプラグインへコメントを転送
        let connection_id = payload.connection_id;
        let connection_manager = self.connection_manager.clone();
        let plugins = self.plugins.clone();
        let msg = message.clone();

        actix::spawn(async move {
            let manager = connection_manager.read().await;
            if let Some(conn_info) = manager.get_connection(&connection_id) {
                if let Some(plugin_id) = conn_info.plugin_id {
                    if let Some(plugin_info) = plugins.get(&plugin_id) {
                        plugin_info.host_addr.do_send(SendMessageToPlugin { message: msg });
                    }
                }
            }
        });
    }

    /// log-entryを処理（プラグインからのログメッセージ）
    fn handle_log_entry(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: LogEntryPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(
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
                    message_type = "log-entry",
                    message_source = ?message.src,
                    "Received log-entry from non-plugin source"
                );
                return;
            }
        };

        // ログレベルに応じてトレース（mcv-loggerが自動記録）
        // 注: Core側のログレベル設定（mcvのfeature フラグ）に従ってフィルタリングされる
        match payload.level.as_str() {
            "error" => {
                tracing::error!(
                    plugin_id = %plugin_id,
                    plugin_version = ?payload.plugin_version,
                    plugin_build_profile = ?payload.plugin_build_profile,
                    connection_id = ?payload.connection_id,
                    context = ?payload.context,
                    "Plugin error: {}",
                    payload.message
                );
            }
            "warn" => {
                tracing::warn!(
                    plugin_id = %plugin_id,
                    plugin_version = ?payload.plugin_version,
                    plugin_build_profile = ?payload.plugin_build_profile,
                    connection_id = ?payload.connection_id,
                    context = ?payload.context,
                    "Plugin warning: {}",
                    payload.message
                );
            }
            "info" => {
                tracing::info!(
                    plugin_id = %plugin_id,
                    plugin_version = ?payload.plugin_version,
                    plugin_build_profile = ?payload.plugin_build_profile,
                    connection_id = ?payload.connection_id,
                    context = ?payload.context,
                    "Plugin info: {}",
                    payload.message
                );
            }
            "debug" => {
                tracing::debug!(
                    plugin_id = %plugin_id,
                    plugin_version = ?payload.plugin_version,
                    plugin_build_profile = ?payload.plugin_build_profile,
                    connection_id = ?payload.connection_id,
                    context = ?payload.context,
                    "Plugin debug: {}",
                    payload.message
                );
            }
            _ => {
                tracing::trace!(
                    plugin_id = %plugin_id,
                    plugin_version = ?payload.plugin_version,
                    plugin_build_profile = ?payload.plugin_build_profile,
                    "Plugin log: {}",
                    payload.message
                );
            }
        }
    }

    /// add-siteを処理
    fn handle_add_site(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: AddSitePayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse AddSitePayload");
                return;
            }
        };

        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                tracing::error!("AddSite must come from a plugin");
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

        let manager = self.site_browser_manager.clone();
        let site_info_clone = site_info.clone();
        actix::spawn(async move {
            let mut mgr = manager.write().await;
            mgr.add_site(site_info_clone);
        });

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
            site_name = %payload.site_name,
            plugin_id = %plugin_id,
            "Site registered"
        );
    }

    /// add-browserを処理
    fn handle_add_browser(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: AddBrowserPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse AddBrowserPayload");
                return;
            }
        };

        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                tracing::error!("AddBrowser must come from a plugin");
                return;
            }
        };

        let browser_info = BrowserInfo {
            browser_id: payload.browser_id,
            browser_name: payload.browser_name.clone(),
            display_name: payload.display_name.clone(),
            plugin_id,
        };

        let manager = self.site_browser_manager.clone();
        let browser_info_clone = browser_info.clone();
        actix::spawn(async move {
            let mut mgr = manager.write().await;
            mgr.add_browser(browser_info_clone);
        });

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
            browser_name = %payload.browser_name,
            plugin_id = %plugin_id,
            "Browser registered"
        );
    }

    /// set-connection-siteを処理（UIから呼ばれる）
    fn handle_set_connection_site(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: SetConnectionSitePayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse SetConnectionSitePayload");
                return;
            }
        };

        let connection_id = payload.connection_id;
        let site_id = payload.site_id;

        tracing::debug!(
            connection_id = %connection_id,
            site_id = %site_id,
            "Setting connection site"
        );

        // 前のサイトを取得してDiscardConnectionSiteを送信
        let conn_mgr = self.connection_manager.clone();
        let site_mgr = self.site_browser_manager.clone();
        let plugins = self.plugins.clone();

        actix::spawn(async move {
            let mut conn_manager = conn_mgr.write().await;
            let site_manager = site_mgr.read().await;

            // 前のplugin_idを取得
            let old_plugin_id = conn_manager
                .get_connection(&connection_id)
                .and_then(|c| c.plugin_id);

            // 新しいサイト情報を取得
            if let Some(site_info) = site_manager.get_site(&site_id) {
                conn_manager.set_site(
                    &connection_id,
                    site_id,
                    site_info.display_name.clone(),
                    site_info.plugin_id,
                );

                // 前のプラグインにDiscardConnectionSiteを送信
                if let Some(old_pid) = old_plugin_id {
                    if old_pid != site_info.plugin_id {
                        if let Some(old_plugin) = plugins.get(&old_pid) {
                            let discard_msg = McvMessage::new(
                                MessageType::DiscardConnectionSite,
                                MessageSource::Core,
                                MessageDestination::Plugin { plugin_id: old_pid },
                                serde_json::to_value(DiscardConnectionSitePayload {
                                    connection_id,
                                    site_id,
                                }).unwrap(),
                            );
                            old_plugin.host_addr.do_send(SendMessageToPlugin {
                                message: discard_msg,
                            });
                        }
                    }
                }

                // 新しいプラグインにSetConnectionSiteを送信
                if let Some(new_plugin) = plugins.get(&site_info.plugin_id) {
                    let set_msg = McvMessage::new(
                        MessageType::SetConnectionSite,
                        MessageSource::Core,
                        MessageDestination::Plugin { plugin_id: site_info.plugin_id },
                        serde_json::to_value(SetConnectionSitePayload {
                            connection_id,
                            site_id,
                        }).unwrap(),
                    );
                    new_plugin.host_addr.do_send(SendMessageToPlugin {
                        message: set_msg,
                    });
                }
            }
        });
    }

    /// update-connection-settingsを処理
    fn handle_update_connection_settings(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: UpdateConnectionSettingsPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                tracing::error!(error = %e, "Failed to parse UpdateConnectionSettingsPayload");
                return;
            }
        };

        tracing::debug!(
            connection_id = %payload.connection_id,
            has_url = payload.url.is_some(),
            has_browser = payload.browser_id.is_some(),
            has_settings = payload.advanced_settings.is_some(),
            "Updating connection settings"
        );

        let conn_mgr = self.connection_manager.clone();
        let site_mgr = self.site_browser_manager.clone();

        actix::spawn(async move {
            let mut manager = conn_mgr.write().await;
            let site_manager = site_mgr.read().await;

            if let Some(url) = payload.url {
                manager.update_url(&payload.connection_id, Some(url));
            }

            if let Some(browser_id) = payload.browser_id {
                let browser_name = site_manager
                    .get_browser(&browser_id)
                    .map(|b| b.display_name.clone());
                manager.update_browser(&payload.connection_id, Some(browser_id), browser_name);
            }

            if let Some(settings) = payload.advanced_settings {
                manager.update_advanced_settings(&payload.connection_id, Some(settings));
            }
        });
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

/// プラグインからcoreへのメッセージ
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendMessageToCore {
    pub message: McvMessage,
}

impl Handler<SendMessageToCore> for CoreActor {
    type Result = ();

    fn handle(&mut self, msg: SendMessageToCore, ctx: &mut Self::Context) {
        let message = msg.message;
        tracing::debug!(
            message_type = ?message.message_type,
            "CoreActor received message"
        );

        match message.message_type {
            MessageType::PluginHello => self.handle_plugin_hello(message, ctx),
            MessageType::AddConnection => self.handle_add_connection(message, ctx),
            MessageType::Connect => self.handle_connect(message, ctx),
            MessageType::Connected => self.handle_connected(message, ctx),
            MessageType::Disconnect => self.handle_disconnect(message, ctx),
            MessageType::Disconnected => self.handle_disconnected(message, ctx),
            MessageType::CommentReceived => self.handle_comment_received(message, ctx),
            MessageType::SendComment => self.handle_send_comment(message, ctx),
            MessageType::LogEntry => self.handle_log_entry(message, ctx),
            MessageType::AddSite => self.handle_add_site(message, ctx),
            MessageType::AddBrowser => self.handle_add_browser(message, ctx),
            MessageType::SetConnectionSite => self.handle_set_connection_site(message, ctx),
            MessageType::UpdateConnectionSettings => self.handle_update_connection_settings(message, ctx),
            _ => {
                tracing::warn!(
                    message_type = ?message.message_type,
                    "Unhandled message type"
                );
            }
        }
    }
}

/// UIからcoreへのメッセージ
#[derive(Message)]
#[rtype(result = "Result<McvMessage, String>")]
pub struct SendRequest {
    pub message: McvMessage,
}

impl Handler<SendRequest> for CoreActor {
    type Result = Result<McvMessage, String>;

    fn handle(&mut self, msg: SendRequest, ctx: &mut Self::Context) -> Self::Result {
        // UIからのリクエストをcoreで処理
        self.handle(
            SendMessageToCore {
                message: msg.message.clone(),
            },
            ctx,
        );

        // 簡易的な応答を返す
        Ok(msg.message)
    }
}

/// プラグインを登録
#[derive(Message)]
#[rtype(result = "()")]
pub struct RegisterPlugin {
    pub plugin_id: Uuid,
    pub plugin_info: PluginInfo,
}

impl Handler<RegisterPlugin> for CoreActor {
    type Result = ();

    fn handle(&mut self, msg: RegisterPlugin, _ctx: &mut Self::Context) {
        self.plugins.insert(msg.plugin_id, msg.plugin_info);
    }
}

/// 接続一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<ConnectionInfo>")]
pub struct GetConnections;

impl Handler<GetConnections> for CoreActor {
    type Result = ResponseActFuture<Self, Vec<ConnectionInfo>>;

    fn handle(&mut self, _msg: GetConnections, _ctx: &mut Self::Context) -> Self::Result {
        let connection_manager = self.connection_manager.clone();

        let fut = async move {
            let manager = connection_manager.read().await;
            manager.list_connections().into_iter().cloned().collect()
        };

        Box::pin(fut.into_actor(self))
    }
}

/// 接続を作成（UI主導）
#[derive(Message)]
#[rtype(result = "Uuid")]
pub struct CreateConnection {
    pub plugin_id: Option<Uuid>,  // 変更: Option<Uuid>に
    pub site_name: String,
    pub input_info: String,
    pub name: String,
}

impl Handler<CreateConnection> for CoreActor {
    type Result = ResponseActFuture<Self, Uuid>;

    fn handle(&mut self, msg: CreateConnection, _ctx: &mut Self::Context) -> Self::Result {
        let connection_id = Uuid::new_v4();
        let connection_manager = self.connection_manager.clone();

        let fut = async move {
            let mut manager = connection_manager.write().await;
            manager.add_connection(
                connection_id,
                msg.plugin_id,
                msg.site_name,
                msg.input_info,
                msg.name,
            );
            connection_id
        };

        Box::pin(fut.into_actor(self))
    }
}

/// 接続を削除
#[derive(Message)]
#[rtype(result = "Result<(), String>")]
pub struct RemoveConnection {
    pub connection_id: Uuid,
}

impl Handler<RemoveConnection> for CoreActor {
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: RemoveConnection, _ctx: &mut Self::Context) -> Self::Result {
        let connection_manager = self.connection_manager.clone();

        let fut = async move {
            let mut manager = connection_manager.write().await;

            // 接続のステータスを確認
            if let Some(status) = manager.get_status(&msg.connection_id) {
                if status == ConnectionStatus::Connected || status == ConnectionStatus::Connecting {
                    return Err("Cannot remove connected connection".to_string());
                }
            }

            manager.remove_connection(&msg.connection_id);
            Ok(())
        };

        Box::pin(fut.into_actor(self))
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
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: RenameConnection, _ctx: &mut Self::Context) -> Self::Result {
        let connection_manager = self.connection_manager.clone();

        let fut = async move {
            let mut manager = connection_manager.write().await;
            manager.rename_connection(&msg.connection_id, msg.new_name);
            Ok(())
        };

        Box::pin(fut.into_actor(self))
    }
}

/// サイト一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<SiteInfo>")]
pub struct GetSites;

impl Handler<GetSites> for CoreActor {
    type Result = ResponseActFuture<Self, Vec<SiteInfo>>;

    fn handle(&mut self, _msg: GetSites, _ctx: &mut Context<Self>) -> Self::Result {
        let manager = self.site_browser_manager.clone();
        Box::pin(
            async move {
                let mgr = manager.read().await;
                mgr.list_sites()
            }
            .into_actor(self),
        )
    }
}

/// ブラウザ一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<BrowserInfo>")]
pub struct GetBrowsers;

impl Handler<GetBrowsers> for CoreActor {
    type Result = ResponseActFuture<Self, Vec<BrowserInfo>>;

    fn handle(&mut self, _msg: GetBrowsers, _ctx: &mut Context<Self>) -> Self::Result {
        let manager = self.site_browser_manager.clone();
        Box::pin(
            async move {
                let mgr = manager.read().await;
                mgr.list_browsers()
            }
            .into_actor(self),
        )
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
            }).unwrap(),
        );
        self.handle_set_connection_site(message, ctx);
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
            }).unwrap(),
        );
        self.handle_update_connection_settings(message, ctx);
        Box::pin(async { Ok(()) }.into_actor(self))
    }
}
