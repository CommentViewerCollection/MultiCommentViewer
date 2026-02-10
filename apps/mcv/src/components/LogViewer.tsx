import { useState, useEffect, useMemo } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { listen } from '@tauri-apps/api/event'
import { DataGrid, Column } from 'my-dataview'

// @ts-ignore - Type compatibility issue with React versions
const DataGridComponent = DataGrid as any

interface LogEntry {
  id: string
  level: string
  timestamp: number
  message: string
  source: {
    file: string
    line: number
    column?: number
    module_path: string
  }
  stacktrace?: any
  context?: any
  system_info: {
    mcv_version: string
    platform: string
    arch: string
    build_profile: string
  }
}

interface BuildProfileInfo {
  profile: string
}

interface LogRow {
  id: string
  level: string
  timestamp: string
  message: string
  file: string
  line: number
}

export function LogViewer() {
  const [logs, setLogs] = useState<LogEntry[]>([])
  const [buildProfile, setBuildProfile] = useState<string>('')
  const [searchText, setSearchText] = useState('')
  const [selectedLevels, setSelectedLevels] = useState<string[]>([
    'trace',
    'debug',
    'info',
    'warn',
    'error',
  ])

  // ビルドプロファイル取得
  useEffect(() => {
    invoke<BuildProfileInfo>('get_build_profile_info')
      .then((info) => setBuildProfile(info.profile))
      .catch(console.error)
  }, [])

  // 初期ログ読み込み
  const loadLogs = async () => {
    try {
      const params = {
        levels: buildProfile === 'alpha' ? selectedLevels : ['error'],
        search: searchText || null,
      }
      const entries = await invoke<LogEntry[]>('get_logs', { params })
      setLogs(entries)
    } catch (error) {
      console.error('Failed to load logs:', error)
    }
  }

  useEffect(() => {
    if (buildProfile) {
      loadLogs()
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [buildProfile, selectedLevels, searchText])

  // リアルタイム更新（新しいログを配列の先頭に追加）
  useEffect(() => {
    const unlisten = listen<LogEntry>('log-added', (event) => {
      const newLog = event.payload

      // チャンネル別フィルタ
      if (buildProfile === 'stable' || buildProfile === 'beta') {
        if (newLog.level.toLowerCase() !== 'error') return
      }

      // レベルフィルタ
      if (!selectedLevels.includes(newLog.level.toLowerCase())) return

      // テキスト検索フィルタ
      if (searchText && !newLog.message.includes(searchText)) return

      // 新しいログを一番上に追加（配列の先頭）
      setLogs((prev) => [newLog, ...prev])
    })

    return () => {
      unlisten.then((fn) => fn())
    }
  }, [buildProfile, selectedLevels, searchText])

  // DataGrid用のカラム定義
  const columns = useMemo<Column<LogRow>[]>(
    () => [
      { key: 'level', label: 'レベル', width: 80, visible: true, resizable: true },
      { key: 'timestamp', label: '日時', width: 180, visible: true, resizable: true },
      {
        key: 'message',
        label: 'メッセージ',
        width: 400,
        visible: true,
        resizable: true,
        wrap: true,
      },
      { key: 'file', label: 'ファイル', width: 200, visible: true, resizable: true },
      { key: 'line', label: '行', width: 60, visible: true, resizable: true },
    ],
    []
  )

  // データ整形
  const rows = useMemo<LogRow[]>(
    () =>
      logs.map((log) => ({
        id: log.id,
        level: log.level.toUpperCase(),
        timestamp: new Date(log.timestamp).toLocaleString('ja-JP'),
        message: log.message,
        file: log.source.file,
        line: log.source.line,
      })),
    [logs]
  )

  const renderCell = (item: LogRow, column: Column<LogRow>) => {
    if (column.key === 'level') {
      const levelColors: { [key: string]: string } = {
        ERROR: 'text-red-400',
        WARN: 'text-yellow-400',
        INFO: 'text-blue-400',
        DEBUG: 'text-gray-400',
        TRACE: 'text-gray-500',
      }
      return <span className={levelColors[item.level] || 'text-gray-400'}>{item.level}</span>
    }
    return <span>{String(item[column.key])}</span>
  }

  return (
    <div className="h-full flex flex-col p-4">
      {/* フィルタUI */}
      <div className="mb-4 flex gap-4 items-center">
        {/* テキスト検索 */}
        <div className="flex-1">
          <input
            type="text"
            placeholder="メッセージを検索..."
            className="w-full px-3 py-2 bg-gray-700 border border-gray-600 rounded focus:outline-none focus:border-blue-500 text-white placeholder-gray-500"
            value={searchText}
            onChange={(e) => setSearchText(e.target.value)}
          />
        </div>

        {/* ログレベル選択（alphaのみ） */}
        {buildProfile === 'alpha' && (
          <div className="flex gap-2">
            {['trace', 'debug', 'info', 'warn', 'error'].map((level) => (
              <label key={level} className="flex items-center gap-1 text-sm text-gray-300">
                <input
                  type="checkbox"
                  checked={selectedLevels.includes(level)}
                  onChange={(e) => {
                    if (e.target.checked) {
                      setSelectedLevels((prev) => [...prev, level])
                    } else {
                      setSelectedLevels((prev) => prev.filter((l) => l !== level))
                    }
                  }}
                  className="rounded"
                />
                <span>{level.toUpperCase()}</span>
              </label>
            ))}
          </div>
        )}

        {/* チャンネル表示 */}
        <div className="text-sm text-gray-400">
          チャンネル: {buildProfile}
          {(buildProfile === 'stable' || buildProfile === 'beta') && ' (ERRORのみ表示)'}
        </div>

        {/* 更新ボタン */}
        <button
          onClick={loadLogs}
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded font-semibold transition-colors"
        >
          更新
        </button>
      </div>

      {/* ログテーブル */}
      <div className="flex-1 border border-gray-600 rounded overflow-hidden">
        <DataGridComponent
          data={rows}
          columns={columns}
          renderCell={renderCell}
          height="100%"
          backgroundColor="#1f2937"
          border="none"
          defaultItemHeight={40}
        />
      </div>
    </div>
  )
}
