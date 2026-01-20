import { useEffect, useState } from 'react'
import { invoke } from '@tauri-apps/api/core'
import type { UninstallTarget } from '../types'

type StepStatus = 'pending' | 'in-progress' | 'completed' | 'error'

interface UninstallStep {
  id: string
  name: string
  status: StepStatus
  details: string
}

interface UninstallingScreenProps {
  target: UninstallTarget
  keepUserData: boolean
  onComplete: () => void
  onError: (error: string) => void
}

export function UninstallingScreen({
  target,
  keepUserData,
  onComplete,
  onError,
}: UninstallingScreenProps) {
  const [steps, setSteps] = useState<UninstallStep[]>([])
  const [overallProgress, setOverallProgress] = useState(0)

  useEffect(() => {
    const startUninstallation = async () => {
      // ステップリストを初期化
      const initialSteps: UninstallStep[] = []

      if (target === 'mcv' || target === 'both') {
        initialSteps.push(
          {
            id: 'check-mcv-running',
            name: 'mcv実行中チェック',
            status: 'pending',
            details: '',
          },
          {
            id: 'delete-mcv-files',
            name: 'mcvファイルを削除中',
            status: 'pending',
            details: '',
          },
          {
            id: 'delete-shortcuts',
            name: 'ショートカットを削除中',
            status: 'pending',
            details: '',
          },
          {
            id: 'unregister-mcv',
            name: 'Windowsアプリ登録を解除中',
            status: 'pending',
            details: '',
          }
        )
      }

      if (target === 'installer' || target === 'both') {
        initialSteps.push(
          {
            id: 'unregister-installer',
            name: 'インストーラーのWindowsアプリ登録を解除中',
            status: 'pending',
            details: '',
          },
          {
            id: 'delete-installer',
            name: 'インストーラーを削除中',
            status: 'pending',
            details: '',
          }
        )
      }

      setSteps(initialSteps)

      try {
        // mcvアンインストール
        if (target === 'mcv' || target === 'both') {
          // Step 1: mcv実行中チェック
          await updateStep('check-mcv-running', 'in-progress', 'チェック中...')
          const isRunning = await invoke<boolean>('is_mcv_running')

          if (isRunning) {
            await updateStep('check-mcv-running', 'error', 'mcvが実行中です')
            onError('mcvが実行中です。mcvを終了してからアンインストールを実行してください。')
            return
          }

          await updateStep('check-mcv-running', 'completed', 'mcvは実行されていません')

          // Step 2: mcvファイル削除
          await updateStep('delete-mcv-files', 'in-progress', '削除中...')
          await invoke('uninstall_mcv', { keepUserData })
          const details = keepUserData
            ? 'ユーザーデータを保持して削除完了'
            : '全てのファイルを削除完了'
          await updateStep('delete-mcv-files', 'completed', details)

          // Step 3: ショートカット削除
          await updateStep('delete-shortcuts', 'completed', '削除完了')

          // Step 4: Windowsアプリ登録解除
          await updateStep('unregister-mcv', 'completed', '登録解除完了')
        }

        // インストーラーアンインストール
        if (target === 'installer' || target === 'both') {
          // Step 5: インストーラーのWindowsアプリ登録解除
          await updateStep('unregister-installer', 'in-progress', '登録解除中...')

          // Step 6: インストーラー削除
          await updateStep('delete-installer', 'in-progress', '削除予約中...')
          await invoke('uninstall_installer')
          await updateStep('unregister-installer', 'completed', '登録解除完了')
          await updateStep('delete-installer', 'completed', '削除予約完了（終了後に削除されます）')
        }

        // 完了
        setOverallProgress(100)
        onComplete()
      } catch (error) {
        console.error('Uninstallation failed:', error)
        onError(`アンインストールに失敗しました: ${error}`)
      }
    }

    const updateStep = async (
      stepId: string,
      status: StepStatus,
      details: string
    ) => {
      setSteps((prev) =>
        prev.map((step) => (step.id === stepId ? { ...step, status, details } : step))
      )
      updateOverallProgress()
    }

    const updateOverallProgress = () => {
      setSteps((prev) => {
        const completed = prev.filter((s) => s.status === 'completed').length
        const total = prev.length
        const progress = total > 0 ? Math.round((completed / total) * 100) : 0
        setOverallProgress(progress)
        return prev
      })
    }

    startUninstallation()
  }, [target, keepUserData, onComplete, onError])

  const getStatusIcon = (status: StepStatus) => {
    switch (status) {
      case 'completed':
        return <span className="text-green-400">✓</span>
      case 'in-progress':
        return <span className="text-blue-400">→</span>
      case 'error':
        return <span className="text-red-400">✗</span>
      default:
        return <span className="text-gray-500">○</span>
    }
  }

  return (
    <div className="p-8">
      <h2 className="text-3xl font-bold mb-6">MultiCommentViewer をアンインストール中</h2>

      {/* 全体進捗 */}
      <div className="mb-6">
        <div className="flex items-center justify-between mb-2">
          <span className="text-gray-300">全体進捗</span>
          <span className="font-semibold">{overallProgress}%</span>
        </div>
        <div className="w-full bg-gray-700 rounded-full h-4">
          <div
            className="bg-blue-600 h-4 rounded-full transition-all duration-300"
            style={{ width: `${overallProgress}%` }}
          ></div>
        </div>
      </div>

      {/* アンインストール詳細 */}
      <div className="bg-gray-800 p-6 rounded-lg border border-gray-700">
        <h3 className="text-xl font-semibold mb-4">アンインストール詳細:</h3>
        <div className="space-y-3">
          {steps.map((step) => (
            <div key={step.id} className="flex items-start gap-3">
              <div className="mt-1">{getStatusIcon(step.status)}</div>
              <div className="flex-1">
                <div className="font-medium">{step.name}</div>
                {step.details && (
                  <div className="text-sm text-gray-400 mt-1">{step.details}</div>
                )}
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="mt-6 text-gray-400 text-sm">
        <p>アンインストール処理を実行しています。しばらくお待ちください...</p>
      </div>
    </div>
  )
}
