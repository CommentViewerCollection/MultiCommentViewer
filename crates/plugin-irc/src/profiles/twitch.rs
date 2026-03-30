use irc_lib::{
    message::{IrcCommand, IrcMessage},
    profile::{ParsedChat, ProfileEvent, ServerProfile},
};

/// Twitch IRC サーバー用プロファイル
///
/// ホスト: `irc.chat.twitch.tv` ポート: `6697`（TLS）
///
/// Twitch IRC は RFC IRC + IRCv3 message tags の組み合わせで動作する。
/// 匿名接続（justinfan）ではコメント閲覧のみが可能。
pub struct TwitchIrcProfile {
    anon_number: Option<u32>,
}

impl TwitchIrcProfile {
    /// 認証接続（`oauth:<token>` を使用）
    pub fn authenticated() -> Self {
        Self { anon_number: None }
    }

    /// 匿名接続（`justinfan<n>` を使用、コメント閲覧のみ）
    pub fn anonymous(n: u32) -> Self {
        Self {
            anon_number: Some(n),
        }
    }
}

impl ServerProfile for TwitchIrcProfile {
    fn capabilities(&self) -> &[&'static str] {
        &[
            "twitch.tv/tags",
            "twitch.tv/commands",
            "twitch.tv/membership",
        ]
    }

    fn auth_commands(&self, nick: &str, pass: Option<&str>) -> Vec<String> {
        let mut cmds = Vec::new();
        if let Some(n) = self.anon_number {
            cmds.push(format!("PASS oauth:justinfan{n}"));
            cmds.push(format!("NICK justinfan{n}"));
        } else {
            if let Some(p) = pass {
                // 既に "oauth:" プレフィックスが付いている場合はそのまま使用
                let oauth = if p.starts_with("oauth:") {
                    p.to_string()
                } else {
                    format!("oauth:{p}")
                };
                cmds.push(format!("PASS {oauth}"));
            }
            cmds.push(format!("NICK {nick}"));
        }
        cmds
    }

    fn parse_chat(&self, msg: &IrcMessage) -> Option<ParsedChat> {
        if msg.command != IrcCommand::Privmsg {
            return None;
        }
        // params[0] = channel, params[1] = body (trailing)
        let body = msg.params.get(1)?.clone();
        let login_name = msg.prefix.as_ref()?.nick()?.to_string();

        let display_name = msg
            .tag("display-name")
            .filter(|s| !s.is_empty())
            .map(str::to_string);
        let user_id = msg.tag("user-id").map(str::to_string);
        let message_id = msg.tag("id").map(str::to_string);
        let color = msg
            .tag("color")
            .filter(|s| !s.is_empty())
            .map(str::to_string);

        // tmi-sent-ts はミリ秒なので秒に変換
        let timestamp: Option<i64> = msg
            .tag("tmi-sent-ts")
            .and_then(|s| s.parse::<i64>().ok())
            .map(|ms| ms / 1000);

        // "broadcaster/1,subscriber/0" → ["broadcaster", "subscriber"]
        let badges: Vec<String> = msg
            .tag("badges")
            .unwrap_or("")
            .split(',')
            .filter(|s| !s.is_empty())
            .map(|b| b.split('/').next().unwrap_or(b).to_string())
            .collect();

        Some(ParsedChat {
            message_id,
            user_id,
            display_name,
            login_name,
            body,
            timestamp,
            color,
            badges,
            extra: serde_json::json!({
                "emotes": msg.tag("emotes").unwrap_or(""),
                "room-id": msg.tag("room-id").unwrap_or(""),
                "subscriber": msg.tag("subscriber").unwrap_or("0") == "1",
                "mod": msg.tag("mod").unwrap_or("0") == "1",
                "turbo": msg.tag("turbo").unwrap_or("0") == "1",
            }),
        })
    }

    fn handle_other(&self, msg: &IrcMessage) -> Vec<ProfileEvent> {
        match &msg.command {
            IrcCommand::Other(cmd) if cmd.eq_ignore_ascii_case("USERNOTICE") => {
                let event_type = msg.tag("msg-id").unwrap_or("").to_string();
                let user_login = msg.tag("login").unwrap_or("").to_string();
                let system_msg = msg.tag("system-msg").map(|s| s.replace("\\s", " "));
                vec![ProfileEvent::UserNotice {
                    event_type,
                    user_login,
                    system_msg,
                }]
            }
            IrcCommand::Other(cmd) if cmd.eq_ignore_ascii_case("CLEARCHAT") => {
                let target_user = msg.trailing().filter(|s| !s.is_empty()).map(str::to_string);
                let target_user_id = msg.tag("target-user-id").map(str::to_string);
                vec![ProfileEvent::ClearChat {
                    target_user,
                    target_user_id,
                }]
            }
            IrcCommand::Other(cmd) if cmd.eq_ignore_ascii_case("CLEARMSG") => {
                let message_id = msg.tag("target-msg-id").unwrap_or("").to_string();
                vec![ProfileEvent::ClearMsg { message_id }]
            }
            _ => vec![],
        }
    }
}
