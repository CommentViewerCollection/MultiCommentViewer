import { useState } from 'react'

interface PluginChannels {
  stable: string | null
  beta: string | null
  alpha: string | null
}

interface PluginListItem {
  id: string
  name: string
  description: string
  channels: PluginChannels
}

interface CompletionScreenProps {
  selectedPlugins: Set<string>
  availablePlugins: PluginListItem[]
  createDesktopShortcut: boolean
  createStartMenuShortcut: boolean
}

export function CompletionScreen({
  selectedPlugins,
  availablePlugins,
  createDesktopShortcut,
  createStartMenuShortcut,
}: CompletionScreenProps) {
  const [launchNow, setLaunchNow] = useState(true)

  // 選択されたプラグインの情報を取得
  const selectedPluginList = availablePlugins.filter((p) => selectedPlugins.has(p.id))

  return (
    <div className="p-8">
      {/* 完了アイコンとタイトル */}
      <div className="text-center mb-8">
        <div className="text-6xl text-green-400 mb-4">✓</div>
        <h2 className="text-3xl font-bold mb-2">セットアップの完了</h2>
        <p className="text-gray-300">
          MultiCommentViewerのインストールが完了しました。
        </p>
      </div>

      {/* インストール済みコンポーネント */}
      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700 mb-6">
        <h3 className="text-xl font-semibold mb-4">■ インストール済みコンポーネント</h3>
        <div className="space-y-2">
          {/* mcv本体 */}
          <div className="flex items-center gap-2">
            <span className="text-green-400">✓</span>
            <span>mcv本体 (v0.1.0)</span>
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
                  <span className="text-green-400">✓</span>
                  <span>
                    {plugin.name} (v{version})
                  </span>
                </div>
              )
            })
          ) : null}
        </div>
      </div>

      {/* 作成されたショートカット */}
      {(createDesktopShortcut || createStartMenuShortcut) && (
        <div className="bg-gray-800 p-6 rounded-lg border border-gray-700 mb-6">
          <h3 className="text-xl font-semibold mb-4">■ 作成されたショートカット</h3>
          <div className="space-y-2">
            {createDesktopShortcut && (
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>デスクトップショートカット</span>
              </div>
            )}
            {createStartMenuShortcut && (
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>スタートメニュー</span>
              </div>
            )}
          </div>
        </div>
      )}

      {/* mcv起動オプション */}
      <div className="bg-blue-900 border border-blue-700 p-4 rounded mb-6">
        <label className="flex items-center gap-3 cursor-pointer">
          <input
            type="checkbox"
            checked={launchNow}
            onChange={(e) => setLaunchNow(e.target.checked)}
            className="w-5 h-5"
          />
          <span className="text-blue-200">MultiCommentViewerを今すぐ起動する</span>
        </label>
      </div>

      <div className="text-gray-400 text-sm">
        <p>「完了」ボタンをクリックしてセットアップを終了してください。</p>
      </div>
    </div>
  )
}
