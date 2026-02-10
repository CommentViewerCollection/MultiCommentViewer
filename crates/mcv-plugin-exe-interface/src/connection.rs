use crate::ExePluginError;
use futures_util::{SinkExt, StreamExt};
use mcv_messages::Message as McvMessage;
use tokio::sync::mpsc::{self, UnboundedReceiver, UnboundedSender};
use tokio_tungstenite::tungstenite::Message as WsMessage;
use tokio_tungstenite::WebSocketStream;

/// WebSocketの読み取り/書き込みループをセットアップ
///
/// # Returns
/// - `UnboundedSender<WsMessage>`: メッセージ送信用チャネル
/// - `UnboundedReceiver<McvMessage>`: メッセージ受信用チャネル
pub(crate) fn setup_websocket_loops<S>(
    ws: WebSocketStream<S>,
) -> (UnboundedSender<WsMessage>, UnboundedReceiver<McvMessage>)
where
    S: tokio::io::AsyncRead + tokio::io::AsyncWrite + Unpin + Send + 'static,
{
    let (write, mut read) = ws.split();
    let (tx, mut rx) = mpsc::unbounded_channel();
    let (message_tx, message_rx) = mpsc::unbounded_channel();

    // Write loop: チャネルからメッセージを受信してWebSocketに送信
    tokio::spawn(async move {
        let mut write = write;
        while let Some(msg) = rx.recv().await {
            println!("sending message: {}", msg);
            let _ = write.send(msg).await;
        }
    });

    // Read loop: WebSocketからメッセージを受信してチャネルに送信
    tokio::spawn(async move {
        tracing::info!(target: "mcv::plugin_exe_interface", "Starting read loop task");
        loop {
            tracing::trace!(target: "mcv::plugin_exe_interface", "Waiting for next message...");
            let msg = read.next().await;
            if msg.is_none() {
                tracing::warn!(target: "mcv::plugin_exe_interface", "WebSocket connection closed");
                break;
            }
            let msg = msg.unwrap();
            match msg {
                Ok(WsMessage::Text(text)) => {
                    tracing::info!(target: "mcv::plugin_exe_interface", message_text = %text, "Received WebSocket text message");
                    match serde_json::from_str::<McvMessage>(&text) {
                        Ok(mcv_message) => {
                            tracing::info!(target: "mcv::plugin_exe_interface", message_type = ?mcv_message.message_type, "Parsed message successfully");
                            match message_tx.send(mcv_message) {
                                Ok(_) => {
                                    tracing::info!(target: "mcv::plugin_exe_interface", "Message sent to handler channel successfully");
                                }
                                Err(e) => {
                                    tracing::error!(target: "mcv::plugin_exe_interface", error = %e, "Failed to send message to handler channel");
                                }
                            }
                        }
                        Err(e) => {
                            tracing::error!(target: "mcv::plugin_exe_interface", error = %e, text = %text, "Failed to parse message");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::info!(target: "mcv::plugin_exe_interface", "WebSocket closed");
                    break;
                }
                Ok(WsMessage::Ping(data)) => {
                    tracing::trace!(target: "mcv::plugin_exe_interface", "Received ping");
                    drop(data);
                }
                Ok(WsMessage::Pong(_)) => {
                    tracing::trace!(target: "mcv::plugin_exe_interface", "Received pong");
                }
                Ok(WsMessage::Binary(_)) => {
                    tracing::warn!(target: "mcv::plugin_exe_interface", "Received unexpected binary message");
                }
                Ok(WsMessage::Frame(_)) => {
                    tracing::trace!(target: "mcv::plugin_exe_interface", "Received frame");
                }
                Err(e) => {
                    tracing::error!(target: "mcv::plugin_exe_interface", error = %e, "WebSocket error");
                    return Err(ExePluginError::WebSocket(e.to_string()));
                }
            }
        }
        Ok(())
    });

    (tx, message_rx)
}
