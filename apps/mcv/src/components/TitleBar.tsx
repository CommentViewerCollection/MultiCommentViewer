import React, { useState, useEffect } from 'react'
import type { CSSProperties } from 'react'
import { getCurrentWindow } from '@tauri-apps/api/window'

interface TitleBarProps {
  theme: string
}

function getThemeStyles(theme: string) {
  if (theme === 'light') {
    return {
      bg: '#e5e7eb',
      border: '#c8cdd5',
      text: '#1f2937',
      btnHover: '#d1d5db',
      closeBtnHover: '#dc2626',
    }
  }
  if (theme === 'modern-dark') {
    return {
      bg: '#1e1e1e',
      border: '#333333',
      text: '#e5e7eb',
      btnHover: '#2a2a2a',
      closeBtnHover: '#dc2626',
    }
  }
  // dark (default)
  return {
    bg: '#111827',
    border: '#374151',
    text: '#f9fafb',
    btnHover: '#374151',
    closeBtnHover: '#dc2626',
  }
}

export function TitleBar({ theme }: TitleBarProps) {
  const [isMaximized, setIsMaximized] = useState(false)
  const [hoveredBtn, setHoveredBtn] = useState<string | null>(null)
  const [windowTitle, setWindowTitle] = useState('MultiCommentViewer')

  useEffect(() => {
    let unlistenFn: (() => void) | null = null

    async function setup() {
      try {
        const appWindow = getCurrentWindow()
        const [maximized, title] = await Promise.all([
          appWindow.isMaximized(),
          appWindow.title(),
        ])
        setIsMaximized(maximized)
        if (title) setWindowTitle(title)

        const unlisten = await appWindow.onResized(async () => {
          try {
            setIsMaximized(await appWindow.isMaximized())
          } catch { /* ignore */ }
        })
        unlistenFn = unlisten
      } catch { /* ignore */ }
    }

    setup()
    return () => { if (unlistenFn) unlistenFn() }
  }, [])

  const styles = getThemeStyles(theme)

  const btnStyle = (id: string, isClose = false): CSSProperties => ({
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'center',
    width: '46px',
    height: '32px',
    background: hoveredBtn === id
      ? (isClose ? styles.closeBtnHover : styles.btnHover)
      : 'transparent',
    color: (hoveredBtn === id && isClose) ? '#ffffff' : styles.text,
    border: 'none',
    cursor: 'default',
    fontSize: '16px',
    lineHeight: 1,
    flexShrink: 0,
    padding: 0,
    outline: 'none',
  })

  const handleDragMouseDown = (e: React.MouseEvent) => {
    if (e.button === 0) {
      getCurrentWindow().startDragging().catch(() => {})
    }
  }

  const handleMinimize = () => { getCurrentWindow().minimize().catch(() => {}) }
  const handleToggleMaximize = () => { getCurrentWindow().toggleMaximize().catch(() => {}) }
  const handleClose = () => { getCurrentWindow().close().catch(() => {}) }

  return (
    <div
      data-tauri-drag-region
      onMouseDown={handleDragMouseDown}
      style={{
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'space-between',
        height: '32px',
        minHeight: '32px',
        flexShrink: 0,
        background: styles.bg,
        borderBottom: `1px solid ${styles.border}`,
        userSelect: 'none',
        WebkitUserSelect: 'none',
        color: styles.text,
        boxSizing: 'border-box',
      }}
    >
      {/* タイトル (ドラッグ領域) */}
      <div
        data-tauri-drag-region
        style={{
          paddingLeft: '12px',
          fontSize: '13px',
          fontWeight: 600,
          pointerEvents: 'none',
          flexGrow: 1,
          overflow: 'hidden',
          whiteSpace: 'nowrap',
          textOverflow: 'ellipsis',
        }}
      >
        {windowTitle}
      </div>

      {/* ウィンドウ操作ボタン (ドラッグ伝播を止める) */}
      <div
        style={{ display: 'flex', alignItems: 'center', flexShrink: 0 }}
        onMouseDown={(e) => e.stopPropagation()}
      >
        {/* 最小化 */}
        <button
          onClick={handleMinimize}
          onMouseEnter={() => setHoveredBtn('min')}
          onMouseLeave={() => setHoveredBtn(null)}
          style={btnStyle('min')}
          title="最小化"
        >
          ─
        </button>

        {/* 最大化 / 元に戻す */}
        <button
          onClick={handleToggleMaximize}
          onMouseEnter={() => setHoveredBtn('max')}
          onMouseLeave={() => setHoveredBtn(null)}
          style={btnStyle('max')}
          title={isMaximized ? '元に戻す' : '最大化'}
        >
          {isMaximized ? '❐' : '☐'}
        </button>

        {/* 閉じる */}
        <button
          onClick={handleClose}
          onMouseEnter={() => setHoveredBtn('close')}
          onMouseLeave={() => setHoveredBtn(null)}
          style={btnStyle('close', true)}
          title="閉じる"
        >
          ✕
        </button>
      </div>
    </div>
  )
}
