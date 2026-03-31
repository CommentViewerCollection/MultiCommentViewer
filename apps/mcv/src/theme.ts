export interface ThemeColors {
  bg_main: string
  bg_sidebar: string
  bg_input: string
  bg_button: string
  text_main: string
  border: string
  titlebar_bg: string
  titlebar_text: string
}

export const PRESET_THEME_COLORS: Record<string, ThemeColors> = {
  dark: {
    bg_main: '#111827',
    bg_sidebar: '#1f2937',
    bg_input: '#374151',
    bg_button: '#4b5563',
    text_main: '#f9fafb',
    border: '#374151',
    titlebar_bg: '#111827',
    titlebar_text: '#f9fafb',
  },
  'modern-dark': {
    bg_main: '#121212',
    bg_sidebar: '#1e1e1e',
    bg_input: '#2a2a2a',
    bg_button: '#333333',
    text_main: '#e5e7eb',
    border: '#2a2a2a',
    titlebar_bg: '#1e1e1e',
    titlebar_text: '#e5e7eb',
  },
  light: {
    bg_main: '#ffffff',
    bg_sidebar: '#f5f5f5',
    bg_input: '#ffffff',
    bg_button: '#efefef',
    text_main: '#000000',
    border: '#cccccc',
    titlebar_bg: '#f5f5f5',
    titlebar_text: '#000000',
  },
}

export function applyThemeColors(colors: ThemeColors): void {
  const el = document.documentElement
  el.style.setProperty('--theme-bg-main', colors.bg_main)
  el.style.setProperty('--theme-bg-sidebar', colors.bg_sidebar)
  el.style.setProperty('--theme-bg-input', colors.bg_input)
  el.style.setProperty('--theme-bg-button', colors.bg_button)
  el.style.setProperty('--theme-text-main', colors.text_main)
  el.style.setProperty('--theme-border', colors.border)
}

export const THEME_CSS_VARS = {
  bg_main: 'var(--theme-bg-main)',
  bg_sidebar: 'var(--theme-bg-sidebar)',
  text_main: 'var(--theme-text-main)',
  border: 'var(--theme-border)',
  border_1px: '1px solid var(--theme-border)',
} as const

export function isDarkTheme(theme: string): boolean {
  return theme !== 'light' && theme !== 'classic'
}

export function resolveThemeColors(theme: string, customColors?: Partial<ThemeColors>): ThemeColors {
  if (theme === 'custom') {
    return { ...PRESET_THEME_COLORS['modern-dark'], ...customColors }
  }
  return PRESET_THEME_COLORS[theme] ?? PRESET_THEME_COLORS['dark']
}
