import { useState, useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";
import { listen } from "@tauri-apps/api/event";
import {
  McvMessage,
  PluginInfo,
  ConnectionInfo,
  SiteInfo,
  BrowserInfo,
} from "./types";
import PluginTab from "./components/PluginTab";
import ConnectionTab from "./components/ConnectionTab";
import CommentTab from "./components/CommentTab";
import RawMessageTab from "./components/RawMessageTab";
import LogViewTab from "./components/LogViewTab";

function App() {
  const [connected, setConnected] = useState(false);
  const [websocketUrl, setWebsocketUrl] = useState("ws://127.0.0.1:28901");
  const [pluginName, setPluginName] = useState("exe-plugin-sample");
  const [pluginId, setPluginId] = useState<string | null>(null);
  const [activeTab, setActiveTab] = useState("plugin");
  const [messages, setMessages] = useState<McvMessage[]>([]);

  // State for entities
  const [plugins, setPlugins] = useState<Map<string, PluginInfo>>(new Map());
  const [connections, setConnections] = useState<Map<string, ConnectionInfo>>(
    new Map()
  );
  const [sites, setSites] = useState<Map<string, SiteInfo>>(new Map());
  const [_browsers, setBrowsers] = useState<Map<string, BrowserInfo>>(new Map());

  // プラグイン一覧を再取得
  const refreshPlugins = async () => {
    try {
      const pluginList = await invoke<PluginInfo[]>("get_plugins");
      const newPlugins = new Map(pluginList.map((p) => [p.plugin_id, p]));
      setPlugins(newPlugins);
    } catch (error) {
      console.error("Failed to get plugins:", error);
    }
  };

  // 接続一覧を再取得
  const refreshConnections = async () => {
    try {
      const connList = await invoke<ConnectionInfo[]>("get_connections");
      const newConnections = new Map(
        connList.map((c) => [c.connection_id, c])
      );
      setConnections(newConnections);
    } catch (error) {
      console.error("Failed to get connections:", error);
    }
  };

  useEffect(() => {
    // メッセージ受信リスナー
    const unlisten = listen<McvMessage>("message-received", (event) => {
      const message = event.payload;
      console.log("Received message:", message);
      setMessages((prev) => [...prev, message]);

      // メッセージタイプに応じてRust側の状態を再取得
      handleMessageReceived(message);
    });

    return () => {
      unlisten.then((fn) => fn());
    };
  }, []);

  const handleMessageReceived = async (message: McvMessage) => {
    const { type } = message;
    console.log("Message type:", type);

    // Rust側で状態管理しているので、Rust側から最新状態を取得
    // 手動更新ロジックを削除し、Rust側の状態取得コマンドを使用
    try {
      switch (type) {
        case "plugin-added":
        case "plugin-removed":
          // プラグイン関連のメッセージ → プラグイン一覧を再取得
          await refreshPlugins();
          break;

        case "connection-added":
        case "connection-removed":
        case "connected":
        case "disconnected":
          // 接続関連のメッセージ → 接続一覧を再取得
          await refreshConnections();
          break;

        case "site-added":
        case "site-removed":
          // サイト関連（Rust側で管理していないためスキップ）
          // 将来的にRust側で管理する場合は refreshSites() を実装
          break;

        case "browser-added":
        case "browser-removed":
          // ブラウザ関連（Rust側で管理していないためスキップ）
          // 将来的にRust側で管理する場合は refreshBrowsers() を実装
          break;
      }
    } catch (error) {
      console.error("Failed to refresh state:", error);
    }
  };

  const handleConnect = async () => {
    try {
      const id = await invoke<string>("connect_to_mcv", {
        url: websocketUrl,
        pluginName: pluginName,
        roles: ["debug-tool"],
      });
      setPluginId(id);
      setConnected(true);
      console.log("Connected with plugin_id:", id);

      // 接続後、初回状態を取得
      await refreshPlugins();
      await refreshConnections();
    } catch (error) {
      console.error("Failed to connect:", error);
      alert(`接続失敗: ${error}`);
    }
  };

  const handleDisconnect = async () => {
    try {
      await invoke("disconnect_from_mcv");
      setConnected(false);
      setPluginId(null);
      setMessages([]);
      setPlugins(new Map());
      setConnections(new Map());
      setSites(new Map());
      setBrowsers(new Map());
      console.log("Disconnected");
    } catch (error) {
      console.error("Failed to disconnect:", error);
    }
  };

  const handleClearLogs = () => {
    setMessages([]);
  };

  const renderTabContent = () => {
    switch (activeTab) {
      case "plugin":
        return <PluginTab plugins={plugins} selfPluginId={pluginId} />;
      case "connection":
        return (
          <ConnectionTab
            connections={connections}
            sites={sites}
            selfPluginId={pluginId}
          />
        );
      case "comment":
        return <CommentTab connections={connections} selfPluginId={pluginId} />;
      case "raw":
        return <RawMessageTab />;
      case "log":
        return <LogViewTab messages={messages} onClear={handleClearLogs} />;
      default:
        return null;
    }
  };

  return (
    <div className="w-full h-full flex flex-col bg-gray-50">
      {/* Header */}
      <div className="bg-blue-600 text-white p-4 shadow-md">
        <h1 className="text-2xl font-bold">MCV EXE Plugin Sample</h1>
        <div className="mt-2 flex items-center gap-4">
          <div className="flex items-center gap-2">
            <span className="text-sm">状態:</span>
            <span
              className={`px-3 py-1 rounded-full text-xs font-semibold ${
                connected ? "bg-green-500" : "bg-red-500"
              }`}
            >
              {connected ? "接続中" : "切断"}
            </span>
          </div>
          {pluginId && (
            <div className="text-sm">
              Plugin ID: <span className="font-mono">{pluginId}</span>
            </div>
          )}
        </div>
      </div>

      {/* Connection Controls */}
      {!connected ? (
        <div className="p-4 bg-white shadow-md">
          <div className="flex gap-4 items-end">
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">
                WebSocket URL
              </label>
              <input
                type="text"
                value={websocketUrl}
                onChange={(e) => setWebsocketUrl(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <div className="flex-1">
              <label className="block text-sm font-medium mb-1">
                プラグイン名
              </label>
              <input
                type="text"
                value={pluginName}
                onChange={(e) => setPluginName(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
              />
            </div>
            <button
              onClick={handleConnect}
              className="px-6 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 font-semibold"
            >
              接続
            </button>
          </div>
        </div>
      ) : (
        <div className="p-4 bg-white shadow-md">
          <button
            onClick={handleDisconnect}
            className="px-6 py-2 bg-red-600 text-white rounded hover:bg-red-700 font-semibold"
          >
            切断
          </button>
        </div>
      )}

      {/* Tabs */}
      {connected && (
        <>
          <div className="flex border-b border-gray-300 bg-white">
            {[
              { id: "plugin", label: "プラグイン" },
              { id: "connection", label: "コネクション" },
              { id: "comment", label: "コメント" },
              { id: "raw", label: "生メッセージ" },
              { id: "log", label: "ログ" },
            ].map((tab) => (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`px-6 py-3 font-medium ${
                  activeTab === tab.id
                    ? "border-b-2 border-blue-600 text-blue-600"
                    : "text-gray-600 hover:text-gray-800"
                }`}
              >
                {tab.label}
              </button>
            ))}
          </div>

          {/* Tab Content */}
          <div className="flex-1 overflow-auto bg-white">
            {renderTabContent()}
          </div>
        </>
      )}
    </div>
  );
}

export default App;
