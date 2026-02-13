use std::collections::HashMap;
use mcv_common::SiteId;
use uuid::Uuid;
use serde::{Serialize, Deserialize};

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
    pub browser_id: Uuid,
    pub browser_name: String,
    pub display_name: String,
    pub plugin_id: Uuid,
}

/// Site And Browser Manager
///
/// サイトとブラウザ情報の管理を担当
pub struct SiteAndBrowserManager {
    sites: HashMap<SiteId, SiteInfo>,
    browsers: HashMap<Uuid, BrowserInfo>,
}

impl SiteAndBrowserManager {
    /// 新しいSiteAndBrowserManagerを作成
    pub fn new() -> Self {
        tracing::debug!("Creating new SiteAndBrowserManager");
        Self {
            sites: HashMap::new(),
            browsers: HashMap::new(),
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
        self.browsers.insert(browser_info.browser_id, browser_info);
    }

    /// サイトを取得
    pub fn get_site(&self, site_id: &SiteId) -> Option<&SiteInfo> {
        self.sites.get(site_id)
    }

    /// ブラウザを取得
    pub fn get_browser(&self, browser_id: &Uuid) -> Option<&BrowserInfo> {
        self.browsers.get(browser_id)
    }

    /// サイト一覧を取得
    pub fn list_sites(&self) -> Vec<SiteInfo> {
        self.sites.values().cloned().collect()
    }

    /// ブラウザ一覧を取得
    pub fn list_browsers(&self) -> Vec<BrowserInfo> {
        self.browsers.values().cloned().collect()
    }

    /// サイトIDからプラグインIDを取得
    pub fn get_plugin_id_for_site(&self, site_id: &SiteId) -> Option<Uuid> {
        self.sites.get(site_id).map(|s| s.plugin_id)
    }

    /// browser_nameから逆引き（永続化復元用）
    pub fn find_browser_by_name(&self, browser_name: &str) -> Option<&BrowserInfo> {
        self.browsers
            .values()
            .find(|b| b.browser_name == browser_name)
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
        assert_eq!(
            manager.get_plugin_id_for_site(&site_id),
            Some(plugin_id)
        );
    }

    #[test]
    fn test_browser_management() {
        let mut manager = SiteAndBrowserManager::new();
        let browser_id = Uuid::new_v4();
        let plugin_id = Uuid::new_v4();

        let browser_info = BrowserInfo {
            browser_id,
            browser_name: "chrome".to_string(),
            display_name: "Google Chrome".to_string(),
            plugin_id,
        };

        manager.add_browser(browser_info.clone());

        assert_eq!(manager.list_browsers().len(), 1);
        assert!(manager.get_browser(&browser_id).is_some());
    }

    #[test]
    fn test_multiple_sites_and_browsers() {
        let mut manager = SiteAndBrowserManager::new();
        let plugin_id = Uuid::new_v4();

        // 複数のサイトを追加
        for i in 0..3 {
            let site_info = SiteInfo {
                site_id: SiteId::new(&format!("site-{}", i), &format!("00000000-0000-0000-0000-00000000000{}", i)),
                display_name: format!("Site {}", i),
                plugin_id,
                options_schema: serde_json::json!({}),
            };
            manager.add_site(site_info);
        }

        // 複数のブラウザを追加
        for i in 0..2 {
            let browser_info = BrowserInfo {
                browser_id: Uuid::new_v4(),
                browser_name: format!("browser-{}", i),
                display_name: format!("Browser {}", i),
                plugin_id,
            };
            manager.add_browser(browser_info);
        }

        assert_eq!(manager.list_sites().len(), 3);
        assert_eq!(manager.list_browsers().len(), 2);
    }
}
