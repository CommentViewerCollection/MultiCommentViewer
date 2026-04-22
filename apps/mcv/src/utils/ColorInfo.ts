import React from 'react'

// ConnectionInfo interface (from App.tsx)
interface ConnectionInfo {
  connection_id: string
  site_name?: string
  advanced_settings?: {
    bgColor?: string
    textColor?: string
    [key: string]: any
  }
  [key: string]: any
}

export class ColorInfo {
  private settingsRef: React.MutableRefObject<any>
  private connectionMapRef: React.MutableRefObject<Map<string, ConnectionInfo>>
  private connectionId: string

  constructor(
    settingsRef: React.MutableRefObject<any>,
    connectionMapRef: React.MutableRefObject<Map<string, ConnectionInfo>>,
    connectionId: string
  ) {
    this.settingsRef = settingsRef
    this.connectionMapRef = connectionMapRef
    this.connectionId = connectionId
  }

  getBackColor(): string {
    const settings = this.settingsRef.current
    const connectionMap = this.connectionMapRef.current

    if (!settings?.enable_color_by_plugin_or_connection) {
      return 'transparent'
    }

    if (settings.color_mode === 'site') {
      // サイト毎モード: site_name → site_colors から取得
      const conn = connectionMap.get(this.connectionId)
      if (conn?.site_name) {
        return settings.site_colors?.[conn.site_name]?.bgColor || 'transparent'
      }
    } else if (settings.color_mode === 'connection') {
      // 接続毎モード: advanced_settings から取得
      const conn = connectionMap.get(this.connectionId)
      return conn?.advanced_settings?.bgColor || 'transparent'
    }

    return 'transparent'
  }

  getTextColor(): string {
    const settings = this.settingsRef.current
    const connectionMap = this.connectionMapRef.current

    if (!settings?.enable_color_by_plugin_or_connection) {
      return 'inherit'
    }

    if (settings.color_mode === 'site') {
      const conn = connectionMap.get(this.connectionId)
      if (conn?.site_name) {
        return settings.site_colors?.[conn.site_name]?.textColor || 'inherit'
      }
    } else if (settings.color_mode === 'connection') {
      const conn = connectionMap.get(this.connectionId)
      return conn?.advanced_settings?.textColor || '#ffffff'
    }

    return 'inherit'
  }
}
