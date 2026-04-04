use std::collections::HashMap;
use std::sync::{Arc, Mutex};
use std::time::Instant;

use tauri::{AppHandle, Emitter};
use uuid::Uuid;

use mcv_messages::{
    CommentReceivedPayload, ConnectFailedPayload, DisconnectedPayload, Message as McvMessage,
    MessagePart, MessageType, ProviderMessageKind, SetSiteNgUsersPayload, SystemKind,
};

use crate::comment::{envelope_to_comment_rows, DeleteAllByUserPayload};
use crate::comment_store::CommentStore;
use crate::types::CommentRow;

/// イベントコールバッククロージャを構築して返す
///
/// CoreActor からのメッセージを受け取り、フロントエンドへ Tauri イベントとして転送する。
/// AppHandle は起動完了後に設定されるため Mutex<Option<AppHandle>> でラップしている。
pub(crate) fn build_event_callback(
    app_handle: Arc<tokio::sync::Mutex<Option<AppHandle>>>,
    comment_store: Arc<Mutex<CommentStore>>,
    comment_timing: Arc<tokio::sync::Mutex<HashMap<Uuid, (i64, Instant)>>>,
) -> Arc<dyn Fn(McvMessage) + Send + Sync> {
    Arc::new(move |message: McvMessage| {
        let app_handle_clone = app_handle.clone();
        let timing_clone = comment_timing.clone();
        let store_clone = comment_store.clone();
        actix::spawn(async move {
            // app_handle の Mutex を最小限の期間だけ保持してすぐに解放する
            let Some(app_handle) = app_handle_clone.lock().await.as_ref().cloned() else {
                return;
            };
            handle_message(message, &app_handle, store_clone, timing_clone).await;
        });
    })
}

/// メッセージ種別ごとに処理を振り分ける
async fn handle_message(
    message: McvMessage,
    app_handle: &AppHandle,
    comment_store: Arc<Mutex<CommentStore>>,
    comment_timing: Arc<tokio::sync::Mutex<HashMap<Uuid, (i64, Instant)>>>,
) {
    match message.message_type {
        MessageType::CommentReceived => {
            handle_comment_received(message.payload, app_handle, comment_store).await;
        }
        MessageType::StreamMetadata => {
            emit_event(app_handle, "stream-metadata", message.payload);
        }
        MessageType::Connected => {
            emit_event(app_handle, "connected", message.payload);
        }
        MessageType::ConnectFailed => {
            handle_connect_failed(message.payload, app_handle);
        }
        MessageType::Disconnected => {
            handle_disconnected(message.payload, app_handle, comment_timing).await;
        }
        MessageType::AddSite => {
            emit_event(app_handle, "site-added", message.payload);
        }
        MessageType::AddBrowser => {
            emit_event(app_handle, "browser-added", message.payload);
        }
        MessageType::RemoveBrowser => {
            emit_event(app_handle, "browser-removed", message.payload);
        }
        MessageType::UpdateConnectionAccount => {
            emit_event(app_handle, "connection-account-updated", message.payload);
        }
        MessageType::SetSiteNgUsers => {
            handle_set_site_ng_users(message.payload, app_handle, comment_store).await;
        }
        _ => {}
    }
}

/// comment-received メッセージを処理してフロントエンドへ emit する
async fn handle_comment_received(
    payload: serde_json::Value,
    app_handle: &AppHandle,
    comment_store: Arc<Mutex<CommentStore>>,
) {
    let payload: CommentReceivedPayload = match serde_json::from_value(payload) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::main",
                error = %e,
                "Failed to parse CommentReceivedPayload"
            );
            return;
        }
    };

    tracing::debug!(
        target: "mcv::main",
        connection_id = %payload.connection_id,
        message_count = payload.envelope.messages.len(),
        "Processing comment-received event"
    );

    // MessageDeleteAll を "delete-all-by-user" イベントとして即座に emit
    for msg in &payload.envelope.messages {
        if let ProviderMessageKind::System(SystemKind::MessageDeleteAll { user_id }) = &msg.kind {
            let store_for_ng = comment_store.clone();
            let user_id_for_ng = user_id.clone();
            tokio::task::spawn_blocking(move || {
                if let Ok(store) = store_for_ng.lock() {
                    let _ = store.set_site_ng(&user_id_for_ng);
                }
            })
            .await
            .ok();
            let evt = DeleteAllByUserPayload {
                user_id: user_id.clone(),
                connection_id: payload.envelope.connection_id.to_string(),
            };
            if let Err(e) = app_handle.emit("delete-all-by-user", evt) {
                tracing::error!(
                    target: "mcv::main",
                    error = %e,
                    "Failed to emit delete-all-by-user event"
                );
            }
        }
    }

    // HistoryChat は timestamp 差分を再生せず短間隔でバースト送信し、
    // それ以外（Chat/Monetary/System）は従来どおり timestamp 再生を行う。
    let mut history_messages = Vec::new();
    let mut timed_messages = Vec::new();
    for msg in payload.envelope.messages.into_iter() {
        if matches!(
            &msg.kind,
            ProviderMessageKind::System(SystemKind::MessageDeleteAll { .. })
        ) {
            continue;
        }
        if matches!(&msg.kind, ProviderMessageKind::HistoryChat) {
            history_messages.push(msg);
        } else {
            timed_messages.push(msg);
        }
    }

    let connection_id = payload.envelope.connection_id;
    let received_at = payload.envelope.received_at;
    let event_id = payload.envelope.event_id;

    if !history_messages.is_empty() || !timed_messages.is_empty() {
        let mut all_rows = Vec::new();
        for msg in history_messages.into_iter().chain(timed_messages) {
            let single_envelope = mcv_messages::McvEnvelope {
                event_id,
                connection_id,
                messages: vec![msg],
                received_at,
                raw_message: None,
            };
            all_rows.extend(envelope_to_comment_rows(&single_envelope));
        }

        // SQLite 挿入を spawn_blocking で実行（actix スレッドをブロックしない）
        let store_for_insert = comment_store.clone();
        let all_rows = tokio::task::spawn_blocking(move || {
            if let Ok(store) = store_for_insert.lock() {
                for row in &all_rows {
                    let _ = store.insert_comment(row);
                }
            }
            all_rows
        })
        .await
        .unwrap_or_default();

        tracing::info!(
            target: "mcv::main",
            batch_size = all_rows.len(),
            connection_id = %connection_id,
            working_set_mb = crate::crash_handler::get_process_memory_mb(),
            "Emitting comment-received batch"
        );
        if let Err(e) = app_handle.emit("comment-received", all_rows) {
            tracing::error!(
                target: "mcv::main",
                error = %e,
                "Failed to emit comment-received event"
            );
        }
    }
}

/// connect-failed メッセージを処理し、システム通知もコメント一覧に追加する
fn handle_connect_failed(payload: serde_json::Value, app_handle: &AppHandle) {
    if let Ok(fail) = serde_json::from_value::<ConnectFailedPayload>(payload.clone()) {
        let now_ts = unix_now_secs();
        let notice_row = system_notice_row(
            &format!("connect-failed-{}-{}", fail.connection_id, Uuid::new_v4()),
            &fail.connection_id.to_string(),
            &format!("接続に失敗しました: {}", fail.reason),
            now_ts,
        );
        if let Err(e) = app_handle.emit("comment-received", vec![notice_row]) {
            tracing::error!(
                target: "mcv::main",
                error = %e,
                "Failed to emit connect-failed notice"
            );
        }
    }
    emit_event(app_handle, "connect-failed", payload);
}

/// disconnected メッセージを処理し、タイミング基準のクリアとシステム通知を行う
async fn handle_disconnected(
    payload: serde_json::Value,
    app_handle: &AppHandle,
    comment_timing: Arc<tokio::sync::Mutex<HashMap<Uuid, (i64, Instant)>>>,
) {
    if let Ok(disc) = serde_json::from_value::<DisconnectedPayload>(payload.clone()) {
        // 切断時に connection_id 単位のタイミング基準をクリア
        comment_timing.lock().await.remove(&disc.connection_id);

        // 「切断されました」システム通知をコメント一覧に追加
        let now_ts = unix_now_secs();
        let notice_row = system_notice_row(
            &format!("disconnected-{}-{}", disc.connection_id, Uuid::new_v4()),
            &disc.connection_id.to_string(),
            "切断されました",
            now_ts,
        );
        if let Err(e) = app_handle.emit("comment-received", vec![notice_row]) {
            tracing::error!(
                target: "mcv::main",
                error = %e,
                "Failed to emit disconnected notice"
            );
        }
    }
    emit_event(app_handle, "disconnected", payload);
}

/// set-site-ng-users メッセージを処理する
async fn handle_set_site_ng_users(
    payload: serde_json::Value,
    app_handle: &AppHandle,
    comment_store: Arc<Mutex<CommentStore>>,
) {
    if let Ok(ng_payload) = serde_json::from_value::<SetSiteNgUsersPayload>(payload) {
        let store_for_ng = comment_store.clone();
        tokio::task::spawn_blocking(move || {
            if let Ok(store) = store_for_ng.lock() {
                if let Err(e) = store.set_site_ng_batch(&ng_payload.user_ids) {
                    tracing::warn!(
                        target: "mcv::main",
                        error = %e,
                        "set_site_ng_batch failed"
                    );
                }
            }
        })
        .await
        .ok();
        let _ = app_handle.emit("site-ng-users-updated", ());
    }
}

// ---- ヘルパー ----

/// 現在時刻を Unix タイムスタンプ（秒）として返す
fn unix_now_secs() -> i64 {
    std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .map(|d| d.as_secs() as i64)
        .unwrap_or(0)
}

/// システム通知用の CommentRow を構築する
fn system_notice_row(id: &str, connection_id: &str, text: &str, timestamp: i64) -> CommentRow {
    CommentRow {
        id: id.to_string(),
        user_name: vec![MessagePart::Text {
            text: "システム".to_string(),
        }],
        user_id: String::new(),
        badges: vec![],
        text: vec![MessagePart::Text {
            text: text.to_string(),
        }],
        timestamp,
        connection_id: connection_id.to_string(),
        is_visible: true,
        replaces_id: None,
        kind: "system".to_string(),
        avatar_url: None,
        amount_text: None,
    }
}

/// 任意の Serialize 値を Tauri イベントとして emit する（エラーはログに記録）
fn emit_event<T: serde::Serialize + Clone>(app_handle: &AppHandle, event: &str, payload: T) {
    if let Err(e) = app_handle.emit(event, payload) {
        tracing::error!(
            target: "mcv::main",
            error = %e,
            event = event,
            "Failed to emit event"
        );
    }
}
