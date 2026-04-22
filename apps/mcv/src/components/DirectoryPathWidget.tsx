import { WidgetProps } from '@rjsf/utils'
import { open } from '@tauri-apps/plugin-dialog'

export function DirectoryPathWidget(props: WidgetProps) {
  const { value, onChange, label, disabled } = props

  const handleBrowse = async () => {
    try {
      const selected = await open({
        directory: true,
        multiple: false,
      })
      if (selected) {
        onChange(selected)
      }
    } catch (err) {
      console.error('Failed to open directory dialog:', err)
    }
  }

  return (
    <div className="flex gap-2">
      <input
        type="text"
        value={value || ''}
        onChange={(e) => onChange(e.target.value)}
        className="flex-1 px-3 py-2 bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-600 rounded text-gray-900 dark:text-white"
        disabled={disabled}
        placeholder={label}
      />
      <button
        type="button"
        onClick={handleBrowse}
        className="px-4 py-2 bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-700 rounded transition-colors disabled:opacity-50 disabled:cursor-not-allowed text-gray-900 dark:text-white"
        disabled={disabled}
      >
        参照...
      </button>
    </div>
  )
}
