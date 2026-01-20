interface ErrorDialogProps {
  error: string | null
  onClose: () => void
}

export function ErrorDialog({ error, onClose }: ErrorDialogProps) {
  if (!error) return null

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-lg border border-red-700 p-6 max-w-md w-full mx-4">
        <div className="flex items-start gap-3 mb-4">
          <span className="text-red-400 text-2xl">✗</span>
          <div>
            <h3 className="text-xl font-semibold text-red-400 mb-2">エラー</h3>
            <p className="text-gray-300">{error}</p>
          </div>
        </div>
        <div className="flex justify-end">
          <button
            onClick={onClose}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors"
          >
            OK
          </button>
        </div>
      </div>
    </div>
  )
}
