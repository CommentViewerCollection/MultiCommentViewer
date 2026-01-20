import type { UninstallTarget } from '../types'

interface UninstallCompletionScreenProps {
  target: UninstallTarget
  keepUserData: boolean
}

export function UninstallCompletionScreen({
  target,
  keepUserData,
}: UninstallCompletionScreenProps) {
  const uninstalledMcv = target === 'mcv' || target === 'both'
  const uninstalledInstaller = target === 'installer' || target === 'both'

  return (
    <div className="p-8">
      {/* 完了アイコンとタイトル */}
      <div className="text-center mb-8">
        <div className="text-6xl text-green-400 mb-4">✓</div>
        <h2 className="text-3xl font-bold mb-2">アンインストールの完了</h2>
        <p className="text-gray-300">
          MultiCommentViewerのアンインストールが完了しました。
        </p>
      </div>

      {/* アンインストールされたコンポーネント */}
      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700 mb-6">
        <h3 className="text-xl font-semibold mb-4">■ アンインストールされたコンポーネント</h3>
        <div className="space-y-2">
          {uninstalledMcv && (
            <>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>MultiCommentViewer 本体</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>プラグイン</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>デスクトップショートカット</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>スタートメニューエントリ</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>Windowsアプリ登録</span>
              </div>
            </>
          )}

          {uninstalledInstaller && (
            <>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>MultiCommentViewer Installer & Updater</span>
              </div>
              <div className="flex items-center gap-2">
                <span className="text-green-400">✓</span>
                <span>インストーラーのWindowsアプリ登録</span>
              </div>
            </>
          )}
        </div>
      </div>

      {/* 保持されたファイル */}
      {uninstalledMcv && keepUserData && (
        <div className="bg-blue-900 border border-blue-700 p-6 rounded-lg mb-6">
          <h3 className="text-xl font-semibold mb-4">■ 保持されたユーザーデータ</h3>
          <div className="space-y-2 mb-4">
            <div className="flex items-center gap-2">
              <span className="text-blue-300">📄</span>
              <span>設定ファイル (config.json)</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-blue-300">📄</span>
              <span>ログデータベース (logs.db)</span>
            </div>
            <div className="flex items-center gap-2">
              <span className="text-blue-300">📁</span>
              <span>ユーザーデータディレクトリ (user_data/)</span>
            </div>
          </div>
          <div className="text-sm text-blue-200">
            <p>これらのファイルは再インストール時に引き続き使用できます。</p>
            <p className="mt-2">
              完全に削除したい場合は、以下のディレクトリを手動で削除してください：
            </p>
            <div className="mt-2 bg-blue-950 p-2 rounded font-mono text-xs">
              %LOCALAPPDATA%\MultiCommentViewer
            </div>
          </div>
        </div>
      )}

      {/* インストーラー削除の注記 */}
      {uninstalledInstaller && (
        <div className="bg-yellow-900 border border-yellow-700 p-4 rounded mb-6">
          <p className="text-yellow-200">
            インストーラーの削除は、このウィンドウを閉じた後に完了します。
          </p>
        </div>
      )}

      <div className="text-gray-400 text-sm">
        <p>「完了」ボタンをクリックしてこのウィンドウを閉じてください。</p>
        <p className="mt-2">
          MultiCommentViewerをご利用いただき、ありがとうございました。
        </p>
      </div>
    </div>
  )
}
