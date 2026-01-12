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
  const dataGridRef = useRef<DataGridRef>(null)
  const [atBottom, setAtBottom] = useState(true)
  const atBottomRef = useRef(true)
  const [editingNames, setEditingNames] = useState<{ [key: string]: string }>({})
  const [selectedConnectionForCommand, setSelectedConnectionForCommand] = useState<string>('')
  const [commandInput, setCommandInput] = useState('')

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
      // 編集中の名前を初期化（既に編集中のものは保持）
      setEditingNames((prev) => {
        const newEditingNames: { [key: string]: string } = {}
        conns.forEach((conn) => {
          newEditingNames[conn.connection_id] = prev[conn.connection_id] ?? conn.name
        })
        return newEditingNames
      })
    } catch (error) {
      console.error('Failed to load connections:', error)
    }
  }

  // atBottomの変更をrefに反映
  useEffect(() => {
    atBottomRef.current = atBottom
  }, [atBottom])

  useEffect(() => {
    // 初回読み込み
    loadConnections()

    // コメント受信イベントをリッスン
    const unlistenComment = listen<Comment>('comment-received', (event) => {
      setComments((prev) => [...prev, event.payload])
      // 最下部にいる場合は自動スクロール
      if (atBottomRef.current) {
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
      unlistenComment.then((fn) => fn())
      unlistenConnected.then((fn) => fn())
      unlistenDisconnected.then((fn) => fn())
    }
  }, [])

  const handleAddConnection = async () => {
    try {
      await invoke<string>('add_connection')
      await loadConnections()
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

  const handleRenameConnection = async (connectionId: string, newName: string) => {
    try {
      // 空欄も許容
      await invoke('rename_connection', { connectionId, newName })
      await loadConnections()
    } catch (error) {
      console.error('Failed to rename connection:', error)
    }
  }

  const handleNameChange = (connectionId: string, newName: string) => {
    // ローカルステートのみ更新（IME入力中でも即座に反映）
    setEditingNames((prev) => ({ ...prev, [connectionId]: newName }))
  }

  const handleNameBlur = (connectionId: string) => {
    // 確定時にバックエンドに送信
    const newName = editingNames[connectionId]
    if (newName !== undefined) {
      const connection = connections.find((c) => c.connection_id === connectionId)
      if (connection && newName !== connection.name) {
        handleRenameConnection(connectionId, newName)
      }
    }
  }

  const handleConnect = async (connectionId: string) => {
    try {
      await invoke('connect', { connectionId })
      // loadConnections()はconnectedイベントで自動実行される
    } catch (error) {
      console.error('Failed to connect:', error)
    }
  }

  const handleDisconnect = async (connectionId: string) => {
    try {
      await invoke('disconnect', { connectionId })
      // loadConnections()はdisconnectedイベントで自動実行される
    } catch (error) {
      console.error('Failed to disconnect:', error)
    }
  }

  const handleSendCommand = async () => {
    if (!selectedConnectionForCommand) {
      alert('接続を選択してください')
      return
    }
    if (!commandInput.trim()) {
      alert('コマンドを入力してください')
      return
    }

    try {
      const result = await invoke<string>('send_command', {
        connectionId: selectedConnectionForCommand,
        command: commandInput.trim(),
      })
      console.log('Command result:', result)
      setCommandInput('')
    } catch (error) {
      console.error('Failed to send command:', error)
      alert(`コマンド送信失敗: ${error}`)
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
    if (column.key === 'connection_name') {
      // 接続名を動的に取得（名前変更に連動、空欄も許容）
      const conn = connections.find(c => c.connection_id === item.connection_id)
      return <span>{conn?.name ?? ''}</span>
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

  return (
    <div className="min-h-screen bg-gray-900 text-white flex">
      {/* サイドバー: 接続一覧 */}
      <div className="w-80 bg-gray-800 border-r border-gray-700 flex flex-col">
        <div className="p-4 border-b border-gray-700">
          <h1 className="text-2xl font-bold mb-2">MultiCommentViewer</h1>
          <button
            onClick={handleAddConnection}
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
                className="p-3 bg-gray-700 rounded transition-colors"
              >
                <div className="flex items-center justify-between mb-2">
                  <input
                    type="text"
                    value={editingNames[conn.connection_id] ?? conn.name}
                    onChange={(e) => handleNameChange(conn.connection_id, e.target.value)}
                    onBlur={() => handleNameBlur(conn.connection_id)}
                    onClick={(e) => e.stopPropagation()}
                    className="font-semibold text-sm bg-transparent border-b border-transparent hover:border-gray-500 focus:border-blue-500 focus:outline-none flex-1 mr-2"
                  />
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
          <span>接続数: {connections.length}</span>
        </div>
      </div>

      {/* メインエリア: コメント表示 */}
      <div className="flex-1 flex flex-col">
        <div className="p-4 bg-gray-800 border-b border-gray-700">
          <h2 className="text-xl font-semibold">コメント</h2>
        </div>

        <div className="flex-1 p-4">
          <DataGridComponent
            ref={dataGridRef}
            data={comments}
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

        {/* コマンド入力セクション */}
        <div className="p-4 bg-gray-800 border-t border-gray-700">
          <div className="flex gap-2 items-end">
            <div className="flex-shrink-0">
              <label className="block text-sm font-medium mb-1 text-gray-300">接続選択</label>
              <select
                value={selectedConnectionForCommand}
                onChange={(e) => setSelectedConnectionForCommand(e.target.value)}
                className="px-3 py-2 bg-gray-700 border border-gray-600 rounded focus:outline-none focus:border-blue-500 text-white"
              >
                <option value="">選択してください</option>
                {connections.map((conn) => (
                  <option key={conn.connection_id} value={conn.connection_id}>
                    {conn.name}
                  </option>
                ))}
              </select>
            </div>

            <div className="flex-1">
              <label className="block text-sm font-medium mb-1 text-gray-300">コマンド</label>
              <input
                type="text"
                value={commandInput}
                onChange={(e) => setCommandInput(e.target.value)}
                onKeyDown={(e) => {
                  if (e.key === 'Enter') {
                    handleSendCommand()
                  }
                }}
                placeholder="例: disconnect, pause, resume, rate 3, comment 太郎 こんにちは"
                className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded focus:outline-none focus:border-blue-500 text-white placeholder-gray-500"
              />
            </div>

            <button
              onClick={handleSendCommand}
              className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
            >
              送信
            </button>
          </div>
        </div>
      </div>
    </div>
  )
}

export default App
