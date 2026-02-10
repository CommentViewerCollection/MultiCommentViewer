use crate::utils::{get_string, get_value, send_graphql_query};
use crate::video_id::VideoId;
use crate::{auth_token::AuthToken, client_id::ClientId};
use anyhow::Result;

struct ComscoreStreamingQueryVideo {
    broadcast_type: String,
    created_at: String,
    id: String,
    length_seconds: u32,
    title: String,
    owner_display_name: String,
    owner_id: String,
}
async fn get_comscore_streaming_query(
    video_id: &VideoId,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<ComscoreStreamingQueryVideo> {
  //本来のAPIでは生配信の情報も取れるけど、他のAPIでも取れるからここでは動画だけにする
  //ちなみに生配信の情報を取るにはchannelに配信者のIDを入れてisLiveをtrueにする
    let query = format!(
        r#"{{
  "operationName": "ComscoreStreamingQuery",
  "variables": {{
    "channel": "",
    "clipSlug": "",
    "isClip": false,
    "isLive": false,
    "isVodOrCollection": true,
    "vodID": "{video_id}"
  }},
  "extensions": {{
    "persistedQuery": {{
      "version": 1,
      "sha256Hash": "e1edae8122517d013405f237ffcc124515dc6ded82480a88daef69c83b53ac01"
    }}
  }}
}}"#,
        video_id = video_id.value()
    );
    let json = send_graphql_query(&query, client_id, auth_token).await?;

    let video = get_value(&json, &["data", "video"])?;
    let broadcast_type = get_string(&video, &["broadcastType"])?;
    let created_at = get_string(&video, &["createdAt"])?;
    let id = get_string(&video, &["id"])?;
    let length_seconds = get_value(&video, &["lengthSeconds"])?
        .as_u64()
        .ok_or_else(|| anyhow::anyhow!("Missing lengthSeconds"))? as u32;
    let title = get_string(&video, &["title"])?;
    let owner_display_name = get_string(&video, &["owner", "displayName"])?;
    let owner_id = get_string(&video, &["owner", "id"])?;
    Ok(ComscoreStreamingQueryVideo {
        broadcast_type,
        created_at,
        id,
        length_seconds,
        title,
        owner_display_name,
        owner_id,
    })
}
#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn test_comscore() {
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let video_id = VideoId::new("2672403100");
        let auth_token = None;
        match get_comscore_streaming_query(&video_id, &client_id, auth_token).await {
            Ok(comscore_data) => {
                println!("{}", comscore_data.title);
            }
            Err(e) => {
                eprintln!("Error: {}", e);
            }
        }
    }
}
