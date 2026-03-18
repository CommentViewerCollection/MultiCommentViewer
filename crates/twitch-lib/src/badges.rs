use std::collections::HashMap;

use anyhow::Result;

use crate::{auth_token::AuthToken, client_id::ClientId, utils::send_graphql_query};

/// バッジキャッシュ: set_id → version_id → image_url
pub type BadgeCache = HashMap<String, HashMap<String, String>>;

/// グローバルバッジを GQL `GlobalBadges` で取得する（認証不要）
///
/// レスポンスパス: `data.badges[]`
pub async fn fetch_global_badges_gql(
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<BadgeCache> {
    let query = r#"{
        "operationName": "GlobalBadges",
        "variables": {},
        "extensions": {
            "persistedQuery": {
                "version": 1,
                "sha256Hash": "9db27e18d61ee393ccfdec8c7d90f14f9a11266298c2e5eb808550b77d7bcdf6"
            }
        }
    }"#;
    let json = send_graphql_query(query, client_id, auth_token).await?;

    let Some(badges) = json["data"]["badges"].as_array() else {
        return Ok(BadgeCache::new());
    };
    Ok(parse_badge_array(badges))
}

/// チャンネルバッジを GQL `ChatList_Badges` で取得する（認証不要）
///
/// レスポンスパス: `data.user.broadcastBadges[]`
pub async fn fetch_channel_badges_gql(
    channel_login: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<BadgeCache> {
    let query = format!(
        r#"{{
            "operationName": "ChatList_Badges",
            "variables": {{
                "channelLogin": "{}"
            }},
            "extensions": {{
                "persistedQuery": {{
                    "version": 1,
                    "sha256Hash": "838a7e0b47c09cac05f93ff081a9ff4f876b68f7624f0fc465fe30031e372fc2"
                }}
            }}
        }}"#,
        channel_login
    );
    let json = send_graphql_query(&query, client_id, auth_token).await?;

    let Some(badges) = json["data"]["user"]["broadcastBadges"].as_array() else {
        return Ok(BadgeCache::new());
    };
    Ok(parse_badge_array(badges))
}

/// バッジ配列（`[{ setID, version, image2x, image1x }]` 形式）を BadgeCache に変換する
fn parse_badge_array(badges: &[serde_json::Value]) -> BadgeCache {
    let mut cache = BadgeCache::new();
    for badge in badges {
        let Some(set_id) = badge["setID"].as_str() else {
            continue;
        };
        let Some(version) = badge["version"].as_str() else {
            continue;
        };
        // 2x 画像を優先、なければ 1x を使用
        let image_url = badge["image2x"]
            .as_str()
            .or_else(|| badge["image1x"].as_str())
            .unwrap_or("")
            .to_string();
        if !image_url.is_empty() {
            cache
                .entry(set_id.to_string())
                .or_default()
                .insert(version.to_string(), image_url);
        }
    }
    cache
}
