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
  const [browsers, setBrowsers] = useState<Map<string, BrowserInfo>>(new Map());

  useEffect(() => {
    // メッセージ受信リスナー
    const unlisten = listen<McvMessage>("message-received", (event) => {
      const message = event.payload;
      console.log("Received message:", message);
      setMessages((prev) => [...prev, message]);

      // メッセージタイプに応じて状態を更新
      handleMessageReceived(message);
    });

    return () => {
      unlisten.then((fn) => fn());
    };
  }, []);

  const handleMessageReceived = (message: McvMessage) => {
    const { type, payload } = message;

    switch (type) {
      case "plugin-added":
        setPlugins((prev) => {
          const newPlugins = new Map(prev);
          newPlugins.set(payload.plugin_id, {
            plugin_id: payload.plugin_id,
            name: payload.name || "Unknown",
            roles: payload.role || [],
            api_version: payload.api_version || "v2",
          });
          return newPlugins;
        });
        break;

      case "plugin-removed":
        setPlugins((prev) => {
          const newPlugins = new Map(prev);
          newPlugins.delete(payload.plugin_id);
          return newPlugins;
        });
        break;

      case "connection-added":
        setConnections((prev) => {
          const newConnections = new Map(prev);
          newConnections.set(payload.connection_id, {
            connection_id: payload.connection_id,
            plugin_id: payload.plugin_id || "unknown",
            name: payload.name || `#${newConnections.size + 1}`,
            status: "disconnected",
          });
          return newConnections;
        });
        break;

      case "connection-removed":
        setConnections((prev) => {
          const newConnections = new Map(prev);
          newConnections.delete(payload.connection_id);
          return newConnections;
        });
        break;

      case "connected":
        setConnections((prev) => {
          const newConnections = new Map(prev);
          const conn = newConnections.get(payload.connection_id);
          if (conn) {
            newConnections.set(payload.connection_id, {
              ...conn,
              status: "connected",
              site_name: payload.site?.site_name,
            });
          }
          return newConnections;
        });
        break;

      case "disconnected":
        setConnections((prev) => {
          const newConnections = new Map(prev);
          const conn = newConnections.get(payload.connection_id);
          if (conn) {
            newConnections.set(payload.connection_id, {
              ...conn,
              status: "disconnected",
            });
          }
          return newConnections;
        });
        break;

      case "site-added":
        setSites((prev) => {
          const newSites = new Map(prev);
          newSites.set(payload.site_id, {
            site_id: payload.site_id,
            site_name: payload.site_name,
            display_name: payload.display_name,
            plugin_id: payload.plugin_id,
          });
          return newSites;
        });
        break;

      case "site-removed":
        setSites((prev) => {
          const newSites = new Map(prev);
          newSites.delete(payload.site_id);
          return newSites;
        });
        break;

      case "browser-added":
        setBrowsers((prev) => {
          const newBrowsers = new Map(prev);
          newBrowsers.set(payload.browser_id, {
            browser_id: payload.browser_id,
            browser_name: payload.browser_name,
            display_name: payload.display_name,
            plugin_id: payload.plugin_id,
          });
          return newBrowsers;
        });
        break;

      case "browser-removed":
        setBrowsers((prev) => {
          const newBrowsers = new Map(prev);
          newBrowsers.delete(payload.browser_id);
          return newBrowsers;
        });
        break;
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
