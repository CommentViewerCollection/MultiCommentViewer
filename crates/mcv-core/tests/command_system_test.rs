use actix::prelude::*;
use mcv_core::*;
use mcv_messages::{Message, MessageDestination, MessageSource, MessageType, SendCommentPayload};
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::Mutex;
use uuid::Uuid;

/// コメント投稿システムの統合テスト

#[actix::test]
async fn test_send_comment_message_routing() {
    // Core Actorを起動
    let mut core_actor = CoreActor::new();

    // コールバックを設定してメッセージをキャプチャ
    let received_messages = Arc::new(Mutex::new(Vec::new()));
    let received_messages_clone = received_messages.clone();

    core_actor.set_event_callback(Arc::new(move |message| {
        let received = received_messages_clone.clone();
        actix::spawn(async move {
            received.lock().await.push(message);
        });
    }));

    let core_addr = core_actor.start();

    // プラグインIDと接続ID
    let plugin_id = Uuid::new_v4();
    let connection_id = Uuid::new_v4();

    // send-commentメッセージを送信
    let message = Message::new(
        MessageType::SendComment,
        MessageSource::Core,
        MessageDestination::Plugin { plugin_id },
        serde_json::to_value(SendCommentPayload {
            connection_id,
            text: "pause".to_string(),
        })
        .unwrap(),
    );

    // メッセージを送信（プラグインがない場合はエラーにならないことを確認）
    let result = core_addr
        .send(SendMessageToCore { message })
        .await;

    assert!(result.is_ok());

    // 少し待機
    tokio::time::sleep(Duration::from_millis(100)).await;

    // メッセージが受信されていないことを確認（プラグインがないため）
    let messages = received_messages.lock().await;
    assert_eq!(messages.len(), 0);
}

#[actix::test]
async fn test_connection_manager_integration() {
    // Core Actorを起動
    let core_actor = CoreActor::new();
    let core_addr = core_actor.start();

    let plugin_id = Uuid::new_v4();

    // 接続を作成
    let connection_id = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Test Site".to_string(),
            input_info: "Test Input".to_string(),
            name: "#1".to_string(),
        })
        .await
        .unwrap();

    // 接続一覧を取得
    let connections = core_addr.send(GetConnections).await.unwrap();

    assert_eq!(connections.len(), 1);
    assert_eq!(connections[0].connection_id, connection_id);
    assert_eq!(connections[0].name, "#1");
    assert_eq!(connections[0].site_name, "Test Site");
}

#[actix::test]
async fn test_rename_connection() {
    // Core Actorを起動
    let core_actor = CoreActor::new();
    let core_addr = core_actor.start();

    let plugin_id = Uuid::new_v4();

    // 接続を作成
    let connection_id = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Test Site".to_string(),
            input_info: "Test Input".to_string(),
            name: "#1".to_string(),
        })
        .await
        .unwrap();

    // 名前を変更
    let result = core_addr
        .send(RenameConnection {
            connection_id,
            new_name: "My Connection".to_string(),
        })
        .await
        .unwrap();

    assert!(result.is_ok());

    // 接続一覧を取得して確認
    let connections = core_addr.send(GetConnections).await.unwrap();
    assert_eq!(connections[0].name, "My Connection");
}

#[actix::test]
async fn test_remove_disconnected_connection() {
    // Core Actorを起動
    let core_actor = CoreActor::new();
    let core_addr = core_actor.start();

    let plugin_id = Uuid::new_v4();

    // 接続を作成
    let connection_id = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Test Site".to_string(),
            input_info: "Test Input".to_string(),
            name: "#1".to_string(),
        })
        .await
        .unwrap();

    // 切断状態（Created）なので削除できるはず
    let result = core_addr
        .send(RemoveConnection { connection_id })
        .await
        .unwrap();

    assert!(result.is_ok());

    // 接続一覧を取得して削除されたことを確認
    let connections = core_addr.send(GetConnections).await.unwrap();
    assert_eq!(connections.len(), 0);
}

#[actix::test]
async fn test_multiple_connections() {
    // Core Actorを起動
    let core_actor = CoreActor::new();
    let core_addr = core_actor.start();

    let plugin_id = Uuid::new_v4();

    // 3つの接続を作成
    let connection_id1 = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Site 1".to_string(),
            input_info: "Input 1".to_string(),
            name: "#1".to_string(),
        })
        .await
        .unwrap();

    let connection_id2 = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Site 2".to_string(),
            input_info: "Input 2".to_string(),
            name: "#2".to_string(),
        })
        .await
        .unwrap();

    let connection_id3 = core_addr
        .send(CreateConnection {
            plugin_id,
            site_name: "Site 3".to_string(),
            input_info: "Input 3".to_string(),
            name: "#3".to_string(),
        })
        .await
        .unwrap();

    // 接続一覧を取得
    let connections = core_addr.send(GetConnections).await.unwrap();
    assert_eq!(connections.len(), 3);

    // connection_id2を削除
    let result = core_addr
        .send(RemoveConnection {
            connection_id: connection_id2,
        })
        .await
        .unwrap();

    assert!(result.is_ok());

    // 接続一覧を取得して2つになったことを確認
    let connections = core_addr.send(GetConnections).await.unwrap();
    assert_eq!(connections.len(), 2);

    // connection_id1とconnection_id3が残っていることを確認
    let ids: Vec<Uuid> = connections
        .iter()
        .map(|c| c.connection_id)
        .collect();

    assert!(ids.contains(&connection_id1));
    assert!(ids.contains(&connection_id3));
    assert!(!ids.contains(&connection_id2));
}
