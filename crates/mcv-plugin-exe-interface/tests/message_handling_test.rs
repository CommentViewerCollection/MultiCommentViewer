/// メッセージハンドリングの基本的なテスト
///
/// WebSocketのモックは複雑なため、チャネルベースの設計が
/// 正しく動作することを確認する簡易テスト

use std::sync::{Arc, Mutex};
use std::time::Duration;
use tokio::sync::mpsc;

#[tokio::test]
async fn test_channel_based_message_handling() {
    // メッセージを受信するためのチャネルを作成
    let (tx, mut rx) = mpsc::unbounded_channel::<String>();

    // 受信したメッセージを保存
    let received = Arc::new(Mutex::new(Vec::new()));
    let received_clone = received.clone();

    // ハンドラータスクをspawn
    let handler_task = tokio::spawn(async move {
        while let Some(message) = rx.recv().await {
            let mut guard = received_clone.lock().unwrap();
            guard.push(message);
        }
    });

    // メッセージを送信
    tx.send("message1".to_string()).unwrap();
    tx.send("message2".to_string()).unwrap();
    tx.send("message3".to_string()).unwrap();

    // 少し待つ
    tokio::time::sleep(Duration::from_millis(100)).await;

    // 送信側をクローズ
    drop(tx);

    // ハンドラータスクの完了を待つ
    let _ = tokio::time::timeout(Duration::from_secs(1), handler_task).await;

    // 検証
    let messages = received.lock().unwrap();
    assert_eq!(messages.len(), 3);
    assert_eq!(messages[0], "message1");
    assert_eq!(messages[1], "message2");
    assert_eq!(messages[2], "message3");
}

#[tokio::test]
async fn test_nested_task_spawn() {
    // ネストされたtokio::spawnが正しく動作することを確認
    let (tx, mut rx) = mpsc::unbounded_channel::<i32>();

    let received = Arc::new(Mutex::new(Vec::new()));
    let received_clone = received.clone();

    // 外側のタスク
    tokio::spawn(async move {
        while let Some(num) = rx.recv().await {
            let received_inner = received_clone.clone();
            // 内側のタスク（ハンドラーのシミュレーション）
            tokio::spawn(async move {
                let mut guard = received_inner.lock().unwrap();
                guard.push(num * 2); // 2倍にして保存
            });
        }
    });

    // メッセージを送信
    tx.send(1).unwrap();
    tx.send(2).unwrap();
    tx.send(3).unwrap();

    // 処理完了を待つ
    tokio::time::sleep(Duration::from_millis(100)).await;

    drop(tx);
    tokio::time::sleep(Duration::from_millis(100)).await;

    // 検証
    let numbers = received.lock().unwrap();
    assert_eq!(numbers.len(), 3);
    // 順序は保証されないが、すべての要素が含まれている
    assert!(numbers.contains(&2));
    assert!(numbers.contains(&4));
    assert!(numbers.contains(&6));
}

#[tokio::test]
async fn test_option_take_pattern() {
    // Option::take()パターンが正しく動作することを確認
    let (tx, rx) = mpsc::unbounded_channel::<String>();

    let rx_option = Arc::new(tokio::sync::Mutex::new(Some(rx)));

    // 最初のtake()は成功
    {
        let mut guard = rx_option.lock().await;
        let result = guard.take();
        assert!(result.is_some());
    }

    // 2回目のtake()は失敗（None）
    {
        let mut guard = rx_option.lock().await;
        let result = guard.take();
        assert!(result.is_none());
    }

    drop(tx);
}
