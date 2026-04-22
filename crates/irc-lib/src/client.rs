use tokio::io::{AsyncBufReadExt, BufReader};
use tokio::net::TcpStream;

use crate::error::IrcError;

pub type IrcLineReader = tokio::io::Lines<BufReader<Box<dyn tokio::io::AsyncRead + Unpin + Send>>>;
pub type IrcWriter = Box<dyn tokio::io::AsyncWrite + Unpin + Send>;

/// TCP 接続を確立して (reader, writer) を返す
pub async fn connect_tcp(host: &str, port: u16) -> Result<(IrcLineReader, IrcWriter), IrcError> {
    let stream = TcpStream::connect((host, port)).await?;
    let (read_half, write_half) = tokio::io::split(stream);
    let reader =
        BufReader::new(Box::new(read_half) as Box<dyn tokio::io::AsyncRead + Unpin + Send>).lines();
    let writer = Box::new(write_half) as IrcWriter;
    Ok((reader, writer))
}

/// TLS 接続を確立して (reader, writer) を返す
pub async fn connect_tls(host: &str, port: u16) -> Result<(IrcLineReader, IrcWriter), IrcError> {
    let connector = native_tls::TlsConnector::new().map_err(|e| IrcError::Tls(e.to_string()))?;
    let connector = tokio_native_tls::TlsConnector::from(connector);
    let stream = TcpStream::connect((host, port)).await?;
    let tls_stream = connector
        .connect(host, stream)
        .await
        .map_err(|e| IrcError::Tls(e.to_string()))?;
    let (read_half, write_half) = tokio::io::split(tls_stream);
    let reader =
        BufReader::new(Box::new(read_half) as Box<dyn tokio::io::AsyncRead + Unpin + Send>).lines();
    let writer = Box::new(write_half) as IrcWriter;
    Ok((reader, writer))
}
