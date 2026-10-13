import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// The dev server runs on http://localhost:5173 (the origin the API allows through CORS).
export default defineConfig({
  plugins: [react()],
  server: {
    host: 'localhost',
    port: 5173,
    strictPort: true
  },
  preview: {
    host: 'localhost',
    port: 4173
  }
});
