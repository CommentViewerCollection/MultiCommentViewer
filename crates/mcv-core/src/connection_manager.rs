use indexmap::IndexMap;
use mcv_common::{BrowserId, SiteId};
use serde::{Deserialize, Serialize};
use uuid::Uuid;

/// 接続ステータス
#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "type", content = "message")]
pub enum ConnectionStatus {
    /// プラグイン待機中（永続化復元時にプラグインが未到着）
    Pending,
    /// 接続が作成された
    Created,
    /// 接続中
    Connecting,
    /// 接続完了
    Connected,
    /// 切断済み
    Disconnected,
    /// エラー
    Error(String),
}

/// 接続情報
#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionInfo {
    pub connection_id: Uuid,
    pub plugin_id: Option<Uuid>,
    pub status: ConnectionStatus,
    pub site_id: Option<SiteId>,
    pub url: Option<String>,
    pub browser_id: Option<BrowserId>,
    pub advanced_settings: Option<serde_json::Value>,
    /// 接続時入力フォームの最後の値（URL以外のサイト固有入力も含む）
    #[serde(default)]
    pub input_state: Option<serde_json::Value>,
    /// コメント投稿フォームの最後の値（text 以外のサイト固有入力を想定）
    #[serde(default)]
    pub comment_state: Option<serde_json::Value>,
    pub input_info: String,
    pub name: String,
    /// ログイン中のアカウント情報（接続後に取得できた場合のみ）
    pub account_info: Option<mcv_messages::AccountInfo>,
}

/// Connection Manager
///
/// 接続インスタンスの管理を担当
pub struct ConnectionManager {
    connections: IndexMap<Uuid, ConnectionInfo>,
}

impl ConnectionManager {
    fn url_from_input_state(state: &Option<serde_json::Value>) -> Option<String> {
        state
            .as_ref()
            .and_then(|v| v.get("url"))
            .and_then(|v| v.as_str())
            .map(|s| s.to_string())
    }

    fn input_state_from_url(url: &str) -> serde_json::Value {
        serde_json::json!({ "url": url })
    }

    /// 新しいConnection Managerを作成
    pub fn new() -> Self {
        Self {
            connections: IndexMap::new(),
        }
    }

    /// 接続を追加
    pub fn add_connection(&mut self, connection_id: Uuid, name: String) {
        tracing::debug!(
            connection_id = %connection_id,
            name = %name,
            "Adding connection"
        );

        let info = ConnectionInfo {
            connection_id,
            plugin_id: None,
            status: ConnectionStatus::Created,
            site_id: None,
            url: None,
            browser_id: None,
            advanced_settings: None,
            input_state: None,
            comment_state: None,
            input_info: "".to_string(),
            name,
            account_info: None,
        };
        self.connections.insert(connection_id, info);
    }

    /// 接続を削除
    pub fn remove_connection(&mut self, connection_id: &Uuid) -> Option<ConnectionInfo> {
        self.connections.shift_remove(connection_id)
    }

    /// アカウント情報を更新
    pub fn update_account(
        &mut self,
        connection_id: &Uuid,
        account: Option<mcv_messages::AccountInfo>,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            info.account_info = account;
        }
    }

    /// 接続のステータスを更新
    pub fn update_status(&mut self, connection_id: &Uuid, status: ConnectionStatus) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            info.status = status;
        }
    }

    /// 接続のステータスを取得
    pub fn get_status(&self, connection_id: &Uuid) -> Option<ConnectionStatus> {
        self.connections
            .get(connection_id)
            .map(|info| info.status.clone())
    }

    /// 接続情報を取得
    pub fn get_connection(&self, connection_id: &Uuid) -> Option<&ConnectionInfo> {
        self.connections.get(connection_id)
    }
    pub fn get_connections(&self) -> Vec<&ConnectionInfo> {
        self.connections.iter().map(|a| a.1).collect()
    }

    /// 全接続のリストを取得
    pub fn list_connections(&self) -> Vec<&ConnectionInfo> {
        self.connections.values().collect()
    }

    /// 接続名を変更
    pub fn rename_connection(&mut self, connection_id: &Uuid, new_name: String) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            info.name = new_name;
        }
    }

    /// サイトを設定
    pub fn set_site(&mut self, connection_id: &Uuid, site_id: SiteId, plugin_id: Uuid) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                site_id = %site_id,
                plugin_id = %plugin_id,
                "Connection site updated"
            );
            info.site_id = Some(site_id);
            info.plugin_id = Some(plugin_id);
        }
    }

    /// URLを更新
    pub fn update_url(&mut self, connection_id: &Uuid, url: Option<String>) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                url = ?url,
                "Connection URL updated"
            );
            info.url = url.clone();
            info.input_state = url.as_deref().map(Self::input_state_from_url);
        }
    }

    /// 接続入力フォーム状態を更新する（現状はURLのみを正規化して保持）
    pub fn update_input_state(
        &mut self,
        connection_id: &Uuid,
        input_state: Option<serde_json::Value>,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            info.url = Self::url_from_input_state(&input_state);
            info.input_state = input_state;
        }
    }

    /// 既存の input_state に `patch` オブジェクトをマージする
    ///
    /// - `patch` のキーが既存 state に追加・上書きされる
    /// - `patch` の値が `null` の場合はそのキーを削除する
    /// - url が変化した場合は `self.url` も同期する
    pub fn merge_input_state(&mut self, connection_id: &Uuid, patch: serde_json::Value) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            let mut state = info
                .input_state
                .take()
                .unwrap_or_else(|| serde_json::json!({}));
            if let (Some(obj), Some(patch_obj)) = (state.as_object_mut(), patch.as_object()) {
                for (k, v) in patch_obj {
                    if v.is_null() {
                        obj.remove(k);
                    } else {
                        obj.insert(k.clone(), v.clone());
                    }
                }
            }
            info.url = Self::url_from_input_state(&Some(state.clone()));
            info.input_state = Some(state);
        }
    }

    /// コメント投稿フォーム状態を更新する（現状はtextのみ想定）
    pub fn update_comment_state(
        &mut self,
        connection_id: &Uuid,
        comment_state: Option<serde_json::Value>,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            info.comment_state = comment_state;
        }
    }

    /// ブラウザを更新
    pub fn update_browser(&mut self, connection_id: &Uuid, browser_id: Option<BrowserId>) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                browser_id = ?browser_id,
                "Connection browser updated"
            );
            info.browser_id = browser_id;
        }
    }

    /// 詳細設定を更新
    pub fn update_advanced_settings(
        &mut self,
        connection_id: &Uuid,
        settings: Option<serde_json::Value>,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                has_settings = settings.is_some(),
                "Connection advanced settings updated"
            );
            info.advanced_settings = settings;
        }
    }

    /// 永続化用データをエクスポート
    pub fn export_for_persistence(
        &self,
    ) -> Vec<crate::connection_persistence::PersistedConnection> {
        self.connections
            .values()
            .map(|conn| crate::connection_persistence::PersistedConnection {
                connection_id: conn.connection_id,
                site_id: conn.site_id.clone(),
                url: conn.url.clone(),
                browser_id: conn.browser_id.clone(),
                advanced_settings: conn.advanced_settings.clone(),
                input_state: conn.input_state.clone(),
                comment_state: conn.comment_state.clone(),
                name: conn.name.clone(),
            })
            .collect()
    }

    /// 永続化データからインポート（Pending状態で復元）
    /// 戻り値: (成功した接続数, スキップした接続のリスト[(name, reason)])
    pub fn import_from_persistence(
        &mut self,
        persisted: Vec<crate::connection_persistence::PersistedConnection>,
        site_browser_manager: &crate::site_browser_manager::SiteAndBrowserManager,
    ) -> (usize, Vec<(String, String)>) {
        let mut success_count = 0;
        let skipped = Vec::new();

        for conn in persisted {
            let mut input_state = conn.input_state;
            let mut url = conn.url;
            if input_state.is_none() {
                if let Some(u) = url.as_ref() {
                    input_state = Some(Self::input_state_from_url(u));
                }
            }
            if url.is_none() {
                url = Self::url_from_input_state(&input_state);
            }

            // site_idから現在のplugin_idを取得
            let (site_id, plugin_id) = if let Some(ref site_id) = conn.site_id {
                match site_browser_manager.get_site(site_id) {
                    Some(site_info) => (Some(site_id.clone()), Some(site_info.plugin_id)),
                    None => {
                        // プラグイン未到着 → Pending状態で復元
                        tracing::warn!(
                            connection_name = %conn.name,
                            site_id = %site_id,
                            "Site not found during restoration, marking as Pending"
                        );
                        (Some(site_id.clone()), None)
                    }
                }
            } else {
                (None, None)
            };

            let browser_id = conn.browser_id.clone();

            // ConnectionInfoを構築
            // Pendingはsite_idが指定されているのにプラグインがまだ未到着の場合のみ
            // site_idが未選択（None）の場合はCreated（通常の未接続状態）
            let status = if site_id.is_some() && plugin_id.is_none() {
                ConnectionStatus::Pending // site_idあり・プラグイン未到着
            } else {
                ConnectionStatus::Created // site_idなし、またはプラグイン到着済み
            };

            let info = ConnectionInfo {
                connection_id: conn.connection_id,
                plugin_id,
                status,
                site_id,
                url,
                browser_id,
                advanced_settings: conn.advanced_settings,
                input_state,
                comment_state: conn.comment_state,
                input_info: String::new(),
                name: conn.name,
                account_info: None,
            };

            self.connections.insert(info.connection_id, info);
            success_count += 1;
        }

        (success_count, skipped)
    }

    /// Pending状態の接続をsite_idで検索し、有効化
    pub fn activate_pending_connections_by_site(
        &mut self,
        site_id: &SiteId,
        plugin_id: Uuid,
    ) -> Vec<Uuid> {
        let mut activated = Vec::new();

        for conn in self.connections.values_mut() {
            if conn.status == ConnectionStatus::Pending && conn.site_id.as_ref() == Some(site_id) {
                tracing::info!(
                    connection_id = %conn.connection_id,
                    connection_name = %conn.name,
                    site_id = %site_id,
                    "Activating pending connection"
                );

                conn.plugin_id = Some(plugin_id);
                conn.status = ConnectionStatus::Created;
                activated.push(conn.connection_id);
            }
        }

        activated
    }
}

impl Default for ConnectionManager {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_connection_lifecycle() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        let _plugin_id = Uuid::new_v4();

        // 接続を追加（plugin_idはOptionに変更）
        manager.add_connection(conn_id, "Test Connection".to_string());
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Created)
        );

        // ステータスを更新
        manager.update_status(&conn_id, ConnectionStatus::Connected);
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Connected)
        );

        // 接続を削除
        let removed = manager.remove_connection(&conn_id);
        assert!(removed.is_some());
        assert_eq!(manager.get_status(&conn_id), None);
    }

    #[test]
    fn test_site_management() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        let site_id = SiteId::new("test-site", "00000000-0000-0000-0000-000000000001");
        let plugin_id = Uuid::new_v4();

        // 接続を追加（サイト未選択）
        manager.add_connection(conn_id, "#1".to_string());

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.plugin_id, None);
        assert_eq!(conn.site_id, None);

        // サイトを設定
        manager.set_site(&conn_id, site_id.clone(), plugin_id);

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.plugin_id, Some(plugin_id));
        assert_eq!(conn.site_id, Some(site_id));
    }

    #[test]
    fn test_connection_settings() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        let browser_id = BrowserId::new("Chrome", "00000000-0000-0000-0000-000000000001");

        // 接続を追加
        manager.add_connection(conn_id, "#1".to_string());

        // URL更新
        manager.update_url(&conn_id, Some("https://example.com".to_string()));
        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.url, Some("https://example.com".to_string()));

        // ブラウザ更新
        manager.update_browser(&conn_id, Some(browser_id.clone()));
        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.browser_id, Some(browser_id));

        // 詳細設定更新
        let settings = serde_json::json!({"key": "value"});
        manager.update_advanced_settings(&conn_id, Some(settings.clone()));
        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.advanced_settings, Some(settings));
    }

    #[test]
    fn test_multiple_connections() {
        let mut manager = ConnectionManager::new();
        let conn1 = Uuid::new_v4();
        let conn2 = Uuid::new_v4();

        manager.add_connection(conn1, "#1".to_string());
        manager.add_connection(conn2, "#2".to_string());

        let connections = manager.list_connections();
        assert_eq!(connections.len(), 2);
        assert!(connections.iter().any(|c| c.connection_id == conn1));
        assert!(connections.iter().any(|c| c.connection_id == conn2));
    }

    #[test]
    fn test_rename_connection() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        manager.rename_connection(&conn_id, "My Stream".to_string());

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.name, "My Stream");
    }

    #[test]
    fn test_connection_status_transitions() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Created)
        );

        manager.update_status(&conn_id, ConnectionStatus::Connecting);
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Connecting)
        );

        manager.update_status(&conn_id, ConnectionStatus::Connected);
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Connected)
        );

        manager.update_status(&conn_id, ConnectionStatus::Disconnected);
        assert_eq!(
            manager.get_status(&conn_id),
            Some(ConnectionStatus::Disconnected)
        );
    }

    #[test]
    fn test_url_update() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        manager.update_url(
            &conn_id,
            Some("https://youtube.com/watch?v=123".to_string()),
        );

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(
            conn.url,
            Some("https://youtube.com/watch?v=123".to_string())
        );
    }

    #[test]
    fn test_set_site_updates_plugin_id() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        let site_id = SiteId::new("YouTube", "00000000-0000-0000-0000-000000000002");
        let plugin_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        assert_eq!(manager.get_connection(&conn_id).unwrap().plugin_id, None);

        manager.set_site(&conn_id, site_id, plugin_id);
        assert_eq!(
            manager.get_connection(&conn_id).unwrap().plugin_id,
            Some(plugin_id)
        );
    }

    #[test]
    fn test_update_input_state_syncs_url() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        manager.add_connection(conn_id, "#1".to_string());

        manager.update_input_state(
            &conn_id,
            Some(serde_json::json!({"url": "https://example.com/live"})),
        );

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.url.as_deref(), Some("https://example.com/live"));
        assert_eq!(
            conn.input_state,
            Some(serde_json::json!({"url": "https://example.com/live"}))
        );
    }

    #[test]
    fn test_update_url_syncs_input_state() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        manager.add_connection(conn_id, "#1".to_string());

        manager.update_url(&conn_id, Some("https://example.com/watch?v=abc".to_string()));

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.url.as_deref(), Some("https://example.com/watch?v=abc"));
        assert_eq!(
            conn.input_state,
            Some(serde_json::json!({"url": "https://example.com/watch?v=abc"}))
        );
    }

    #[test]
    fn test_remove_nonexistent_connection() {
        let mut manager = ConnectionManager::new();
        let non_existent_id = Uuid::new_v4();

        let result = manager.remove_connection(&non_existent_id);
        assert!(result.is_none());
    }
}
