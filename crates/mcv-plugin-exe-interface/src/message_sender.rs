use mcv_messages::{
    Message as McvMessage, MessageDestination, MessageSource, MessageType, PluginHelloPayload,
};
use uuid::Uuid;

/// plugin-helloメッセージを構築
///
/// # Arguments
/// * `plugin_id` - プラグインID
/// * `name` - プラグイン名
/// * `roles` - プラグインのロール
pub(crate) fn build_plugin_hello_message(
    plugin_id: Uuid,
    name: &str,
    roles: Vec<&str>,
) -> Result<McvMessage, serde_json::Error> {
    let payload = PluginHelloPayload {
        name: name.to_string(),
        plugin_id,
        role: roles.iter().map(|s| s.to_string()).collect(),
        api_version: "v2".to_string(),
        send_comment_schema: None,
    };

    Ok(McvMessage::new_request(
        MessageType::PluginHello,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::to_value(&payload)?,
    ))
}

/// get-pluginsメッセージを構築
///
/// # Arguments
/// * `plugin_id` - プラグインID
pub(crate) fn build_get_plugins_message(plugin_id: Uuid) -> McvMessage {
    McvMessage::new_request(
        MessageType::GetPlugins,
        MessageSource::Plugin { plugin_id },
        MessageDestination::Core,
        serde_json::json!({}),
    )
}
