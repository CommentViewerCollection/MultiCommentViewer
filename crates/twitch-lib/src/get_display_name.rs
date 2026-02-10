use anyhow::Result;
use crate::utils::{get_string, send_graphql_query};
use crate::{auth_token::AuthToken, client_id::ClientId};

async fn get_display_name(
    channel_login: &str,
    client_id: &ClientId,
    auth_token: Option<&AuthToken>,
) -> Result<String, anyhow::Error> {
    let query =format!(r#"{{
  "operationName": "GetDisplayName",
  "variables": {{
    "login": "{channel_login}"
  }},
  "extensions": {{
    "persistedQuery": {{
      "version": 1,
      "sha256Hash": "ba351b3d3018c3779fcaa398507e41579ae6cf12ad123a04f090943c21dedb8a"
  }}
  }}
}}"#);

    let json = send_graphql_query(&query, client_id, auth_token).await?;
println!("{:#}", json);
    let display_name = get_string(&json, &["data", "user"])?;

    Ok(display_name)
}
#[cfg(test)]
mod tests { 
    use super::*;
    #[tokio::test]
    async fn test_get_display_name() {
        let client_id = ClientId::new("kimne78kx3ncx6brgo4mv6wki5h1ko");
        let channel_login = "amauta_sau";
        let auth_token = None;
        match get_display_name(channel_login, &client_id, auth_token).await {
            Ok(display_name) => {
                println!("Display Name: {}", display_name);
            }
            Err(e) => {
                eprintln!("Error: {}", e);
            }
        }
    }
}
