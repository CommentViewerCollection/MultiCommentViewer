//! plugin-bouyomi - 棒読みちゃん連携プラグイン
//!
//! comment-processor ロールで CommentReceived メッセージを受信し、
//! 棒読みちゃん（BouyomiChan）の TCP プロトコルを通じてコメントを読み上げさせる。

use mcv_messages::{
    CommentReceivedPayload, Message as McvMessage, MessageDestination, MessageSource, MessageType,
    PluginHelloPayload, PluginId, SettingsDataPayload, SettingsSchemaPayload,
    UpdateSettingsPayload,
};
use plugin_abi_helper::v3::prelude::*;
use serde::{Deserialize, Serialize};
use std::time::Duration;
use tokio::io::AsyncWriteExt;
use tokio::net::TcpStream;
use uuid::Uuid;

// ============================================================================
// 設定
// ============================================================================

/// 棒読みちゃん連携プラグインの設定
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BouyomiSettings {
    /// プラグインを有効にするか
    pub is_enabled: bool,

    /// 棒読みちゃんのホスト
    pub host: String,

    /// 棒読みちゃんのポート
    pub port: u16,

    /// ハンドル名（ユーザー名）を読み上げるか
    pub is_read_handle_name: bool,

    /// コメント本文を読み上げるか
    pub is_read_comment: bool,

    /// ハンドル名の末尾に称号を付与するか（例: "○○さん"）
    pub is_append_nick_title: bool,

    /// ハンドル名に付与する称号（デフォルト: "さん"）
    pub nick_title: String,

    /// 音量（-1: 棒読みちゃんの画面設定を使用、0〜100: 指定値）
    pub voice_volume: i16,

    /// 速度（-1: 棒読みちゃんの画面設定を使用、0〜200: 指定値）
    pub voice_speed: i16,

    /// 音程（-1: 棒読みちゃんの画面設定を使用、0〜200: 指定値）
    pub voice_tone: i16,

    /// 声質（0: デフォルト, 1-8: AquesTalk, 10001〜: SAPI5）
    pub voice_type: i16,
}

impl Default for BouyomiSettings {
    fn default() -> Self {
        Self {
            is_enabled: false,
            host: "127.0.0.1".to_string(),
            port: 50001,
            is_read_handle_name: true,
            is_read_comment: true,
            is_append_nick_title: true,
            nick_title: "さん".to_string(),
            // -1 は棒読みちゃんの画面設定をそのまま使う
            voice_volume: -1,
            voice_speed: -1,
            voice_tone: -1,
            voice_type: 0,
        }
    }
}

impl BouyomiSettings {
    fn schema() -> serde_json::Value {
        serde_json::json!({
            "type": "object",
            "x-ui-order": [
                "is_enabled",
                "host",
                "port",
                "is_read_handle_name",
                "is_read_comment",
                "is_append_nick_title",
                "nick_title",
                "voice_volume",
                "voice_speed",
                "voice_tone",
                "voice_type"
            ],
            "properties": {
                "is_enabled": {
                    "type": "boolean",
                    "title": "有効",
                    "description": "棒読みちゃん連携を有効にする",
                    "default": false
                },
                "host": {
                    "type": "string",
                    "title": "ホスト",
                    "description": "棒読みちゃんのホストアドレス",
                    "default": "127.0.0.1"
                },
                "port": {
                    "type": "integer",
                    "title": "ポート",
                    "description": "棒読みちゃんのポート番号",
                    "default": 50001,
                    "minimum": 1,
                    "maximum": 65535
                },
                "is_read_handle_name": {
                    "type": "boolean",
                    "title": "ハンドル名を読み上げる",
                    "default": true
                },
                "is_read_comment": {
                    "type": "boolean",
                    "title": "コメントを読み上げる",
                    "default": true
                },
                "is_append_nick_title": {
                    "type": "boolean",
                    "title": "ハンドル名に称号を付ける",
                    "description": "例: 「○○さん こんにちは」",
                    "default": true
                },
                "nick_title": {
                    "type": "string",
                    "title": "称号",
                    "description": "ハンドル名に付与する称号",
                    "default": "さん"
                },
                "voice_volume": {
                    "type": "integer",
                    "title": "音量",
                    "description": "-1: 棒読みちゃんの設定を使用、0〜100: 指定値",
                    "default": -1,
                    "minimum": -1,
                    "maximum": 100
                },
                "voice_speed": {
                    "type": "integer",
                    "title": "速度",
                    "description": "-1: 棒読みちゃんの設定を使用、0〜200: 指定値",
                    "default": -1,
                    "minimum": -1,
                    "maximum": 200
                },
                "voice_tone": {
                    "type": "integer",
                    "title": "音程",
                    "description": "-1: 棒読みちゃんの設定を使用、0〜200: 指定値",
                    "default": -1,
                    "minimum": -1,
                    "maximum": 200
                },
                "voice_type": {
                    "type": "integer",
                    "title": "声質",
                    "description": "0: デフォルト, 1: 女性1, 2: 女性2, 3: 男性1, 4: 男性2, 5: 中性, 6: ロボット, 7: 機械1, 8: 機械2",
                    "default": 0,
                    "minimum": 0
                }
            }
        })
    }
}

// ============================================================================
// 棒読みちゃん TCP 送信
// ============================================================================

/// 棒読みちゃんの TCP プロトコルでテキストを送信する
///
/// バイナリフォーマット（リトルエンディアン）:
///   Int16  COMMAND      = 0x0001
///   Int16  voiceSpeed   = speed (-1: 画面設定)
///   Int16  voiceTone    = tone  (-1: 画面設定)
///   Int16  voiceVolume  = vol   (-1: 画面設定)
///   Int16  voiceType    = type  (0: デフォルト)
///   u8     charCode     = 0     (UTF-8)
///   Int32  textLength
///   u8[]   textBytes    (UTF-8)
async fn send_to_bouyomi(
    host: String,
    port: u16,
    text: String,
    voice_speed: i16,
    voice_tone: i16,
    voice_volume: i16,
    voice_type: i16,
) {
    let addr = format!("{}:{}", host, port);

    let mut stream = match TcpStream::connect(&addr).await {
        Ok(s) => s,
        Err(e) => {
            tracing::warn!(
                error = %e,
                addr = %addr,
                "棒読みちゃんへの接続に失敗しました（起動していない可能性があります）"
            );
            return;
        }
    };

    let text_bytes = text.as_bytes();
    let text_len = text_bytes.len() as i32;

    let mut buf: Vec<u8> = Vec::with_capacity(13 + text_bytes.len());
    buf.extend_from_slice(&1i16.to_le_bytes()); // COMMAND = 0x0001
    buf.extend_from_slice(&voice_speed.to_le_bytes());
    buf.extend_from_slice(&voice_tone.to_le_bytes());
    buf.extend_from_slice(&voice_volume.to_le_bytes());
    buf.extend_from_slice(&voice_type.to_le_bytes());
    buf.push(0u8); // charCode = UTF-8
    buf.extend_from_slice(&text_len.to_le_bytes());
    buf.extend_from_slice(text_bytes);

    if let Err(e) = stream.write_all(&buf).await {
        tracing::warn!(error = %e, "棒読みちゃんへのデータ送信に失敗しました");
    } else {
        tracing::debug!(text = %text, "棒読みちゃんへ送信しました");
    }
}

// ============================================================================
// テキスト抽出ユーティリティ
// ============================================================================

// ============================================================================
// プラグイン本体
// ============================================================================

#[derive(Default)]
struct BouyomiPlugin {
    logical_plugin_id: PluginId,
    settings: BouyomiSettings,
}

impl BouyomiPlugin {
    /// comment-received ペイロードから読み上げテキストを組み立てる
    fn build_talk_text(&self, payload: &CommentReceivedPayload) -> Option<String> {
        let settings = &self.settings;

        // エンベロープ内の最初の通常チャットメッセージを使用
        // HistoryChat は履歴表示専用のため読み上げ対象にしない
        let first_msg = payload
            .envelope
            .messages
            .iter()
            .find(|m| matches!(m.kind, mcv_messages::ProviderMessageKind::Chat))?;

        let handle_name = first_msg.sender.display_name_text();
        let comment_text = first_msg.content.to_plain_text();

        let mut parts: Vec<String> = Vec::new();

        if settings.is_read_handle_name && !handle_name.is_empty() {
            let mut name = handle_name.clone();
            if settings.is_append_nick_title && !settings.nick_title.is_empty() {
                name.push_str(&settings.nick_title);
            }
            parts.push(name);
        }

        if settings.is_read_comment && !comment_text.is_empty() {
            parts.push(comment_text);
        }

        if parts.is_empty() {
            None
        } else {
            Some(parts.join(" "))
        }
    }
}

#[async_trait]
impl PluginImplV3Async for BouyomiPlugin {
    async fn on_loaded(&mut self, ctx: PluginContext) {
        self.logical_plugin_id = PluginId::new(format!("Bouyomi_logical_{}", Uuid::new_v4()));
        tracing::info!(plugin_id = %self.logical_plugin_id, "BouyomiPlugin loaded");

        let hello_payload = PluginHelloPayload {
            name: "棒読みちゃん連携".to_string(),
            plugin_id: self.logical_plugin_id.clone(),
            role: vec!["comment-processor".to_string()],
            api_version: "v3".to_string(),
            send_comment_schema: None,
        };
        let message = McvMessage::new_request(
            MessageType::PluginHello,
            MessageSource::Plugin {
                plugin_id: self.logical_plugin_id.clone(),
            },
            MessageDestination::Core,
            serde_json::to_value(&hello_payload).unwrap(),
        );
        let _ = ctx.send_request(message, Duration::from_secs(10)).await;

        tracing::info!("plugin-hello 送信完了（role: comment-processor）");

        // 設定スキーマを Core にキャッシュ登録
        let src = MessageSource::Plugin {
            plugin_id: self.logical_plugin_id.clone(),
        };
        let schema_message = McvMessage::new_notification(
            MessageType::SettingsSchema,
            src.clone(),
            MessageDestination::Core,
            serde_json::to_value(SettingsSchemaPayload {
                target: self.logical_plugin_id.to_string(),
                schema: BouyomiSettings::schema(),
            })
            .unwrap(),
        );
        let _ = ctx.send_notification(schema_message).await;

        // 初期設定データを Core にキャッシュ登録
        let data_message = McvMessage::new_notification(
            MessageType::SettingsData,
            src,
            MessageDestination::Core,
            serde_json::to_value(SettingsDataPayload {
                target: self.logical_plugin_id.to_string(),
                data: serde_json::to_value(&self.settings).unwrap(),
            })
            .unwrap(),
        );
        let _ = ctx.send_notification(data_message).await;

        tracing::info!("設定スキーマ・初期データをキャッシュ登録しました");
    }

    async fn on_message(&mut self, ctx: PluginContext, msg: &[u8]) {
        let incoming: McvMessage = match serde_json::from_slice(msg) {
            Ok(m) => m,
            Err(e) => {
                tracing::error!(error = %e, "メッセージのパースに失敗しました");
                return;
            }
        };

        match incoming.message_type {
            MessageType::CommentReceived => {
                if !self.settings.is_enabled {
                    return;
                }

                let payload: CommentReceivedPayload =
                    match serde_json::from_value(incoming.payload.clone()) {
                        Ok(p) => p,
                        Err(e) => {
                            tracing::error!(
                                error = %e,
                                "CommentReceivedPayload のデシリアライズに失敗しました"
                            );
                            return;
                        }
                    };

                let Some(talk_text) = self.build_talk_text(&payload) else {
                    return;
                };

                tracing::debug!(
                    connection_id = %payload.connection_id,
                    text = %talk_text,
                    "棒読みちゃんへ転送します"
                );

                let s = self.settings.clone();
                ctx.spawn(send_to_bouyomi(
                    s.host,
                    s.port,
                    talk_text,
                    s.voice_speed,
                    s.voice_tone,
                    s.voice_volume,
                    s.voice_type,
                ));
            }

            MessageType::GetSettingsSchema => {
                let response = incoming.create_response(
                    MessageType::SettingsSchema,
                    serde_json::json!({
                        "target": self.logical_plugin_id.to_string(),
                        "schema": BouyomiSettings::schema()
                    }),
                );
                let _ = ctx.send_notification(response).await;
            }

            MessageType::GetSettings => {
                let data = serde_json::to_value(&self.settings).unwrap_or_default();
                let response = incoming.create_response(
                    MessageType::SettingsData,
                    serde_json::json!({
                        "target": self.logical_plugin_id.to_string(),
                        "data": data
                    }),
                );
                let _ = ctx.send_notification(response).await;
            }

            MessageType::UpdateSettings => {
                let payload: UpdateSettingsPayload =
                    match serde_json::from_value(incoming.payload.clone()) {
                        Ok(p) => p,
                        Err(e) => {
                            tracing::warn!(
                                error = %e,
                                "UpdateSettingsPayload のパースに失敗しました"
                            );
                            return;
                        }
                    };
                match serde_json::from_value::<BouyomiSettings>(payload.data) {
                    Ok(new_settings) => {
                        self.settings = new_settings;
                        tracing::info!(
                            is_enabled = self.settings.is_enabled,
                            host = %self.settings.host,
                            port = self.settings.port,
                            "設定を更新しました"
                        );
                    }
                    Err(e) => {
                        tracing::warn!(error = %e, "設定データのデシリアライズに失敗しました");
                    }
                }
            }

            _ => {}
        }
    }

    async fn on_shutdown(&mut self, _ctx: PluginContext) {
        tracing::info!("BouyomiPlugin シャットダウン");
    }
}

export_plugin_v3_async!(BouyomiPlugin);
