// MCV Message Types
export interface McvMessage {
  type: string;
  src: MessageSource;
  dst: MessageDestination;
  request_id?: string;
  timestamp: number;
  payload: any;
}

export type MessageSource =
  | { Core: null }
  | { Plugin: { plugin_id: string } };

export type MessageDestination =
  | { Core: null }
  | { Plugin: { plugin_id: string } };

// Plugin Info
export interface PluginInfo {
  plugin_id: string;
  name: string;
  roles: string[];
  api_version: string;
}

// Connection Info
export interface ConnectionInfo {
  connection_id: string;
  plugin_id: string;
  name: string;
  status: "disconnected" | "connecting" | "connected";
  site_name?: string;
}

// Site Info
export interface SiteInfo {
  site_id: string;
  site_name: string;
  display_name: string;
  plugin_id: string;
}

// Browser Info
export interface BrowserInfo {
  browser_id: string;
  browser_name: string;
  display_name: string;
  plugin_id: string;
}

// App State
export interface AppState {
  plugins: Map<string, PluginInfo>;
  connections: Map<string, ConnectionInfo>;
  sites: Map<string, SiteInfo>;
  browsers: Map<string, BrowserInfo>;
}
