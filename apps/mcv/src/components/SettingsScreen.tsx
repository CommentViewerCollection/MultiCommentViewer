import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { RJSFSchema } from '@rjsf/utils'

interface SettingsTab {
  id: string
  name: string
  schema: RJSFSchema
  data: any
}

interface PluginInfo {
  plugin_id: string
  name: string
}

export function SettingsScreen({ onClose }: { onClose: () => void }) {
  const [tabs, setTabs] = useState<SettingsTab[]>([])
  const [activeTab, setActiveTab] = useState<string>('core')
  const [originalData, setOriginalData] = useState<{ [key: string]: any }>({})
  const [currentData, setCurrentData] = useState<{ [key: string]: any }>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    loadSettings()
  }, [])

  const loadSettings = async () => {
    try {
      setLoading(true)
      setError(null)

      // Core 設定を読み込み
      const coreSchema = await invoke<any>('get_settings_schema', { target: 'core' })
      const coreData = await invoke<any>('get_settings', { target: 'core' })

      const allTabs: SettingsTab[] = [{
        id: 'core',
        name: 'Core',
        schema: coreSchema,
        data: coreData,
      }]

      // プラグイン一覧を取得
      const plugins = await invoke<PluginInfo[]>('get_plugins')

      // 各プラグインの設定を読み込み
      for (const plugin of plugins) {
        try {
          const schema = await invoke<any>('get_settings_schema', { target: plugin.plugin_id })
          const data = await invoke<any>('get_settings', { target: plugin.plugin_id })

          allTabs.push({
            id: plugin.plugin_id,
            name: plugin.name,
            schema,
            data,
          })
        } catch (err) {
          console.error(`Failed to load settings for plugin ${plugin.name}:`, err)
        }
      }

      setTabs(allTabs)

      const original: { [key: string]: any } = {}
      allTabs.forEach(tab => {
        original[tab.id] = JSON.parse(JSON.stringify(tab.data))
      })
      setOriginalData(original)
      setCurrentData(original)
    } catch (err) {
      console.error('Failed to load settings:', err)
      setError(err instanceof Error ? err.message : String(err))
    } finally {
      setLoading(false)
    }
  }

  const handleOk = async () => {
    await applySettings()
    onClose()
  }

  const handleApply = async () => {
    await applySettings()
  }

  const handleCancel = () => {
    onClose()
  }

  const applySettings = async () => {
    try {
      for (const tab of tabs) {
        if (JSON.stringify(currentData[tab.id]) !== JSON.stringify(originalData[tab.id])) {
          await invoke('update_settings', {
            target: tab.id,
            data: currentData[tab.id],
          })
        }
      }
      setOriginalData(JSON.parse(JSON.stringify(currentData)))
    } catch (err) {
      console.error('Failed to apply settings:', err)
      setError(err instanceof Error ? err.message : String(err))
    }
  }

  const handleFormChange = (tabId: string, formData: any) => {
    setCurrentData(prev => ({
      ...prev,
      [tabId]: formData,
    }))
  }

  const activeTabData = tabs.find(t => t.id === activeTab)

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-gray-800 rounded-lg w-[800px] h-[600px] flex flex-col border border-gray-700">
        {/* ヘッダー */}
        <div className="px-6 py-4 border-b border-gray-700">
          <h2 className="text-xl font-bold">設定</h2>
        </div>

        {/* タブヘッダー */}
        <div className="flex border-b border-gray-700 px-6">
          {tabs.map(tab => (
            <button
              key={tab.id}
              className={`px-4 py-2 transition-colors ${
                activeTab === tab.id
                  ? 'text-blue-400 border-b-2 border-blue-400'
                  : 'text-gray-400 hover:text-gray-200'
              }`}
              onClick={() => setActiveTab(tab.id)}
            >
              {tab.name}
            </button>
          ))}
        </div>

        {/* フォームエリア */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {loading && (
            <div className="flex items-center justify-center h-full">
              <div className="text-gray-400">読み込み中...</div>
            </div>
          )}

          {error && (
            <div className="bg-red-900 bg-opacity-50 border border-red-700 rounded p-4 mb-4">
              <div className="text-red-200">エラー: {error}</div>
            </div>
          )}

          {!loading && !error && activeTabData && (
            <Form
              schema={activeTabData.schema}
              formData={currentData[activeTab]}
              validator={validator}
              onChange={(e) => handleFormChange(activeTab, e.formData)}
              uiSchema={{
                'ui:submitButtonOptions': {
                  norender: true,
                },
              }}
            >
              <></>  {/* ボタンを非表示 */}
            </Form>
          )}
        </div>

        {/* フッター（ボタン） */}
        <div className="px-6 py-4 border-t border-gray-700 flex justify-end gap-2">
          <button
            onClick={handleOk}
            className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors"
            disabled={loading}
          >
            OK
          </button>
          <button
            onClick={handleApply}
            className="px-4 py-2 bg-gray-600 hover:bg-gray-700 rounded transition-colors"
            disabled={loading}
          >
            適用
          </button>
          <button
            onClick={handleCancel}
            className="px-4 py-2 bg-gray-600 hover:bg-gray-700 rounded transition-colors"
          >
            キャンセル
          </button>
        </div>
      </div>
    </div>
  )
}
