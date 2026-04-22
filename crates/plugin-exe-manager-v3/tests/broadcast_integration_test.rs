// 統合テストは外部クレートとして実行されるため、
// plugin-exe-manager-v3 クレートを明示的にインポート
extern crate plugin_exe_manager_v3;

mod helpers;

use helpers::MockExePlugin;
use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginId,
};
use mcv_plugin_interface::{PluginError, PluginHost};
use plugin_exe_manager_v3::websocket_server::WebSocketServer;
use std::sync::Arc;
use tokio::sync::Mutex;
use uuid::Uuid;

/// テスト用のダミーPluginHost
struct DummyPluginHost {
    received_messages: Arc<Mutex<Vec<McvMessage>>>,
}

#[allow(dead_code)]
impl DummyPluginHost {
    fn new() -> Self {
        Self {
            received_messages: Arc::new(Mutex::new(Vec::new())),
        }
    }

    async fn get_received_messages(&self) -> Vec<McvMessage> {
        self.received_messages.lock().await.clone()
    }
}

#[async_trait::async_trait]
impl PluginHost for DummyPluginHost {
    async fn send_message(&self, message: McvMessage) -> Result<(), PluginError> {
        self.received_messages.lock().await.push(message);
        Ok(())
    }
}

/// WebSocketサーバーを起動してポートを取得
async fn start_websocket_server() -> (Arc<WebSocketServer>, u16, Arc<DummyPluginHost>) {
    let host = Arc::new(DummyPluginHost::new());
    let server = WebSocketServer::new("127.0.0.1:0", Arc::clone(&host) as Arc<dyn PluginHost>)
        .await
        .expect("Failed to start WebSocket server");
    let port = server.get_port();
    (Arc::new(server), port, host)
}

#[tokio::test]
async fn test_websocket_server_broadcast_e2e() {
    let (server, port, _host) = start_websocket_server().await;

    // 3つのモックEXEプラグインを接続
    let mut plugin1 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin1");
    let mut plugin2 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin2");
    let mut plugin3 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin3");

    // plugin-helloを送信して登録
    plugin1
        .send_plugin_hello("test-plugin-1")
        .await
        .expect("Failed to send plugin-hello");
    plugin2
        .send_plugin_hello("test-plugin-2")
        .await
        .expect("Failed to send plugin-hello");
    plugin3
        .send_plugin_hello("test-plugin-3")
        .await
        .expect("Failed to send plugin-hello");

    // サーバーが登録を処理するまで少し待つ
    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // ブロードキャストメッセージを送信
    let router = server.get_router();
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::json!({"connection_id": Uuid::new_v4()}),
    );

    router
        .broadcast(broadcast_msg)
        .await
        .expect("Failed to broadcast");

    // 全クライアントがメッセージを受信することを確認
    let received1 = plugin1
        .receive_message()
        .await
        .expect("Plugin1 did not receive message");
    let received2 = plugin2
        .receive_message()
        .await
        .expect("Plugin2 did not receive message");
    let received3 = plugin3
        .receive_message()
        .await
        .expect("Plugin3 did not receive message");

    assert_eq!(received1.message_type, MessageType::ConnectionAdded);
    assert_eq!(received2.message_type, MessageType::ConnectionAdded);
    assert_eq!(received3.message_type, MessageType::ConnectionAdded);

    // クリーンアップ
    plugin1.close().await.ok();
    plugin2.close().await.ok();
    plugin3.close().await.ok();
}

#[tokio::test]
async fn test_broadcast_after_plugin_disconnect() {
    let (server, port, _host) = start_websocket_server().await;

    // 3つのモックEXEプラグインを接続
    let mut plugin1 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin1");
    let mut plugin2 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin2");
    let mut plugin3 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin3");

    // plugin-helloを送信して登録
    plugin1
        .send_plugin_hello("test-plugin-1")
        .await
        .expect("Failed to send plugin-hello");
    plugin2
        .send_plugin_hello("test-plugin-2")
        .await
        .expect("Failed to send plugin-hello");
    plugin3
        .send_plugin_hello("test-plugin-3")
        .await
        .expect("Failed to send plugin-hello");

    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // plugin2を切断
    plugin2.close().await.expect("Failed to close plugin2");
    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // ブロードキャストメッセージを送信
    let router = server.get_router();
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::json!({"connection_id": Uuid::new_v4()}),
    );

    // ブロードキャストは成功する（切断されたクライアントはスキップ）
    router.broadcast(broadcast_msg).await.ok();

    // 残り2つのクライアントがメッセージを受信
    let received1 = plugin1
        .receive_message()
        .await
        .expect("Plugin1 did not receive message");
    let received3 = plugin3
        .receive_message()
        .await
        .expect("Plugin3 did not receive message");

    assert_eq!(received1.message_type, MessageType::ConnectionAdded);
    assert_eq!(received3.message_type, MessageType::ConnectionAdded);

    // クリーンアップ
    plugin1.close().await.ok();
    plugin3.close().await.ok();
}

#[tokio::test]
async fn test_no_duplicate_broadcasts_bug1() {
    // このテストは、バグ1（3回送信）が修正されていることを確認する
    // ルーティングテーブルアプローチにより、EXEプラグインは1回だけメッセージを受信する

    let (server, port, _host) = start_websocket_server().await;

    // 1つのモックEXEプラグインを接続
    let mut plugin = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin");

    // plugin-helloを送信して登録
    plugin
        .send_plugin_hello("test-plugin")
        .await
        .expect("Failed to send plugin-hello");

    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // ブロードキャストメッセージを1回送信
    let router = server.get_router();
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Core,
        MessageDestination::Broadcast,
        serde_json::json!({"connection_id": Uuid::new_v4()}),
    );

    router
        .broadcast(broadcast_msg)
        .await
        .expect("Failed to broadcast");

    // メッセージを1回だけ受信することを確認
    let received = plugin
        .receive_message()
        .await
        .expect("Plugin did not receive message");
    assert_eq!(received.message_type, MessageType::ConnectionAdded);

    // 2回目の受信を試みる（タイムアウトすることを期待）
    let timeout_result = tokio::time::timeout(
        tokio::time::Duration::from_millis(500),
        plugin.receive_message(),
    )
    .await;

    assert!(
        timeout_result.is_err(),
        "Plugin should not receive duplicate message"
    );

    // クリーンアップ
    plugin.close().await.ok();
}

#[tokio::test]
async fn test_no_double_send_to_requester_bug2() {
    // このテストは、バグ2（ユニキャスト+ブロードキャストの二重送信）が修正されていることを確認する
    // リクエスト元プラグインもブロードキャストで1回だけ受信する

    let (server, port, _host) = start_websocket_server().await;

    // 2つのモックEXEプラグインを接続
    let mut plugin1 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin1");
    let mut plugin2 = MockExePlugin::connect(port)
        .await
        .expect("Failed to connect plugin2");

    // plugin-helloを送信して登録
    plugin1
        .send_plugin_hello("test-plugin-1")
        .await
        .expect("Failed to send plugin-hello");
    plugin2
        .send_plugin_hello("test-plugin-2")
        .await
        .expect("Failed to send plugin-hello");

    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // ブロードキャストメッセージを送信（リクエスト元はplugin1と仮定）
    let router = server.get_router();
    let broadcast_msg = McvMessage::new_notification(
        MessageType::ConnectionAdded,
        MessageSource::Plugin {
            plugin_id: PluginId::new(plugin1.logical_plugin_id().to_string()),
        },
        MessageDestination::Broadcast,
        serde_json::json!({"connection_id": Uuid::new_v4()}),
    );

    router
        .broadcast(broadcast_msg)
        .await
        .expect("Failed to broadcast");

    // 両方のプラグインがメッセージを1回だけ受信
    let received1 = plugin1
        .receive_message()
        .await
        .expect("Plugin1 did not receive message");
    let received2 = plugin2
        .receive_message()
        .await
        .expect("Plugin2 did not receive message");

    assert_eq!(received1.message_type, MessageType::ConnectionAdded);
    assert_eq!(received2.message_type, MessageType::ConnectionAdded);

    // plugin1が2回目の受信を試みる（タイムアウトすることを期待）
    let timeout_result = tokio::time::timeout(
        tokio::time::Duration::from_millis(500),
        plugin1.receive_message(),
    )
    .await;

    assert!(
        timeout_result.is_err(),
        "Requester plugin should not receive duplicate message"
    );

    // クリーンアップ
    plugin1.close().await.ok();
    plugin2.close().await.ok();
}
