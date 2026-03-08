//! TwitCasting 内部 API ライブラリ
//!
//! # 主な機能
//!
//! - ライブページHTML解析（[`html`] モジュール）
//! - 認証キー生成（[`auth`] モジュール）
//! - フロントエンド API クライアント（[`api`] モジュール）

pub mod api;
pub mod auth;
pub mod html;

pub use api::{
    fetch_event_pubsub_url, fetch_item_box, fetch_latest_movie, fetch_movie_info,
    fetch_movie_token, fetch_session_id, fetch_session_ids, fetch_viewer_status, post_comment,
    resolve_wpass, BroadcasterInfo, ContinueInfo, Cookie, Item, ItemBoxResponse, ItemBoxStatus,
    LatestMovieResponse, MovieInfo, MovieInfoResponse, MovieTokenResponse, PostCommentError,
    PostCommentResponse, RequestBody, ViewerCount, ViewerMovieInfo, ViewerStatusResponse,
    VisibilityInfo, WpassError,
};
pub use auth::generate_authorize_key;
pub use html::{
    extract_cs_session_id, extract_movie_id_from_html, extract_tc_page_variables, get_tc_variable,
};
