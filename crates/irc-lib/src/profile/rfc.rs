use super::{ParsedChat, ServerProfile};
use crate::message::{IrcCommand, IrcMessage};

/// RFC 1459 / RFC 2812 準拠のデフォルトプロファイル
pub struct RfcProfile;

impl ServerProfile for RfcProfile {
    fn auth_commands(&self, nick: &str, pass: Option<&str>) -> Vec<String> {
        let mut cmds = Vec::new();
        if let Some(p) = pass {
            cmds.push(format!("PASS {p}"));
        }
        cmds.push(format!("NICK {nick}"));
        cmds.push(format!("USER {nick} 0 * :MultiCommentViewer"));
        cmds
    }

    fn parse_chat(&self, msg: &IrcMessage) -> Option<ParsedChat> {
        if msg.command != IrcCommand::Privmsg {
            return None;
        }
        // params[0] = channel, params[1] = body (trailing)
        let body = msg.params.get(1)?.clone();
        let login_name = msg.prefix.as_ref()?.nick()?.to_string();
        Some(ParsedChat {
            message_id: None,
            user_id: None,
            display_name: None,
            login_name,
            body,
            timestamp: None,
            color: None,
            badges: vec![],
            extra: serde_json::Value::Null,
        })
    }
}
