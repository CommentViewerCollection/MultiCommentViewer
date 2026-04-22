import { useState, useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";

export default function ReplayTab() {
  const [sessionFiles, setSessionFiles] = useState<string[]>([]);
  const [selectedFile, setSelectedFile] = useState<string>("");
  const [isReplaying, setIsReplaying] = useState(false);
  const [message, setMessage] = useState<string | null>(null);

  const refreshFiles = async () => {
    try {
      const files = await invoke<string[]>("list_session_files");
      setSessionFiles(files);
      if (files.length > 0 && !selectedFile) {
        setSelectedFile(files[files.length - 1]);
      }
    } catch (error) {
      console.error("Failed to list session files:", error);
    }
  };

  useEffect(() => {
    refreshFiles();
  }, []);

  const handleReplay = async () => {
    if (!selectedFile) {
      setMessage("ファイルを選択してください");
      return;
    }
    setIsReplaying(true);
    setMessage(null);
    try {
      await invoke("start_replay", { fileName: selectedFile });
      setMessage(`再生を開始しました: ${selectedFile}`);
    } catch (error) {
      setMessage(`エラー: ${error}`);
    } finally {
      setIsReplaying(false);
    }
  };

  return (
    <div className="p-4">
      <h2 className="text-lg font-semibold mb-4">セッションリプレイ</h2>

      <div className="mb-4">
        <div className="flex items-center gap-2 mb-2">
          <label className="text-sm font-medium">セッションファイル</label>
          <button
            onClick={refreshFiles}
            className="px-2 py-1 text-xs bg-gray-200 rounded hover:bg-gray-300"
          >
            更新
          </button>
        </div>

        {sessionFiles.length === 0 ? (
          <p className="text-sm text-gray-500">
            保存済みセッションファイルがありません
          </p>
        ) : (
          <select
            value={selectedFile}
            onChange={(e) => setSelectedFile(e.target.value)}
            className="w-full px-3 py-2 border border-gray-300 rounded focus:outline-none focus:ring-2 focus:ring-blue-500 font-mono text-sm"
          >
            {sessionFiles.map((f) => (
              <option key={f} value={f}>
                {f}
              </option>
            ))}
          </select>
        )}
      </div>

      <button
        onClick={handleReplay}
        disabled={isReplaying || !selectedFile}
        className={`px-6 py-2 rounded font-semibold text-white ${
          isReplaying || !selectedFile
            ? "bg-gray-400 cursor-not-allowed"
            : "bg-blue-600 hover:bg-blue-700"
        }`}
      >
        {isReplaying ? "再生中..." : "再生"}
      </button>

      {message && (
        <p
          className={`mt-3 text-sm ${
            message.startsWith("エラー") ? "text-red-600" : "text-green-700"
          }`}
        >
          {message}
        </p>
      )}

      <div className="mt-6 p-3 bg-gray-50 rounded border border-gray-200 text-xs text-gray-600">
        <p className="font-medium mb-1">使い方</p>
        <ul className="list-disc ml-4 space-y-1">
          <li>
            セッションファイルは CommentReceived を受信した際に自動的に保存されます
          </li>
          <li>
            「再生」を押すと、保存時と同じタイミングでコメントが apps/mcv に送られます
          </li>
          <li>
            apps/mcv では「メッセージ再生」サイトを接続カードで選択し、URL
            欄にファイル名を入力して接続することでも再生できます
          </li>
        </ul>
      </div>
    </div>
  );
}
