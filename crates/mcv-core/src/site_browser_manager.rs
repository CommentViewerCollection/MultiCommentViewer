use mcv_common::{BrowserId, SiteId};
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use uuid::{uuid, Uuid};

/// "なし"ブラウザ専用の plugin_id（固定UUID）
///
/// デバッグログで識別しやすくするため nil ではなく固定値を使う。
/// mcv-core 内部でのみ使用し、外部には公開しない。
pub(crate) const NONE_BROWSER_PLUGIN_ID: Uuid = uuid!("faceb00c-0000-0000-0000-000000000000");

/// "なし"ブラウザの文字列ID（mcv-core 内部用）
const NONE_BROWSER_ID_STR: &str = "none_00000000-0000-0000-0000-000000000000";

/// "なし"ブラウザの BrowserId を返す（mcv-core 内部用）
pub(crate) fn none_browser_id() -> BrowserId {
    BrowserId::from_string(NONE_BROWSER_ID_STR.to_string())
}

/// BrowserId が "なし"ブラウザかどうかを判定するトレイト（mcv-core 内部用）
pub(crate) trait BrowserIdNoneExt {
    fn is_none_browser(&self) -> bool;
}

impl BrowserIdNoneExt for BrowserId {
    fn is_none_browser(&self) -> bool {
        self.as_str() == NONE_BROWSER_ID_STR
    }
}

/// サイト情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SiteInfo {
    pub site_id: SiteId,
    pub display_name: String,
    pub plugin_id: Uuid,
    pub options_schema: serde_json::Value,
}

/// ブラウザ情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct BrowserInfo {
    pub browser_id: BrowserId,
    pub browser_name: String,
    pub display_name: String,
    pub plugin_id: Uuid,
}

/// Site And Browser Manager
///
/// サイトとブラウザ情報の管理を担当
pub struct SiteAndBrowserManager {
    sites: HashMap<SiteId, SiteInfo>,
    browsers: HashMap<BrowserId, BrowserInfo>,
}

impl SiteAndBrowserManager {
    /// 新しいSiteAndBrowserManagerを作成
    pub fn new() -> Self {
        tracing::debug!("Creating new SiteAndBrowserManager");
        let mut browsers = HashMap::new();
        let none_browser = BrowserInfo {
            browser_id: none_browser_id(),
            browser_name: "none".to_string(),
            display_name: "なし".to_string(),
            plugin_id: NONE_BROWSER_PLUGIN_ID,
        };
        browsers.insert(none_browser_id(), none_browser);
        Self {
            sites: HashMap::new(),
            browsers,
        }
    }

    /// サイトを追加
    pub fn add_site(&mut self, site_info: SiteInfo) {
        tracing::debug!(
            site_id = %site_info.site_id,
            display_name = %site_info.display_name,
            plugin_id = %site_info.plugin_id,
            "Adding site to manager"
        );
        let key = site_info.site_id.clone();
        self.sites.insert(key, site_info);
    }

    /// ブラウザを追加
    pub fn add_browser(&mut self, browser_info: BrowserInfo) {
        tracing::debug!(
            browser_id = %browser_info.browser_id,
            browser_name = %browser_info.browser_name,
            display_name = %browser_info.display_name,
            plugin_id = %browser_info.plugin_id,
            "Adding browser to manager"
        );
        self.browsers
            .insert(browser_info.browser_id.clone(), browser_info);
    }

    /// サイトを取得
    pub fn get_site(&self, site_id: &SiteId) -> Option<&SiteInfo> {
        self.sites.get(site_id)
    }

    /// ブラウザを取得
    pub fn get_browser(&self, browser_id: &BrowserId) -> Option<&BrowserInfo> {
        self.browsers.get(browser_id)
    }

    /// サイト一覧を取得
    pub fn list_sites(&self) -> Vec<SiteInfo> {
        self.sites.values().cloned().collect()
    }

    /// ブラウザ一覧を取得（"なし"ブラウザを先頭に返す）
    pub fn list_browsers(&self) -> Vec<BrowserInfo> {
        let mut list: Vec<BrowserInfo> = self.browsers.values().cloned().collect();
        list.sort_by_key(|b| {
            if b.browser_id.is_none_browser() {
                0u8
            } else {
                1u8
            }
        });
        list
    }

    /// ブラウザを削除（"なし"ブラウザは削除できない）
    pub fn remove_browser(&mut self, browser_id: &BrowserId) -> bool {
        if browser_id.is_none_browser() {
            tracing::warn!(browser_id = %browser_id, "\"なし\" ブラウザは削除できません");
            return false;
        }
        let removed = self.browsers.remove(browser_id).is_some();
        if removed {
            tracing::debug!(browser_id = %browser_id, "Browser removed from manager");
        }
        removed
    }

    /// サイトIDからプラグインIDを取得
    pub fn get_plugin_id_for_site(&self, site_id: &SiteId) -> Option<Uuid> {
        self.sites.get(site_id).map(|s| s.plugin_id)
    }
}

impl Default for SiteAndBrowserManager {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_site_management() {
        let mut manager = SiteAndBrowserManager::new();
        let site_id = SiteId::new("test-site", "00000000-0000-0000-0000-000000000001");
        let plugin_id = Uuid::new_v4();

        let site_info = SiteInfo {
            site_id: site_id.clone(),
            display_name: "Test Site".to_string(),
            plugin_id,
            options_schema: serde_json::json!({}),
        };

        manager.add_site(site_info.clone());

        assert_eq!(manager.list_sites().len(), 1);
        assert!(manager.get_site(&site_id).is_some());
        assert_eq!(manager.get_plugin_id_for_site(&site_id), Some(plugin_id));
    }

    #[test]
    fn test_browser_management() {
        let mut manager = SiteAndBrowserManager::new();
        let browser_id = BrowserId::new("chrome", "00000000-0000-0000-0000-000000000001");
        let plugin_id = Uuid::new_v4();

        let browser_info = BrowserInfo {
            browser_id: browser_id.clone(),
            browser_name: "chrome".to_string(),
            display_name: "Google Chrome".to_string(),
            plugin_id,
        };

        manager.add_browser(browser_info.clone());

        // new() で "なし" ブラウザが追加されるため、追加後は 2 件
        assert_eq!(manager.list_browsers().len(), 2);
        assert!(manager.get_browser(&browser_id).is_some());
    }

    #[test]
    fn test_multiple_sites_and_browsers() {
        let mut manager = SiteAndBrowserManager::new();
        let plugin_id = Uuid::new_v4();

        // 複数のサイトを追加
        for i in 0..3 {
            let site_info = SiteInfo {
                site_id: SiteId::new(
                    &format!("site-{}", i),
                    &format!("00000000-0000-0000-0000-00000000000{}", i),
                ),
                display_name: format!("Site {}", i),
                plugin_id,
                options_schema: serde_json::json!({}),
            };
            manager.add_site(site_info);
        }

        // 複数のブラウザを追加
        for i in 0..2 {
            let browser_info = BrowserInfo {
                browser_id: BrowserId::new(
                    &format!("browser-{}", i),
                    &format!("00000000-0000-0000-0000-00000000000{}", i),
                ),
                browser_name: format!("browser-{}", i),
                display_name: format!("Browser {}", i),
                plugin_id,
            };
            manager.add_browser(browser_info);
        }

        assert_eq!(manager.list_sites().len(), 3);
        // new() で "なし" ブラウザが追加されるため、追加後は 3 件
        assert_eq!(manager.list_browsers().len(), 3);
    }

    #[test]
    fn test_none_browser_is_always_present() {
        let manager = SiteAndBrowserManager::new();
        let none_id = none_browser_id();

        assert!(
            manager.get_browser(&none_id).is_some(),
            "\"なし\" ブラウザは new() 直後から登録されているべき"
        );
    }

    #[test]
    fn test_none_browser_is_first_in_list() {
        let mut manager = SiteAndBrowserManager::new();
        let plugin_id = Uuid::new_v4();

        // 別のブラウザを追加
        manager.add_browser(BrowserInfo {
            browser_id: BrowserId::new("chrome", "00000000-0000-0000-0000-000000000001"),
            browser_name: "chrome".to_string(),
            display_name: "Google Chrome".to_string(),
            plugin_id,
        });

        let list = manager.list_browsers();
        assert!(!list.is_empty(), "ブラウザリストは空でないべき");
        assert!(
            list[0].browser_id.is_none_browser(),
            "\"なし\" ブラウザはリストの先頭に返ってくるべき"
        );
    }

    #[test]
    fn test_none_browser_has_fixed_plugin_id() {
        let manager = SiteAndBrowserManager::new();
        let none_browser = manager.get_browser(&none_browser_id()).unwrap();

        assert_eq!(
            none_browser.plugin_id, NONE_BROWSER_PLUGIN_ID,
            "\"なし\" ブラウザの plugin_id は固定UUID (NONE_BROWSER_PLUGIN_ID) であるべき"
        );
    }
}
