const express = require('express');
const { createProxyMiddleware } = require('http-proxy-middleware');
const path = require('path');

const app = express();
const PORT = 4200;

// Configurar proxy para rutas de API
app.use('/api', createProxyMiddleware({
  target: 'http://localhost:5000',
  changeOrigin: true,
  logLevel: 'debug'
}));

// Servir archivos estáticos de Angular
app.use(express.static(path.join(__dirname, 'dist/frontend-new')));

// Manejar rutas de Angular (SPA)
app.get('*', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist/frontend-new/index.html'));
});

app.listen(PORT, () => {
  console.log(`Frontend server running on http://localhost:${PORT}`);
  console.log(`API proxy configured to http://localhost:5000`);
});