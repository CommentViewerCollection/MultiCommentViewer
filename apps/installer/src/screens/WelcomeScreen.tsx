import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'

interface InstallerUpdateInfo {
  version: string
  required: boolean
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
}

interface WelcomeScreenProps {
  onInstallerUpdateDetected: (update: InstallerUpdateInfo | null) => void
  onExistingInstallationDetected: (version: string | null) => void
  onInitComplete: () => void
}

export function WelcomeScreen({
  onInstallerUpdateDetected,
  onExistingInstallationDetected,
  onInitComplete,
}: WelcomeScreenProps) {
  const [loading, setLoading] = useState(true)
  const [installerUpdate, setInstallerUpdate] = useState<InstallerUpdateInfo | null>(null)
  const [existingVersion, setExistingVersion] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const init = async () => {
      try {
        // Step 1: インストーラー更新確認
        const update = await invoke<InstallerUpdateInfo | null>('check_installer_update')
        setInstallerUpdate(update)
        onInstallerUpdateDetected(update)

        if (update && update.required) {
          // 更新が必要な場合はここでブロック
          setLoading(false)
          return
        }

        // Step 2: 既存インストール検出
        const existing = await invoke<string | null>('check_existing_installation')
        setExistingVersion(existing)
        onExistingInstallationDetected(existing)

        setLoading(false)
        onInitComplete()
      } catch (e) {
        console.error('Initialization failed:', e)
        setError(`初期化に失敗しました: ${e}`)
        setLoading(false)
      }
    }

    init()
  }, [])

  const handleInstallerUpdate = async () => {
    // TODO: インストーラー更新処理
    alert('インストーラー更新機能は未実装です')
  }

  if (loading) {
    return (
      <div className="p-8">
        <h2 className="text-3xl font-bold mb-4">MultiCommentViewer セットアップウィザードへようこそ</h2>
        <p className="text-xl text-gray-400 mb-6">バージョン 0.1.0 インストーラー</p>
        <div className="mt-8">
          <div className="inline-block animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-blue-500"></div>
          <p className="mt-4 text-gray-400">初期化中...</p>
        </div>
      </div>
    )
  }

  if (error) {
    return (
      <div className="p-8">
        <h2 className="text-3xl font-bold mb-4">エラー</h2>
        <div className="bg-red-900 border border-red-700 p-4 rounded">
          <p className="text-red-200">{error}</p>
        </div>
      </div>
    )
  }

  // インストーラー更新が必要な場合
  if (installerUpdate && installerUpdate.required) {
    return (
      <div className="p-8">
        <h2 className="text-3xl font-bold mb-4">インストーラーの更新</h2>
        <div className="space-y-4 mb-6">
          <p className="text-gray-300">新しいバージョンのインストーラーが利用可能です。</p>
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
            <p className="font-semibold text-yellow-200">⚠ この更新は必須です</p>
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
    )
  }

  // 通常の初期化完了画面
  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-4">MultiCommentViewer セットアップウィザードへようこそ</h2>
      <p className="text-xl text-gray-400 mb-6">バージョン 0.1.0 インストーラー</p>

      {existingVersion ? (
        <div className="space-y-4">
          <div className="bg-blue-900 border border-blue-700 p-4 rounded">
            <p className="font-semibold text-blue-200 mb-2">✓ 更新可能</p>
            <p className="text-blue-300">既存のインストールが検出されました。</p>
            <p className="text-blue-400 mt-2">
              現在のバージョン: <span className="font-semibold">{existingVersion}</span>
            </p>
          </div>
          <p className="text-gray-300">
            このウィザードがMultiCommentViewerの更新プロセスをガイドします。
          </p>
        </div>
      ) : (
        <div className="space-y-4">
          <div className="bg-green-900 border border-green-700 p-4 rounded">
            <p className="font-semibold text-green-200 mb-2">○ 新規インストール</p>
            <p className="text-green-300">既存のインストールは検出されませんでした。</p>
          </div>
          <p className="text-gray-300">
            このウィザードがMultiCommentViewerのインストールプロセスをガイドします。
          </p>
        </div>
      )}
    </div>
  )
}
