import type { StackFrame } from "../types/log";

interface StackTraceProps {
  frames: StackFrame[];
}

export default function StackTrace({ frames }: StackTraceProps) {
  return (
    <div className="bg-gray-50 p-3 rounded border border-gray-200 max-h-96 overflow-auto">
      {frames.map((frame, index) => (
        <div
          key={index}
          className="mb-2 pb-2 border-b border-gray-300 last:border-b-0 last:mb-0 last:pb-0"
        >
          {/* Symbol Name */}
          {frame.symbol && (
            <div className="font-mono text-sm font-semibold text-gray-900">
              {frame.symbol}
            </div>
          )}

          {/* File Location */}
          {frame.filename && (
            <div className="font-mono text-xs text-blue-600">
              {frame.filename}
              {frame.lineno && `:${frame.lineno}`}
            </div>
          )}

          {/* Address */}
          <div className="font-mono text-xs text-gray-500">{frame.addr}</div>
        </div>
      ))}
    </div>
  );
}
