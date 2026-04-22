use crate::{auth_token::AuthToken, client_id::ClientId, utils::send_graphql_query};
use anyhow::Result;

/// Twitch GQL `BlockedUsers` クエリでブロック済みユーザーIDリストを取得する
pub async fn fetch_blocked_users(
    client_id: &ClientId,
    auth_token: &AuthToken,
    user_agent: &str,
) -> Result<Vec<String>> {
    let query = r#"{
        "operationName": "BlockedUsers",
        "variables": {},
        "extensions": {
            "persistedQuery": {
                "version": 1,
                "sha256Hash": "8044e3fd61f8158a39e07b38f5d1a279d1fdb748faa9889fde046feae640f76b"
            }
        }
    }"#;
    let json = send_graphql_query(query, client_id, Some(auth_token), user_agent).await?;

    let user_ids = json["data"]["currentUser"]["blockedUsers"]
        .as_array()
        .map(|arr| {
            arr.iter()
                .filter_map(|u| u["id"].as_str().map(|s| s.to_string()))
                .collect()
        })
        .unwrap_or_default();

    Ok(user_ids)
}
