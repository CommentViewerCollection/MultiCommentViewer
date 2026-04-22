use crate::auth_token::AuthToken;
use crate::client_id::ClientId;
use crate::utils::send_graphql_query;
use anyhow::Result;
use chrono::DateTime;

/// Twitch ストリームのメタデータ
pub struct TwitchStreamInfo {
    pub title: Option<String>,
    /// 配信開始時刻（Unix 秒）
    pub start_time: Option<i64>,
}

struct StreamMetadata {
    last_broadcast_title: Option<String>,
    stream_created_at: Option<String>,
}

/// GQL Persisted Query で配信タイトルと開始時刻を取得する（認証不要）
async fn get_stream_metadata(
    channel_login: &str,
    client_id: &ClientId,
    user_agent: &str,
) -> anyhow::Result<StreamMetadata> {
    //{"operationName":"StreamMetadata","variables":{"channelLogin":"amauta_sau","includeIsDJ":true},"extensions":{"persistedQuery":{"version":1,"sha256Hash":"b57f9b910f8cd1a4659d894fe7550ccc81ec9052c01e438b290fd66a040b9b93"}}}

    //response
    //
    // {
    //     "data": {
    //         "user": {
    //             "id": "825030170",
    //             "primaryColorHex": "65BFF1",
    //             "roles": {
    //                 "isPartner": true,
    //                 "isParticipatingDJ": false,
    //                 "__typename": "UserRoles"
    //             },
    //             "profileImageURL": "https://static-cdn.jtvnw.net/jtv_user_pictures/4a66d3c0-471f-4814-b7a2-5448da3fc027-profile_image-70x70.png",
    //             "primaryTeam": null,
    //             "channel": {
    //                 "id": "825030170",
    //                 "__typename": "Channel"
    //             },
    //             "lastBroadcast": {
    //                 "id": "315344929393",
    //                 "title": "ストグラseason２ 11日目🐼",
    //                 "__typename": "Broadcast"
    //             },
    //             "stream": {
    //                 "id": "315344929393",
    //                 "type": "live",
    //                 "createdAt": "2026-01-15T12:15:50Z",
    //                 "game": {
    //                     "id": "32982",
    //                     "slug": "grand-theft-auto-v",
    //                     "name": "Grand Theft Auto V",
    //                     "__typename": "Game"
    //                 },
    //                 "__typename": "Stream"
    //             },
    //             "__typename": "User"
    //         }
    //     },
    //     "extensions": {
    //         "durationMilliseconds": 47,
    //         "operationName": "StreamMetadata",
    //         "requestID": "01KF102N9VG7CNMFZ6XT2KQNJZ"
    //     }
    // }

    let query = format!(
        r#"{{
            "operationName":"StreamMetadata",
            "variables":{{"channelLogin":"{}","includeIsDJ":true}},
            "extensions":{{"persistedQuery":{{"version":1,"sha256Hash":"b57f9b910f8cd1a4659d894fe7550ccc81ec9052c01e438b290fd66a040b9b93"}}}}
        }}"#,
        channel_login
    );
    let json = send_graphql_query(&query, client_id, None, user_agent).await?;
    let data = &json["data"]["user"];
    let last_broadcast_title = data["lastBroadcast"]["title"]
        .as_str()
        .map(|s| s.to_string());
    let stream_created_at = data["stream"]["createdAt"].as_str().map(|s| s.to_string());

    Ok(StreamMetadata {
        last_broadcast_title,
        stream_created_at,
    })
}

/// GQL Persisted Query でチャンネルの数値 ID（broadcaster_id）を取得する（認証不要）
pub async fn fetch_broadcaster_id(
    channel_login: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
    user_agent: &str,
) -> Result<Option<String>> {
    let query = format!(
        r#"{{
            "operationName":"StreamMetadata",
            "variables":{{"channelLogin":"{}","includeIsDJ":true}},
            "extensions":{{"persistedQuery":{{"version":1,"sha256Hash":"b57f9b910f8cd1a4659d894fe7550ccc81ec9052c01e438b290fd66a040b9b93"}}}}
        }}"#,
        channel_login
    );
    let json = send_graphql_query(&query, client_id, auth_token, user_agent).await?;
    let id = json["data"]["user"]["id"].as_str().map(|s| s.to_string());
    Ok(id)
}

/// Twitch チャンネルのストリームメタデータ（タイトル・開始時刻）を取得する。
///
/// GQL Persisted Query を使用（認証不要）。
/// 視聴者数は Hermes WebSocket の video-playback-by-id トピックから取得するため、ここでは取得しない。
pub async fn fetch_stream_info(channel_login: &str, user_agent: &str) -> Result<TwitchStreamInfo> {
    let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");

    let gql_meta = get_stream_metadata(channel_login, &client_id, user_agent).await?;

    let start_time = gql_meta
        .stream_created_at
        .as_deref()
        .and_then(|s| DateTime::parse_from_rfc3339(s).ok())
        .map(|dt| dt.timestamp());

    Ok(TwitchStreamInfo {
        title: gql_meta.last_broadcast_title,
        start_time,
    })
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn test_fetch_stream_info() {
        let channel_login = "amauta_sau";
        match fetch_stream_info(channel_login, "test-agent").await {
            Ok(info) => {
                println!("Title: {:?}", info.title);
                println!("Start Time: {:?}", info.start_time);
            }
            Err(e) => {
                eprintln!("Error fetching stream info: {}", e);
            }
        }
    }
}
