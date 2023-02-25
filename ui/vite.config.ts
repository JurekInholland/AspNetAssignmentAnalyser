import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vitejs.dev/config/
export default defineConfig({
  
  logLevel: 'info',
  // build: {
  //   outDir: '../Api/wwwroot',
  // },
  plugins: [vue()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:5003',
        changeOrigin: true,
        secure: false,
        ws: true,
      }
    }
  }

})
