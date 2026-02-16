use futures_util::{SinkExt, StreamExt};
use mcv_messages::{Message as McvMessage, MessageDestination, MessageSource, MessageType};
use mcv_plugin_exe_interface::ExePluginClient;
use std::sync::Arc;
use tokio::net::TcpListener;
use tokio::sync::Mutex;
use tokio_tungstenite::accept_async;
use tokio_tungstenite::tungstenite::Message as WsMessage;

/// モックWebSocketサーバーを起動
async fn start_mock_server() -> String {
    let listener = TcpListener::bind("127.0.0.1:0").await.unwrap();
    let addr = listener.local_addr().unwrap();
    let url = format!("ws://{}", addr);

    // サーバータスクをspawn
    tokio::spawn(async move {
        while let Ok((stream, _)) = listener.accept().await {
            tokio::spawn(async move {
                let ws_stream = accept_async(stream).await.unwrap();
                let (mut write, mut read) = ws_stream.split();

                // エコーサーバーとして動作
                while let Some(msg) = read.next().await {
                    match msg {
                        Ok(WsMessage::Text(text)) => {
                            // 受信したメッセージをパースして、適切なレスポンスを返す
                            if let Ok(mcv_msg) = serde_json::from_str::<McvMessage>(&text) {
                                match mcv_msg.message_type {
                                    MessageType::PluginHello => {
                                        // plugin-addedレスポンスを送信
                                        let response = McvMessage::new_request(
                                            MessageType::PluginAdded,
                                            MessageSource::Core,
                                            MessageDestination::Broadcast,
                                            serde_json::json!({}),
                                        );
                                        let response_text =
                                            serde_json::to_string(&response).unwrap();
                                        let _ =
                                            write.send(WsMessage::Text(response_text.into())).await;
                                    }
                                    MessageType::GetPlugins => {
                                        // get-pluginsに対するレスポンスとしてエコーバック
                                        let plugin_id = match mcv_msg.src {
                                            MessageSource::Plugin { plugin_id } => plugin_id,
                                            _ => uuid::Uuid::nil(),
                                        };
                                        let response = McvMessage::new_request(
                                            MessageType::PluginAdded,
                                            MessageSource::Core,
                                            MessageDestination::Plugin { plugin_id },
                                            serde_json::json!({}),
                                        );
                                        let response_text =
                                            serde_json::to_string(&response).unwrap();
                                        let _ =
                                            write.send(WsMessage::Text(response_text.into())).await;
                                    }
                                    _ => {
                                        // エコーバック
                                        let _ = write.send(WsMessage::Text(text)).await;
                                    }
                                }
                            }
                        }
                        Ok(WsMessage::Close(_)) => break,
                        Err(_) => break,
                        _ => {}
                    }
                }
            });
        }
    });

    url
}

#[tokio::test]
async fn test_connect_to_server() {
    let url = start_mock_server().await;

    // 接続テスト
    let client = ExePluginClient::connect(&url).await;
    assert!(client.is_ok());

    let client = client.unwrap();
    assert!(!client.plugin_id().is_nil());
}

#[tokio::test]
async fn test_send_plugin_hello() {
    let url = start_mock_server().await;
    let client = ExePluginClient::connect(&url).await.unwrap();

    // メッセージ受信用
    let received = Arc::new(Mutex::new(Vec::new()));
    let received_clone = received.clone();

    // ハンドラーを登録
    client
        .on_message(move |msg| {
            let received = received_clone.clone();
            tokio::spawn(async move {
                received.lock().await.push(msg);
            });
        })
        .await;

    // plugin-helloを送信
    let result = client
        .send_plugin_hello("Test Plugin", vec!["test-role"])
        .await;
    assert!(result.is_ok());

    // レスポンスを待つ
    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // plugin-addedレスポンスを受信したことを確認
    let messages = received.lock().await;
    assert!(!messages.is_empty());
    assert_eq!(messages[0].message_type, MessageType::PluginAdded);
}

#[tokio::test]
async fn test_send_get_plugins() {
    let url = start_mock_server().await;
    let client = ExePluginClient::connect(&url).await.unwrap();

    // メッセージ受信用
    let received = Arc::new(Mutex::new(Vec::new()));
    let received_clone = received.clone();

    // ハンドラーを登録
    client
        .on_message(move |msg| {
            let received = received_clone.clone();
            tokio::spawn(async move {
                received.lock().await.push(msg);
            });
        })
        .await;

    // get-pluginsを送信
    let result = client.send_get_plugins().await;
    assert!(result.is_ok());

    // レスポンスを待つ
    tokio::time::sleep(tokio::time::Duration::from_millis(100)).await;

    // レスポンスを受信したことを確認
    let messages = received.lock().await;
    assert!(!messages.is_empty());
    assert_eq!(messages[0].message_type, MessageType::PluginAdded);
}

#[tokio::test]
async fn test_message_handler() {
    let url = start_mock_server().await;
    let client = ExePluginClient::connect(&url).await.unwrap();

    // メッセージ受信カウンター
    let count = Arc::new(Mutex::new(0));
    let count_clone = count.clone();

    // ハンドラーを登録
    client
        .on_message(move |_msg| {
            let count = count_clone.clone();
            tokio::spawn(async move {
                let mut c = count.lock().await;
                *c += 1;
            });
        })
        .await;

    // 複数のメッセージを送信
    for _ in 0..3 {
        let _ = client.send_get_plugins().await;
    }

    // メッセージが処理されるまで待つ
    tokio::time::sleep(tokio::time::Duration::from_millis(200)).await;

    // すべてのメッセージが処理されたことを確認
    let c = count.lock().await;
    assert_eq!(*c, 3);
}

#[tokio::test]
async fn test_connection_failure() {
    // 存在しないサーバーに接続
    let result = ExePluginClient::connect("ws://127.0.0.1:1").await;
    assert!(result.is_err());
}

#[tokio::test]
async fn test_multiple_handlers_not_allowed() {
    let url = start_mock_server().await;
    let client = ExePluginClient::connect(&url).await.unwrap();

    // 最初のハンドラーを登録
    client.on_message(|_| {}).await;

    // 2回目のハンドラー登録は無視される（エラーログが出るが、panicしない）
    client.on_message(|_| {}).await;

    // クライアントは正常に動作し続ける
    let result = client.send_get_plugins().await;
    assert!(result.is_ok());
}
