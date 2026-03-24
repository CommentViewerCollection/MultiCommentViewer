import { useState, useEffect, useRef, useReducer, useLayoutEffect, useMemo } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'
import { DataGrid, DataGridRef, Column } from 'my-dataview'
import { SettingsScreen } from './components/SettingsScreen'
import { ColorInfo } from './utils/ColorInfo'
import { TitleBar } from './components/TitleBar'
import { type ThemeColors, PRESET_THEME_COLORS, applyThemeColors, resolveThemeColors } from './theme'
import type { UserSettings } from './types/user'
import { SearchTab } from './components/SearchTab'
import { UserListTab } from './components/UserListTab'
import type { RJSFSchema } from '@rjsf/utils'

// @ts-ignore - Type compatibility issue with React versions
const DataGridComponent = DataGrid as any

// MessagePart type matching backend structure
export type MessagePart =
  | { type: 'text'; text: string }
  | { type: 'image'; url: string; width?: number; height?: number; alt?: string }

// バッジ（ProviderBadge に対応）
export interface Badge {
  id: string
  name: string
  image_url?: string
}

// バックエンドから送られてくる表示用コメント行（main.rs の CommentRow に対応）
interface CommentRow {
  id: string
  user_name: MessagePart[]
  user_id: string
  badges: Badge[]
  text: MessagePart[]
  timestamp: number
  connection_id: string
  is_visible: boolean
  replaces_id?: string
  /** メッセージ種別: "chat" | "history_chat" | "monetary" | "system" */
  kind: string
  avatar_url?: string
  /** スーパーチャット等の金額テキスト（例: "¥8,000"）。monetary 種別のみ設定。 */
  amount_text?: string
}

// Display-friendly comment row (for DataGrid)
export interface Comment {
  id: string
  user_name: MessagePart[]
  user_id: string
  badges: Badge[]
  text: MessagePart[]
  timestamp: number
  connection_id?: string
  connection_name?: string
  backgroundColor?: string
  color?: string
  colorInfo?: ColorInfo
  is_visible: boolean
  replaces_id?: string
  /** メッセージ種別: "chat" | "history_chat" | "monetary" | "system" */
  kind: string
  avatar_url?: string
  /** スーパーチャット等の金額テキスト（例: "¥8,000"）。monetary 種別のみ設定。 */
  amount_text?: string
}


export interface AccountInfo {
  user_id: string
  display_name: string
  avatar_url?: string
}

export interface ConnectionInfo {
  connection_id: string
  plugin_id?: string
  status: { type: string; message?: string }
  site_id?: string
  site_name: string
  url?: string
  browser_id?: string
  advanced_settings?: {
    bgColor?: string      // 新規: 接続毎の背景色
    textColor?: string    // 新規: 接続毎の文字色
    [key: string]: any
  }
  input_state?: {
    url?: string
    password?: string
    [key: string]: any
  }
  input_info: string
  name: string
  account_info?: AccountInfo
}

// プラグインから送られてくる配信メタデータ
interface StreamMetadataPayload {
  connection_id: string
  title?: string
  viewer_count?: number
  total_viewer_count?: number
  start_time?: number
  others?: string
  clear?: boolean
}

// メタデータビュー用の行データ
interface MetadataRow {
  connection_id: string
  connection_name: string
  title: string
  elapsed_time: string
  viewer_count: string
  total_viewer_count: string
  others: string
}

const TWICAS_PRIVATE_SITE_ID = 'ツイキャス（プライベート）_6f3a9f72-9b0c-4e2f-8a1d-5c7e3b4d2f9a'

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
  channel: string
  fileName: string
  fileSize?: number
  sha256: string
  uploadedAt: string
}

interface RegistryPlugin {
  id: string
  name: string
  description: string
  download_count: number
  channels: {
    stable: string | null
    beta: string | null
    alpha: string | null
  }
}

interface InstalledPluginMeta {
  id: string
  version: string | null
  channel: string | null
}


type TabType = 'comments' | 'settings' | 'updates' | 'plugins' | 'search' | 'users'

// Render a single MessagePart (text or image)
function RenderMessagePart({
  part,
  isUsername = false
}: {
  part: MessagePart;
  isUsername?: boolean
}) {
  if (part.type === 'text') {
    return <span>{part.text}</span>
  }

  // Image part
  const size = isUsername ? 16 : 22  // Badges smaller than emojis
  const [error, setError] = useState(false)

  if (error) {
    // Fallback to alt text if image fails to load
    return <span className="text-gray-500 text-xs">[{part.alt || 'image'}]</span>
  }

  return (
    <img
      src={part.url}
      alt={part.alt || ''}
      title={part.alt || undefined}
      width={part.width || size}
      height={part.height || size}
      onError={() => setError(true)}
      className={`inline-block mx-0.5 ${isUsername ? 'align-middle' : 'align-text-bottom'}`}
      style={{
        maxWidth: `${size}px`,
        maxHeight: `${size}px`,
        objectFit: 'contain'
      }}
      loading="lazy"
    />
  )
}

// Render an array of MessageParts
function RenderMessageParts({
  parts,
  isUsername = false
}: {
  parts: MessagePart[];
  isUsername?: boolean
}) {
  return (
    <span className="inline-flex flex-wrap items-center gap-0.5">
      {parts.map((part, index) => (
        <RenderMessagePart
          key={index}
          part={part}
          isUsername={isUsername}
        />
      ))}
    </span>
  )
}

function App() {
  const [activeTab, setActiveTab] = useState<TabType>('comments')
  const [comments, setComments] = useState<Comment[]>([])
  const [connections, setConnections] = useState<ConnectionInfo[]>([])
  const [sites, setSites] = useState<SiteInfo[]>([])
  const [browsers, setBrowsers] = useState<BrowserInfo[]>([])
  const dataGridRef = useRef<DataGridRef>(null)
  const [isAutoScrollEnabled, setIsAutoScrollEnabled] = useState(true)
  const [editingNames, setEditingNames] = useState<{ [key: string]: string }>({})
  const [connectingIds, setConnectingIds] = useState<Set<string>>(new Set())
  const [showPasswordIds, setShowPasswordIds] = useState<Set<string>>(new Set())
  const [selectedConnectionForCommand, setSelectedConnectionForCommand] = useState<string>('')
  const [commentFormSchema, setCommentFormSchema] = useState<RJSFSchema | null>(null)
  const [commentFormData, setCommentFormData] = useState<Record<string, unknown>>({ text: '' })
  const [updateInfo, setUpdateInfo] = useState<UpdateInfo | null>(null)
  const [checkingUpdate, setCheckingUpdate] = useState(false)
  const [downloadingUpdate, setDownloadingUpdate] = useState(false)
  const [downloadedUpdatePath, setDownloadedUpdatePath] = useState<string | null>(null)
  const [updateMessage, setUpdateMessage] = useState<string>('')
  const [registryPlugins, setRegistryPlugins] = useState<RegistryPlugin[]>([])
  const [installedPlugins, setInstalledPlugins] = useState<Map<string, InstalledPluginMeta>>(new Map())
  const [pendingUpdateIds, setPendingUpdateIds] = useState<Set<string>>(new Set())
  const [pluginBusyId, setPluginBusyId] = useState<string | null>(null)
  const [selectedPluginId, setSelectedPluginId] = useState<string | null>(null)
  const [coreSettings, setCoreSettings] = useState<any>(null)
  const [currentThemeColors, setCurrentThemeColors] = useState<ThemeColors>(PRESET_THEME_COLORS['dark'])
  const [userSettings, setUserSettings] = useState<Map<string, UserSettings>>(new Map())
  const [contextMenu, setContextMenu] = useState<{ x: number; y: number; comment: Comment } | null>(null)
  const [searchQuery, setSearchQuery] = useState<string | undefined>(undefined)
  const [focusUserId, setFocusUserId] = useState<string | undefined>(undefined)
  const [metadataMap, setMetadataMap] = useState<Map<string, StreamMetadataPayload>>(new Map())
  const [elapsedTick, setElapsedTick] = useState(0)
  const [metadataHeight, setMetadataHeight] = useState(150)

  type FrontendTraceSource = {
    file: string
    line: number
    column: number
    function?: string
  }

  const sourceMapRef = useRef<Map<string, Promise<any | null>>>(new Map())

  // frontend_trace の呼び出し元を推定するため、JS stack から file/line/column を取り出す。
  const parseStackSource = (): FrontendTraceSource | null => {
    const stack = new Error().stack
    if (!stack) return null

    const lines = stack.split('\n').slice(2)
    for (const line of lines) {
      const trimmed = line.trim()

      let fnName: string | undefined
      let filePart: string | undefined
      let lineNum: number | undefined
      let colNum: number | undefined

      let m = trimmed.match(/^at\s+(.*?)\s+\((.*):(\d+):(\d+)\)$/)
      if (m) {
        fnName = m[1]
        filePart = m[2]
        lineNum = Number(m[3])
        colNum = Number(m[4])
      } else {
        m = trimmed.match(/^at\s+(.*):(\d+):(\d+)$/)
        if (m) {
          filePart = m[1]
          lineNum = Number(m[2])
          colNum = Number(m[3])
        }
      }

      if (!filePart || !lineNum || !colNum) continue

      let normalized = filePart
      if (normalized.includes('://')) {
        try {
          normalized = decodeURIComponent(new URL(normalized).pathname)
        } catch {
          // URL パースできない場合はそのまま使う
        }
      }

      if (/^\/[A-Za-z]:\//.test(normalized)) {
        normalized = normalized.slice(1)
      }
      const srcIdx = normalized.indexOf('/src/')
      if (srcIdx >= 0) {
        normalized = normalized.slice(srcIdx + 1)
      }

      return {
        file: normalized,
        line: lineNum,
        column: colNum,
        function: fnName,
      }
    }

    return null
  }

  const resolveSourceMap = async (
    source: FrontendTraceSource | null
  ): Promise<FrontendTraceSource | null> => {
    if (!source) return null
    if (source.file.startsWith('/src/') || source.file.startsWith('src/')) {
      return source
    }
    if (!source.file.startsWith('/assets/') || !source.file.endsWith('.js')) {
      return source
    }

    const mapUrl = `${source.file}.map`
    let mapPromise = sourceMapRef.current.get(mapUrl)
    if (!mapPromise) {
      mapPromise = (async () => {
        try {
          // 本番ビルドでは /assets/*.js になるため、sourcemap で元の src/*.tsx 位置へ戻す。
          const res = await fetch(mapUrl)
          if (!res.ok) return null
          const raw = await res.json()
          const { TraceMap } = await import('@jridgewell/trace-mapping')
          return new TraceMap(raw, mapUrl)
        } catch {
          return null
        }
      })()
      sourceMapRef.current.set(mapUrl, mapPromise)
    }

    const traceMap = await mapPromise
    if (!traceMap) return source

    try {
      const { originalPositionFor } = await import('@jridgewell/trace-mapping')
      const pos = originalPositionFor(traceMap, {
        line: source.line,
        column: Math.max(source.column - 1, 0),
      })
      if (!pos.source || !pos.line) return source
      return {
        file: pos.source,
        line: pos.line,
        column: (pos.column ?? 0) + 1,
        function: pos.name || source.function,
      }
    } catch {
      return source
    }
  }

  const frontendTrace = (
    level: 'trace' | 'debug' | 'info' | 'warn' | 'error',
    message: string,
    fields?: Record<string, unknown>
  ) => {
    const rawSource = parseStackSource()
    void (async () => {
      // backend 側で SourceLocation を上書きできるよう、解決後の source を明示的に渡す。
      const source = await resolveSourceMap(rawSource)
      invoke('frontend_trace', {
        level,
        message,
        fields: fields ?? null,
        source,
      }).catch(() => {
        // フロント診断ログの転送失敗は通常フローを止めない
      })
    })()
  }

  // 新規: Ref を作成
  const coreSettingsRef = useRef<any>(null)
  const connectionMapRef = useRef<Map<string, ConnectionInfo>>(new Map())
  const commentBufferRef = useRef<Comment[]>([])
  const flushTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  // サイドバー幅リサイズ（core.json で永続化）
  const [sidebarWidth, setSidebarWidth] = useState(320)
  const isResizing = useRef(false)
  const resizeStartX = useRef(0)
  const resizeStartWidth = useRef(0)
  const sidebarWidthRef = useRef(sidebarWidth)
  const sidebarWidthInitialized = useRef(false)

  // サイドバー折りたたみ（起動時は常に展開）
  const [isSidebarCollapsed, setIsSidebarCollapsed] = useState(false)

  const isResizingMetadata = useRef(false)
  const metadataResizeStartY = useRef(0)
  const metadataResizeStartHeight = useRef(0)
  const metadataHeightRef = useRef(150)
  const metadataHeightInitialized = useRef(false)

  // 新規: forceUpdate のための useReducer
  const [, forceUpdate] = useReducer(x => x + 1, 0)

  // 経過時間を1秒ごとに再計算するタイマー
  useEffect(() => {
    const timer = setInterval(() => setElapsedTick(t => t + 1), 1000)
    return () => clearInterval(timer)
  }, [])

  // sidebarWidthRef を sidebarWidth と同期
  useEffect(() => {
    sidebarWidthRef.current = sidebarWidth
  }, [sidebarWidth])

  // coreSettings が初めてロードされたとき、sidebar_width を反映（以降は変更しない）
  useEffect(() => {
    if (coreSettings && !sidebarWidthInitialized.current) {
      sidebarWidthInitialized.current = true
      const saved = coreSettings.sidebar_width
      if (typeof saved === 'number' && saved >= 200 && saved <= 600) {
        setSidebarWidth(saved)
        sidebarWidthRef.current = saved
      }
    }
  }, [coreSettings])


  // coreSettings が初めてロードされたとき、metadata_height を反映（以降は変更しない）
  useEffect(() => {
    if (coreSettings && !metadataHeightInitialized.current) {
      metadataHeightInitialized.current = true
      const saved = coreSettings.metadata_height
      if (typeof saved === 'number' && saved >= 60 && saved <= 600) {
        setMetadataHeight(saved)
        metadataHeightRef.current = saved
      }
    }
  }, [coreSettings])

  // サイドバーリサイズ: グローバルマウスイベント
  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isResizing.current) return
      const delta = e.clientX - resizeStartX.current
      const newWidth = Math.max(200, Math.min(600, resizeStartWidth.current + delta))
      setSidebarWidth(newWidth)
    }
    const handleMouseUp = () => {
      isResizing.current = false
      document.body.style.cursor = ''
      document.body.style.userSelect = ''
      // sidebar_width を core.json に保存（coreSettingsRef の他フィールドを引き継ぐ）
      invoke('update_settings', {
        target: 'core',
        data: { ...coreSettingsRef.current, sidebar_width: sidebarWidthRef.current },
      }).catch(console.error)
      // ローカルの ref も即時更新
      if (coreSettingsRef.current) {
        coreSettingsRef.current = { ...coreSettingsRef.current, sidebar_width: sidebarWidthRef.current }
      }
    }
    window.addEventListener('mousemove', handleMouseMove)
    window.addEventListener('mouseup', handleMouseUp)
    return () => {
      window.removeEventListener('mousemove', handleMouseMove)
      window.removeEventListener('mouseup', handleMouseUp)
    }
  }, [])

  // メタデータビュー高さリサイズ: グローバルマウスイベント
  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (!isResizingMetadata.current) return
      const delta = e.clientY - metadataResizeStartY.current
      const newHeight = Math.max(60, Math.min(600, metadataResizeStartHeight.current + delta))
      setMetadataHeight(newHeight)
      metadataHeightRef.current = newHeight
    }
    const handleMouseUp = () => {
      if (!isResizingMetadata.current) return
      isResizingMetadata.current = false
      document.body.style.cursor = ''
      document.body.style.userSelect = ''
      invoke('update_settings', {
        target: 'core',
        data: { ...coreSettingsRef.current, metadata_height: metadataHeightRef.current },
      }).catch(console.error)
      if (coreSettingsRef.current) {
        coreSettingsRef.current = { ...coreSettingsRef.current, metadata_height: metadataHeightRef.current }
      }
    }
    window.addEventListener('mousemove', handleMouseMove)
    window.addEventListener('mouseup', handleMouseUp)
    return () => {
      window.removeEventListener('mousemove', handleMouseMove)
      window.removeEventListener('mouseup', handleMouseUp)
    }
  }, [])

  const handleResizeMouseDown = (e: React.MouseEvent) => {
    isResizing.current = true
    resizeStartX.current = e.clientX
    resizeStartWidth.current = sidebarWidth
    document.body.style.cursor = 'col-resize'
    document.body.style.userSelect = 'none'
    e.preventDefault()
  }

  const handleToggleSidebar = () => {
    const next = !isSidebarCollapsed
    setIsSidebarCollapsed(next)
  }

  // DataGridのカラム定義
  const [columns, setColumns] = useState<Column<Comment>[]>([
    { key: 'connection_name', label: '接続', width: 150, visible: true, resizable: true },
    { key: 'avatar_url', label: 'サムネ', width: 48, visible: true, resizable: true },
    { key: 'user_name', label: 'ユーザー名', width: 150, visible: true, resizable: true },
    { key: 'text', label: 'コメント', width: 400, visible: true, resizable: true, wrap: true, hideable: false },
    { key: 'timestamp', label: '時刻', width: 150, visible: true, resizable: true },
  ])

  // 列設定の保存・復元
  interface ColumnSettings {
    order: string[]
    widths: Record<string, number>
    visibility: Record<string, boolean>
  }

  const saveColumnSettings = (cols: Column<Comment>[]) => {
    const settings: ColumnSettings = {
      order: cols.map(c => String(c.key)),
      widths: Object.fromEntries(cols.map(c => [String(c.key), c.width ?? 100])),
      visibility: Object.fromEntries(cols.map(c => [String(c.key), c.visible !== false])),
    }
    invoke('save_column_settings', { settings })
  }

  // 起動時に列設定を復元
  useEffect(() => {
    invoke<ColumnSettings | null>('get_column_settings').then((saved) => {
      if (saved) {
        setColumns((prev) => {
          const keyMap = new Map(prev.map(c => [String(c.key), c]))
          const ordered = saved.order
            .map(k => keyMap.get(k))
            .filter((c): c is Column<Comment> => c !== undefined)
          const remaining = prev.filter(c => !saved.order.includes(String(c.key)))
          return [...ordered, ...remaining].map(c => ({
            ...c,
            width: saved.widths[String(c.key)] ?? c.width,
            visible: saved.visibility[String(c.key)] ?? c.visible,
          }))
        })
      }
    }).catch(() => {})
  }, [])

  // 接続一覧を読み込む
  const loadConnections = async (): Promise<ConnectionInfo[]> => {
    try {
      const conns = await invoke<ConnectionInfo[]>('get_connections')
      setConnections(conns)
      // 削除された接続のメタデータをクリア
      setMetadataMap(prev => {
        const connIdSet = new Set(conns.map(c => c.connection_id))
        const next = new Map(prev)
        for (const key of next.keys()) {
          if (!connIdSet.has(key)) next.delete(key)
        }
        return next
      })
      // 編集中の名前を初期化（既に編集中のものは保持）
      setEditingNames((prev) => {
        const newEditingNames: { [key: string]: string } = {}
        conns.forEach((conn) => {
          newEditingNames[conn.connection_id] = prev[conn.connection_id] ?? conn.name
        })
        return newEditingNames
      })
      return conns
    } catch (error) {
      console.error('Failed to load connections:', error)
      return []
    }
  }

  const prefetchAccountInfoForConnections = async (
    conns: ConnectionInfo[],
    reason: string
  ) => {
    const targets = conns.filter((c) => c.plugin_id && c.site_id && c.browser_id && !c.account_info)
    frontendTrace('info', 'startup account-info prefetch check', {
      reason,
      connectionCount: conns.length,
      targetCount: targets.length,
    })
    await Promise.allSettled(
      targets.map((conn) =>
        invoke('fetch_account_info', { connectionId: conn.connection_id })
          .then(() =>
            frontendTrace('info', 'fetch_account_info invoked from startup preload', {
              reason,
              connectionId: conn.connection_id,
              siteId: conn.site_id,
              browserId: conn.browser_id,
            })
          )
          .catch((e) =>
            frontendTrace('warn', 'fetch_account_info failed from startup preload', {
              reason,
              connectionId: conn.connection_id,
              error: String(e),
            })
          )
      )
    )
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

  // Core設定を読み込む
  const loadCoreSettings = async () => {
    try {
      const settings = await invoke('get_settings', { target: 'core' })
      setCoreSettings(settings)
      forceUpdate()  // DataGrid を強制的に再レンダリング
    } catch (error) {
      console.error('Failed to load core settings:', error)
    }
  }

  // 新規: Ref を state と同期
  useEffect(() => {
    coreSettingsRef.current = coreSettings
  }, [coreSettings])

  // テーマをhtmlタグに適用（coreSettings が null の場合はダークがデフォルト）
  useEffect(() => {
    const theme = coreSettings?.theme ?? 'dark'
    const isDark = theme !== 'light'
    document.documentElement.classList.toggle('dark', isDark)
    const colors = resolveThemeColors(theme, coreSettings?.custom_theme_colors)
    applyThemeColors(colors)
    setCurrentThemeColors(colors)
  }, [coreSettings])

  // AutoScroll の真のON/OFF状態（設定値）を同期
  useEffect(() => {
    setIsAutoScrollEnabled(coreSettings?.auto_scroll ?? true)
  }, [coreSettings?.auto_scroll])

  const handleAtBottomChange = (isAtBottom: boolean) => {
    if (!isAtBottom) return
    // 設定で auto_scroll が無効化されている場合は自動復帰しない
    if ((coreSettings?.auto_scroll ?? true) === false) return
    setIsAutoScrollEnabled(true)
  }

  useEffect(() => {
    // Connection Map を作成（O(1) 検索のため）
    const map = new Map<string, ConnectionInfo>()
    connections.forEach(conn => {
      map.set(conn.connection_id, conn)
    })
    connectionMapRef.current = map
  }, [connections])

  // ユーザー設定（ニックネーム・mcvNG）を読み込む
  const loadUserSettings = async () => {
    try {
      const rawSettings = await invoke<Record<string, UserSettings> | null>(
        'get_settings', { target: 'user_settings' }
      )
      if (rawSettings) {
        setUserSettings(new Map(Object.entries(rawSettings)))
      }
    } catch (error) {
      console.error('Failed to load user settings:', error)
    }
  }

  useEffect(() => {
    // 初回読み込み
    void (async () => {
      const initialConnections = await loadConnections()
      await prefetchAccountInfoForConnections(initialConnections, 'initial-load')
    })()
    // 起動直後は plugin_id が遅れて反映される場合があるため、短時間だけ再試行する
    const startupRetryTimers = [1000, 3000, 5000].map((delayMs) =>
      setTimeout(() => {
        void (async () => {
          const latest = await loadConnections()
          await prefetchAccountInfoForConnections(latest, `startup-retry-${delayMs}ms`)
        })()
      }, delayMs)
    )
    loadSitesAndBrowsers()
    loadCoreSettings()
    loadUserSettings()

    // コメント受信イベントをリッスン（CommentRow[] を受け取る）
    // バッファに蓄積して一定間隔でまとめて反映することで UI フリーズを防ぐ
    const unlistenComment = listen<CommentRow[]>('comment-received', (event) => {
      for (const row of event.payload) {
        const colorInfo = new ColorInfo(coreSettingsRef, connectionMapRef, row.connection_id)
        commentBufferRef.current.push({ ...row, colorInfo })
      }
      if (flushTimerRef.current === null) {
        flushTimerRef.current = setTimeout(() => {
          const batch = commentBufferRef.current.splice(0)
          flushTimerRef.current = null
          if (batch.length === 0) return
          setComments((prev) => {
            let updated = [...prev]
            const newComments: Comment[] = []

            for (const comment of batch) {
              if (comment.replaces_id) {
                // Replace（承認）または Delete（特定コメント削除）
                const idx = updated.findIndex(c => c.id === comment.replaces_id)
                if (idx !== -1) {
                  updated[idx] = { ...comment, id: comment.replaces_id }
                } else {
                  // 同フラッシュバッチ内の未コミット分を検索
                  const buffIdx = newComments.findIndex(c => c.id === comment.replaces_id)
                  if (buffIdx !== -1) {
                    newComments[buffIdx] = { ...comment, id: comment.replaces_id }
                  } else if (comment.is_visible) {
                    // Replace でプレースホルダーが見つからない場合のみ通常コメントとして追加
                    // Delete（is_visible: false）の場合は対象が既に存在しないため何もしない
                    newComments.push(comment)
                  }
                }
              } else {
                newComments.push(comment)
              }
            }

            return [...updated, ...newComments]
          })
        }, 32)
      }
    })

    // ユーザー全コメント削除イベントをリッスン（BAN 等）
    const unlistenDeleteAll = listen<{ user_id: string; connection_id: string }>(
      'delete-all-by-user',
      (event) => {
        const { user_id, connection_id } = event.payload
        setComments((prev) =>
          prev.map((c) =>
            c.user_id === user_id && c.connection_id === connection_id
              ? { ...c, is_visible: false }
              : c
          )
        )
      }
    )

    // 接続完了イベントをリッスン
    const unlistenConnected = listen<{ connection_id: string }>('connected', (event) => {
      setConnectingIds((prev) => {
        const next = new Set(prev)
        next.delete(event.payload.connection_id)
        return next
      })
      loadConnections()
    })

    // 切断完了イベントをリッスン
    const unlistenDisconnected = listen<{ connection_id: string }>('disconnected', (event) => {
      setConnectingIds((prev) => {
        const next = new Set(prev)
        next.delete(event.payload.connection_id)
        return next
      })
      setMetadataMap((prev) => {
        const next = new Map(prev)
        const existing = next.get(event.payload.connection_id)
        if (existing) {
          const { start_time: _, ...rest } = existing
          next.set(event.payload.connection_id, rest)
        }
        return next
      })
      loadConnections()
    })

    // 接続失敗イベントをリッスン
    const unlistenConnectFailed = listen<{ connection_id: string; reason: string }>('connect-failed', (event) => {
      const { connection_id, reason } = event.payload
      setConnectingIds((prev) => {
        const next = new Set(prev)
        next.delete(connection_id)
        return next
      })
      loadConnections()
      // エラーをコメント欄にシステムメッセージとして表示
      const errorComment: Comment = {
        id: `connect-failed-${connection_id}-${Date.now()}`,
        user_name: [{ type: 'text', text: 'システム' }],
        user_id: '',
        badges: [],
        text: [{ type: 'text', text: `接続に失敗しました: ${reason}` }],
        timestamp: Date.now(),
        connection_id,
        is_visible: true,
        kind: 'system',
      }
      setComments((prev) => [...prev, errorComment])
    })

    // サイト追加イベントをリッスン
    const unlistenSiteAdded = listen<SiteInfo>('site-added', (event) => {
      frontendTrace('info', 'site-added event received', { payload: event.payload })
      setSites((prev) => {
        if (prev.some((s) => s.site_id === event.payload.site_id)) {
          return prev
        }
        return [...prev, event.payload]
      })
      // サイト登録完了後にPending接続がCreatedに変わっている可能性があるため再取得
      loadConnections()
    })

    // ブラウザ追加イベントをリッスン
    const unlistenBrowserAdded = listen<BrowserInfo>('browser-added', (event) => {
      frontendTrace('info', 'browser-added event received', { payload: event.payload })
      setBrowsers((prev) => {
        if (prev.some((b) => b.browser_id === event.payload.browser_id)) {
          return prev
        }
        return [...prev, event.payload]
      })
    })

    // ブラウザ削除イベントをリッスン
    const unlistenBrowserRemoved = listen<{ browser_id: string }>('browser-removed', (event) => {
      frontendTrace('info', 'browser-removed event received', { payload: event.payload })
      setBrowsers((prev) => prev.filter((b) => b.browser_id !== event.payload.browser_id))
    })

    // アカウント情報更新イベントをリッスン
    const unlistenAccountUpdated = listen('connection-account-updated', () => {
      loadConnections()
    })

    // 配信メタデータ更新イベントをリッスン
    const unlistenStreamMetadata = listen<StreamMetadataPayload>('stream-metadata', (event) => {
      const p = event.payload
      setMetadataMap(prev => {
        const next = new Map(prev)
        // clear: true の場合は既存値を破棄してからマージ（配信終了など全フィールドリセット）
        const base = p.clear ? {} : (next.get(p.connection_id) ?? {})
        next.set(p.connection_id, { ...base, ...p })
        return next
      })
    })

    return () => {
      unlistenComment.then((fn) => fn())
      unlistenDeleteAll.then((fn) => fn())
      unlistenConnected.then((fn) => fn())
      unlistenDisconnected.then((fn) => fn())
      unlistenConnectFailed.then((fn) => fn())
      unlistenSiteAdded.then((fn) => fn())
      unlistenBrowserAdded.then((fn) => fn())
      unlistenBrowserRemoved.then((fn) => fn())
      unlistenAccountUpdated.then((fn) => fn())
      unlistenStreamMetadata.then((fn) => fn())
      startupRetryTimers.forEach((timerId) => clearTimeout(timerId))
      if (flushTimerRef.current !== null) {
        clearTimeout(flushTimerRef.current)
        flushTimerRef.current = null
      }
    }
  }, [])

  useEffect(() => {
    const startupCheck = async () => {
      setCheckingUpdate(true)
      try {
        const update = await invoke<UpdateInfo | null>('check_for_updates')
        if (update) {
          setUpdateInfo(update)
          setActiveTab('updates')
          setUpdateMessage(`新しいバージョン ${update.version} (${update.channel}) が利用可能です。`)
        } else {
          setUpdateMessage('最新バージョンです。')
        }
      } catch (error) {
        console.error('Failed startup update check:', error)
      } finally {
        setCheckingUpdate(false)
      }
    }
    startupCheck()
  }, [])

  useEffect(() => {
    if (activeTab === 'plugins') {
      loadRegistryPlugins()
    }
  }, [activeTab])

  const metadataRows = useMemo<MetadataRow[]>(() => {
    const formatElapsed = (startTime?: number): string => {
      if (startTime == null) return '-'
      const elapsed = Math.max(0, Math.floor(Date.now() / 1000) - startTime)
      const h = Math.floor(elapsed / 3600)
      const m = Math.floor((elapsed % 3600) / 60)
      const s = elapsed % 60
      if (h > 0) return `${h}:${String(m).padStart(2, '0')}:${String(s).padStart(2, '0')}`
      return `${m}:${String(s).padStart(2, '0')}`
    }
    return connections.map(conn => {
      const meta = metadataMap.get(conn.connection_id)
      return {
        connection_id: conn.connection_id,
        connection_name: conn.name,
        title: meta?.title ?? '-',
        elapsed_time: formatElapsed(meta?.start_time),
        viewer_count: meta?.viewer_count != null ? String(meta.viewer_count) : '-',
        total_viewer_count: meta?.total_viewer_count != null ? String(meta.total_viewer_count) : '-',
        others: meta?.others ?? '-',
      }
    })
  }, [connections, metadataMap, elapsedTick])

  const metadataColumns: Column<MetadataRow>[] = [
    { key: 'connection_name',   label: '接続名',   width: 120, visible: true, resizable: true },
    { key: 'title',             label: 'タイトル', width: 300, visible: true, resizable: true },
    { key: 'elapsed_time',      label: '経過時間', width: 90,  visible: true, resizable: true },
    { key: 'viewer_count',      label: '視聴者数', width: 90,  visible: true, resizable: true },
    { key: 'total_viewer_count',label: '総視聴者数', width: 100, visible: true, resizable: true },
    { key: 'others',            label: 'その他',   width: 200, visible: true, resizable: true },
  ]

  const renderMetadataCell = (item: MetadataRow, column: Column<MetadataRow>) => (
    <span>{String(item[column.key as keyof MetadataRow])}</span>
  )

  const visibleComments = useMemo(
    () => comments.filter(c => {
      if (c.is_visible === false) return false
      if (userSettings.get(c.user_id)?.is_mcv_ng) return false
      return true
    }),
    [comments, userSettings]
  )

  // AutoScroll ON に切り替わったら即座に末尾へ移動
  useEffect(() => {
    if (!isAutoScrollEnabled) return
    const raf = requestAnimationFrame(() => {
      dataGridRef.current?.scrollToBottom()
    })
    return () => cancelAnimationFrame(raf)
  }, [isAutoScrollEnabled])

  // 新規データ反映後に Bottom 追従（setComments直後ではなく描画後に実行）
  // atBottom の真偽に関わらず、AutoScroll ON のときは件数変化で必ず追従させる
  // （複数行追加時に仮想リストの表示更新が遅延するケースを防ぐ）
  useLayoutEffect(() => {
    if (!isAutoScrollEnabled || visibleComments.length === 0) return
    const raf = requestAnimationFrame(() => {
      dataGridRef.current?.scrollToBottom()
    })
    return () => cancelAnimationFrame(raf)
  }, [visibleComments.length, isAutoScrollEnabled])

  // ユーザー設定の全体をJSONファイルに保存
  const saveUserSettings = async (next: Map<string, UserSettings>) => {
    try {
      await invoke('update_settings', {
        target: 'user_settings',
        data: Object.fromEntries(next),
      })
    } catch (error) {
      console.error('Failed to save user settings:', error)
    }
  }

  const handleUserNicknameChange = (userId: string, nickname: string) => {
    setUserSettings(prev => {
      const next = new Map(prev)
      const ex = next.get(userId) ?? { nickname: '', is_mcv_ng: false }
      next.set(userId, { ...ex, nickname })
      saveUserSettings(next)
      return next
    })
  }

  const handleUserMcvNgChange = (userId: string, isNg: boolean) => {
    setUserSettings(prev => {
      const next = new Map(prev)
      const ex = next.get(userId) ?? { nickname: '', is_mcv_ng: false }
      next.set(userId, { ...ex, is_mcv_ng: isNg })
      saveUserSettings(next)
      return next
    })
  }

  const handleRowContextMenu = (comment: Comment, event: React.MouseEvent) => {
    setContextMenu({ x: event.clientX, y: event.clientY, comment })
  }

  const handleContextMenuSearch = () => {
    if (!contextMenu) return
    setSearchQuery(contextMenu.comment.user_id)
    setActiveTab('search')
    setContextMenu(null)
  }

  const handleContextMenuViewUser = () => {
    if (!contextMenu) return
    setFocusUserId(contextMenu.comment.user_id)
    setActiveTab('users')
    setContextMenu(null)
  }

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
    setConnectingIds((prev) => new Set(prev).add(connectionId))
    try {
      await invoke('connect', { connectionId })
      // loadConnections()はconnectedイベントで自動実行される
    } catch (error) {
      console.error('Failed to connect:', error)
      setConnectingIds((prev) => {
        const next = new Set(prev)
        next.delete(connectionId)
        return next
      })
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
      frontendTrace('info', 'set_connection_site start', { connectionId, siteId })
      await invoke('set_connection_site', { connectionId, siteId })
      // loadConnections() はURLなどのローカル編集を上書きするため使わず、
      // site_id のみをローカルステートに反映する
      const browserId = connections.find((c) => c.connection_id === connectionId)?.browser_id
      setConnections((prev) =>
        prev.map((c) => (c.connection_id === connectionId ? { ...c, site_id: siteId } : c))
      )
      frontendTrace('info', 'site changed, fetch-account precheck', {
        connectionId,
        siteId,
        browserId,
        willFetch: !!(siteId && browserId),
      })
      if (siteId && browserId) {
        invoke('fetch_account_info', { connectionId })
          .then(() => frontendTrace('info', 'fetch_account_info invoked from site change', { connectionId }))
          .catch((e) => frontendTrace('warn', 'fetch_account_info failed from site change', { connectionId, error: String(e) }))
      }
    } catch (error) {
      frontendTrace('error', 'set_connection_site failed', {
        connectionId,
        siteId,
        error: String(error),
      })
      console.error('[Connection] Failed to set site:', error)
      alert('サイトの設定に失敗しました')
    }
  }

  const handleUrlChange = (connectionId: string, url: string) => {
    setConnections((prev) =>
      prev.map((conn) =>
        conn.connection_id === connectionId ? { ...conn, url } : conn
      )
    )
  }

  const handleUrlPaste = (connectionId: string, e: React.ClipboardEvent<HTMLInputElement>) => {
    const input = e.currentTarget
    // onPaste 時点では input.value にペースト後の値がまだ反映されていないため
    // 1tick待ってから取得する
    setTimeout(async () => {
      const url = input.value
      try {
        await invoke('update_connection_settings', {
          connectionId,
          url: url || null,
          browserId: null,
          advancedSettings: null,
        })
        if (url) {
          const detectedSiteId = await invoke<string | null>('detect_url', { url })
          if (detectedSiteId) {
            await invoke('set_connection_site', { connectionId, siteId: detectedSiteId })
            await loadConnections()
          }
        }
      } catch (error) {
        console.error('[Connection] Failed to detect URL:', error)
      }
    }, 0)
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

  const handlePasswordChange = (connectionId: string, password: string) => {
    setConnections((prev) =>
      prev.map((conn) =>
        conn.connection_id === connectionId
          ? { ...conn, input_state: { ...conn.input_state, password } }
          : conn
      )
    )
  }

  const handlePasswordBlur = async (connectionId: string) => {
    const conn = connections.find((c) => c.connection_id === connectionId)
    if (!conn) return
    try {
      await invoke('update_connection_settings', {
        connectionId,
        url: null,
        browserId: null,
        advancedSettings: null,
        inputState: { password: conn.input_state?.password ?? null },
      })
    } catch (error) {
      console.error('[Connection] Failed to update password:', error)
    }
  }

  const handleBrowserChange = async (connectionId: string, browserId: string) => {
    try {
      frontendTrace('info', 'update_connection_settings(browser) start', { connectionId, browserId })
      await invoke('update_connection_settings', {
        connectionId,
        url: null,
        browserId,
        advancedSettings: null,
      })
      const latest = await loadConnections()
      const updatedConn = latest.find((c) => c.connection_id === connectionId)
      frontendTrace('info', 'browser changed, fetch-account precheck', {
        connectionId,
        siteId: updatedConn?.site_id,
        browserId: updatedConn?.browser_id,
        willFetch: !!(updatedConn?.site_id && updatedConn?.browser_id),
      })
      if (updatedConn?.site_id && updatedConn?.browser_id) {
        invoke('fetch_account_info', { connectionId })
          .then(() => frontendTrace('info', 'fetch_account_info invoked from browser change', { connectionId }))
          .catch((e) => frontendTrace('warn', 'fetch_account_info failed from browser change', { connectionId, error: String(e) }))
      }
    } catch (error) {
      frontendTrace('error', 'update_connection_settings(browser) failed', {
        connectionId,
        browserId,
        error: String(error),
      })
      console.error('[Connection] Failed to set browser:', error)
      alert('ブラウザの設定に失敗しました')
    }
  }

  const handleConnectionColorChange = async (
    connectionId: string,
    colorType: 'bgColor' | 'textColor',
    value: string
  ) => {
    // バリデーション: 色の値が正しい形式か確認（#RRGGBB）
    const colorRegex = /^#[0-9A-Fa-f]{6}$/
    if (!colorRegex.test(value)) {
      console.warn('[Connection] Invalid color format:', value)
      // 不正な値の場合は何もしない
      return
    }

    // 即座にローカル状態を更新（UIのレスポンス性向上）
    setConnections((prev) =>
      prev.map((conn) => {
        if (conn.connection_id === connectionId) {
          return {
            ...conn,
            advanced_settings: {
              ...conn.advanced_settings,
              [colorType]: value,
            },
          }
        }
        return conn
      })
    )

    // バックエンドに永続化
    try {
      const conn = connections.find((c) => c.connection_id === connectionId)
      await invoke('update_connection_settings', {
        connectionId,
        url: null,
        browserId: null,
        advancedSettings: {
          ...conn?.advanced_settings,
          [colorType]: value,
        },
      })
    } catch (error) {
      console.error('[Connection] Failed to update connection color:', error)
    }
  }

  const handleSendComment = async () => {
    if (!selectedConnectionForCommand) {
      alert('接続を選択してください')
      return
    }
    const text = (commentFormData.text as string | undefined)?.trim() ?? ''
    if (!text) {
      alert('コメントを入力してください')
      return
    }

    // text 以外のフィールドを extra として送信
    const { text: _text, ...extra } = commentFormData

    try {
      const result = await invoke<string>('send_comment', {
        connectionId: selectedConnectionForCommand,
        text,
        extra: Object.keys(extra).length > 0 ? extra : null,
      })
      frontendTrace('info', 'send_comment completed', {
        connectionId: selectedConnectionForCommand,
        result,
      })
      // テキストだけリセット（anonymous 等の設定は保持）
      setCommentFormData(prev => ({ ...prev, text: '' }))
    } catch (error) {
      console.error('Failed to send comment:', error)
      alert(`コメント送信失敗: ${error}`)
    }
  }

  // 接続が変わったらコメントフォームスキーマを取得
  useEffect(() => {
    if (!selectedConnectionForCommand) {
      setCommentFormSchema(null)
      setCommentFormData({ text: '' })
      return
    }
    invoke<{ schema: RJSFSchema }>('get_comment_schema', {
      connectionId: selectedConnectionForCommand,
    })
      .then(payload => {
        setCommentFormSchema(payload.schema)
        setCommentFormData({ text: '' })
      })
      .catch(() => {
        setCommentFormSchema(null)
        setCommentFormData({ text: '' })
      })
  }, [selectedConnectionForCommand])

  const handleCheckForUpdates = async () => {
    setCheckingUpdate(true)
    try {
      const update = await invoke<UpdateInfo | null>('check_for_updates')
      if (update) {
        setUpdateInfo(update)
        setActiveTab('updates')
        setUpdateMessage(`新しいバージョン ${update.version} (${update.channel}) が利用可能です。`)
      } else {
        setUpdateInfo(null)
        setUpdateMessage('最新バージョンです。')
      }
    } catch (error) {
      console.error('Failed to check for updates:', error)
      setUpdateMessage(`更新確認失敗: ${error}`)
    } finally {
      setCheckingUpdate(false)
    }
  }

  const handleDownloadUpdate = async () => {
    if (!updateInfo) return
    setDownloadingUpdate(true)
    setUpdateMessage('アップデートをダウンロード中...')
    try {
      const zipPath = await invoke<string>('download_core_update', {
        version: updateInfo.version,
        channel: updateInfo.channel,
        sha256: updateInfo.sha256,
      })
      setDownloadedUpdatePath(zipPath)
      setUpdateMessage('ダウンロード完了。適用して再起動できます。')
    } catch (error) {
      console.error('Failed to download update:', error)
      setUpdateMessage(`アップデートのダウンロードに失敗しました: ${error}`)
    } finally {
      setDownloadingUpdate(false)
    }
  }

  const handleApplyUpdate = async () => {
    if (!downloadedUpdatePath) return
    try {
      await invoke('apply_core_update', { zipPath: downloadedUpdatePath })
    } catch (error) {
      console.error('Failed to apply update:', error)
      setUpdateMessage(`アップデート適用失敗: ${error}`)
    }
  }

  const compareVersions = (a: string, b: string): number => {
    const aParts = a.split('.').map(Number)
    const bParts = b.split('.').map(Number)
    for (let i = 0; i < Math.max(aParts.length, bParts.length); i++) {
      const aNum = aParts[i] ?? 0
      const bNum = bParts[i] ?? 0
      if (aNum !== bNum) return aNum - bNum
    }
    return 0
  }

  const getPreferredChannel = (plugin: RegistryPlugin): { channel: string; version: string } | null => {
    if (plugin.channels.stable) return { channel: 'stable', version: plugin.channels.stable }
    if (plugin.channels.beta) return { channel: 'beta', version: plugin.channels.beta }
    if (plugin.channels.alpha) return { channel: 'alpha', version: plugin.channels.alpha }
    return null
  }

  // アップデート対象チャンネルを返す
  // インストール済みの channel（plugin.json の channel フィールド）を厳守し、チャンネルを跨いだ移行は行わない。
  // channel 情報がない場合（手動インストール等）のみ優先チャンネルにフォールバック。
  const getUpdateTarget = (plugin: RegistryPlugin): { channel: string; version: string } | null => {
    const installed = installedPlugins.get(plugin.id)
    if (installed?.channel) {
      const ver = plugin.channels[installed.channel as keyof typeof plugin.channels]
      if (ver) return { channel: installed.channel, version: ver }
      // 同チャンネルのバージョンがレジストリにない場合はアップデートなし
      return null
    }
    return getPreferredChannel(plugin)
  }

  const loadRegistryPlugins = async () => {
    try {
      const [registry, installedList] = await Promise.all([
        invoke<RegistryPlugin[]>('list_registry_plugins'),
        invoke<InstalledPluginMeta[]>('list_installed_plugins'),
      ])

      const installedMap = new Map(installedList.map(m => [m.id, m]))
      setInstalledPlugins(installedMap)

      // レジストリに存在しないローカルインストール済みプラグインを追加
      const registryIdSet = new Set(registry.map(p => p.id))
      const localOnlyPlugins: RegistryPlugin[] = installedList
        .filter(m => !registryIdSet.has(m.id))
        .map(m => ({
          id: m.id,
          name: m.id,
          description: '(レジストリ未登録)',
          download_count: 0,
          channels: { stable: null, beta: null, alpha: null },
        }))

      const allPlugins = [...registry, ...localOnlyPlugins]
      const sorted = allPlugins.sort((a, b) => {
        const aInstalled = installedMap.has(a.id)
        const bInstalled = installedMap.has(b.id)
        if (aInstalled !== bInstalled) return aInstalled ? -1 : 1
        if (a.download_count !== b.download_count) return b.download_count - a.download_count
        return a.name.localeCompare(b.name, 'ja')
      })
      setRegistryPlugins(sorted)
    } catch (error) {
      console.error('Failed to load registry plugins:', error)
    }
  }

  const handleInstallPlugin = async (plugin: RegistryPlugin) => {
    const target = getPreferredChannel(plugin)
    if (!target) return
    setPluginBusyId(plugin.id)
    try {
      await invoke('install_registry_plugin', {
        pluginId: plugin.id,
        version: target.version,
        channel: target.channel,
      })
      await loadRegistryPlugins()
      await loadSitesAndBrowsers()
    } catch (error) {
      console.error('Failed to install plugin:', error)
      alert(`プラグインインストール失敗: ${error}`)
    } finally {
      setPluginBusyId(null)
    }
  }

  const isUpdateAvailable = (plugin: RegistryPlugin): boolean => {
    const installed = installedPlugins.get(plugin.id)
    if (!installed?.version) return false
    const target = getUpdateTarget(plugin)
    if (!target) return false
    return compareVersions(target.version, installed.version) > 0
  }

  const handleUpdatePlugin = async (plugin: RegistryPlugin) => {
    const target = getUpdateTarget(plugin)
    if (!target) return
    setPluginBusyId(plugin.id)
    try {
      await invoke('install_registry_plugin', {
        pluginId: plugin.id,
        version: target.version,
        channel: target.channel,
      })
      setPendingUpdateIds(prev => new Set(prev).add(plugin.id))
      await loadRegistryPlugins()
      await loadSitesAndBrowsers()
    } catch (error) {
      console.error('Failed to update plugin:', error)
      alert(`プラグインアップデート失敗: ${error}`)
    } finally {
      setPluginBusyId(null)
    }
  }

  const handleUninstallPlugin = async (plugin: RegistryPlugin) => {
    setPluginBusyId(plugin.id)
    try {
      await invoke('uninstall_registry_plugin', { pluginId: plugin.id })
      await loadRegistryPlugins()
      await loadSitesAndBrowsers()
    } catch (error) {
      console.error('Failed to uninstall plugin:', error)
      alert(`プラグインアンインストール失敗: ${error}`)
    } finally {
      setPluginBusyId(null)
    }
  }

  const formatTime = (timestamp: number) => {
    const date = new Date(timestamp * 1000)
    return date.toLocaleTimeString('ja-JP')
  }

  const handleColumnResize = (columnKey: string, width: number) => {
    const newCols = columns.map((col) => (col.key === columnKey ? { ...col, width } : col))
    setColumns(newCols)
    saveColumnSettings(newCols)
  }

  const handleColumnVisibilityChange = (columnKey: string, visible: boolean) => {
    const newCols = columns.map((col) => (col.key === columnKey ? { ...col, visible } : col))
    setColumns(newCols)
    saveColumnSettings(newCols)
  }

  const handleColumnOrderChange = (newColumns: Column<Comment>[]) => {
    setColumns(newColumns)
    saveColumnSettings(newColumns)
  }

  const kindCellClass = (kind: string) => {
    if (kind === 'monetary') return 'text-yellow-700 dark:text-yellow-300 italic'
    if (kind === 'system') return 'text-gray-500 dark:text-gray-400 italic'
    if (kind === 'history_chat') return ''
    return ''
  }

  const renderCell = (item: Comment, column: Column<Comment>) => {
    const extraClass = kindCellClass(item.kind)
    if (column.key === 'timestamp') {
      return <span className={extraClass}>{formatTime(item.timestamp)}</span>
    }
    if (column.key === 'connection_name') {
      // 接続名を動的に取得（名前変更に連動、空欄も許容）
      const conn = connections.find(c => c.connection_id === item.connection_id)
      return <span className={extraClass}>{conn?.name ?? ''}</span>
    }
    if (column.key === 'avatar_url') {
      return item.avatar_url ? (
        <img
          src={item.avatar_url}
          alt=""
          width={32}
          height={32}
          className="rounded-full"
          style={{ width: '32px', height: '32px', objectFit: 'cover' }}
        />
      ) : null
    }
    if (column.key === 'user_name') {
      return (
        <span className={extraClass}>
          <RenderMessageParts parts={item.user_name} isUsername={true} />
          {item.badges?.map((badge, i) =>
            badge.image_url ? (
              <img
                key={i}
                src={badge.image_url}
                alt={badge.name}
                title={badge.name}
                width={16}
                height={16}
                className="inline-block align-middle ml-0.5"
                style={{ maxWidth: '16px', maxHeight: '16px' }}
              />
            ) : null
          )}
        </span>
      )
    }
    if (column.key === 'text') {
      if (item.kind === 'monetary' && item.amount_text) {
        return (
          <span className={extraClass}>
            <span className="font-bold mr-1">{item.amount_text}</span>
            <RenderMessageParts parts={item.text} isUsername={false} />
          </span>
        )
      }
      return (
        <span className={extraClass}>
          <RenderMessageParts parts={item.text} isUsername={false} />
        </span>
      )
    }
    return <span className={extraClass}>{String(item[column.key])}</span>
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
    <div
      className="h-screen bg-gray-50 dark:bg-gray-900 text-gray-900 dark:text-white flex flex-col overflow-hidden"
      onClick={() => contextMenu && setContextMenu(null)}
    >
      <TitleBar themeColors={currentThemeColors} />
      <div className="flex-1 flex overflow-hidden min-h-0">
      {/* サイドバー: 接続一覧 */}
      <div
        style={{ width: isSidebarCollapsed ? 32 : sidebarWidth }}
        className="shrink-0 bg-white dark:bg-gray-800 border-r border-gray-200 dark:border-gray-700 flex flex-col h-full relative overflow-hidden"
      >
        {isSidebarCollapsed ? (
          /* 折りたたみ時: 展開ボタンのみ */
          <div className="flex flex-col items-center pt-3">
            <button
              onClick={handleToggleSidebar}
              title="接続一覧を展開"
              className="w-6 h-6 flex items-center justify-center text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors text-xl leading-none"
            >
              ›
            </button>
          </div>
        ) : (
        <>
        <div className="p-4 border-b border-gray-200 dark:border-gray-700">
          <div className="flex items-center gap-2">
            <button
              onClick={handleAddConnection}
              className="flex-1 px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
            >
              + 接続を追加
            </button>
            <button
              onClick={handleToggleSidebar}
              title="接続一覧を折りたたむ"
              className="w-6 h-6 flex items-center justify-center text-gray-500 dark:text-gray-400 hover:text-gray-900 dark:hover:text-white hover:bg-gray-100 dark:hover:bg-gray-700 rounded transition-colors text-xl leading-none"
            >
              ‹
            </button>
          </div>
        </div>

        <div className="flex-1 overflow-y-scroll p-4 space-y-2">
          <h2 className="text-sm font-semibold text-gray-500 dark:text-gray-400 mb-2">接続一覧</h2>
          {connections.length === 0 ? (
            <div className="text-center py-8 text-gray-600 dark:text-gray-500 text-sm">
              接続がありません
            </div>
          ) : (
            connections.map((conn) => {
              const isConnected = conn.status.type === 'Connected'
              const isDisconnected = conn.status.type === 'Disconnected' || conn.status.type === 'Created'
              const isConnecting = connectingIds.has(conn.connection_id)
              const canModify = isDisconnected
              const canModifyUrl = true  // URLは常に編集可能
              const canModifyColors = true  // 色は常に編集可能
              const canConnect = conn.site_id && conn.url

              return (
                <div
                  key={conn.connection_id}
                  className="p-3 bg-gray-100 dark:bg-gray-700 rounded transition-colors space-y-1.5"
                >
                  {/* 接続名 */}
                  <div className="flex items-center justify-between">
                    <input
                      type="text"
                      value={editingNames[conn.connection_id] ?? conn.name}
                      onChange={(e) => handleNameChange(conn.connection_id, e.target.value)}
                      onBlur={() => handleNameBlur(conn.connection_id)}
                      onClick={(e) => e.stopPropagation()}
                      className="font-semibold text-sm bg-transparent border-b border-transparent hover:border-gray-400 dark:hover:border-gray-500 focus:border-blue-500 focus:outline-none flex-1 mr-2"
                    />
                    <div className="flex items-center gap-2">
                      <div
                        className={`w-2 h-2 rounded-full ${getStatusColor(conn.status)}`}
                      />
                      <span className="text-xs text-gray-500 dark:text-gray-400">
                        {getStatusText(conn.status)}
                      </span>
                    </div>
                  </div>

                  {/* アカウント情報 */}
                  {conn.account_info && (
                    <div className="flex items-center gap-2 px-2 py-1.5 rounded bg-black/5 dark:bg-white/5">
                      {conn.account_info.avatar_url ? (
                        <img
                          src={conn.account_info.avatar_url}
                          alt=""
                          className="w-6 h-6 rounded-full object-cover flex-shrink-0"
                        />
                      ) : (
                        <div className="w-6 h-6 rounded-full bg-gray-400 flex items-center justify-center text-white text-xs flex-shrink-0">
                          {conn.account_info.display_name[0]}
                        </div>
                      )}
                      <span className="text-xs truncate text-gray-700 dark:text-gray-300">
                        {conn.account_info.display_name}
                      </span>
                    </div>
                  )}

                  {/* サイト選択 + ブラウザ選択 */}
                  <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.5rem' }}>
                    <div>
                      <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">配信サイト</label>
                      <select
                        value={conn.site_id || ''}
                        onChange={(e) => handleSiteChange(conn.connection_id, e.target.value)}
                        disabled={!canModify}
                        className="w-full px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                      >
                        <option value="">選択してください</option>
                        {sites.map((site) => (
                          <option key={site.site_id} value={site.site_id}>
                            {site.display_name}
                          </option>
                        ))}
                      </select>
                    </div>

                    <div>
                      <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">ブラウザ</label>
                      <select
                        value={conn.browser_id || browsers[0]?.browser_id || ''}
                        onChange={(e) => handleBrowserChange(conn.connection_id, e.target.value)}
                        disabled={!canModify}
                        className="w-full px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                      >
                        {browsers.map((browser) => (
                          <option key={browser.browser_id} value={browser.browser_id}>
                            {browser.display_name}
                          </option>
                        ))}
                      </select>
                    </div>
                  </div>

                  {/* URL入力 */}
                  <div>
                    <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">URL</label>
                    <input
                      type="text"
                      value={conn.url || ''}
                      onChange={(e) => handleUrlChange(conn.connection_id, e.target.value)}
                      onPaste={(e) => handleUrlPaste(conn.connection_id, e)}
                      onBlur={() => handleUrlBlur(conn.connection_id)}
                      disabled={!canModifyUrl}
                      placeholder="https://..."
                      className="w-full px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                    />
                  </div>

                  {/* 合言葉入力（ツイキャス（プライベート）のみ） */}
                  {conn.site_id === TWICAS_PRIVATE_SITE_ID && (
                    <div>
                      <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">合言葉</label>
                      <div className="relative">
                        <input
                          type={showPasswordIds.has(conn.connection_id) ? 'text' : 'password'}
                          value={conn.input_state?.password ?? ''}
                          onChange={(e) => handlePasswordChange(conn.connection_id, e.target.value)}
                          onBlur={() => handlePasswordBlur(conn.connection_id)}
                          placeholder=""
                          className="w-full px-2 py-1 pr-7 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded focus:outline-none focus:border-blue-500 text-gray-900 dark:text-white"
                        />
                        <button
                          type="button"
                          onClick={() => setShowPasswordIds((prev) => {
                            const next = new Set(prev)
                            if (next.has(conn.connection_id)) next.delete(conn.connection_id)
                            else next.add(conn.connection_id)
                            return next
                          })}
                          className="absolute right-1 top-1/2 -translate-y-1/2 text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200"
                          tabIndex={-1}
                        >
                          {showPasswordIds.has(conn.connection_id) ? (
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                              <path d="M10 12a2 2 0 100-4 2 2 0 000 4z" />
                              <path fillRule="evenodd" d="M.458 10C1.732 5.943 5.522 3 10 3s8.268 2.943 9.542 7c-1.274 4.057-5.064 7-9.542 7S1.732 14.057.458 10zM14 10a4 4 0 11-8 0 4 4 0 018 0z" clipRule="evenodd" />
                            </svg>
                          ) : (
                            <svg xmlns="http://www.w3.org/2000/svg" className="h-4 w-4" viewBox="0 0 20 20" fill="currentColor">
                              <path fillRule="evenodd" d="M3.707 2.293a1 1 0 00-1.414 1.414l14 14a1 1 0 001.414-1.414l-1.473-1.473A10.014 10.014 0 0019.542 10C18.268 5.943 14.478 3 10 3a9.958 9.958 0 00-4.512 1.074l-1.78-1.781zm4.261 4.26l1.514 1.515a2.003 2.003 0 012.45 2.45l1.514 1.514a4 4 0 00-5.478-5.478z" clipRule="evenodd" />
                              <path d="M12.454 16.697L9.75 13.992a4 4 0 01-3.742-3.741L2.335 6.578A9.98 9.98 0 00.458 10c1.274 4.057 5.064 7 9.542 7 .847 0 1.669-.105 2.454-.303z" />
                            </svg>
                          )}
                        </button>
                      </div>
                    </div>
                  )}

                  {/* 接続毎の色設定（color_mode="connection" の時のみ表示） */}
                  {coreSettings?.enable_color_by_plugin_or_connection &&
                    coreSettings?.color_mode === 'connection' && (
                      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, minmax(0, 1fr))', gap: '0.5rem' }}>
                        <div>
                          <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">背景色</label>
                          <div className="flex gap-1 items-center">
                            <input
                              type="color"
                              value={conn.advanced_settings?.bgColor || '#1f2937'}
                              onChange={(e) =>
                                handleConnectionColorChange(
                                  conn.connection_id,
                                  'bgColor',
                                  e.target.value
                                )
                              }
                              disabled={!canModifyColors}
                              className="w-10 h-6 rounded cursor-pointer disabled:opacity-50 border-0"
                            />
                            <input
                              type="text"
                              value={conn.advanced_settings?.bgColor || '#1f2937'}
                              onChange={(e) =>
                                handleConnectionColorChange(
                                  conn.connection_id,
                                  'bgColor',
                                  e.target.value
                                )
                              }
                              disabled={!canModifyColors}
                              className="w-20 px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded font-mono focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                              pattern="^#[0-9A-Fa-f]{6}$"
                              maxLength={7}
                              placeholder="#1f2937"
                            />
                          </div>
                        </div>

                        <div>
                          <label className="text-xs text-gray-500 dark:text-gray-400 block mb-0.5">文字色</label>
                          <div className="flex gap-1 items-center">
                            <input
                              type="color"
                              value={conn.advanced_settings?.textColor || '#ffffff'}
                              onChange={(e) =>
                                handleConnectionColorChange(
                                  conn.connection_id,
                                  'textColor',
                                  e.target.value
                                )
                              }
                              disabled={!canModifyColors}
                              className="w-10 h-6 rounded cursor-pointer disabled:opacity-50 border-0"
                            />
                            <input
                              type="text"
                              value={conn.advanced_settings?.textColor || '#ffffff'}
                              onChange={(e) =>
                                handleConnectionColorChange(
                                  conn.connection_id,
                                  'textColor',
                                  e.target.value
                                )
                              }
                              disabled={!canModifyColors}
                              className="w-20 px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 border border-gray-300 dark:border-gray-500 rounded font-mono focus:outline-none focus:border-blue-500 disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                              pattern="^#[0-9A-Fa-f]{6}$"
                              maxLength={7}
                              placeholder="#ffffff"
                            />
                          </div>
                        </div>
                      </div>
                    )}

                  {/* アクションボタン */}
                  <div className="flex gap-2 pt-1">
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleConnect(conn.connection_id)
                      }}
                      disabled={isConnected || isConnecting || !canConnect}
                      className="flex-1 px-2 py-1 text-xs bg-green-600 hover:bg-green-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                      接続
                    </button>
                    <button
                      onClick={(e) => {
                        e.stopPropagation()
                        handleDisconnect(conn.connection_id)
                      }}
                      disabled={isDisconnected || isConnecting}
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
                      className="px-2 py-1 text-xs bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
                    >
                      削除
                    </button>
                  </div>
                </div>
              )
            })
          )}
        </div>

        <div className="p-4 border-t border-gray-200 dark:border-gray-700 space-y-2">
          <div className="text-xs text-gray-600 dark:text-gray-500">
            <span>接続数: {connections.length}</span>
          </div>
        </div>

        {/* リサイズハンドル */}
        <div
          onMouseDown={handleResizeMouseDown}
          className="absolute right-0 top-0 bottom-0 w-1 cursor-col-resize hover:bg-blue-500 transition-colors"
        />
        </>
        )}
      </div>

      {/* メインエリア */}
      <div className="flex-1 min-w-0 flex flex-col">
        {/* タブヘッダー */}
        <div className="bg-white dark:bg-gray-800 border-b border-gray-200 dark:border-gray-700">
          <div className="flex">
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'comments'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('comments')}
            >
              コメント
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'updates'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('updates')}
            >
              アップデート
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'plugins'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('plugins')}
            >
              プラグイン
            </button>
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'settings'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('settings')}
            >
              設定
            </button>
            {__IS_SEARCH_ENABLED__ && (
              <button
                className={`px-6 py-3 font-medium transition-colors ${
                  activeTab === 'search'
                    ? 'text-blue-400 border-b-2 border-blue-400'
                    : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
                }`}
                onClick={() => setActiveTab('search')}
              >
                検索
              </button>
            )}
            <button
              className={`px-6 py-3 font-medium transition-colors ${
                activeTab === 'users'
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
              }`}
              onClick={() => setActiveTab('users')}
            >
              ユーザー
            </button>
          </div>
        </div>

        {/* タブコンテンツ */}
        <div className="flex-1 overflow-hidden flex flex-col">
          {/* コメントタブ - 常にレンダリング、CSS で表示/非表示 */}
          <div className={activeTab === 'comments' ? 'flex-1 overflow-hidden flex flex-col' : 'hidden'}>
            {/* メタデータビュー */}
            {connections.length > 0 && (
              <>
                <div
                  className="shrink-0"
                  style={{ height: `${metadataHeight}px` }}
                >
                  <DataGridComponent
                    data={metadataRows}
                    columns={metadataColumns}
                    renderCell={renderMetadataCell}
                    height="100%"
                    backgroundColor={currentThemeColors.bg_sidebar}
                    headerBackgroundColor={currentThemeColors.bg_main}
                    border={`1px solid ${currentThemeColors.border}`}
                    defaultItemHeight={44}
                    alwaysShowScrollbar
                  />
                </div>
                {/* 高さリサイズハンドル */}
                <div
                  className="shrink-0 border-b border-gray-200 dark:border-gray-700"
                  style={{ height: '6px', cursor: 'row-resize', flexShrink: 0 }}
                  onMouseDown={(e) => {
                    isResizingMetadata.current = true
                    metadataResizeStartY.current = e.clientY
                    metadataResizeStartHeight.current = metadataHeightRef.current
                    document.body.style.cursor = 'row-resize'
                    document.body.style.userSelect = 'none'
                    e.preventDefault()
                  }}
                />
              </>
            )}

            {/* コメント表示 */}
            <div className="flex-1 min-h-0 p-4">
              <DataGridComponent
                ref={dataGridRef}
                data={visibleComments}
                columns={columns}
                renderCell={renderCell}
                height="100%"
                backgroundColor={currentThemeColors.bg_main}
                headerBackgroundColor={currentThemeColors.bg_sidebar}
                border={`1px solid ${currentThemeColors.border}`}
                onAtBottomChange={handleAtBottomChange}
                autoScrollEnabled={isAutoScrollEnabled}
                onUserDetachedFromBottom={() => setIsAutoScrollEnabled(false)}
                onColumnResize={handleColumnResize}
                onColumnVisibilityChange={handleColumnVisibilityChange}
                onColumnOrderChange={handleColumnOrderChange}
                onRowContextMenu={handleRowContextMenu}
                alwaysShowScrollbar
              />
            </div>

            {/* コメント投稿セクション */}
            <div className="p-3 bg-white dark:bg-gray-800 border-t border-gray-200 dark:border-gray-700">
              <div className="flex gap-2 items-center">
                <div className="flex-shrink-0">
                  <label className="block text-xs font-medium mb-1 text-gray-600 dark:text-gray-300">接続選択</label>
                  <select
                    value={selectedConnectionForCommand}
                    onChange={(e) => setSelectedConnectionForCommand(e.target.value)}
                    className="px-2 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded focus:outline-none focus:border-blue-500 text-gray-900 dark:text-white"
                  >
                    <option value="">選択してください</option>
                    {connections.map((conn) => (
                      <option key={conn.connection_id} value={conn.connection_id}>
                        {conn.name}
                      </option>
                    ))}
                  </select>
                </div>

                {/* テキスト入力（スキーマ有無にかかわらず共通） */}
                <div className="flex-1">
                  <input
                    type="text"
                    value={(commentFormData.text as string) ?? ''}
                    onChange={(e) => setCommentFormData(prev => ({ ...prev, text: e.target.value }))}
                    onKeyDown={(e) => {
                      if (e.key === 'Enter') handleSendComment()
                    }}
                    placeholder="コメントを入力してください"
                    className="w-full px-3 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded focus:outline-none focus:border-blue-500 text-gray-900 dark:text-white placeholder-gray-400 dark:placeholder-gray-500"
                  />
                </div>

                {/* スキーマで定義された追加フィールド（text 以外）をインラインで表示 */}
                {commentFormSchema && Object.keys((commentFormSchema.properties as Record<string, unknown>) ?? {})
                  .filter(key => key !== 'text')
                  .map(key => {
                    const fieldDef = (commentFormSchema.properties as Record<string, { type?: string; title?: string }>)[key]
                    if (fieldDef?.type === 'boolean') {
                      return (
                        <label key={key} className="flex-shrink-0 flex items-center gap-1 text-sm text-gray-600 dark:text-gray-300 cursor-pointer select-none">
                          <input
                            type="checkbox"
                            checked={(commentFormData[key] as boolean) ?? false}
                            onChange={(e) => setCommentFormData(prev => ({ ...prev, [key]: e.target.checked }))}
                            className="w-4 h-4 accent-blue-600"
                          />
                          {fieldDef.title ?? key}
                        </label>
                      )
                    }
                    return null
                  })}

                <button
                  onClick={handleSendComment}
                  className="flex-shrink-0 px-4 py-1.5 bg-blue-600 hover:bg-blue-700 rounded text-sm font-semibold transition-colors text-white"
                >
                  送信
                </button>
              </div>
            </div>
          </div>


          {/* アップデートタブ */}
          {activeTab === 'updates' && (
            <div className="flex-1 overflow-auto p-6 bg-white dark:bg-gray-800 space-y-4">
              <h2 className="text-lg font-semibold">アップデート</h2>
              <div className="text-sm text-gray-600 dark:text-gray-400">
                {updateMessage || '更新状態を確認できます。'}
              </div>
              <div className="border border-gray-200 dark:border-gray-700 rounded p-4 space-y-2">
                <div className="text-sm">
                  <span className="text-gray-500 dark:text-gray-400">現在の状態: </span>
                  {checkingUpdate ? '確認中...' : updateInfo ? '更新あり' : '最新'}
                </div>
                {updateInfo && (
                  <>
                    <div className="text-sm">
                      <span className="text-gray-500 dark:text-gray-400">新バージョン: </span>
                      {updateInfo.version} ({updateInfo.channel})
                    </div>
                    <div className="text-sm">
                      <span className="text-gray-500 dark:text-gray-400">公開日: </span>
                      {new Date(updateInfo.uploadedAt).toLocaleString('ja-JP')}
                    </div>
                  </>
                )}
              </div>
              <div className="flex gap-2">
                <button
                  onClick={handleCheckForUpdates}
                  disabled={checkingUpdate}
                  className="px-4 py-2 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 rounded text-sm disabled:opacity-50"
                >
                  {checkingUpdate ? '確認中...' : '更新を確認'}
                </button>
                <button
                  onClick={handleDownloadUpdate}
                  disabled={!updateInfo || downloadingUpdate}
                  className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded text-sm disabled:opacity-50"
                >
                  {downloadingUpdate ? 'ダウンロード中...' : 'ダウンロード'}
                </button>
                <button
                  onClick={handleApplyUpdate}
                  disabled={!downloadedUpdatePath}
                  className="px-4 py-2 bg-green-600 hover:bg-green-700 rounded text-sm disabled:opacity-50"
                >
                  適用して再起動
                </button>
              </div>
            </div>
          )}

          {/* プラグインタブ */}
          {activeTab === 'plugins' && (
            <div className="flex flex-col h-full bg-white dark:bg-gray-800">
              {/* ヘッダー */}
              <div className="flex items-center justify-between px-4 py-2 border-b border-gray-200 dark:border-gray-700 shrink-0">
                <h2 className="text-base font-semibold">プラグイン</h2>
                <button
                  onClick={loadRegistryPlugins}
                  className="px-2 py-1 text-xs bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 rounded"
                >
                  再読み込み
                </button>
              </div>

              {/* 2カラムレイアウト */}
              <div className="flex flex-1 min-h-0">
                {/* 左: コンパクトリスト */}
                <div className="w-56 shrink-0 border-r border-gray-200 dark:border-gray-700 overflow-y-auto">
                  {(() => {
                    const installedList = registryPlugins.filter(p => installedPlugins.has(p.id))
                    const notInstalledList = registryPlugins.filter(p => !installedPlugins.has(p.id))
                    const renderItem = (plugin: RegistryPlugin) => {
                      const selected = selectedPluginId === plugin.id
                      const hasUpdate = isUpdateAvailable(plugin)
                      const isPending = pendingUpdateIds.has(plugin.id)
                      return (
                        <button
                          key={plugin.id}
                          onClick={() => setSelectedPluginId(plugin.id)}
                          className={`w-full text-left flex items-center gap-2 px-3 py-2 text-sm border-b border-gray-100 dark:border-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 ${
                            selected ? 'bg-blue-50 dark:bg-blue-900/30' : ''
                          }`}
                        >
                          <span className="text-green-500 shrink-0 w-3 text-center">✓</span>
                          <span className="truncate">{plugin.name}</span>
                          {hasUpdate && (
                            <span className={`ml-auto text-xs shrink-0 ${isPending ? 'text-gray-400' : 'text-yellow-400'}`}>
                              {isPending ? '再起動待ち' : '更新あり'}
                            </span>
                          )}
                        </button>
                      )
                    }
                    return (
                      <>
                        {installedList.length > 0 && (
                          <>
                            <div className="px-3 py-1.5 text-xs font-semibold text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900/50 border-b border-gray-100 dark:border-gray-700/50">
                              インストール済み ({installedList.length})
                            </div>
                            {installedList.map(renderItem)}
                          </>
                        )}
                        {notInstalledList.length > 0 && (
                          <>
                            <div className="px-3 py-1.5 text-xs font-semibold text-gray-500 dark:text-gray-400 bg-gray-50 dark:bg-gray-900/50 border-b border-gray-100 dark:border-gray-700/50">
                              未インストール ({notInstalledList.length})
                            </div>
                            {notInstalledList.map((plugin) => {
                              const selected = selectedPluginId === plugin.id
                              return (
                                <button
                                  key={plugin.id}
                                  onClick={() => setSelectedPluginId(plugin.id)}
                                  className={`w-full text-left flex items-center gap-2 px-3 py-2 text-sm border-b border-gray-100 dark:border-gray-700/50 hover:bg-gray-100 dark:hover:bg-gray-700 ${
                                    selected ? 'bg-blue-50 dark:bg-blue-900/30' : ''
                                  }`}
                                >
                                  <span className="shrink-0 w-3" />
                                  <span className="truncate text-gray-500 dark:text-gray-400">{plugin.name}</span>
                                </button>
                              )
                            })}
                          </>
                        )}
                      </>
                    )
                  })()}
                </div>

                {/* 右: 詳細パネル */}
                <div className="flex-1 overflow-y-auto p-4">
                  {(() => {
                    const plugin = registryPlugins.find(p => p.id === selectedPluginId)
                    if (!plugin) {
                      return (
                        <div className="h-full flex items-center justify-center text-sm text-gray-400 dark:text-gray-500">
                          左のリストからプラグインを選択してください
                        </div>
                      )
                    }
                    const installed = installedPlugins.has(plugin.id)
                    const installedMeta = installedPlugins.get(plugin.id)
                    const target = getPreferredChannel(plugin)
                    return (
                      <div className="space-y-3">
                        <div>
                          <div className="text-lg font-semibold">{plugin.name}</div>
                          <div className="text-xs text-gray-500 dark:text-gray-400">{plugin.id}</div>
                        </div>
                        <p className="text-sm text-gray-700 dark:text-gray-300">{plugin.description}</p>
                        <div className="text-xs text-gray-500 dark:text-gray-400 space-y-1">
                          <div>ダウンロード数: {plugin.download_count.toLocaleString('ja-JP')}</div>
                          <div>対象バージョン: {target ? `${target.version} (${target.channel})` : 'なし'}</div>
                          {installedMeta?.version && (
                            <div>インストール済みバージョン: {installedMeta.version}</div>
                          )}
                        </div>
                        <div className="flex items-center gap-3 pt-2 flex-wrap">
                          <span className={`text-xs px-2 py-1 rounded text-white ${installed ? 'bg-green-700' : 'bg-gray-600'}`}>
                            {installed ? 'インストール済み' : '未インストール'}
                          </span>
                          {installed && isUpdateAvailable(plugin) && (
                            pendingUpdateIds.has(plugin.id) ? (
                              <span className="text-xs text-gray-400 dark:text-gray-500 italic">
                                再起動後にアップデートされます
                              </span>
                            ) : (
                              <button
                                onClick={() => handleUpdatePlugin(plugin)}
                                disabled={pluginBusyId === plugin.id}
                                className="px-3 py-1.5 text-sm bg-yellow-600 hover:bg-yellow-500 rounded disabled:opacity-50"
                              >
                                {pluginBusyId === plugin.id ? '処理中...' : 'アップデート'}
                              </button>
                            )
                          )}
                          {installed ? (
                            <button
                              onClick={() => handleUninstallPlugin(plugin)}
                              disabled={pluginBusyId === plugin.id}
                              className="px-3 py-1.5 text-sm bg-red-700 hover:bg-red-600 rounded disabled:opacity-50"
                            >
                              {pluginBusyId === plugin.id ? '処理中...' : 'アンインストール'}
                            </button>
                          ) : (
                            <button
                              onClick={() => handleInstallPlugin(plugin)}
                              disabled={!target || pluginBusyId === plugin.id}
                              className="px-3 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 rounded disabled:opacity-50"
                            >
                              {pluginBusyId === plugin.id ? '処理中...' : 'インストール'}
                            </button>
                          )}
                        </div>
                      </div>
                    )
                  })()}
                </div>
              </div>
            </div>
          )}

          {/* 設定タブ */}
          {activeTab === 'settings' && (
            <SettingsScreen onClose={() => {
              setActiveTab('comments')
              loadCoreSettings()
            }} />
          )}

          {__IS_SEARCH_ENABLED__ && activeTab === 'search' && (
            <SearchTab
              columns={columns}
              renderCell={renderCell}
              connections={connections}
              themeColors={currentThemeColors}
              externalQuery={searchQuery}
            />
          )}

          {activeTab === 'users' && (
            <UserListTab
              userSettings={userSettings}
              connections={connections}
              onNicknameChange={handleUserNicknameChange}
              onMcvNgChange={handleUserMcvNgChange}
              focusUserId={focusUserId}
            />
          )}
        </div>
      </div>
      </div>

      {/* コンテキストメニュー */}
      {contextMenu && (
        <div
          className="fixed z-50 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-600 rounded shadow-lg py-1 text-sm"
          style={{ left: contextMenu.x, top: contextMenu.y }}
          onClick={e => e.stopPropagation()}
        >
          {__IS_SEARCH_ENABLED__ && (
            <button
              className="w-full text-left px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
              onClick={handleContextMenuSearch}
            >
              このユーザーのコメントを検索
            </button>
          )}
          <button
            className="w-full text-left px-4 py-2 hover:bg-gray-100 dark:hover:bg-gray-700 transition-colors"
            onClick={handleContextMenuViewUser}
          >
            ユーザー情報を見る
          </button>
        </div>
      )}
    </div>
  )
}

export default App
