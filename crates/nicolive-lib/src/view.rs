//! viewUri / セグメント URI のデコード

use crate::proto;
use prost::Message as _;
use prost::bytes::{Buf, Bytes};

// ── 公開型 ────────────────────────────────────────────────────────────────────

/// viewUri レスポンス内の1エントリ
#[derive(Debug, Clone)]
pub enum ViewEntry {
    /// ライブ中の現セグメント
    Segment { uri: String },
    /// 直近の過去セグメント (Segment と同じ形式でフェッチ可能)
    Previous { uri: String },
    /// 次回ポーリング可能になる Unix タイムスタンプ (秒)
    Next { at: i64 },
    /// 過去データ (/data/backward/v4/ — ChunkedMessage と異なるフォーマット)
    Backward {
        segment_uri: Option<String>,
        snapshot_uri: Option<String>,
    },
}

/// セグメントから取得したチャットメッセージ
#[derive(Debug, Clone)]
pub struct ChatMessage {
    /// メッセージ ID
    pub id: String,
    /// タイムスタンプ (Unix秒)。メタデータが存在しない場合は `None`
    pub at_secs: Option<i64>,
    /// コメント本文
    pub content: String,
    /// ユーザー名 (任意)
    pub name: Option<String>,
    /// ユーザー ID (ログイン済みは数値文字列、匿名はハッシュ)
    pub user_id: String,
    /// コメント番号
    pub no: i32,
}

/// セグメントから取得した全イベント（Chat 以外も含む）
#[derive(Debug, Clone)]
pub enum SegmentEvent {
    /// 通常コメント（Chat / OverflowedChat）
    Chat(ChatMessage),
    /// クルーズ等から転送されたコメント
    ForwardedChat {
        chat: ChatMessage,
        source_live_id: i64,
    },
    /// システム通知 (SimpleNotification V1)
    SimpleNotification {
        id: String,
        at_secs: Option<i64>,
        text: String,
    },
    /// システム通知 (SimpleNotificationV2、show_in_list=true のみ)
    SimpleNotificationV2 {
        id: String,
        at_secs: Option<i64>,
        text: String,
    },
    /// ギフト
    Gift {
        id: String,
        at_secs: Option<i64>,
        advertiser_name: String,
        advertiser_user_id: Option<i64>,
        point: i64,
        item_name: String,
        message: String,
    },
    /// ニコニコ広告
    Nicoad {
        id: String,
        at_secs: Option<i64>,
        advertiser: String,
        point: i64,
        message: Option<String>,
    },
}

// ── デコード関数 ───────────────────────────────────────────────────────────────

/// viewUri レスポンスボディ全体をデコードし、全エントリのリストを返す。
///
/// レスポンスには複数の length-delimited `ChunkedEntry` が含まれる場合がある。
/// デコードに失敗したエントリはスキップしてループを終了する。
pub fn decode_view_entries(data: &[u8]) -> Vec<ViewEntry> {
    use proto::chunked_entry::Entry;

    let mut buf = Bytes::copy_from_slice(data);
    let mut entries = Vec::new();

    while buf.has_remaining() {
        let entry = match proto::ChunkedEntry::decode_length_delimited(&mut buf) {
            Ok(e) => e,
            Err(_) => break,
        };

        match entry.entry {
            Some(Entry::Segment(seg)) if !seg.uri.is_empty() => {
                entries.push(ViewEntry::Segment { uri: seg.uri });
            }
            Some(Entry::Previous(seg)) if !seg.uri.is_empty() => {
                entries.push(ViewEntry::Previous { uri: seg.uri });
            }
            Some(Entry::Next(next)) => {
                entries.push(ViewEntry::Next { at: next.at });
            }
            Some(Entry::Backward(bwd)) => {
                entries.push(ViewEntry::Backward {
                    segment_uri: bwd.segment.map(|s| s.uri).filter(|u| !u.is_empty()),
                    snapshot_uri: bwd.snapshot.map(|s| s.uri).filter(|u| !u.is_empty()),
                });
            }
            _ => {}
        }
    }

    entries
}

/// ストリーミング受信バッファから `ViewEntry` を 1 件デコードする。
///
/// 返り値: `(Option<ViewEntry>, consumed_bytes)`
/// - `consumed_bytes == 0`: データ不足（次のチャンクを待つ必要あり）
/// - `consumed_bytes > 0 && None`: デコードはできたが変換不要なエントリ（スキップ）
/// - `consumed_bytes > 0 && Some(e)`: デコード成功
///
/// 呼び出し元は `consumed_bytes > 0` の場合だけバッファを進めること。
pub fn try_pop_view_entry(data: &[u8]) -> (Option<ViewEntry>, usize) {
    use proto::chunked_entry::Entry;

    let mut cursor = Bytes::copy_from_slice(data);
    let initial_len = data.len();

    match proto::ChunkedEntry::decode_length_delimited(&mut cursor) {
        Ok(entry) => {
            let consumed = initial_len - cursor.remaining();
            let view_entry = match entry.entry {
                Some(Entry::Segment(seg)) if !seg.uri.is_empty() => {
                    Some(ViewEntry::Segment { uri: seg.uri })
                }
                Some(Entry::Previous(seg)) if !seg.uri.is_empty() => {
                    Some(ViewEntry::Previous { uri: seg.uri })
                }
                Some(Entry::Next(next)) => Some(ViewEntry::Next { at: next.at }),
                Some(Entry::Backward(bwd)) => Some(ViewEntry::Backward {
                    segment_uri: bwd.segment.map(|s| s.uri).filter(|u| !u.is_empty()),
                    snapshot_uri: bwd.snapshot.map(|s| s.uri).filter(|u| !u.is_empty()),
                }),
                _ => None,
            };
            (view_entry, consumed)
        }
        Err(_) => (None, 0),
    }
}

/// ストリーミング受信バッファから `SegmentEvent` を 1 件デコードする。
///
/// `try_pop_view_entry` と同じインターフェース。
/// - `consumed_bytes == 0`: データ不足
/// - `consumed_bytes > 0 && None`: デコードできたがスキップ対象
/// - `consumed_bytes > 0 && Some(e)`: デコード・変換成功
pub fn try_pop_segment_event(data: &[u8]) -> (Option<SegmentEvent>, usize) {
    use proto::chunked_message::Payload;
    use proto::nicolive_message::Data;
    use proto::simple_notification::Message as SNMessage;

    let mut cursor = Bytes::copy_from_slice(data);
    let initial_len = data.len();

    let msg = match proto::ChunkedMessage::decode_length_delimited(&mut cursor) {
        Ok(m) => m,
        Err(_) => return (None, 0),
    };
    let consumed = initial_len - cursor.remaining();

    let Some(Payload::Message(nicolive_msg)) = msg.payload else {
        return (None, consumed);
    };

    let id = msg.meta.as_ref().map(|m| m.id.clone()).unwrap_or_default();
    let at_secs = msg
        .meta
        .as_ref()
        .and_then(|m| m.at.as_ref())
        .map(|t| t.seconds);

    let event = match nicolive_msg.data {
        Some(Data::Chat(chat)) | Some(Data::OverflowedChat(chat)) => {
            if chat.content.is_empty() {
                return (None, consumed);
            }
            let user_id = if let Some(uid) = chat.raw_user_id {
                uid.to_string()
            } else {
                chat.hashed_user_id.clone().unwrap_or_default()
            };
            Some(SegmentEvent::Chat(ChatMessage {
                id,
                at_secs,
                content: chat.content,
                name: chat.name,
                user_id,
                no: chat.no,
            }))
        }
        Some(Data::ForwardedChat(fwd)) => {
            let Some(chat) = fwd.chat else {
                return (None, consumed);
            };
            if chat.content.is_empty() {
                return (None, consumed);
            }
            let user_id = if let Some(uid) = chat.raw_user_id {
                uid.to_string()
            } else {
                chat.hashed_user_id.clone().unwrap_or_default()
            };
            Some(SegmentEvent::ForwardedChat {
                chat: ChatMessage {
                    id,
                    at_secs,
                    content: chat.content,
                    name: chat.name,
                    user_id,
                    no: chat.no,
                },
                source_live_id: fwd.source_live_id,
            })
        }
        Some(Data::SimpleNotification(sn)) => {
            let text = match sn.message {
                Some(SNMessage::Ichiba(s)) => s,
                Some(SNMessage::Quote(s)) => s,
                Some(SNMessage::Emotion(s)) => s,
                Some(SNMessage::Cruise(s)) => s,
                Some(SNMessage::ProgramExtended(s)) => s,
                Some(SNMessage::RankingIn(s)) => s,
                Some(SNMessage::Visited(s)) => s,
                Some(SNMessage::RankingUpdated(s)) => s,
                Some(SNMessage::SupporterRegistered(s)) => s,
                Some(SNMessage::UserLevelUp(s)) => s,
                None => return (None, consumed),
            };
            if text.is_empty() {
                return (None, consumed);
            }
            Some(SegmentEvent::SimpleNotification { id, at_secs, text })
        }
        Some(Data::SimpleNotificationV2(sn2)) => {
            if !sn2.show_in_list || sn2.message.is_empty() {
                return (None, consumed);
            }
            Some(SegmentEvent::SimpleNotificationV2 {
                id,
                at_secs,
                text: sn2.message,
            })
        }
        Some(Data::Gift(gift)) => Some(SegmentEvent::Gift {
            id,
            at_secs,
            advertiser_name: gift.advertiser_name,
            advertiser_user_id: gift.advertiser_user_id,
            point: gift.point,
            item_name: gift.item_name,
            message: gift.message,
        }),
        Some(Data::Nicoad(nicoad)) => {
            use proto::nicoad::Versions;
            let (advertiser, point, message) = match nicoad.versions {
                Some(Versions::V0(v0)) => {
                    let latest = v0.latest.unwrap_or_default();
                    (latest.advertiser, v0.total_point as i64, latest.message)
                }
                Some(Versions::V1(v1)) => {
                    ("".to_string(), v1.total_ad_point as i64, Some(v1.message))
                }
                None => return (None, consumed),
            };
            Some(SegmentEvent::Nicoad {
                id,
                at_secs,
                advertiser,
                point,
                message,
            })
        }
        _ => return (None, consumed),
    };

    (event, consumed)
}

/// セグメント URL のレスポンスボディを生の `ChunkedMessage` リストとしてデコードして返す。
///
/// フィールドの絞り込みや変換は一切行わない純粋なパース関数。
/// デコードに失敗したメッセージはスキップしてループを終了する。
pub fn decode_chunked_messages(data: &[u8]) -> Vec<proto::ChunkedMessage> {
    let mut buf = Bytes::copy_from_slice(data);
    let mut messages = Vec::new();

    while buf.has_remaining() {
        match proto::ChunkedMessage::decode_length_delimited(&mut buf) {
            Ok(msg) => messages.push(msg),
            Err(_) => break,
        }
    }

    messages
}

/// セグメント URL のレスポンスボディをデコードし、全イベントのリストを返す。
///
/// Chat 以外のイベント（Gift, Nicoad, SimpleNotification 等）も含む。
/// デコードに失敗したメッセージはスキップしてループを終了する。
pub fn decode_segment_events(data: &[u8]) -> Vec<SegmentEvent> {
    use proto::chunked_message::Payload;
    use proto::nicolive_message::Data;
    use proto::simple_notification::Message as SNMessage;

    let mut buf = Bytes::copy_from_slice(data);
    let mut events = Vec::new();

    while buf.has_remaining() {
        let msg = match proto::ChunkedMessage::decode_length_delimited(&mut buf) {
            Ok(m) => m,
            Err(_) => break,
        };

        let Some(Payload::Message(nicolive_msg)) = msg.payload else {
            continue;
        };

        let id = msg.meta.as_ref().map(|m| m.id.clone()).unwrap_or_default();
        let at_secs = msg
            .meta
            .as_ref()
            .and_then(|m| m.at.as_ref())
            .map(|t| t.seconds);

        match nicolive_msg.data {
            Some(Data::Chat(chat)) | Some(Data::OverflowedChat(chat)) => {
                if chat.content.is_empty() {
                    continue;
                }
                let user_id = if let Some(uid) = chat.raw_user_id {
                    uid.to_string()
                } else {
                    chat.hashed_user_id.clone().unwrap_or_default()
                };
                events.push(SegmentEvent::Chat(ChatMessage {
                    id,
                    at_secs,
                    content: chat.content,
                    name: chat.name,
                    user_id,
                    no: chat.no,
                }));
            }

            Some(Data::ForwardedChat(fwd)) => {
                let Some(chat) = fwd.chat else { continue };
                if chat.content.is_empty() {
                    continue;
                }
                let user_id = if let Some(uid) = chat.raw_user_id {
                    uid.to_string()
                } else {
                    chat.hashed_user_id.clone().unwrap_or_default()
                };
                events.push(SegmentEvent::ForwardedChat {
                    chat: ChatMessage {
                        id,
                        at_secs,
                        content: chat.content,
                        name: chat.name,
                        user_id,
                        no: chat.no,
                    },
                    source_live_id: fwd.source_live_id,
                });
            }

            Some(Data::SimpleNotification(sn)) => {
                let text = match sn.message {
                    Some(SNMessage::Ichiba(s)) => s,
                    Some(SNMessage::Quote(s)) => s,
                    Some(SNMessage::Emotion(s)) => s,
                    Some(SNMessage::Cruise(s)) => s,
                    Some(SNMessage::ProgramExtended(s)) => s,
                    Some(SNMessage::RankingIn(s)) => s,
                    Some(SNMessage::Visited(s)) => s,
                    Some(SNMessage::RankingUpdated(s)) => s,
                    Some(SNMessage::SupporterRegistered(s)) => s,
                    Some(SNMessage::UserLevelUp(s)) => s,
                    None => continue,
                };
                if text.is_empty() {
                    continue;
                }
                events.push(SegmentEvent::SimpleNotification { id, at_secs, text });
            }

            Some(Data::SimpleNotificationV2(sn2)) => {
                if !sn2.show_in_list || sn2.message.is_empty() {
                    continue;
                }
                events.push(SegmentEvent::SimpleNotificationV2 {
                    id,
                    at_secs,
                    text: sn2.message,
                });
            }

            Some(Data::Gift(gift)) => {
                events.push(SegmentEvent::Gift {
                    id,
                    at_secs,
                    advertiser_name: gift.advertiser_name,
                    advertiser_user_id: gift.advertiser_user_id,
                    point: gift.point,
                    item_name: gift.item_name,
                    message: gift.message,
                });
            }

            Some(Data::Nicoad(nicoad)) => {
                use proto::nicoad::Versions;
                let (advertiser, point, message) = match nicoad.versions {
                    Some(Versions::V0(v0)) => {
                        let latest = v0.latest.unwrap_or_default();
                        (latest.advertiser, v0.total_point as i64, latest.message)
                    }
                    Some(Versions::V1(v1)) => {
                        ("".to_string(), v1.total_ad_point as i64, Some(v1.message))
                    }
                    None => continue,
                };
                events.push(SegmentEvent::Nicoad {
                    id,
                    at_secs,
                    advertiser,
                    point,
                    message,
                });
            }

            // TagUpdated / ModeratorUpdated / SsngUpdated / PreCensored /
            // GameUpdate / AkashicMessageEvent / FeaturesUpdated はスキップ
            _ => {}
        }
    }

    events
}

/// セグメント URL のレスポンスボディをデコードし、チャットメッセージのリストを返す。
///
/// レスポンスは複数の length-delimited `ChunkedMessage` が連続したバイナリ。
/// デコードに失敗したメッセージはスキップしてループを終了する。
pub fn decode_segment_messages(data: &[u8]) -> Vec<ChatMessage> {
    use proto::chunked_message::Payload;
    use proto::nicolive_message::Data;

    let mut buf = Bytes::copy_from_slice(data);
    let mut messages = Vec::new();

    while buf.has_remaining() {
        let msg = match proto::ChunkedMessage::decode_length_delimited(&mut buf) {
            Ok(m) => m,
            Err(_) => break,
        };

        let Some(Payload::Message(nicolive_msg)) = msg.payload else {
            continue;
        };
        let Some(Data::Chat(chat)) = nicolive_msg.data else {
            continue;
        };

        if chat.content.is_empty() {
            continue;
        }

        let id = msg.meta.as_ref().map(|m| m.id.clone()).unwrap_or_default();
        let at_secs = msg
            .meta
            .as_ref()
            .and_then(|m| m.at.as_ref())
            .map(|t| t.seconds);

        let user_id = if let Some(uid) = chat.raw_user_id {
            uid.to_string()
        } else {
            chat.hashed_user_id.clone().unwrap_or_default()
        };

        messages.push(ChatMessage {
            id,
            at_secs,
            content: chat.content,
            name: chat.name,
            user_id,
            no: chat.no,
        });
    }

    messages
}
