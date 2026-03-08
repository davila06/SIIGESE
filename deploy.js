const { execSync } = require('child_process');
const fs = require('fs');
const path = require('path');

console.log('🚀 Iniciando deployment a Azure Static Web Apps...');

const frontendPath = 'frontend-new/dist/frontend-new';
const apiPath = 'dist/api';
const deploymentToken = 'ba9c8c8e6f2d1c04b52c77e1b5e47b7e2b0c5a8d9e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b';

// Verificar que los directorios existen
if (!fs.existsSync(frontendPath)) {
    console.error('❌ Error: No se encuentra el directorio del frontend:', frontendPath);
    process.exit(1);
}

console.log('✅ Frontend encontrado:', frontendPath);

// Listar archivos principales
console.log('📋 Archivos principales:');
const files = fs.readdirSync(frontendPath);
files.filter(f => f.endsWith('.html') || f.endsWith('.js') || f.endsWith('.css')).forEach(file => {
    const filePath = path.join(frontendPath, file);
    const stats = fs.statSync(filePath);
    console.log(`  - ${file} (${Math.round(stats.size / 1024)}KB)`);
});

// Verificar staticwebapp.config.json
const configPath = path.join(frontendPath, 'staticwebapp.config.json');
if (fs.existsSync(configPath)) {
    console.log('✅ Configuración encontrada: staticwebapp.config.json');
} else {
    console.log('⚠️  No se encuentra staticwebapp.config.json');
}

try {
    console.log('🚀 Ejecutando SWA CLI...');
    
    // Comando SWA con timeout
    const command = `npx @azure/static-web-apps-cli deploy "${frontendPath}" --deployment-token "${deploymentToken}" --verbose`;
    
    console.log('📝 Comando:', command);
    
    const result = execSync(command, { 
        encoding: 'utf8', 
        timeout: 180000, // 3 minutos
        stdio: 'inherit'
    });
    
    console.log('✅ Deployment completado exitosamente!');
    
} catch (error) {
    console.error('❌ Error durante el deployment:', error.message);
    
    // Intentar método alternativo o mostrar instrucciones manuales
    console.log('\n📋 Para deployment manual:');
    console.log('1. Ve a: https://portal.azure.com');
    console.log('2. Busca tu Azure Static Web App: agreeable-water-06170cf10');
    console.log('3. Sube los archivos del directorio:', path.resolve(frontendPath));
    console.log('4. URL final: https://agreeable-water-06170cf10.1.azurestaticapps.net/');
}

console.log('\n🎯 URL de destino: https://agreeable-water-06170cf10.1.azurestaticapps.net/');