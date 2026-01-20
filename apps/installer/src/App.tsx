import { useState } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { getCurrentWindow } from '@tauri-apps/api/window'
import { Sidebar } from './components/Sidebar'
import { ButtonBar } from './components/ButtonBar'
import { WelcomeScreen } from './screens/WelcomeScreen'
import { OptionsScreen } from './screens/OptionsScreen'
import { ReadyScreen } from './screens/ReadyScreen'
import { InstallingScreen } from './screens/InstallingScreen'
import { CompletionScreen } from './screens/CompletionScreen'

type ScreenType = 'welcome' | 'options' | 'ready' | 'installing' | 'complete'

// インストーラ更新情報
interface InstallerUpdateInfo {
  version: string
  required: boolean
  download_url: string
  sha256: string
  release_notes: string
  released_at: string
}

// mcv更新情報
interface McvUpdateInfo {
  version: string
  channel: string
  fileName: string
  fileSize?: number
  sha256: string
  uploadedAt: string
}

// プラグインのチャンネル情報
interface PluginChannels {
  stable: string | null
  beta: string | null
  alpha: string | null
}

// プラグイン一覧の各アイテム
interface PluginListItem {
  id: string
  name: string
  description: string
  channels: PluginChannels
}

function App() {
  const [currentScreen, setCurrentScreen] = useState<ScreenType>('welcome')
  const [completedScreens, setCompletedScreens] = useState<ScreenType[]>([])

  // インストール状態
  const [installerUpdate, setInstallerUpdate] = useState<InstallerUpdateInfo | null>(null)
  const [existingVersion, setExistingVersion] = useState<string | null>(null)
  const [isNewInstall, setIsNewInstall] = useState(true)
  const [mcvUpdate, setMcvUpdate] = useState<McvUpdateInfo | null>(null)
  const [availablePlugins, setAvailablePlugins] = useState<PluginListItem[]>([])
  const [selectedPlugins, setSelectedPlugins] = useState<Set<string>>(new Set())
  const [createDesktopShortcut, setCreateDesktopShortcut] = useState(true)
  const [createStartMenuShortcut, setCreateStartMenuShortcut] = useState(true)

  // Screen to step number mapping
  const screenToStep: Record<ScreenType, number> = {
    welcome: 1,
    options: 2,
    ready: 3,
    installing: 4,
    complete: 5,
  }

  // Navigation handlers
  const handleNext = () => {
    // Mark current screen as completed
    if (!completedScreens.includes(currentScreen)) {
      setCompletedScreens([...completedScreens, currentScreen])
    }

    // Navigate to next screen
    switch (currentScreen) {
      case 'welcome':
        setCurrentScreen('options')
        break
      case 'options':
        setCurrentScreen('ready')
        break
      case 'ready':
        setCurrentScreen('installing')
        break
      case 'installing':
        setCurrentScreen('complete')
        break
      case 'complete':
        handleClose()
        break
    }
  }

  const handleBack = () => {
    switch (currentScreen) {
      case 'options':
        setCurrentScreen('welcome')
        break
      case 'ready':
        setCurrentScreen('options')
        break
      // Cannot go back from installing or complete
    }
  }

  const handleCancel = async () => {
    if (
      currentScreen !== 'installing' &&
      confirm('インストールをキャンセルしてもよろしいですか？')
    ) {
      await getCurrentWindow().close()
    }
  }

  const handleClose = async () => {
    await getCurrentWindow().close()
  }

  // Handlers for WelcomeScreen
  const handleInstallerUpdateDetected = (update: InstallerUpdateInfo | null) => {
    setInstallerUpdate(update)
  }

  const handleExistingInstallationDetected = (version: string | null) => {
    setExistingVersion(version)
    setIsNewInstall(!version)
  }

  const handleWelcomeInitComplete = async () => {
    // プラグイン一覧を取得
    try {
      const plugins = await invoke<PluginListItem[]>('list_plugins')
      setAvailablePlugins(plugins)

      // 更新の場合はmcv更新情報も取得
      if (existingVersion) {
        const update = await invoke<McvUpdateInfo | null>('check_mcv_update', {
          currentVersion: existingVersion,
        })
        setMcvUpdate(update)
      }
    } catch (error) {
      console.error('Failed to load plugins:', error)
    }
  }

  // Handlers for OptionsScreen
  const handlePluginSelectionChange = (pluginId: string, selected: boolean) => {
    setSelectedPlugins((prev) => {
      const newSet = new Set(prev)
      if (selected) {
        newSet.add(pluginId)
      } else {
        newSet.delete(pluginId)
      }
      return newSet
    })
  }

  // Handlers for InstallingScreen
  const handleInstallComplete = () => {
    setCurrentScreen('complete')
  }

  const handleInstallError = (error: string) => {
    console.error('Installation failed:', error)
    // TODO: Show error dialog or error screen
    alert(`インストールエラー: ${error}`)
  }

  // Button state logic
  const canGoBack = currentScreen === 'options' || currentScreen === 'ready'
  const canGoNext = () => {
    // インストーラー更新が必要な場合はブロック
    if (currentScreen === 'welcome' && installerUpdate && installerUpdate.required) {
      return false
    }
    return true
  }
  const showButtonBar = currentScreen !== 'installing'

  // Get Next button text
  const getNextButtonText = (): string => {
    switch (currentScreen) {
      case 'ready':
        return 'インストール'
      case 'complete':
        return '完了'
      default:
        return '次へ >'
    }
  }

  // Map completed screens to step numbers
  const completedStepNumbers = completedScreens.map((screen) => screenToStep[screen])

  return (
    <div className="flex h-screen bg-gray-900 text-white">
      {/* Sidebar */}
      <Sidebar currentStep={screenToStep[currentScreen]} completedSteps={completedStepNumbers} />

      {/* Main content area */}
      <div className="flex-1 flex flex-col">
        {/* Screen content */}
        <div className="flex-1 overflow-auto">
          {currentScreen === 'welcome' && (
            <WelcomeScreen
              onInstallerUpdateDetected={handleInstallerUpdateDetected}
              onExistingInstallationDetected={handleExistingInstallationDetected}
              onInitComplete={handleWelcomeInitComplete}
            />
          )}
          {currentScreen === 'options' && (
            <OptionsScreen
              isNewInstall={isNewInstall}
              existingVersion={existingVersion}
              mcvUpdate={mcvUpdate}
              availablePlugins={availablePlugins}
              selectedPlugins={selectedPlugins}
              onPluginSelectionChange={handlePluginSelectionChange}
              createDesktopShortcut={createDesktopShortcut}
              onDesktopShortcutChange={setCreateDesktopShortcut}
              createStartMenuShortcut={createStartMenuShortcut}
              onStartMenuShortcutChange={setCreateStartMenuShortcut}
            />
          )}
          {currentScreen === 'ready' && (
            <ReadyScreen
              isNewInstall={isNewInstall}
              existingVersion={existingVersion}
              selectedPlugins={selectedPlugins}
              availablePlugins={availablePlugins}
              createDesktopShortcut={createDesktopShortcut}
              createStartMenuShortcut={createStartMenuShortcut}
            />
          )}
          {currentScreen === 'installing' && (
            <InstallingScreen
              existingVersion={existingVersion}
              selectedPlugins={selectedPlugins}
              availablePlugins={availablePlugins}
              createDesktopShortcut={createDesktopShortcut}
              createStartMenuShortcut={createStartMenuShortcut}
              onInstallComplete={handleInstallComplete}
              onInstallError={handleInstallError}
            />
          )}
          {currentScreen === 'complete' && (
            <CompletionScreen
              selectedPlugins={selectedPlugins}
              availablePlugins={availablePlugins}
              createDesktopShortcut={createDesktopShortcut}
              createStartMenuShortcut={createStartMenuShortcut}
            />
          )}
        </div>

        {/* Button bar */}
        {showButtonBar && (
          <ButtonBar
            onBack={handleBack}
            onNext={handleNext}
            onCancel={handleCancel}
            canGoBack={canGoBack}
            canGoNext={canGoNext()}
            nextButtonText={getNextButtonText()}
          />
        )}
      </div>
    </div>
  )
}

export default App
