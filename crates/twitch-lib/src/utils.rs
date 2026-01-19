use crate::client_id::ClientId;
use crate::auth_token::AuthToken;
use anyhow::Result;

pub async fn send_graphql_query(
    query: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<serde_json::Value, reqwest::Error> {
    let client = reqwest::Client::new();
    let res = client
        .post("https://gql.twitch.tv/gql")
        .header("Client-ID", client_id.value())
        .header("Content-Type", "application/json");
    let res = if let Some(token) = auth_token {
        res.header("Authorization", format!("OAuth {}", token.value()))
    } else {
        res
    };
    let res = res.body(query.to_string()).send().await?;
    let json: serde_json::Value = res.json().await.unwrap();
    Ok(json)
}
pub fn get_string(value: &serde_json::Value, path: &[&str]) -> Result<String> {
    path.iter()
        .try_fold(value, |acc, key| acc.get(key))
        .and_then(|v| v.as_str())
        .map(|s| s.to_string())
        .ok_or_else(|| anyhow::anyhow!("Missing string value"))
}
pub fn get_value<'a>(
    value: &'a serde_json::Value,
    path: &'a [&'a str],
) -> Result<&'a serde_json::Value> {
    path.iter()
        .try_fold(value, |acc, key| acc.get(key))
        .ok_or_else(|| anyhow::anyhow!("Missing value"))
}
pub fn get_auth_token_from_env()->Option<String>{
    dotenvy::dotenv().ok()?;
    std::env::var("AUTH_TOKEN").ok()
}