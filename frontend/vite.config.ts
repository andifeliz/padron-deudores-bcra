import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// En desarrollo (npm run dev), Vite reenvía /api a las APIs levantadas con docker compose.
// En Docker lo hace nginx (nginx.conf), con las mismas rutas.
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5003,
    proxy: {
      "/api/importador": { target: "http://localhost:5001", rewrite: (ruta) => ruta.replace(/^\/api\/importador/, "") },
      "/api/consulta": { target: "http://localhost:5002", rewrite: (ruta) => ruta.replace(/^\/api\/consulta/, "") },
    },
  },
});
