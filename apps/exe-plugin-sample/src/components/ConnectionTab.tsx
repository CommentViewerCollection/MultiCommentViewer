import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { ConnectionInfo, SiteInfo } from "../types";

interface ConnectionTabProps {
  connections: Map<string, ConnectionInfo>;
  sites: Map<string, SiteInfo>;
  selfPluginId: string | null;
}

export default function ConnectionTab({
  connections,
  sites,
  selfPluginId,
}: ConnectionTabProps) {
  const [selectedSiteId, setSelectedSiteId] = useState("");
  const [selectedConnectionId, setSelectedConnectionId] = useState("");
  const [connectionName, setConnectionName] = useState("");

  const handleAddConnection = async () => {
    if (!selectedSiteId) {
      alert("サイトを選択してください");
      return;
    }

    try {
      const message = {
        type: "add-connection",
        src: { Plugin: { plugin_id: selfPluginId } },
        dst: { Core: null },
        timestamp: Date.now(),
        payload: {
          site_id: selectedSiteId,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent add-connection:", message);
      alert("add-connectionを送信しました");
    } catch (error) {
      console.error("Failed to send add-connection:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleConnect = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }

    try {
      const message = {
        type: "connect",
        src: { Core: null },
        dst: { Plugin: { plugin_id: selfPluginId } },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
          site: {
            site_id: "test-site",
            site_name: "test-site",
            display_name: "Test Site",
          },
          input: {
            url: "https://example.com/live/12345",
          },
          browser: {
            browser_id: "test-browser",
            browser_name: "test-browser",
            display_name: "Test Browser",
          },
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent connect:", message);
      alert("connectを送信しました");
    } catch (error) {
      console.error("Failed to send connect:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleDisconnect = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }

    try {
      const message = {
        type: "disconnect",
        src: { Core: null },
        dst: { Plugin: { plugin_id: selfPluginId } },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent disconnect:", message);
      alert("disconnectを送信しました");
    } catch (error) {
      console.error("Failed to send disconnect:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleRemoveConnection = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }

    try {
      const message = {
        type: "remove-connection",
        src: { Core: null },
        dst: { Plugin: { plugin_id: selfPluginId } },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent remove-connection:", message);
      alert("remove-connectionを送信しました");
    } catch (error) {
      console.error("Failed to send remove-connection:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleRenameConnection = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }
    if (!connectionName.trim()) {
      alert("新しい名前を入力してください");
      return;
    }

    try {
      const message = {
        type: "rename-connection",
        src: { Core: null },
        dst: { Plugin: { plugin_id: selfPluginId } },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
          name: connectionName,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent rename-connection:", message);
      alert("rename-connectionを送信しました");
      setConnectionName("");
    } catch (error) {
      console.error("Failed to send rename-connection:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  return (
    <div className="p-4 space-y-6">
      <div>
        <h2 className="text-xl font-bold mb-4">登録済みコネクション</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full bg-white border border-gray-300">
            <thead className="bg-gray-100">
              <tr>
                <th className="px-4 py-2 border">Connection ID</th>
                <th className="px-4 py-2 border">名前</th>
                <th className="px-4 py-2 border">Plugin ID</th>
                <th className="px-4 py-2 border">状態</th>
                <th className="px-4 py-2 border">サイト</th>
              </tr>
            </thead>
            <tbody>
              {Array.from(connections.values()).map((conn) => (
                <tr key={conn.connection_id} className="hover:bg-gray-50">
                  <td className="px-4 py-2 border font-mono text-sm">
                    {conn.connection_id}
                  </td>
                  <td className="px-4 py-2 border">{conn.name}</td>
                  <td className="px-4 py-2 border font-mono text-sm">
                    {conn.plugin_id}
                  </td>
                  <td className="px-4 py-2 border">
                    <span
                      className={`px-2 py-1 rounded text-xs font-semibold ${
                        conn.status === "connected"
                          ? "bg-green-100 text-green-800"
                          : conn.status === "connecting"
                          ? "bg-yellow-100 text-yellow-800"
                          : "bg-gray-100 text-gray-800"
                      }`}
                    >
                      {conn.status}
                    </span>
                  </td>
                  <td className="px-4 py-2 border">{conn.site_name || "-"}</td>
                </tr>
              ))}
              {connections.size === 0 && (
                <tr>
                  <td
                    colSpan={5}
                    className="px-4 py-8 text-center text-gray-500"
                  >
                    コネクションがありません
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">add-connection送信</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              サイトを選択
            </label>
            <select
              value={selectedSiteId}
              onChange={(e) => setSelectedSiteId(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
            >
              <option value="">選択してください</option>
              {Array.from(sites.values()).map((site) => (
                <option key={site.site_id} value={site.site_id}>
                  {site.display_name} ({site.site_name})
                </option>
              ))}
            </select>
          </div>

          <button
            onClick={handleAddConnection}
            className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
          >
            送信
          </button>
        </div>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">コネクション操作</h3>
        <div className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              コネクションを選択
            </label>
            <select
              value={selectedConnectionId}
              onChange={(e) => setSelectedConnectionId(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
            >
              <option value="">選択してください</option>
              {Array.from(connections.values()).map((conn) => (
                <option key={conn.connection_id} value={conn.connection_id}>
                  {conn.name} ({conn.connection_id})
                </option>
              ))}
            </select>
          </div>

          <div className="flex gap-2">
            <button
              onClick={handleConnect}
              className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
            >
              connect送信
            </button>
            <button
              onClick={handleDisconnect}
              className="px-4 py-2 bg-yellow-600 text-white rounded hover:bg-yellow-700"
            >
              disconnect送信
            </button>
            <button
              onClick={handleRemoveConnection}
              className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
            >
              remove-connection送信
            </button>
          </div>

          <div className="flex gap-2">
            <input
              type="text"
              value={connectionName}
              onChange={(e) => setConnectionName(e.target.value)}
              className="flex-1 px-3 py-2 border border-gray-300 rounded"
              placeholder="新しい名前"
            />
            <button
              onClick={handleRenameConnection}
              className="px-4 py-2 bg-purple-600 text-white rounded hover:bg-purple-700"
            >
              rename-connection送信
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
