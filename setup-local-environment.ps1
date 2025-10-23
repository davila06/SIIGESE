# ===============================================
# SIIGESE - Configuración Completa de Entorno
# ===============================================

Write-Host "🚀 SIIGESE - Configuración de Entorno de Desarrollo" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar Node.js
Write-Host "📦 Verificando Node.js..." -ForegroundColor Yellow
try {
    $nodeVersion = node --version
    Write-Host "✅ Node.js instalado: $nodeVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ Node.js no encontrado. Por favor instala Node.js desde https://nodejs.org" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Verificar npm
Write-Host "📦 Verificando npm..." -ForegroundColor Yellow
try {
    $npmVersion = npm --version
    Write-Host "✅ npm instalado: v$npmVersion" -ForegroundColor Green
} catch {
    Write-Host "❌ npm no encontrado" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Cambiar al directorio del frontend
Write-Host "📁 Configurando directorio..." -ForegroundColor Yellow
Set-Location -Path "$PSScriptRoot\frontend-new"

# Instalar dependencias
Write-Host "📥 Instalando dependencias..." -ForegroundColor Yellow
npm install

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Dependencias instaladas correctamente" -ForegroundColor Green
} else {
    Write-Host "❌ Error instalando dependencias" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Probar build local
Write-Host "🔨 Probando build local..." -ForegroundColor Yellow
npm run build:local

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build local exitoso" -ForegroundColor Green
} else {
    Write-Host "❌ Error en build local" -ForegroundColor Red
    Read-Host "Presiona Enter para salir"
    exit 1
}

# Resumen de configuración
Write-Host ""
Write-Host "🎉 CONFIGURACIÓN COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 Comandos disponibles:" -ForegroundColor Cyan
Write-Host "  npm run start:local    - Servidor de desarrollo local" -ForegroundColor White
Write-Host "  npm run build:local    - Build para desarrollo local" -ForegroundColor White
Write-Host "  npm run watch:local    - Build continuo local" -ForegroundColor White
Write-Host ""
Write-Host "🌐 URLs de acceso:" -ForegroundColor Cyan
Write-Host "  Local:      http://localhost:4200" -ForegroundColor White
Write-Host "  Producción: https://polite-flower-0f3168a1e.3.azurestaticapps.net" -ForegroundColor White
Write-Host ""
Write-Host "🔐 Credenciales:" -ForegroundColor Cyan
Write-Host "  Usuario: admin@sinseg.com" -ForegroundColor White
Write-Host "  Password: password123" -ForegroundColor White
Write-Host ""
Write-Host "📚 Documentación:" -ForegroundColor Cyan
Write-Host "  README_LOCAL_DEV.md - Guía completa de desarrollo local" -ForegroundColor White
Write-Host "  ACCESS_INFO.md      - Información de acceso y credenciales" -ForegroundColor White
Write-Host ""

$choice = Read-Host "¿Quieres iniciar el servidor de desarrollo ahora? (S/N)"
if ($choice -eq "S" -or $choice -eq "s" -or $choice -eq "Y" -or $choice -eq "y") {
    Write-Host ""
    Write-Host "🚀 Iniciando servidor de desarrollo local..." -ForegroundColor Green
    Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Red
    Write-Host ""
    npm run start:local
} else {
    Write-Host ""
    Write-Host "👍 Para iniciar el servidor más tarde, ejecuta:" -ForegroundColor Yellow
    Write-Host "  cd frontend-new" -ForegroundColor White
    Write-Host "  npm run start:local" -ForegroundColor White
    Write-Host ""
}

Read-Host "Presiona Enter para salir"