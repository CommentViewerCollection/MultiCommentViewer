use std::sync::Arc;

use crate::connection::setup_websocket_loops;
use crate::message_sender::{build_get_plugins_message, build_plugin_hello_message};
use crate::ExePluginError;
use mcv_messages::Message as McvMessage;
use tokio::sync::mpsc::{self, UnboundedSender};
use tokio_tungstenite::connect_async;
use tungstenite::Message as WsMessage;
use uuid::Uuid;

/// EXEプラグインクライアント
///
/// WebSocket経由でmcvと通信するためのクライアント
pub struct ExePluginClient {
    plugin_id: Uuid,
    tx: UnboundedSender<WsMessage>,
    message_rx: Arc<tokio::sync::Mutex<Option<mpsc::UnboundedReceiver<McvMessage>>>>,
}

impl ExePluginClient {
    /// WebSocketサーバーに接続
    ///
    /// # Arguments
    /// * `url` - WebSocketサーバーのURL (例: "ws://localhost:28901")
    pub async fn connect(url: &str) -> Result<Arc<Self>, ExePluginError> {
        let (ws, _) = connect_async(url)
            .await
            .map_err(|e| ExePluginError::Connection(e.to_string()))?;

        // WebSocketのread/writeループをセットアップ
        let (tx, message_rx) = setup_websocket_loops(ws);

        let client = Arc::new(Self {
            plugin_id: Uuid::new_v4(),
            tx,
            message_rx: Arc::new(tokio::sync::Mutex::new(Some(message_rx))),
        });

        Ok(client)
    }

    /// plugin_idを取得
    pub fn plugin_id(&self) -> Uuid {
        self.plugin_id
    }

    /// plugin-helloメッセージを送信
    ///
    /// # Arguments
    /// * `name` - プラグイン名
    /// * `roles` - プラグインのロール
    pub async fn send_plugin_hello(
        &self,
        name: &str,
        roles: Vec<&str>,
    ) -> Result<(), ExePluginError> {
        let message = build_plugin_hello_message(self.plugin_id, name, roles)?;
        self.send_message(message)
    }

    /// get-pluginsメッセージを送信
    ///
    /// 既存のプラグイン一覧を取得する
    pub async fn send_get_plugins(&self) -> Result<(), ExePluginError> {
        let message = build_get_plugins_message(self.plugin_id);
        self.send_message(message)
    }

    /// メッセージを送信
    pub fn send_message(&self, message: McvMessage) -> Result<(), ExePluginError> {
        let json = serde_json::to_string(&message)?;
        let _ = self.tx.send(WsMessage::Text(json.into()));
        Ok(())
    }

    /// メッセージハンドラーを登録
    ///
    /// ハンドラーは別タスクで実行されるため、RwLockの競合問題を回避できます。
    /// 注意: このメソッドは一度だけ呼ばれることを想定しています。
    pub async fn on_message<F>(&self, handler: F)
    where
        F: Fn(McvMessage) + Send + Sync + 'static,
    {
        tracing::info!(target: "mcv::plugin_exe_interface", "Registering message handler");

        let handler = Arc::new(handler);
        let mut rx_guard = self.message_rx.lock().await;

        // receiverの所有権を取得（一度のみ）
        let mut rx = match rx_guard.take() {
            Some(rx) => {
                tracing::info!(target: "mcv::plugin_exe_interface", "Message receiver acquired successfully");
                rx
            }
            None => {
                tracing::error!(target: "mcv::plugin_exe_interface", "Message handler already registered");
                return;
            }
        };

        // ハンドラータスクをspawn
        tracing::info!(target: "mcv::plugin_exe_interface", "Spawning handler task");
        tokio::spawn(async move {
            tracing::info!(target: "mcv::plugin_exe_interface", "Handler task started, waiting for messages...");
            let mut count = 0;
            while let Some(message) = rx.recv().await {
                count += 1;
                tracing::info!(target: "mcv::plugin_exe_interface", count = count, message_type = ?message.message_type, "Received message from channel, spawning handler");
                let handler_clone = handler.clone();
                // ハンドラーを別タスクで実行（ブロッキングを避ける）
                tokio::spawn(async move {
                    tracing::info!(target: "mcv::plugin_exe_interface", "Executing handler");
                    handler_clone(message);
                    tracing::info!(target: "mcv::plugin_exe_interface", "Handler executed successfully");
                });
            }
            tracing::warn!(target: "mcv::plugin_exe_interface", "Handler task terminated (channel closed)");
        });
        tracing::info!(target: "mcv::plugin_exe_interface", "Message handler registration complete");
    }
}

#[cfg(test)]
mod tests {
    #[tokio::test]
    #[ignore] // FIXME: ExePluginClient の構造が変更されたため、このテストは再設計が必要
    async fn test_plugin_id_generation() {
        // TODO: ExePluginClient の新しい構造に合わせてテストを書き直す
        // 現在の構造: { plugin_id, message_handler: RwLock<Option<Arc<...>>>, tx }
    }
}
