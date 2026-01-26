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
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "kebab-case")]
pub enum MessageSource {
    Core,
    Plugin { plugin_id: Uuid },
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
#[derive(Debug, Clone, Serialize, Deserialize, PartialEq, Eq)]
#[serde(rename_all = "kebab-case")]
pub enum MessageDestination {
    Core,
    Plugin { plugin_id: Uuid },
}

impl MessageDestination {
    /// MessageDestinationをMessageSourceに変換
    pub fn to_source(&self) -> MessageSource {
        match self {
            MessageDestination::Core => MessageSource::Core,
            MessageDestination::Plugin { plugin_id } => MessageSource::Plugin {
                plugin_id: *plugin_id,
            },
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
    pub site: SiteInfo,
}

/// connection-addedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionAddedPayload {
    pub connection_id: Uuid,
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
    pub id: Uuid,
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

/// コメント情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Comment {
    pub id: String,
    pub user_name: String,
    pub user_id: String,
    pub text: String,
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
    pub site_id: Uuid,
    pub site_name: String,
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
    pub site_id: Uuid,
}

/// discard-connection-siteのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct DiscardConnectionSitePayload {
    pub connection_id: Uuid,
    pub site_id: Uuid,
}

/// update-connection-settingsのpayload (UI → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UpdateConnectionSettingsPayload {
    pub connection_id: Uuid,
    pub url: Option<String>,
    pub browser_id: Option<Uuid>,
    pub advanced_settings: Option<serde_json::Value>,
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
            user_name: "太郎".to_string(),
            user_id: "user_1234".to_string(),
            text: "こんにちは!".to_string(),
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
}
