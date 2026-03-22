import { useState, useEffect, useRef, useCallback } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { DataGrid, Column } from 'my-dataview'
import type { Comment, ConnectionInfo } from '../App'
import type { ThemeColors } from '../theme'

// @ts-ignore
const DataGridComponent = DataGrid as any

interface Props {
  columns: Column<Comment>[]
  renderCell: (item: Comment, column: Column<Comment>) => React.ReactNode
  connections: ConnectionInfo[]
  themeColors: ThemeColors
  externalQuery?: string
}

export function SearchTab({ columns, renderCell, connections, themeColors, externalQuery }: Props) {
  const [query, setQuery] = useState(externalQuery ?? '')
  const [results, setResults] = useState<Comment[]>([])
  const [isSearching, setIsSearching] = useState(false)
  const debounceRef = useRef<ReturnType<typeof setTimeout> | null>(null)

  const doSearch = useCallback(async (q: string) => {
    if (!q.trim()) {
      setResults([])
      return
    }
    setIsSearching(true)
    try {
      const rows = await invoke<Comment[]>('search_comments', {
        query: q,
      })
      // connection_id → connection_name に変換
      const withNames = rows.map(row => {
        const conn = connections.find(c => c.connection_id === row.connection_id)
        return { ...row, connection_name: conn?.name ?? row.connection_id }
      })
      setResults(withNames)
    } catch (error) {
      console.error('Search failed:', error)
    } finally {
      setIsSearching(false)
    }
  }, [connections])

  useEffect(() => {
    if (externalQuery !== undefined) {
      setQuery(externalQuery)
    }
  }, [externalQuery])

  useEffect(() => {
    if (debounceRef.current) clearTimeout(debounceRef.current)
    debounceRef.current = setTimeout(() => {
      doSearch(query)
    }, 300)
    return () => {
      if (debounceRef.current) clearTimeout(debounceRef.current)
    }
  }, [query, doSearch])

  return (
    <div className="flex flex-col h-full">
      {/* 検索入力 */}
      <div
        className="p-3 border-b"
        style={{ borderColor: themeColors.border, backgroundColor: themeColors.bg_sidebar }}
      >
        <input
          type="text"
          value={query}
          onChange={e => setQuery(e.target.value)}
          placeholder="コメント・ユーザー名・ユーザーID・接続名で検索..."
          autoFocus
          className="w-full px-3 py-2 rounded text-sm focus:outline-none focus:ring-1 focus:ring-blue-500"
          style={{
            backgroundColor: themeColors.bg_main,
            color: themeColors.text_main,
            border: `1px solid ${themeColors.border}`,
          }}
        />
        {query.trim() && (
          <div className="mt-1 text-xs" style={{ color: themeColors.text_main }}>
            {isSearching ? '検索中...' : `${results.length} 件`}
          </div>
        )}
      </div>

      {/* 結果表示 */}
      <div className="flex-1 p-4 overflow-hidden">
        {query.trim() ? (
          <DataGridComponent
            data={results}
            columns={columns}
            renderCell={renderCell}
            height="100%"
            backgroundColor={themeColors.bg_main}
            headerBackgroundColor={themeColors.bg_sidebar}
            border={`1px solid ${themeColors.border}`}
            autoScrollEnabled={false}
            defaultItemHeight={65}
          />
        ) : (
          <div
            className="h-full flex items-center justify-center text-sm"
            style={{ color: themeColors.text_main }}
          >
            検索ワードを入力してください
          </div>
        )}
      </div>
    </div>
  )
}
