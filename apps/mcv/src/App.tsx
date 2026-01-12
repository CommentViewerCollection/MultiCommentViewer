import { useState, useEffect, useRef } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'
import { DataGrid, DataGridRef, Column } from 'my-dataview'

interface Comment {
  id: string
  user_name: string
  user_id: string
  text: string
  timestamp: number
}

function App() {
  const [comments, setComments] = useState<Comment[]>([])
  const [connectionId, setConnectionId] = useState<string | null>(null)
  const [isConnected, setIsConnected] = useState(false)
  const [isLoading, setIsLoading] = useState(false)
  const dataGridRef = useRef<DataGridRef>(null)
  const [atBottom, setAtBottom] = useState(true)

  // DataGridのカラム定義
  const [columns, setColumns] = useState<Column<Comment>[]>([
    { key: 'user_name', label: 'ユーザー名', width: 150, visible: true, resizable: true },
    { key: 'text', label: 'コメント', width: 400, visible: true, resizable: true, wrap: true },
    { key: 'timestamp', label: '時刻', width: 150, visible: true, resizable: true },
  ])

  useEffect(() => {
    // コメント受信イベントをリッスン
    const unlistenComment = listen<Comment>('comment-received', (event) => {
      setComments((prev) => [...prev, event.payload])
      // 最下部にいる場合は自動スクロール
      if (atBottom) {
        setTimeout(() => {
          dataGridRef.current?.scrollToBottom()
        }, 50)
      }
    })

    // 接続完了イベントをリッスン
    const unlistenConnected = listen('connected', () => {
      setIsConnected(true)
      setIsLoading(false)
    })

    // 切断完了イベントをリッスン
    const unlistenDisconnected = listen('disconnected', () => {
      setIsConnected(false)
      setIsLoading(false)
    })

    return () => {
      unlistenComment.then((fn) => fn())
      unlistenConnected.then((fn) => fn())
      unlistenDisconnected.then((fn) => fn())
    }
  }, [])

  const handleConnect = async () => {
    try {
      setIsLoading(true)

      // 接続を開始（add_connection + connect）
      const connId = await invoke<string>('start_connection')
      setConnectionId(connId)
    } catch (error) {
      console.error('Failed to connect:', error)
      setIsLoading(false)
    }
  }

  const handleDisconnect = async () => {
    if (!connectionId) return

    try {
      setIsLoading(true)
      await invoke('disconnect', { connectionId })
    } catch (error) {
      console.error('Failed to disconnect:', error)
      setIsLoading(false)
    }
  }

  const formatTime = (timestamp: number) => {
    const date = new Date(timestamp * 1000)
    return date.toLocaleTimeString('ja-JP')
  }

  const handleColumnResize = (columnKey: keyof Comment, width: number) => {
    setColumns((prev) =>
      prev.map((col) => (col.key === columnKey ? { ...col, width } : col))
    )
  }

  const handleColumnVisibilityChange = (columnKey: keyof Comment, visible: boolean) => {
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

  return (
    <div className="min-h-screen bg-gray-900 text-white">
      <div className="max-w-4xl mx-auto p-4">
        {/* ヘッダー */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold mb-2">MultiCommentViewer</h1>
          <p className="text-gray-400 text-sm">動作確認用ダミープラグイン</p>
        </div>

        {/* 接続コントロール */}
        <div className="bg-gray-800 rounded-lg p-4 mb-4">
          <div className="flex items-center gap-4">
            <button
              onClick={handleConnect}
              disabled={isConnected || isLoading}
              className="px-6 py-2 bg-green-600 hover:bg-green-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-semibold transition-colors"
            >
              {isLoading && !isConnected ? '接続中...' : '接続'}
            </button>
            <button
              onClick={handleDisconnect}
              disabled={!isConnected || isLoading}
              className="px-6 py-2 bg-red-600 hover:bg-red-700 disabled:bg-gray-600 disabled:cursor-not-allowed rounded font-semibold transition-colors"
            >
              {isLoading && isConnected ? '切断中...' : '切断'}
            </button>
            <div className="flex items-center gap-2">
              <div
                className={`w-3 h-3 rounded-full ${
                  isConnected ? 'bg-green-500' : 'bg-gray-500'
                }`}
              />
              <span className="text-sm text-gray-300">
                {isConnected ? '接続中' : '未接続'}
              </span>
            </div>
          </div>
        </div>

        {/* コメント表示エリア */}
        <div className="bg-gray-800 rounded-lg p-4">
          <h2 className="text-xl font-semibold mb-4">コメント</h2>
          {comments.length === 0 ? (
            <div className="text-center py-12 text-gray-400">
              <p className="text-lg">コメントがまだありません</p>
              <p className="text-sm mt-2">
                {isConnected
                  ? 'コメントが表示されるまでお待ちください...'
                  : '接続ボタンをクリックしてコメント受信を開始してください'}
              </p>
            </div>
          ) : (
            <DataGrid
              ref={dataGridRef}
              data={comments}
              columns={columns}
              renderCell={renderCell}
              height="600px"
              backgroundColor="#1f2937"
              border="1px solid #374151"
              onAtBottomChange={setAtBottom}
              onColumnResize={handleColumnResize}
              onColumnVisibilityChange={handleColumnVisibilityChange}
              defaultItemHeight={60}
            />
          )}
        </div>

        {/* フッター */}
        <div className="mt-8 text-center text-sm text-gray-500">
          <p>
            ダミープラグインが1-5秒間隔でランダムにコメントを生成します
          </p>
        </div>
      </div>
    </div>
  )
}

export default App
