//! ニコニコ生放送 API ライブラリ
//!
//! # 提供機能
//! - [`extract_live_id`] / [`fetch_websocket_url`] — URL ユーティリティ
//! - [`decode_view_entries`] / [`decode_segment_messages`] — バイナリデコード (純粋関数、ユニットテスト容易)
//! - [`ServerTimeCache`] — サーバー時刻補正
//! - [`domain_state_machine`] — WebSocket セッション用ドメイン状態機械

mod proto;

pub mod domain_state_machine;
pub mod time_cache;
pub mod url;
pub mod view;

pub use proto::ChunkedMessage;
pub use time_cache::ServerTimeCache;
pub use url::{
    Cookie, NicoLiveAccountInfo, NicoLiveConnectionData, extract_data_props, extract_live_id,
    fetch_account_info_from_top, fetch_websocket_url,
};
pub use view::{
    ChatMessage, SegmentEvent, ViewEntry, decode_chunked_messages, decode_segment_events,
    decode_segment_messages, decode_view_entries, try_pop_segment_event, try_pop_view_entry,
};
