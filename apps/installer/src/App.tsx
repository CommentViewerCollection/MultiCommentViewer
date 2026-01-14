import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { getCurrentWindow } from '@tauri-apps/api/window'
import { listen } from '@tauri-apps/api/event'

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
  channel: string
  fileName: string
  fileSize?: number
  sha256: string
  uploadedAt: string
}

// プラグインのチャンネル情報
interface PluginChannels {
  stable: string | null
  beta: string | null
  alpha: string | null
}

// プラグイン一覧の各アイテム
interface PluginListItem {
  id: string
  name: string
  description: string
  channels: PluginChannels
}

// ダウンロード進捗
interface DownloadProgress {
  mcv: { status: 'pending' | 'downloading' | 'completed' | 'error'; progress: number }
  plugins: { status: 'pending' | 'downloading' | 'completed' | 'error'; progress: number }
}

function App() {
  const [screen, setScreen] = useState<ScreenType>('splash')
  const [installerUpdate, setInstallerUpdate] = useState<InstallerUpdateInfo | null>(null)
  const [existingVersion, setExistingVersion] = useState<string | null>(null)
  const [mcvUpdate, setMcvUpdate] = useState<McvUpdateInfo | null>(null)
  const [availablePlugins, setAvailablePlugins] = useState<PluginListItem[]>([])
  const [selectedPlugins, setSelectedPlugins] = useState<Set<string>>(new Set())
  const [downloadProgress, setDownloadProgress] = useState<DownloadProgress>({
    mcv: { status: 'pending', progress: 0 },
    plugins: { status: 'pending', progress: 0 },
  })

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
      const plugins = await invoke<PluginListItem[]>('list_plugins')
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

      const plugins = await invoke<PluginListItem[]>('list_plugins')
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

  const installMcv = async () => {
    console.log('Starting mcv installation...')

    // 1. mcv更新情報を取得
    console.log('Fetching mcv update info...')
    const mcvInfo = await invoke<McvUpdateInfo>('check_mcv_update', {
      currentVersion: existingVersion || '0.0.0'
    })

    console.log('mcvInfo:', mcvInfo)

    if (!mcvInfo) {
      throw new Error('No mcv update available')
    }

    // 2. ダウンロードURLを構築
    console.log('Building download URL...')
    const downloadUrl = await invoke<string>('get_mcv_download_url', {
      version: mcvInfo.version,
      channel: mcvInfo.channel
    })

    console.log('Download URL:', downloadUrl)

    // 3. 一時ディレクトリにダウンロード
    const tempDir = await invoke<string>('get_temp_dir')
    const tempZipPath = `${tempDir}\\mcv_${mcvInfo.version}.zip`

    console.log('Downloading to:', tempZipPath)

    await invoke('download_file', {
      url: downloadUrl,
      dest: tempZipPath
    })

    console.log('Download completed')

    // 4. チェックサム検証
    const isValid = await invoke<boolean>('verify_checksum', {
      filePath: tempZipPath,
      expected: mcvInfo.sha256
    })

    if (!isValid) {
      throw new Error('Checksum verification failed')
    }

    // 5. インストール（ZIP展開）
    const localAppData = await invoke<string>('get_local_app_data')
    const installDir = `${localAppData}\\MultiCommentViewer`

    await invoke('install_mcv', {
      zipPath: tempZipPath,
      destDir: installDir
    })

    setDownloadProgress(prev => ({
      ...prev,
      mcv: { status: 'completed', progress: 100 }
    }))
  }

  const installPlugins = async () => {
    if (selectedPlugins.size === 0) {
      setDownloadProgress(prev => ({
        ...prev,
        plugins: { status: 'completed', progress: 100 }
      }))
      return
    }

    const tempDir = await invoke<string>('get_temp_dir')
    const localAppData = await invoke<string>('get_local_app_data')
    const pluginDir = `${localAppData}\\MultiCommentViewer\\plugins`

    let completedCount = 0
    const totalPlugins = selectedPlugins.size

    for (const pluginId of selectedPlugins) {
      const plugin = availablePlugins.find(p => p.id === pluginId)
      if (!plugin) continue

      // チャンネルとバージョンを取得
      const channel = plugin.channels.stable ? 'stable'
                    : plugin.channels.beta ? 'beta'
                    : 'alpha'
      const version = plugin.channels[channel]

      if (!version) continue

      // ダウンロードURLを構築
      const downloadUrl = await invoke<string>('get_plugin_download_url', {
        pluginId: pluginId,
        version: version,
        channel: channel
      })

      // ダウンロード
      const tempDllPath = `${tempDir}\\${pluginId}_${version}.dll`
      await invoke('download_file', {
        url: downloadUrl,
        dest: tempDllPath
      })

      // インストール（DLLコピー）
      // 注意: プラグインのチェックサム検証はスキップ（APIがsha256を返さないため）
      await invoke('install_plugin', {
        dllPath: tempDllPath,
        destDir: pluginDir
      })

      completedCount++
      const progress = Math.round((completedCount / totalPlugins) * 100)
      setDownloadProgress(prev => ({
        ...prev,
        plugins: { status: 'downloading', progress }
      }))
    }

    setDownloadProgress(prev => ({
      ...prev,
      plugins: { status: 'completed', progress: 100 }
    }))
  }

  const handleStartInstall = async () => {
    console.log('=== handleStartInstall called ===')

    setScreen('download')

    // 進捗状態をリセット
    setDownloadProgress({
      mcv: { status: 'pending', progress: 0 },
      plugins: { status: 'pending', progress: 0 },
    })

    console.log('Setting up progress listener...')

    // 進捗イベントリスナーを設定
    const unlisten = await listen('download-progress', (event: any) => {
      const { url, progress } = event.payload

      if (url.includes('/mcv/core/')) {
        setDownloadProgress(prev => ({
          ...prev,
          mcv: { status: 'downloading', progress: Math.round(progress) }
        }))
      } else if (url.includes('/plugins/')) {
        setDownloadProgress(prev => ({
          ...prev,
          plugins: { status: 'downloading', progress: Math.round(progress) }
        }))
      }
    })

    try {
      // mcv本体をインストール
      setDownloadProgress((prev) => ({
        ...prev,
        mcv: { status: 'downloading', progress: 0 },
      }))

      await installMcv()

      // プラグインをインストール（選択されている場合）
      if (selectedPlugins.size > 0) {
        setDownloadProgress((prev) => ({
          ...prev,
          plugins: { status: 'downloading', progress: 0 },
        }))

        await installPlugins()
      } else {
        // プラグインが選択されていない場合はスキップ
        setDownloadProgress((prev) => ({
          ...prev,
          plugins: { status: 'completed', progress: 100 },
        }))
      }

      // インストール完了
      setScreen('complete')
    } catch (error) {
      console.error('Installation failed:', error)
      alert(`インストール失敗: ${error}`)

      // エラー状態を設定
      setDownloadProgress((prev) => ({
        mcv: prev.mcv.status === 'completed' ? prev.mcv : { status: 'error', progress: prev.mcv.progress },
        plugins: prev.plugins.status === 'completed' ? prev.plugins : { status: 'error', progress: prev.plugins.progress },
      }))
    } finally {
      unlisten()
    }
  }

  const handleLaunchMcv = async () => {
    try {
      // mcvを起動
      await invoke('launch_mcv')

      // インストーラを閉じる
      await getCurrentWindow().close()
    } catch (error) {
      console.error('Failed to launch mcv:', error)
      alert(`mcvの起動に失敗しました: ${error}`)
    }
  }

  const handleClose = async () => {
    try {
      await getCurrentWindow().close()
    } catch (error) {
      console.error('Failed to close window:', error)
    }
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
                      <span className="text-sm text-gray-400">
                        {plugin.channels.stable ? `v${plugin.channels.stable}` : '(beta/alpha)'}
                      </span>
                    </div>
                    <p className="text-sm text-gray-300">{plugin.description}</p>
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
                    <p className="text-sm text-gray-400">
                      チャンネル: {mcvUpdate.channel}
                    </p>
                    <p className="text-sm text-gray-400">
                      アップロード日時: {new Date(mcvUpdate.uploadedAt).toLocaleString('ja-JP')}
                    </p>
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
                        <span className="ml-2 text-sm text-gray-400">
                          {plugin.channels.stable ? `v${plugin.channels.stable}` : '(beta/alpha)'}
                        </span>
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
                  <span className="font-semibold">
                    {Math.round((downloadProgress.mcv.progress + downloadProgress.plugins.progress) / 2)}%
                  </span>
                </div>
                <div className="w-full bg-gray-600 rounded-full h-4">
                  <div
                    className="bg-blue-600 h-4 rounded-full transition-all duration-300"
                    style={{
                      width: `${Math.round((downloadProgress.mcv.progress + downloadProgress.plugins.progress) / 2)}%`,
                    }}
                  ></div>
                </div>
              </div>
              <div className="space-y-2 text-sm">
                <div className="flex items-center gap-2">
                  {downloadProgress.mcv.status === 'completed' && (
                    <>
                      <span className="text-green-400">✓</span>
                      <span>mcv本体 - 完了</span>
                    </>
                  )}
                  {downloadProgress.mcv.status === 'downloading' && (
                    <>
                      <span className="text-blue-400">→</span>
                      <span>mcv本体 - ダウンロード中 {downloadProgress.mcv.progress}%</span>
                    </>
                  )}
                  {downloadProgress.mcv.status === 'pending' && (
                    <>
                      <span className="text-gray-500">○</span>
                      <span>mcv本体 - 待機中</span>
                    </>
                  )}
                  {downloadProgress.mcv.status === 'error' && (
                    <>
                      <span className="text-red-400">✗</span>
                      <span>mcv本体 - エラー</span>
                    </>
                  )}
                </div>
                <div className="flex items-center gap-2">
                  {downloadProgress.plugins.status === 'completed' && (
                    <>
                      <span className="text-green-400">✓</span>
                      <span>プラグイン - {selectedPlugins.size > 0 ? '完了' : 'スキップ'}</span>
                    </>
                  )}
                  {downloadProgress.plugins.status === 'downloading' && (
                    <>
                      <span className="text-blue-400">→</span>
                      <span>プラグイン - ダウンロード中 {downloadProgress.plugins.progress}%</span>
                    </>
                  )}
                  {downloadProgress.plugins.status === 'pending' && (
                    <>
                      <span className="text-gray-500">○</span>
                      <span>プラグイン - 待機中</span>
                    </>
                  )}
                  {downloadProgress.plugins.status === 'error' && (
                    <>
                      <span className="text-red-400">✗</span>
                      <span>プラグイン - エラー</span>
                    </>
                  )}
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
                onClick={handleLaunchMcv}
                className="flex-1 px-6 py-3 bg-blue-600 hover:bg-blue-700 rounded-lg font-semibold transition-colors"
              >
                mcvを起動
              </button>
              <button
                onClick={handleClose}
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
