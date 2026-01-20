interface UninstallOptionsScreenProps {
  keepUserData: boolean
  onKeepUserDataChange: (value: boolean) => void
}

export function UninstallOptionsScreen({
  keepUserData,
  onKeepUserDataChange,
}: UninstallOptionsScreenProps) {
  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-4">アンインストールオプション</h2>
      <p className="text-gray-300 mb-6">
        MultiCommentViewerをアンインストールします。ユーザーデータの取り扱いを選択してください。
      </p>

      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700 space-y-4">
        <h3 className="text-xl font-semibold mb-4">ユーザーデータの取り扱い</h3>

        {/* ユーザーデータを保持 */}
        <label className="flex items-start gap-3 p-4 bg-gray-700 rounded cursor-pointer hover:bg-gray-600 transition-colors">
          <input
            type="radio"
            name="user-data-option"
            checked={keepUserData}
            onChange={() => onKeepUserDataChange(true)}
            className="mt-1 w-5 h-5"
          />
          <div className="flex-1">
            <div className="font-semibold text-blue-200 mb-1">
              ユーザーデータを保持（推奨）
            </div>
            <div className="text-sm text-gray-300">
              設定ファイル、ログデータベースなどを保持します。再インストール時にそのまま使用できます。
            </div>
            <div className="text-xs text-gray-400 mt-2">
              保持されるファイル: config.json, logs.db, user_data/
            </div>
          </div>
        </label>

        {/* 全て削除 */}
        <label className="flex items-start gap-3 p-4 bg-gray-700 rounded cursor-pointer hover:bg-gray-600 transition-colors">
          <input
            type="radio"
            name="user-data-option"
            checked={!keepUserData}
            onChange={() => onKeepUserDataChange(false)}
            className="mt-1 w-5 h-5"
          />
          <div className="flex-1">
            <div className="font-semibold text-red-200 mb-1">
              全て削除
            </div>
            <div className="text-sm text-gray-300">
              MultiCommentViewerに関連する全てのファイルを削除します。
            </div>
            <div className="text-xs text-red-400 mt-2">
              警告: この操作は元に戻せません。全てのデータが失われます。
            </div>
          </div>
        </label>
      </div>

      <div className="mt-6 bg-yellow-900 border border-yellow-700 p-4 rounded">
        <p className="text-yellow-200">
          アンインストールを実行する前に、MultiCommentViewerが終了していることを確認してください。
        </p>
      </div>
    </div>
  )
}
