use mcv_messages::Message as McvMessage;
use uuid::Uuid;

/// メッセージルーティング
///
/// Core ↔ EXEプラグイン間のメッセージをルーティングする
pub struct MessageRouter {
    // TODO: ルーティングテーブルを保持
}

impl MessageRouter {
    pub fn new() -> Self {
        Self {}
    }

    /// Coreから受信したメッセージを適切なEXEプラグインへルーティング
    pub async fn route_from_core(&self, _message: McvMessage) -> Result<(), String> {
        // TODO: 実装
        Ok(())
    }

    /// EXEプラグインから受信したメッセージをCoreへルーティング
    pub async fn route_from_plugin(&self, _plugin_id: Uuid, _message: McvMessage) -> Result<(), String> {
        // TODO: 実装
        Ok(())
    }
}

impl Default for MessageRouter {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_router_creation() {
        let _router = MessageRouter::new();
    }
}
