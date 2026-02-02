use actix::prelude::*;
use mcv_common::{LogicalPluginId, PhysicalPluginId};
use mcv_logger::LogStorage;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
use crate::internal_message::InternalMessage;
use crate::message_handlers;
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
    pub(crate) connection_manager: ConnectionManager,
    pub(crate) site_browser_manager: SiteAndBrowserManager,
    /// 論理プラグイン（ユーザーから見えるプラグイン）
    pub(crate) logical_plugins: HashMap<LogicalPluginId, LogicalPluginInfo>,
    /// 物理プラグイン（DLLファイル）
    pub(crate) physical_plugin_hosts: HashMap<PhysicalPluginId, Addr<PhysicalPluginHostActor>>,
    /// UIへのイベント送信用コールバック
    pub(crate) event_callback: Option<Arc<dyn Fn(McvMessage) + Send + Sync>>,
    /// プラグインログの直接ストレージ保存用
    pub(crate) log_storage: Option<Arc<Mutex<LogStorage>>>,
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
            MessageType::PluginHello => {
                message_handlers::plugin_hello::handle_plugin_hello(self, physical_plugin_id, &message, ctx)
            }
            MessageType::GetPlugins => {
                message_handlers::plugin_hello::handle_get_plugins(self, physical_plugin_id, &message, ctx)
            }
            MessageType::AddConnection => {
                message_handlers::connection::handle_add_connection(self, &message, ctx)
            }
            MessageType::Connect => message_handlers::connection::handle_connect(self, &message, ctx),
            MessageType::Connected => {
                message_handlers::connection::handle_connected(self, &message, ctx)
            }
            MessageType::Disconnect => {
                message_handlers::connection::handle_disconnect(self, &message, ctx)
            }
            MessageType::Disconnected => {
                message_handlers::connection::handle_disconnected(self, &message, ctx)
            }
            MessageType::CommentReceived => {
                message_handlers::comment::handle_comment_received(self, &message, ctx)
            }
            MessageType::SendComment => {
                message_handlers::comment::handle_send_comment(self, &message, ctx)
            }
            MessageType::LogEntry => {
                message_handlers::comment::handle_log_entry(self, &message, ctx)
            }
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
            MessageType::AddConnection => {
                message_handlers::connection::handle_add_connection(self, &message, ctx)
            }
            MessageType::Connect => message_handlers::connection::handle_connect(self, &message, ctx),
            MessageType::Disconnect => {
                message_handlers::connection::handle_disconnect(self, &message, ctx)
            }
            MessageType::SendComment => {
                message_handlers::comment::handle_send_comment(self, &message, ctx)
            }
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
