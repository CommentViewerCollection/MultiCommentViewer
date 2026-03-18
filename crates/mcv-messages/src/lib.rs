pub use mcv_common::BrowserId;
pub use mcv_common::PluginId;
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
    Plugin { plugin_id: PluginId },
}

impl Serialize for MessageSource {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        match self {
            MessageSource::Core => serializer.serialize_str("core"),
            MessageSource::Plugin { plugin_id } => serializer.serialize_str(plugin_id.as_str()),
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
            Ok(MessageSource::Plugin {
                plugin_id: PluginId::new(s),
            })
        }
    }
}

impl MessageSource {
    /// MessageSourceをMessageDestinationに変換
    pub fn to_destination(&self) -> MessageDestination {
        match self {
            MessageSource::Core => MessageDestination::Core,
            MessageSource::Plugin { plugin_id } => MessageDestination::Plugin {
                plugin_id: plugin_id.clone(),
            },
        }
    }
}

/// メッセージの宛先
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum MessageDestination {
    Core,
    Plugin { plugin_id: PluginId },
    Broadcast,
}

impl Serialize for MessageDestination {
    fn serialize<S>(&self, serializer: S) -> Result<S::Ok, S::Error>
    where
        S: serde::Serializer,
    {
        match self {
            MessageDestination::Core => serializer.serialize_str("core"),
            MessageDestination::Plugin { plugin_id } => {
                serializer.serialize_str(plugin_id.as_str())
            }
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
            _ => Ok(MessageDestination::Plugin {
                plugin_id: PluginId::new(s),
            }),
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
                plugin_id: plugin_id.clone(),
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
    PluginHelloAck,
    PluginAdded,
    PluginRemoved,
    PluginError,
    GetPlugins,
    GetBrowserPlugin,
    GetBrowserPluginAck,
    GetCookie,
    GetCookieAck,

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
    GetConnectionInputSchema,
    ConnectionInputSchema,

    // Comment関連
    CommentReceived,
    SendComment,
    GetSendCommentSchema,
    SendCommentSchema,
    StreamMetadata,

    // Logging関連
    LogEntry,

    // その他
    GetAppName,
    GetAppVersion,
    GetLogsDir,
    LogsDirAck,
    GetPluginsDir,
    PluginsDirAck,

    // Site/Browser管理関連
    AddSite,
    AddSiteAck,
    AddBrowser,
    AddBrowserAck,
    RemoveBrowser,
    SetConnectionSite,
    DiscardConnectionSite,
    UpdateConnectionSettings,

    // Settings関連
    GetSettingsSchema,
    SettingsSchema,
    GetSettings,
    SettingsData,
    UpdateSettings,

    // Account関連
    UpdateConnectionAccount,
    FetchAccountInfo,

    // URL自動検出関連
    CanHandleUrl,
    CanHandleUrlResult,

    // 接続一覧取得
    GetConnections,
}

// ============================================================================
// Payload型定義
// ============================================================================

/// plugin-helloのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginHelloPayload {
    pub name: String,
    pub plugin_id: PluginId,
    pub role: Vec<String>,
    pub api_version: String,
    /// このプラグインのコメント投稿フォームスキーマ（省略可）。
    /// None の場合は Core のデフォルト（text のみ）を使用する。
    #[serde(default)]
    pub send_comment_schema: Option<serde_json::Value>,
}

/// plugin-hello-ackのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginHelloAckPayload {
    pub plugin_id: PluginId,
}

/// plugin-addedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginAddedPayload {
    pub name: String,
    pub plugin_id: PluginId,
    pub role: Vec<String>,
    pub api_version: String,
}

/// plugin-removedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginRemovedPayload {
    pub plugin_id: PluginId,
}

/// get-plugins(response)のpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetPluginsPayload {
    pub plugins: Vec<PluginAddedPayload>,
}

/// get-connections(response)のpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetConnectionsPayload {
    pub connections: Vec<ConnectionAddedPayload>,
}

/// get-browser-pluginのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetBrowserPluginPayload {
    pub browser_id: BrowserId,
}

/// get-browser-plugin-ackのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetBrowserPluginAckPayload {
    pub browser_id: BrowserId,
    pub plugin_id: PluginId,
}

/// get-cookieのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetCookiePayload {
    pub browser_id: BrowserId,
    pub domain: String,
}

/// cookie情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Cookie {
    pub name: String,
    pub value: String,
    pub domain: String,
    pub path: String,
}

/// get-cookie-ackのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetCookieAckPayload {
    pub cookies: Vec<Cookie>,
}

/// add-connectionのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddConnectionPayload {}

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
    pub id: BrowserId,
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

/// get-connection-input-schemaのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetConnectionInputSchemaPayload {
    pub connection_id: Uuid,
}

/// connection-input-schemaのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionInputSchemaPayload {
    pub connection_id: Uuid,
    pub schema: serde_json::Value,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub ui_schema: Option<serde_json::Value>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub initial_data: Option<serde_json::Value>,
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

// ============================================================================
// ProviderMessage — 正規化済み配信メッセージ（Comment の後継）
// ============================================================================

/// 配信サービスの識別子（例: "twitch", "youtube", "nicolive", "dummy"）
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct ServiceId(pub String);

/// チャンネル識別子（プラットフォーム固有）
#[derive(Debug, Clone, PartialEq, Eq, Hash, Serialize, Deserialize)]
pub struct ChannelId(pub String);

/// 送信者バッジ
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ProviderBadge {
    pub id: String,
    pub name: String,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub image_url: Option<String>,
}

/// 送信者のロール
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(rename_all = "snake_case")]
pub enum UserRole {
    Streamer,
    Moderator,
    Vip,
    Subscriber,
    Member,
    Viewer,
    Staff,
}

/// 送信者情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ProviderSender {
    /// プラットフォーム上のユーザーID（旧 user_id）
    pub id: String,
    /// リッチ表示名（旧 user_name）
    pub display_name: Vec<MessagePart>,
    pub badges: Vec<ProviderBadge>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub role: Option<UserRole>,
    /// ユーザーアイコン画像URL
    #[serde(skip_serializing_if = "Option::is_none")]
    pub avatar_url: Option<String>,
}

impl ProviderSender {
    /// display_name をプレーンテキストとして返す
    pub fn display_name_text(&self) -> String {
        extract_message_parts_text(&self.display_name)
    }
}

/// 投げ銭・サブスク等の金額情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct Money {
    /// ISO 4217 通貨コード (例: "JPY", "USD")
    pub currency: String,
    /// 最小単位での金額 (JPY=1円, USD=セント)
    pub value_minor: i64,
}

/// マネタイズイベント情報（スーパーチャット・メンバーシップ等）
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct MonetaryInfo {
    pub amount: Money,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub tier: Option<String>,
    pub recurring: bool,
}

/// モデレーションアクション
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "action", rename_all = "snake_case")]
pub enum ModerationAction {
    Delete {
        target_message_id: String,
    },
    Timeout {
        target_user_id: String,
        duration_sec: u64,
    },
    Ban {
        target_user_id: String,
    },
}

/// システム通知の種別
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "system_kind", rename_all = "snake_case")]
pub enum SystemKind {
    Notice,
    Subscription,
    Membership,
    MessageUpdate {
        target_message_id: String,
    },
    MessageDelete {
        target_message_id: String,
    },
    ChannelEvent,
    /// 承認待ちコメント（配信者の許可が来るまで非表示）
    Placeholder,
    /// 指定ユーザーのコメントを全て削除（BAN 等）
    MessageDeleteAll {
        user_id: String,
    },
    /// プラットフォームが自動生成するアナウンス（ギフトメンバーシップ受け取り通知など）
    GiftAnnouncement,
}

/// メッセージの種別
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "kind", rename_all = "snake_case")]
pub enum ProviderMessageKind {
    Chat,
    /// 接続直後に取得した過去ログ（バックログ）コメント。
    ///
    /// 表示上はチャットだが、UI は timestamp 差分再生を行わず
    /// 短間隔で一気に表示してよい。
    HistoryChat,
    Monetary(MonetaryInfo),
    Moderation(ModerationAction),
    System(SystemKind),
}

/// メッセージ本文
#[derive(Debug, Clone, Serialize, Deserialize)]
#[serde(tag = "content_type", rename_all = "snake_case")]
pub enum ProviderContent {
    Empty,
    Text { text: Vec<MessagePart> },
}

impl ProviderContent {
    /// テキスト文字列を抽出する（Image の alt も含む）
    pub fn to_plain_text(&self) -> String {
        match self {
            ProviderContent::Empty => String::new(),
            ProviderContent::Text { text } => extract_message_parts_text(text),
        }
    }
}

/// 正規化済み配信メッセージ（1 件分）。
///
/// # 設計
/// [`McvEnvelope`] に 0..N 件格納される。
/// `timestamp` はメッセージの**投稿時刻**（プラットフォーム提供）であり、
/// [`McvEnvelope::received_at`]（Core の受信時刻）とは独立している。
/// 表示・replay 等の時刻基準には `timestamp` を使用すること。
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ProviderMessage {
    /// 内部一意ID
    pub id: String,
    /// プラットフォーム固有のメッセージID
    #[serde(skip_serializing_if = "Option::is_none")]
    pub platform_message_id: Option<String>,
    /// 配信サービス識別子
    pub service: ServiceId,
    /// チャンネル識別子
    pub channel: ChannelId,
    /// 送信者情報
    pub sender: ProviderSender,
    /// メッセージの投稿時刻（Unix タイムスタンプ、秒）。
    ///
    /// プラットフォームが返すメッセージ送信時刻であり、[`McvEnvelope::received_at`]
    /// （Core の受信時刻）とは異なる。
    ///
    /// # 用途
    /// - フロントエンドでの表示順序の決定
    /// - 複数メッセージを 1 件ずつ表示する際のタイミング計算
    ///   （`apps/mcv` の `event_callback` が担当）
    /// - replay 時の元の投稿間隔の再現
    pub timestamp: i64,
    /// メッセージ種別
    pub kind: ProviderMessageKind,
    /// メッセージ本文
    pub content: ProviderContent,
    /// 返信先の platform_message_id
    #[serde(skip_serializing_if = "Option::is_none")]
    pub reply_to: Option<String>,
    /// プラットフォーム固有の追加データ
    pub metadata: serde_json::Value,
}

/// 1 つのプラットフォームイベント（WebSocket フレーム等）を表すコンテナ。
///
/// # 設計
/// - **1 envelope = 1 プラットフォームイベント**（サーバーから届いた生メッセージと 1:1）
/// - 1 イベント中に複数のアクションが含まれることがあるため
///   `messages` フィールドは **0..N 件の [`ProviderMessage`]** を保持する。
///   例: YouTube Live のポーリングレスポンスは複数のチャットアクションを一括で返す。
/// - `received_at` は Core がイベントを受信した時刻であり、各 [`ProviderMessage`] の
///   `timestamp`（ユーザーが投稿した時刻）とは異なる。
/// - `messages` の表示順・表示タイミングは [`ProviderMessage::timestamp`] に従うこと。
///   複数メッセージをまとめて表示するのではなく、timestamp 差分を尊重して
///   1 件ずつ表示する（担当: `apps/mcv` の `event_callback`）。
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct McvEnvelope {
    /// イベント一意ID
    pub event_id: Uuid,
    /// このイベントを受信した接続のID
    pub connection_id: Uuid,
    /// このイベントに含まれる正規化済みメッセージ一覧（0..N 件）。
    ///
    /// 複数プラットフォームでは 1 フレームに複数アクションが含まれるため
    /// N > 1 になる場合がある。表示側では [`ProviderMessage::timestamp`] に
    /// 従い 1 件ずつ時刻順に表示すること。
    pub messages: Vec<ProviderMessage>,
    /// Core がこのイベントを受信した時刻（Unix タイムスタンプ、秒）。
    ///
    /// 各メッセージの投稿時刻は [`ProviderMessage::timestamp`] を参照すること。
    /// replay 時はエンベロープ送信間隔の基準として使用する。
    pub received_at: i64,
    /// プラットフォームからの生データ（デバッグ・再処理用）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub raw_message: Option<String>,
}

/// comment-receivedのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CommentReceivedPayload {
    /// 後方互換のために残す（envelope.connection_id と同値）
    pub connection_id: Uuid,
    pub envelope: McvEnvelope,
}

// ============================================================================
// ヘルパー関数（プライベート）
// ============================================================================

fn extract_message_parts_text(parts: &[MessagePart]) -> String {
    parts
        .iter()
        .filter_map(|p| match p {
            MessagePart::Text { text } => Some(text.as_str()),
            MessagePart::Image { alt, .. } => alt.as_deref(),
        })
        .collect::<Vec<_>>()
        .join("")
}

/// send-commentのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SendCommentPayload {
    pub connection_id: Uuid,
    pub text: String,
    /// プラットフォーム固有の追加入力（emote / visibility / metadata など）
    #[serde(default, skip_serializing_if = "serde_json::Value::is_null")]
    pub extra: serde_json::Value,
}

/// get-send-comment-schemaのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetSendCommentSchemaPayload {
    pub connection_id: Uuid,
}

/// send-comment-schemaのpayload
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SendCommentSchemaPayload {
    pub connection_id: Uuid,
    pub schema: serde_json::Value,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub ui_schema: Option<serde_json::Value>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub initial_data: Option<serde_json::Value>,
}

/// stream-metadata のペイロード（Plugin → Core → UI）
///
/// 全フィールドは Option — None の場合はフロントエンドで "-" 表示。
/// `others` にプラグインが任意の表示文字列を設定できる。
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StreamMetadataPayload {
    pub connection_id: Uuid,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub title: Option<String>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub viewer_count: Option<u64>,
    #[serde(skip_serializing_if = "Option::is_none")]
    pub total_viewer_count: Option<u64>,
    /// 配信開始時刻（Unix 秒）。フロントエンドが経過時間を計算する。
    #[serde(skip_serializing_if = "Option::is_none")]
    pub start_time: Option<i64>,
    /// その他：プラグインが任意の表示文字列を設定する。
    #[serde(skip_serializing_if = "Option::is_none")]
    pub others: Option<String>,
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

/// get-logs-dirのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetLogsDirPayload {}

/// logs-dir-ackのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LogsDirAckPayload {
    pub path: String,
}

/// get-plugins-dirのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct GetPluginsDirPayload {}

/// plugins-dir-ackのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PluginsDirAckPayload {
    pub path: String,
}

/// add-siteのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddSitePayload {
    pub site_id: SiteId,
    pub display_name: String,
    pub options_schema: serde_json::Value,
}

/// add-site-ackのpayload (Core -> Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddSiteAckPayload {
    pub site_id: SiteId,
}

/// add-browserのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddBrowserPayload {
    pub browser_id: BrowserId,
    pub browser_name: String,
    pub display_name: String,
}

/// add-browser-ackのpayload (Core -> Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AddBrowserAckPayload {
    pub browser_id: BrowserId,
}

/// remove-browserのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RemoveBrowserPayload {
    pub browser_id: BrowserId,
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
    pub browser_id: Option<BrowserId>,
    pub advanced_settings: Option<serde_json::Value>,
    /// URL以外のサイト固有入力値（例: `{"password": "..."}` ）。
    /// 既存の input_state に対してマージされる。
    #[serde(default)]
    pub input_state: Option<serde_json::Value>,
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

/// ログイン中のアカウント情報（プラットフォーム非依存）
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AccountInfo {
    /// プラットフォーム上のユーザーID
    pub user_id: String,
    /// 表示名
    pub display_name: String,
    /// アバター画像URL（取得できない場合はNone）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub avatar_url: Option<String>,
}

/// fetch-account-infoのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct FetchAccountInfoPayload {
    pub connection_id: Uuid,
    pub browser: BrowserInfo,
}

/// update-connection-accountのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct UpdateConnectionAccountPayload {
    pub connection_id: Uuid,
    /// Noneの場合はアカウント情報をクリアする（切断時など）
    pub account: Option<AccountInfo>,
}

/// can-handle-urlのpayload (Core → Plugin)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CanHandleUrlPayload {
    pub url: String,
}

/// can-handle-url-resultのpayload (Plugin → Core)
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CanHandleUrlResultPayload {
    pub supported: bool,
    pub site_id: Option<SiteId>,
}

// ============================================================================
// ヘルパー関数
// ============================================================================

impl Message {
    /// request_idありのメッセージを作成（request用）
    pub fn new_request(
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
    pub fn create_response(&self, message_type: MessageType, payload: serde_json::Value) -> Self {
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
        let plugin_id = PluginId::new("TestPlugin_logical_550e8400-e29b-41d4-a716-446655440000");
        let message = Message::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::json!({
                "name": "Test Plugin",
                "plugin_id": plugin_id.as_str(),
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
        let connection_id = Uuid::new_v4();
        let msg = ProviderMessage {
            id: Uuid::new_v4().to_string(),
            platform_message_id: None,
            service: ServiceId("dummy".to_string()),
            channel: ChannelId("test".to_string()),
            sender: ProviderSender {
                id: "user_1234".to_string(),
                display_name: vec![MessagePart::Text {
                    text: "太郎".to_string(),
                }],
                badges: vec![],
                role: None,
                avatar_url: None,
            },
            timestamp: chrono::Utc::now().timestamp(),
            kind: ProviderMessageKind::Chat,
            content: ProviderContent::Text {
                text: vec![MessagePart::Text {
                    text: "こんにちは!".to_string(),
                }],
            },
            reply_to: None,
            metadata: serde_json::Value::Null,
        };

        let envelope = McvEnvelope {
            event_id: Uuid::new_v4(),
            connection_id,
            messages: vec![msg],
            received_at: chrono::Utc::now().timestamp(),
            raw_message: None,
        };

        let payload = CommentReceivedPayload {
            connection_id,
            envelope,
        };

        let json = serde_json::to_value(&payload).unwrap();
        let deserialized: CommentReceivedPayload = serde_json::from_value(json).unwrap();

        let orig_msg = &payload.envelope.messages[0];
        let deser_msg = &deserialized.envelope.messages[0];
        assert_eq!(orig_msg.sender.display_name, deser_msg.sender.display_name);
        assert_eq!(
            orig_msg.content.to_plain_text(),
            deser_msg.content.to_plain_text()
        );
    }

    #[test]
    fn test_send_comment_payload() {
        let connection_id = Uuid::new_v4();
        let payload = SendCommentPayload {
            connection_id,
            text: "テストコメント".to_string(),
            extra: serde_json::Value::Null,
        };

        let json = serde_json::to_value(&payload).unwrap();
        let deserialized: SendCommentPayload = serde_json::from_value(json).unwrap();

        assert_eq!(payload.connection_id, deserialized.connection_id);
        assert_eq!(payload.text, deserialized.text);
    }

    #[test]
    fn test_provider_message_kind_history_chat_serialization() {
        let kind = ProviderMessageKind::HistoryChat;
        let json = serde_json::to_value(&kind).unwrap();
        assert_eq!(json, serde_json::json!({ "kind": "history_chat" }));

        let deserialized: ProviderMessageKind = serde_json::from_value(json).unwrap();
        assert!(matches!(deserialized, ProviderMessageKind::HistoryChat));
    }

    #[test]
    fn test_send_comment_message_type() {
        let plugin_id = Uuid::new_v4();
        let connection_id = Uuid::new_v4();

        let message = Message::new_request(
            MessageType::SendComment,
            MessageSource::Core,
            MessageDestination::Plugin {
                plugin_id: PluginId::new(plugin_id.to_string()),
            },
            serde_json::to_value(SendCommentPayload {
                connection_id,
                text: "pause".to_string(),
                extra: serde_json::Value::Null,
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
        let plugin_id = PluginId::new("TestPlugin_logical_10000000-2000-3000-4000-500000000000");
        let plugin = MessageSource::Plugin { plugin_id };
        let json = serde_json::to_value(&plugin).unwrap();
        assert_eq!(
            json,
            serde_json::Value::String(
                "TestPlugin_logical_10000000-2000-3000-4000-500000000000".to_string()
            )
        );

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
        let plugin_id = PluginId::new("TestPlugin_logical_10000000-2000-3000-4000-500000000000");
        let plugin = MessageDestination::Plugin { plugin_id };
        let json = serde_json::to_value(&plugin).unwrap();
        assert_eq!(
            json,
            serde_json::Value::String(
                "TestPlugin_logical_10000000-2000-3000-4000-500000000000".to_string()
            )
        );

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
        let plugin_id = PluginId::new("TestPlugin_logical_10000000-2000-3000-4000-500000000000");
        let message = Message::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::json!({
                "name": "Test Plugin",
                "plugin_id": plugin_id.as_str(),
                "role": ["test"],
                "api_version": "v2"
            }),
        );

        let json = serde_json::to_string(&message).unwrap();

        // PluginId フォーマットでは "src": "{name}_logical_{uuid}", "dst": "core" となることを確認
        assert!(json.contains(r#""src":"TestPlugin_logical_10000000-2000-3000-4000-500000000000""#));
        assert!(json.contains(r#""dst":"core""#));

        // デシリアライズが正常に動作することを確認
        let deserialized: Message = serde_json::from_str(&json).unwrap();
        assert_eq!(message.message_type, deserialized.message_type);
        assert_eq!(message.src, deserialized.src);
        assert_eq!(message.dst, deserialized.dst);
    }
}
