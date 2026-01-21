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

// アンインストール対象
export type UninstallTarget = 'mcv' | 'installer' | 'both'

// 画面タイプ（アンインストール画面を含む）
export type ScreenType =
  | 'installer-setup'
  | 'welcome'
  | 'options'
  | 'ready'
  | 'installing'
  | 'complete'
  | 'uninstall-options'
  | 'uninstalling'
  | 'uninstall-complete'

// モード
export type Mode = 'install' | 'uninstall'
