pub use mcv_common::SiteId;
use serde::{Deserialize, Serialize};
use uuid::Uuid;

/// メッセージヘッダー
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Message {
    #[serde(rename = "type")]
    pub message_type: MessageType,
    pub src: MessageSource,
    pub dst: MessageDestination,
    pub request_id: Option<Uuid>,
    pub timestamp: i64,
    pub payload: serde_json::Value,
}

/// メッセージの送信元
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum MessageSource {
    Core,
    Plugin { plugin_id: Uuid },
}

impl Serialize for MessageSource {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        match self {
            MessageSource::Core => serializer.serialize_str("core"),
            MessageSource::Plugin { plugin_id } => serializer.serialize_str(&plugin_id.to_string()),
        }
    }
}

impl<'de> Deserialize<'de> for MessageSource {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
    where
        D: serde::Deserializer<'de>,
    {
        let s = String::deserialize(deserializer)?;
        if s == "core" {
            Ok(MessageSource::Core)
        } else {
            let plugin_id = Uuid::parse_str(&s)
                .map_err(|_| serde::de::Error::custom(format!("Invalid UUID: {}", s)))?;
            Ok(MessageSource::Plugin { plugin_id })
        }
    }
}

impl MessageSource {
    /// MessageSourceをMessageDestinationに変換
    pub fn to_destination(&self) -> MessageDestination {
        match self {
            MessageSource::Core => MessageDestination::Core,
            MessageSource::Plugin { plugin_id } => MessageDestination::Plugin {
                plugin_id: *plugin_id,
            },
        }
    }
}

/// メッセージの宛先
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum MessageDestination {
    Core,
    Plugin { plugin_id: Uuid },
    Broadcast,
}

impl Serialize for MessageDestination {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        match self {
            MessageDestination::Core => serializer.serialize_str("core"),
            MessageDestination::Plugin { plugin_id } => serializer.serialize_str(&plugin_id.to_string()),
            MessageDestination::Broadcast => serializer.serialize_str("broadcast"),
        }
    }
}

impl<'de> Deserialize<'de> for MessageDestination {
    fn deserialize<D>(deserializer: D) -> Result<Self, D::Error>
    where
        D: serde::Deserializer<'de>,
    {
        let s = String::deserialize(deserializer)?;
        match s.as_str() {
            "core" => Ok(MessageDestination::Core),
            "broadcast" => Ok(MessageDestination::Broadcast),
            _ => {
                let plugin_id = Uuid::parse_str(&s)
                    .map_err(|_| serde::de::Error::custom(format!("Invalid UUID: {}", s)))?;
                Ok(MessageDestination::Plugin { plugin_id })
            }
        }
    }
}

impl MessageDestination {
    /// MessageDestinationをMessageSourceに変換
    /// 注意: Broadcastの場合はCoreに変換されます（ブロードキャストメッセージへの直接的なレスポンスは想定されていません）
    pub fn to_source(&self) -> MessageSource {
        match self {
            MessageDestination::Core => MessageSource::Core,
            MessageDestination::Plugin { plugin_id } => MessageSource::Plugin {
                plugin_id: *plugin_id,
            },
            MessageDestination::Broadcast => MessageSource::Core,
        }
    }
}

/// メッセージ種別（kebab-case）
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "kebab-case")]
pub enum MessageType {
    // Plugin関連
    PluginHello,
    PluginAdded,
    PluginRemoved,
    PluginError,
    GetPlugins,

    // Connection関連
    AddConnection,
    ConnectionAdded,
    ConnectionAddFailed,
    RemoveConnection,
    ConnectionRemoved,
    ConnectionRemoveFailed,
    Connect,
    Connected,
    ConnectFailed,
    Disconnect,
    Disconnected,
    DisconnectFailed,
    GetConnectionStatus,

    // Comment関連
    CommentReceived,
    SendComment,

    // Logging関連
    LogEntry,

    // その他
    GetAppName,
    GetAppVersion,

    // Site/Browser管理関連
    AddSite,
    AddBrowser,
    SetConnectionSite,
    DiscardConnectionSite,
    UpdateConnectionSettings,

    // Settings関連
    GetSettingsSchema,
    SettingsSchema,
    GetSettings,
    SettingsData,
    UpdateSettings,
}

// ============================================================================
// Payload型定義
// ============================================================================

/// plugin-helloのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginHelloPayload {
    pub name: String,
    pub plugin_id: Uuid,
    pub role: Vec<String>,
    pub api_version: String,
}

/// plugin-addedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginAddedPayload {
    pub name: String,
    pub plugin_id: Uuid,
    pub role: Vec<String>,
    pub api_version: String,
}

/// add-connectionのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddConnectionPayload {
}

/// connection-addedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionAddedPayload {
    pub connection_id: Uuid,
    pub name: String,
}

/// connection-add-failedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionAddFailedPayload {
    pub reason: String,
}

/// connection-removedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionRemovedPayload {
    pub connection_id: Uuid,
}

/// connection-remove-failedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionRemoveFailedPayload {
    pub connection_id: Uuid,
    pub reason: String,
}

/// connectのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectPayload {
    pub connection_id: Uuid,
    pub site: SiteInfo,
    pub input: InputInfo,
    pub browser: BrowserInfo,
}

/// 配信サイト情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SiteInfo {
    pub name: String,
    pub id: SiteId,
}

/// 入力情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct InputInfo {
    pub input_type: String,
    #[serde(flatten)]
    pub extra: serde_json::Value,
}

/// ブラウザ情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BrowserInfo {
    pub name: String,
    pub id: Uuid,
}

/// connectedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectedPayload {
    pub connection_id: Uuid,
}

/// connect-failedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectFailedPayload {
    pub connection_id: Uuid,
    pub reason: String,
}

/// disconnectのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DisconnectPayload {
    pub connection_id: Uuid,
}

/// disconnectedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DisconnectedPayload {
    pub connection_id: Uuid,
}

/// disconnect-failedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DisconnectFailedPayload {
    pub connection_id: Uuid,
    pub reason: String,
}

/// get-connection-statusのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetConnectionStatusPayload {
    pub connection_id: Uuid,
}

/// comment-receivedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CommentReceivedPayload {
    pub connection_id: Uuid,
    pub comment: Comment,
}

/// メッセージパーツ（テキストまたは画像）
#[derive(Debug, Clone, PartialEq, Serialize, Deserialize)]
#[serde(tag = "type")]
pub enum MessagePart {
    #[serde(rename = "text")]
    Text { text: String },
    #[serde(rename = "image")]
    Image {
        url: String,
        #[serde(skip_serializing_if = "Option::is_none")]
        width: Option<u32>,
        #[serde(skip_serializing_if = "Option::is_none")]
        height: Option<u32>,
        #[serde(skip_serializing_if = "Option::is_none")]
        alt: Option<String>,
    },
}

/// コメント情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Comment {
    pub id: String,
    pub user_name: Vec<MessagePart>,
    pub user_id: String,
    pub text: Vec<MessagePart>,
    pub timestamp: i64,
}

/// send-commentのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SendCommentPayload {
    pub connection_id: Uuid,
    pub text: String,
}

/// log-entryのpayload（プラグインからCoreへログ送信）
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LogEntryPayload {
    /// ログレベル（"trace", "debug", "info", "warn", "error"）
    pub level: String,

    /// ログメッセージ
    pub message: String,

    /// コンテキスト情報（任意の構造化データ）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub context: Option<serde_json::Value>,

    /// 関連する接続ID（オプション）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub connection_id: Option<Uuid>,

    /// プラグインのバージョン（オプション）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub plugin_version: Option<String>,

    /// プラグインのビルドプロファイル（"alpha", "beta", "stable"、オプション）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub plugin_build_profile: Option<String>,
}

/// add-siteのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddSitePayload {
    pub site_id: SiteId,
    pub display_name: String,
    pub options_schema: serde_json::Value,
}

/// add-browserのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddBrowserPayload {
    pub browser_id: Uuid,
    pub browser_name: String,
    pub display_name: String,
}

/// set-connection-siteのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SetConnectionSitePayload {
    pub connection_id: Uuid,
    pub site_id: SiteId,
}

/// discard-connection-siteのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DiscardConnectionSitePayload {
    pub connection_id: Uuid,
    pub site_id: SiteId,
}

/// update-connection-settingsのpayload (UI → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UpdateConnectionSettingsPayload {
    pub connection_id: Uuid,
    pub url: Option<String>,
    pub browser_id: Option<Uuid>,
    pub advanced_settings: Option<serde_json::Value>,
}

/// get-settings-schemaのpayload (UI → Core/Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetSettingsSchemaPayload {
    pub target: String, // "core" または plugin_id (UUID文字列)
}

/// settings-schemaのpayload (Core/Plugin → UI)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SettingsSchemaPayload {
    pub target: String,
    pub schema: serde_json::Value,
}

/// get-settingsのpayload (UI → Core/Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetSettingsPayload {
    pub target: String,
}

/// settings-dataのpayload (Core/Plugin → UI)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SettingsDataPayload {
    pub target: String,
    pub data: serde_json::Value,
}

/// update-settingsのpayload (UI → Core/Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UpdateSettingsPayload {
    pub target: String,
    pub data: serde_json::Value,
}

// ============================================================================
// ヘルパー関数
// ============================================================================

impl Message {
    /// 新しいメッセージを作成
    pub fn new(
        message_type: MessageType,
        src: MessageSource,
        dst: MessageDestination,
        payload: serde_json::Value,
    ) -> Self {
        Self {
            message_type,
            src,
            dst,
            request_id: Some(Uuid::new_v4()),
            timestamp: chrono::Utc::now().timestamp(),
            payload,
        }
    }

    /// request_idなしのメッセージを作成（通知用）
    pub fn new_notification(
        message_type: MessageType,
        src: MessageSource,
        dst: MessageDestination,
        payload: serde_json::Value,
    ) -> Self {
        Self {
            message_type,
            src,
            dst,
            request_id: None,
            timestamp: chrono::Utc::now().timestamp(),
            payload,
        }
    }

    /// レスポンスメッセージを作成
    pub fn create_response(
        &self,
        message_type: MessageType,
        payload: serde_json::Value,
    ) -> Self {
        Self {
            message_type,
            src: self.dst.to_source(),
            dst: self.src.to_destination(),
            request_id: self.request_id,
            timestamp: chrono::Utc::now().timestamp(),
            payload,
        }
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_message_serialization() {
        let plugin_id = Uuid::new_v4();
        let message = Message::new(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::json!({
                "name": "Test Plugin",
                "plugin_id": plugin_id,
                "role": ["test"],
                "api_version": "v2"
            }),
        );

        let json = serde_json::to_string(&message).unwrap();
        let deserialized: Message = serde_json::from_str(&json).unwrap();

        assert_eq!(message.message_type, deserialized.message_type);
        assert_eq!(message.src, deserialized.src);
        assert_eq!(message.dst, deserialized.dst);
    }

    #[test]
    fn test_comment_payload() {
        let comment = Comment {
            id: "test-id".to_string(),
            user_name: vec![MessagePart::Text {
                text: "太郎".to_string(),
            }],
            user_id: "user_1234".to_string(),
            text: vec![MessagePart::Text {
                text: "こんにちは!".to_string(),
            }],
            timestamp: chrono::Utc::now().timestamp(),
        };

        let payload = CommentReceivedPayload {
            connection_id: Uuid::new_v4(),
            comment,
        };

        let json = serde_json::to_value(&payload).unwrap();
        let deserialized: CommentReceivedPayload = serde_json::from_value(json).unwrap();

        assert_eq!(payload.comment.user_name, deserialized.comment.user_name);
        assert_eq!(payload.comment.text, deserialized.comment.text);
    }

    #[test]
    fn test_send_comment_payload() {
        let connection_id = Uuid::new_v4();
        let payload = SendCommentPayload {
            connection_id,
            text: "テストコメント".to_string(),
        };

        let json = serde_json::to_value(&payload).unwrap();
        let deserialized: SendCommentPayload = serde_json::from_value(json).unwrap();

        assert_eq!(payload.connection_id, deserialized.connection_id);
        assert_eq!(payload.text, deserialized.text);
    }

    #[test]
    fn test_send_comment_message_type() {
        let plugin_id = Uuid::new_v4();
        let connection_id = Uuid::new_v4();

        let message = Message::new(
            MessageType::SendComment,
            MessageSource::Core,
            MessageDestination::Plugin { plugin_id },
            serde_json::to_value(SendCommentPayload {
                connection_id,
                text: "pause".to_string(),
            })
            .unwrap(),
        );

        let json = serde_json::to_string(&message).unwrap();
        let deserialized: Message = serde_json::from_str(&json).unwrap();

        assert_eq!(message.message_type, deserialized.message_type);
        assert_eq!(message.message_type, MessageType::SendComment);
    }

    #[test]
    fn test_message_source_serialization() {
        // Test Core serialization
        let core = MessageSource::Core;
        let json = serde_json::to_value(&core).unwrap();
        assert_eq!(json, serde_json::Value::String("core".to_string()));

        let deserialized: MessageSource = serde_json::from_value(json).unwrap();
        assert_eq!(core, deserialized);

        // Test Plugin serialization
        let plugin_id = Uuid::parse_str("10000000-2000-3000-4000-500000000000").unwrap();
        let plugin = MessageSource::Plugin { plugin_id };
        let json = serde_json::to_value(&plugin).unwrap();
        assert_eq!(json, serde_json::Value::String("10000000-2000-3000-4000-500000000000".to_string()));

        let deserialized: MessageSource = serde_json::from_value(json).unwrap();
        assert_eq!(plugin, deserialized);
    }

    #[test]
    fn test_message_destination_serialization() {
        // Test Core serialization
        let core = MessageDestination::Core;
        let json = serde_json::to_value(&core).unwrap();
        assert_eq!(json, serde_json::Value::String("core".to_string()));

        let deserialized: MessageDestination = serde_json::from_value(json).unwrap();
        assert_eq!(core, deserialized);

        // Test Plugin serialization
        let plugin_id = Uuid::parse_str("10000000-2000-3000-4000-500000000000").unwrap();
        let plugin = MessageDestination::Plugin { plugin_id };
        let json = serde_json::to_value(&plugin).unwrap();
        assert_eq!(json, serde_json::Value::String("10000000-2000-3000-4000-500000000000".to_string()));

        let deserialized: MessageDestination = serde_json::from_value(json).unwrap();
        assert_eq!(plugin, deserialized);

        // Test Broadcast serialization
        let broadcast = MessageDestination::Broadcast;
        let json = serde_json::to_value(&broadcast).unwrap();
        assert_eq!(json, serde_json::Value::String("broadcast".to_string()));

        let deserialized: MessageDestination = serde_json::from_value(json).unwrap();
        assert_eq!(broadcast, deserialized);
    }

    #[test]
    fn test_message_serialization_new_format() {
        let plugin_id = Uuid::parse_str("10000000-2000-3000-4000-500000000000").unwrap();
        let message = Message::new(
            MessageType::PluginHello,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            serde_json::json!({
                "name": "Test Plugin",
                "plugin_id": plugin_id,
                "role": ["test"],
                "api_version": "v2"
            }),
        );

        let json = serde_json::to_string(&message).unwrap();

        // 新しいフォーマットでは "src": "uuid", "dst": "core" となることを確認
        assert!(json.contains(r#""src":"10000000-2000-3000-4000-500000000000""#));
        assert!(json.contains(r#""dst":"core""#));

        // デシリアライズが正常に動作することを確認
        let deserialized: Message = serde_json::from_str(&json).unwrap();
        assert_eq!(message.message_type, deserialized.message_type);
        assert_eq!(message.src, deserialized.src);
        assert_eq!(message.dst, deserialized.dst);
    }
}
