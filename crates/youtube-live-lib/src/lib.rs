use anyhow::Result;
pub struct Vid {
    value: String,
}
impl Vid {
    pub fn new(value: String) -> Self {
        Vid { value }
    }
    pub fn value(&self) -> &str {
        &self.value
    }
}
#[derive(Clone)]
pub struct Continuation {
    value: String,
}
impl Continuation {
    pub fn new(value: String) -> Self {
        Continuation { value }
    }
    pub fn value(&self) -> &str {
        &self.value
    }
}
pub struct YtInitialData {
    actions: Vec<Action>,
    continuation: Continuation,
    raw: String,
}
impl YtInitialData {
    pub fn actions(&self) -> &Vec<Action> {
        &self.actions
    }
    pub fn continuation(&self) -> &Continuation {
        &self.continuation
    }
    pub fn raw(&self) -> &str {
        &self.raw
    }
}
pub struct LiveChat {
    value: String,
}
impl LiveChat {
    pub fn new(value: String) -> Self {
        LiveChat { value }
    }
    pub fn value(&self) -> &str {
        &self.value
    }
}
pub struct Ytcfg {
    client: serde_json::Value,
}
#[derive(Debug)]
struct remove_chat_item_action {
    target_item_id: String,
}
impl remove_chat_item_action {
    fn new(target_item_id: String) -> Self {
        remove_chat_item_action { target_item_id }
    }
}

pub async fn get_live_chat(vid: &Vid) -> Result<LiveChat, mcv_tracing::TracingError> {
    let url = format!(
        "https://www.youtube.com/live_chat?&is_popout=1&v={}",
        vid.value()
    );
    let client = reqwest::Client::new();
    let res = client
        .get(&url)
        .header(
            "User-Agent",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:146.0) Gecko/20100101 Firefox/146.0",
        )
        .header("Accept-Encoding", "gzip, deflate")
        .header(
            "Accept",
            "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
        )
        .header("Sec-Fetch-Site", "same-origin")
        .header("Sec-Fetch-Dest", "document")
        .header("Sec-Fetch-Mode", "navigate")
        .header("Upgrade-Insecure-Requests", "1")
        .send()
        .await
        .map_err(|_| mcv_tracing::capture_context!("live_chatの取得に失敗"))?;
    let body = res
        .text()
        .await
        .map_err(|_| mcv_tracing::capture_context!("live_chatの取得に失敗"))?;
    Ok(LiveChat::new(body))
}
pub async fn get_yt_initial_data(
    live_chat: &LiveChat,
) -> Result<YtInitialData, mcv_tracing::TracingError> {
    let yt_initial_data = extract_yt_initial_data(&live_chat).map_err(|inner| {
        mcv_tracing::capture_context!(
            inner.to_string(),
            body = live_chat.value().to_owned(),
            inner = inner.to_string()
        )
    })?;
    Ok(yt_initial_data)
}
pub async fn get_live_chat_messages(
    vid: &Vid,
    ytcfg: &Ytcfg,
    continuation: &Continuation,
) -> Result<(Option<Continuation>, Vec<Action>, String), mcv_tracing::TracingError> {
    let mut obj: serde_json::Value = serde_json::from_str(r#"{"context":{}}"#).unwrap();
    //objのcontextにytcfg.clientをセットする
    obj["context"]["client"] = ytcfg.client.clone();
    obj["continuation"] = serde_json::Value::String(continuation.value().to_owned());

    let url =
        format!("https://www.youtube.com/youtubei/v1/live_chat/get_live_chat?prettyPrint=false");
    let client = reqwest::Client::new();
    let res = client.post(&url)
    .header("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/143.0.0.0 Safari/537.36")
    .json(&obj)
    .send().await.map_err(|e| {
        mcv_tracing::capture_context!(
            "Failed to send request to get live chat messages",
            error = e.to_string()
        )
    })?;
    let body = res.text().await.map_err(|e| {
        mcv_tracing::capture_context!("Failed to read response body", error = e.to_string())
    })?;
    let json: serde_json::Value = serde_json::from_str(&body).map_err(|e| {
        mcv_tracing::capture_context!(
            "Failed to parse response JSON",
            error = e.to_string(),
            body = body.to_owned()
        )
    })?;
    let live_chat_continuation = get_value(
        &json,
        &[
            // "responseContext",
            "continuationContents",
            "liveChatContinuation",
        ],
    )?;

    let mut aabb = Vec::new();
    match live_chat_continuation.get("actions") {
        Some(actions) => {
            let actions = actions.as_array().ok_or_else(|| {
                mcv_tracing::capture_context!(
                    "Missing actions array in live chat continuation",
                    json = json.to_string()
                )
            })?;
            for action in actions {
                let a = parse_action(&action);
                aabb.push(a);
            }
        }
        None => {}
    };
    let continuation = if let Some(k) = live_chat_continuation.get("continuations") {
        let c = k.as_array().unwrap();
        let c0 = &c[0];
        let con = get_string(c0, &["invalidationContinuationData", "continuation"])?;
        Some(Continuation::new(con))
    } else {
        None
    };
    Ok((continuation, aabb, body))
}
fn extract_ytcfg_raw<'a>(live_chat: &'a LiveChat) -> Option<&'a str> {
    let before_part = "ytcfg.set({";
    let after_part = "});";
    let start = live_chat.value().find(before_part)?;
    let end = live_chat.value()[start..].find(after_part)?;
    let json_str = &live_chat.value()[start + before_part.len() - 1..start + end + 1];
    Some(json_str)
}
pub fn extract_ytcfg(live_chat: &LiveChat) -> Result<Ytcfg, mcv_tracing::TracingError> {
    let json_str = match extract_ytcfg_raw(live_chat) {
        Some(s) => s,
        None => {
            let ctx = mcv_tracing::capture_context!("", body = live_chat.value().to_owned());
            return Err(ctx.into());
        }
    };
    let json: serde_json::Value = serde_json::from_str(json_str).map_err(|e| {
        let ctx = mcv_tracing::capture_context!(
            "Failed to parse ytcfg JSON",
            error = e.to_string(),
            json_str = json_str.to_owned()
        );
        ctx
    })?;
    let client = get_value(&json, &["INNERTUBE_CONTEXT", "client"])?;

    Ok(Ytcfg {
        client: client.clone(),
    })
}

fn extract_yt_initial_data_raw<'a>(body: &'a str) -> Option<&'a str> {
    //bodyからytInitialDataを抽出するロジックを実装する
    //window["ytInitialData"] = {
    //};</script>
    let before_part = r#"window["ytInitialData"] = "#;
    let after_part = ";</script>";
    let start = body.find(before_part)?;
    let end = body[start..].find(after_part)?;
    let json_str = &body[start + before_part.len()..start + end];
    Some(json_str)
}
fn extract_yt_initial_data(
    live_chat: &LiveChat,
) -> Result<YtInitialData, mcv_tracing::TracingError> {
    let json_str = match extract_yt_initial_data_raw(live_chat.value()) {
        Some(s) => s,
        None => {
            let ctx = mcv_tracing::capture_context!("aaaaaa", body = live_chat.value().to_owned());
            return Err(ctx.into());
        }
    };
    let json: serde_json::Value = serde_json::from_str(json_str).map_err(|e| {
        let ctx = mcv_tracing::capture_context!(
            "Failed to parse ytInitialData JSON",
            error = e.to_string(),
            json_str = json_str.to_owned()
        );
        ctx
    })?;
    let continuations = get_value(&json, &["contents", "liveChatRenderer", "continuations"])?
        .as_array()
        .ok_or_else(|| {
            mcv_tracing::capture_context!(
                "Missing continuations array in ytInitialData",
                json = json.to_string()
            )
        })?;
    let continuation_data = &continuations[0];
    let continuation = if continuation_data
        .as_object()
        .ok_or_else(|| {
            mcv_tracing::capture_context!("Missing continuation data", json = json.to_string())
        })?
        .contains_key("invalidationContinuationData")
    {
        get_string(
            continuation_data,
            &["invalidationContinuationData", "continuation"],
        )?
    } else if continuation_data
        .as_object()
        .ok_or_else(|| {
            mcv_tracing::capture_context!("Missing continuation data", json = json.to_string())
        })?
        .contains_key("timedContinuationData")
    {
        get_string(
            continuation_data,
            &["timedContinuationData", "continuation"],
        )?
    } else {
        return Err(mcv_tracing::capture_context!(
            "No valid continuation data",
            json = json.to_string()
        )
        .into());
    };
    //actionsを取得
    let actions = get_value(&json, &["contents", "liveChatRenderer", "actions"])?
        .as_array()
        .ok_or_else(|| {
            mcv_tracing::capture_context!(
                "Missing actions array in ytInitialData",
                json = json.to_string()
            )
        })?;
    let mut aabb = Vec::new();
    for action in actions {
        tracing::trace!(
            target: "mcv::youtube-live-lib",
            action = action.to_string(),
            "action received"
        );
        let ret = parse_action(&action);
        if let Action::ParseError(a) = ret {
            tracing::error!(
                target: "mcv::youtube-live-lib",
                raw = a,
                "Failed to parse action"
            );
        } else if let Action::IgnoreAction = ret {
            continue;
        } else {
            aabb.push(ret);
        }
    }

    //emojiを取得

    let yt_initial_data = YtInitialData {
        actions: aabb,
        continuation: Continuation::new(continuation),
        raw: json_str.to_owned(),
    };
    Ok(yt_initial_data)
}
pub fn get_string(
    value: &serde_json::Value,
    path: &[&str],
) -> Result<String, mcv_tracing::TracingError> {
    path.iter()
        .try_fold(value, |acc, key| acc.get(key))
        .and_then(|v| v.as_str())
        .map(|s| s.to_string())
        .ok_or_else(|| {
            mcv_tracing::capture_context!(
                "Missing string value",
                path = path.join("."),
                value = value.to_string()
            )
            .into()
        })
}
pub fn get_value<'a>(
    value: &'a serde_json::Value,
    path: &'a [&'a str],
) -> Result<&'a serde_json::Value, mcv_tracing::TracingError> {
    path.iter()
        .try_fold(value, |acc, key| acc.get(key))
        .ok_or_else(|| {
            mcv_tracing::capture_context!(
                format!("Missing value {}", value.to_string()),
                path = path.join("."),
                value = value.to_string()
            )
            .into()
        })
}
#[derive(Debug, Clone, PartialEq)]
pub struct Emoji {
    pub emoji_id: String,
    pub label: String,
    pub thumbnails: Vec<Thumbnail>,
    pub is_custom_emoji: bool,
}
#[derive(Debug, Clone, PartialEq)]
pub struct Thumbnail {
    pub url: String,
    pub width: u64,
    pub height: u64,
}
#[derive(Debug, Clone, PartialEq)]
pub struct AuthorBadge {
    pub tooltip: String,
    pub thumbnails: Vec<Thumbnail>,
}
#[derive(Debug, Clone, PartialEq)]
pub enum MessagePart {
    Text(String),
    Emoji(Emoji),
}
fn parse_message(
    message: &serde_json::Value,
) -> Result<Vec<MessagePart>, mcv_tracing::TracingError> {
    let runs = message
        .get("runs")
        .and_then(|v| v.as_array())
        .ok_or_else(|| {
            mcv_tracing::capture_context!("Invalid message runs", message = message.to_string())
        })?;
    let mut parts = vec![];
    for run in runs {
        if let Some(text) = run.get("text").and_then(|v| v.as_str()) {
            parts.push(MessagePart::Text(text.to_string()));
        } else if let Some(emoji_value) = run.get("emoji") {
            let emoji_id = get_string(emoji_value, &["emojiId"])?;
            let label = emoji_value
                .get("image")
                .and_then(|v| v.get("accessibility"))
                .and_then(|v| v.get("accessibilityData"))
                .and_then(|v| v.get("label"))
                .and_then(|v| v.as_str())
                .unwrap_or("")
                .to_string();
            let thumbnails_values = emoji_value
                .get("image")
                .and_then(|v| v.get("thumbnails"))
                .and_then(|v| v.as_array())
                .ok_or_else(|| {
                    mcv_tracing::capture_context!(
                        "Invalid emoji thumbnails",
                        emoji = emoji_value.to_string()
                    )
                })?;
            let mut thumbnails = vec![];
            for tn in thumbnails_values {
                let url = get_string(tn, &["url"])?;
                let width = tn.get("width").and_then(|v| v.as_u64()).unwrap_or(0);
                let height = tn.get("height").and_then(|v| v.as_u64()).unwrap_or(0);
                thumbnails.push(Thumbnail { url, width, height });
            }
            parts.push(MessagePart::Emoji(Emoji {
                emoji_id,
                label,
                thumbnails,
                is_custom_emoji: true,
            }));
        }
    }
    Ok(parts)
}
impl AuthorBadge {
    fn new(tooltip: String, thumbnails: Vec<Thumbnail>) -> Self {
        AuthorBadge {
            tooltip,
            thumbnails,
        }
    }
}
fn parse_author_badges(
    message: &serde_json::Value,
) -> Result<Vec<AuthorBadge>, mcv_tracing::TracingError> {
    let mut abs = vec![];
    if let Some(author_badges_values) = message.get("authorBadges") {
        let author_badges_values = author_badges_values.as_array().ok_or_else(|| {
            mcv_tracing::capture_context!("Invalid authorBadges", message = message.to_string())
        })?;

        for badge in author_badges_values {
            let tooltip = get_string(badge, &["liveChatAuthorBadgeRenderer", "tooltip"])?;
            let mut thumbnails = vec![];
            if let Some(tns) = badge
                .get("liveChatAuthorBadgeRenderer")
                .and_then(|v| v.get("thumbnails"))
                .and_then(|v| v.as_array())
            {
                for tn in tns {
                    let url = get_string(tn, &["url"])?;
                    let width = tn.get("width").and_then(|v| v.as_u64()).unwrap_or(0);
                    let height = tn.get("height").and_then(|v| v.as_u64()).unwrap_or(0);
                    thumbnails.push(Thumbnail { url, width, height });
                }
            }
            abs.push(AuthorBadge::new(tooltip, thumbnails));
        }
    }
    Ok(abs)
}

#[derive(Debug, Clone, PartialEq)]
pub enum Action {
    LiveChatTextMessage1(LiveChatTextMessage),
    Membership,
    RemoveChatItem(RemoveChatItemAction),
    ReplaceChatItem,
    UpdatePoll,
    ReportModerationState,
    ViewerEngagementMessage,
    IgnoreAction,
    ParseError(String),
}
#[derive(Debug, Clone, PartialEq)]
pub struct LiveChatTextMessage {
    pub author_name: String,
    pub message_parts: Vec<MessagePart>,
    pub timestamp_usec: String,
    pub author_badges: Vec<AuthorBadge>, // 型は実際の戻り値に合わせてください
    pub author_external_channel_id: String,
}

#[derive(Debug, Clone, PartialEq)]
pub struct RemoveChatItemAction {
    pub target_item_id: String,
}

fn parse_action(action: &serde_json::Value) -> Action {
    let obj = match action.as_object() {
        Some(o) => o,
        None => {
            mcv_tracing::capture_context!("Invalid action", action = action.to_string());
            return Action::ParseError(action.to_string());
        }
    };

    if obj.contains_key("addChatItemAction") {
        let item = match get_value(action, &["addChatItemAction", "item"]) {
            Ok(v) => v,
            Err(_) => return Action::ParseError(action.to_string()),
        };

        let item_map = match item.as_object() {
            Some(m) => m,
            None => return Action::ParseError(action.to_string()),
        };

        if item_map.contains_key("liveChatTextMessageRenderer") {
            let message = match get_value(item, &["liveChatTextMessageRenderer"]) {
                Ok(v) => v,
                Err(_) => return Action::ParseError(action.to_string()),
            };

            let message_parts = {
                let message_value = match get_value(&message, &["message"]) {
                    Ok(v) => v,
                    Err(_) => return Action::ParseError(action.to_string()),
                };

                match parse_message(message_value) {
                    Ok(parts) => parts,
                    Err(_) => return Action::ParseError(action.to_string()),
                }
            };

            let author_badges = match parse_author_badges(&message) {
                Ok(b) => b,
                Err(_) => return Action::ParseError(action.to_string()),
            };

            let author_name = match get_string(message, &["authorName", "simpleText"]) {
                Ok(s) => s,
                Err(_) => return Action::ParseError(action.to_string()),
            };

            let timestamp_usec = match get_string(message, &["timestampUsec"]) {
                Ok(s) => s,
                Err(_) => return Action::ParseError(action.to_string()),
            };
            let author_external_channel_id = match get_string(message, &["authorExternalChannelId"])
            {
                Ok(s) => s,
                Err(_) => return Action::ParseError(action.to_string()),
            };
            let action = LiveChatTextMessage {
                author_name,
                message_parts,
                timestamp_usec,
                author_badges,
                author_external_channel_id,
            };

            return Action::LiveChatTextMessage1(action);
        } else if item_map.contains_key("liveChatMembershipItemRenderer") {
            return Action::Membership;
        } else if item_map.contains_key("liveChatViewerEngagementMessageRenderer") {
            return Action::ViewerEngagementMessage;
        } else {
            return Action::ParseError(action.to_string());
        }
    } else if obj.contains_key("liveChatReportModerationStateCommand") {
        return Action::ReportModerationState;
    } else if obj.contains_key("updateLiveChatPollAction") {
        return Action::UpdatePoll;
    } else if obj.contains_key("replaceChatItemAction") {
        return Action::ReplaceChatItem;
    } else if obj.contains_key("removeChatItemAction") {
        let target_item_id = match get_string(action, &["removeChatItemAction", "targetItemId"]) {
            Ok(a) => a,
            Err(_) => return Action::ParseError(action.to_string()),
        };

        let rcia = RemoveChatItemAction { target_item_id };

        return Action::RemoveChatItem(rcia);
    } else {
        return Action::ParseError(action.to_string());
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn test_extract_yt_initial_data_raw() -> Result<(), ()> {
        let sample_html = r#"
        <html>
        <head><title>Test</title></head>
        <body>
        <script>
        window["ytInitialData"] = {"contents":{"liveChatRenderer":{"continuations":[{"invalidationContinuationData":{"continuation":"CONTINUATION_TOKEN"}}],"actions":[]}}};</script>
        </body>
        </html>
        "#;
        let json_str = extract_yt_initial_data_raw(sample_html).unwrap();
        assert!(json_str.contains(r#"{"contents":{"liveChatRenderer":{"continuations":[{"invalidationContinuationData":{"continuation":"CONTINUATION_TOKEN"}}],"actions":[]}}}"#));
        Ok(())
    }
    #[tokio::test]
    async fn test_parse_poll_action() -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"
{"updateLiveChatPollAction":{"pollToUpdate":{"pollRenderer":{"choices":[{"selected":false,"signinEndpoint":{"commandMetadata":{"webCommandMetadata":{"rootVe":83769,"url":"https://accounts.google.com/ServiceLogin?service=youtube&uilel=3&passive=true&continue=https%3A%2F%2Fwww.youtube.com%2Fsignin%3Faction_handle_signin%3Dtrue%26app%3Ddesktop%26hl%3Dja&hl=ja","webPageType":"WEB_PAGE_TYPE_UNKNOWN"}},"signInEndpoint":{"nextEndpoint":{}}},"text":{"runs":[{"text":"このゲーム見たことある！"}]},"votePercentage":{"simpleText":"26%"},"voteRatio":0.2618296444416046},{"selected":false,"signinEndpoint":{"commandMetadata":{"webCommandMetadata":{"rootVe":83769,"url":"https://accounts.google.com/ServiceLogin?service=youtube&uilel=3&passive=true&continue=https%3A%2F%2Fwww.youtube.com%2Fsignin%3Faction_handle_signin%3Dtrue%26app%3Ddesktop%26hl%3Dja&hl=ja","webPageType":"WEB_PAGE_TYPE_UNKNOWN"}},"signInEndpoint":{"nextEndpoint":{}}},"text":{"runs":[{"text":"初めて見るよ！"}]},"votePercentage":{"simpleText":"74%"},"voteRatio":0.738170325756073}],"header":{"pollHeaderRenderer":{"contextMenuButton":{"buttonRenderer":{"accessibility":{"label":"チャットの操作"},"accessibilityData":{"accessibilityData":{"label":"チャットの操作"}},"command":{"commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMHgxY3pFMlQwRnhXa2xFUm1aTVVuZG5VV1I1UTBVMlgyY2FLU29uQ2hoVlEyWnVNVTk1UjNKQ1FXUllNbmhXYzBOaU5WVnZZV2NTQzNGblFsUm9aMVJCZG5KM0lBSW9CRElhQ2hoVlEyWnVNVTk1UjNKQ1FXUllNbmhXYzBOaU5WVnZZV2M0QTBnQVVCVSUzRA=="}},"icon":{"iconType":"MORE_VERT"},"targetId":"live-chat-action-panel-poll-context-menu"}},"liveChatPollType":"LIVE_CHAT_POLL_TYPE_CREATOR","metadataText":{"runs":[{"text":"@amautasau"},{"text":" • "},{"text":"5 時間前"},{"text":" • "},{"text":"317 票"}]},"pollQuestion":{"runs":[{"text":"高評価で応援お願いします"},{"emoji":{"emojiId":"🦖","image":{"accessibility":{"accessibilityData":{"label":"🦖"}},"thumbnails":[{"url":"https://fonts.gstatic.com/s/e/notoemoji/15.1/1f996/72.png"}]},"searchTerms":["t","rex"],"shortcuts":[":t_rex:"]}},{"text":"！"}]},"thumbnail":{"thumbnails":[{"height":32,"url":"https://yt4.ggpht.com/KuDmJxrSryEH71SFcQ9U9CaA-k8p_Py9MMmhS3be1ESn4NaUhEyFOOEWAElaI6xZB1iyMy9m_w=s32-c-k-c0x00ffffff-no-rj","width":32},{"height":64,"url":"https://yt4.ggpht.com/KuDmJxrSryEH71SFcQ9U9CaA-k8p_Py9MMmhS3be1ESn4NaUhEyFOOEWAElaI6xZB1iyMy9m_w=s64-c-k-c0x00ffffff-no-rj","width":64}]}}},"liveChatPollId":"ChwKGkNMdXMxNk9BcVpJREZmTFJ3Z1FkeUNFNl9n"}}}}
        "#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let a = parse_action(&action_json);
        assert_eq!(Action::UpdatePoll, a);
        Ok(())
    }
    #[tokio::test]
    async fn test_replace_chat_item_action() -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"{"replaceChatItemAction":{"replacementItem":{"liveChatTextMessageRenderer":{"authorExternalChannelId":"UCzaldqARrkEzL3hN8j1pyJA","authorName":{"simpleText":"@かす.-r"},"authorPhoto":{"thumbnails":[{"height":32,"url":"https://yt4.ggpht.com/ytc/AIdro_m9gY2yREG_m6tve7_Aw4mJQWdGAevU5k2niOTeP9BdWpc=s32-c-k-c0x00ffffff-no-rj","width":32},{"height":64,"url":"https://yt4.ggpht.com/ytc/AIdro_m9gY2yREG_m6tve7_Aw4mJQWdGAevU5k2niOTeP9BdWpc=s64-c-k-c0x00ffffff-no-rj","width":64}]},"contextMenuAccessibility":{"accessibilityData":{"label":"チャットの操作"}},"contextMenuEndpoint":{"commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMHhEY0RCUFlrOXdjRWxFUmxsbVIwWm5hMlJxUzNOMmVFRWFLU29uQ2hoVlEzbE1SMk54V1hNM1VuTkNZak5NTUZOS1pucEhXVUVTQ3pSYVpUTlFjMjVpVVVOTklBSW9CRElhQ2hoVlEzcGhiR1J4UVZKeWEwVjZURE5vVGpocU1YQjVTa0U0QWtnQVVBRSUzRA=="}},"id":"ChwKGkNMQ3AwT2JPcHBJREZZZkdGZ2tkaktzdnhB","message":{"runs":[{"text":"おい"},{"text":"w"}]},"timestampUsec":"1769341520715607"}},"targetItemId":"ChwKGkNMQ3AwT2JPcHBJREZZZkdGZ2tkaktzdnhB"}}"#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let result = parse_action(&action_json);
        assert!(matches!(result, Action::ReplaceChatItem));
        Ok(())
    }
    #[tokio::test]
    async fn a() -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"{"addChatItemAction":{"item":{"liveChatViewerEngagementMessageRenderer":{"actionButton":{"buttonRenderer":{"accessibilityData":{"accessibilityData":{"label":"詳細"}},"isDisabled":false,"navigationEndpoint":{"clickTrackingParams":"CBsQ8FsiEwjU9vbuzqmSAxX1sukFHUgVE2XKAQQu9Zrw","commandMetadata":{"webCommandMetadata":{"rootVe":83769,"url":"//support.google.com/youtube/?p=subs_only_chat_viewer&hl=ja","webPageType":"WEB_PAGE_TYPE_UNKNOWN"}},"urlEndpoint":{"target":"TARGET_NEW_WINDOW","url":"//support.google.com/youtube/?p=subs_only_chat_viewer&hl=ja"}},"size":"SIZE_DEFAULT","style":"STYLE_BLUE_TEXT","text":{"simpleText":"詳細"},"trackingParams":"CBsQ8FsiEwjU9vbuzqmSAxX1sukFHUgVE2U="}},"icon":{"iconType":"YOUTUBE_ROUND"},"id":"Ci0KK1NVQlNDUklCRVJTX09OTFlfVkVNMjAyNi8wMS8yNi0wODoyMzozNy4zNjQ%3D","message":{"runs":[{"text":"チャンネル登録者のみモード。このチャンネルの登録期間が "},{"text":"24 時間"},{"text":" 以上のユーザーからのメッセージが表示されます。"}]},"timestampUsec":"1769444617364806","trackingParams":"CAEQl98BIhMI1Pb27s6pkgMV9bLpBR1IFRNl"}}},"clickTrackingParams":"CAEQl98BIhMI1Pb27s6pkgMV9bLpBR1IFRNlygEELvWa8A=="}"#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let result = parse_action(&action_json);
        assert!(matches!(result, Action::ViewerEngagementMessage));
        Ok(())
    }
    #[tokio::test]
    async fn b() -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"{"addChatItemAction":{"clientId":"CIqiz-rOqZIDFeY_rQYdpXQcig","item":{"liveChatTextMessageRenderer":{"authorBadges":[{"liveChatAuthorBadgeRenderer":{"accessibility":{"accessibilityData":{"label":"メンバー（1 年）"}},"customThumbnail":{"thumbnails":[{"height":16,"url":"https://yt3.ggpht.com/ClyDE5R3mRDi0JqhkgEsOzCsxVThtWRCqy1OMZY0KlgmFMLttkPsfwPDgf2iPTZ7QizuzInrRQ=s16-c-k","width":16},{"height":32,"url":"https://yt3.ggpht.com/ClyDE5R3mRDi0JqhkgEsOzCsxVThtWRCqy1OMZY0KlgmFMLttkPsfwPDgf2iPTZ7QizuzInrRQ=s32-c-k","width":32}]},"tooltip":"メンバー（1 年）"}}],"authorExternalChannelId":"UCsb6cFzbDKlFMjxv0WA_cYw","authorName":{"simpleText":"@whitefox4229"},"authorPhoto":{"thumbnails":[{"height":32,"url":"https://yt4.ggpht.com/ytc/AIdro_km83l0N4nnREMosigKlGA3N5fyfRUDnEv5HTDyJZNNakA=s32-c-k-c0x00ffffff-no-rj","width":32},{"height":64,"url":"https://yt4.ggpht.com/ytc/AIdro_km83l0N4nnREMosigKlGA3N5fyfRUDnEv5HTDyJZNNakA=s64-c-k-c0x00ffffff-no-rj","width":64}]},"contextMenuAccessibility":{"accessibilityData":{"label":"チャットの操作"}},"contextMenuEndpoint":{"clickTrackingParams":"CAEQl98BIhMI1Pb27s6pkgMV9bLpBR1IFRNlygEELvWa8A==","commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMGx4YVhvdGNrOXhXa2xFUm1WWlgzSlJXV1J3V0ZGamFXY2FLU29uQ2hoVlEzRmpOMTl6YnpONFpGcEtibE5zWmtScWNHaDNjR2NTQzFZdE0wWk9kV2hIWW1GRklBSW9CRElhQ2hoVlEzTmlObU5HZW1KRVMyeEdUV3A0ZGpCWFFWOWpXWGM0QWtnQVVBRSUzRA=="}},"id":"ChwKGkNJcWl6LXJPcVpJREZlWV9yUVlkcFhRY2ln","message":{"runs":[{"emoji":{"emojiId":"UCqc7_so3xdZJnSlfDjphwpg/0_b4Zf3JCoi2_9EPk8GyyA4","image":{"accessibility":{"accessibilityData":{"label":"2424"}},"thumbnails":[{"height":24,"url":"https://yt3.ggpht.com/PGvh1Vjvy57g3EIy8XUU2qd7UQi2HDdlbp7jlB3bPJwGpO2kOpJrg5fwoMlYA0wV4Cl-_-kFRMQ=w24-h24-c-k-nd","width":24},{"height":48,"url":"https://yt3.ggpht.com/PGvh1Vjvy57g3EIy8XUU2qd7UQi2HDdlbp7jlB3bPJwGpO2kOpJrg5fwoMlYA0wV4Cl-_-kFRMQ=w48-h48-c-k-nd","width":48}]},"isCustomEmoji":true,"searchTerms":["_2424","2424"],"shortcuts":[":_2424:",":2424:"]}},{"emoji":{"emojiId":"UCqc7_so3xdZJnSlfDjphwpg/0_b4Zf3JCoi2_9EPk8GyyA4","image":{"accessibility":{"accessibilityData":{"label":"2424"}},"thumbnails":[{"height":24,"url":"https://yt3.ggpht.com/PGvh1Vjvy57g3EIy8XUU2qd7UQi2HDdlbp7jlB3bPJwGpO2kOpJrg5fwoMlYA0wV4Cl-_-kFRMQ=w24-h24-c-k-nd","width":24},{"height":48,"url":"https://yt3.ggpht.com/PGvh1Vjvy57g3EIy8XUU2qd7UQi2HDdlbp7jlB3bPJwGpO2kOpJrg5fwoMlYA0wV4Cl-_-kFRMQ=w48-h48-c-k-nd","width":48}]},"isCustomEmoji":true,"searchTerms":["_2424","2424"],"shortcuts":[":_2424:",":2424:"]}}]},"timestampUsec":"1769444608300982","trackingParams":"CAEQl98BIhMI1Pb27s6pkgMV9bLpBR1IFRNl"}}},"clickTrackingParams":"CAEQl98BIhMI1Pb27s6pkgMV9bLpBR1IFRNlygEELvWa8A=="}"#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let result = parse_action(&action_json);
        assert!(matches!(result, Action::LiveChatTextMessage1(_)));
        Ok(())
    }
    #[tokio::test]
    async fn test_live_chat_text_message_renderer_without_author_badges()
    -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"{"addChatItemAction":{"clientId":"CKWZ3frkqZIDFR3BwgQd3FYbLA","item":{"liveChatTextMessageRenderer":{"authorExternalChannelId":"UCChvnKzebvM0tcDDbj2Ee8A","authorName":{"simpleText":"@Ria_KK07238"},"authorPhoto":{"thumbnails":[{"height":32,"url":"https://yt4.ggpht.com/FBV-lsPfHEPz6a78D47ZQAl5sSssHr64vUrnSdmOHTwfnjDkWpu1gBxmhULrqtEtYlUL5Gd4cRw=s32-c-k-c0x00ffffff-no-rj","width":32},{"height":64,"url":"https://yt4.ggpht.com/FBV-lsPfHEPz6a78D47ZQAl5sSssHr64vUrnSdmOHTwfnjDkWpu1gBxmhULrqtEtYlUL5Gd4cRw=s64-c-k-c0x00ffffff-no-rj","width":64}]},"contextMenuAccessibility":{"accessibilityData":{"label":"チャットの操作"}},"contextMenuEndpoint":{"commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMHRYV2pObWNtdHhXa2xFUmxJelFuZG5VV1F6UmxsaVRFRWFLU29uQ2hoVlEzRmpOMTl6YnpONFpGcEtibE5zWmtScWNHaDNjR2NTQzFZdE0wWk9kV2hIWW1GRklBSW9CRElhQ2hoVlEwTm9kbTVMZW1WaWRrMHdkR05FUkdKcU1rVmxPRUU0QWtnQVVBRSUzRA=="}},"id":"ChwKGkNLV1ozZnJrcVpJREZSM0J3Z1FkM0ZZYkxB","message":{"runs":[{"text":"おつかれさまでした！"}]},"timestampUsec":"1769450547664748"}}}}"#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let result = parse_action(&action_json);
        assert!(matches!(result, Action::LiveChatTextMessage1(_)));
        Ok(())
    }
    #[tokio::test]
    async fn test_membership_action() -> Result<(), mcv_tracing::TracingError> {
        let sample_action = r#"{"addChatItemAction":{"clientId":"CLuzoPzkzJIDFXnBwgQd72o66g","item":{"liveChatMembershipItemRenderer":{"authorBadges":[{"liveChatAuthorBadgeRenderer":{"accessibility":{"accessibilityData":{"label":"メンバー（6 か月）"}},"customThumbnail":{"thumbnails":[{"height":16,"url":"https://yt3.ggpht.com/xRSwAO3CwSvSWwCJuilRdGYb-ds4jn7X3_fToTq3tIqTmhcKHDXD4lEebUotUNzSS2swL6NB=s16-c-k","width":16},{"height":32,"url":"https://yt3.ggpht.com/xRSwAO3CwSvSWwCJuilRdGYb-ds4jn7X3_fToTq3tIqTmhcKHDXD4lEebUotUNzSS2swL6NB=s32-c-k","width":32}]},"tooltip":"メンバー（6 か月）"}}],"authorExternalChannelId":"UCVXpwoUHh1v7VcfyseIpTEg","authorName":{"simpleText":"@K41-z1e"},"authorPhoto":{"thumbnails":[{"height":32,"url":"https://yt4.ggpht.com/4otcpEywmhlyWMmF0kbipBTbixC2jQQI2CINMJKl7QupQBfpnS9OQc0qshtDoj1I9maenb1jJg=s32-c-k-c0x00ffffff-no-rj","width":32},{"height":64,"url":"https://yt4.ggpht.com/4otcpEywmhlyWMmF0kbipBTbixC2jQQI2CINMJKl7QupQBfpnS9OQc0qshtDoj1I9maenb1jJg=s64-c-k-c0x00ffffff-no-rj","width":64}]},"contextMenuAccessibility":{"accessibilityData":{"label":"チャットの操作"}},"contextMenuEndpoint":{"clickTrackingParams":"CAUQ4P0GIhMIqdvi_uTMkgMVWoSmAx2dDDykygEEo6F3mw==","commandMetadata":{"webCommandMetadata":{"ignoreNavigation":true}},"liveChatItemContextMenuEndpoint":{"params":"Q2g0S0hBb2FRMHgxZW05UWVtdDZTa2xFUmxodVFuZG5VV1EzTW04Mk5tY2FLU29uQ2hoVlEyeFRNMk51U1ZWTk9YbDZjMEpRVVhwbGVWaGZPRkVTQzFoTlUwSkpVVzFMVkRsM0lBSW9CRElhQ2hoVlExWlljSGR2VlVob01YWTNWbU5tZVhObFNYQlVSV2M0QWtnQVVBUSUzRA=="}},"headerSubtext":{"runs":[{"text":"トレーナーさん"},{"text":" へようこそ！"}]},"id":"ChwKGkNMdXpvUHprekpJREZYbkJ3Z1FkNzJvNjZn","timestampUsec":"1770653141704914","trackingParams":"CAUQ4P0GIhMIqdvi_uTMkgMVWoSmAx2dDDyk"}}},"clickTrackingParams":"CAEQl98BIhMIqdvi_uTMkgMVWoSmAx2dDDykygEEo6F3mw=="}"#.trim();
        let action_json: serde_json::Value = serde_json::from_str(sample_action).unwrap();
        let result = parse_action(&action_json);
        assert!(matches!(result, Action::Membership));
        Ok(())
    }
}
