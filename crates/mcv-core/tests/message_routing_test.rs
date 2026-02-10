use actix::prelude::*;
use mcv_core::*;
use mcv_messages::{
    CommentReceivedPayload, ConnectedPayload, DisconnectedPayload,
    Message, MessageDestination, MessageSource, MessageType,
};
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::Mutex;
use uuid::Uuid;

/// メッセージルーティングの統合テスト

#[actix::test]
async fn test_event_callback_routing() {
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

    let connection_id = Uuid::new_v4();
    let plugin_id = Uuid::new_v4();

    // comment-received メッセージを送信（UIへのイベント通知対象）
    let comment_message = Message::new(
        MessageType::CommentReceived,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(CommentReceivedPayload {
            connection_id,
            comment: mcv_messages::Comment {
                id: Uuid::new_v4().to_string(),
                user_name: vec![mcv_messages::MessagePart::Text {
                    text: "TestUser".to_string(),
                }],
                user_id: "user123".to_string(),
                text: vec![mcv_messages::MessagePart::Text {
                    text: "Test comment".to_string(),
                }],
                timestamp: chrono::Utc::now().timestamp_millis(),
            },
        })
        .unwrap(),
    );

    let _ = core_addr
        .send(SendRequest {
            message: comment_message,
        })
        .await
        .unwrap();

    // 少し待機
    tokio::time::sleep(Duration::from_millis(100)).await;

    // メッセージが受信されたことを確認
    let messages = received_messages.lock().await;
    assert_eq!(messages.len(), 1);
    assert_eq!(messages[0].message_type, MessageType::CommentReceived);
}

#[actix::test]
async fn test_connected_event_routing() {
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

    let connection_id = Uuid::new_v4();
    let plugin_id = Uuid::new_v4();

    // connected メッセージを送信
    let connected_message = Message::new(
        MessageType::Connected,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(ConnectedPayload { connection_id }).unwrap(),
    );

    let _ = core_addr
        .send(SendRequest {
            message: connected_message,
        })
        .await
        .unwrap();

    // 少し待機
    tokio::time::sleep(Duration::from_millis(100)).await;

    // メッセージが受信されたことを確認
    let messages = received_messages.lock().await;
    assert_eq!(messages.len(), 1);
    assert_eq!(messages[0].message_type, MessageType::Connected);
}

#[actix::test]
async fn test_disconnected_event_routing() {
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

    let connection_id = Uuid::new_v4();
    let plugin_id = Uuid::new_v4();

    // disconnected メッセージを送信
    let disconnected_message = Message::new(
        MessageType::Disconnected,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
    );

    let _ = core_addr
        .send(SendRequest {
            message: disconnected_message,
        })
        .await
        .unwrap();

    // 少し待機
    tokio::time::sleep(Duration::from_millis(100)).await;

    // メッセージが受信されたことを確認
    let messages = received_messages.lock().await;
    assert_eq!(messages.len(), 1);
    assert_eq!(messages[0].message_type, MessageType::Disconnected);
}

#[actix::test]
async fn test_multiple_event_routing() {
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

    let connection_id = Uuid::new_v4();
    let plugin_id = Uuid::new_v4();

    // 複数のメッセージを順次送信
    let messages_to_send = vec![
        (MessageType::Connected, serde_json::to_value(ConnectedPayload { connection_id }).unwrap()),
        (
            MessageType::CommentReceived,
            serde_json::to_value(CommentReceivedPayload {
                connection_id,
                comment: mcv_messages::Comment {
                    id: Uuid::new_v4().to_string(),
                    user_name: vec![mcv_messages::MessagePart::Text {
                        text: "User1".to_string(),
                    }],
                    user_id: "user1".to_string(),
                    text: vec![mcv_messages::MessagePart::Text {
                        text: "Comment 1".to_string(),
                    }],
                    timestamp: chrono::Utc::now().timestamp_millis(),
                },
            })
            .unwrap(),
        ),
        (
            MessageType::CommentReceived,
            serde_json::to_value(CommentReceivedPayload {
                connection_id,
                comment: mcv_messages::Comment {
                    id: Uuid::new_v4().to_string(),
                    user_name: vec![mcv_messages::MessagePart::Text {
                        text: "User2".to_string(),
                    }],
                    user_id: "user2".to_string(),
                    text: vec![mcv_messages::MessagePart::Text {
                        text: "Comment 2".to_string(),
                    }],
                    timestamp: chrono::Utc::now().timestamp_millis(),
                },
            })
            .unwrap(),
        ),
        (MessageType::Disconnected, serde_json::to_value(DisconnectedPayload { connection_id }).unwrap()),
    ];

    for (msg_type, payload) in messages_to_send {
        let message = Message::new(
            msg_type,
            MessageSource::Plugin { plugin_id },
            MessageDestination::Core,
            payload,
        );

        let _ = core_addr
            .send(SendRequest { message })
            .await
            .unwrap();
    }

    // 少し待機
    tokio::time::sleep(Duration::from_millis(200)).await;

    // 4つのメッセージが受信されたことを確認
    let messages = received_messages.lock().await;
    assert_eq!(messages.len(), 4);
    assert_eq!(messages[0].message_type, MessageType::Connected);
    assert_eq!(messages[1].message_type, MessageType::CommentReceived);
    assert_eq!(messages[2].message_type, MessageType::CommentReceived);
    assert_eq!(messages[3].message_type, MessageType::Disconnected);
}
