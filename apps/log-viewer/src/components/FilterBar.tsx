import { useState } from "react";
import type { LogQueryFilters } from "../types/log";

interface FilterBarProps {
  filters: LogQueryFilters;
  onFiltersChange: (filters: LogQueryFilters) => void;
  onExport?: () => void;
  exportLoading?: boolean;
}

export default function FilterBar({ filters, onFiltersChange, onExport, exportLoading }: FilterBarProps) {
  const [level, setLevel] = useState(filters.level || "");
  const [search, setSearch] = useState(filters.search || "");
  const [fromDate, setFromDate] = useState("");
  const [toDate, setToDate] = useState("");

  const handleApply = () => {
    const newFilters: LogQueryFilters = {
      level: level || undefined,
      search: search || undefined,
      from: fromDate ? new Date(fromDate).getTime() : undefined,
      to: toDate ? new Date(toDate).getTime() : undefined,
    };
    onFiltersChange(newFilters);
  };

  const handleReset = () => {
    setLevel("");
    setSearch("");
    setFromDate("");
    setToDate("");
    onFiltersChange({});
  };

  return (
    <div className="bg-white border-b border-gray-200 p-4">
      <div className="flex items-end gap-4">
        {/* Level Filter */}
        <div className="flex flex-col">
          <label className="text-xs font-medium text-gray-700 mb-1">
            Log Level
          </label>
          <select
            value={level}
            onChange={(e) => setLevel(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          >
            <option value="">All</option>
            <option value="trace">Trace</option>
            <option value="debug">Debug</option>
            <option value="info">Info</option>
            <option value="warn">Warn</option>
            <option value="error">Error</option>
          </select>
        </div>

        {/* Search Filter */}
        <div className="flex flex-col flex-1">
          <label className="text-xs font-medium text-gray-700 mb-1">
            Search (message, file, module)
          </label>
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
            placeholder="Enter search keyword..."
          />
        </div>

        {/* From Date */}
        <div className="flex flex-col">
          <label className="text-xs font-medium text-gray-700 mb-1">
            From Date
          </label>
          <input
            type="datetime-local"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          />
        </div>

        {/* To Date */}
        <div className="flex flex-col">
          <label className="text-xs font-medium text-gray-700 mb-1">
            To Date
          </label>
          <input
            type="datetime-local"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="px-3 py-2 border border-gray-300 rounded-md text-sm"
          />
        </div>

        {/* Action Buttons */}
        <div className="flex gap-2">
          <button
            onClick={handleApply}
            className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 text-sm"
          >
            Apply
          </button>
          <button
            onClick={handleReset}
            className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 text-sm"
          >
            Reset
          </button>
          {onExport && (
            <button
              onClick={onExport}
              disabled={exportLoading}
              className={`px-4 py-2 rounded-md text-sm ${
                exportLoading
                  ? "bg-gray-300 text-gray-500 cursor-not-allowed"
                  : "bg-green-500 text-white hover:bg-green-600"
              }`}
            >
              {exportLoading ? "Exporting..." : "Export JSON"}
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
