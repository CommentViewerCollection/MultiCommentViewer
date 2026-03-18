pub mod browser_id;
/// 共通ユーティリティクレート
/// MVP版では最小限の内容
///
/// 将来的には以下の機能を追加予定:
/// - ロギングユーティリティ
/// - エラーハンドリング
/// - 共通型定義
pub mod plugin_id;
pub mod site_id;

pub use browser_id::BrowserId;
pub use plugin_id::{PhysicalPluginId, PluginId};
pub use site_id::SiteId;

pub const APP_NAME: &str = "MultiCommentViewer";
pub const APP_VERSION: &str = env!("CARGO_PKG_VERSION");

/// アプリケーションのベースディレクトリを返す（実行ファイルと同じディレクトリ）。
///
/// インストーラ配布・ZIP配布のいずれでも実行ファイルの隣にデータを配置するために使用する。
pub fn get_base_dir() -> std::path::PathBuf {
    std::env::current_exe()
        .expect("failed to get current_exe")
        .parent()
        .expect("exe has no parent")
        .to_path_buf()
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_constants() {
        assert_eq!(APP_NAME, "MultiCommentViewer");
        assert!(!APP_VERSION.is_empty());
    }
}
