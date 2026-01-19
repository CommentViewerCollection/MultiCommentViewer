import { Virtuoso } from "react-virtuoso";
import type { LogEntry } from "../types/log";

interface LogTableProps {
  logs: LogEntry[];
  selectedLog: LogEntry | null;
  onSelectLog: (log: LogEntry) => void;
  onDeleteLog: (id: string) => void;
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

function truncate(str: string, maxLength: number): string {
  if (str.length <= maxLength) return str;
  return str.substring(0, maxLength) + "...";
}

export default function LogTable({
  logs,
  selectedLog,
  onSelectLog,
  onDeleteLog,
}: LogTableProps) {
  return (
    <div className="flex-1 overflow-hidden">
      <Virtuoso
        style={{ height: "100%" }}
        data={logs}
        itemContent={(_index, log) => (
          <div
            key={log.id}
            className={`flex items-center gap-3 px-4 py-3 border-b border-gray-200 hover:bg-gray-50 cursor-pointer ${
              selectedLog?.id === log.id ? "bg-blue-50" : ""
            }`}
            onClick={() => onSelectLog(log)}
          >
            {/* Level Badge */}
            <span
              className={`px-2 py-1 rounded text-xs font-medium uppercase w-16 text-center ${
                levelColors[log.level]
              }`}
            >
              {log.level}
            </span>

            {/* Timestamp */}
            <span className="text-xs text-gray-500 w-40">
              {formatTimestamp(log.timestamp)}
            </span>

            {/* Message */}
            <span className="flex-1 text-sm text-gray-800 truncate">
              {truncate(log.message, 100)}
            </span>

            {/* Source Location */}
            <span className="text-xs text-gray-500 w-48 truncate">
              {log.source.file}:{log.source.line}
            </span>

            {/* Delete Button */}
            <button
              onClick={(e) => {
                e.stopPropagation();
                if (confirm("Are you sure you want to delete this log entry?")) {
                  onDeleteLog(log.id);
                }
              }}
              className="px-2 py-1 text-xs text-red-600 hover:bg-red-50 rounded"
            >
              Delete
            </button>
          </div>
        )}
      />
    </div>
  );
}
