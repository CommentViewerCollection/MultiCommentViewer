import { WidgetProps } from '@rjsf/utils'
import { useRef } from 'react'

export function ColorPickerWidget(props: WidgetProps) {
  const { value, onChange, disabled } = props
  const colorInputRef = useRef<HTMLInputElement>(null)

  return (
    <div className="flex gap-2 items-center">
      <div className="relative">
        <button
          type="button"
          onClick={() => colorInputRef.current?.click()}
          disabled={disabled}
          className="relative w-12 h-10 rounded border-2 border-gray-400 dark:border-gray-500 hover:border-blue-400 hover:scale-105 transition-all shadow-sm hover:shadow-md group disabled:opacity-50 disabled:cursor-not-allowed overflow-hidden"
          style={{ backgroundColor: value || '#000000' }}
          title="クリックして色を変更"
        >
          <span className="absolute inset-0 bg-white opacity-0 group-hover:opacity-10 transition-opacity" />
          <span className="absolute bottom-0.5 right-0.5 opacity-50 group-hover:opacity-100 drop-shadow transition-opacity">
            <svg width="10" height="10" viewBox="0 0 10 10" fill="white" xmlns="http://www.w3.org/2000/svg">
              <path d="M1,1 L1,7.5 L3.2,5.8 L4.8,9 L6,8.4 L4.4,5.2 L7,5.2 Z" />
            </svg>
          </span>
        </button>
        <input
          ref={colorInputRef}
          type="color"
          value={value || '#000000'}
          onChange={(e) => onChange(e.target.value)}
          className="absolute inset-0 opacity-0 w-0 h-0 pointer-events-none"
          disabled={disabled}
        />
      </div>
      <input
        type="text"
        value={value || '#000000'}
        onChange={(e) => onChange(e.target.value)}
        className="flex-1 px-3 py-2 bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded font-mono text-gray-900 dark:text-white"
        pattern="^#[0-9A-Fa-f]{6}$"
        disabled={disabled}
      />
    </div>
  )
}
