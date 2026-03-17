use crate::{
    auth_token::AuthToken,
    client_id::ClientId,
    integrity::Integrity,
    utils::{get_string, get_value},
    video_id::VideoId,
};
use anyhow::Result;

#[allow(dead_code)]
pub enum OffsetOrCursor {
    Offset(u32),
    Cursor(String),
}
#[allow(dead_code)]
struct VideoComments {
    video_id: String,
    creator_id: String,
    comments: Vec<VideoCommentEdge>,
    has_next_page: bool,
}
#[allow(dead_code)]
struct VideoCommentEdge {
    cursor: String,
    id: String,
    commenter_id: String,
    commenter_login: String,
    commenter_display_name: String,
    offset_seconds: u32,
    created_at: String,
    message_fragments: Vec<MessageFragment>,
    user_badges: Vec<UserBadge>,
    has_next_page: bool,
    has_previous_page: bool,
}
#[allow(dead_code)]
struct MessageFragment {
    text: String,
    emotes: Vec<Emote>,
}
#[allow(dead_code)]
struct Emote {}
#[allow(dead_code)]
struct UserBadge {
    id: String,
    set_id: String,
    version: String,
}
#[allow(dead_code)]
async fn get_video_comments(
    video_id: &VideoId,
    offset_or_cursor: OffsetOrCursor,
    auth_token: &AuthToken,
    client_id: &ClientId,
    integrity: &Integrity,
    interval_millis: u64,
) -> Result<Vec<VideoCommentEdge>> {
    let mut all_comments = Vec::new();

    let mut next_offset_or_cursor = offset_or_cursor;
    loop {
        let comments = get_video_comments_by_offset_or_cursor(
            video_id,
            next_offset_or_cursor,
            auth_token,
            Some(integrity),
            client_id,
        )
        .await;

        tokio::time::sleep(std::time::Duration::from_millis(interval_millis)).await;
        match comments {
            Ok(video_comments) => {
                if video_comments.has_next_page == false {
                    break;
                }
                let num_comments = video_comments.comments.len();
                let last_comment = &video_comments.comments[num_comments - 1];
                next_offset_or_cursor = OffsetOrCursor::Cursor(last_comment.cursor.clone());
                all_comments.extend(video_comments.comments);
                println!("今回取得したコメントの数{}", num_comments);
                println!("総取得コメント数{}", all_comments.len());
            }
            Err(_) => {
                break;
            }
        }
    }
    Ok(all_comments)
}
#[allow(dead_code)]
async fn get_video_comments_by_offset_or_cursor(
    video_id: &VideoId,
    offset_or_cursor: OffsetOrCursor,
    auth_token: &AuthToken,
    integrity: Option<&Integrity>,
    client_id: &ClientId,
) -> Result<VideoComments> {
    let query = match offset_or_cursor {
        OffsetOrCursor::Offset(offset) => format!(
            r#"{{"operationName": "VideoCommentsByOffsetOrCursor",
            "variables": {{
            "videoID": "{vid}",
            "contentOffsetSeconds": {offset}
            }},"extensions": {{"persistedQuery": {{"version": 1,"sha256Hash": "b70a3591ff0f4e0313d126c6a1502d79a1c02baebb288227c582044aa76adf6a"}}}}}}"#,
            vid = video_id.value()
        ),
        OffsetOrCursor::Cursor(cursor) => format!(
            r#"{{"operationName": "VideoCommentsByOffsetOrCursor",
            "variables": {{
            "videoID": "{vid}","cursor": "{cursor}"}},
            "extensions": {{"persistedQuery": {{"version": 1,"sha256Hash": "b70a3591ff0f4e0313d126c6a1502d79a1c02baebb288227c582044aa76adf6a"}}}}}}"#,
            vid = video_id.value()
        ),
    };

    let res = reqwest::Client::new()
        .post("https://gql.twitch.tv/gql")
        .header("Client-Id", client_id.value())
        .header("Content-Type", "application/json");
    let res = res.header(
        "Authorization",
        format!("OAuth {token}", token = auth_token.value()),
    );
    let res = if let Some(int) = integrity {
        let res = res.header("client-integrity", int.token());
        res
    } else {
        res
    };

    let res = res.body(query).send().await?.text().await?;
    println!("{res}");

    let json = serde_json::from_str::<serde_json::Value>(&res)?;
    let creator_id = get_string(&json, &["data", "video", "creator", "id"])?;
    let edges = get_value(&json, &["data", "video", "comments", "edges"])?;
    let mut comments = Vec::new();
    for edge in edges.as_array().unwrap_or(&Vec::new()) {
        let cursor = get_string(edge, &["cursor"])?;
        let node = get_value(edge, &["node"])?;
        let id = get_string(&node, &["id"])?;
        let commenter = get_value(&node, &["commenter"])?;
        let commenter_id = get_string(&commenter, &["id"])?;
        let commenter_login = get_string(&commenter, &["login"])?;
        let commenter_display_name = get_string(&commenter, &["displayName"])?;
        let offset_seconds = node
            .get("contentOffsetSeconds")
            .and_then(|v| v.as_u64())
            .ok_or_else(|| anyhow::anyhow!("Failed to get contentOffsetSeconds"))?
            as u32;
        let created_at = get_string(&node, &["createdAt"])?;
        let mut message_fragments = Vec::new();
        let fragments = get_value(&node, &["message", "fragments"])?;
        for fragment in fragments.as_array().unwrap_or(&Vec::new()) {
            let text = get_string(fragment, &["text"])?;
            let emotes = Vec::new();
            let fragment = MessageFragment { text, emotes };
            message_fragments.push(fragment);
        }
        let mut user_badges = Vec::new();
        let user_badges_value = get_value(&node, &["message", "userBadges"])?;
        for badge in user_badges_value.as_array().unwrap_or(&Vec::new()) {
            let id = get_string(badge, &["id"])?;
            let set_id = get_string(badge, &["setID"])?;
            let version = get_string(badge, &["version"])?;
            let user_badge = UserBadge {
                id,
                set_id,
                version,
            };
            user_badges.push(user_badge);
        }

        let comment = VideoCommentEdge {
            cursor,
            id,
            commenter_id,
            commenter_login,
            commenter_display_name,
            offset_seconds,
            created_at,
            message_fragments,
            user_badges,
            has_next_page: false,
            has_previous_page: false,
        };
        comments.push(comment);
    }
    let has_next_page = get_value(
        &json,
        &["data", "video", "comments", "pageInfo", "hasNextPage"],
    )?
    .as_bool()
    .ok_or_else(|| anyhow::anyhow!(""))?;

    Ok(VideoComments {
        video_id: video_id.value().to_owned(),
        creator_id,
        comments,
        has_next_page,
    })
}
#[cfg(test)]
mod tests {
    use crate::utils::get_auth_token_from_env;

    use super::*;
    #[tokio::test]
    async fn test_get_video_comments_by_offset_or_cursor() {
        let video_id = VideoId::new("2669748313");
        let auth_token = AuthToken::new(&get_auth_token_from_env().unwrap());
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let offset_or_cursor = OffsetOrCursor::Offset(13649);
        let result = get_video_comments_by_offset_or_cursor(
            &video_id,
            offset_or_cursor,
            &auth_token,
            None,
            &client_id,
        )
        .await;
        match result {
            Ok(video_comments) => {
                // Add assertions based on expected video_comments structure
            }
            Err(e) => {
                panic!("Error fetching video comments: {:?}", e);
            }
        }
    }
    #[tokio::test]
    async fn test_get_video_comments() {
        let video_id = VideoId::new("2667124934");
        let auth_token = AuthToken::new(&get_auth_token_from_env().unwrap());
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let offset_or_cursor = OffsetOrCursor::Offset(0);

        let integrity = get_integrity(&client_id, &auth_token).await.unwrap();

        let result = get_video_comments(
            &video_id,
            offset_or_cursor,
            &auth_token,
            &client_id,
            &integrity,
            1000,
        )
        .await;
        match result {
            Ok(all_comments) => {
                // Add assertions based on expected all_comments structure
            }
            Err(e) => {
                panic!("Error fetching all video comments: {:?}", e);
            }
        }
    }
}
