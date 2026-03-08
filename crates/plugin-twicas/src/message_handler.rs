//! メッセージハンドリングとペイロード解析

use std::any::type_name;

use mcv_messages::{
    CanHandleUrlPayload, CanHandleUrlResultPayload, ConnectPayload, ConnectedPayload,
    ConnectionRemovedPayload, DisconnectPayload, Message as McvMessage, MessageDestination,
    MessageSource, MessageType, SetConnectionSitePayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::TwicasPlugin;

pub(crate) async fn on_message_impl(
    plugin: &mut TwicasPlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let set_conn_site: SetConnectionSitePayload = parse_payload(&message.payload)?;
            let conn_id = set_conn_site.connection_id;
            plugin.connections.insert(conn_id, Connection::new(&conn_id));
        }
        MessageType::Connect => {
            let connect: ConnectPayload = parse_payload(&message.payload)?;
            let conn = match plugin.connections.get_mut(&connect.connection_id) {
                Some(v) => v,
                None => Err(mcv_plugin_telemetry::capture_context!("connection not found"))?,
            };

            let input_extra: Input = serde_json::from_value(connect.input.extra).map_err(|e| {
                mcv_plugin_telemetry::capture_context!(
                    "Connect input.extra parse failed",
                    error = e.to_string()
                )
            })?;

            let user_name = match Connection::extract_user_name(&input_extra.url) {
                Some(v) => v,
                None => Err(mcv_plugin_telemetry::capture_context!(
                    "twicas user_name extraction failed",
                    url = input_extra.url.clone()
                ))?,
            };

            tracing::info!(
                target: "mcv::plugin-twicas",
                connection_id = %connect.connection_id,
                url = %input_extra.url,
                user_name = %user_name,
                "Connect prerequisites resolved"
            );

            let connected = McvMessage::new_notification(
                MessageType::Connected,
                MessageSource::Plugin {
                    plugin_id: plugin.logical_plugin_id,
                },
                MessageDestination::Core,
                serde_json::to_value(ConnectedPayload {
                    connection_id: connect.connection_id,
                })
                .unwrap(),
            );
            TwicasPlugin::send_message(ctx.clone(), connected).await;

            conn.connect(ctx, plugin.logical_plugin_id, &user_name);
        }
        MessageType::Disconnect => {
            let disconnect: DisconnectPayload = parse_payload(&message.payload)?;
            if let Some(conn) = plugin.connections.get_mut(&disconnect.connection_id) {
                conn.stop();
            }
        }
        MessageType::ConnectionRemoved => {
            let removed: ConnectionRemovedPayload = parse_payload(&message.payload)?;
            if let Some(mut conn) = plugin.connections.remove(&removed.connection_id) {
                conn.stop();
            }
        }
        MessageType::CanHandleUrl => {
            let payload: CanHandleUrlPayload = parse_payload(&message.payload)?;
            let supported = Connection::extract_user_name(&payload.url).is_some();
            let site_id = supported.then(|| {
                mcv_common::SiteId::new("ツイキャス", "8cb52621-4f4f-4337-b6a7-8b4a436d91d7")
            });
            let response = message.create_response(
                MessageType::CanHandleUrlResult,
                serde_json::to_value(CanHandleUrlResultPayload { supported, site_id }).unwrap(),
            );
            TwicasPlugin::send_message(ctx, response).await;
        }
        _ => {}
    }
    Ok(())
}

fn parse_payload<T>(payload: &serde_json::Value) -> Result<T, mcv_plugin_telemetry::TracingError>
where
    T: DeserializeOwned,
{
    let result = T::deserialize(payload);
    match result {
        Ok(v) => Ok(v),
        Err(e) => Err(mcv_plugin_telemetry::capture_context!(
            "payloadが復元できない",
            type = type_name::<T>(),
            error = format!("{:#?}", e),
            raw = format!("{:#?}", payload),
        )
        .into()),
    }
}

#[derive(Deserialize)]
struct Input {
    url: String,
}
