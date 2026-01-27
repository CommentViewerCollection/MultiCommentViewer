use mcv_common::PhysicalPluginId;
use mcv_messages::Message as McvMessage;

/// Core内部でのみ使用するメッセージ型
/// physical_plugin_idを含む
#[derive(Debug, Clone)]
pub struct InternalMessage {
    pub physical_plugin_id: PhysicalPluginId,
    pub message: McvMessage,
}
