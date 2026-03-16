use actix::prelude::*;
use mcv_core::*;
use mcv_messages::{
    ChannelId, CommentReceivedPayload, ConnectedPayload, DisconnectedPayload, McvEnvelope, Message,
    MessageDestination, MessagePart, MessageSource, MessageType, PluginId, ProviderContent,
    ProviderMessage, ProviderMessageKind, ProviderSender, ServiceId,
};
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::Mutex;
use uuid::Uuid;

fn make_test_comment_payload(
    connection_id: Uuid,
    user: &str,
    text: &str,
) -> CommentReceivedPayload {
    let msg = ProviderMessage {
        id: Uuid::new_v4().to_string(),
        platform_message_id: None,
        service: ServiceId("test".to_string()),
        channel: ChannelId("test".to_string()),
        sender: ProviderSender {
            id: user.to_string(),
            display_name: vec![MessagePart::Text {
                text: user.to_string(),
            }],
            badges: vec![],
            role: None,
            avatar_url: None,
        },
        timestamp: chrono::Utc::now().timestamp(),
        kind: ProviderMessageKind::Chat,
        content: ProviderContent::Text {
            text: vec![MessagePart::Text {
                text: text.to_string(),
            }],
        },
        reply_to: None,
        metadata: serde_json::Value::Null,
    };
    let envelope = McvEnvelope {
        event_id: Uuid::new_v4(),
        connection_id,
        messages: vec![msg],
        received_at: chrono::Utc::now().timestamp(),
        raw_message: None,
    };
    CommentReceivedPayload {
        connection_id,
        envelope,
    }
}

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
    let comment_message = Message::new_request(
        MessageType::CommentReceived,
        MessageSource::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
        MessageDestination::Core,
        serde_json::to_value(make_test_comment_payload(
            connection_id,
            "TestUser",
            "Test comment",
        ))
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
    let connected_message = Message::new_request(
        MessageType::Connected,
        MessageSource::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
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
    let disconnected_message = Message::new_request(
        MessageType::Disconnected,
        MessageSource::Plugin {
            plugin_id: PluginId::new(plugin_id.to_string()),
        },
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
        (
            MessageType::Connected,
            serde_json::to_value(ConnectedPayload { connection_id }).unwrap(),
        ),
        (
            MessageType::CommentReceived,
            serde_json::to_value(make_test_comment_payload(
                connection_id,
                "User1",
                "Comment 1",
            ))
            .unwrap(),
        ),
        (
            MessageType::CommentReceived,
            serde_json::to_value(make_test_comment_payload(
                connection_id,
                "User2",
                "Comment 2",
            ))
            .unwrap(),
        ),
        (
            MessageType::Disconnected,
            serde_json::to_value(DisconnectedPayload { connection_id }).unwrap(),
        ),
    ];

    for (msg_type, payload) in messages_to_send {
        let message = Message::new_request(
            msg_type,
            MessageSource::Plugin {
                plugin_id: PluginId::new(plugin_id.to_string()),
            },
            MessageDestination::Core,
            payload,
        );

        let _ = core_addr.send(SendRequest { message }).await.unwrap();
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
