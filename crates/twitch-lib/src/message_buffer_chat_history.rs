use crate::auth_token::AuthToken;
use crate::client_id::ClientId;
use crate::utils::{get_string, get_value, send_graphql_query};
use anyhow::{Context, Result};
use serde_json::Value;

#[derive(Debug)]
pub struct RecentChatMessageFragment {
    pub text: String,
    pub content: Option<String>,
}
#[derive(Debug)]
pub struct SenderBadge {
    pub set_id: String,
    pub version: String,
    pub id: String,
}

#[derive(Debug)]
pub struct RecentChatMessage {
    pub message_id: String,
    pub deleted_at: Option<String>,
    pub sent_at: String,
    pub text: String,
    pub fragments: Vec<RecentChatMessageFragment>,
    pub parent_message: Option<String>,
    pub thread_parent_message: Option<String>,
    pub sender_id: String,
    pub sender_login: String,
    pub sender_display_name: String,
    pub sender_badges: Vec<SenderBadge>,
}
impl RecentChatMessage {
    fn parse(v: &Value) -> Result<Self> {
        let message_id = v
            .get("id")
            .and_then(|v| v.as_str())
            .ok_or_else(|| anyhow::anyhow!("Missing message id"))?
            .to_owned();
        let deleted_at = v["deletedAt"].as_str().map(|s| s.to_owned());
        let sent_at = v
            .get("sentAt")
            .and_then(|v| v.as_str())
            .ok_or_else(|| anyhow::anyhow!("Missing sent at"))?
            .to_owned();
        let text = get_string(v, &["content", "text"])?;
        let mut fragments = Vec::new();

        for fragment in get_value(v, &["content", "fragments"])?
            .as_array()
            .unwrap_or(&Vec::new())
        {
            let fragment_text = fragment["text"].as_str().unwrap_or("").to_string();
            let content = fragment["content"].as_str().map(|s| s.to_string());
            fragments.push(RecentChatMessageFragment {
                text: fragment_text,
                content,
            });
        }
        let parent_message = v["parentMessage"].as_str().map(|s| s.to_string());
        let thread_parent_message = v["threadParentMessage"].as_str().map(|s| s.to_string());
        let sender = &v["sender"];
        let sender_id = sender["id"].as_str().unwrap_or("").to_string();
        let sender_login = sender["login"].as_str().unwrap_or("").to_string();
        let sender_display_name = sender["displayName"].as_str().unwrap_or("").to_string();
        let mut sender_badges = Vec::new();
        for badge in sender["badges"].as_array().unwrap_or(&Vec::new()) {
            let set_id = badge["setID"].as_str().unwrap_or("").to_string();
            let version = badge["version"].as_str().unwrap_or("").to_string();
            let id = badge["id"].as_str().unwrap_or("").to_string();
            sender_badges.push(SenderBadge {
                set_id,
                version,
                id,
            });
        }
        Ok(RecentChatMessage {
            message_id,
            deleted_at,
            sent_at,
            text,
            fragments,
            parent_message,
            thread_parent_message,
            sender_id,
            sender_login,
            sender_display_name,
            sender_badges,
        })
    }
}
pub async fn fetch_recent_chat_messages(
    channel_login: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<Vec<RecentChatMessage>> {
    let query = format!(
        r#"{{
  "operationName": "MessageBufferChatHistory",
  "variables": {{
    "channelLogin": "{channel_login}"
  }},
  "extensions": {{
    "persistedQuery": {{
      "version": 1,
      "sha256Hash": "33dba0e0c249135052e930cbd6c4a66daa32249ba00d1c8def75857fa3f3431d"
    }}
  }}
}}"#
    );

    //send query and parse response
    let json = send_graphql_query(&query, client_id, auth_token).await?;

    //parse json to RecentChatMessage
    let mut messages = Vec::new();
    let data = get_value(&json, &["data", "channel", "recentChatMessages"])
        .with_context(|| format!("response body: {json}"))?;
    for message in data.as_array().unwrap_or(&Vec::new()) {
        let recent_message = RecentChatMessage::parse(message)?;
        messages.push(recent_message);
    }
    Ok(messages)
}

#[cfg(test)]
mod chat_tests {
    use super::*;
    #[tokio::test]
    async fn test_get_recent_chat_messages() {
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let channel_login = "stray_channel";
        let auth_token = AuthToken::new("ypp1z781ywsilphtfyo2jyfo7aqtw7");
        match fetch_recent_chat_messages(channel_login, &client_id, Some(&auth_token)).await {
            Ok(messages) => {
                for message in messages {
                    println!("Message ID: {}", message.message_id);
                    // println!("Sent At: {}", message.sent_at);
                    //  println!("Text: {}", message.text);
                    // println!("Sender Login: {}", message.sender_login);
                    for f in &message.fragments {
                        // println!("  Fragment Text: {}", f.text);
                        if let Some(content) = &f.content {
                            println!("    Content: {}", content);
                        }
                    }
                    // println!("{:#?}", message.fragments);
                }
            }
            Err(e) => {
                eprintln!("Error fetching recent chat messages: {}", e);
            }
        }
    }
}
