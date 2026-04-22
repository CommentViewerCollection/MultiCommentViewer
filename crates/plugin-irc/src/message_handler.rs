//! Core から受信したメッセージの処理

use std::any::type_name;

use mcv_messages::{
    CanHandleUrlPayload, CanHandleUrlResultPayload, ConnectPayload, ConnectionInputSchemaPayload,
    ConnectionRemovedPayload, DisconnectPayload, GetConnectionInputSchemaPayload,
    Message as McvMessage, MessageType, SetConnectionSitePayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::de::DeserializeOwned;

use crate::{
    connection::{Connection, ServerType},
    IrcPlugin,
};

/// 接続入力フォームの JSON Schema
const INPUT_SCHEMA: &str = r#"{
  "type": "object",
  "properties": {
    "server_host": {
      "type": "string",
      "title": "サーバーホスト",
      "default": "irc.libera.chat"
    },
    "server_port": {
      "type": "integer",
      "title": "ポート",
      "default": 6697,
      "minimum": 1,
      "maximum": 65535
    },
    "use_tls": {
      "type": "boolean",
      "title": "TLS 使用",
      "default": true
    },
    "nick": {
      "type": "string",
      "title": "ニックネーム"
    },
    "password": {
      "type": "string",
      "title": "パスワード（省略可）"
    },
    "channel": {
      "type": "string",
      "title": "チャンネル（例: #channel）"
    },
    "server_type": {
      "type": "string",
      "title": "サーバー種別",
      "enum": ["rfc", "twitch"],
      "default": "rfc"
    }
  },
  "required": ["server_host", "server_port", "nick", "channel"]
}"#;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut IrcPlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let payload: SetConnectionSitePayload = parse_payload(&message.payload)?;
            plugin.connections.insert(
                payload.connection_id,
                Connection::new(payload.connection_id),
            );
        }

        MessageType::GetConnectionInputSchema => {
            let payload: GetConnectionInputSchemaPayload = parse_payload(&message.payload)?;
            let response = message.create_response(
                MessageType::ConnectionInputSchema,
                serde_json::to_value(ConnectionInputSchemaPayload {
                    connection_id: payload.connection_id,
                    schema: serde_json::from_str(INPUT_SCHEMA).unwrap(),
                    ui_schema: None,
                    initial_data: None,
                })
                .unwrap(),
            );
            IrcPlugin::send_message(ctx, response).await;
        }

        MessageType::Connect => {
            let payload: ConnectPayload = parse_payload(&message.payload)?;
            let conn_id = payload.connection_id;
            let extra = &payload.input.extra;

            let server_host = extra["server_host"]
                .as_str()
                .unwrap_or("irc.libera.chat")
                .to_string();
            let server_port = extra["server_port"].as_u64().unwrap_or(6697) as u16;
            let use_tls = extra["use_tls"].as_bool().unwrap_or(true);
            let nick = extra["nick"].as_str().unwrap_or("mcv_viewer").to_string();
            let password = extra["password"]
                .as_str()
                .filter(|s| !s.is_empty())
                .map(str::to_string);
            let channel = extra["channel"].as_str().unwrap_or("#channel").to_string();
            let server_type_str = extra["server_type"].as_str().unwrap_or("rfc");
            let server_type = ServerType::from_str(server_type_str);

            if let Some(conn) = plugin.connections.get_mut(&conn_id) {
                conn.connect(
                    ctx,
                    plugin.logical_plugin_id.clone(),
                    server_host,
                    server_port,
                    nick,
                    password,
                    channel,
                    use_tls,
                    server_type,
                );
            } else {
                tracing::warn!(
                    target: "mcv::plugin-irc",
                    conn_id = %conn_id,
                    "Connect: 未登録の connection_id"
                );
            }
        }

        MessageType::Disconnect => {
            let payload: DisconnectPayload = parse_payload(&message.payload)?;
            if let Some(conn) = plugin.connections.get_mut(&payload.connection_id) {
                conn.stop();
            }
        }

        MessageType::ConnectionRemoved => {
            let payload: ConnectionRemovedPayload = parse_payload(&message.payload)?;
            if let Some(mut conn) = plugin.connections.remove(&payload.connection_id) {
                conn.stop();
            }
        }

        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let url = &payload.url;
            // irc:// または ircs:// スキームを対応とする
            let supported = url.starts_with("irc://") || url.starts_with("ircs://");
            let site_id = supported
                .then(|| mcv_common::SiteId::new("IRC", "b1c2d3e4-1234-5678-abcd-000000000001"));
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            IrcPlugin::send_message(ctx, response).await;
        }

        _ => {}
    }
    Ok(())
}

#[allow(clippy::result_large_err)]
fn parse_payload<T>(payload: &serde_json::Value) -> Result<T, mcv_plugin_telemetry::TracingError>
where
    T: DeserializeOwned,
{
    T::deserialize(payload).map_err(|e| {
        mcv_plugin_telemetry::capture_context!(
            "payloadが復元できない",
            type = type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),
        )
        .into()
    })
}
