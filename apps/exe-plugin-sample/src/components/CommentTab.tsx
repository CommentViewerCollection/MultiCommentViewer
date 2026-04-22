import { useState } from "react";
import { invoke } from "@tauri-apps/api/core";
import { ConnectionInfo } from "../types";

interface CommentTabProps {
  connections: Map<string, ConnectionInfo>;
  selfPluginId: string | null;
}

export default function CommentTab({
  connections,
  selfPluginId,
}: CommentTabProps) {
  const [selectedConnectionId, setSelectedConnectionId] = useState("");
  const [commentText, setCommentText] = useState("");
  const [userName, setUserName] = useState("テストユーザー");
  const [userId, setUserId] = useState("test-user-123");

  const handleSendComment = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }
    if (!commentText.trim()) {
      alert("コメントを入力してください");
      return;
    }

    try {
      const message = {
        type: "send-comment",
        src: { Core: null },
        dst: { Plugin: { plugin_id: selfPluginId } },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
          text: commentText,
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent send-comment:", message);
      alert("send-commentを送信しました");
      setCommentText("");
    } catch (error) {
      console.error("Failed to send send-comment:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  const handleCommentReceived = async () => {
    if (!selectedConnectionId) {
      alert("コネクションを選択してください");
      return;
    }
    if (!commentText.trim()) {
      alert("コメントを入力してください");
      return;
    }

    try {
      const message = {
        type: "comment-received",
        src: { Plugin: { plugin_id: selfPluginId } },
        dst: { Core: null },
        timestamp: Date.now(),
        payload: {
          connection_id: selectedConnectionId,
          comment: {
            id: `comment-${Date.now()}`,
            user_name: userName,
            user_id: userId,
            text: commentText,
            timestamp: Date.now(),
            platform_type: "test",
          },
        },
      };

      await invoke("send_message", {
        messageJson: JSON.stringify(message),
      });

      console.log("Sent comment-received:", message);
      alert("comment-receivedを送信しました（テスト用）");
      setCommentText("");
    } catch (error) {
      console.error("Failed to send comment-received:", error);
      alert(`送信失敗: ${error}`);
    }
  };

  return (
    <div className="p-4 space-y-6">
      <div>
        <h2 className="text-xl font-bold mb-4">コメント送信</h2>

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

          <div>
            <label className="block text-sm font-medium mb-1">
              コメント内容
            </label>
            <textarea
              value={commentText}
              onChange={(e) => setCommentText(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
              rows={4}
              placeholder="コメントを入力..."
            />
          </div>
        </div>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">send-comment送信</h3>
        <p className="text-sm text-gray-600 mb-4">
          プラグインにコメント投稿を依頼します
        </p>
        <button
          onClick={handleSendComment}
          className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700"
        >
          送信
        </button>
      </div>

      <div className="border-t pt-6">
        <h3 className="text-lg font-semibold mb-4">
          comment-received送信（テスト用）
        </h3>
        <p className="text-sm text-gray-600 mb-4">
          プラグインからコメントを受信したことをシミュレートします
        </p>

        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">
                ユーザー名
              </label>
              <input
                type="text"
                value={userName}
                onChange={(e) => setUserName(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded"
              />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">
                ユーザーID
              </label>
              <input
                type="text"
                value={userId}
                onChange={(e) => setUserId(e.target.value)}
                className="w-full px-3 py-2 border border-gray-300 rounded"
              />
            </div>
          </div>

          <button
            onClick={handleCommentReceived}
            className="px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700"
          >
            送信
          </button>
        </div>
      </div>
    </div>
  );
}
