import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'

interface InstallerSetupScreenProps {
  onInstallComplete: () => void
  onInstallError: (error: string) => void
}

export function InstallerSetupScreen({
  onInstallComplete,
  onInstallError,
}: InstallerSetupScreenProps) {
  const [installPath, setInstallPath] = useState<string>('')
  const [installing, setInstalling] = useState(false)
  const [progress, setProgress] = useState<string>('')
  const [warning, setWarning] = useState<string | null>(null)

  useEffect(() => {
    invoke<string>('get_installer_default_install_path')
      .then(setInstallPath)
      .catch((error) => {
        console.error('Failed to get install path:', error)
        onInstallError(`インストール先の取得に失敗しました: ${error}`)
      })
  }, [])

  const handleInstall = async () => {
    setInstalling(true)
    setWarning(null)

    try {
      // 1. 自己コピー
      setProgress('インストーラーをコピー中...')
      const copiedPath = await invoke<string>('copy_installer_to_persistent_location')

      // 2. レジストリ登録
      setProgress('Windowsアプリとして登録中...')
      try {
        await invoke('register_installer_to_windows_apps', {
          installerPath: copiedPath,
        })
      } catch (regError) {
        console.warn('Registry registration failed (non-critical):', regError)
        setWarning(
          'Windowsの「アプリと機能」への登録に失敗しました。' +
            'インストーラーは正常に動作しますが、アンインストーラーが表示されません。'
        )
      }

      setProgress('完了')
      setTimeout(() => {
        onInstallComplete()
      }, 500)
    } catch (error) {
      console.error('Installation failed:', error)
      setInstalling(false)
      onInstallError(`インストーラーのセットアップに失敗しました: ${error}

考えられる原因:
- ディスク容量不足
- ファイルシステムの権限エラー

対処方法:
- ディスク容量を確認してください
- 別のユーザーアカウントで試してください`)
    }
  }

  if (!installPath) {
    return (
      <div className="p-8">
        <h2 className="text-3xl font-bold mb-4">インストーラーのセットアップ</h2>
        <div className="mt-8">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
          <p className="mt-4 text-gray-400">初期化中...</p>
        </div>
      </div>
    )
  }

  return (
    <div className="p-8 max-w-4xl">
      <h2 className="text-3xl font-bold mb-4">インストーラーのセットアップ</h2>
      <p className="text-xl text-gray-400 mb-8">
        MultiCommentViewer Installerをこのコンピューターにインストールします。
      </p>

      {/* インストール先表示 */}
      <div className="space-y-6 mb-8">
        <div>
          <h3 className="text-lg font-semibold mb-2 text-gray-300">インストール先</h3>
          <div className="bg-gray-700 p-4 rounded">
            <code className="text-blue-300 text-sm break-all">{installPath}</code>
          </div>
        </div>

        {/* セットアップ内容 */}
        <div>
          <h3 className="text-lg font-semibold mb-2 text-gray-300">セットアップ内容</h3>
          <ul className="space-y-2 text-gray-300">
            <li className="flex items-start">
              <span className="mr-2 text-blue-400">✓</span>
              <span>インストーラーを固定場所にコピー</span>
            </li>
            <li className="flex items-start">
              <span className="mr-2 text-blue-400">✓</span>
              <span>Windowsの「アプリと機能」に登録</span>
            </li>
            <li className="flex items-start">
              <span className="mr-2 text-blue-400">✓</span>
              <span>アンインストール機能を有効化</span>
            </li>
          </ul>
        </div>

        {/* 説明 */}
        <div className="bg-blue-900 border border-blue-700 p-4 rounded">
          <p className="text-blue-200">
            このセットアップにより、インストーラーがシステムに固定され、
            MultiCommentViewerのインストール・更新・アンインストールを管理できるようになります。
          </p>
        </div>
      </div>

      {/* 警告表示 */}
      {warning && (
        <div className="bg-yellow-900 border border-yellow-700 p-4 rounded mb-6">
          <p className="font-semibold text-yellow-200">⚠ 警告</p>
          <p className="text-yellow-300 text-sm mt-2">{warning}</p>
        </div>
      )}

      {/* インストール中の表示 */}
      {installing && (
        <div className="mb-6">
          <div className="flex items-center">
            <div className="inline-block animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-blue-500 mr-4"></div>
            <span className="text-gray-300">{progress}</span>
          </div>
        </div>
      )}

      {/* インストールボタン */}
      <button
        onClick={handleInstall}
        disabled={installing}
        className={`w-full px-6 py-3 rounded-lg font-semibold text-lg transition-colors ${
          installing
            ? 'bg-gray-600 cursor-not-allowed'
            : 'bg-blue-600 hover:bg-blue-700'
        }`}
      >
        {installing ? 'インストール中...' : 'インストール'}
      </button>
    </div>
  )
}
