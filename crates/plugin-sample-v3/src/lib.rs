use plugin_abi_helper::abi::v3::PluginV3;
use plugin_abi_helper::export_plugin_v3;
use plugin_abi_helper::v3::host::Host;
use plugin_abi_helper::v3::plugin::PluginFactoryV3;
use plugin_abi_helper::v3::plugin::PluginImplV3;

#[derive(Default)]
struct SamplePlugin;

impl PluginImplV3 for SamplePlugin {
    fn on_loaded(&mut self, host: Host) {
        host.send_message(b"plugin-hello");
    }

    fn on_message(&mut self, _host: Host, _msg: &[u8]) {
        // 今回は未使用
    }

    fn on_shutdown(&mut self, _host: Host) {
        // クリーンアップがあればここ
    }
}

// これだけで ABI エクスポートが完成
export_plugin_v3!(SamplePlugin);
