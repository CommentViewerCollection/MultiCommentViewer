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

/// セグメント URL のレスポンスボディを生の `ChunkedMessage` リストとしてデコードして返す。
///
/// フィールドの絞り込みや変換は一切行わない純粋なパース関数。
/// デコードに失敗したメッセージはスキップしてループを終了する。
pub fn decode_chunked_messages(data: &[u8]) -> Vec<proto::ChunkedMessage> {
    use prost::Message as _;
    use prost::bytes::{Buf, Bytes};

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
