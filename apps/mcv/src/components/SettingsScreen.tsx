import { useState, useEffect, useRef } from 'react'
import { invoke } from '@tauri-apps/api/core'
import { open } from '@tauri-apps/plugin-dialog'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { RJSFSchema, ObjectFieldTemplateProps } from '@rjsf/utils'
import { ColorPickerWidget } from './ColorPickerWidget'
import { applyThemeColors, resolveThemeColors } from '../theme'

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
      <div className="border border-[var(--theme-border)] rounded-lg p-4 bg-[var(--theme-bg-sidebar)] space-y-3">
        <h3 className="text-sm font-semibold text-[var(--theme-text-main)]">新しいファイルを登録</h3>

        <div>
          <label className="block text-xs text-gray-500 mb-1">名前（任意）</label>
          <input
            type="text"
            value={name}
            onChange={e => setName(e.target.value)}
            placeholder="例: My YouTube Cookie"
            className="w-full px-3 py-1.5 text-sm bg-[var(--theme-bg-input)] border border-[var(--theme-border)] rounded focus:outline-none focus:border-blue-500 text-[var(--theme-text-main)]"
          />
        </div>

        <div>
          <label className="block text-xs text-gray-500 mb-1">cookies.txt ファイル</label>
          <div className="flex gap-2">
            <input
              type="text"
              value={path}
              readOnly
              placeholder="ファイルを選択してください"
              className="flex-1 px-3 py-1.5 text-sm bg-[var(--theme-bg-input)] border border-[var(--theme-border)] rounded text-gray-500 cursor-default"
            />
            <button
              onClick={handlePickFile}
              className="px-3 py-1.5 text-sm bg-[var(--theme-bg-button)] hover:bg-[var(--theme-bg-button)] border border-[var(--theme-border)] rounded transition-colors text-[var(--theme-text-main)]"
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
        <h3 className="text-sm font-semibold text-[var(--theme-text-main)] mb-2">
          登録済みファイル（{entries.length}件）
        </h3>
        {entries.length === 0 ? (
          <div className="text-sm text-gray-500 py-4 text-center border border-[var(--theme-border)] rounded-lg">
            登録済みのファイルはありません
          </div>
        ) : (
          <div className="space-y-2">
            {entries.map(entry => (
              <div
                key={entry.browser_id}
                className="flex items-center gap-3 px-3 py-2 bg-[var(--theme-bg-sidebar)] border border-[var(--theme-border)] rounded-lg"
              >
                <div className="flex-1 min-w-0">
                  <div className="text-sm text-[var(--theme-text-main)] font-medium truncate">
                    {entry.name || entry.path}
                  </div>
                  {entry.name && (
                    <div className="text-xs text-gray-500 truncate">{entry.path}</div>
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

export function SettingsScreen({ onClose, onApply }: { onClose: () => void; onApply?: () => void }) {
  const [tabs, setTabs] = useState<SettingsTab[]>([])
  const [activeTab, setActiveTab] = useState<string>('core')
  const [originalData, setOriginalData] = useState<{ [key: string]: any }>({})
  const [currentData, setCurrentData] = useState<{ [key: string]: any }>({})
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  // アンマウント時のテーマ復元用に最新の originalData を ref で追跡
  const originalDataRef = useRef<{ [key: string]: any }>({})
  useEffect(() => {
    originalDataRef.current = originalData
  })

  // アンマウント時（Cancel未クリックでタブ移動した場合）に元のテーマを復元する
  useEffect(() => {
    return () => {
      const coreData = originalDataRef.current?.['core']
      if (coreData?.theme) {
        const colors = resolveThemeColors(coreData.theme, coreData?.custom_theme_colors)
        applyThemeColors(colors)
      }
    }
  }, [])

  // RJSF用のカスタムスタイル（全テーマ CSS変数対応）
  const customStyles = `
    /* 入力フィールドとセレクトボックスのスタイル（全テーマ共通） */
    .rjsf input[type="text"],
    .rjsf input[type="number"],
    .rjsf input[type="email"],
    .rjsf select,
    .rjsf textarea {
      background-color: var(--theme-bg-input) !important;
      border: 1px solid var(--theme-border) !important;
      color: var(--theme-text-main) !important;
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
      color: var(--theme-text-main) !important;
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
      border: 1px solid var(--theme-border);
      border-radius: 0.5rem;
      padding: 1rem;
      margin-top: 0.5rem;
      background-color: rgba(0, 0, 0, 0.1);
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
      color: #60a5fa !important;
      font-weight: 600;
    }

    /* readOnlyフィールド（disabled状態）のスタイル */
    .rjsf .conditional-field:has(input[readonly]),
    .rjsf .conditional-field:has(select[disabled]),
    .rjsf .conditional-field-disabled {
      opacity: 0.5;
      pointer-events: none;
      background-color: rgba(0, 0, 0, 0.1) !important;
      border-left-color: #9ca3af !important;
    }

    .rjsf .conditional-field:has(input[readonly]) > label,
    .rjsf .conditional-field:has(select[disabled]) > label,
    .rjsf .conditional-field-disabled > label {
      color: #9ca3af !important;
    }

    /* readOnlyの入力フィールド */
    .rjsf input[readonly],
    .rjsf select[disabled],
    .rjsf textarea[readonly] {
      cursor: not-allowed;
      opacity: 0.6;
    }

    /* 配信サイト毎の色設定 - サイトカード */
    .rjsf .site-color-item {
      border: 1px solid var(--theme-border);
      border-radius: 0.5rem;
      padding: 0.75rem 1rem;
      margin-bottom: 0.75rem;
      background-color: rgba(0, 0, 0, 0.1);
    }

    .rjsf .site-color-item-title {
      color: #60a5fa;
      font-weight: 600;
      font-size: 0.9rem;
      margin-bottom: 0.5rem;
      padding-bottom: 0.375rem;
      border-bottom: 1px solid var(--theme-border);
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

  const handleApply = async () => {
    await applySettings()
    onApply?.()
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
  const applyThemeToHtml = (theme: string, customColors?: any) => {
    const colors = resolveThemeColors(theme, customColors)
    applyThemeColors(colors)
  }

  const handleFormChange = (tabId: string, formData: any) => {
    setCurrentData(prev => ({
      ...prev,
      [tabId]: formData,
    }))
    // coreタブのテーマまたはカスタム色が変更されたら即座にUIに反映
    if (tabId === 'core' && formData?.theme) {
      applyThemeToHtml(formData.theme, formData?.custom_theme_colors)
    }
  }

  const activeTabData = tabs.find(t => t.id === activeTab)

  const hasChanges = tabs.some(
    tab => JSON.stringify(currentData[tab.id]) !== JSON.stringify(originalData[tab.id])
  )
  const tabHasChanges = (tabId: string) =>
    JSON.stringify(currentData[tabId]) !== JSON.stringify(originalData[tabId])

  // Core設定用の動的uiSchema生成（ColorPickerWidget適用）
  const getCoreUiSchema = () => {
    const baseUiSchema: any = {
      'ui:submitButtonOptions': {
        norender: true,
      },
      'ui:order': [
        'theme',
        'custom_theme_colors',
        'max_comments',
        'enable_color_by_plugin_or_connection',
        'color_mode',
        'site_colors',
        '*'
      ],
      color_mode: {
        'ui:classNames': `conditional-field ${!currentData[activeTab]?.enable_color_by_plugin_or_connection
            ? 'conditional-field-disabled'
            : ''
          }`.trim(),
        'ui:readonly': !currentData[activeTab]?.enable_color_by_plugin_or_connection,
        'ui:enumNames': ['配信サイト毎', '接続毎']
      },
      site_colors: {
        'ui:classNames': `conditional-field ${!currentData[activeTab]?.enable_color_by_plugin_or_connection ||
            currentData[activeTab]?.color_mode === 'connection'
            ? 'conditional-field-disabled'
            : ''
          }`.trim(),
        'ui:readonly': !currentData[activeTab]?.enable_color_by_plugin_or_connection ||
          currentData[activeTab]?.color_mode === 'connection',
        'ui:options': {
          orderable: false
        }
      },
      custom_theme_colors: {
        'ui:classNames': `conditional-field ${currentData[activeTab]?.theme !== 'custom' ? 'conditional-field-disabled' : ''
          }`.trim(),
        'ui:readonly': currentData[activeTab]?.theme !== 'custom',
        bg_main: { 'ui:widget': 'ColorPickerWidget' },
        bg_sidebar: { 'ui:widget': 'ColorPickerWidget' },
        bg_input: { 'ui:widget': 'ColorPickerWidget' },
        bg_button: { 'ui:widget': 'ColorPickerWidget' },
        text_main: { 'ui:widget': 'ColorPickerWidget' },
        border: { 'ui:widget': 'ColorPickerWidget' },
        titlebar_bg: { 'ui:widget': 'ColorPickerWidget' },
        titlebar_text: { 'ui:widget': 'ColorPickerWidget' },
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
    <div className="flex-1 flex overflow-hidden">
      <style>{customStyles}</style>

      {/* 縦タブリスト */}
      <div className="w-48 shrink-0 border-r border-[var(--theme-border)] overflow-y-auto bg-[var(--theme-bg-sidebar)]">
        {tabs.map(tab => (
          <button
            key={tab.id}
            className={`w-full text-left px-4 py-2 text-sm transition-colors border-b border-[var(--theme-border)] flex items-center justify-between ${activeTab === tab.id
                ? 'text-blue-400 border-l-2 border-blue-400 bg-blue-50'
                : 'text-gray-500 hover:bg-[var(--theme-bg-input)]'
              }`}
            onClick={() => setActiveTab(tab.id)}
          >
            <span>{tab.name}</span>
            {tabHasChanges(tab.id) && (
              <span className="text-yellow-400 text-xs leading-none">●</span>
            )}
          </button>
        ))}
      </div>

      {/* 右側: ヘッダー + フォームエリア */}
      <div className="flex-1 flex flex-col overflow-hidden">

        {/* ヘッダー（適用・キャンセルボタン） */}
        <div className="flex items-center justify-end px-4 py-2 gap-2 border-b border-[var(--theme-border)] bg-[var(--theme-bg-sidebar)] shrink-0">
          <button
            onClick={handleApply}
            disabled={loading || !hasChanges}
            className={`px-3 py-1 text-sm rounded transition-colors ${hasChanges
                ? 'bg-blue-600 hover:bg-blue-700 text-white'
                : 'bg-[var(--theme-bg-input)] text-gray-400 cursor-not-allowed'
              }`}
          >
            適用
          </button>
          <button
            onClick={handleCancel}
            className="px-3 py-1 text-sm bg-[var(--theme-bg-button)] hover:bg-[var(--theme-bg-input)] rounded transition-colors text-[var(--theme-text-main)]"
          >
            キャンセル
          </button>
        </div>

        {/* フォームエリア */}
        <div className="flex-1 overflow-y-auto px-6 py-4">
          {loading && (
            <div className="flex items-center justify-center h-full">
              <div className="text-gray-500">読み込み中...</div>
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
      </div>
    </div>
  )
}
