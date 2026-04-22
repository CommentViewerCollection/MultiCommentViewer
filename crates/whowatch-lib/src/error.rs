use thiserror::Error;

#[derive(Debug, Error)]
pub enum WhoWatchError {
    #[error("HTTP request failed: {0}")]
    Http(#[from] reqwest::Error),

    #[error("JSON parse failed: {0}")]
    Json(#[from] serde_json::Error),

    #[error("API returned error: code={code}, message={message}")]
    Api { code: String, message: String },

    #[error("Live not found for user")]
    LiveNotFound,
}
