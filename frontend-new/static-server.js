const express = require('express');
const path = require('path');

const app = express();
const PORT = 4200;

// Servir archivos estáticos de Angular desde dist
app.use(express.static(path.join(__dirname, 'dist', 'frontend-new')));

// Rutas específicas de Angular
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist', 'frontend-new', 'index.html'));
});

app.get('/cobros', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist', 'frontend-new', 'index.html'));
});

app.get('/cobros/dashboard', (req, res) => {
  res.sendFile(path.join(__dirname, 'dist', 'frontend-new', 'index.html'));
});

app.listen(PORT, () => {
  console.log(`Frontend server running on http://localhost:${PORT}`);
  console.log(`Serving files from: ${path.join(__dirname, 'dist', 'frontend-new')}`);
});