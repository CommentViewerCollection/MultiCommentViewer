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
            let _ = write.send(msg).await;
        }
    });

    // Read loop: WebSocketからメッセージを受信してチャネルに送信
    tokio::spawn(async move {
        tracing::debug!(target: "mcv::plugin_exe_interface", "WebSocket 読み取りループ開始");
        loop {
            let msg = read.next().await;
            if msg.is_none() {
                tracing::warn!(target: "mcv::plugin_exe_interface", "WebSocket 接続が閉じられた");
                break;
            }
            let msg = msg.unwrap();
            match msg {
                Ok(WsMessage::Text(text)) => {
                    tracing::trace!(target: "mcv::plugin_exe_interface", message_text = %text, "WebSocket テキストメッセージ受信");
                    match serde_json::from_str::<McvMessage>(&text) {
                        Ok(mcv_message) => {
                            tracing::trace!(target: "mcv::plugin_exe_interface", message_type = ?mcv_message.message_type, "メッセージパース成功");
                            if let Err(e) = message_tx.send(mcv_message) {
                                tracing::error!(target: "mcv::plugin_exe_interface", error = %e, "ハンドラチャネルへの送信失敗");
                            }
                        }
                        Err(e) => {
                            tracing::error!(target: "mcv::plugin_exe_interface", error = %e, text = %text, "メッセージパース失敗");
                        }
                    }
                }
                Ok(WsMessage::Close(_)) => {
                    tracing::debug!(target: "mcv::plugin_exe_interface", "WebSocket 切断");
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
