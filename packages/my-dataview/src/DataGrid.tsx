import React, { useState, useRef, useCallback, forwardRef, useImperativeHandle } from 'react';
import { Virtuoso, VirtuosoHandle } from 'react-virtuoso';

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
  border?: string;
  onAtBottomChange?: (atBottom: boolean) => void;
  onItemSelect?: (index: number, item: T) => void;
  onColumnResize?: (columnKey: keyof T, width: number) => void;
  onColumnVisibilityChange?: (columnKey: keyof T, visible: boolean) => void;
  defaultItemHeight?: number;
}

export interface DataGridRef {
  scrollToBottom: () => void;
}

export const DataGrid = forwardRef<DataGridRef, DataGridProps<any>>(function DataGrid<T>({
  data,
  columns,
  renderCell,
  height = '100vh',
  width = '100%',
  backgroundColor = '#1a1a1a',
  border = '2px solid red',
  onAtBottomChange,
  onItemSelect,
  onColumnResize,
  onColumnVisibilityChange,
  defaultItemHeight = 50,
}: DataGridProps<T>, ref: React.Ref<DataGridRef>) {
  const [selectedIndex, setSelectedIndex] = useState<number | null>(null);
  const virtuosoRef = useRef<VirtuosoHandle>(null);
  const [atBottom, setAtBottom] = useState(true);
  const visibleColumns = columns.filter(col => col.visible !== false);

  const defaultRenderCell = (item: T, column: Column<T>) => {
    const value = item[column.key];
    return <span>{String(value)}</span>;
  };

  const renderCellFunc = renderCell || defaultRenderCell;

  const [showColumnMenu, setShowColumnMenu] = useState(false);
  const [rightClickedColumn, setRightClickedColumn] = useState<Column<T> | null>(null);
  const [menuPosition, setMenuPosition] = useState<{ x: number; y: number } | null>(null);

  const renderHeader = () => (
    <div
      style={{
        display: 'flex',
        backgroundColor: '#333',
        borderBottom: '1px solid #555',
        fontWeight: 'bold',
        position: 'relative',
        width: totalWidth,
      }}
    >
      {visibleColumns.map((column) => (
        <div
          key={String(column.key)}
          style={{
            width: column.width || 100,
            padding: '8px',
            position: 'relative',
            cursor: 'default',
          }}
          onContextMenu={(e) => {
            e.preventDefault();
            setRightClickedColumn(column);
            setMenuPosition({ x: e.clientX, y: e.clientY });
          }}
        >
          {column.label}
          {column.resizable !== false && (
            <div
              style={{
                position: 'absolute',
                right: 0,
                top: 0,
                bottom: 0,
                width: '5px',
                cursor: 'col-resize',
                backgroundColor: 'transparent',
                zIndex: 10,
              }}
              onMouseDown={(e) => handleResizeStart(e, column)}
            />
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
        style={{
          display: 'flex',
          borderBottom: '1px solid #2a2a2a',
          backgroundColor,
          color: textColor,
          cursor: 'pointer',
        }}
      >
        {visibleColumns.map((column) => {
          const cellStyle: React.CSSProperties = {
            width: column.width || 100,
            padding: '8px',
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
  }, [visibleColumns, selectedIndex, handleItemSelect, renderCellFunc]);

  const totalWidth = visibleColumns.reduce((sum, col) => sum + (col.width || 100), 0);

  useImperativeHandle(ref, () => ({
    scrollToBottom: () => {
      virtuosoRef.current?.scrollToIndex(data.length - 1);
    },
  }), [data.length]);

  return (
    <div style={{ height, width: '100%', backgroundColor, border, position: 'relative', overflowX: 'auto' }}>
      <div style={{ width: totalWidth, height: '100%', display: 'flex', flexDirection: 'column' }}>
        {renderHeader()}
        <Virtuoso
          ref={virtuosoRef}
          data={data}
          itemContent={itemContent}
          defaultItemHeight={defaultItemHeight}
          atBottomStateChange={(atBottom) => {
            setAtBottom(atBottom);
            onAtBottomChange?.(atBottom);
          }}
          atBottomThreshold={60}
          initialTopMostItemIndex={0}
          style={{ flex: 1 }}
        />
      </div>
    </div>
  );
});