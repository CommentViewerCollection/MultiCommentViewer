use actix::Addr;
use mcv_core::{CoreActor, PluginManager};
use std::sync::{Arc, Mutex};

/// McvEnvelope をフロントエンド表示用に変換した行
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct CommentRow {
    pub(crate) id: String,
    pub(crate) user_name: Vec<mcv_messages::MessagePart>,
    pub(crate) user_id: String,
    pub(crate) badges: Vec<mcv_messages::ProviderBadge>,
    pub(crate) text: Vec<mcv_messages::MessagePart>,
    pub(crate) timestamp: i64,
    pub(crate) connection_id: String,
    /// false の場合は非表示（承認待ちプレースホルダー）
    pub(crate) is_visible: bool,
    /// 置き換え・削除対象の CommentRow の id
    pub(crate) replaces_id: Option<String>,
    /// メッセージ種別: "chat" | "history_chat" | "monetary" | "system"
    pub(crate) kind: String,
    /// ユーザーアイコン URL（省略可）
    #[serde(skip_serializing_if = "Option::is_none")]
    pub(crate) avatar_url: Option<String>,
    /// 投げ銭・スーパーチャットの金額テキスト（例: "¥8,000"）。monetary 種別のみ設定。
    #[serde(skip_serializing_if = "Option::is_none")]
    pub(crate) amount_text: Option<String>,
}

/// アプリケーションの状態
pub(crate) struct AppState {
    pub(crate) core_addr: Addr<CoreActor>,
    pub(crate) plugin_manager: Arc<tokio::sync::Mutex<PluginManager>>,
    pub(crate) comment_store: Arc<Mutex<crate::comment_store::CommentStore>>,
    /// 起動時の core 制約チェック結果（フロントエンドが未ロードのまま emit されるのを防ぐためキャッシュ）
    pub(crate) pending_core_update: Arc<tokio::sync::Mutex<Option<CoreUpdatePayload>>>,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub(crate) struct CoreUpdatePayload {
    pub(crate) target_version: String,
    pub(crate) channel: String,
    pub(crate) sha256: String,
}

/// settings_dir を Tauri コマンドへ渡すための管理状態
pub(crate) struct SettingsDirState {
    pub(crate) settings_dir: std::path::PathBuf,
}

/// プラグインロードフェーズ。watch チャンネルで状態遷移を通知する。
#[derive(Debug, Clone, PartialEq)]
pub(crate) enum PluginsPhase {
    Loading,
    Ready,
}

/// 列設定（順序・幅・表示状態）
#[derive(serde::Serialize, serde::Deserialize, Clone)]
pub(crate) struct ColumnSettings {
    pub(crate) order: Vec<String>,
    pub(crate) widths: std::collections::HashMap<String, u32>,
    pub(crate) visibility: std::collections::HashMap<String, bool>,
}
