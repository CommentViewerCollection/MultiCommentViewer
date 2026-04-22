pub mod rfc;
pub use rfc::RfcProfile;

use crate::message::IrcMessage;

/// PRIVMSG から抽出した汎用チャットコメント
#[derive(Debug, Clone)]
pub struct ParsedChat {
    /// プラットフォームが付与するメッセージID
    pub message_id: Option<String>,
    /// ユーザー固有ID
    pub user_id: Option<String>,
    /// 表示名（Display name）
    pub display_name: Option<String>,
    /// ログイン名 (nick)
    pub login_name: String,
    /// メッセージ本文
    pub body: String,
    /// タイムスタンプ（Unix 秒）
    pub timestamp: Option<i64>,
    /// チャットカラー (#RRGGBB)
    pub color: Option<String>,
    /// バッジ一覧（表示ラベル）
    pub badges: Vec<String>,
    /// プロファイル固有の追加情報
    pub extra: serde_json::Value,
}

/// プロファイルが生成するイベント
#[derive(Debug, Clone)]
pub enum ProfileEvent {
    /// チャットコメント
    Chat(ParsedChat),
    /// システムメッセージ
    SystemMessage(String),
    /// ユーザー通知（サブスクリプション等）
    UserNotice {
        event_type: String,
        user_login: String,
        system_msg: Option<String>,
    },
    /// チャットクリア（ユーザー全削除 / タイムアウト / BAN）
    ClearChat {
        target_user: Option<String>,
        target_user_id: Option<String>,
    },
    /// 特定メッセージ削除
    ClearMsg { message_id: String },
}

/// サーバーごとの動作差分を定義するトレイト
pub trait ServerProfile: Send + Sync + 'static {
    /// 接続時に要求する CAP（空の場合スキップ）
    fn capabilities(&self) -> &[&'static str] {
        &[]
    }

    /// 認証コマンド列を生成
    ///
    /// PASS/NICK 送信順序・SASL 等の差分を吸収する。
    fn auth_commands(&self, nick: &str, pass: Option<&str>) -> Vec<String>;

    /// JOIN 後に追加送信するコマンド
    fn post_join_commands(&self, _channel: &str) -> Vec<String> {
        vec![]
    }

    /// PRIVMSG を ParsedChat に変換（対応しない場合は None）
    fn parse_chat(&self, msg: &IrcMessage) -> Option<ParsedChat>;

    /// PRIVMSG 以外のコマンドをイベントに変換
    fn handle_other(&self, _msg: &IrcMessage) -> Vec<ProfileEvent> {
        vec![]
    }
}
