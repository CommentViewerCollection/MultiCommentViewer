import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'

// 画面タイプ
type ScreenType =
  | 'splash'
  | 'installer-update'
  | 'install-type'
  | 'plugin-select'
  | 'update-check'
  | 'download'
  | 'complete'

// インストーラ更新情報
interface InstallerUpdateInfo {
  version: string
  required: boolean
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
}

// mcv更新情報
interface McvUpdateInfo {
  version: string
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
  min_installer_version: string
}

// プラグイン情報
interface PluginInfo {
  id: string
  name: string
  description: string
  version: string
  download_url: string
  sha256: string
  file_size: number
  author: string
  license: string
  released_at: string
  min_mcv_version: string
}

function App() {
  const [screen, setScreen] = useState<ScreenType>('splash')
  const [installerUpdate, setInstallerUpdate] = useState<InstallerUpdateInfo | null>(null)
  const [existingVersion, setExistingVersion] = useState<string | null>(null)
  const [mcvUpdate, setMcvUpdate] = useState<McvUpdateInfo | null>(null)
  const [availablePlugins, setAvailablePlugins] = useState<PluginInfo[]>([])
  const [selectedPlugins, setSelectedPlugins] = useState<Set<string>>(new Set())

  // スプラッシュ画面の初期化
  useEffect(() => {
    if (screen === 'splash') {
      setTimeout(async () => {
        // インストーラ自身の更新確認
        await checkInstallerUpdate()
      }, 1000)
    }
  }, [screen])

  const checkInstallerUpdate = async () => {
    try {
      const update = await invoke<InstallerUpdateInfo | null>('check_installer_update')
      if (update) {
        setInstallerUpdate(update)
        setScreen('installer-update')
      } else {
        // インストーラ更新なし、次へ
        await checkExistingInstallation()
      }
    } catch (error) {
      console.error('Failed to check installer update:', error)
      alert(`インストーラ更新確認失敗: ${error}`)
    }
  }

  const checkExistingInstallation = async () => {
    try {
      const version = await invoke<string | null>('check_existing_installation')
      setExistingVersion(version)
      setScreen('install-type')
    } catch (error) {
      console.error('Failed to check existing installation:', error)
      alert(`インストール確認失敗: ${error}`)
    }
  }

  const handleInstallerUpdate = async () => {
    // TODO: インストーラの更新処理
    alert('インストーラ更新機能は未実装です')
  }

  const handleNewInstall = async () => {
    // プラグイン一覧を取得
    try {
      const plugins = await invoke<PluginInfo[]>('list_plugins')
      setAvailablePlugins(plugins)
      setScreen('plugin-select')
    } catch (error) {
      console.error('Failed to list plugins:', error)
      alert(`プラグイン一覧取得失敗: ${error}`)
    }
  }

  const handleUpdate = async () => {
    if (!existingVersion) return

    // mcv本体とプラグインの更新確認
    try {
      const mcvUpdate = await invoke<McvUpdateInfo | null>('check_mcv_update', {
        currentVersion: existingVersion,
      })
      setMcvUpdate(mcvUpdate)

      const plugins = await invoke<PluginInfo[]>('list_plugins')
      setAvailablePlugins(plugins)

      setScreen('update-check')
    } catch (error) {
      console.error('Failed to check updates:', error)
      alert(`更新確認失敗: ${error}`)
    }
  }

  const handlePluginSelection = (pluginId: string, selected: boolean) => {
    setSelectedPlugins((prev) => {
      const newSet = new Set(prev)
      if (selected) {
        newSet.add(pluginId)
      } else {
        newSet.delete(pluginId)
      }
      return newSet
    })
  }

  const handleStartInstall = () => {
    setScreen('download')
    // TODO: ダウンロード・インストール処理
  }

  return (
    <div className="min-h-screen bg-gray-900 text-white flex items-center justify-center">
      {screen === 'splash' && (
        <div className="text-center">
          <h1 className="text-4xl font-bold mb-4">MultiCommentViewer</h1>
          <p className="text-xl text-gray-400">Installer</p>
          <div className="mt-8">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
            <p className="mt-4 text-gray-400">初期化中...</p>
          </div>
        </div>
      )}

      {screen === 'installer-update' && installerUpdate && (
        <div className="max-w-2xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700">
            <h2 className="text-3xl font-bold mb-6">インストーラの更新</h2>
            <div className="space-y-4 mb-6">
              <p className="text-gray-300">
                新しいバージョンのインストーラが利用可能です。
              </p>
              <div className="bg-gray-700 p-4 rounded">
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <span className="text-gray-400">バージョン:</span>
                    <span className="ml-2 font-semibold">{installerUpdate.version}</span>
                  </div>
                  <div>
                    <span className="text-gray-400">リリース日:</span>
                    <span className="ml-2">
                      {new Date(installerUpdate.released_at).toLocaleDateString('ja-JP')}
                    </span>
                  </div>
                </div>
                <div className="mt-4">
                  <span className="text-gray-400 block mb-2">リリースノート:</span>
                  <div className="bg-gray-800 p-3 rounded text-sm whitespace-pre-wrap">
                    {installerUpdate.release_notes}
                  </div>
                </div>
              </div>
              <div className="bg-yellow-900 border border-yellow-700 p-4 rounded">
                <p className="font-semibold text-yellow-200">
                  ⚠ この更新は必須です
                </p>
                <p className="text-yellow-300 text-sm mt-2">
                  更新しないとインストールを続行できません。
                </p>
              </div>
            </div>
            <button
              onClick={handleInstallerUpdate}
              className="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold text-lg transition-colors"
            >
              今すぐ更新
            </button>
          </div>
        </div>
      )}

      {screen === 'install-type' && (
        <div className="max-w-2xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700">
            <h2 className="text-3xl font-bold mb-6">インストール方法を選択</h2>
            {existingVersion ? (
              <div className="space-y-4">
                <div className="bg-gray-700 p-4 rounded">
                  <p className="text-gray-300">
                    既存のインストールが検出されました。
                  </p>
                  <p className="text-gray-400 mt-2">
                    バージョン: <span className="font-semibold">{existingVersion}</span>
                  </p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <button
                    onClick={handleUpdate}
                    className="px-6 py-4 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold transition-colors"
                  >
                    アップデート
                  </button>
                  <button
                    onClick={handleNewInstall}
                    className="px-6 py-4 bg-gray-700 hover:bg-gray-600 rounded-lg font-semibold transition-colors"
                  >
                    再インストール
                  </button>
                </div>
              </div>
            ) : (
              <div className="space-y-4">
                <p className="text-gray-300">
                  MultiCommentViewerへようこそ！
                </p>
                <button
                  onClick={handleNewInstall}
                  className="w-full px-6 py-4 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold text-lg transition-colors"
                >
                  新規インストール
                </button>
              </div>
            )}
          </div>
        </div>
      )}

      {screen === 'plugin-select' && (
        <div className="max-w-4xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700">
            <h2 className="text-3xl font-bold mb-6">プラグインを選択</h2>
            <p className="text-gray-400 mb-6">
              インストールするプラグインを選択してください。後から追加することもできます。
            </p>
            <div className="space-y-3 mb-8 max-h-96 overflow-y-auto">
              {availablePlugins.map((plugin) => (
                <label
                  key={plugin.id}
                  className="flex items-start p-4 bg-gray-700 rounded-lg cursor-pointer hover:bg-gray-600 transition-colors"
                >
                  <input
                    type="checkbox"
                    checked={selectedPlugins.has(plugin.id)}
                    onChange={(e) => handlePluginSelection(plugin.id, e.target.checked)}
                    className="mt-1 mr-4 w-5 h-5"
                  />
                  <div className="flex-1">
                    <div className="flex items-center justify-between mb-2">
                      <span className="font-semibold text-lg">{plugin.name}</span>
                      <span className="text-sm text-gray-400">v{plugin.version}</span>
                    </div>
                    <p className="text-sm text-gray-300">{plugin.description}</p>
                    <div className="flex gap-4 mt-2 text-xs text-gray-500">
                      <span>サイズ: {(plugin.file_size / 1024 / 1024).toFixed(2)} MB</span>
                      <span>作者: {plugin.author}</span>
                      <span>ライセンス: {plugin.license}</span>
                    </div>
                  </div>
                </label>
              ))}
            </div>
            <button
              onClick={handleStartInstall}
              className="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold text-lg transition-colors"
            >
              インストール開始 ({selectedPlugins.size}個のプラグインを選択)
            </button>
          </div>
        </div>
      )}

      {screen === 'update-check' && (
        <div className="max-w-4xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700">
            <h2 className="text-3xl font-bold mb-6">更新内容を確認</h2>
            <div className="space-y-6 mb-8">
              {mcvUpdate && (
                <div className="bg-gray-700 p-4 rounded-lg">
                  <h3 className="text-xl font-semibold mb-3">■ mcv本体</h3>
                  <div className="space-y-2">
                    <p>
                      <span className="text-gray-400">現在:</span>{' '}
                      <span className="font-semibold">{existingVersion}</span>
                      {' → '}
                      <span className="text-gray-400">最新:</span>{' '}
                      <span className="font-semibold text-green-400">{mcvUpdate.version}</span>
                    </p>
                    <div>
                      <span className="text-gray-400 block mb-1">リリースノート:</span>
                      <div className="bg-gray-800 p-3 rounded text-sm whitespace-pre-wrap">
                        {mcvUpdate.release_notes}
                      </div>
                    </div>
                  </div>
                </div>
              )}
              <div className="bg-gray-700 p-4 rounded-lg">
                <h3 className="text-xl font-semibold mb-3">■ プラグイン</h3>
                <p className="text-gray-400 mb-4">追加・更新するプラグインを選択してください。</p>
                <div className="space-y-2">
                  {availablePlugins.map((plugin) => (
                    <label
                      key={plugin.id}
                      className="flex items-center p-3 bg-gray-800 rounded cursor-pointer hover:bg-gray-750 transition-colors"
                    >
                      <input
                        type="checkbox"
                        checked={selectedPlugins.has(plugin.id)}
                        onChange={(e) => handlePluginSelection(plugin.id, e.target.checked)}
                        className="mr-3 w-4 h-4"
                      />
                      <div className="flex-1">
                        <span className="font-semibold">{plugin.name}</span>
                        <span className="ml-2 text-sm text-gray-400">v{plugin.version}</span>
                      </div>
                    </label>
                  ))}
                </div>
              </div>
            </div>
            <button
              onClick={handleStartInstall}
              className="w-full px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold text-lg transition-colors"
            >
              アップデート開始
            </button>
          </div>
        </div>
      )}

      {screen === 'download' && (
        <div className="max-w-2xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700">
            <h2 className="text-3xl font-bold mb-6">ダウンロード中...</h2>
            <div className="space-y-4">
              <div className="bg-gray-700 p-4 rounded-lg">
                <div className="flex items-center justify-between mb-2">
                  <span>全体進捗</span>
                  <span className="font-semibold">60%</span>
                </div>
                <div className="w-full bg-gray-600 rounded-full h-4">
                  <div className="bg-blue-600 h-4 rounded-full" style={{ width: '60%' }}></div>
                </div>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2">
                  <span className="text-green-400">✓</span>
                  <span>mcv本体 - 完了</span>
                </div>
                <div className="flex items-center gap-2">
                  <span className="text-blue-400">→</span>
                  <span>プラグイン - ダウンロード中 40%</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      )}

      {screen === 'complete' && (
        <div className="max-w-2xl w-full mx-4">
          <div className="bg-gray-800 rounded-lg p-8 border border-gray-700 text-center">
            <div className="text-6xl mb-6">✓</div>
            <h2 className="text-3xl font-bold mb-4">インストールが完了しました</h2>
            <p className="text-gray-400 mb-8">
              MultiCommentViewerをお楽しみください！
            </p>
            <div className="flex gap-4">
              <button
                onClick={() => {
                  // TODO: mcvを起動
                }}
                className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold transition-colors"
              >
                mcvを起動
              </button>
              <button
                onClick={() => {
                  // TODO: 閉じる
                }}
                className="px-6 py-3 bg-gray-700 hover:bg-gray-600 rounded-lg font-semibold transition-colors"
              >
                閉じる
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default App
