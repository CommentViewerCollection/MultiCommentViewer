use serde_json::Value;
use anyhow::Result;
use crate::client_id::ClientId;
use crate::auth_token::AuthToken;
use crate::utils::{get_string, get_value, send_graphql_query};

struct StreamMetadata {
    user_id: String,
    last_broadcast_id: Option<String>,
    last_broadcast_title: Option<String>,
    stream_type: Option<String>,
    stream_created_at: Option<String>,
}

async fn get_stream_metadata(
    channel_login: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<StreamMetadata, reqwest::Error> {
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
    //send query and parse response
    let json = send_graphql_query(&query, client_id, auth_token).await?;
    let data = &json["data"]["user"];
    let user_id = data["id"].as_str().unwrap_or("").to_string();
    let last_broadcast_id = data["lastBroadcast"]["id"].as_str().map(|s| s.to_string());
    let last_broadcast_title = data["lastBroadcast"]["title"]
        .as_str()
        .map(|s| s.to_string());
    let stream_type = data["stream"]["type"].as_str().map(|s| s.to_string());
    let stream_created_at = data["stream"]["createdAt"].as_str().map(|s| s.to_string());

    let metadata = StreamMetadata {
        user_id,
        last_broadcast_id,
        last_broadcast_title,
        stream_type,
        stream_created_at,
    };
    Ok(metadata)
}

#[cfg(test)]
mod tests {
    use super::*;
    #[tokio::test]
    async fn test_get_stream_metadata() {
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let channel_login = "amauta_sau";
        let auth_token = None;
        match get_stream_metadata(channel_login, &client_id, auth_token).await {
            Ok(metadata) => {
                println!("User ID: {}", metadata.user_id);
                println!(
                    "Last Broadcast ID: {}",
                    metadata
                        .last_broadcast_id
                        .unwrap_or_else(|| "none".to_string())
                );
                println!(
                    "Last Broadcast Title: {}",
                    metadata
                        .last_broadcast_title
                        .unwrap_or_else(|| "none".to_string())
                );
                println!(
                    "Stream Type: {}",
                    metadata.stream_type.unwrap_or_else(|| "none".to_string())
                );
                println!(
                    "Stream Created At: {}",
                    metadata
                        .stream_created_at
                        .unwrap_or_else(|| "none".to_string())
                );
            }
            Err(e) => {
                eprintln!("Error fetching stream metadata: {}", e);
            }
        }
    }
}
