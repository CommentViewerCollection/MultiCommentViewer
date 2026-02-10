use actix::prelude::*;
use mcv_common::{LogicalPluginId, PhysicalPluginId};
use mcv_log_core::LogStorage;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use uuid::Uuid;

use crate::connection_manager::{ConnectionInfo, ConnectionManager, ConnectionStatus};
use crate::internal_message::InternalMessage;
use crate::message_handlers;
use crate::plugin_loader_strategy::PluginHostAddr;
use crate::site_browser_manager::{BrowserInfo, SiteAndBrowserManager, SiteInfo};

/// 論理プラグイン情報（ユーザーから見えるプラグイン単位）
#[derive(Debug, Clone)]
pub struct LogicalPluginInfo {
    pub logical_plugin_id: LogicalPluginId,
    pub physical_plugin_id: PhysicalPluginId,
    pub name: String,
    pub role: Vec<String>,
    pub api_version: String,
    pub host_addr: PluginHostAddr,
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
    pub(crate) physical_plugin_hosts: HashMap<PhysicalPluginId, PluginHostAddr>,
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
            MessageType::AddSite => {
                message_handlers::site_browser::handle_add_site(self, &message, ctx)
            }
            MessageType::AddBrowser => {
                message_handlers::site_browser::handle_add_browser(self, &message, ctx)
            }
            MessageType::SetConnectionSite => {
                message_handlers::site_browser::handle_set_connection_site(self, &message, ctx)
            }
            MessageType::UpdateConnectionSettings => {
                message_handlers::site_browser::handle_update_connection_settings(self, &message, ctx)
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
            MessageType::AddSite => {
                message_handlers::site_browser::handle_add_site(self, &message, ctx)
            }
            MessageType::AddBrowser => {
                message_handlers::site_browser::handle_add_browser(self, &message, ctx)
            }
            MessageType::SetConnectionSite => {
                message_handlers::site_browser::handle_set_connection_site(self, &message, ctx)
            }
            MessageType::UpdateConnectionSettings => {
                message_handlers::site_browser::handle_update_connection_settings(self, &message, ctx)
            }
            MessageType::CommentReceived => {
                message_handlers::comment::handle_comment_received(self, &message, ctx)
            }
            MessageType::Connected => {
                message_handlers::connection::handle_connected(self, &message, ctx)
            }
            MessageType::Disconnected => {
                message_handlers::connection::handle_disconnected(self, &message, ctx)
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
    pub host_addr: PluginHostAddr,
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
        message_handlers::site_browser::handle_set_connection_site(self, &message, ctx);
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
        message_handlers::site_browser::handle_update_connection_settings(self, &message, ctx);
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
