/** @type {import('tailwindcss').Config} */
export default {
  darkMode: 'class',
  content: [
    "./index.html",
    "./src/**/*.{js,ts,jsx,tsx}",
  ],
  safelist: [
    'grid',
    'grid-cols-2',
    'gap-2',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
