use std::collections::HashMap;

#[derive(Debug, Clone)]
pub enum TwitchEvent {
    PrivMsg {
        channel: String,
        user: String,
        text: String,
        tags: HashMap<String, String>,
    },
    Join {
        channel: String,
        user: String,
    },
    Part {
        channel: String,
        user: String,
    },
    RoomState {
        channel: String,
        tags: HashMap<String, String>,
    },
    Notice {
        channel: Option<String>,
        message: String,
        tags: HashMap<String, String>,
    },
    GlobalUserState {
        display_name: Option<String>,
        user_id: Option<String>,
    },
    UserNotice {
        channel: String,
        tags: HashMap<String, String>,
    },
    Ping,
    Pong,
    Other(IrcMessage),
}

#[derive(Debug, Clone)]
pub struct IrcMessage {
    pub tags: HashMap<String, String>,
    pub prefix: Option<String>,
    pub command: String,
    pub params: Vec<String>,
}
pub fn to_twitch_event(msg: IrcMessage) -> TwitchEvent {
    match msg.command.as_str() {
        "PING" => TwitchEvent::Ping,
        "PONG" => TwitchEvent::Pong,

        "PRIVMSG" => {
            // params: [channel, text]
            if msg.params.len() >= 2 {
                let channel = msg.params[0].clone();
                let text = msg.params[1].clone();
                let user = extract_user(&msg);

                TwitchEvent::PrivMsg {
                    channel,
                    user,
                    text,
                    tags: msg.tags,
                }
            } else {
                TwitchEvent::Other(msg)
            }
        }

        "JOIN" => {
            if let Some(channel) = msg.params.first() {
                let user = extract_user(&msg);
                TwitchEvent::Join {
                    channel: channel.clone(),
                    user,
                }
            } else {
                TwitchEvent::Other(msg)
            }
        }

        "PART" => {
            if let Some(channel) = msg.params.first() {
                let user = extract_user(&msg);
                TwitchEvent::Part {
                    channel: channel.clone(),
                    user,
                }
            } else {
                TwitchEvent::Other(msg)
            }
        }

        "ROOMSTATE" => {
            if let Some(channel) = msg.params.first() {
                TwitchEvent::RoomState {
                    channel: channel.clone(),
                    tags: msg.tags,
                }
            } else {
                TwitchEvent::Other(msg)
            }
        }

        "NOTICE" => {
            let channel = msg.params.first().cloned();
            let message = msg.params.get(1).cloned().unwrap_or_default();

            TwitchEvent::Notice {
                channel,
                message,
                tags: msg.tags,
            }
        }

        "GLOBALUSERSTATE" => TwitchEvent::GlobalUserState {
            display_name: msg.tags.get("display-name").cloned(),
            user_id: msg.tags.get("user-id").cloned(),
        },

        "USERNOTICE" => {
            if let Some(channel) = msg.params.first() {
                TwitchEvent::UserNotice {
                    channel: channel.clone(),
                    tags: msg.tags,
                }
            } else {
                TwitchEvent::Other(msg)
            }
        }

        _ => TwitchEvent::Other(msg),
    }
}
fn extract_user(msg: &IrcMessage) -> String {
    if let Some(prefix) = &msg.prefix {
        if let Some(exclam) = prefix.find('!') {
            return prefix[..exclam].to_string();
        }
        return prefix.clone();
    }
    String::new()
}

/// Twitch IRC の1行をパース
pub fn parse_irc_line(line: &str) -> IrcMessage {
    let mut rest = line.trim();
    let mut tags = HashMap::new();
    let mut prefix = None;

    // ---- Tags (@key=value;...)
    if rest.starts_with('@')
        && let Some(space) = rest.find(' ')
    {
        let tag_str = &rest[1..space];
        tags = parse_tags(tag_str);
        rest = &rest[space + 1..];
    }

    // ---- Prefix (:prefix)
    if rest.starts_with(':')
        && let Some(space) = rest.find(' ')
    {
        prefix = Some(rest[1..space].to_string());
        rest = &rest[space + 1..];
    }

    // ---- Command + Params
    let (command, params) = parse_command_and_params(rest);

    IrcMessage {
        tags,
        prefix,
        command,
        params,
    }
}

/// IRCv3 tags を HashMap に変換
fn parse_tags(tag_str: &str) -> HashMap<String, String> {
    let mut map = HashMap::new();

    for pair in tag_str.split(';') {
        let mut parts = pair.splitn(2, '=');
        let key = parts.next().unwrap_or("").to_string();
        let value = parts.next().unwrap_or("");
        map.insert(key, unescape_tag_value(value));
    }

    map
}

/// Twitchタグのエスケープ解除
fn unescape_tag_value(value: &str) -> String {
    value
        .replace(r"\s", " ")
        .replace(r"\:", ";")
        .replace(r"\\", "\\")
        .replace(r"\r", "\r")
        .replace(r"\n", "\n")
}

/// Command と params を分離
fn parse_command_and_params(input: &str) -> (String, Vec<String>) {
    let mut parts = Vec::new();
    let mut rest = input;

    while !rest.is_empty() {
        if let Some(stripped) = rest.strip_prefix(':') {
            // trailing parameter（最後まで）
            parts.push(stripped.to_string());
            break;
        }

        if let Some(space) = rest.find(' ') {
            parts.push(rest[..space].to_string());
            rest = &rest[space + 1..];
        } else {
            parts.push(rest.to_string());
            break;
        }
    }

    let command = parts.first().cloned().unwrap_or_default();
    let params = if parts.len() > 1 {
        parts[1..].to_vec()
    } else {
        Vec::new()
    };

    (command, params)
}
