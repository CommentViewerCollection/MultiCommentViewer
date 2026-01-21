import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'
import type { PluginListItem } from '../types'

interface ReadyScreenProps {
  isNewInstall: boolean
  existingVersion: string | null
  selectedPlugins: Set<string>
  availablePlugins: PluginListItem[]
  createDesktopShortcut: boolean
  createStartMenuShortcut: boolean
}

export function ReadyScreen({
  isNewInstall,
  existingVersion,
  selectedPlugins,
  availablePlugins,
  createDesktopShortcut,
  createStartMenuShortcut,
}: ReadyScreenProps) {
  const [installPath, setInstallPath] = useState('')

  useEffect(() => {
    const getInstallPath = async () => {
      try {
        const localAppData = await invoke<string>('get_local_app_data')
        setInstallPath(`${localAppData}\\Programs\\MultiCommentViewer`)
      } catch (e) {
        console.error('Failed to get install path:', e)
      }
    }
    getInstallPath()
  }, [])

  // 選択されたプラグインの情報を取得
  const selectedPluginList = availablePlugins.filter((p) => selectedPlugins.has(p.id))

  // 合計サイズを計算（仮の値）
  const estimatedSize = 40 + selectedPluginList.length * 0.5 // MB

  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-4">インストール準備完了</h2>
      <p className="text-gray-300 mb-6">
        インストールを開始する準備ができました。
      </p>

      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700 space-y-4">
        <h3 className="text-xl font-semibold mb-4">■ インストールサマリー</h3>

        {/* インストールタイプ */}
        <div>
          <span className="text-gray-400">インストールタイプ:</span>{' '}
          <span className="font-semibold">
            {isNewInstall ? '新規インストール' : '更新'}
          </span>
        </div>

        {/* インストール先 */}
        <div>
          <span className="text-gray-400">インストール先:</span>
          <div className="mt-1 bg-gray-700 p-2 rounded font-mono text-sm">
            {installPath || '読み込み中...'}
          </div>
        </div>

        {/* インストールコンポーネント */}
        <div>
          <span className="text-gray-400 block mb-2">インストールするコンポーネント:</span>
          <div className="bg-gray-700 p-3 rounded space-y-2">
            {/* mcv本体 */}
            <div className="flex items-center gap-2">
              <span className="text-blue-400">•</span>
              <span>
                mcv本体 {isNewInstall ? '(v0.1.0)' : `(v${existingVersion} → v0.1.0)`} - 約40 MB
              </span>
            </div>

            {/* プラグイン */}
            {selectedPluginList.length > 0 ? (
              selectedPluginList.map((plugin) => {
                const version =
                  plugin.channels.stable ||
                  plugin.channels.beta ||
                  plugin.channels.alpha ||
                  'unknown'
                return (
                  <div key={plugin.id} className="flex items-center gap-2">
                    <span className="text-green-400">•</span>
                    <span>
                      {plugin.name} (v{version}) - 約0.5 MB
                    </span>
                  </div>
                )
              })
            ) : (
              <div className="flex items-center gap-2 text-gray-500">
                <span>•</span>
                <span>プラグインは選択されていません</span>
              </div>
            )}
          </div>
        </div>

        {/* ショートカット */}
        <div>
          <span className="text-gray-400 block mb-2">ショートカット:</span>
          <div className="bg-gray-700 p-3 rounded space-y-1">
            <div className="flex items-center gap-2">
              <span className={createDesktopShortcut ? 'text-green-400' : 'text-gray-500'}>
                {createDesktopShortcut ? '✓' : '○'}
              </span>
              <span className={!createDesktopShortcut ? 'text-gray-500' : ''}>
                デスクトップショートカット: {createDesktopShortcut ? 'あり' : 'なし'}
              </span>
            </div>
            <div className="flex items-center gap-2">
              <span className={createStartMenuShortcut ? 'text-green-400' : 'text-gray-500'}>
                {createStartMenuShortcut ? '✓' : '○'}
              </span>
              <span className={!createStartMenuShortcut ? 'text-gray-500' : ''}>
                スタートメニュー: {createStartMenuShortcut ? 'あり' : 'なし'}
              </span>
            </div>
          </div>
        </div>

        {/* ディスク容量 */}
        <div>
          <span className="text-gray-400 block mb-2">ディスク容量:</span>
          <div className="bg-gray-700 p-3 rounded space-y-1">
            <div>
              <span className="text-gray-400">必要な容量:</span>{' '}
              <span className="font-semibold">約 {estimatedSize.toFixed(1)} MB</span>
            </div>
            <div>
              <span className="text-gray-400">利用可能な容量:</span>{' '}
              <span className="text-green-400 font-semibold">十分（確認機能は未実装）</span>
            </div>
          </div>
        </div>
      </div>

      <div className="mt-6 bg-blue-900 border border-blue-700 p-4 rounded">
        <p className="text-blue-200">
          「インストール」ボタンをクリックしてインストールを開始してください。
        </p>
      </div>
    </div>
  )
}
