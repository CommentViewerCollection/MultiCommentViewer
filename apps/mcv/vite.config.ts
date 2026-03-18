import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

const isAlphaChannel = process.env.MCV_CHANNEL === 'alpha'
// Cargo の comment-search feature が有効な場合に CARGO_FEATURE_COMMENT_SEARCH=1 を設定することで連動させる
const isSearchEnabled = !!process.env.CARGO_FEATURE_COMMENT_SEARCH

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      react: path.resolve('./node_modules/react'),
      'react-dom': path.resolve('./node_modules/react-dom'),
    },
  },
  define: {
    __IS_SEARCH_ENABLED__: isSearchEnabled,
  },
  clearScreen: false,
  server: {
    port: 5174,
    strictPort: true,
  },
  envPrefix: ['VITE_', 'TAURI_'],
  build: {
    target: ['es2021', 'chrome100', 'safari13'],
    minify: !process.env.TAURI_DEBUG && !isAlphaChannel ? true : false,
    sourcemap: !!process.env.TAURI_DEBUG || isAlphaChannel,
  },
})
