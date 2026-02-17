use actix::prelude::*;
use mcv_common::{LogicalPluginId, PhysicalPluginId, SiteId};
use mcv_log_core::LogStorage;
use mcv_messages::{Message as McvMessage, MessageSource, MessageType, *};
use mcv_settings_core::SettingsStorage;
use std::collections::HashMap;
use std::path::PathBuf;
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
    /// プラグインが登録した設定スキーマ（キャッシュ）
    pub settings_schema: Option<serde_json::Value>,
    /// プラグインの現在の設定値（キャッシュ）
    pub settings_data: Option<serde_json::Value>,
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
    /// 設定ストレージ
    pub(crate) settings_storage: Option<Arc<Mutex<SettingsStorage>>>,
    /// 接続永続化ファイルのパス
    pub(crate) connections_file_path: Option<PathBuf>,
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
            settings_storage: None,
            connections_file_path: None,
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

    /// 設定ストレージを設定
    pub fn set_settings_storage(&mut self, storage: Arc<Mutex<SettingsStorage>>) {
        self.settings_storage = Some(storage);
    }

    /// 接続ファイルパスを設定
    pub fn set_connections_file_path(&mut self, path: PathBuf) {
        self.connections_file_path = Some(path);
    }

    /// 接続をファイルに保存
    pub fn save_connections(&self) -> Result<(), String> {
        let path = self
            .connections_file_path
            .as_ref()
            .ok_or_else(|| "Connections file path not set".to_string())?;

        let storage = crate::connection_persistence::ConnectionsStorage::from_connection_manager(
            &self.connection_manager,
        );
        storage.save_to_file(path)?;

        tracing::debug!(path = ?path, "Connections saved");
        Ok(())
    }

    /// 接続をファイルから復元
    pub fn restore_connections(&mut self) -> Result<(), String> {
        let path = self
            .connections_file_path
            .as_ref()
            .ok_or_else(|| "Connections file path not set".to_string())?;

        let storage = crate::connection_persistence::ConnectionsStorage::load_from_file(path)?;

        tracing::info!(
            connection_count = storage.connections.len(),
            "Restoring connections from file"
        );

        let (success, skipped) = self
            .connection_manager
            .import_from_persistence(storage.connections, &self.site_browser_manager);

        tracing::info!(
            success_count = success,
            skipped_count = skipped.len(),
            "Connection restoration complete"
        );

        if !skipped.is_empty() {
            for (name, reason) in &skipped {
                tracing::warn!(
                    connection_name = %name,
                    reason = %reason,
                    "Skipped connection restoration"
                );
            }
        }

        // UIに復元された接続を通知
        if let Some(ref callback) = self.event_callback {
            for conn in self.connection_manager.list_connections() {
                let msg = McvMessage::new_notification(
                    MessageType::ConnectionAdded,
                    MessageSource::Core,
                    MessageDestination::Core,
                    serde_json::to_value(ConnectionAddedPayload {
                        connection_id: conn.connection_id,
                        name: conn.name.clone(),
                    })
                    .unwrap(),
                );
                callback(msg);
            }
        }

        Ok(())
    }

    /// SetConnectionSiteメッセージをプラグインに送信（共通処理）
    ///
    /// UIからの呼び出しとPending有効化の両方から使用される
    pub(crate) fn send_set_connection_site(&mut self, connection_id: Uuid, site_id: SiteId) {
        tracing::debug!(
            target: "mcv::core::CoreActor",
            connection_id = %connection_id,
            site_id = %site_id,
            "Sending SetConnectionSite to plugin"
        );

        // 前のplugin_idを取得
        let old_plugin_id = self
            .connection_manager
            .get_connection(&connection_id)
            .and_then(|c| c.plugin_id);

        // 新しいサイト情報を取得
        if let Some(site_info) = self.site_browser_manager.get_site(&site_id) {
            // ConnectionManagerを更新
            self.connection_manager
                .set_site(&connection_id, site_id.clone(), site_info.plugin_id);

            let plugins = self.logical_plugins.clone();

            // 前のプラグインにDiscardConnectionSiteを送信（プラグインが変わった場合）
            if let Some(old_pid) = old_plugin_id {
                if old_pid != site_info.plugin_id {
                    let old_logical_plugin_id = LogicalPluginId::from_uuid(old_pid);
                    if let Some(old_plugin) = plugins.get(&old_logical_plugin_id) {
                        let discard_msg = McvMessage::new_notification(
                            MessageType::DiscardConnectionSite,
                            MessageSource::Core,
                            MessageDestination::Plugin { plugin_id: old_pid },
                            serde_json::to_value(DiscardConnectionSitePayload {
                                connection_id,
                                site_id: site_id.clone(),
                            })
                            .unwrap(),
                        );
                        old_plugin.host_addr.do_send(
                            crate::plugin_host_actor::SendMessageToPlugin {
                                message: discard_msg,
                            },
                        );
                    }
                }
            }

            // 新しいプラグインにSetConnectionSiteを送信
            let new_logical_plugin_id = LogicalPluginId::from_uuid(site_info.plugin_id);
            if let Some(new_plugin) = plugins.get(&new_logical_plugin_id) {
                let set_msg = McvMessage::new_notification(
                    MessageType::SetConnectionSite,
                    MessageSource::Core,
                    MessageDestination::Plugin {
                        plugin_id: site_info.plugin_id,
                    },
                    serde_json::to_value(SetConnectionSitePayload {
                        connection_id,
                        site_id: site_id.clone(),
                    })
                    .unwrap(),
                );
                new_plugin
                    .host_addr
                    .do_send(crate::plugin_host_actor::SendMessageToPlugin { message: set_msg });

                tracing::info!(
                    target: "mcv::core::CoreActor",
                    connection_id = %connection_id,
                    site_id = %site_id,
                    plugin_id = %site_info.plugin_id,
                    "SetConnectionSite message sent to plugin"
                );
            }
        }
    }

    /// Core設定のJSON Schemaを取得
    pub fn get_core_settings_schema() -> serde_json::Value {
        serde_json::json!({
            "type": "object",
            "properties": {
                "theme": {
                    "type": "string",
                    "title": "テーマ",
                    "enum": ["dark", "light"],
                    "default": "dark"
                },
                "auto_scroll": {
                    "type": "boolean",
                    "title": "自動スクロール",
                    "description": "新しいコメントが届いたときに自動的にスクロールします",
                    "default": true
                },
                "max_comments": {
                    "type": "integer",
                    "title": "最大コメント数",
                    "description": "保持する最大コメント数（メモリ使用量に影響）",
                    "default": 1000,
                    "minimum": 100,
                    "maximum": 10000
                },
                "enable_color_by_plugin_or_connection": {
                    "type": "boolean",
                    "title": "サイト毎または接続毎に色を付ける",
                    "description": "チェックすると、配信サイトまたは接続ごとにコメントの背景色と文字色を設定できます",
                    "default": true
                },
                "color_mode": {
                    "type": "string",
                    "title": "↳ 色分けモード",
                    "description": "配信サイト毎: 設定画面で各サイトの色を設定 / 接続毎: 接続一覧で各接続の色を設定",
                    "enum": ["site", "connection"],
                    "enumNames": ["配信サイト毎", "接続毎"],
                    "default": "site"
                },
                "site_colors": {
                    "type": "object",
                    "title": "↳ 配信サイト毎の色設定",
                    "description": "各配信サイト（YouTubeLive、OPENREC、Twitch等）の背景色と文字色を設定します。「配信サイト毎」モード選択時のみ有効です。",
                    "default": {
                        "ダミーサイト": {
                            "bgColor": "#1e3a8a",
                            "textColor": "#ffffff"
                        }
                    },
                    "additionalProperties": {
                        "type": "object",
                        "properties": {
                            "bgColor": {
                                "type": "string",
                                "title": "背景色",
                                "default": "#1f2937"
                            },
                            "textColor": {
                                "type": "string",
                                "title": "文字色",
                                "default": "#ffffff"
                            }
                        }
                    }
                }
            },
            "dependencies": {
                "enable_color_by_plugin_or_connection": {
                    "oneOf": [
                        {
                            "properties": {
                                "enable_color_by_plugin_or_connection": { "const": false }
                            }
                        },
                        {
                            "properties": {
                                "enable_color_by_plugin_or_connection": { "const": true },
                                "color_mode": {
                                    "type": "string",
                                    "enum": ["site", "connection"]
                                }
                            },
                            "dependencies": {
                                "color_mode": {
                                    "oneOf": [
                                        {
                                            "properties": {
                                                "color_mode": { "const": "connection" }
                                            }
                                        },
                                        {
                                            "properties": {
                                                "color_mode": { "const": "site" },
                                                "site_colors": {
                                                    "type": "object"
                                                }
                                            }
                                        }
                                    ]
                                }
                            }
                        }
                    ]
                }
            }
        })
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

fn handle_core_request_message(
    core: &mut CoreActor,
    message: &McvMessage,
    ctx: &mut Context<CoreActor>,
    response_mode: bool,
) -> Result<McvMessage, String> {
    // UI向けのrequestハンドラ。Plugin起点requestはhandle_plugin_request_message()で扱う。

    // ① レスポンスが必要なメッセージ（early return）
    match message.message_type {
        MessageType::GetSettingsSchema => {
            return message_handlers::settings::handle_get_settings_schema(core, message, ctx);
        }
        MessageType::GetSettings => {
            return message_handlers::settings::handle_get_settings(core, message, ctx);
        }
        MessageType::UpdateSettings => {
            return message_handlers::settings::handle_update_settings(core, message, ctx);
        }
        MessageType::GetPlugins => {
            let plugins = core
                .logical_plugins
                .values()
                .map(|info| PluginAddedPayload {
                    name: info.name.clone(),
                    plugin_id: info.logical_plugin_id.inner(),
                    role: info.role.clone(),
                    api_version: info.api_version.clone(),
                })
                .collect::<Vec<_>>();

            return Ok(message.create_response(
                MessageType::GetPlugins,
                serde_json::to_value(GetPluginsPayload { plugins }).unwrap(),
            ));
        }
        _ => {}
    }

    // ② UIとプラグインで共通の通知メッセージ
    if message_handlers::handle_common_notification(core, message, ctx) {
        return if response_mode {
            Ok(message.create_response(message.message_type.clone(), serde_json::json!({})))
        } else {
            Ok(message.clone())
        };
    }

    // ③ 未処理
    if response_mode {
        return Err(format!(
            "Unsupported plugin request message type: {:?}",
            message.message_type
        ));
    }
    tracing::warn!(
        target: "mcv::core::CoreActor",
        message_type = ?message.message_type,
        "Unhandled UI message type"
    );
    Ok(message.clone())
}

fn is_supported_plugin_request_type(message_type: &MessageType) -> bool {
    matches!(
        message_type,
        MessageType::PluginHello
            | MessageType::AddSite
            | MessageType::AddBrowser
            | MessageType::GetPlugins
            | MessageType::GetBrowserPlugin
    )
}

fn handle_plugin_request_message(
    core: &mut CoreActor,
    physical_plugin_id: PhysicalPluginId,
    message: &McvMessage,
    ctx: &mut Context<CoreActor>,
) -> Result<McvMessage, String> {
    match message.message_type {
        MessageType::PluginHello => {
            message_handlers::plugin_hello::handle_plugin_hello(
                core,
                physical_plugin_id,
                message,
                ctx,
            )?;
            let payload: PluginHelloPayload = serde_json::from_value(message.payload.clone())
                .map_err(|e| format!("Failed to parse PluginHello payload for ack: {}", e))?;
            Ok(message.create_response(
                MessageType::PluginHelloAck,
                serde_json::to_value(PluginHelloAckPayload {
                    plugin_id: payload.plugin_id,
                })
                .unwrap(),
            ))
        }
        MessageType::AddSite => {
            let payload: AddSitePayload = serde_json::from_value(message.payload.clone())
                .map_err(|e| format!("Failed to parse AddSite payload for ack: {}", e))?;
            message_handlers::site_browser::handle_add_site(core, message, ctx);
            Ok(message.create_response(
                MessageType::AddSiteAck,
                serde_json::to_value(AddSiteAckPayload {
                    site_id: payload.site_id,
                })
                .unwrap(),
            ))
        }
        MessageType::AddBrowser => {
            let payload: AddBrowserPayload = serde_json::from_value(message.payload.clone())
                .map_err(|e| format!("Failed to parse AddBrowser payload for ack: {}", e))?;
            message_handlers::site_browser::handle_add_browser(core, message, ctx);
            Ok(message.create_response(
                MessageType::AddBrowserAck,
                serde_json::to_value(AddBrowserAckPayload {
                    browser_id: payload.browser_id,
                })
                .unwrap(),
            ))
        }
        MessageType::GetPlugins => {
            let plugins = core
                .logical_plugins
                .values()
                .map(|info| PluginAddedPayload {
                    name: info.name.clone(),
                    plugin_id: info.logical_plugin_id.inner(),
                    role: info.role.clone(),
                    api_version: info.api_version.clone(),
                })
                .collect::<Vec<_>>();
            Ok(message.create_response(
                MessageType::GetPlugins,
                serde_json::to_value(GetPluginsPayload { plugins }).unwrap(),
            ))
        }
        MessageType::GetBrowserPlugin => {
            let payload: GetBrowserPluginPayload = serde_json::from_value(message.payload.clone())
                .map_err(|e| format!("Failed to parse GetBrowserPlugin payload: {}", e))?;

            let browser = core
                .site_browser_manager
                .get_browser(&payload.browser_id)
                .ok_or_else(|| format!("Browser not found: {}", payload.browser_id))?;

            Ok(message.create_response(
                MessageType::GetBrowserPluginAck,
                serde_json::to_value(GetBrowserPluginAckPayload {
                    browser_id: payload.browser_id,
                    plugin_id: browser.plugin_id,
                })
                .unwrap(),
            ))
        }
        _ => Err(format!(
            "Unsupported plugin request message type: {:?}",
            message.message_type
        )),
    }
}

fn send_response_to_plugin(
    core: &CoreActor,
    plugin_id: Uuid,
    physical_plugin_id: PhysicalPluginId,
    response: McvMessage,
) {
    let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id);
    if let Some(plugin_info) = core.logical_plugins.get(&logical_plugin_id) {
        plugin_info
            .host_addr
            .send_plugin_message(crate::plugin_host_actor::SendMessageToPlugin {
                message: response,
            });
    } else if let Some(host_addr) = core.physical_plugin_hosts.get(&physical_plugin_id) {
        host_addr.send_plugin_message(crate::plugin_host_actor::SendMessageToPlugin {
            message: response,
        });
    } else {
        tracing::error!(
            target: "mcv::core::CoreActor",
            plugin_id = %plugin_id,
            physical_plugin_id = %physical_plugin_id,
            response = ?response,
            "Failed to route response to plugin (logical and physical plugin not found)"
        );
    }
}

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

        if matches!(message.src, MessageSource::Plugin { .. })
            && matches!(message.dst, MessageDestination::Plugin { .. })
        {
            //プラグイン間通信

            let destination_plugin_id = match message.dst {
                MessageDestination::Plugin { plugin_id } => plugin_id,
                _ => unreachable!(),
            };

            // "なし" ブラウザ宛ての GetCookie を intercept して空クッキーを返す
            if destination_plugin_id == crate::site_browser_manager::NONE_BROWSER_PLUGIN_ID
                && message.message_type == MessageType::GetCookie
            {
                let src_plugin_id = match message.src {
                    MessageSource::Plugin { plugin_id } => plugin_id,
                    _ => unreachable!(),
                };
                let response = message.create_response(
                    MessageType::GetCookieAck,
                    serde_json::to_value(mcv_messages::GetCookieAckPayload {
                        cookies: vec![],
                    })
                    .unwrap_or_default(),
                );
                tracing::debug!(
                    target: "mcv::core::CoreActor",
                    src_plugin_id = %src_plugin_id,
                    "GetCookie to NONE_BROWSER intercepted; returning empty cookies"
                );
                send_response_to_plugin(self, src_plugin_id, physical_plugin_id, response);
                return;
            }

            let destination_logical_id = LogicalPluginId::from_uuid(destination_plugin_id);
            if let Some(plugin_info) = self.logical_plugins.get(&destination_logical_id) {
                plugin_info
                    .host_addr
                    .do_send(crate::plugin_host_actor::SendMessageToPlugin {
                        message: message.clone(),
                    });
            } else {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    destination_plugin_id = %destination_plugin_id,
                    message_type = ?message.message_type,
                    "Failed to route plugin-to-plugin message (destination not found)"
                );
            }
            return;
        }

        if message.request_id.is_some()
            && matches!(message.dst, MessageDestination::Core)
            && matches!(message.src, MessageSource::Plugin { .. })
        {
            // pluginからのrequest_id付きメッセージはrequest/responseとして処理する。

            let requester = match message.src {
                MessageSource::Plugin { plugin_id } => plugin_id,
                MessageSource::Core => {
                    tracing::warn!(
                        target: "mcv::core::CoreActor",
                        "Unexpected core source for plugin request branch"
                    );
                    return;
                }
            };

            let response = match if is_supported_plugin_request_type(&message.message_type) {
                handle_plugin_request_message(self, physical_plugin_id, &message, ctx)
            } else {
                Err(format!(
                    "Unsupported plugin request message type: {:?}",
                    message.message_type
                ))
            } {
                Ok(resp) => resp,
                Err(err) => {
                    // 待機側をハングさせないため、失敗時も必ず同一request_idで応答する。
                    message.create_response(
                        MessageType::PluginError,
                        serde_json::json!({ "reason": err }),
                    )
                }
            };

            send_response_to_plugin(self, requester, physical_plugin_id, response);
            return;
        }

        // プラグイン固有メッセージ（plugin-hello、get-plugins 等）
        match message.message_type {
            MessageType::PluginHello => {
                if let Err(err) = message_handlers::plugin_hello::handle_plugin_hello(
                    self,
                    physical_plugin_id,
                    &message,
                    ctx,
                ) {
                    tracing::error!(
                        target: "mcv::core::CoreActor",
                        physical_plugin_id = %physical_plugin_id,
                        error = %err,
                        "Failed to handle plugin-hello"
                    );
                }
                return;
            }
            MessageType::GetPlugins => {
                message_handlers::plugin_hello::handle_get_plugins(
                    self,
                    physical_plugin_id,
                    &message,
                    ctx,
                );
                return;
            }
            MessageType::LogEntry => {
                message_handlers::comment::handle_log_entry(self, &message, ctx);
                return;
            }
            MessageType::SettingsSchema => {
                // プラグインから受信したスキーマをキャッシュして、UIにも転送
                if let Ok(payload) =
                    serde_json::from_value::<SettingsSchemaPayload>(message.payload.clone())
                {
                    if let Ok(plugin_uuid) = Uuid::parse_str(&payload.target) {
                        let logical_id = LogicalPluginId::from_uuid(plugin_uuid);
                        if let Some(info) = self.logical_plugins.get_mut(&logical_id) {
                            info.settings_schema = Some(payload.schema);
                        }
                    }
                }
                if let Some(ref callback) = self.event_callback {
                    callback(message.clone());
                }
                return;
            }
            MessageType::SettingsData => {
                // プラグインから受信した設定データをキャッシュして、UIにも転送
                if let Ok(payload) =
                    serde_json::from_value::<SettingsDataPayload>(message.payload.clone())
                {
                    if let Ok(plugin_uuid) = Uuid::parse_str(&payload.target) {
                        let logical_id = LogicalPluginId::from_uuid(plugin_uuid);
                        if let Some(info) = self.logical_plugins.get_mut(&logical_id) {
                            info.settings_data = Some(payload.data);
                        }
                    }
                }
                if let Some(ref callback) = self.event_callback {
                    callback(message.clone());
                }
                return;
            }
            MessageType::GetSettingsSchema => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    "GetSettingsSchema from plugin is not supported yet"
                );
                return;
            }
            MessageType::GetSettings => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    "GetSettings from plugin is not supported yet"
                );
                return;
            }
            MessageType::UpdateSettings => {
                tracing::warn!(
                    target: "mcv::core::CoreActor",
                    "UpdateSettings from plugin is not supported yet"
                );
                return;
            }
            _ => {}
        }

        // UIとプラグインで共通の通知メッセージ
        if message_handlers::handle_common_notification(self, &message, ctx) {
            return;
        }

        tracing::warn!(
            target: "mcv::core::CoreActor",
            message_type = ?message.message_type,
            "Unhandled message type"
        );
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
        handle_core_request_message(self, &msg.message, ctx, false)
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

/// プラグイン一覧を取得
#[derive(Message)]
#[rtype(result = "Vec<PluginInfo>")]
pub struct GetLogicalPlugins;

impl Handler<GetLogicalPlugins> for CoreActor {
    type Result = Vec<PluginInfo>;

    fn handle(&mut self, _msg: GetLogicalPlugins, _ctx: &mut Self::Context) -> Self::Result {
        self.logical_plugins.values().cloned().collect()
    }
}

/// 接続を作成（UI主導）
#[derive(Message)]
#[rtype(result = "Uuid")]
pub struct CreateConnection {
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

        // 保存
        let _ = self.save_connections();

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

        // 保存
        let _ = self.save_connections();

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
    pub site_id: SiteId,
}

impl Handler<SetConnectionSite> for CoreActor {
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: SetConnectionSite, ctx: &mut Context<Self>) -> Self::Result {
        let message = McvMessage::new_notification(
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
    pub browser_id: Option<BrowserId>,
    pub advanced_settings: Option<serde_json::Value>,
}

impl Handler<UpdateConnectionSettings> for CoreActor {
    type Result = ResponseActFuture<Self, Result<(), String>>;

    fn handle(&mut self, msg: UpdateConnectionSettings, ctx: &mut Context<Self>) -> Self::Result {
        let message = McvMessage::new_notification(
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

/// テスト専用：論理プラグインを直接登録するメッセージ。
///
/// 外部テストクレート（`crates/mcv-core/tests/`）から CoreActor に
/// テスト用の論理プラグインを注入するために使用する。
#[cfg(test)]
#[derive(Message)]
#[rtype(result = "()")]
pub struct RegisterTestLogicalPlugin {
    pub logical_plugin_id: LogicalPluginId,
    pub physical_plugin_id: PhysicalPluginId,
    pub host_addr: crate::plugin_loader_strategy::PluginHostAddr,
}

#[cfg(test)]
impl Handler<RegisterTestLogicalPlugin> for CoreActor {
    type Result = ();

    fn handle(&mut self, msg: RegisterTestLogicalPlugin, _ctx: &mut Self::Context) {
        let info = LogicalPluginInfo {
            logical_plugin_id: msg.logical_plugin_id,
            physical_plugin_id: msg.physical_plugin_id,
            name: "test-plugin".to_string(),
            role: vec![],
            api_version: "test".to_string(),
            host_addr: msg.host_addr,
            settings_schema: None,
            settings_data: None,
        };
        self.logical_plugins.insert(msg.logical_plugin_id, info);
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

    // ---- なし ブラウザ GetCookie intercept テスト ----

    /// テスト専用プラグインアクター（受信メッセージをキャプチャする）
    struct TestPluginActor {
        received: std::sync::Arc<tokio::sync::Mutex<Vec<McvMessage>>>,
    }

    impl actix::Actor for TestPluginActor {
        type Context = actix::Context<Self>;
    }

    impl actix::Handler<crate::plugin_host_actor::SendMessageToPlugin> for TestPluginActor {
        type Result = ();
        fn handle(
            &mut self,
            msg: crate::plugin_host_actor::SendMessageToPlugin,
            _ctx: &mut Self::Context,
        ) {
            let received = self.received.clone();
            actix::spawn(async move {
                received.lock().await.push(msg.message);
            });
        }
    }

    /// なし ブラウザ宛て GetCookie を送ったとき GetCookieAck { cookies: [] } が返ること。
    ///
    /// **修正前はこのテストが失敗する**（CoreActor に intercept ロジックがないため
    /// メッセージが TestPluginActor に届かない）。
    #[actix::test]
    async fn test_none_browser_get_cookie_returns_empty_ack() {
        use crate::internal_message::InternalMessage;
        use crate::plugin_loader_strategy::PluginHostAddr;
        use crate::site_browser_manager::NONE_BROWSER_PLUGIN_ID;
        use mcv_common::PhysicalPluginId;
        use mcv_messages::{
            GetCookieAckPayload, GetCookiePayload, MessageDestination, MessageSource, MessageType,
        };
        use std::sync::Arc;
        use std::time::Duration;
        use tokio::sync::Mutex;
        use uuid::Uuid;

        // CoreActor 起動
        let core_actor = CoreActor::new();
        let core_addr = core_actor.start();

        // テスト用プラグインアクター（Twitch プラグイン相当）
        let received_messages = Arc::new(Mutex::new(Vec::<McvMessage>::new()));
        let test_actor = TestPluginActor {
            received: received_messages.clone(),
        };
        let test_actor_addr = test_actor.start();

        // プラグイン ID を決める
        let src_plugin_id = Uuid::new_v4();
        let src_physical_plugin_id = PhysicalPluginId::from_uuid(Uuid::new_v4());
        let logical_id = LogicalPluginId::from_uuid(src_plugin_id);

        // テスト用プラグインを論理プラグインとして CoreActor に登録
        let host_addr = PluginHostAddr::Test(
            test_actor_addr
                .recipient::<crate::plugin_host_actor::SendMessageToPlugin>(),
        );
        core_addr
            .send(RegisterTestLogicalPlugin {
                logical_plugin_id: logical_id,
                physical_plugin_id: src_physical_plugin_id,
                host_addr,
            })
            .await
            .unwrap();

        // NONE_BROWSER_PLUGIN_ID は crate 内部定数として直接使用する
        // （GetBrowserPlugin 経由で取得する必要なし）

        // plugin-to-plugin GetCookie を SendMessageToCore 経由で送る
        let get_cookie_msg = McvMessage::new_request(
            MessageType::GetCookie,
            MessageSource::Plugin {
                plugin_id: src_plugin_id,
            },
            MessageDestination::Plugin {
                plugin_id: NONE_BROWSER_PLUGIN_ID,
            },
            serde_json::to_value(GetCookiePayload {
                browser_id: crate::site_browser_manager::none_browser_id(),
                domain: "twitch.tv".to_string(),
            })
            .unwrap(),
        );
        core_addr.do_send(SendMessageToCore {
            internal_message: InternalMessage {
                physical_plugin_id: src_physical_plugin_id,
                message: get_cookie_msg,
            },
        });

        // CoreActor がメッセージを処理するまで待機
        tokio::time::sleep(Duration::from_millis(200)).await;

        // GetCookieAck { cookies: [] } が届いていること
        let messages = received_messages.lock().await;
        assert_eq!(
            messages.len(),
            1,
            "GetCookieAck が 1 件届くべき（実際: {} 件）",
            messages.len()
        );
        assert_eq!(
            messages[0].message_type,
            MessageType::GetCookieAck,
            "返ってくるメッセージは GetCookieAck であるべき（実際: {:?}）",
            messages[0].message_type
        );
        let payload: GetCookieAckPayload =
            serde_json::from_value(messages[0].payload.clone()).unwrap();
        assert!(
            payload.cookies.is_empty(),
            "なし ブラウザの cookies は空であるべき（実際: {:?}）",
            payload.cookies
        );
    }
}
