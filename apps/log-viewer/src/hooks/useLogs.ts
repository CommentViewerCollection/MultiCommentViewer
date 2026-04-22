import { useState, useEffect } from "react";
import { invoke } from "@tauri-apps/api/core";
import type { LogEntry, LogQueryFilters, LogQueryResult } from "../types/log";

export type DataSource = "local" | "server";

export function useLogs(
  dataSource: DataSource,
  apiUrl: string,
  filters: LogQueryFilters,
  limit: number,
  offset: number,
  refreshTrigger: number
) {
  const [logs, setLogs] = useState<LogEntry[]>([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchLogs = async () => {
      setLoading(true);
      setError(null);

      try {
        let result: LogQueryResult;

        if (dataSource === "local") {
          result = await invoke<LogQueryResult>("get_local_logs", {
            filters,
            limit,
            offset,
          });
        } else {
          result = await invoke<LogQueryResult>("get_server_logs", {
            apiUrl,
            filters,
            limit,
            offset,
          });
        }

        setLogs(result.logs);
        setTotal(result.total);
      } catch (err) {
        setError(err instanceof Error ? err.message : String(err));
        console.error("Failed to fetch logs:", err);
      } finally {
        setLoading(false);
      }
    };

    fetchLogs();
  }, [dataSource, apiUrl, filters, limit, offset, refreshTrigger]);

  return { logs, total, loading, error };
}

export async function deleteLogs(
  dataSource: DataSource,
  apiUrl: string,
  ids: string[]
): Promise<number> {
  if (dataSource === "local") {
    return await invoke<number>("delete_local_logs", { ids });
  } else {
    return await invoke<number>("delete_server_logs", { apiUrl, ids });
  }
}

export async function exportLogs(
  dataSource: DataSource,
  apiUrl: string,
  filters: LogQueryFilters
): Promise<string | null> {
  if (dataSource === "local") {
    return await invoke<string | null>("export_local_logs", { filters });
  } else {
    return await invoke<string | null>("export_server_logs", { apiUrl, filters });
  }
}

export async function fetchLogsJson(
  dataSource: DataSource,
  apiUrl: string,
  filters: LogQueryFilters
): Promise<string> {
  if (dataSource === "local") {
    return await invoke<string>("get_local_logs_json", { filters });
  } else {
    return await invoke<string>("get_server_logs_json", { apiUrl, filters });
  }
}
