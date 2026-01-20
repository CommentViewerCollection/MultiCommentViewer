interface ButtonBarProps {
  onBack: () => void;
  onNext: () => void;
  onCancel: () => void;
  canGoBack: boolean;
  canGoNext: boolean;
  nextButtonText?: string;
}

export function ButtonBar({
  onBack,
  onNext,
  onCancel,
  canGoBack,
  canGoNext,
  nextButtonText = '次へ >',
}: ButtonBarProps) {
  return (
    <div className="h-16 border-t border-gray-700 bg-gray-800 flex items-center justify-between px-6">
      <button
        onClick={onBack}
        disabled={!canGoBack}
        className={`px-6 py-2 rounded-lg font-semibold transition-colors ${
          canGoBack
            ? 'bg-gray-700 hover:bg-gray-600 text-white'
            : 'bg-gray-800 text-gray-600 cursor-not-allowed'
        }`}
      >
        &lt; 戻る
      </button>

      <div className="flex gap-4">
        <button
          onClick={onNext}
          disabled={!canGoNext}
          className={`px-6 py-2 rounded-lg font-semibold transition-colors ${
            canGoNext
              ? 'bg-blue-600 hover:bg-blue-700 text-white'
              : 'bg-gray-700 text-gray-500 cursor-not-allowed'
          }`}
        >
          {nextButtonText}
        </button>
        <button
          onClick={onCancel}
          className="px-6 py-2 bg-gray-700 hover:bg-gray-600 rounded-lg font-semibold text-white transition-colors"
        >
          キャンセル
        </button>
      </div>
    </div>
  );
}
