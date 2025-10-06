const http = require('http');

const options = {
  hostname: 'localhost',
  port: 5000,
  path: '/api/cobros',
  method: 'GET',
  headers: {
    'Accept': 'application/json',
    'Content-Type': 'application/json'
  }
};

console.log('Enviando request a:', `http://${options.hostname}:${options.port}${options.path}`);

const req = http.request(options, (res) => {
  console.log('Status Code:', res.statusCode);
  console.log('Headers:', res.headers);
  
  let data = '';

  res.on('data', (chunk) => {
    data += chunk;
  });

  res.on('end', () => {
    try {
      const cobros = JSON.parse(data);
      console.log('\n=== RESPUESTA DEL API ===');
      console.log('Total de cobros:', cobros.length);
      
      if (cobros.length > 0) {
        console.log('\nPrimeros 2 cobros:');
        cobros.slice(0, 2).forEach((cobro, index) => {
          console.log(`\nCobro ${index + 1}:`);
          console.log('  - numeroRecibo:', cobro.numeroRecibo);
          console.log('  - estado:', cobro.estado, '(tipo:', typeof cobro.estado + ')');
          console.log('  - clienteNombre:', cobro.clienteNombre);
          console.log('  - clienteApellido:', cobro.clienteApellido);
        });
        
        console.log('\nTodos los estados únicos:');
        const estados = [...new Set(cobros.map(c => c.estado))];
        estados.forEach(estado => {
          const count = cobros.filter(c => c.estado === estado).length;
          console.log(`  - Estado ${estado} (${typeof estado}): ${count} cobros`);
        });
      }
    } catch (error) {
      console.error('Error parsing JSON:', error);
      console.log('Raw response:', data);
    }
  });
});

req.on('error', (error) => {
  console.error('Request error:', error);
});

req.end();