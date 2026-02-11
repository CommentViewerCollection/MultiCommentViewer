import { WidgetProps } from '@rjsf/utils'

export function ColorPickerWidget(props: WidgetProps) {
  const { value, onChange, disabled } = props

  return (
    <div className="flex gap-2 items-center">
      <input
        type="color"
        value={value || '#000000'}
        onChange={(e) => onChange(e.target.value)}
        className="w-12 h-10 rounded cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed"
        disabled={disabled}
      />
      <input
        type="text"
        value={value || '#000000'}
        onChange={(e) => onChange(e.target.value)}
        className="flex-1 px-3 py-2 bg-gray-700 border border-gray-600 rounded font-mono text-white"
        pattern="^#[0-9A-Fa-f]{6}$"
        disabled={disabled}
      />
    </div>
  )
}
