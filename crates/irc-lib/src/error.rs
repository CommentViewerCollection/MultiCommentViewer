use thiserror::Error;

#[derive(Debug, Error)]
pub enum IrcError {
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
    #[error("TLS error: {0}")]
    Tls(String),
    #[error("Connection closed unexpectedly")]
    ConnectionClosed,
    #[error("IRC parse error: {0}")]
    Parse(String),
    #[error("Authentication timed out")]
    AuthTimeout,
    #[error("Channel join timed out")]
    JoinTimeout,
    #[error("Server error {code}: {message}")]
    ServerError { code: u16, message: String },
}
