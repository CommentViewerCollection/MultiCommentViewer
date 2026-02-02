use std::collections::HashMap;
use uuid::Uuid;
use serde::{Serialize, Deserialize};

/// 接続ステータス
#[derive(Debug, Clone, PartialEq, Eq, Serialize, Deserialize)]
#[serde(tag = "type", content = "message")]
pub enum ConnectionStatus {
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
    pub plugin_id: Option<Uuid>,      // 変更: Option<Uuid>に
    pub status: ConnectionStatus,
    pub site_id: Option<Uuid>,        // 新規
    pub site_name: Option<String>,
    pub url: Option<String>,          // 新規
    pub browser_id: Option<Uuid>,     // 新規
    pub browser_name: Option<String>, // 新規
    pub advanced_settings: Option<serde_json::Value>, // 新規
    pub input_info: String,
    pub name: String,
}

/// Connection Manager
///
/// 接続インスタンスの管理を担当
pub struct ConnectionManager {
    connections: HashMap<Uuid, ConnectionInfo>,
}

impl ConnectionManager {
    /// 新しいConnection Managerを作成
    pub fn new() -> Self {
        Self {
            connections: HashMap::new(),
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
            site_name:None,
            url: None,
            browser_id: None,
            browser_name: None,
            advanced_settings: None,
            input_info: "".to_string(),
            name,
        };
        self.connections.insert(connection_id, info);
    }

    /// 接続を削除
    pub fn remove_connection(&mut self, connection_id: &Uuid) -> Option<ConnectionInfo> {
        self.connections.remove(connection_id)
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
    pub fn get_connections(&self)->Vec<&ConnectionInfo>{
        self.connections.iter().map(|a|a.1).collect()
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
    pub fn set_site(
        &mut self,
        connection_id: &Uuid,
        site_id: Uuid,
        site_name: String,
        plugin_id: Uuid,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                site_id = %site_id,
                site_name = %site_name,
                plugin_id = %plugin_id,
                "Connection site updated"
            );
            info.site_id = Some(site_id);
            info.site_name = Some(site_name);
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
            info.url = url;
        }
    }

    /// ブラウザを更新
    pub fn update_browser(
        &mut self,
        connection_id: &Uuid,
        browser_id: Option<Uuid>,
        browser_name: Option<String>,
    ) {
        if let Some(info) = self.connections.get_mut(connection_id) {
            tracing::debug!(
                connection_id = %connection_id,
                browser_id = ?browser_id,
                browser_name = ?browser_name,
                "Connection browser updated"
            );
            info.browser_id = browser_id;
            info.browser_name = browser_name;
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
        let plugin_id = Uuid::new_v4();

        // 接続を追加（plugin_idはOptionに変更）
        manager.add_connection(conn_id,  "Test Connection".to_string());
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
        let site_id = Uuid::new_v4();
        let plugin_id = Uuid::new_v4();

        // 接続を追加（サイト未選択）
        manager.add_connection(conn_id, "#1".to_string());

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.plugin_id, None);
        assert_eq!(conn.site_id, None);

        // サイトを設定
        manager.set_site(&conn_id, site_id, "Test Site".to_string(), plugin_id);

        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.plugin_id, Some(plugin_id));
        assert_eq!(conn.site_id, Some(site_id));
        assert_eq!(conn.site_name, Some("Test Site".to_owned()));
    }

    #[test]
    fn test_connection_settings() {
        let mut manager = ConnectionManager::new();
        let conn_id = Uuid::new_v4();
        let browser_id = Uuid::new_v4();

        // 接続を追加
        manager.add_connection(conn_id, "#1".to_string());

        // URL更新
        manager.update_url(&conn_id, Some("https://example.com".to_string()));
        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.url, Some("https://example.com".to_string()));

        // ブラウザ更新
        manager.update_browser(&conn_id, Some(browser_id), Some("Chrome".to_string()));
        let conn = manager.get_connection(&conn_id).unwrap();
        assert_eq!(conn.browser_id, Some(browser_id));
        assert_eq!(conn.browser_name, Some("Chrome".to_string()));

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
        manager.update_url(&conn_id, Some("https://youtube.com/watch?v=123".to_string()));

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
        let site_id = Uuid::new_v4();
        let plugin_id = Uuid::new_v4();

        manager.add_connection(conn_id, "#1".to_string());
        assert_eq!(manager.get_connection(&conn_id).unwrap().plugin_id, None);

        manager.set_site(&conn_id, site_id, "YouTube".to_string(), plugin_id);
        assert_eq!(
            manager.get_connection(&conn_id).unwrap().plugin_id,
            Some(plugin_id)
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
