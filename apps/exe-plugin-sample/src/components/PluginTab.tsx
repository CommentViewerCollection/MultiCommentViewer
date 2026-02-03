import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { PluginInfo } from "../types";

interface PluginTabProps {
  plugins: Map<string, PluginInfo>;
  selfPluginId: string | null;
}

export default function PluginTab({ plugins, selfPluginId }: PluginTabProps) {
  const [pluginName, setPluginName] = useState("");
  const [roles, setRoles] = useState("");
  const [apiVersion, setApiVersion] = useState("v2");
  const [selectedPluginId, setSelectedPluginId] = useState("");

  const handleSendPluginHello = async () => {
    if (!pluginName.trim()) {
      alert("プラグイン名を入力してください");
      return;
    }

    try {
      // ロールをカンマ区切りから配列に変換
      const roleArray = roles
        .split(',')
        .map(r => r.trim())
        .filter(r => r.length > 0);

      const message = {
        type: "plugin-hello",
        src: selfPluginId,
        dst: "core",
        timestamp: Date.now(),
        payload: {
          name: pluginName,
          plugin_id: selfPluginId,
          role: roleArray,
          api_version: apiVersion,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent plugin-hello:", message);
      alert("plugin-helloを送信しました");
    } catch (error) {
      console.error("Failed to send plugin-hello:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleSendPluginRemoved = async () => {
    if (!selectedPluginId) {
      alert("プラグインを選択してください");
      return;
    }

    try {
      const message = {
        type: "plugin-removed",
        src: selfPluginId,
        dst: "core",
        timestamp: Date.now(),
        payload: {
          plugin_id: selectedPluginId,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent plugin-removed:", message);
      alert("plugin-removedを送信しました");
    } catch (error) {
      console.error("Failed to send plugin-removed:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  return (
    <div className="p-4 space-y-6">
      <div>
        <h2 className="text-xl font-bold mb-4">登録済みプラグイン</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border border-gray-300">
            <thead className="bg-gray-100">
              <tr>
                <th className="px-4 py-2 border">Plugin ID</th>
                <th className="px-4 py-2 border">名前</th>
                <th className="px-4 py-2 border">ロール</th>
                <th className="px-4 py-2 border">API Version</th>
              </tr>
            </thead>
            <tbody>
              {Array.from(plugins.values()).map((plugin) => (
                <tr key={plugin.plugin_id} className="hover:bg-gray-50">
                  <td className="px-4 py-2 border font-mono text-sm">
                    {plugin.plugin_id}
                  </td>
                  <td className="px-4 py-2 border">{plugin.name}</td>
                  <td className="px-4 py-2 border">
                    {plugin.roles.join(", ")}
                  </td>
                  <td className="px-4 py-2 border">{plugin.api_version}</td>
                </tr>
              ))}
              {plugins.size === 0 && (
                <tr>
                  <td
                    colSpan={4}
                    className="px-4 py-8 text-center text-gray-500"
                  >
                    プラグインがありません
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">plugin-hello送信</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              プラグイン名
            </label>
            <input
              type="text"
              value={pluginName}
              onChange={(e) => setPluginName(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
              placeholder="My Plugin"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              ロール（カンマ区切り）
            </label>
            <input
              type="text"
              value={roles}
              onChange={(e) => setRoles(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
              placeholder="comment-provider, debug-tool"
            />
          </div>

          <div>
            <label className="block text-sm font-medium mb-1">
              API Version
            </label>
            <input
              type="text"
              value={apiVersion}
              onChange={(e) => setApiVersion(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
            />
          </div>

          <button
            onClick={handleSendPluginHello}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            送信
          </button>
        </div>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">plugin-removed送信</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              プラグインを選択
            </label>
            <select
              value={selectedPluginId}
              onChange={(e) => setSelectedPluginId(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
            >
              <option value="">選択してください</option>
              {Array.from(plugins.values()).map((plugin) => (
                <option key={plugin.plugin_id} value={plugin.plugin_id}>
                  {plugin.name} ({plugin.plugin_id})
                </option>
              ))}
            </select>
          </div>

          <button
            onClick={handleSendPluginRemoved}
            className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
          >
            送信
          </button>
        </div>
      </div>
    </div>
  );
}
