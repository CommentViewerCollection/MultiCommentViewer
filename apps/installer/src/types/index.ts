// インストーラー更新情報
export interface InstallerUpdateInfo {
  version: string
  required: boolean
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
}

// mcv更新情報
export interface McvUpdateInfo {
  version: string
  channel: string
  fileName: string
  fileSize?: number
  sha256: string
  uploadedAt: string
}

// プラグインのチャンネル情報
export interface PluginChannels {
  stable: string | null
  beta: string | null
  alpha: string | null
}

// プラグイン一覧の各アイテム
export interface PluginListItem {
  id: string
  name: string
  description: string
  channels: PluginChannels
}
