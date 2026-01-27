import { useState } from "react";
import { McvMessage } from "../types";

interface LogViewTabProps {
  messages: McvMessage[];
  onClear: () => void;
}

export default function LogViewTab({ messages, onClear }: LogViewTabProps) {
  const [filterType, setFilterType] = useState("");
  const [filterSrc, setFilterSrc] = useState("");
  const [filterDst, setFilterDst] = useState("");

  const filteredMessages = messages.filter((msg) => {
    if (filterType && msg.type !== filterType) return false;

    if (filterSrc) {
      const srcStr = JSON.stringify(msg.src).toLowerCase();
      if (!srcStr.includes(filterSrc.toLowerCase())) return false;
    }

    if (filterDst) {
      const dstStr = JSON.stringify(msg.dst).toLowerCase();
      if (!dstStr.includes(filterDst.toLowerCase())) return false;
    }

    return true;
  });

  // メッセージタイプの一覧を取得
  const messageTypes = Array.from(
    new Set(messages.map((msg) => msg.type))
  ).sort();

  return (
    <div className="p-4 space-y-4">
      <div className="flex justify-between items-center">
        <h2 className="text-xl font-bold">受信メッセージログ</h2>
        <button
          onClick={onClear}
          className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700"
        >
          ログをクリア
        </button>
      </div>

      {/* フィルター */}
      <div className="bg-gray-100 p-4 rounded space-y-3">
        <h3 className="font-semibold">フィルター</h3>
        <div className="grid grid-cols-3 gap-4">
          <div>
            <label className="block text-sm font-medium mb-1">
              メッセージタイプ
            </label>
            <select
              value={filterType}
              onChange={(e) => setFilterType(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded bg-white"
            >
              <option value="">すべて</option>
              {messageTypes.map((type) => (
                <option key={type} value={type}>
                  {type}
                </option>
              ))}
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">
              送信元 (src)
            </label>
            <input
              type="text"
              value={filterSrc}
              onChange={(e) => setFilterSrc(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
              placeholder="キーワード..."
            />
          </div>
          <div>
            <label className="block text-sm font-medium mb-1">宛先 (dst)</label>
            <input
              type="text"
              value={filterDst}
              onChange={(e) => setFilterDst(e.target.value)}
              className="w-full px-3 py-2 border border-gray-300 rounded"
              placeholder="キーワード..."
            />
          </div>
        </div>
        <div className="text-sm text-gray-600">
          {filteredMessages.length} / {messages.length} 件のメッセージを表示
        </div>
      </div>

      {/* メッセージリスト */}
      <div className="space-y-2 max-h-[600px] overflow-y-auto">
        {filteredMessages.map((msg, idx) => (
          <div
            key={idx}
            className="bg-white border border-gray-300 p-3 rounded hover:shadow-md transition-shadow"
          >
            <div className="flex justify-between items-start mb-2">
              <div className="flex items-center gap-2">
                <span className="px-2 py-1 bg-blue-100 text-blue-800 rounded text-xs font-semibold">
                  {msg.type}
                </span>
                <span className="text-xs text-gray-500">
                  {new Date(msg.timestamp).toLocaleString("ja-JP")}
                </span>
              </div>
              {msg.request_id && (
                <span className="text-xs font-mono text-gray-400">
                  Request: {msg.request_id}
                </span>
              )}
            </div>

            <div className="grid grid-cols-2 gap-2 mb-2 text-sm">
              <div>
                <span className="font-semibold text-gray-600">送信元:</span>
                <span className="ml-2 font-mono text-xs">
                  {JSON.stringify(msg.src)}
                </span>
              </div>
              <div>
                <span className="font-semibold text-gray-600">宛先:</span>
                <span className="ml-2 font-mono text-xs">
                  {JSON.stringify(msg.dst)}
                </span>
              </div>
            </div>

            <details className="text-sm">
              <summary className="cursor-pointer text-gray-600 hover:text-gray-800 font-semibold">
                Payload
              </summary>
              <pre className="mt-2 bg-gray-50 p-2 rounded overflow-x-auto text-xs font-mono">
                {JSON.stringify(msg.payload, null, 2)}
              </pre>
            </details>
          </div>
        ))}
        {filteredMessages.length === 0 && (
          <div className="text-center py-8 text-gray-500">
            {messages.length === 0
              ? "メッセージがありません"
              : "フィルターに一致するメッセージがありません"}
          </div>
        )}
      </div>
    </div>
  );
}
