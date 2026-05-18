import type { Config } from 'tailwindcss'
import typography from '@tailwindcss/typography'
import plugin from 'tailwindcss/plugin'
import tailwindcssAnimate from 'tailwindcss-animate'

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        forge: {
          50:  '#f0f4ff',
          100: '#e0e9ff',
          500: '#4f6ef7',
          600: '#3b54d9',
          700: '#2c3eb5',
          900: '#1a2470',
        },
        surface: {
          DEFAULT: '#0f1117',
          card:    '#1a1d27',
          border:  '#2a2d3e',
          hover:   '#22263a',
        },
        status: {
          idle:     '#6b7280',
          working:  '#4f6ef7',
          blocked:  '#ef4444',
          finished: '#22c55e',
          done:     '#22c55e',
          pending:  '#f59e0b',
          approved: '#22c55e',
          rejected: '#ef4444',
        },
      },
    },
  },
  plugins: [
    typography,
    tailwindcssAnimate,
    plugin(({ addBase }) => {
      addBase({
        '@media (prefers-reduced-motion: reduce)': {
          '*': {
            'animation-duration': '0.01ms !important',
            'transition-duration': '0.01ms !important',
          },
        },
      })
    }),
  ],
} satisfies Config
