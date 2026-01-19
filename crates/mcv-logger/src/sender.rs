use crate::schema::LogEntry;
use crate::storage::LogStorage;
use actix::prelude::*;
use reqwest::Client;
use std::sync::{Arc, Mutex};
use std::time::Duration;

/// ログ送信Actor
pub struct LogSenderActor {
    storage: Arc<Mutex<LogStorage>>,
    client: Client,
    api_base_url: String,
}

impl LogSenderActor {
    /// 新しいログ送信Actorを作成
    pub fn new(storage: Arc<Mutex<LogStorage>>, api_base_url: String) -> Self {
        Self {
            storage,
            client: Client::new(),
            api_base_url,
        }
    }
}

impl Actor for LogSenderActor {
    type Context = Context<Self>;

    fn started(&mut self, ctx: &mut Self::Context) {
        println!("LogSenderActor started");

        // 起動時に未送信ログを送信
        ctx.notify(SendUnsentLogs);

        // 5分ごとに定期送信
        ctx.run_interval(Duration::from_secs(300), |_act, ctx| {
            ctx.notify(SendUnsentLogs);
        });
    }
}

/// 未送信ログを送信するメッセージ
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendUnsentLogs;

impl Handler<SendUnsentLogs> for LogSenderActor {
    type Result = ResponseActFuture<Self, ()>;

    fn handle(&mut self, _msg: SendUnsentLogs, _ctx: &mut Self::Context) -> Self::Result {
        let storage = self.storage.clone();
        let client = self.client.clone();
        let url = format!("{}/api/logs", self.api_base_url);

        let fut = async move {
            // 未送信ログを取得
            let unsent_logs = {
                let storage_guard = match storage.lock() {
                    Ok(guard) => guard,
                    Err(e) => {
                        eprintln!("Failed to lock storage: {}", e);
                        return;
                    }
                };

                match storage_guard.get_unsent(100) {
                    Ok(logs) => logs,
                    Err(e) => {
                        eprintln!("Failed to get unsent logs: {}", e);
                        return;
                    }
                }
            };

            if unsent_logs.is_empty() {
                return;
            }

            println!("Sending {} unsent logs to {}", unsent_logs.len(), url);

            // バッチ送信
            match client.post(&url).json(&unsent_logs).send().await {
                Ok(response) => {
                    if response.status().is_success() {
                        println!("Successfully sent {} logs", unsent_logs.len());

                        // 送信成功、フラグ更新
                        let ids: Vec<String> = unsent_logs.iter().map(|e| e.id.clone()).collect();

                        let storage_guard = match storage.lock() {
                            Ok(guard) => guard,
                            Err(e) => {
                                eprintln!("Failed to lock storage: {}", e);
                                return;
                            }
                        };

                        if let Err(e) = storage_guard.mark_as_sent(&ids) {
                            eprintln!("Failed to mark logs as sent: {}", e);
                        }

                        // 古いログのクリーンアップ（1000件超過分）
                        if let Err(e) = storage_guard.cleanup_old_logs(1000) {
                            eprintln!("Failed to cleanup old logs: {}", e);
                        }
                    } else {
                        eprintln!(
                            "Failed to send logs: HTTP {} - {}",
                            response.status(),
                            response.text().await.unwrap_or_else(|_| "Unable to read response".to_string())
                        );
                    }
                }
                Err(e) => {
                    eprintln!("Failed to send logs: {}", e);
                    // 送信失敗時はローカルに保持（次回リトライ）
                }
            }
        };

        Box::pin(fut.into_actor(self))
    }
}

/// エラー発生時に即座に送信をトリガー
#[derive(Message)]
#[rtype(result = "()")]
pub struct SendImmediately {
    pub entry: LogEntry,
}

impl Handler<SendImmediately> for LogSenderActor {
    type Result = ResponseActFuture<Self, ()>;

    fn handle(&mut self, msg: SendImmediately, ctx: &mut Self::Context) -> Self::Result {
        let storage = self.storage.clone();

        let fut = async move {
            // ローカルに保存
            let storage_guard = match storage.lock() {
                Ok(guard) => guard,
                Err(e) => {
                    eprintln!("Failed to lock storage: {}", e);
                    return;
                }
            };

            if let Err(e) = storage_guard.insert(&msg.entry) {
                eprintln!("Failed to insert log entry: {}", e);
            }
        };

        // 保存後、即座に送信を試行
        ctx.notify(SendUnsentLogs);

        Box::pin(fut.into_actor(self))
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::schema::{LogLevel, SourceLocation, SystemInfo};

    fn create_test_entry() -> LogEntry {
        LogEntry {
            id: uuid::Uuid::new_v4().to_string(),
            level: LogLevel::Error,
            timestamp: chrono::Utc::now().timestamp_millis(),
            message: "Test error".to_string(),
            source: SourceLocation {
                file: "test.rs".to_string(),
                line: 42,
                column: Some(10),
                module_path: "test::module".to_string(),
            },
            stacktrace: None,
            context: Some(serde_json::json!({"key": "value"})),
            system_info: SystemInfo {
                mcv_version: "0.1.0".to_string(),
                platform: "windows".to_string(),
                arch: "x86_64".to_string(),
                build_profile: "alpha".to_string(),
            },
        }
    }

    #[actix::test]
    async fn test_sender_actor_creation() {
        let storage = Arc::new(Mutex::new(LogStorage::new(":memory:").unwrap()));
        let actor = LogSenderActor::new(storage, "http://localhost:8080".to_string());
        let _addr = actor.start();
        // Actor が正常に起動できることを確認
    }

    #[actix::test]
    async fn test_send_immediately() {
        let storage = Arc::new(Mutex::new(LogStorage::new(":memory:").unwrap()));
        let actor = LogSenderActor::new(storage.clone(), "http://localhost:8080".to_string());
        let addr = actor.start();

        let entry = create_test_entry();
        let _ = addr.send(SendImmediately { entry }).await;

        // ストレージに保存されたことを確認
        tokio::time::sleep(Duration::from_millis(100)).await;
        let storage_guard = storage.lock().unwrap();
        assert_eq!(storage_guard.count().unwrap(), 1);
    }
}
