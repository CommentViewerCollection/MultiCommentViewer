use std::collections::HashMap;

use crate::error::IrcError;

/// IRCv3 タグ付き IRC メッセージ（RFC 1459 / RFC 2812 / IRCv3 準拠）
#[derive(Debug, Clone)]
pub struct IrcMessage {
    /// IRCv3 message tags (@key=value;key2=value2...)
    pub tags: HashMap<String, Option<String>>,
    /// メッセージのプレフィックス
    pub prefix: Option<Prefix>,
    /// コマンド
    pub command: IrcCommand,
    /// パラメータ一覧（trailing を含む最後の要素として格納）
    pub params: Vec<String>,
}

/// プレフィックスの種別
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum Prefix {
    /// サーバー名（ドットを含む）
    Server(String),
    /// ユーザー (nick[!user[@host]])
    User {
        nick: String,
        user: Option<String>,
        host: Option<String>,
    },
}

impl Prefix {
    /// ニックネームを返す（User の場合のみ）
    pub fn nick(&self) -> Option<&str> {
        match self {
            Prefix::User { nick, .. } => Some(nick),
            Prefix::Server(_) => None,
        }
    }
}

/// IRC コマンド種別
#[derive(Debug, Clone, PartialEq, Eq)]
pub enum IrcCommand {
    Nick,
    User,
    Pass,
    Join,
    Part,
    Quit,
    Privmsg,
    Notice,
    Ping,
    Pong,
    Cap,
    Kick,
    Mode,
    Topic,
    Who,
    Whois,
    List,
    Names,
    /// 数値応答（001〜999）
    Numeric(u16),
    /// 未知のコマンド
    Other(String),
}

impl IrcMessage {
    /// テキスト行をパースして IrcMessage を返す
    pub fn parse(line: &str) -> Result<Self, IrcError> {
        let mut rest = line.trim_end_matches('\r');

        // IRCv3 tags (@...)
        let tags = if rest.starts_with('@') {
            let (tags_str, remaining) = rest[1..]
                .split_once(' ')
                .ok_or_else(|| IrcError::Parse(format!("invalid tags: {line}")))?;
            rest = remaining.trim_start();
            parse_tags(tags_str)
        } else {
            HashMap::new()
        };

        // prefix (:...)
        let prefix = if rest.starts_with(':') {
            let (prefix_str, remaining) = rest[1..]
                .split_once(' ')
                .ok_or_else(|| IrcError::Parse(format!("invalid prefix: {line}")))?;
            rest = remaining.trim_start();
            Some(parse_prefix(prefix_str))
        } else {
            None
        };

        // command + params
        let (command_str, params_raw) = if let Some(idx) = rest.find(' ') {
            (&rest[..idx], rest[idx + 1..].trim_start())
        } else {
            (rest, "")
        };
        let command = parse_command(command_str);
        let params = parse_params(params_raw);

        Ok(IrcMessage {
            tags,
            prefix,
            command,
            params,
        })
    }

    /// タグの値を取得（存在しないか値なしの場合は None）
    pub fn tag(&self, key: &str) -> Option<&str> {
        self.tags.get(key)?.as_deref()
    }

    /// チャンネル（最初のパラメータ）を取得
    pub fn channel(&self) -> Option<&str> {
        self.params.first().map(String::as_str)
    }

    /// trailing パラメータ（最後のパラメータ）を取得
    pub fn trailing(&self) -> Option<&str> {
        self.params.last().map(String::as_str)
    }
}

fn parse_tags(tags_str: &str) -> HashMap<String, Option<String>> {
    let mut map = HashMap::new();
    for pair in tags_str.split(';') {
        if let Some((k, v)) = pair.split_once('=') {
            let decoded = unescape_tag_value(v);
            map.insert(
                k.to_string(),
                if decoded.is_empty() {
                    None
                } else {
                    Some(decoded)
                },
            );
        } else if !pair.is_empty() {
            map.insert(pair.to_string(), None);
        }
    }
    map
}

/// IRCv3 タグ値のエスケープを解除する
fn unescape_tag_value(s: &str) -> String {
    let mut result = String::with_capacity(s.len());
    let mut chars = s.chars().peekable();
    while let Some(c) = chars.next() {
        if c == '\\' {
            match chars.peek() {
                Some(':') => {
                    result.push(';');
                    chars.next();
                }
                Some('s') => {
                    result.push(' ');
                    chars.next();
                }
                Some('\\') => {
                    result.push('\\');
                    chars.next();
                }
                Some('r') => {
                    result.push('\r');
                    chars.next();
                }
                Some('n') => {
                    result.push('\n');
                    chars.next();
                }
                _ => result.push(c),
            }
        } else {
            result.push(c);
        }
    }
    result
}

fn parse_prefix(s: &str) -> Prefix {
    if let Some((nick_user, host)) = s.split_once('@') {
        if let Some((nick, user)) = nick_user.split_once('!') {
            Prefix::User {
                nick: nick.to_string(),
                user: Some(user.to_string()),
                host: Some(host.to_string()),
            }
        } else {
            Prefix::User {
                nick: nick_user.to_string(),
                user: None,
                host: Some(host.to_string()),
            }
        }
    } else if s.contains('.') {
        // ドットを含む場合はサーバー名と判断
        Prefix::Server(s.to_string())
    } else {
        Prefix::User {
            nick: s.to_string(),
            user: None,
            host: None,
        }
    }
}

fn parse_command(s: &str) -> IrcCommand {
    if let Ok(n) = s.parse::<u16>() {
        return IrcCommand::Numeric(n);
    }
    match s.to_ascii_uppercase().as_str() {
        "NICK" => IrcCommand::Nick,
        "USER" => IrcCommand::User,
        "PASS" => IrcCommand::Pass,
        "JOIN" => IrcCommand::Join,
        "PART" => IrcCommand::Part,
        "QUIT" => IrcCommand::Quit,
        "PRIVMSG" => IrcCommand::Privmsg,
        "NOTICE" => IrcCommand::Notice,
        "PING" => IrcCommand::Ping,
        "PONG" => IrcCommand::Pong,
        "CAP" => IrcCommand::Cap,
        "KICK" => IrcCommand::Kick,
        "MODE" => IrcCommand::Mode,
        "TOPIC" => IrcCommand::Topic,
        "WHO" => IrcCommand::Who,
        "WHOIS" => IrcCommand::Whois,
        "LIST" => IrcCommand::List,
        "NAMES" => IrcCommand::Names,
        _ => IrcCommand::Other(s.to_string()),
    }
}

fn parse_params(mut s: &str) -> Vec<String> {
    let mut params = Vec::new();
    loop {
        s = s.trim_start();
        if s.is_empty() {
            break;
        }
        if s.starts_with(':') {
            params.push(s[1..].to_string());
            break;
        }
        if let Some(idx) = s.find(' ') {
            params.push(s[..idx].to_string());
            s = &s[idx + 1..];
        } else {
            params.push(s.to_string());
            break;
        }
    }
    params
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_parse_privmsg() {
        let msg = IrcMessage::parse(":nick!user@host PRIVMSG #channel :Hello world").unwrap();
        assert_eq!(msg.command, IrcCommand::Privmsg);
        assert_eq!(msg.channel(), Some("#channel"));
        assert_eq!(msg.trailing(), Some("Hello world"));
        assert_eq!(msg.prefix.as_ref().and_then(|p| p.nick()), Some("nick"));
    }

    #[test]
    fn test_parse_ping() {
        let msg = IrcMessage::parse("PING :irc.server.example").unwrap();
        assert_eq!(msg.command, IrcCommand::Ping);
        assert_eq!(msg.trailing(), Some("irc.server.example"));
    }

    #[test]
    fn test_parse_tags() {
        let msg = IrcMessage::parse(
            "@badge-info=;badges=broadcaster/1;color=#FF0000;display-name=Test \
             :test!test@test.tmi.twitch.tv PRIVMSG #channel :Hello",
        )
        .unwrap();
        assert_eq!(msg.tag("color"), Some("#FF0000"));
        assert_eq!(msg.tag("display-name"), Some("Test"));
        assert_eq!(msg.tag("badge-info"), None); // 空値は None
        assert_eq!(msg.trailing(), Some("Hello"));
    }

    #[test]
    fn test_parse_numeric() {
        let msg = IrcMessage::parse(":server.example 001 nick :Welcome").unwrap();
        assert_eq!(msg.command, IrcCommand::Numeric(1));
    }

    #[test]
    fn test_parse_no_prefix() {
        let msg = IrcMessage::parse("PING :tmi.twitch.tv").unwrap();
        assert!(msg.prefix.is_none());
        assert_eq!(msg.command, IrcCommand::Ping);
    }

    #[test]
    fn test_tag_unescape() {
        let msg = IrcMessage::parse("@system-msg=Hello\\sWorld :server 001 nick :hi").unwrap();
        assert_eq!(msg.tag("system-msg"), Some("Hello World"));
    }

    #[test]
    fn test_parse_server_prefix() {
        let msg = IrcMessage::parse(":irc.libera.chat 001 mynick :Welcome to Libera.Chat").unwrap();
        assert_eq!(msg.command, IrcCommand::Numeric(1));
        assert!(matches!(msg.prefix, Some(Prefix::Server(_))));
    }
}
