import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { RJSFSchema } from '@rjsf/utils'
import { ColorPickerWidget } from './ColorPickerWidget'

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

  // RJSF用のカスタムスタイル
  const customStyles = `
    /* 入力フィールドとセレクトボックスのスタイル */
    .rjsf input[type="text"],
    .rjsf input[type="number"],
    .rjsf input[type="email"],
    .rjsf select,
    .rjsf textarea {
      background-color: #374151 !important;
      border: 1px solid #4b5563 !important;
      color: #f3f4f6 !important;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
    }

    .rjsf input[type="text"]:focus,
    .rjsf input[type="number"]:focus,
    .rjsf input[type="email"]:focus,
    .rjsf select:focus,
    .rjsf textarea:focus {
      outline: none;
      border-color: #3b82f6 !important;
      box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.3);
    }

    /* チェックボックスのスタイル */
    .rjsf input[type="checkbox"] {
      width: 1.25rem;
      height: 1.25rem;
      cursor: pointer;
    }

    /* ラジオボタンのスタイル */
    .rjsf input[type="radio"] {
      width: 1.25rem;
      height: 1.25rem;
      cursor: pointer;
      margin-right: 0.5rem;
    }

    /* ラベルのスタイル */
    .rjsf label {
      color: #e5e7eb !important;
      font-weight: 500;
      margin-bottom: 0.25rem;
      display: block;
    }

    /* 説明文のスタイル */
    .rjsf .field-description {
      color: #9ca3af !important;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    /* 条件付きフィールドのグループ化 */
    .rjsf .field-object > fieldset {
      border: 1px solid #4b5563;
      border-radius: 0.5rem;
      padding: 1rem;
      margin-top: 0.5rem;
      background-color: rgba(55, 65, 81, 0.3);
    }

    .rjsf .field-object > fieldset > legend {
      color: #9ca3af !important;
      font-size: 0.875rem;
      font-weight: 600;
      padding: 0 0.5rem;
    }

    /* 条件付きフィールドを視覚的に区別 */
    .rjsf .conditional-field {
      margin-left: 1.5rem;
      margin-top: 0.75rem;
      padding: 0.75rem;
      padding-left: 1rem;
      border-left: 3px solid #3b82f6;
      background-color: rgba(59, 130, 246, 0.08);
      border-radius: 0.375rem;
    }

    /* 条件付きフィールドのラベルを強調 */
    .rjsf .conditional-field > label {
      color: #93c5fd !important;
      font-weight: 600;
    }

    /* readOnlyフィールド（disabled状態）のスタイル */
    .rjsf .conditional-field:has(input[readonly]),
    .rjsf .conditional-field:has(select[disabled]),
    .rjsf .conditional-field-disabled {
      opacity: 0.5;
      pointer-events: none;
      background-color: rgba(55, 65, 81, 0.2) !important;
      border-left-color: #6b7280 !important;
    }

    .rjsf .conditional-field:has(input[readonly]) > label,
    .rjsf .conditional-field:has(select[disabled]) > label,
    .rjsf .conditional-field-disabled > label {
      color: #6b7280 !important;
    }

    /* readOnlyの入力フィールド */
    .rjsf input[readonly],
    .rjsf select[disabled],
    .rjsf textarea[readonly] {
      cursor: not-allowed;
      opacity: 0.6;
    }
  `

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

      // Siteリストを取得して、動的にsite_colorsスキーマを生成
      try {
        const sites = await invoke<any[]>('get_sites')

        // site_colors プロパティを動的に生成
        if (coreSchema.properties?.site_colors && sites && sites.length > 0) {
          const siteColorsProperties: any = {}

          sites.forEach((site: any) => {
            siteColorsProperties[site.display_name] = {
              type: "object",
              title: site.display_name,
              properties: {
                bgColor: {
                  type: "string",
                  title: "背景色",
                  default: "#1f2937"
                },
                textColor: {
                  type: "string",
                  title: "文字色",
                  default: "#ffffff"
                }
              }
            }
          })

          coreSchema.properties.site_colors.properties = siteColorsProperties
        }
      } catch (err) {
        console.error('Failed to load sites for color settings:', err)
      }

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

  // Core設定用の動的uiSchema生成（ColorPickerWidget適用）
  const getCoreUiSchema = () => {
    const baseUiSchema: any = {
      'ui:submitButtonOptions': {
        norender: true,
      },
      'ui:order': [
        'theme',
        'auto_scroll',
        'max_comments',
        'enable_color_by_plugin_or_connection',
        'color_mode',
        'site_colors'
      ],
      color_mode: {
        'ui:classNames': `conditional-field ${
          !currentData[activeTab]?.enable_color_by_plugin_or_connection
            ? 'conditional-field-disabled'
            : ''
        }`.trim(),
        'ui:readonly': !currentData[activeTab]?.enable_color_by_plugin_or_connection,
        'ui:enumNames': ['配信サイト毎', '接続毎']
      },
      site_colors: {
        'ui:classNames': `conditional-field ${
          !currentData[activeTab]?.enable_color_by_plugin_or_connection ||
          currentData[activeTab]?.color_mode === 'connection'
            ? 'conditional-field-disabled'
            : ''
        }`.trim(),
        'ui:readonly': !currentData[activeTab]?.enable_color_by_plugin_or_connection ||
                       currentData[activeTab]?.color_mode === 'connection',
        'ui:options': {
          orderable: false
        }
      }
    };

    // 各Siteのbgcolor/textColorにColorPickerWidgetを動的に割り当て
    const siteColors = currentData[activeTab]?.site_colors;
    if (siteColors && typeof siteColors === 'object') {
      Object.keys(siteColors).forEach((siteName) => {
        baseUiSchema.site_colors[siteName] = {
          bgColor: {
            'ui:widget': 'ColorPickerWidget'
          },
          textColor: {
            'ui:widget': 'ColorPickerWidget'
          }
        };
      });
    }

    return baseUiSchema;
  };

  // Core設定の動的スキーマ生成（条件付きフィールドのdisabled制御）
  const getEffectiveSchema = () => {
    if (!activeTabData || activeTab !== 'core') {
      return activeTabData?.schema
    }

    const schema = JSON.parse(JSON.stringify(activeTabData.schema))
    const formData = currentData[activeTab]

    // enable_color_by_plugin_or_connection が false の場合、条件付きフィールドをreadOnlyに
    if (!formData?.enable_color_by_plugin_or_connection) {
      if (schema.properties.color_mode) {
        schema.properties.color_mode.readOnly = true
      }
      if (schema.properties.site_colors) {
        schema.properties.site_colors.readOnly = true
      }
    } else {
      // 有効な場合はreadOnlyを解除
      if (schema.properties.color_mode) {
        delete schema.properties.color_mode.readOnly
      }

      // color_mode が "connection" の場合、site_colors をreadOnlyに
      if (formData?.color_mode === 'connection') {
        if (schema.properties.site_colors) {
          schema.properties.site_colors.readOnly = true
        }
      } else {
        if (schema.properties.site_colors) {
          delete schema.properties.site_colors.readOnly
        }
      }
    }

    return schema
  }

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <style>{customStyles}</style>
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
            <div className="rjsf">
              <Form
                schema={getEffectiveSchema()}
                formData={currentData[activeTab]}
                validator={validator}
                onChange={(e) => handleFormChange(activeTab, e.formData)}
                uiSchema={activeTab === 'core' ? getCoreUiSchema() : {
                  'ui:submitButtonOptions': {
                    norender: true,
                  },
                }}
                widgets={{
                  ColorPickerWidget: ColorPickerWidget
                }}
              >
                <></>  {/* ボタンを非表示 */}
              </Form>
            </div>
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
