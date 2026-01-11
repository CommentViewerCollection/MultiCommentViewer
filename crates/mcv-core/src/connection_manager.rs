use std::collections::HashMap;
use uuid::Uuid;

/// 接続ステータス
#[derive(Debug, Clone, PartialEq, Eq)]
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
#[derive(Debug, Clone)]
pub struct ConnectionInfo {
    pub connection_id: Uuid,
    pub plugin_id: Uuid,
    pub status: ConnectionStatus,
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
    pub fn add_connection(&mut self, connection_id: Uuid, plugin_id: Uuid) {
        let info = ConnectionInfo {
            connection_id,
            plugin_id,
            status: ConnectionStatus::Created,
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

    /// 全接続のリストを取得
    pub fn list_connections(&self) -> Vec<&ConnectionInfo> {
        self.connections.values().collect()
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

        // 接続を追加
        manager.add_connection(conn_id, plugin_id);
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
}
