//! Kick バッジ SVG データ
//!
//! Kick の JavaScript バンドル (3782-c250e2d38878d546.js) から抽出した
//! バッジの SVG データを data:image/svg+xml URI として提供します。
//!
//! 対応バッジ: broadcaster, moderator, og, sidekick, staff, sub_gifter, subscriber, verified, vip

const SVG_BROADCASTER: &str = include_str!("badges/broadcaster.svg");
const SVG_MODERATOR: &str = include_str!("badges/moderator.svg");
const SVG_OG: &str = include_str!("badges/og.svg");
const SVG_SIDEKICK: &str = include_str!("badges/sidekick.svg");
const SVG_STAFF: &str = include_str!("badges/staff.svg");
const SVG_SUB_GIFTER: &str = include_str!("badges/sub_gifter.svg");
const SVG_SUBSCRIBER: &str = include_str!("badges/subscriber.svg");
const SVG_VERIFIED: &str = include_str!("badges/verified.svg");
const SVG_VIP: &str = include_str!("badges/vip.svg");

fn svg_to_data_uri(svg: &str) -> String {
    let encoded = svg
        .replace('%', "%25")
        .replace('"', "'")
        .replace('<', "%3C")
        .replace('>', "%3E")
        .replace('#', "%23")
        .replace(' ', "%20")
        .replace('\n', "")
        .replace('\r', "");
    format!("data:image/svg+xml,{encoded}")
}

/// バッジタイプに対応する SVG の data URI を返す。
/// 未知のバッジタイプは `None` を返す。
pub(crate) fn badge_image_url(badge_type: &str) -> Option<String> {
    let svg = match badge_type {
        "broadcaster" => SVG_BROADCASTER,
        "moderator" => SVG_MODERATOR,
        "og" => SVG_OG,
        "sidekick" => SVG_SIDEKICK,
        "staff" => SVG_STAFF,
        "sub_gifter" => SVG_SUB_GIFTER,
        "subscriber" => SVG_SUBSCRIBER,
        "verified" => SVG_VERIFIED,
        "vip" => SVG_VIP,
        _ => return None,
    };
    Some(svg_to_data_uri(svg))
}
