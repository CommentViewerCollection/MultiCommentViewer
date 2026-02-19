import { useState, useEffect } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { open } from '@tauri-apps/plugin-dialog'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { RJSFSchema, ObjectFieldTemplateProps } from '@rjsf/utils'
import { ColorPickerWidget } from './ColorPickerWidget'

// 配信サイト毎の色設定をカード形式で表示するテンプレート
const SiteColorTemplate = ({ title, properties }: ObjectFieldTemplateProps) => {
  return (
    <div className="site-color-item">
      <div className="site-color-item-title">{title}</div>
      <div className="site-color-item-fields">
        {properties.map(prop => (
          <div key={prop.name}>{prop.content}</div>
        ))}
      </div>
    </div>
  )
}

// ============================================================================
// cookie.txt読み込みプラグイン用カスタム設定UI
// ============================================================================

interface CookiesTxtEntry {
  name: string
  path: string
  browser_id: string
}

function CookiesTxtSettings({
  pluginId,
  data,
}: {
  pluginId: string
  data: any
}) {
  const [name, setName] = useState('')
  const [path, setPath] = useState('')
  const [loading, setLoading] = useState(false)
  // ローカル状態で管理し、即時UIフィードバックを実現
  const [localData, setLocalData] = useState<any>(data)

  // 親から渡される data が変わったときに同期
  useEffect(() => {
    setLocalData(data)
  }, [data])

  const entries: CookiesTxtEntry[] = localData?.entries ?? []

  const handlePickFile = async () => {
    const selected = await open({
      filters: [{ name: 'Cookies', extensions: ['txt'] }],
      multiple: false,
    })
    if (typeof selected === 'string') {
      setPath(selected)
    }
  }

  // update_settings 後にプラグインの処理完了を待ってから設定を再取得する
  // Core の handle_update_settings はアクションJSONをキャッシュに保存するため、
  // プラグインが SettingsData を Core に送り返すまで少し待つ必要がある
  const reloadLocalData = async () => {
    await new Promise(resolve => setTimeout(resolve, 300))
    try {
      const freshData = await invoke<any>('get_settings', { target: pluginId })
      setLocalData(freshData)
    } catch (e) {
      console.error('設定の再取得に失敗しました:', e)
    }
  }

  const handleRegister = async () => {
    if (!path) return
    try {
      setLoading(true)
      await invoke('update_settings', {
        target: pluginId,
        data: { action: 'add', name, path },
      })
      setName('')
      setPath('')
      await reloadLocalData()
    } catch (e) {
      console.error('cookies.txt の登録に失敗しました:', e)
    } finally {
      setLoading(false)
    }
  }

  const handleDelete = async (browserId: string) => {
    try {
      setLoading(true)
      await invoke('update_settings', {
        target: pluginId,
        data: { action: 'remove', browser_id: browserId },
      })
      await reloadLocalData()
    } catch (e) {
      console.error('cookies.txt の削除に失敗しました:', e)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="space-y-6">
      {/* ファイル追加フォーム */}
      <div className="border border-gray-300 dark:border-gray-600 rounded-lg p-4 bg-white dark:bg-gray-800 space-y-3">
        <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-200">新しいファイルを登録</h3>

        <div>
          <label className="block text-xs text-gray-500 dark:text-gray-400 mb-1">名前（任意）</label>
          <input
            type="text"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="例: My YouTube Cookie"
            className="w-full px-3 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-500 rounded focus:outline-none focus:border-blue-500 text-gray-900 dark:text-gray-100"
          />
        </div>

        <div>
          <label className="block text-xs text-gray-500 dark:text-gray-400 mb-1">cookies.txt ファイル</label>
          <div className="flex gap-2">
            <input
              type="text"
              value={path}
              readOnly
              placeholder="ファイルを選択してください"
              className="flex-1 px-3 py-1.5 text-sm bg-gray-100 dark:bg-gray-700 border border-gray-300 dark:border-gray-500 rounded text-gray-600 dark:text-gray-300 cursor-default"
            />
            <button
              onClick={handlePickFile}
              className="px-3 py-1.5 text-sm bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-500 border border-gray-300 dark:border-gray-500 rounded transition-colors text-gray-700 dark:text-gray-200"
            >
              選択...
            </button>
          </div>
        </div>

        <button
          onClick={handleRegister}
          disabled={!path || loading}
          className="px-4 py-1.5 text-sm bg-blue-600 hover:bg-blue-700 disabled:opacity-50 disabled:cursor-not-allowed rounded transition-colors text-white"
        >
          登録
        </button>
      </div>

      {/* 登録済みファイル一覧 */}
      <div>
        <h3 className="text-sm font-semibold text-gray-700 dark:text-gray-200 mb-2">
          登録済みファイル（{entries.length}件）
        </h3>
        {entries.length === 0 ? (
          <div className="text-sm text-gray-600 dark:text-gray-500 py-4 text-center border border-gray-200 dark:border-gray-700 rounded-lg">
            登録済みのファイルはありません
          </div>
        ) : (
          <div className="space-y-2">
            {entries.map(entry => (
              <div
                key={entry.browser_id}
                className="flex items-center gap-3 px-3 py-2 bg-gray-50 dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg"
              >
                <div className="flex-1 min-w-0">
                  <div className="text-sm text-gray-700 dark:text-gray-200 font-medium truncate">
                    {entry.name || entry.path}
                  </div>
                  {entry.name && (
                    <div className="text-xs text-gray-600 dark:text-gray-500 truncate">{entry.path}</div>
                  )}
                </div>
                <button
                  onClick={() => handleDelete(entry.browser_id)}
                  disabled={loading}
                  className="flex-shrink-0 px-2 py-1 text-xs bg-red-700 hover:bg-red-600 disabled:opacity-50 rounded transition-colors text-white"
                >
                  削除
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  )
}

// ============================================================================

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

  // RJSF用のカスタムスタイル（ライト/ダーク両対応）
  const customStyles = `
    /* 入力フィールドとセレクトボックスのスタイル（ライトモードデフォルト） */
    .rjsf input[type="text"],
    .rjsf input[type="number"],
    .rjsf input[type="email"],
    .rjsf select,
    .rjsf textarea {
      background-color: #f3f4f6 !important;
      border: 1px solid #d1d5db !important;
      color: #111827 !important;
      border-radius: 0.375rem;
      padding: 0.5rem 0.75rem;
    }

    /* ダークモード: 入力フィールド */
    .dark .rjsf input[type="text"],
    .dark .rjsf input[type="number"],
    .dark .rjsf input[type="email"],
    .dark .rjsf select,
    .dark .rjsf textarea {
      background-color: #374151 !important;
      border: 1px solid #4b5563 !important;
      color: #f3f4f6 !important;
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

    /* ラベルのスタイル（ライトモードデフォルト） */
    .rjsf label {
      color: #374151 !important;
      font-weight: 500;
      margin-bottom: 0.25rem;
      display: block;
    }

    /* ダークモード: ラベル */
    .dark .rjsf label {
      color: #e5e7eb !important;
    }

    /* 説明文のスタイル（ライトモードデフォルト） */
    .rjsf .field-description {
      color: #6b7280 !important;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }

    /* ダークモード: 説明文 */
    .dark .rjsf .field-description {
      color: #9ca3af !important;
    }

    /* 条件付きフィールドのグループ化（ライトモードデフォルト） */
    .rjsf .field-object > fieldset {
      border: 1px solid #d1d5db;
      border-radius: 0.5rem;
      padding: 1rem;
      margin-top: 0.5rem;
      background-color: rgba(209, 213, 219, 0.3);
    }

    /* ダークモード: fieldset */
    .dark .rjsf .field-object > fieldset {
      border: 1px solid #4b5563;
      background-color: rgba(55, 65, 81, 0.3);
    }

    .rjsf .field-object > fieldset > legend {
      color: #6b7280 !important;
      font-size: 0.875rem;
      font-weight: 600;
      padding: 0 0.5rem;
    }

    .dark .rjsf .field-object > fieldset > legend {
      color: #9ca3af !important;
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
      color: #2563eb !important;
      font-weight: 600;
    }

    .dark .rjsf .conditional-field > label {
      color: #93c5fd !important;
    }

    /* readOnlyフィールド（disabled状態）のスタイル */
    .rjsf .conditional-field:has(input[readonly]),
    .rjsf .conditional-field:has(select[disabled]),
    .rjsf .conditional-field-disabled {
      opacity: 0.5;
      pointer-events: none;
      background-color: rgba(209, 213, 219, 0.2) !important;
      border-left-color: #9ca3af !important;
    }

    .dark .rjsf .conditional-field:has(input[readonly]),
    .dark .rjsf .conditional-field:has(select[disabled]),
    .dark .rjsf .conditional-field-disabled {
      background-color: rgba(55, 65, 81, 0.2) !important;
      border-left-color: #6b7280 !important;
    }

    .rjsf .conditional-field:has(input[readonly]) > label,
    .rjsf .conditional-field:has(select[disabled]) > label,
    .rjsf .conditional-field-disabled > label {
      color: #9ca3af !important;
    }

    .dark .rjsf .conditional-field:has(input[readonly]) > label,
    .dark .rjsf .conditional-field:has(select[disabled]) > label,
    .dark .rjsf .conditional-field-disabled > label {
      color: #6b7280 !important;
    }

    /* readOnlyの入力フィールド */
    .rjsf input[readonly],
    .rjsf select[disabled],
    .rjsf textarea[readonly] {
      cursor: not-allowed;
      opacity: 0.6;
    }

    /* 配信サイト毎の色設定 - サイトカード（ライトモードデフォルト） */
    .rjsf .site-color-item {
      border: 1px solid #d1d5db;
      border-radius: 0.5rem;
      padding: 0.75rem 1rem;
      margin-bottom: 0.75rem;
      background-color: rgba(209, 213, 219, 0.3);
    }

    /* ダークモード: サイトカード */
    .dark .rjsf .site-color-item {
      border: 1px solid #4b5563;
      background-color: rgba(55, 65, 81, 0.3);
    }

    .rjsf .site-color-item-title {
      color: #2563eb;
      font-weight: 600;
      font-size: 0.9rem;
      margin-bottom: 0.5rem;
      padding-bottom: 0.375rem;
      border-bottom: 1px solid #d1d5db;
    }

    .dark .rjsf .site-color-item-title {
      color: #93c5fd;
      border-bottom: 1px solid #374151;
    }

    .rjsf .site-color-item-fields {
      padding-left: 0.75rem;
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
    // キャンセル時は元のテーマに戻す
    const originalTheme = originalData['core']?.theme ?? 'dark'
    applyThemeToHtml(originalTheme)
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

  // テーマをhtmlタグに即座に適用するヘルパー
  const applyThemeToHtml = (theme: string) => {
    const isDark = theme !== 'light'
    document.documentElement.classList.toggle('dark', isDark)
    document.documentElement.classList.toggle('modern-dark', theme === 'modern-dark')
  }

  const handleFormChange = (tabId: string, formData: any) => {
    setCurrentData(prev => ({
      ...prev,
      [tabId]: formData,
    }))
    // coreタブのテーマが変更されたら即座にUIに反映
    if (tabId === 'core' && formData?.theme) {
      applyThemeToHtml(formData.theme)
    }
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
          'ui:ObjectFieldTemplate': SiteColorTemplate,
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

  // プラグイン設定用の動的uiSchema生成
  // - is_enabled フィールドがあるプラグインでは最上段に配置し、
  //   Core設定の "条件付きフィールド" と同じ見た目で disabled 制御する
  const getPluginUiSchema = (tabId: string): any => {
    const uiSchema: any = {
      'ui:submitButtonOptions': { norender: true },
    }
    const schema = tabs.find(t => t.id === tabId)?.schema
    if (!schema?.properties?.is_enabled) return uiSchema

    // スキーマの x-ui-order があればそれを使用、なければ is_enabled を先頭にする
    const xUiOrder = (schema as any)['x-ui-order'] as string[] | undefined
    uiSchema['ui:order'] = xUiOrder ?? ['is_enabled', '*']

    const isEnabled = currentData[tabId]?.is_enabled ?? true

    // is_enabled 以外の全フィールドに conditional-field スタイルを適用し、
    // is_enabled=false のとき conditional-field-disabled で視覚的に無効化する
    Object.keys(schema.properties as object).forEach(key => {
      if (key === 'is_enabled') return
      uiSchema[key] = {
        'ui:classNames': `conditional-field${!isEnabled ? ' conditional-field-disabled' : ''}`,
        'ui:readonly': !isEnabled,
      }
    })

    return uiSchema
  }

  // Core・プラグイン共通の動的スキーマ生成（条件付きフィールドのreadOnly制御）
  const getEffectiveSchema = () => {
    if (!activeTabData) return undefined

    // プラグインタブ: is_enabled が false のとき他フィールドを readOnly に
    if (activeTab !== 'core') {
      const pluginSchema = activeTabData.schema
      if (!pluginSchema?.properties?.is_enabled) return pluginSchema

      const schema = JSON.parse(JSON.stringify(pluginSchema))
      const isEnabled = currentData[activeTab]?.is_enabled ?? true

      Object.keys(schema.properties as object).forEach(key => {
        if (key === 'is_enabled') return
        if (!isEnabled) {
          schema.properties[key].readOnly = true
        } else {
          delete schema.properties[key].readOnly
        }
      })
      return schema
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
    <div className="flex-1 flex flex-col overflow-hidden">
      <style>{customStyles}</style>

      {/* タブヘッダー */}
      <div className="flex border-b border-gray-200 dark:border-gray-700 px-6 bg-white dark:bg-gray-800">
        {tabs.map(tab => (
          <button
            key={tab.id}
            className={`px-4 py-2 transition-colors ${
              activeTab === tab.id
                ? 'text-blue-400 border-b-2 border-blue-400'
                : 'text-gray-500 dark:text-gray-400 hover:text-gray-700 dark:hover:text-gray-200'
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
            <div className="text-gray-500 dark:text-gray-400">読み込み中...</div>
          </div>
        )}

        {error && (
          <div className="bg-red-900 bg-opacity-50 border border-red-700 rounded p-4 mb-4">
            <div className="text-red-200">エラー: {error}</div>
          </div>
        )}

        {!loading && !error && activeTabData && (
          activeTabData.name === 'cookie.txt読み込み' ? (
            <CookiesTxtSettings
              pluginId={activeTab}
              data={activeTabData.data}
            />
          ) : (
            <div className="rjsf">
              <Form
                schema={getEffectiveSchema()}
                formData={currentData[activeTab]}
                validator={validator}
                onChange={(e) => handleFormChange(activeTab, e.formData)}
                uiSchema={activeTab === 'core' ? getCoreUiSchema() : getPluginUiSchema(activeTab)}
                widgets={{
                  ColorPickerWidget: ColorPickerWidget
                }}
              >
                <></>  {/* ボタンを非表示 */}
              </Form>
            </div>
          )
        )}
      </div>

      {/* フッター（ボタン） */}
      <div className="px-6 py-4 border-t border-gray-200 dark:border-gray-700 flex justify-end gap-2 bg-white dark:bg-gray-800">
        <button
          onClick={handleOk}
          className="px-4 py-2 bg-blue-600 hover:bg-blue-700 rounded transition-colors text-white"
          disabled={loading}
        >
          OK
        </button>
        <button
          onClick={handleApply}
          className="px-4 py-2 bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-700 rounded transition-colors text-gray-900 dark:text-white"
          disabled={loading}
        >
          適用
        </button>
        <button
          onClick={handleCancel}
          className="px-4 py-2 bg-gray-200 dark:bg-gray-600 hover:bg-gray-300 dark:hover:bg-gray-700 rounded transition-colors text-gray-900 dark:text-white"
        >
          キャンセル
        </button>
      </div>
    </div>
  )
}
