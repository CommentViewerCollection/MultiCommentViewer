use futures_util::{SinkExt, StreamExt};
use mcv_messages::{Message as McvMessage, MessageType, PluginHelloPayload};
use std::error::Error;
use tokio::net::TcpStream;
use tokio_tungstenite::{
    MaybeTlsStream, WebSocketStream, connect_async, tungstenite::protocol::Message as WsMessage,
};
use uuid::Uuid;

/// モックEXEプラグイン（テスト用）
///
/// WebSocketクライアントとして動作し、EXE Plugin Managerとの通信をテストする
pub struct MockExePlugin {
    ws_stream: WebSocketStream<MaybeTlsStream<TcpStream>>,
    plugin_id: Uuid,
    logical_plugin_id: Uuid,
}

impl MockExePlugin {
    /// WebSocketサーバーに接続
    pub async fn connect(port: u16) -> Result<Self, Box<dyn Error>> {
        let url = format!("ws://127.0.0.1:{}", port);
        let (ws_stream, _) = connect_async(&url).await?;

        let plugin_id = Uuid::new_v4(); // internal_physical_plugin_id
        let logical_plugin_id = Uuid::new_v4(); // logical_plugin_id

        Ok(Self {
            ws_stream,
            plugin_id,
            logical_plugin_id,
        })
    }

    /// plugin-helloを送信してプラグインを登録
    pub async fn send_plugin_hello(&mut self, name: &str) -> Result<(), Box<dyn Error>> {
        let payload = PluginHelloPayload {
            plugin_id: self.plugin_id, // internal_physical_plugin_id
            name: name.to_string(),
            role: vec!["test".to_string()],
            api_version: "v2".to_string(),
            send_comment_schema: None,
        };

        let message = McvMessage::new_request(
            MessageType::PluginHello,
            mcv_messages::MessageSource::Plugin {
                plugin_id: self.logical_plugin_id, // srcにlogical_plugin_idを設定
            },
            mcv_messages::MessageDestination::Core,
            serde_json::to_value(payload)?,
        );

        let json = serde_json::to_string(&message)?;
        self.ws_stream.send(WsMessage::Text(json.into())).await?;

        Ok(())
    }

    /// メッセージを受信（タイムアウト付き）
    pub async fn receive_message(&mut self) -> Result<McvMessage, Box<dyn Error>> {
        let timeout = tokio::time::Duration::from_secs(5);
        let msg = tokio::time::timeout(timeout, self.ws_stream.next())
            .await?
            .ok_or("Connection closed")??;

        match msg {
            WsMessage::Text(text) => {
                let message: McvMessage = serde_json::from_str(&text)?;
                Ok(message)
            }
            _ => Err("Unexpected message type".into()),
        }
    }

    /// メッセージを送信
    pub async fn send_message(&mut self, message: McvMessage) -> Result<(), Box<dyn Error>> {
        let json = serde_json::to_string(&message)?;
        self.ws_stream.send(WsMessage::Text(json.into())).await?;
        Ok(())
    }

    /// 接続をクローズ
    pub async fn close(mut self) -> Result<(), Box<dyn Error>> {
        self.ws_stream.close(None).await?;
        Ok(())
    }

    /// internal_physical_plugin_idを取得
    pub fn plugin_id(&self) -> Uuid {
        self.plugin_id
    }

    /// logical_plugin_idを取得
    pub fn logical_plugin_id(&self) -> Uuid {
        self.logical_plugin_id
    }
}
