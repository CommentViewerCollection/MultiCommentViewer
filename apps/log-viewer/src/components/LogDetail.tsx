import { useState } from "react";
import StackTrace from "./StackTrace";
import type { LogEntry } from "../types/log";

interface LogDetailProps {
  log: LogEntry;
  onDelete: () => void;
}

const levelColors: Record<string, string> = {
  trace: "bg-gray-100 text-gray-800",
  debug: "bg-blue-100 text-blue-800",
  info: "bg-green-100 text-green-800",
  warn: "bg-yellow-100 text-yellow-800",
  error: "bg-red-100 text-red-800",
};

function formatTimestamp(timestamp: number): string {
  const date = new Date(timestamp);
  return date.toLocaleString();
}

export default function LogDetail({ log, onDelete }: LogDetailProps) {
  const [copied, setCopied] = useState(false);

  const handleCopy = () => {
    navigator.clipboard.writeText(JSON.stringify(log, null, 2));
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="p-4">
      {/* Header */}
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-bold text-gray-800">Log Details</h2>
        <div className="flex items-center gap-2">
          <div className="relative">
            <button
              onClick={handleCopy}
              className="px-3 py-1 text-sm text-gray-600 hover:bg-gray-100 rounded"
            >
              Copy
            </button>
            {copied && (
              <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-1 px-2 py-1 text-xs text-white bg-gray-800 rounded whitespace-nowrap pointer-events-none">
                Copied!
              </div>
            )}
          </div>
          <button
            onClick={() => {
              if (confirm("Are you sure you want to delete this log entry?")) {
                onDelete();
              }
            }}
            className="px-3 py-1 text-sm text-red-600 hover:bg-red-50 rounded"
          >
            Delete
          </button>
        </div>
      </div>

      {/* Level and Timestamp */}
      <div className="mb-4">
        <div className="flex items-center gap-2 mb-2">
          <span
            className={`px-3 py-1 rounded text-sm font-medium uppercase ${
              levelColors[log.level]
            }`}
          >
            {log.level}
          </span>
          <span className="text-sm text-gray-500">
            {formatTimestamp(log.timestamp)}
          </span>
        </div>
      </div>

      {/* Message */}
      <div className="mb-4">
        <h3 className="text-sm font-semibold text-gray-700 mb-1">Message</h3>
        <p className="text-sm text-gray-800 bg-gray-50 p-3 rounded border border-gray-200 whitespace-pre-wrap">
          {log.message}
        </p>
      </div>

      {/* Source Location */}
      <div className="mb-4">
        <h3 className="text-sm font-semibold text-gray-700 mb-1">
          Source Location
        </h3>
        <div className="text-sm text-gray-800 bg-gray-50 p-3 rounded border border-gray-200">
          <div>
            <span className="font-medium">File:</span> {log.source.file}
          </div>
          <div>
            <span className="font-medium">Line:</span> {log.source.line}
            {log.source.column && `, Column: ${log.source.column}`}
          </div>
          <div>
            <span className="font-medium">Module:</span> {log.source.module_path}
          </div>
        </div>
      </div>

      {/* Context */}
      {log.context && Object.keys(log.context).length > 0 && (
        <div className="mb-4">
          <h3 className="text-sm font-semibold text-gray-700 mb-1">Context</h3>
          <pre className="text-xs text-gray-800 bg-gray-50 p-3 rounded border border-gray-200 overflow-auto max-h-64">
            {JSON.stringify(log.context, null, 2)}
          </pre>
        </div>
      )}

      {/* Stack Trace */}
      {log.stacktrace && log.stacktrace.length > 0 && (
        <div className="mb-4">
          <h3 className="text-sm font-semibold text-gray-700 mb-1">
            Stack Trace
          </h3>
          <StackTrace frames={log.stacktrace} />
        </div>
      )}

      {/* System Info */}
      <div className="mb-4">
        <h3 className="text-sm font-semibold text-gray-700 mb-1">
          System Info
        </h3>
        <div className="text-sm text-gray-800 bg-gray-50 p-3 rounded border border-gray-200">
          <div>
            <span className="font-medium">MCV Version:</span>{" "}
            {log.system_info.mcv_version}
          </div>
          <div>
            <span className="font-medium">Platform:</span>{" "}
            {log.system_info.platform}
          </div>
          <div>
            <span className="font-medium">Architecture:</span>{" "}
            {log.system_info.arch}
          </div>
          <div>
            <span className="font-medium">Build Profile:</span>{" "}
            {log.system_info.build_profile}
          </div>
        </div>
      </div>

      {/* Log ID */}
      <div className="text-xs text-gray-500">
        <span className="font-medium">ID:</span> {log.id}
      </div>
    </div>
  );
}
