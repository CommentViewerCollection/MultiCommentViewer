import React, { useState, useRef, useCallback, forwardRef, useImperativeHandle } from 'react';
import { Virtuoso, VirtuosoHandle } from 'react-virtuoso';

// 行の最小高さの構成要素
const CELL_PADDING_PX = 8;          // padding: '8px' の上下分
const CELL_BORDER_BOTTOM_PX = 1;    // borderBottom: '1px'
const CELL_LINE_HEIGHT_PX = 24;     // ブラウザデフォルト: 16px × 1.5
export const ROW_MIN_HEIGHT =
  CELL_PADDING_PX * 2 + CELL_BORDER_BOTTOM_PX + CELL_LINE_HEIGHT_PX; // 41

export interface Column<T> {
  key: keyof T;
  label: string;
  width?: number;
  visible?: boolean;
  resizable?: boolean;
  hideable?: boolean;
  wrap?: boolean;
}

export interface DataGridProps<T> {
  data: T[];
  columns: Column<T>[];
  renderCell?: (item: T, column: Column<T>) => React.ReactNode;
  height?: string;
  width?: string;
  backgroundColor?: string;
  headerBackgroundColor?: string;
  border?: string;
  onAtBottomChange?: (atBottom: boolean) => void;
  autoScrollEnabled?: boolean;
  onUserDetachedFromBottom?: () => void;
  onItemSelect?: (index: number, item: T) => void;
  onRowContextMenu?: (item: T, event: React.MouseEvent) => void;
  onColumnResize?: (columnKey: keyof T, width: number) => void;
  onColumnVisibilityChange?: (columnKey: keyof T, visible: boolean) => void;
  onColumnOrderChange?: (columns: Column<T>[]) => void;
  defaultItemHeight?: number;
  alwaysShowScrollbar?: boolean;
}

export interface DataGridRef {
  scrollToBottom: (opts?: { behavior?: ScrollBehavior; reason?: 'new-data' | 'manual' }) => void;
}

export const DataGrid = forwardRef<DataGridRef, DataGridProps<any>>(function DataGrid<T>({
  data,
  columns,
  renderCell,
  height = '100vh',
  width = '100%',
  backgroundColor = '#1a1a1a',
  headerBackgroundColor = '#333',
  border = '2px solid red',
  onAtBottomChange,
  autoScrollEnabled = true,
  onUserDetachedFromBottom,
  onItemSelect,
  onRowContextMenu,
  onColumnResize,
  onColumnVisibilityChange,
  onColumnOrderChange,
  defaultItemHeight = ROW_MIN_HEIGHT,
  alwaysShowScrollbar = false,
}: DataGridProps<T>, ref: React.Ref<DataGridRef>) {
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const virtuosoRef = useRef<VirtuosoHandle>(null);
  const [atBottom, setAtBottom] = useState(true);
  const scrollerRef = useRef<HTMLElement | null>(null);
  const [scrollParent, setScrollParent] = useState<HTMLElement | undefined>(undefined);
  const userScrollIntentRef = useRef(false);
  const userDetachedCandidateRef = useRef(false);
  const userUpwardScrollPxRef = useRef(0);
  const lastScrollTopRef = useRef(0);
  const programmaticScrollRef = useRef(false);
  const userIntentTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const visibleColumns = columns.filter(col => col.visible !== false);

  // ---- 列並び替え（ポインターイベントベース） ----
  const [draggedKey, setDraggedKey] = useState<keyof T | null>(null);
  const [dragOverKey, setDragOverKey] = useState<keyof T | null>(null);
  const dragSourceKeyRef = useRef<keyof T | null>(null);
  const dragOverKeyRef = useRef<keyof T | null>(null);
  // columns は render ごとに新しい参照になるため ref で最新値を保持
  const columnsRef = useRef(columns);
  React.useEffect(() => { columnsRef.current = columns; }, [columns]);

  // ゴースト要素（ドラッグ中にカーソルに追随するラベル）
  const ghostRef = useRef<HTMLDivElement | null>(null);

  const createGhost = useCallback((label: string, x: number, y: number) => {
    const ghost = document.createElement('div');
    ghost.textContent = label;
    ghost.style.cssText = [
      'position:fixed',
      'pointer-events:none',
      'z-index:99999',
      'background:rgba(74,158,255,0.92)',
      'color:#fff',
      'padding:3px 10px',
      'border-radius:4px',
      'font-size:12px',
      'font-weight:bold',
      'white-space:nowrap',
      'box-shadow:0 2px 8px rgba(0,0,0,0.45)',
      'user-select:none',
      `left:${x + 14}px`,
      `top:${y - 14}px`,
    ].join(';');
    document.body.appendChild(ghost);
    ghostRef.current = ghost;
    document.body.style.cursor = 'grabbing';
    document.body.style.userSelect = 'none';
  }, []);

  const moveGhost = useCallback((x: number, y: number) => {
    if (!ghostRef.current) return;
    ghostRef.current.style.left = `${x + 14}px`;
    ghostRef.current.style.top = `${y - 14}px`;
  }, []);

  const removeGhost = useCallback(() => {
    ghostRef.current?.remove();
    ghostRef.current = null;
    document.body.style.cursor = '';
    document.body.style.userSelect = '';
  }, []);

  // アンマウント時のゴースト残留防止
  React.useEffect(() => () => removeGhost(), [removeGhost]);

  const handleDragHandlePointerDown = useCallback((e: React.PointerEvent, key: keyof T) => {
    // リサイズハンドル領域（右端 8px）はリサイズに譲る
    const rect = (e.currentTarget as HTMLElement).getBoundingClientRect();
    if (e.clientX >= rect.right - 8) return;
    e.preventDefault();
    (e.currentTarget as HTMLElement).setPointerCapture(e.pointerId);
    dragSourceKeyRef.current = key;
    dragOverKeyRef.current = key;
    setDraggedKey(key);
    setDragOverKey(key);
    const col = columnsRef.current.find(c => c.key === key);
    createGhost(col?.label ?? String(key), e.clientX, e.clientY);
  }, [createGhost]);

  const handleDragHandlePointerMove = useCallback((e: React.PointerEvent) => {
    if (!dragSourceKeyRef.current) return;
    moveGhost(e.clientX, e.clientY);
    const elements = document.elementsFromPoint(e.clientX, e.clientY);
    let foundKey: keyof T | null = null;
    for (const el of elements) {
      const colKey = (el as HTMLElement).dataset?.columnKey;
      if (colKey !== undefined) {
        foundKey = colKey as keyof T;
        break;
      }
    }
    if (foundKey !== dragOverKeyRef.current) {
      dragOverKeyRef.current = foundKey;
      setDragOverKey(foundKey);
    }
  }, [moveGhost]);

  const handleDragHandlePointerUp = useCallback((e: React.PointerEvent) => {
    if (!dragSourceKeyRef.current) return;
    (e.currentTarget as HTMLElement).releasePointerCapture(e.pointerId);
    removeGhost();
    const sourceKey = dragSourceKeyRef.current;
    const targetKey = dragOverKeyRef.current;
    dragSourceKeyRef.current = null;
    dragOverKeyRef.current = null;
    setDraggedKey(null);
    setDragOverKey(null);
    if (sourceKey && targetKey && sourceKey !== targetKey) {
      const newColumns = [...columnsRef.current];
      const fromIdx = newColumns.findIndex(c => c.key === sourceKey);
      const toIdx = newColumns.findIndex(c => c.key === targetKey);
      if (fromIdx !== -1 && toIdx !== -1) {
        const [removed] = newColumns.splice(fromIdx, 1);
        newColumns.splice(toIdx, 0, removed);
        onColumnOrderChange?.(newColumns);
      }
    }
  }, [removeGhost, onColumnOrderChange]);

  const handleDragHandlePointerCancel = useCallback(() => {
    removeGhost();
    dragSourceKeyRef.current = null;
    dragOverKeyRef.current = null;
    setDraggedKey(null);
    setDragOverKey(null);
  }, [removeGhost]);
  // ---- 列並び替えここまで ----

  const markProgrammaticScroll = useCallback(() => {
    programmaticScrollRef.current = true;
    requestAnimationFrame(() => {
      requestAnimationFrame(() => {
        programmaticScrollRef.current = false;
      });
    });
  }, []);

  const markUserIntent = useCallback(() => {
    userScrollIntentRef.current = true;
    userDetachedCandidateRef.current = false;
    userUpwardScrollPxRef.current = 0;
    if (userIntentTimerRef.current !== null) {
      clearTimeout(userIntentTimerRef.current);
    }
    userIntentTimerRef.current = setTimeout(() => {
      userScrollIntentRef.current = false;
      userDetachedCandidateRef.current = false;
      userUpwardScrollPxRef.current = 0;
      userIntentTimerRef.current = null;
    }, 1200);
  }, []);

  const defaultRenderCell = (item: T, column: Column<T>) => {
    const value = item[column.key];
    return <span>{String(value)}</span>;
  };

  const renderCellFunc = renderCell || defaultRenderCell;

  const [showColumnMenu, setShowColumnMenu] = useState(false);
  const [rightClickedColumn, setRightClickedColumn] = useState<Column<T> | null>(null);
  const [menuPosition, setMenuPosition] = useState<{ x: number; y: number } | null>(null);
  const [resizeHoverKey, setResizeHoverKey] = useState<keyof T | null>(null);

  const renderHeader = () => (
    <div
      style={{
        display: 'flex',
        backgroundColor: headerBackgroundColor,
        borderBottom: '1px solid #555',
        fontWeight: 'bold',
        position: 'relative',
        width: totalWidth,
      }}
    >
      {visibleColumns.map((column) => (
        <div
          key={String(column.key)}
          data-column-key={String(column.key)}
          style={{
            width: column.width || 100,
            minWidth: column.width || 100,
            maxWidth: column.width || 100,
            padding: '2px 8px',
            position: 'relative',
            cursor: draggedKey === column.key ? 'grabbing' : 'grab',
            overflow: 'hidden',
            textOverflow: 'ellipsis',
            whiteSpace: 'nowrap',
            opacity: draggedKey === column.key ? 0.4 : 1,
            outline: dragOverKey === column.key && draggedKey !== column.key ? '2px solid #4a9eff' : 'none',
            outlineOffset: '-2px',
            display: 'flex',
            alignItems: 'center',
            userSelect: 'none',
            touchAction: 'none',
          }}
          title={column.label}
          onContextMenu={(e) => {
            e.preventDefault();
            setRightClickedColumn(column);
            setMenuPosition({ x: e.clientX, y: e.clientY });
          }}
          onPointerDown={(e) => handleDragHandlePointerDown(e, column.key)}
          onPointerMove={handleDragHandlePointerMove}
          onPointerUp={handleDragHandlePointerUp}
          onPointerCancel={handleDragHandlePointerCancel}
        >
          <span
            style={{
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              whiteSpace: 'nowrap',
              flex: 1,
              pointerEvents: 'none',
            }}
          >
            {column.label}
          </span>
          {column.resizable !== false && (
            <div
              style={{
                position: 'absolute',
                right: 0,
                top: 0,
                bottom: 0,
                width: '8px',
                cursor: 'col-resize',
                zIndex: 10,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
              }}
              onMouseEnter={() => setResizeHoverKey(column.key)}
              onMouseLeave={() => setResizeHoverKey(null)}
              onMouseDown={(e) => handleResizeStart(e, column)}
            >
              <div
                style={{
                  width: '2px',
                  height: '60%',
                  borderRadius: '1px',
                  backgroundColor:
                    resizing?.column.key === column.key || resizeHoverKey === column.key
                      ? '#4a9eff'
                      : 'rgba(255,255,255,0.2)',
                  transition: 'background-color 0.15s',
                  pointerEvents: 'none',
                }}
              />
            </div>
          )}
        </div>
      ))}
      {rightClickedColumn && menuPosition && (
        <div
          style={{
            position: 'fixed',
            left: menuPosition.x,
            top: menuPosition.y,
            backgroundColor: '#444',
            border: '1px solid #555',
            borderRadius: '4px',
            padding: '8px',
            zIndex: 1000,
            minWidth: '150px',
          }}
          onClick={(e) => e.stopPropagation()}
        >
          <div style={{ marginBottom: '8px', fontSize: '12px', color: '#ccc' }}>
            列の表示設定
          </div>
          {columns.map((column) => (
            <label key={String(column.key)} style={{ display: 'block', marginBottom: '4px' }}>
              <input
                type="checkbox"
                checked={column.visible !== false}
                disabled={column.hideable === false}
                onChange={(e) => {
                  if (column.hideable !== false) {
                    onColumnVisibilityChange?.(column.key, e.target.checked);
                  }
                }}
              />
              {column.label}
            </label>
          ))}
        </div>
      )}
    </div>
  );

  const [resizing, setResizing] = useState<{ column: Column<T>; startX: number; startWidth: number } | null>(null);

  const handleResizeStart = useCallback((e: React.MouseEvent, column: Column<T>) => {
    console.log('handleResizeStart', column);
    setResizing({
      column,
      startX: e.clientX,
      startWidth: column.width || 100,
    });
  }, []);

  const handleResizeMove = useCallback((e: MouseEvent) => {
    if (resizing) {
      const newWidth = resizing.startWidth + (e.clientX - resizing.startX);
      onColumnResize?.(resizing.column.key, Math.max(50, newWidth));
    }
  }, [resizing, onColumnResize]);

  const handleResizeEnd = useCallback(() => {
    setResizing(null);
  }, []);

  React.useEffect(() => {
    const handleClickOutside = () => {
      setRightClickedColumn(null);
      setMenuPosition(null);
    };

    if (rightClickedColumn) {
      document.addEventListener('click', handleClickOutside);
      return () => document.removeEventListener('click', handleClickOutside);
    }
  }, [rightClickedColumn]);

  React.useEffect(() => {
    if (resizing) {
      const handleMouseMove = (e: MouseEvent) => {
        const newWidth = resizing.startWidth + (e.clientX - resizing.startX);
        onColumnResize?.(resizing.column.key, Math.max(50, newWidth));
      };

      const handleMouseUp = () => {
        setResizing(null);
      };

      document.addEventListener('mousemove', handleMouseMove);
      document.addEventListener('mouseup', handleMouseUp);

      return () => {
        document.removeEventListener('mousemove', handleMouseMove);
        document.removeEventListener('mouseup', handleMouseUp);
      };
    }
  }, [resizing, onColumnResize]);

  const handleItemSelect = useCallback((index: number) => {
    setSelectedIndex(index);
    onItemSelect?.(index, data[index]);
  }, [data, onItemSelect]);

  const handleRowContextMenu = useCallback((item: T, event: React.MouseEvent) => {
    event.preventDefault();
    onRowContextMenu?.(item, event);
  }, [onRowContextMenu]);

  const itemContent = useCallback((index: number, item: T) => {
    const isSelected = selectedIndex === index;

    // ColorInfo による動的色解決
    let backgroundColor = 'transparent'
    let textColor = 'inherit'

    if ((item as any).colorInfo) {
      // 描画時に最新の色を取得
      backgroundColor = (item as any).colorInfo.getBackColor()
      textColor = (item as any).colorInfo.getTextColor()
    } else {
      // 後方互換性のため、静的フィールドも確認
      backgroundColor = (item as any).backgroundColor || 'transparent'
      textColor = (item as any).color || 'inherit'
    }

    // 選択時の色を上書き
    if (isSelected) {
      backgroundColor = '#264653'
    }

    return (
      <div
        onClick={() => handleItemSelect(index)}
        onContextMenu={(e) => handleRowContextMenu(item, e)}
        style={{
          display: 'flex',
          minHeight: ROW_MIN_HEIGHT,
          borderBottom: `1px solid #2a2a2a`,
          backgroundColor,
          color: textColor,
          cursor: 'pointer',
        }}
      >
        {visibleColumns.map((column) => {
          const cellStyle: React.CSSProperties = {
            width: column.width || 100,
            padding: `${CELL_PADDING_PX}px`,
          };
          if (column.wrap) {
            cellStyle.whiteSpace = 'pre-wrap';
            cellStyle.wordBreak = 'break-all';
            cellStyle.overflow = 'visible';
          } else {
            cellStyle.overflow = 'hidden';
            cellStyle.textOverflow = 'ellipsis';
            cellStyle.whiteSpace = 'nowrap';
          }
          return (
            <div
              key={String(column.key)}
              style={cellStyle}
            >
              {renderCellFunc(item, column)}
            </div>
          );
        })}
      </div>
    );
  }, [visibleColumns, selectedIndex, handleItemSelect, handleRowContextMenu, renderCellFunc]);

  const totalWidth = visibleColumns.reduce((sum, col) => sum + (col.width || 100), 0);

  useImperativeHandle(ref, () => ({
    scrollToBottom: (opts) => {
      if (data.length === 0) return;
      markProgrammaticScroll();
      const behavior = opts?.behavior === 'smooth' ? 'smooth' : 'auto';
      if (scrollerRef.current) {
        scrollerRef.current.scrollTo({ top: scrollerRef.current.scrollHeight, behavior });
      }
    },
  }), [data.length, markProgrammaticScroll]);

  React.useEffect(() => {
    return () => {
      if (userIntentTimerRef.current !== null) {
        clearTimeout(userIntentTimerRef.current);
      }
    };
  }, []);

  React.useEffect(() => {
    const scroller = scrollerRef.current;
    if (!scroller) return;

    const onWheel = () => {
      markUserIntent();
    };

    const onTouchMove = () => {
      markUserIntent();
    };

    const onPointerDown = () => {
      markUserIntent();
      lastScrollTopRef.current = scroller.scrollTop;
    };

    const onKeyDown = (e: KeyboardEvent) => {
      if (
        e.key === 'PageUp' ||
        e.key === 'PageDown' ||
        e.key === 'ArrowUp' ||
        e.key === 'ArrowDown' ||
        e.key === 'Home' ||
        (e.key === ' ' && e.shiftKey)
      ) {
        markUserIntent();
      }
    };

    const onScroll = () => {
      const currentTop = scroller.scrollTop;
      const delta = lastScrollTopRef.current - currentTop;
      lastScrollTopRef.current = currentTop;
      if (!programmaticScrollRef.current && userScrollIntentRef.current && delta > 0) {
        userUpwardScrollPxRef.current += delta;
        if (userUpwardScrollPxRef.current >= 24) {
          userDetachedCandidateRef.current = true;
        }
      }
    };

    lastScrollTopRef.current = scroller.scrollTop;
    scroller.addEventListener('wheel', onWheel, { passive: true });
    scroller.addEventListener('touchmove', onTouchMove, { passive: true });
    scroller.addEventListener('pointerdown', onPointerDown);
    scroller.addEventListener('scroll', onScroll, { passive: true });
    window.addEventListener('keydown', onKeyDown);

    return () => {
      scroller.removeEventListener('wheel', onWheel);
      scroller.removeEventListener('touchmove', onTouchMove);
      scroller.removeEventListener('pointerdown', onPointerDown);
      scroller.removeEventListener('scroll', onScroll);
      window.removeEventListener('keydown', onKeyDown);
    };
  }, [markUserIntent]);

  return (
    <div
      ref={(el: HTMLElement | null) => {
        scrollerRef.current = el;
        setScrollParent(el ?? undefined);
      }}
      style={{
        height,
        width: '100%',
        backgroundColor,
        border,
        position: 'relative',
        overflowX: 'auto',
        overflowY: alwaysShowScrollbar ? 'scroll' : 'auto',
      }}
    >
      <div style={{ position: 'sticky', top: 0, zIndex: 1, width: totalWidth }}>
        {renderHeader()}
      </div>
      {scrollParent !== undefined && (
        <Virtuoso
          ref={virtuosoRef}
          customScrollParent={scrollParent}
          style={{ width: totalWidth }}
          data={data}
          itemContent={itemContent}
          defaultItemHeight={defaultItemHeight}
          atBottomStateChange={(atBottom) => {
            setAtBottom(atBottom);
            onAtBottomChange?.(atBottom);
            if (
              !atBottom &&
              autoScrollEnabled &&
              userDetachedCandidateRef.current &&
              userScrollIntentRef.current &&
              !programmaticScrollRef.current
            ) {
              onUserDetachedFromBottom?.();
              userDetachedCandidateRef.current = false;
              userScrollIntentRef.current = false;
              userUpwardScrollPxRef.current = 0;
            }
          }}
          atBottomThreshold={60}
          initialTopMostItemIndex={0}
        />
      )}
    </div>
  );
});
