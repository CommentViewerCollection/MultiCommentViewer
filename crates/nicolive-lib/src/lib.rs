//! ニコニコ生放送 API ライブラリ
//!
//! # 提供機能
//! - [`extract_live_id`] / [`fetch_websocket_url`] — URL ユーティリティ
//! - [`decode_view_entries`] / [`decode_segment_messages`] — バイナリデコード (純粋関数、ユニットテスト容易)
//! - [`ServerTimeCache`] — サーバー時刻補正

mod proto;

pub mod time_cache;
pub mod url;
pub mod view;

pub use time_cache::ServerTimeCache;
pub use url::{extract_data_props, extract_live_id, fetch_websocket_url};
pub use proto::ChunkedMessage;
pub use view::{decode_chunked_messages, decode_segment_events, decode_segment_messages, decode_view_entries, try_pop_segment_event, try_pop_view_entry, ChatMessage, SegmentEvent, ViewEntry};
