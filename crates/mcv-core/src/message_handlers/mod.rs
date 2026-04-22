/// CoreActor のメッセージハンドラーモジュール
///
/// CoreActor の handle_* 関数を責任ごとに分割したモジュール群
pub mod comment;
pub mod connection;
pub mod plugin_hello;
pub mod settings;
pub mod site_browser;

use actix::Context;
use mcv_messages::{Message as McvMessage, MessageType};

use crate::core_actor::CoreActor;

/// UIとプラグイン双方で共通して処理される通知メッセージのハンドラー
///
/// 処理済みなら `true`、未処理（呼び出し元で対応が必要）なら `false` を返す。
pub fn handle_common_notification(
    core: &mut CoreActor,
    message: &McvMessage,
    ctx: &mut Context<CoreActor>,
) -> bool {
    match message.message_type {
        MessageType::AddConnection => {
            connection::handle_add_connection(core, message, ctx);
            true
        }
        MessageType::Connect => {
            connection::handle_connect(core, message, ctx);
            true
        }
        MessageType::ConnectFailed => {
            connection::handle_connect_failed(core, message, ctx);
            true
        }
        MessageType::Connected => {
            connection::handle_connected(core, message, ctx);
            true
        }
        MessageType::Disconnect => {
            connection::handle_disconnect(core, message, ctx);
            true
        }
        MessageType::Disconnected => {
            connection::handle_disconnected(core, message, ctx);
            true
        }
        MessageType::CommentReceived => {
            comment::handle_comment_received(core, message, ctx);
            true
        }
        MessageType::SendComment => {
            comment::handle_send_comment(core, message, ctx);
            true
        }
        MessageType::AddSite => {
            site_browser::handle_add_site(core, message, ctx);
            true
        }
        MessageType::AddBrowser => {
            site_browser::handle_add_browser(core, message, ctx);
            true
        }
        MessageType::RemoveBrowser => {
            site_browser::handle_remove_browser(core, message, ctx);
            true
        }
        MessageType::SetConnectionSite => {
            site_browser::handle_set_connection_site(core, message, ctx);
            true
        }
        MessageType::UpdateConnectionSettings => {
            site_browser::handle_update_connection_settings(core, message, ctx);
            true
        }
        MessageType::PluginRemoved => {
            plugin_hello::handle_plugin_removed(core, message, ctx);
            true
        }
        MessageType::UpdateConnectionAccount => {
            connection::handle_update_connection_account(core, message, ctx);
            true
        }
        MessageType::StreamMetadata => {
            comment::handle_stream_metadata(core, message, ctx);
            true
        }
        _ => false,
    }
}
