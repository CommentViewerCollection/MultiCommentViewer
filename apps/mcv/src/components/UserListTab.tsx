import { useState, useEffect, useCallback } from 'react'
import { invoke } from '@tauri-apps/api/core'
import type { MessagePart, ConnectionInfo } from '../App'
import type { UserInfo, UserSettings } from '../types/user'

interface Props {
  userSettings: Map<string, UserSettings>
  connections: ConnectionInfo[]
  onNicknameChange: (userId: string, nickname: string) => void
  onMcvNgChange: (userId: string, isNg: boolean) => void
  focusUserId?: string
}

function extractText(parts: MessagePart[]): string {
  return parts
    .filter((p): p is Extract<MessagePart, { type: 'text' }> => p.type === 'text')
    .map(p => p.text)
    .join('')
}

function Avatar({ url, size }: { url?: string; size: number }) {
  const [error, setError] = useState(false)
  if (url && !error) {
    return (
      <img
        src={url}
        alt=""
        width={size}
        height={size}
        onError={() => setError(true)}
        className="rounded-full object-cover shrink-0"
        style={{ width: size, height: size }}
      />
    )
  }
  return (
    <div
      className="rounded-full bg-gray-400 dark:bg-gray-600 shrink-0"
      style={{ width: size, height: size }}
    />
  )
}

export function UserListTab({ userSettings, connections, onNicknameChange, onMcvNgChange, focusUserId }: Props) {
  const [users, setUsers] = useState<UserInfo[]>([])
  const [selectedUserId, setSelectedUserId] = useState<string | null>(focusUserId ?? null)
  const [nicknameInput, setNicknameInput] = useState('')

  const loadUsers = useCallback(async () => {
    try {
      const list = await invoke<UserInfo[]>('get_users', { limit: 1000, offset: 0 })
      setUsers(list)
    } catch (error) {
      console.error('Failed to load users:', error)
    }
  }, [])

  useEffect(() => {
    loadUsers()
  }, [loadUsers])

  useEffect(() => {
    if (focusUserId) {
      setSelectedUserId(focusUserId)
      const settings = userSettings.get(focusUserId)
      setNicknameInput(settings?.nickname ?? '')
    }
  }, [focusUserId, userSettings])

  const selectedUser = users.find(u => u.user_id === selectedUserId) ?? null
  const selectedSettings = selectedUserId
    ? userSettings.get(selectedUserId) ?? { nickname: '', is_mcv_ng: false }
    : null

  const handleSelectUser = (userId: string) => {
    setSelectedUserId(userId)
    const settings = userSettings.get(userId)
    setNicknameInput(settings?.nickname ?? '')
  }

  const handleNicknameCommit = () => {
    if (selectedUserId) {
      onNicknameChange(selectedUserId, nicknameInput)
    }
  }

  const connectionName = (connectionId: string) =>
    connections.find(c => c.connection_id === connectionId)?.name ?? connectionId

  return (
    <div className="flex h-full">
      {/* 左ペイン: ユーザーリスト */}
      <div className="w-64 shrink-0 border-r border-gray-200 dark:border-gray-700 overflow-y-auto">
        <div className="px-3 py-2 text-xs text-gray-500 dark:text-gray-400 border-b border-gray-200 dark:border-gray-700 flex items-center justify-between">
          <span>{users.length} ユーザー</span>
          <button
            onClick={loadUsers}
            className="text-blue-400 hover:text-blue-300 transition-colors"
            title="更新"
          >
            ↻
          </button>
        </div>
        {users.map(user => {
          const settings = userSettings.get(user.user_id)
          const displayName = settings?.nickname || extractText(user.display_name) || user.user_id
          const isSelected = selectedUserId === user.user_id

          return (
            <button
              key={user.user_id}
              onClick={() => handleSelectUser(user.user_id)}
              className={`w-full text-left flex items-center gap-2 px-3 py-2 text-sm border-b border-gray-100 dark:border-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors ${
                isSelected ? 'bg-blue-50 dark:bg-blue-900/30' : ''
              }`}
            >
              <Avatar url={user.avatar_url} size={28} />
              <div className="min-w-0 flex-1">
                <div
                  className={`truncate text-sm ${
                    settings?.is_mcv_ng ? 'line-through text-gray-400' : ''
                  } ${user.is_site_ng ? 'text-red-400' : ''}`}
                >
                  {displayName}
                </div>
                <div className="text-xs text-gray-400 truncate">
                  {user.comment_count} コメント
                </div>
              </div>
            </button>
          )
        })}
      </div>

      {/* 右ペイン: ユーザー詳細 */}
      <div className="flex-1 overflow-y-auto p-4">
        {!selectedUser ? (
          <div className="h-full flex items-center justify-center text-sm text-gray-400 dark:text-gray-500">
            左のリストからユーザーを選択してください
          </div>
        ) : (
          <div className="space-y-4 max-w-md">
            {/* アバターとユーザー名 */}
            <div className="flex items-center gap-3">
              <Avatar url={selectedUser.avatar_url} size={56} />
              <div>
                <div className="text-base font-semibold">
                  {extractText(selectedUser.display_name) || selectedUser.user_id}
                </div>
                <div className="text-xs text-gray-500 dark:text-gray-400 font-mono break-all">
                  {selectedUser.user_id}
                </div>
              </div>
            </div>

            {/* 接続・コメント数 */}
            <div className="text-sm space-y-1">
              <div>
                <span className="text-gray-500 dark:text-gray-400">接続: </span>
                {connectionName(selectedUser.connection_id)}
              </div>
              <div>
                <span className="text-gray-500 dark:text-gray-400">コメント数: </span>
                {selectedUser.comment_count}
              </div>
            </div>

            {/* サイトNG（読み取り専用） */}
            <div className="flex items-center gap-2 text-sm">
              <span className="text-gray-500 dark:text-gray-400">サイトNG:</span>
              <span
                className={
                  selectedUser.is_site_ng
                    ? 'text-red-400 font-medium'
                    : 'text-gray-400 dark:text-gray-500'
                }
              >
                {selectedUser.is_site_ng ? 'あり（BAN検出）' : 'なし'}
              </span>
            </div>

            {/* mcv内NG */}
            <div className="text-sm">
              <label className="flex items-center gap-2 cursor-pointer select-none">
                <input
                  type="checkbox"
                  checked={selectedSettings?.is_mcv_ng ?? false}
                  onChange={e => onMcvNgChange(selectedUser.user_id, e.target.checked)}
                  className="w-4 h-4 accent-blue-500"
                />
                <span>mcv内NG（コメントを非表示）</span>
              </label>
            </div>

            {/* ニックネーム */}
            <div className="space-y-1">
              <label className="block text-sm text-gray-500 dark:text-gray-400">
                ニックネーム
              </label>
              <input
                type="text"
                value={nicknameInput}
                onChange={e => setNicknameInput(e.target.value)}
                onBlur={handleNicknameCommit}
                onKeyDown={e => {
                  if (e.key === 'Enter') handleNicknameCommit()
                }}
                placeholder="（未設定）"
                className="w-full px-3 py-2 bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500 text-sm"
              />
              <div className="text-xs text-gray-400 dark:text-gray-500">
                Enter またはフォーカス解除で保存
              </div>
            </div>

            {/* バッジ一覧 */}
            {selectedUser.badges.length > 0 && (
              <div className="space-y-1">
                <div className="text-sm text-gray-500 dark:text-gray-400">バッジ</div>
                <div className="flex flex-wrap gap-1">
                  {selectedUser.badges.map((badge, i) =>
                    badge.image_url ? (
                      <img
                        key={i}
                        src={badge.image_url}
                        alt={badge.name}
                        title={badge.name}
                        width={20}
                        height={20}
                        className="rounded"
                      />
                    ) : (
                      <span
                        key={i}
                        className="text-xs bg-gray-200 dark:bg-gray-600 px-1 py-0.5 rounded"
                      >
                        {badge.name}
                      </span>
                    )
                  )}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  )
}
