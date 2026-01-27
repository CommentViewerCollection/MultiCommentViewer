import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";

export default function RawMessageTab() {
  const [messageJson, setMessageJson] = useState(`{
  "type": "plugin-hello",
  "src": { "Plugin": { "plugin_id": "00000000-0000-0000-0000-000000000000" } },
  "dst": { "Core": null },
  "timestamp": ${Date.now()},
  "payload": {
    "name": "Test Plugin",
    "plugin_id": "00000000-0000-0000-0000-000000000000",
    "role": ["debug-tool"],
    "api_version": "v2"
  }
}`);

  const [error, setError] = useState<string | null>(null);

  const handleSend = async () => {
    setError(null);

    // JSONの妥当性チェック
    try {
      JSON.parse(messageJson);
    } catch (e) {
      setError(`無効なJSON: ${e}`);
      return;
    }

    try {
      await invoke("send_message", {
        messageJson: messageJson,
      });

      console.log("Sent raw message");
      alert("メッセージを送信しました");
    } catch (error) {
      console.error("Failed to send message:", error);
      setError(`送信失敗: ${error}`);
    }
  };

  const handleFormat = () => {
    try {
      const parsed = JSON.parse(messageJson);
      setMessageJson(JSON.stringify(parsed, null, 2));
      setError(null);
    } catch (e) {
      setError(`無効なJSON: ${e}`);
    }
  };

  const handleClear = () => {
    setMessageJson("{\n  \n}");
    setError(null);
  };

  return (
    <div className="p-4 space-y-4">
      <div>
        <h2 className="text-xl font-bold mb-4">生メッセージ送信</h2>
        <p className="text-sm text-gray-600 mb-4">
          JSON形式でメッセージを直接入力して送信できます
        </p>
      </div>

      <div>
        <label className="block text-sm font-medium mb-2">JSON入力</label>
        <textarea
          value={messageJson}
          onChange={(e) => {
            setMessageJson(e.target.value);
            setError(null);
          }}
          className="w-full px-3 py-2 border border-gray-300 rounded font-mono text-sm"
          rows={20}
          spellCheck={false}
        />
      </div>

      {error && (
        <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {error}
        </div>
      )}

      <div className="flex gap-2">
        <button
          onClick={handleSend}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 font-semibold"
        >
          送信
        </button>
        <button
          onClick={handleFormat}
          className="px-4 py-2 bg-gray-600 text-white rounded hover:bg-gray-700"
        >
          フォーマット
        </button>
        <button
          onClick={handleClear}
          className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
        >
          クリア
        </button>
      </div>

      <div className="border-t pt-4">
        <h3 className="text-lg font-semibold mb-2">テンプレート</h3>
        <p className="text-sm text-gray-600 mb-4">
          クリックしてテンプレートを読み込みます
        </p>
        <div className="grid grid-cols-2 gap-2">
          <button
            onClick={() =>
              setMessageJson(
                JSON.stringify(
                  {
                    type: "plugin-hello",
                    src: {
                      Plugin: {
                        plugin_id: "00000000-0000-0000-0000-000000000000",
                      },
                    },
                    dst: { Core: null },
                    timestamp: Date.now(),
                    payload: {
                      name: "Test Plugin",
                      plugin_id: "00000000-0000-0000-0000-000000000000",
                      role: ["debug-tool"],
                      api_version: "v2",
                    },
                  },
                  null,
                  2
                )
              )
            }
            className="px-3 py-2 bg-gray-100 border border-gray-300 rounded hover:bg-gray-200 text-sm"
          >
            plugin-hello
          </button>
          <button
            onClick={() =>
              setMessageJson(
                JSON.stringify(
                  {
                    type: "add-connection",
                    src: {
                      Plugin: {
                        plugin_id: "00000000-0000-0000-0000-000000000000",
                      },
                    },
                    dst: { Core: null },
                    timestamp: Date.now(),
                    payload: {
                      site_id: "test-site",
                    },
                  },
                  null,
                  2
                )
              )
            }
            className="px-3 py-2 bg-gray-100 border border-gray-300 rounded hover:bg-gray-200 text-sm"
          >
            add-connection
          </button>
          <button
            onClick={() =>
              setMessageJson(
                JSON.stringify(
                  {
                    type: "comment-received",
                    src: {
                      Plugin: {
                        plugin_id: "00000000-0000-0000-0000-000000000000",
                      },
                    },
                    dst: { Core: null },
                    timestamp: Date.now(),
                    payload: {
                      connection_id: "00000000-0000-0000-0000-000000000000",
                      comment: {
                        id: `comment-${Date.now()}`,
                        user_name: "テストユーザー",
                        user_id: "test-user-123",
                        text: "テストコメント",
                        timestamp: Date.now(),
                        platform_type: "test",
                      },
                    },
                  },
                  null,
                  2
                )
              )
            }
            className="px-3 py-2 bg-gray-100 border border-gray-300 rounded hover:bg-gray-200 text-sm"
          >
            comment-received
          </button>
          <button
            onClick={() =>
              setMessageJson(
                JSON.stringify(
                  {
                    type: "send-comment",
                    src: { Core: null },
                    dst: {
                      Plugin: {
                        plugin_id: "00000000-0000-0000-0000-000000000000",
                      },
                    },
                    timestamp: Date.now(),
                    payload: {
                      connection_id: "00000000-0000-0000-0000-000000000000",
                      text: "送信するコメント",
                    },
                  },
                  null,
                  2
                )
              )
            }
            className="px-3 py-2 bg-gray-100 border border-gray-300 rounded hover:bg-gray-200 text-sm"
          >
            send-comment
          </button>
        </div>
      </div>
    </div>
  );
}
