import { useState, useEffect, useRef } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'
import { DataGrid, DataGridRef, Column } from 'my-dataview'

// @ts-ignore - Type compatibility issue with React versions
const DataGridComponent = DataGrid as any

interface Comment {
  id: string
  user_name: string
  user_id: string
  text: string
  timestamp: number
  connection_id?: string
  connection_name?: string
}

interface ConnectionInfo {
  connection_id: string
  plugin_id: string
  status: { type: string; message?: string }
  site_name: string
  input_info: string
  name: string
}

function App() {
  const [comments, setComments] = useState<Comment[]>([])
  const [connections, setConnections] = useState<ConnectionInfo[]>([])
  const [selectedConnectionId, setSelectedConnectionId] = useState<string | null>(null)
  const dataGridRef = useRef<DataGridRef>(null)
  const [atBottom, setAtBottom] = useState(true)

  const [showAddDialog, setShowAddDialog] = useState(false)
  const [newConnectionName, setNewConnectionName] = useState('')

  // DataGridのカラム定義
  const [columns, setColumns] = useState<Column<Comment>[]>([
    { key: 'connection_name', label: '接続', width: 150, visible: true, resizable: true },
    { key: 'user_name', label: 'ユーザー名', width: 150, visible: true, resizable: true },
    { key: 'text', label: 'コメント', width: 400, visible: true, resizable: true, wrap: true },
    { key: 'timestamp', label: '時刻', width: 150, visible: true, resizable: true },
  ])

  // 接続一覧を読み込む
  const loadConnections = async () => {
    try {
      const conns = await invoke<ConnectionInfo[]>('get_connections')
      setConnections(conns)
    } catch (error) {
      console.error('Failed to load connections:', error)
    }
  }

  useEffect(() => {
    // 初回読み込み
    loadConnections()

    // 定期的に接続一覧を更新
    const interval = setInterval(loadConnections, 1000)

    // コメント受信イベントをリッスン
    const unlistenComment = listen<Comment>('comment-received', (event) => {
      // 接続名を付与
      const commentWithName = {
        ...event.payload,
        connection_name: connections.find(c => c.connection_id === event.payload.connection_id)?.name || '不明',
      }
      setComments((prev) => [...prev, commentWithName])
      // 最下部にいる場合は自動スクロール
      if (atBottom) {
        setTimeout(() => {
          dataGridRef.current?.scrollToBottom()
        }, 50)
      }
    })

    // 接続完了イベントをリッスン
    const unlistenConnected = listen('connected', () => {
      loadConnections()
    })

    // 切断完了イベントをリッスン
    const unlistenDisconnected = listen('disconnected', () => {
      loadConnections()
    })

    return () => {
      clearInterval(interval)
      unlistenComment.then((fn) => fn())
      unlistenConnected.then((fn) => fn())
      unlistenDisconnected.then((fn) => fn())
    }
  }, [atBottom, connections])

  const handleAddConnection = async () => {
    if (!newConnectionName.trim()) {
      alert('接続名を入力してください')
      return
    }
    try {
      await invoke<string>('add_connection', { name: newConnectionName })
      await loadConnections()
      setShowAddDialog(false)
      setNewConnectionName('')
    } catch (error) {
      console.error('Failed to add connection:', error)
    }
  }

  const handleRemoveConnection = async (connectionId: string) => {
    try {
      await invoke('remove_connection', { connectionId })
      await loadConnections()
    } catch (error) {
      console.error('Failed to remove connection:', error)
      alert('接続の削除に失敗しました。接続中の場合は削除できません。')
    }
  }

  const handleConnect = async (connectionId: string) => {
    try {
      await invoke('connect', { connectionId })
      await loadConnections()
    } catch (error) {
      console.error('Failed to connect:', error)
    }
  }

  const handleDisconnect = async (connectionId: string) => {
    try {
      await invoke('disconnect', { connectionId })
      await loadConnections()
    } catch (error) {
      console.error('Failed to disconnect:', error)
    }
  }

  const formatTime = (timestamp: number) => {
    const date = new Date(timestamp * 1000)
    return date.toLocaleTimeString('ja-JP')
  }

  const handleColumnResize = (columnKey: string, width: number) => {
    setColumns((prev) =>
      prev.map((col) => (col.key === columnKey ? { ...col, width } : col))
    )
  }

  const handleColumnVisibilityChange = (columnKey: string, visible: boolean) => {
    setColumns((prev) =>
      prev.map((col) => (col.key === columnKey ? { ...col, visible } : col))
    )
  }

  const renderCell = (item: Comment, column: Column<Comment>) => {
    if (column.key === 'timestamp') {
      return <span>{formatTime(item.timestamp)}</span>
    }
    return <span>{String(item[column.key])}</span>
  }

  const getStatusColor = (status: { type: string }) => {
    switch (status.type) {
      case 'Connected':
        return 'bg-green-500'
      case 'Connecting':
        return 'bg-yellow-500'
      case 'Disconnected':
        return 'bg-gray-500'
      case 'Error':
        return 'bg-red-500'
      default:
        return 'bg-gray-500'
    }
  }

  const getStatusText = (status: { type: string }) => {
    switch (status.type) {
      case 'Connected':
        return '接続中'
      case 'Connecting':
        return '接続中...'
      case 'Disconnected':
        return '切断'
      case 'Created':
        return '作成済み'
      case 'Error':
        return 'エラー'
      default:
        return '不明'
    }
  }

  // フィルタされたコメント（選択された接続のみ）
  const filteredComments = selectedConnectionId
    ? comments.filter((c) => c.connection_id === selectedConnectionId)
    : comments

  return (
    <div className="min-h-screen bg-gray-900 text-white flex">
      {/* サイドバー: 接続一覧 */}
      <div className="w-80 bg-gray-800 border-r border-gray-700 flex flex-col">
        <div className="p-4 border-b border-gray-700">
          <h1 className="text-2xl font-bold mb-2">MultiCommentViewer</h1>
          <button
            onClick={() => setShowAddDialog(true)}
            className="w-full px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
          >
            + 接続を追加
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-2">
          <h2 className="text-sm font-semibold text-gray-400 mb-2">接続一覧</h2>
          {connections.length === 0 ? (
            <div className="text-center py-8 text-gray-500 text-sm">
              接続がありません
            </div>
          ) : (
            connections.map((conn) => (
              <div
                key={conn.connection_id}
                className={`p-3 bg-gray-700 rounded cursor-pointer transition-colors ${
                  selectedConnectionId === conn.connection_id
                    ? 'ring-2 ring-blue-500'
                    : 'hover:bg-gray-650'
                }`}
                onClick={() => setSelectedConnectionId(conn.connection_id)}
              >
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold text-sm truncate">{conn.name}</span>
                  <div className="flex items-center gap-2">
                    <div
                      className={`w-2 h-2 rounded-full ${getStatusColor(conn.status)}`}
                    />
                    <span className="text-xs text-gray-400">
                      {getStatusText(conn.status)}
                    </span>
                  </div>
                </div>
                <div className="text-xs text-gray-400 mb-2 truncate">
                  {conn.site_name} - {conn.input_info}
                </div>
                <div className="flex gap-2">
                  {(conn.status.type === 'Created' || conn.status.type === 'Disconnected') && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleConnect(conn.connection_id)
                      }}
                      className="flex-1 px-2 py-1 text-xs bg-green-600 hover:bg-green-700 rounded transition-colors"
                    >
                      接続
                    </button>
                  )}
                  {conn.status.type === 'Connected' && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleDisconnect(conn.connection_id)
                      }}
                      className="flex-1 px-2 py-1 text-xs bg-red-600 hover:bg-red-700 rounded transition-colors"
                    >
                      切断
                    </button>
                  )}
                  {(conn.status.type === 'Created' || conn.status.type === 'Disconnected') && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleRemoveConnection(conn.connection_id)
                      }}
                      className="px-2 py-1 text-xs bg-gray-600 hover:bg-gray-700 rounded transition-colors"
                    >
                      削除
                    </button>
                  )}
                </div>
              </div>
            ))
          )}
        </div>

        <div className="p-4 border-t border-gray-700 text-xs text-gray-500">
          <div className="flex items-center justify-between">
            <span>接続数: {connections.length}</span>
            <button
              onClick={() => setSelectedConnectionId(null)}
              className="text-blue-400 hover:text-blue-300"
            >
              すべて表示
            </button>
          </div>
        </div>
      </div>

      {/* メインエリア: コメント表示 */}
      <div className="flex-1 flex flex-col">
        <div className="p-4 bg-gray-800 border-b border-gray-700">
          <h2 className="text-xl font-semibold">
            コメント
            {selectedConnectionId && (
              <span className="ml-2 text-sm text-gray-400">
                ({connections.find((c) => c.connection_id === selectedConnectionId)?.name})
              </span>
            )}
          </h2>
        </div>

        <div className="flex-1 p-4">
          <DataGridComponent
            ref={dataGridRef}
            data={filteredComments}
            columns={columns}
            renderCell={renderCell}
            height="100%"
            backgroundColor="#1f2937"
            border="1px solid #374151"
            onAtBottomChange={setAtBottom}
            onColumnResize={handleColumnResize}
            onColumnVisibilityChange={handleColumnVisibilityChange}
            defaultItemHeight={60}
          />
        </div>
      </div>

      {/* 接続追加ダイアログ */}
      {showAddDialog && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg p-6 w-96 border border-gray-700">
            <h2 className="text-xl font-semibold mb-4">接続を追加</h2>
            <div className="mb-4">
              <label className="block text-sm font-medium mb-2">接続名</label>
              <input
                type="text"
                value={newConnectionName}
                onChange={(e) => setNewConnectionName(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    handleAddConnection()
                  }
                }}
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded focus:outline-none focus:ring-2 focus:ring-blue-500"
                placeholder="例: メイン配信"
                autoFocus
              />
            </div>
            <div className="flex gap-2 justify-end">
              <button
                onClick={() => {
                  setShowAddDialog(false)
                  setNewConnectionName('')
                }}
                className="px-4 py-2 bg-gray-600 hover:bg-gray-700 rounded transition-colors"
              >
                キャンセル
              </button>
              <button
                onClick={handleAddConnection}
                className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors"
              >
                追加
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default App
