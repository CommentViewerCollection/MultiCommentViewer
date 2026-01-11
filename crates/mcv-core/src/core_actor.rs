use actix::prelude::*;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

use crate::connection_manager::{ConnectionManager, ConnectionStatus};
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
                eprintln!("Failed to parse plugin-hello payload: {}", e);
                return;
            }
        };

        println!("Plugin registered: {} ({})", payload.name, payload.plugin_id);

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
        let connection_id = Uuid::new_v4();

        // プラグインIDを取得
        let plugin_id = match &message.src {
            MessageSource::Plugin { plugin_id } => *plugin_id,
            _ => {
                eprintln!("add-connection must come from a plugin");
                return;
            }
        };

        // Connection Managerに登録
        let connection_manager = self.connection_manager.clone();
        let response_message = message.clone();

        actix::spawn(async move {
            let mut manager = connection_manager.write().await;
            manager.add_connection(connection_id, plugin_id);
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
                eprintln!("Failed to parse connect payload: {}", e);
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
            eprintln!("Plugin not found: {}", plugin_id);
        }
    }

    /// connectedを処理
    fn handle_connected(&mut self, message: McvMessage, _ctx: &mut Context<Self>) {
        let payload: ConnectedPayload = match serde_json::from_value(message.payload.clone()) {
            Ok(p) => p,
            Err(e) => {
                eprintln!("Failed to parse connected payload: {}", e);
                return;
            }
        };

        println!("Connection established: {}", payload.connection_id);

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
                eprintln!("Failed to parse disconnect payload: {}", e);
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
                    if let Some(plugin_info) = plugins.get(&conn_info.plugin_id) {
                        plugin_info.host_addr.do_send(SendMessageToPlugin { message: msg });
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
                eprintln!("Failed to parse disconnected payload: {}", e);
                return;
            }
        };

        println!("Connection disconnected: {}", payload.connection_id);

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
        println!("CoreActor received message: {:?}", message.message_type);

        match message.message_type {
            MessageType::PluginHello => self.handle_plugin_hello(message, ctx),
            MessageType::AddConnection => self.handle_add_connection(message, ctx),
            MessageType::Connect => self.handle_connect(message, ctx),
            MessageType::Connected => self.handle_connected(message, ctx),
            MessageType::Disconnect => self.handle_disconnect(message, ctx),
            MessageType::Disconnected => self.handle_disconnected(message, ctx),
            MessageType::CommentReceived => self.handle_comment_received(message, ctx),
            _ => {
                eprintln!("Unhandled message type: {:?}", message.message_type);
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
