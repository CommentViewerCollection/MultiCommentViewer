use actix::prelude::*;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use crate::connection_manager::{ConnectionManager, ConnectionStatus, ConnectionInfo};
use crate::plugin_host_actor::{PluginHostActor, SendMessageToPlugin};

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
    plugins: HashMap<Uuid, PluginInfo>,
    /// UIへのイベント送信用コールバック
    event_callback: Option<Arc<dyn Fn(McvMessage) + Send + Sync>>,
}

impl CoreActor {
    /// 新しいCore Actorを作成
    pub fn new() -> Self {
        Self {
            connection_manager: Arc::new(RwLock::new(ConnectionManager::new())),
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

        // Connection Managerのステータスを更新
        let connection_manager = self.connection_manager.clone();
        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.update_status(&connection_id, ConnectionStatus::Connecting);
        });

        // プラグインへconnectメッセージを転送
        let plugin_id = payload.site.id;
        if let Some(plugin_info) = self.plugins.get(&plugin_id) {
            plugin_info.host_addr.do_send(SendMessageToPlugin { message });
        } else {
            tracing::error!(
                plugin_id = %plugin_id,
                "Plugin not found"
            );
        }
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
