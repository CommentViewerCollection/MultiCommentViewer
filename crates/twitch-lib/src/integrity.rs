use crate::{auth_token::AuthToken, client_id::ClientId};
use anyhow::Result;

#[allow(dead_code)]
pub struct Integrity {
    client_id: String,
    auth_token: String,
    token: String,
}
#[allow(dead_code)]
impl Integrity {
    fn new(client_id: &str, auth_token: &str, token: &str) -> Self {
        Integrity {
            client_id: client_id.to_string(),
            auth_token: auth_token.to_string(),
            token: token.to_string(),
        }
    }
    pub fn token(&self) -> &str {
        &self.token
    }
}
#[allow(dead_code)]
pub async fn get_integrity(
    client_id: &ClientId,
    auth_token: &AuthToken,
    user_agent: &str,
) -> Result<Integrity> {
    let url = "https://gql.twitch.tv/integrity";

    let query = "";
    let res = reqwest::Client::builder()
        .user_agent(user_agent)
        .build()?
        .post(url)
        .header("Client-Id", client_id.value())
        .header("Authorization", format!("OAuth {}", auth_token.value()))
        .body(query)
        .send()
        .await?
        .text()
        .await?;
    let json = serde_json::from_str::<serde_json::Value>(&res)?;
    println!("integrity response json: {}", json);
    let integrity = Integrity::new(
        client_id.value(),
        auth_token.value(),
        json.get("token")
            .and_then(|v| v.as_str())
            .ok_or_else(|| anyhow::anyhow!(""))?,
    );
    Ok(integrity)
}
#[cfg(test)]
mod tests {
    use crate::utils::get_auth_token_from_env;

    use super::*;
    #[tokio::test]
    async fn test_get_integrity() {
        let auth_token = AuthToken::new(&get_auth_token_from_env().unwrap());
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let result = get_integrity(&client_id, &auth_token, "test-agent").await;
        match result {
            Ok(integrity) => {
                println!("client_id: {}", integrity.client_id);
                println!("auth_token: {}", integrity.auth_token);
                println!("token: {}", integrity.token);
            }
            Err(e) => {
                panic!("Failed to get integrity: {}", e);
            }
        }
    }
}
