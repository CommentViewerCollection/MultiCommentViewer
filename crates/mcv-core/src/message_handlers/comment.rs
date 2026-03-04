use actix::Context;
use mcv_common::LogicalPluginId;
use mcv_log_core::{
    LogEntry as LoggerEntry, LogLevel, SourceLocation as LoggerSourceLocation,
    StackFrame as LoggerStackFrame, SystemInfo as LoggerSystemInfo,
};
use mcv_messages::{
    GetSendCommentSchemaPayload, LogEntryPayload, Message as McvMessage, MessageDestination,
    MessageSource, MessageType, SendCommentPayload, SendCommentSchemaPayload,
};
use uuid::Uuid;

use crate::core_actor::CoreActor;
use crate::plugin_host_actor::SendMessageToPlugin;

/// comment-received メッセージのハンドラー
pub fn handle_comment_received(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    // UIへイベント通知
    if let Some(callback) = &actor.event_callback {
        callback(message.clone());
    }

    // "comment-processor" ロールを持つプラグインへ転送
    for (logical_plugin_id, plugin_info) in &actor.logical_plugins {
        if plugin_info.role.contains(&"comment-processor".to_string()) {
            let mut forwarded = message.clone();
            forwarded.dst = MessageDestination::Plugin {
                plugin_id: logical_plugin_id.inner(),
            };
            plugin_info
                .host_addr
                .do_send(SendMessageToPlugin { message: forwarded });
        }
    }
}

/// send-comment メッセージのハンドラー
pub fn handle_send_comment(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: SendCommentPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "send-comment",
                "Failed to parse message payload"
            );
            return;
        }
    };

    let connection_id = payload.connection_id;

    actor.connection_manager.update_comment_state(
        &connection_id,
        Some(serde_json::json!({
            "text": payload.text,
            "extra": payload.extra,
        })),
    );
    let _ = actor.save_connections();

    // 該当する接続のプラグインへコメントを転送
    let plugins = actor.logical_plugins.clone();
    let msg = message.clone();

    if let Some(conn_info) = actor.connection_manager.get_connection(&connection_id) {
        if let Some(plugin_id_uuid) = conn_info.plugin_id {
            let logical_plugin_id = LogicalPluginId::from_uuid(plugin_id_uuid);
            if let Some(plugin_info) = plugins.get(&logical_plugin_id) {
                plugin_info
                    .host_addr
                    .do_send(SendMessageToPlugin { message: msg });
            }
        }
    }
}

/// get-send-comment-schema メッセージのハンドラー（現状はtext入力のみ）
pub fn handle_get_send_comment_schema(
    _actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) -> Result<McvMessage, String> {
    let payload: GetSendCommentSchemaPayload = serde_json::from_value(message.payload.clone())
        .map_err(|e| format!("Failed to parse GetSendCommentSchemaPayload: {}", e))?;
    let schema = serde_json::json!({
        "type": "object",
        "properties": {
            "text": { "type": "string", "title": "コメント" }
        },
        "required": ["text"]
    });
    Ok(message.create_response(
        MessageType::SendCommentSchema,
        serde_json::to_value(SendCommentSchemaPayload {
            connection_id: payload.connection_id,
            schema,
            ui_schema: None,
            initial_data: None,
        })
        .map_err(|e| format!("Failed to serialize SendCommentSchemaPayload: {}", e))?,
    ))
}

/// log-entry メッセージのハンドラー（プラグインからのログメッセージ）
pub fn handle_log_entry(
    actor: &mut CoreActor,
    message: &McvMessage,
    _ctx: &mut Context<CoreActor>,
) {
    let payload: LogEntryPayload = match serde_json::from_value(message.payload.clone()) {
        Ok(p) => p,
        Err(e) => {
            tracing::error!(
                target: "mcv::core::CoreActor",
                error = %e,
                message_type = "log-entry",
                "Failed to parse message payload"
            );
            return;
        }
    };

    // プラグインIDを取得
    let plugin_id = match message.src {
        MessageSource::Plugin { plugin_id } => plugin_id,
        _ => {
            tracing::warn!(
                target: "mcv::core::CoreActor",
                message_type = "log-entry",
                message_source = ?message.src,
                "Received log-entry from non-plugin source"
            );
            return;
        }
    };

    // ログレベルを変換
    let level = match payload.level.as_str() {
        "error" => LogLevel::Error,
        "warn" => LogLevel::Warn,
        "info" => LogLevel::Info,
        "debug" => LogLevel::Debug,
        _ => LogLevel::Trace,
    };

    // context から元のソース位置情報を抽出
    let (source_file, source_line, source_module) = match &payload.context {
        Some(ctx) => {
            let file = ctx
                .get("source")
                .and_then(|s| s.get("file"))
                .and_then(|f| f.as_str())
                .unwrap_or("unknown")
                .to_string();
            let line = ctx
                .get("source")
                .and_then(|s| s.get("line"))
                .and_then(|l| l.as_u64())
                .unwrap_or(0) as u32;
            let module = ctx
                .get("source")
                .and_then(|s| s.get("module_path"))
                .and_then(|m| m.as_str())
                .unwrap_or("unknown")
                .to_string();
            (file, line, module)
        }
        None => ("unknown".to_string(), 0, "unknown".to_string()),
    };

    // context から スタックトレースを抽出
    let stacktrace = payload
        .context
        .as_ref()
        .and_then(|ctx| ctx.get("stacktrace"))
        .and_then(|st| st.as_array())
        .map(|frames| {
            frames
                .iter()
                .map(|f| LoggerStackFrame {
                    symbol: f
                        .get("symbol")
                        .and_then(|s| s.as_str())
                        .map(|s| s.to_string()),
                    filename: f
                        .get("filename")
                        .and_then(|s| s.as_str())
                        .map(|s| s.to_string()),
                    lineno: f.get("lineno").and_then(|n| n.as_u64()).map(|n| n as u32),
                    addr: f
                        .get("addr")
                        .and_then(|s| s.as_str())
                        .unwrap_or("0x0")
                        .to_string(),
                })
                .collect()
        });

    // context から source・stacktrace を除いた残りを保持
    let context = payload.context.as_ref().and_then(|ctx| {
        if let Some(obj) = ctx.as_object() {
            let filtered: serde_json::Map<String, serde_json::Value> = obj
                .iter()
                .filter(|(k, _)| k.as_str() != "source" && k.as_str() != "stacktrace")
                .map(|(k, v)| (k.clone(), v.clone()))
                .collect();
            if filtered.is_empty() {
                None
            } else {
                // plugin_id を構造化データとして追加
                let mut with_plugin = filtered;
                with_plugin.insert(
                    "plugin_id".to_string(),
                    serde_json::Value::String(plugin_id.to_string()),
                );
                Some(serde_json::Value::Object(with_plugin))
            }
        } else {
            Some(ctx.clone())
        }
    });

    // LogEntry を構築して直接ストレージに保存
    let entry = LoggerEntry {
        id: Uuid::new_v4().to_string(),
        level,
        timestamp: chrono::Utc::now().timestamp_millis(),
        message: payload.message,
        source: LoggerSourceLocation {
            file: source_file,
            line: source_line,
            column: None,
            module_path: source_module,
        },
        stacktrace,
        context,
        system_info: LoggerSystemInfo {
            mcv_version: env!("CARGO_PKG_VERSION").to_string(),
            platform: std::env::consts::OS.to_string(),
            arch: std::env::consts::ARCH.to_string(),
            build_profile: payload
                .plugin_build_profile
                .unwrap_or_else(|| "unknown".to_string()),
        },
    };

    if let Some(ref storage) = actor.log_storage {
        if let Ok(storage) = storage.lock() {
            if let Err(e) = storage.insert(&entry) {
                tracing::error!(
                    target: "mcv::core::CoreActor",
                    error = %e,
                    plugin_id = %plugin_id,
                    "Failed to insert plugin log entry into storage"
                );
            }
        }
    }
}
