import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { getCurrentWindow } from '@tauri-apps/api/window'
import { listen } from '@tauri-apps/api/event'
import { Sidebar } from './components/Sidebar'
import { ButtonBar } from './components/ButtonBar'
import { ErrorDialog } from './components/ErrorDialog'
import { WelcomeScreen } from './screens/WelcomeScreen'
import { OptionsScreen } from './screens/OptionsScreen'
import { ReadyScreen } from './screens/ReadyScreen'
import { InstallingScreen } from './screens/InstallingScreen'
import { CompletionScreen } from './screens/CompletionScreen'
import { UninstallOptionsScreen } from './screens/UninstallOptionsScreen'
import { UninstallingScreen } from './screens/UninstallingScreen'
import { UninstallCompletionScreen } from './screens/UninstallCompletionScreen'
import type {
  InstallerUpdateInfo,
  McvUpdateInfo,
  PluginListItem,
  ScreenType,
  Mode,
  UninstallTarget,
} from './types'

function App() {
  const [currentScreen, setCurrentScreen] = useState<ScreenType>('welcome')
  const [completedScreens, setCompletedScreens] = useState<ScreenType[]>([])
  const [mode, setMode] = useState<Mode>('install')

  // インストール状態
  const [installerUpdate, setInstallerUpdate] = useState<InstallerUpdateInfo | null>(null)
  const [existingVersion, setExistingVersion] = useState<string | null>(null)
  const [isNewInstall, setIsNewInstall] = useState(true)
  const [mcvUpdate, setMcvUpdate] = useState<McvUpdateInfo | null>(null)
  const [availablePlugins, setAvailablePlugins] = useState<PluginListItem[]>([])
  const [selectedPlugins, setSelectedPlugins] = useState<Set<string>>(new Set())
  const [createDesktopShortcut, setCreateDesktopShortcut] = useState(true)
  const [createStartMenuShortcut, setCreateStartMenuShortcut] = useState(true)
  const [launchNow, setLaunchNow] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // アンインストール状態
  const [uninstallTarget] = useState<UninstallTarget>('mcv')
  const [keepUserData, setKeepUserData] = useState(true)

  // Screen to step number mapping
  const screenToStep: Record<ScreenType, number> = {
    welcome: 1,
    options: 2,
    ready: 3,
    installing: 4,
    complete: 5,
    'uninstall-options': 2,
    'uninstalling': 3,
    'uninstall-complete': 4,
  }

  // Navigation handlers
  const handleNext = () => {
    // Mark current screen as completed
    if (!completedScreens.includes(currentScreen)) {
      setCompletedScreens([...completedScreens, currentScreen])
    }

    // Navigate to next screen
    if (mode === 'install') {
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
    } else {
      // Uninstall mode
      switch (currentScreen) {
        case 'welcome':
          setCurrentScreen('uninstall-options')
          break
        case 'uninstall-options':
          setCurrentScreen('uninstalling')
          break
        case 'uninstalling':
          setCurrentScreen('uninstall-complete')
          break
        case 'uninstall-complete':
          handleClose()
          break
      }
    }
  }

  const handleBack = () => {
    if (mode === 'install') {
      switch (currentScreen) {
        case 'options':
          setCurrentScreen('welcome')
          break
        case 'ready':
          setCurrentScreen('options')
          break
        // Cannot go back from installing or complete
      }
    } else {
      // Uninstall mode
      switch (currentScreen) {
        case 'uninstall-options':
          setCurrentScreen('welcome')
          break
        // Cannot go back from uninstalling or uninstall-complete
      }
    }
  }

  const handleCancel = async () => {
    const isProcessing = currentScreen === 'installing' || currentScreen === 'uninstalling'
    const message = mode === 'install'
      ? 'インストールをキャンセルしてもよろしいですか？'
      : 'アンインストールをキャンセルしてもよろしいですか？'

    if (!isProcessing && confirm(message)) {
      await getCurrentWindow().close()
    }
  }

  const handleClose = async () => {
    // 完了画面から閉じる場合、launchNowがtrueならmcvを起動
    if (currentScreen === 'complete' && launchNow) {
      try {
        await invoke('launch_mcv')
      } catch (error) {
        console.error('Failed to launch mcv:', error)
        // 起動失敗してもインストーラーは閉じる
      }
    }
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

  const handleInstallError = (errorMessage: string) => {
    console.error('Installation failed:', errorMessage)
    setError(errorMessage)
  }

  // Handlers for UninstallingScreen
  const handleUninstallComplete = () => {
    setCurrentScreen('uninstall-complete')
  }

  const handleUninstallError = (errorMessage: string) => {
    console.error('Uninstallation failed:', errorMessage)
    setError(errorMessage)
  }

  // Handler to switch to uninstall mode
  const handleStartUninstall = () => {
    setMode('uninstall')
    setCurrentScreen('uninstall-options')
  }

  // コマンドライン引数からのアンインストールモード検出
  useEffect(() => {
    const unlisten = listen<string>('uninstall-mode', (event) => {
      console.log('Uninstall mode event received:', event.payload)
      setMode('uninstall')
      setCurrentScreen('uninstall-options')
    })

    return () => {
      unlisten.then((fn) => fn())
    }
  }, [])

  // Button state logic
  const canGoBack = mode === 'install'
    ? (currentScreen === 'options' || currentScreen === 'ready')
    : (currentScreen === 'uninstall-options')

  const canGoNext = () => {
    // インストーラー更新が必要な場合はブロック
    if (currentScreen === 'welcome' && installerUpdate && installerUpdate.required) {
      return false
    }
    return true
  }
  const showButtonBar = currentScreen !== 'installing' && currentScreen !== 'uninstalling'

  // Get Next button text
  const getNextButtonText = (): string => {
    if (mode === 'install') {
      switch (currentScreen) {
        case 'ready':
          return 'インストール'
        case 'complete':
          return '完了'
        default:
          return '次へ >'
      }
    } else {
      switch (currentScreen) {
        case 'uninstall-options':
          return 'アンインストール'
        case 'uninstall-complete':
          return '完了'
        default:
          return '次へ >'
      }
    }
  }

  // Map completed screens to step numbers
  const completedStepNumbers = completedScreens.map((screen) => screenToStep[screen])

  return (
    <div className="flex h-screen bg-gray-900 text-white">
      {/* Error Dialog */}
      <ErrorDialog error={error} onClose={() => setError(null)} />

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
              onStartUninstall={handleStartUninstall}
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
              launchNow={launchNow}
              onLaunchNowChange={setLaunchNow}
            />
          )}
          {currentScreen === 'uninstall-options' && (
            <UninstallOptionsScreen
              keepUserData={keepUserData}
              onKeepUserDataChange={setKeepUserData}
            />
          )}
          {currentScreen === 'uninstalling' && (
            <UninstallingScreen
              target={uninstallTarget}
              keepUserData={keepUserData}
              onComplete={handleUninstallComplete}
              onError={handleUninstallError}
            />
          )}
          {currentScreen === 'uninstall-complete' && (
            <UninstallCompletionScreen
              target={uninstallTarget}
              keepUserData={keepUserData}
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
