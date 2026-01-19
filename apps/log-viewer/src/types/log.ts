export interface SourceLocation {
  file: string;
  line: number;
  column?: number;
  module_path: string;
}

export interface StackFrame {
  symbol?: string;
  filename?: string;
  lineno?: number;
  addr: string;
}

export interface SystemInfo {
  mcv_version: string;
  platform: string;
  arch: string;
  build_profile: string;
}

export interface LogEntry {
  id: string;
  level: "trace" | "debug" | "info" | "warn" | "error";
  timestamp: number;
  message: string;
  source: SourceLocation;
  stacktrace?: StackFrame[];
  context?: Record<string, any>;
  system_info: SystemInfo;
}

export interface LogQueryFilters {
  level?: string;
  from?: number;
  to?: number;
  search?: string;
}

export interface LogQueryResult {
  logs: LogEntry[];
  total: number;
}
