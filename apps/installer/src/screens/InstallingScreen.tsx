import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'

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

type StepStatus = 'pending' | 'in-progress' | 'completed' | 'error'

interface InstallStep {
  id: string
  name: string
  status: StepStatus
  progress: number
  details: string
}

interface InstallingScreenProps {
  existingVersion: string | null
  selectedPlugins: Set<string>
  availablePlugins: PluginListItem[]
  createDesktopShortcut: boolean
  createStartMenuShortcut: boolean
  onInstallComplete: () => void
  onInstallError: (error: string) => void
}

export function InstallingScreen({
  existingVersion,
  selectedPlugins,
  availablePlugins,
  createDesktopShortcut,
  createStartMenuShortcut,
  onInstallComplete,
  onInstallError,
}: InstallingScreenProps) {
  const [steps, setSteps] = useState<InstallStep[]>([])
  const [overallProgress, setOverallProgress] = useState(0)

  useEffect(() => {
    const startInstallation = async () => {
      // ステップリストを初期化
      const initialSteps: InstallStep[] = [
        {
          id: 'download-mcv',
          name: 'mcv本体をダウンロード中',
          status: 'pending',
          progress: 0,
          details: '',
        },
        {
          id: 'verify-mcv',
          name: 'mcv本体を検証中',
          status: 'pending',
          progress: 0,
          details: '',
        },
        {
          id: 'extract-mcv',
          name: 'mcv本体を展開中',
          status: 'pending',
          progress: 0,
          details: '',
        },
      ]

      // プラグインステップを追加
      selectedPlugins.forEach((pluginId) => {
        const plugin = availablePlugins.find((p) => p.id === pluginId)
        if (plugin) {
          initialSteps.push({
            id: `download-plugin-${pluginId}`,
            name: `${plugin.name}をダウンロード中`,
            status: 'pending',
            progress: 0,
            details: '',
          })
        }
      })

      // ショートカット作成ステップ
      if (createDesktopShortcut || createStartMenuShortcut) {
        initialSteps.push({
          id: 'create-shortcuts',
          name: 'ショートカットを作成中',
          status: 'pending',
          progress: 0,
          details: '',
        })
      }

      setSteps(initialSteps)

      // ダウンロード進捗リスナーを設定
      const unlisten = await listen('download-progress', (event: any) => {
        const { url, progress } = event.payload

        setSteps((prev) =>
          prev.map((step) => {
            if (url.includes('/mcv/core/') && step.id === 'download-mcv') {
              return {
                ...step,
                status: 'in-progress',
                progress: Math.round(progress),
                details: `${Math.round(progress)}% 完了`,
              }
            }
            return step
          })
        )
      })

      try {
        // Step 1: mcv本体をダウンロード
        await updateStep('download-mcv', 'in-progress', 0, 'ダウンロード開始...')
        await installMcv()
        await updateStep('download-mcv', 'completed', 100, '完了')

        // Step 2: mcv本体を検証
        await updateStep('verify-mcv', 'completed', 100, 'チェックサム検証済み')

        // Step 3: mcv本体を展開
        await updateStep('extract-mcv', 'completed', 100, 'インストールディレクトリに展開済み')

        // Step 4: プラグインをインストール
        await installPlugins()

        // Step 5: ショートカット作成
        if (createDesktopShortcut || createStartMenuShortcut) {
          await updateStep('create-shortcuts', 'in-progress', 0, 'ショートカット作成中...')
          await createShortcuts()
          await updateStep('create-shortcuts', 'completed', 100, 'ショートカット作成完了')
        }

        // 完了
        setOverallProgress(100)
        onInstallComplete()
      } catch (error) {
        console.error('Installation failed:', error)
        onInstallError(`インストールに失敗しました: ${error}`)
      } finally {
        unlisten()
      }
    }

    const updateStep = async (
      stepId: string,
      status: StepStatus,
      progress: number,
      details: string
    ) => {
      setSteps((prev) =>
        prev.map((step) => (step.id === stepId ? { ...step, status, progress, details } : step))
      )
      updateOverallProgress()
    }

    const updateOverallProgress = () => {
      setSteps((prev) => {
        const completed = prev.filter((s) => s.status === 'completed').length
        const total = prev.length
        const progress = total > 0 ? Math.round((completed / total) * 100) : 0
        setOverallProgress(progress)
        return prev
      })
    }

    const installMcv = async () => {
      // 1. mcv更新情報を取得
      const mcvInfo = await invoke<any>('check_mcv_update', {
        currentVersion: existingVersion || '0.0.0',
      })

      if (!mcvInfo) {
        throw new Error('No mcv update available')
      }

      // 2. ダウンロードURLを構築
      const downloadUrl = await invoke<string>('get_mcv_download_url', {
        version: mcvInfo.version,
        channel: mcvInfo.channel,
      })

      // 3. 一時ディレクトリにダウンロード
      const tempDir = await invoke<string>('get_temp_dir')
      const tempZipPath = `${tempDir}\\mcv_${mcvInfo.version}.zip`

      await invoke('download_file', {
        url: downloadUrl,
        dest: tempZipPath,
      })

      // 4. チェックサム検証
      const isValid = await invoke<boolean>('verify_checksum', {
        filePath: tempZipPath,
        expected: mcvInfo.sha256,
      })

      if (!isValid) {
        throw new Error('Checksum verification failed')
      }

      // 5. インストール（ZIP展開）
      const localAppData = await invoke<string>('get_local_app_data')
      const installDir = `${localAppData}\\MultiCommentViewer`

      await invoke('install_mcv', {
        zipPath: tempZipPath,
        destDir: installDir,
      })
    }

    const installPlugins = async () => {
      if (selectedPlugins.size === 0) {
        return
      }

      const tempDir = await invoke<string>('get_temp_dir')
      const localAppData = await invoke<string>('get_local_app_data')
      const pluginDir = `${localAppData}\\MultiCommentViewer\\plugins`

      for (const pluginId of selectedPlugins) {
        const plugin = availablePlugins.find((p) => p.id === pluginId)
        if (!plugin) continue

        const stepId = `download-plugin-${pluginId}`

        await updateStep(stepId, 'in-progress', 0, 'ダウンロード中...')

        // チャンネルとバージョンを取得
        const channel = plugin.channels.stable
          ? 'stable'
          : plugin.channels.beta
            ? 'beta'
            : 'alpha'
        const version = plugin.channels[channel]

        if (!version) continue

        // ダウンロードURLを構築
        const downloadUrl = await invoke<string>('get_plugin_download_url', {
          pluginId: pluginId,
          version: version,
          channel: channel,
        })

        // ダウンロード
        const tempDllPath = `${tempDir}\\${pluginId}_${version}.dll`
        await invoke('download_file', {
          url: downloadUrl,
          dest: tempDllPath,
        })

        // インストール（DLLコピー）
        await invoke('install_plugin', {
          dllPath: tempDllPath,
          destDir: pluginDir,
        })

        await updateStep(stepId, 'completed', 100, '完了')
      }
    }

    const createShortcuts = async () => {
      const localAppData = await invoke<string>('get_local_app_data')
      const targetPath = `${localAppData}\\MultiCommentViewer\\mcv.exe`

      if (createDesktopShortcut) {
        try {
          await invoke('create_desktop_shortcut', {
            targetPath,
            shortcutName: 'MultiCommentViewer',
          })
        } catch (error) {
          console.error('Failed to create desktop shortcut:', error)
          // ショートカット作成失敗は致命的ではないので続行
        }
      }

      if (createStartMenuShortcut) {
        try {
          await invoke('create_start_menu_entry', {
            targetPath,
            appName: 'MultiCommentViewer',
          })
        } catch (error) {
          console.error('Failed to create start menu entry:', error)
          // ショートカット作成失敗は致命的ではないので続行
        }
      }
    }

    startInstallation()
  }, [])

  const getStatusIcon = (status: StepStatus) => {
    switch (status) {
      case 'completed':
        return <span className="text-green-400">✓</span>
      case 'in-progress':
        return <span className="text-blue-400">→</span>
      case 'error':
        return <span className="text-red-400">✗</span>
      default:
        return <span className="text-gray-500">○</span>
    }
  }

  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-6">MultiCommentViewer をインストール中</h2>

      {/* 全体進捗 */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <span className="text-gray-300">全体進捗</span>
          <span className="font-semibold">{overallProgress}%</span>
        </div>
        <div className="w-full bg-gray-700 rounded-full h-4">
          <div
            className="bg-blue-600 h-4 rounded-full transition-all duration-300"
            style={{ width: `${overallProgress}%` }}
          ></div>
        </div>
      </div>

      {/* インストール詳細 */}
      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700">
        <h3 className="text-xl font-semibold mb-4">インストール詳細:</h3>
        <div className="space-y-3 max-h-96 overflow-y-auto">
          {steps.map((step) => (
            <div key={step.id} className="flex items-start gap-3">
              <div className="mt-1">{getStatusIcon(step.status)}</div>
              <div className="flex-1">
                <div className="font-medium">{step.name}</div>
                {step.details && (
                  <div className="text-sm text-gray-400 mt-1">{step.details}</div>
                )}
                {step.status === 'in-progress' && step.progress > 0 && (
                  <div className="mt-2">
                    <div className="w-full bg-gray-700 rounded-full h-2">
                      <div
                        className="bg-blue-500 h-2 rounded-full transition-all duration-300"
                        style={{ width: `${step.progress}%` }}
                      ></div>
                    </div>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="mt-6 text-gray-400 text-sm">
        <p>セットアップがMultiCommentViewerをインストールしています。しばらくお待ちください...</p>
      </div>
    </div>
  )
}
