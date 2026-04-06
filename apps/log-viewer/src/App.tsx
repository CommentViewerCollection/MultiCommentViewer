import { useState } from "react";
import FilterBar from "./components/FilterBar";
import LogTable from "./components/LogTable";
import LogDetail from "./components/LogDetail";
import { useLogs, deleteLogs, exportLogs, fetchLogsJson, type DataSource } from "./hooks/useLogs";
import type { LogEntry, LogQueryFilters } from "./types/log";

function App() {
  const [dataSource, setDataSource] = useState<DataSource>("local");
  const [apiUrl, setApiUrl] = useState("https://int-main.net");
  const [filters, setFilters] = useState<LogQueryFilters>({});
  const [selectedLog, setSelectedLog] = useState<LogEntry | null>(null);
  const [limit] = useState(100);
  const [offset, setOffset] = useState(0);
  const [refreshTrigger, setRefreshTrigger] = useState(0);
  const [exportLoading, setExportLoading] = useState(false);
  const [copyLoading, setCopyLoading] = useState(false);

  const { logs, total, loading, error } = useLogs(
    dataSource,
    apiUrl,
    filters,
    limit,
    offset,
    refreshTrigger
  );

  const handleRefresh = () => {
    setRefreshTrigger((prev) => prev + 1);
  };

  const handleDelete = async (ids: string[]) => {
    try {
      const deleted = await deleteLogs(dataSource, apiUrl, ids);
      console.log(`Deleted ${deleted} logs`);
      handleRefresh();
      if (selectedLog && ids.includes(selectedLog.id)) {
        setSelectedLog(null);
      }
    } catch (err) {
      console.error("Failed to delete logs:", err);
      alert(`Failed to delete logs: ${err}`);
    }
  };

  const handleExport = async () => {
    setExportLoading(true);
    try {
      await exportLogs(dataSource, apiUrl, filters);
    } catch (err) {
      alert(`Failed to export logs: ${err}`);
    } finally {
      setExportLoading(false);
    }
  };

  const handleCopyToClipboard = async () => {
    setCopyLoading(true);
    try {
      const json = await fetchLogsJson(dataSource, apiUrl, filters);
      await navigator.clipboard.writeText(json);
    } catch (err) {
      alert(`Failed to copy logs: ${err}`);
    } finally {
      setCopyLoading(false);
    }
  };

  const handleLoadMore = () => {
    setOffset((prev) => prev + limit);
  };

  const handleLoadPrev = () => {
    setOffset((prev) => Math.max(0, prev - limit));
  };

  const handleReset = () => {
    setOffset(0);
    handleRefresh();
  };

  return (
    <div className="flex flex-col h-screen bg-gray-100">
      {/* Header */}
      <div className="bg-white shadow-sm border-b border-gray-200 p-4">
        <div className="flex items-center justify-between">
          <h1 className="text-2xl font-bold text-gray-800">MCV Log Viewer</h1>
          <div className="flex items-center gap-4">
            {/* Data Source Selector */}
            <div className="flex items-center gap-2">
              <label className="text-sm font-medium text-gray-700">
                Data Source:
              </label>
              <select
                value={dataSource}
                onChange={(e) => {
                  setDataSource(e.target.value as DataSource);
                  setOffset(0);
                }}
                className="px-3 py-1 border border-gray-300 rounded-md text-sm"
              >
                <option value="local">Local DB</option>
                <option value="server">Server API</option>
              </select>
            </div>

            {/* API URL Input (only for server mode) */}
            {dataSource === "server" && (
              <div className="flex items-center gap-2">
                <label className="text-sm font-medium text-gray-700">
                  API URL:
                </label>
                <input
                  type="text"
                  value={apiUrl}
                  onChange={(e) => setApiUrl(e.target.value)}
                  className="px-3 py-1 border border-gray-300 rounded-md text-sm w-64"
                  placeholder="https://int-main.net"
                />
              </div>
            )}

            {/* Refresh Button */}
            <button
              onClick={handleReset}
              className="px-4 py-1 bg-blue-500 text-white rounded-md hover:bg-blue-600 text-sm"
            >
              Refresh
            </button>
          </div>
        </div>
      </div>

      {/* Filter Bar */}
      <FilterBar
        filters={filters}
        onFiltersChange={(newFilters) => {
          setFilters(newFilters);
          setOffset(0);
        }}
        onExport={handleExport}
        exportLoading={exportLoading}
        onCopyToClipboard={handleCopyToClipboard}
        copyLoading={copyLoading}
      />

      {/* Main Content */}
      <div className="flex flex-1 overflow-hidden">
        {/* Left: Log Table (2/3) */}
        <div className="w-2/3 border-r border-gray-200 bg-white flex flex-col">
          {error && (
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 m-4 rounded">
              <strong>Error:</strong> {error}
            </div>
          )}

          {loading && (
            <div className="text-center py-8 text-gray-500">Loading...</div>
          )}

          {!loading && !error && (
            <>
              <LogTable
                logs={logs}
                selectedLog={selectedLog}
                onSelectLog={setSelectedLog}
                onDeleteLog={(id) => handleDelete([id])}
              />

              {/* Pagination Info */}
              <div className="border-t border-gray-200 px-4 py-3 bg-gray-50 flex items-center justify-between">
                <div className="text-sm text-gray-700">
                  Showing {offset + 1} - {Math.min(offset + limit, total)} of{" "}
                  {total} logs
                </div>
                <div className="flex gap-2">
                  <button
                    onClick={handleLoadPrev}
                    disabled={offset === 0}
                    className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 text-sm disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    Previous
                  </button>
                  <button
                    onClick={handleLoadMore}
                    disabled={offset + limit >= total}
                    className="px-4 py-2 bg-gray-200 text-gray-700 rounded-md hover:bg-gray-300 text-sm disabled:opacity-40 disabled:cursor-not-allowed"
                  >
                    Load More
                  </button>
                </div>
              </div>
            </>
          )}
        </div>

        {/* Right: Log Detail (1/3) */}
        <div className="w-1/3 bg-white overflow-auto">
          {selectedLog ? (
            <LogDetail
              log={selectedLog}
              onDelete={() => handleDelete([selectedLog.id])}
            />
          ) : (
            <div className="flex items-center justify-center h-full text-gray-400">
              Select a log entry to view details
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
