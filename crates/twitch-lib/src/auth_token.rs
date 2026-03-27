#[derive(Clone)]
pub struct AuthToken {
    token: String,
}
impl AuthToken {
    pub fn new(token: &str) -> Self {
        AuthToken {
            token: token.to_string(),
        }
    }
    pub fn value(&self) -> &str {
        &self.token
    }
}

/// `GET https://id.twitch.tv/oauth2/validate` のレスポンス
#[derive(Debug, serde::Deserialize)]
pub struct ValidateTokenResponse {
    pub user_id: String,
    pub login: String,
}

/// auth-token を Twitch の OAuth 検証エンドポイントで確認し、ユーザー情報を返す
pub async fn validate_token(token: &str) -> Result<ValidateTokenResponse, String> {
    let client = reqwest::Client::new();
    let response = client
        .get("https://id.twitch.tv/oauth2/validate")
        .header("Authorization", format!("OAuth {}", token))
        .send()
        .await
        .map_err(|e| e.to_string())?;

    let status = response.status();
    if !status.is_success() {
        let body = response
            .text()
            .await
            .unwrap_or_else(|_| "(ボディ読み取り失敗)".to_string());
        return Err(format!(
            "Token validation failed: HTTP {} - {}",
            status,
            body.chars().take(200).collect::<String>()
        ));
    }

    response
        .json::<ValidateTokenResponse>()
        .await
        .map_err(|e| e.to_string())
}
