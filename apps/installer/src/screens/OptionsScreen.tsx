import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'
import type { PluginListItem, McvUpdateInfo } from '../types'

interface OptionsScreenProps {
  isNewInstall: boolean
  existingVersion: string | null
  mcvUpdate: McvUpdateInfo | null
  availablePlugins: PluginListItem[]
  selectedPlugins: Set<string>
  onPluginSelectionChange: (pluginId: string, selected: boolean) => void
  createDesktopShortcut: boolean
  onDesktopShortcutChange: (value: boolean) => void
  createStartMenuShortcut: boolean
  onStartMenuShortcutChange: (value: boolean) => void
}

export function OptionsScreen({
  isNewInstall,
  existingVersion,
  mcvUpdate,
  availablePlugins,
  selectedPlugins,
  onPluginSelectionChange,
  createDesktopShortcut,
  onDesktopShortcutChange,
  createStartMenuShortcut,
  onStartMenuShortcutChange,
}: OptionsScreenProps) {
  const [installPath, setInstallPath] = useState('')

  useEffect(() => {
    const getInstallPath = async () => {
      try {
        const localAppData = await invoke<string>('get_local_app_data')
        setInstallPath(`${localAppData}\\MultiCommentViewer`)
      } catch (e) {
        console.error('Failed to get install path:', e)
      }
    }
    getInstallPath()
  }, [])

  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-6">
        {isNewInstall ? 'インストールオプション' : '更新オプション'}
      </h2>

      {/* 更新情報（更新の場合のみ） */}
      {!isNewInstall && mcvUpdate && (
        <div className="mb-6">
          <h3 className="text-xl font-semibold mb-3">■ 更新サマリー</h3>
          <div className="bg-gray-700 p-4 rounded-lg">
            <p className="text-gray-300">
              <span className="text-gray-400">mcv本体:</span>{' '}
              <span className="font-semibold">{existingVersion}</span>
              {' → '}
              <span className="text-green-400 font-semibold">{mcvUpdate.version}</span>
            </p>
            <p className="text-sm text-gray-400 mt-2">
              チャンネル: {mcvUpdate.channel}
            </p>
            <p className="text-sm text-gray-400">
              アップロード日時: {new Date(mcvUpdate.uploadedAt).toLocaleString('ja-JP')}
            </p>
          </div>
        </div>
      )}

      {/* インストール先 */}
      <div className="mb-6">
        <h3 className="text-xl font-semibold mb-3">■ インストール先</h3>
        <div className="bg-gray-700 p-3 rounded font-mono text-sm text-gray-300">
          {installPath || '読み込み中...'}
        </div>
        <p className="text-xs text-gray-500 mt-1">（固定位置、情報表示のみ）</p>
      </div>

      {/* ショートカット */}
      <div className="mb-6">
        <h3 className="text-xl font-semibold mb-3">■ ショートカット</h3>
        <div className="space-y-2">
          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              checked={createDesktopShortcut}
              onChange={(e) => onDesktopShortcutChange(e.target.checked)}
              className="w-5 h-5"
            />
            <span className="text-gray-300">デスクトップショートカットを作成</span>
          </label>
          <label className="flex items-center gap-3 cursor-pointer">
            <input
              type="checkbox"
              checked={createStartMenuShortcut}
              onChange={(e) => onStartMenuShortcutChange(e.target.checked)}
              className="w-5 h-5"
            />
            <span className="text-gray-300">スタートメニューに登録</span>
          </label>
        </div>
      </div>

      {/* プラグイン選択 */}
      <div className="mb-6">
        <h3 className="text-xl font-semibold mb-3">
          {isNewInstall ? '■ インストールするプラグインを選択' : '■ プラグイン'}
        </h3>
        <p className="text-gray-400 mb-4 text-sm">
          {isNewInstall
            ? 'インストールするプラグインを選択してください。後から追加することもできます。'
            : '追加・更新するプラグインを選択してください。'}
        </p>
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {availablePlugins.length === 0 ? (
            <div className="text-gray-500 text-center py-8">
              プラグインを読み込み中...
            </div>
          ) : (
            availablePlugins.map((plugin) => (
              <label
                key={plugin.id}
                className="flex items-start p-4 bg-gray-700 rounded-lg cursor-pointer hover:bg-gray-600 transition-colors"
              >
                <input
                  type="checkbox"
                  checked={selectedPlugins.has(plugin.id)}
                  onChange={(e) => onPluginSelectionChange(plugin.id, e.target.checked)}
                  className="mt-1 mr-4 w-5 h-5"
                />
                <div className="flex-1">
                  <div className="flex items-center justify-between mb-2">
                    <span className="font-semibold text-lg">{plugin.name}</span>
                    <span className="text-sm text-gray-400">
                      {plugin.channels.stable
                        ? `v${plugin.channels.stable}`
                        : plugin.channels.beta
                          ? `v${plugin.channels.beta} (beta)`
                          : `v${plugin.channels.alpha} (alpha)`}
                    </span>
                  </div>
                  <p className="text-sm text-gray-300">{plugin.description}</p>
                </div>
              </label>
            ))
          )}
        </div>
        <p className="text-sm text-gray-500 mt-3">
          選択中: {selectedPlugins.size}個のプラグイン
        </p>
      </div>
    </div>
  )
}
