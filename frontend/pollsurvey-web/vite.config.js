import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        // Đã sửa từ localhost:5000 thành 127.0.0.1:5139
        target: 'http://127.0.0.1:5139', 
        changeOrigin: true,
        rewrite: (path) => path.replace(/^\/api/, '/api')
      },
      '/pollHub': {
        // Đã sửa cho phần kết nối Real-time (SignalR) đồng bộ với backend
        target: 'http://127.0.0.1:5139', 
        ws: true,
        changeOrigin: true
      }
    }
  }
})