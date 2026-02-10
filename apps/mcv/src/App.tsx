import { useState, useEffect, useRef } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'
import { DataGrid, DataGridRef, Column } from 'my-dataview'
import { LogViewer } from './components/LogViewer'

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
  plugin_id?: string
  status: { type: string; message?: string }
  site_id?: string
  site_name: string
  url?: string
  browser_id?: string
  browser_name?: string
  advanced_settings?: any
  input_info: string
  name: string
}

interface SiteInfo {
  site_id: string
  site_name: string
  display_name: string
  plugin_id: string
  options_schema: any
}

interface BrowserInfo {
  browser_id: string
  browser_name: string
  display_name: string
  plugin_id: string
}

interface UpdateInfo {
  version: string
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
  min_installer_version: string
}

type TabType = 'comments' | 'logs'

function App() {
  const [activeTab, setActiveTab] = useState<TabType>('comments')
  const [comments, setComments] = useState<Comment[]>([])
  const [connections, setConnections] = useState<ConnectionInfo[]>([])
  const [sites, setSites] = useState<SiteInfo[]>([])
  const [browsers, setBrowsers] = useState<BrowserInfo[]>([])
  const dataGridRef = useRef<DataGridRef>(null)
  const [atBottom, setAtBottom] = useState(true)
  const atBottomRef = useRef(true)
  const [editingNames, setEditingNames] = useState<{ [key: string]: string }>({})
  const [selectedConnectionForCommand, setSelectedConnectionForCommand] = useState<string>('')
  const [commandInput, setCommandInput] = useState('')
  const [updateInfo, setUpdateInfo] = useState<UpdateInfo | null>(null)
  const [showUpdateDialog, setShowUpdateDialog] = useState(false)
  const [checkingUpdate, setCheckingUpdate] = useState(false)

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

  // サイト/ブラウザ情報を読み込む
  const loadSitesAndBrowsers = async () => {
    try {
      const [sitesData, browsersData] = await Promise.all([
        invoke<SiteInfo[]>('get_sites'),
        invoke<BrowserInfo[]>('get_browsers'),
      ])
      setSites(sitesData)
      setBrowsers(browsersData)
    } catch (error) {
      console.error('Failed to load sites and browsers:', error)
    }
  }

  // atBottomの変更をrefに反映
  useEffect(() => {
    atBottomRef.current = atBottom
  }, [atBottom])

  useEffect(() => {
    // 初回読み込み
    loadConnections()
    loadSitesAndBrowsers()

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

    // サイト追加イベントをリッスン
    const unlistenSiteAdded = listen<SiteInfo>('site-added', (event) => {
      console.log('[Site] Site added:', event.payload)
      setSites((prev) => [...prev, event.payload])
    })

    // ブラウザ追加イベントをリッスン
    const unlistenBrowserAdded = listen<BrowserInfo>('browser-added', (event) => {
      console.log('[Browser] Browser added:', event.payload)
      setBrowsers((prev) => [...prev, event.payload])
    })

    return () => {
      unlistenComment.then((fn) => fn())
      unlistenConnected.then((fn) => fn())
      unlistenDisconnected.then((fn) => fn())
      unlistenSiteAdded.then((fn) => fn())
      unlistenBrowserAdded.then((fn) => fn())
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

  const handleSiteChange = async (connectionId: string, siteId: string) => {
    if (!siteId) return
    try {
      console.log('[Connection] Setting site:', { connectionId, siteId })
      await invoke('set_connection_site', { connectionId, siteId })
      await loadConnections()
    } catch (error) {
      console.error('[Connection] Failed to set site:', error)
      alert('サイトの設定に失敗しました')
    }
  }

  const handleUrlChange = (connectionId: string, url: string) => {
    // ローカルステートのみ更新
    setConnections((prev) =>
      prev.map((conn) =>
        conn.connection_id === connectionId ? { ...conn, url } : conn
      )
    )
  }

  const handleUrlBlur = async (connectionId: string) => {
    const conn = connections.find((c) => c.connection_id === connectionId)
    if (!conn) return
    try {
      await invoke('update_connection_settings', {
        connectionId,
        url: conn.url || null,
        browserId: null,
        advancedSettings: null,
      })
    } catch (error) {
      console.error('[Connection] Failed to update URL:', error)
    }
  }

  const handleBrowserChange = async (connectionId: string, browserId: string) => {
    if (!browserId) return
    try {
      await invoke('update_connection_settings', {
        connectionId,
        url: null,
        browserId,
        advancedSettings: null,
      })
      await loadConnections()
    } catch (error) {
      console.error('[Connection] Failed to set browser:', error)
      alert('ブラウザの設定に失敗しました')
    }
  }

  const handleSendComment = async () => {
    if (!selectedConnectionForCommand) {
      alert('接続を選択してください')
      return
    }
    if (!commandInput.trim()) {
      alert('コメントを入力してください')
      return
    }

    try {
      const result = await invoke<string>('send_comment', {
        connectionId: selectedConnectionForCommand,
        text: commandInput.trim(),
      })
      console.log('Comment result:', result)
      setCommandInput('')
    } catch (error) {
      console.error('Failed to send comment:', error)
      alert(`コメント送信失敗: ${error}`)
    }
  }

  const handleCheckForUpdates = async () => {
    setCheckingUpdate(true)
    try {
      const update = await invoke<UpdateInfo | null>('check_for_updates')
      if (update) {
        setUpdateInfo(update)
        setShowUpdateDialog(true)
      } else {
        alert('最新バージョンです')
      }
    } catch (error) {
      console.error('Failed to check for updates:', error)
      alert(`更新確認失敗: ${error}`)
    } finally {
      setCheckingUpdate(false)
    }
  }

  const handleUpdateNow = async () => {
    try {
      await invoke('launch_installer')
      // インストーラが起動してmcvが終了する
    } catch (error) {
      console.error('Failed to launch installer:', error)
      alert(`インストーラ起動失敗: ${error}`)
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
            connections.map((conn) => {
              const isConnected = conn.status.type === 'Connected'
              const isDisconnected = conn.status.type === 'Disconnected' || conn.status.type === 'Created'
              const canModify = isDisconnected
              const canConnect = conn.site_id && conn.url

              return (
                <div
                  key={conn.connection_id}
                  className="p-3 bg-gray-700 rounded transition-colors space-y-2"
                >
                  {/* 接続名 */}
                  <div className="flex items-center justify-between">
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

                  {/* サイト選択 */}
                  <div>
                    <label className="text-xs text-gray-400 block mb-1">配信サイト</label>
                    <select
                      value={conn.site_id || ''}
                      onChange={(e) => handleSiteChange(conn.connection_id, e.target.value)}
                      disabled={!canModify}
                      className="w-full px-2 py-1 text-xs bg-gray-600 border border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <option value="">選択してください</option>
                      {sites.map((site) => (
                        <option key={site.site_id} value={site.site_id}>
                          {site.display_name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* URL入力 */}
                  <div>
                    <label className="text-xs text-gray-400 block mb-1">URL</label>
                    <input
                      type="text"
                      value={conn.url || ''}
                      onChange={(e) => handleUrlChange(conn.connection_id, e.target.value)}
                      onBlur={() => handleUrlBlur(conn.connection_id)}
                      disabled={!canModify}
                      placeholder="https://..."
                      className="w-full px-2 py-1 text-xs bg-gray-600 border border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                    />
                  </div>

                  {/* ブラウザ選択 */}
                  <div>
                    <label className="text-xs text-gray-400 block mb-1">ブラウザ</label>
                    <select
                      value={conn.browser_id || ''}
                      onChange={(e) => handleBrowserChange(conn.connection_id, e.target.value)}
                      disabled={!canModify}
                      className="w-full px-2 py-1 text-xs bg-gray-600 border border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      <option value="">選択してください</option>
                      {browsers.map((browser) => (
                        <option key={browser.browser_id} value={browser.browser_id}>
                          {browser.display_name}
                        </option>
                      ))}
                    </select>
                  </div>

                  {/* アクションボタン */}
                  <div className="flex gap-2 pt-1">
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleConnect(conn.connection_id)
                      }}
                      disabled={isConnected || !canConnect}
                      className="flex-1 px-2 py-1 text-xs bg-green-600 hover:bg-green-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      接続
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleDisconnect(conn.connection_id)
                      }}
                      disabled={isDisconnected}
                      className="flex-1 px-2 py-1 text-xs bg-red-600 hover:bg-red-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      切断
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleRemoveConnection(conn.connection_id)
                      }}
                      disabled={isConnected || conn.status.type === 'Connecting'}
                      className="px-2 py-1 text-xs bg-gray-600 hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      削除
                    </button>
                  </div>
                </div>
              )
            })
          )}
        </div>

        <div className="p-4 border-t border-gray-700 space-y-2">
          <div className="text-xs text-gray-500">
            <span>接続数: {connections.length}</span>
          </div>
          <button
            onClick={handleCheckForUpdates}
            disabled={checkingUpdate}
            className="w-full px-3 py-2 bg-gray-700 hover:bg-gray-600 rounded text-sm transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {checkingUpdate ? '確認中...' : '更新を確認'}
          </button>
        </div>
      </div>

      {/* メインエリア */}
      <div className="flex-1 flex flex-col">
        {/* タブヘッダー */}
        <div className="bg-gray-800 border-b border-gray-700">
          <div className="flex">
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'comments'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-400 hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('comments')}
            >
              コメント
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'logs'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-400 hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('logs')}
            >
              ログ
            </button>
          </div>
        </div>

        {/* タブコンテンツ */}
        <div className="flex-1 overflow-hidden flex flex-col">
          {activeTab === 'comments' && (
            <>
              {/* コメント表示 */}
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

              {/* コメント投稿セクション */}
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
                    <label className="block text-sm font-medium mb-1 text-gray-300">コメント</label>
                    <input
                      type="text"
                      value={commandInput}
                      onChange={(e) => setCommandInput(e.target.value)}
                      onKeyDown={(e) => {
                        if (e.key === 'Enter') {
                          handleSendComment()
                        }
                      }}
                      placeholder="例: disconnect, pause, resume, rate 3, comment 太郎 こんにちは"
                      className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded focus:outline-none focus:border-blue-500 text-white placeholder-gray-500"
                    />
                  </div>

                  <button
                    onClick={handleSendComment}
                    className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
                  >
                    送信
                  </button>
                </div>
              </div>
            </>
          )}

          {activeTab === 'logs' && <LogViewer />}
        </div>
      </div>

      {/* 更新ダイアログ */}
      {showUpdateDialog && updateInfo && (
        <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
          <div className="bg-gray-800 rounded-lg p-6 max-w-md w-full mx-4 border border-gray-700">
            <h3 className="text-xl font-bold mb-4">新しいバージョンが利用可能です</h3>
            <div className="space-y-3 mb-6">
              <div>
                <span className="text-gray-400">バージョン: </span>
                <span className="font-semibold">{updateInfo.version}</span>
              </div>
              <div>
                <span className="text-gray-400">リリース日: </span>
                <span>{new Date(updateInfo.released_at).toLocaleDateString('ja-JP')}</span>
              </div>
              <div>
                <span className="text-gray-400 block mb-1">リリースノート:</span>
                <div className="bg-gray-700 p-3 rounded text-sm whitespace-pre-wrap">
                  {updateInfo.release_notes}
                </div>
              </div>
            </div>
            <div className="flex gap-3">
              <button
                onClick={handleUpdateNow}
                className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
              >
                今すぐ更新
              </button>
              <button
                onClick={() => setShowUpdateDialog(false)}
                className="px-4 py-2 bg-gray-600 hover:bg-gray-700 rounded transition-colors"
              >
                後で
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default App
