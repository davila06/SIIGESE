const http = require('http');
const https = require('https');
const fs = require('fs');
const path = require('path');
const url = require('url');

const PORT = 4200;
const DIST_PATH = path.join(__dirname, 'dist', 'frontend-new');

// Función para hacer proxy a la API
function proxyRequest(req, res) {
  const options = {
    hostname: 'localhost',
    port: 5000,
    path: req.url,
    method: req.method,
    headers: {
      ...req.headers,
      'host': 'localhost:5000'
    }
  };

  const proxyReq = http.request(options, (proxyRes) => {
    // Copiar headers de respuesta
    res.writeHead(proxyRes.statusCode, proxyRes.headers);
    proxyRes.pipe(res);
  });

  proxyReq.on('error', (err) => {
    console.error('Error en proxy:', err);
    res.writeHead(500);
    res.end('Error en proxy');
  });

  req.pipe(proxyReq);
}

// Función para servir archivos estáticos
function serveStatic(req, res, filePath) {
  fs.readFile(filePath, (err, data) => {
    if (err) {
      res.writeHead(404);
      res.end('File not found');
      return;
    }

    const ext = path.extname(filePath);
    const contentType = {
      '.html': 'text/html',
      '.js': 'application/javascript',
      '.css': 'text/css',
      '.json': 'application/json',
      '.png': 'image/png',
      '.jpg': 'image/jpeg',
      '.ico': 'image/x-icon'
    }[ext] || 'text/plain';

    res.writeHead(200, { 'Content-Type': contentType });
    res.end(data);
  });
}

const server = http.createServer((req, res) => {
  const parsedUrl = url.parse(req.url);
  
  // Si es una llamada a API, hacer proxy
  if (parsedUrl.pathname.startsWith('/api')) {
    proxyRequest(req, res);
    return;
  }

  // Determinar qué archivo servir
  let filePath = path.join(DIST_PATH, parsedUrl.pathname);
  
  // Si es un directorio o no existe, servir index.html (SPA)
  if (parsedUrl.pathname === '/' || !fs.existsSync(filePath)) {
    filePath = path.join(DIST_PATH, 'index.html');
  }

  serveStatic(req, res, filePath);
});

server.listen(PORT, () => {
  console.log(`Servidor frontend corriendo en http://localhost:${PORT}`);
  console.log(`Proxy API configurado para http://localhost:5000`);
  console.log(`Sirviendo archivos desde: ${DIST_PATH}`);
});