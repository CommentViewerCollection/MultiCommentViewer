/// 共通ユーティリティクレート
/// MVP版では最小限の内容
///
/// 将来的には以下の機能を追加予定:
/// - ロギングユーティリティ
/// - エラーハンドリング
/// - 共通型定義

pub mod plugin_id;
pub mod site_id;
pub mod browser_id;

pub use plugin_id::{LogicalPluginId, PhysicalPluginId};
pub use site_id::SiteId;
pub use browser_id::BrowserId;

pub const APP_NAME: &str = "MultiCommentViewer";
pub const APP_VERSION: &str = env!("CARGO_PKG_VERSION");

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_constants() {
        assert_eq!(APP_NAME, "MultiCommentViewer");
        assert!(!APP_VERSION.is_empty());
    }
}
