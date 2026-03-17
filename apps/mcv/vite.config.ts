import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import path from 'path'

const isAlphaChannel = process.env.MCV_CHANNEL === 'alpha'

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      react: path.resolve('./node_modules/react'),
      'react-dom': path.resolve('./node_modules/react-dom'),
    },
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
