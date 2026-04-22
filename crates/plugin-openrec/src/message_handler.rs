//! Core から受信したメッセージの処理

use std::any::type_name;

use mcv_messages::{
    CanHandleUrlPayload, CanHandleUrlResultPayload, ConnectPayload, ConnectionInputSchemaPayload,
    ConnectionRemovedPayload, DisconnectPayload, GetConnectionInputSchemaPayload,
    Message as McvMessage, MessageType, SetConnectionSitePayload,
};
use openrec_lib::extract_channel_id;
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::{connection::Connection, OpenrecPlugin};

/// 接続入力フォームの JSON Schema（URL のみ）
const INPUT_SCHEMA: &str = r#"{
  "type": "object",
  "properties": {
    "url": {
      "type": "string",
      "title": "配信URL",
      "description": "例: https://www.openrec.tv/live/channel_name"
    }
  },
  "required": ["url"]
}"#;

/// URL フィールドを持つ入力データ
#[derive(Deserialize)]
struct Input {
    url: String,
}

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut OpenrecPlugin,
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
            OpenrecPlugin::send_message(ctx, response).await;
        }

        MessageType::Connect => {
            let payload: ConnectPayload = parse_payload(&message.payload)?;
            let conn_id = payload.connection_id;

            let input: Input = serde_json::from_value(payload.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect input.extra のパース失敗",
                    error = e.to_string()
                )
            })?;

            if extract_channel_id(&input.url).is_none() {
                tracing::warn!(
                    target: "mcv::plugin-openrec",
                    conn_id = %conn_id,
                    url = %input.url,
                    "Connect: 不正な URL（openrec.tv/live/... 形式が必要）"
                );
            }

            if let Some(conn) = plugin.connections.get_mut(&conn_id) {
                conn.connect(ctx, plugin.logical_plugin_id.clone(), input.url);
            } else {
                tracing::warn!(
                    target: "mcv::plugin-openrec",
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
            let supported = extract_channel_id(url).is_some();
            let site_id = supported.then(|| {
                mcv_common::SiteId::new("OPENREC", "c2d3e4f5-2345-6789-bcde-111111111111")
            });
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            OpenrecPlugin::send_message(ctx, response).await;
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
