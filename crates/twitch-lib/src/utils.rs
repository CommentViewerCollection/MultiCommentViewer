use crate::auth_token::AuthToken;
use crate::client_id::ClientId;
use anyhow::Result;
use mcv_plugin_telemetry::{TracingError, capture_context};

const GQL_URL: &str = "https://gql.twitch.tv/gql";

pub async fn send_graphql_query(
    query: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
    user_agent: &str,
) -> Result<serde_json::Value, TracingError> {
    let client = reqwest::Client::builder()
        .user_agent(user_agent)
        .build()
        .unwrap_or_default();
    let res = client
        .post(GQL_URL)
        .header("Client-ID", client_id.value())
        .header("Content-Type", "application/json")
        .header("Accept-Language", "ja-JP");
    let res = if let Some(token) = auth_token {
        res.header("Authorization", format!("OAuth {}", token.value()))
    } else {
        res
    };
    let res = res.body(query.to_string()).send().await.map_err(|e| {
        tracing::error!(
            target: "mcv::twitch-lib",
            url = GQL_URL,
            error = %e,
            "GraphQL リクエスト送信失敗"
        );
        capture_context!(
            "GraphQL リクエスト送信失敗",
            url = GQL_URL,
            error = e.to_string()
        )
    })?;

    let status = res.status();
    let body = res.text().await.map_err(|e| {
        tracing::error!(
            target: "mcv::twitch-lib",
            url = GQL_URL,
            status = %status,
            error = %e,
            "GraphQL レスポンスボディ読み取り失敗"
        );
        capture_context!(
            "GraphQL レスポンスボディ読み取り失敗",
            url = GQL_URL,
            status = status.as_u16(),
            error = e.to_string()
        )
    })?;

    if !status.is_success() {
        tracing::error!(
            target: "mcv::twitch-lib",
            url = GQL_URL,
            status = %status,
            response_body = %body.chars().take(500).collect::<String>(),
            "GraphQL HTTP エラー"
        );
        return Err(capture_context!(
            "GraphQL HTTP エラー",
            url = GQL_URL,
            status = status.as_u16(),
            response_body = body.chars().take(500).collect::<String>()
        )
        .into());
    }

    serde_json::from_str::<serde_json::Value>(&body)
        .map_err(|e| {
            tracing::error!(
                target: "mcv::twitch-lib",
                url = GQL_URL,
                status = %status,
                error = %e,
                response_body = %body.chars().take(500).collect::<String>(),
                "GraphQL レスポンス JSON パース失敗"
            );
            capture_context!(
                "GraphQL レスポンス JSON パース失敗",
                url = GQL_URL,
                status = status.as_u16(),
                error = e.to_string(),
                response_body = body.chars().take(500).collect::<String>()
            )
        })
        .map_err(TracingError::from)
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
#[allow(dead_code)]
pub fn get_auth_token_from_env() -> Option<String> {
    dotenvy::dotenv().ok()?;
    std::env::var("AUTH_TOKEN").ok()
}
