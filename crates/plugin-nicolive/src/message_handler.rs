//! メッセージハンドリングとペイロード解析
//!
//! Coreから受信したメッセージの処理と、
//! JSONペイロードの型安全なデシリアライゼーションを提供します。

use std::any::type_name;

use mcv_messages::{
    ConnectPayload, ConnectedPayload, ConnectionRemovedPayload, DisconnectPayload,
    Message as McvMessage, MessageDestination, MessageSource, MessageType,
    SetConnectionSitePayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{de::DeserializeOwned, Deserialize};

use crate::connection::Connection;
use crate::NicoLivePlugin;

/// 受信メッセージの処理を行う
pub(crate) async fn on_message_impl(
    plugin: &mut NicoLivePlugin,
    ctx: PluginContext,
    message: McvMessage,
) -> Result<(), mcv_plugin_telemetry::TracingError> {
    match message.message_type {
        MessageType::SetConnectionSite => {
            let set_conn_site: SetConnectionSitePayload = parse_payload(&message.payload)?;
            let conn_id = set_conn_site.connection_id;
            let conn = Connection::new(&conn_id);
            plugin.connections.insert(conn_id, conn);
        }
        MessageType::Connect => {
            let connect: ConnectPayload = parse_payload(&message.payload)?;
            let conn = match plugin.connections.get_mut(&connect.connection_id) {
                Some(a) => a,
                None => Err(mcv_plugin_telemetry::capture_context!("connection not found"))?,
            };

            let input_extra: Input =
                serde_json::from_value(connect.input.extra).map_err(|e| {
                    mcv_plugin_telemetry::capture_context!(
                        "Connect input.extra parse failed",
                        error = e.to_string()
                    )
                })?;

            let live_id = Connection::extract_live_id(&input_extra.url).ok_or_else(|| {
                mcv_plugin_telemetry::capture_context!(
                    "live_id の抽出に失敗",
                    url = input_extra.url.as_str()
                )
            })?;

            tracing::info!(
                target: "mcv::plugin-nicolive",
                connection_id = %connect.connection_id,
                live_id = %live_id,
                "WebSocket URL を取得中"
            );

            let ws_url = match Connection::fetch_websocket_url(&live_id).await {
                Ok(url) => {
                    tracing::info!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connect.connection_id,
                        live_id = %live_id,
                        ws_url = %url,
                        "WebSocket URL 取得成功"
                    );
                    url
                }
                Err(e) => {
                    tracing::error!(
                        target: "mcv::plugin-nicolive",
                        connection_id = %connect.connection_id,
                        live_id = %live_id,
                        error = %e,
                        "WebSocket URL 取得失敗"
                    );
                    return Err(mcv_plugin_telemetry::capture_context!(
                        "WebSocket URL 取得失敗",
                        error = e
                    )
                    .into());
                }
            };

            // Connected を先に返信してから接続タスクを開始する
            let response = McvMessage::new_notification(
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
            NicoLivePlugin::send_message(ctx.clone(), response).await;

            conn.connect(ctx, plugin.logical_plugin_id, &ws_url);
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
        _ => {}
    }
    Ok(())
}

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

#[derive(Deserialize)]
struct Input {
    url: String,
}
