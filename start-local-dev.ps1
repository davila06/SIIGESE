# SIIGESE - Desarrollo Local PowerShell
Write-Host "============================================" -ForegroundColor Cyan
Write-Host " SIIGESE - Inicio de Desarrollo Local" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Configurando entorno local..." -ForegroundColor Yellow
Write-Host ""

# Cambiar al directorio del frontend
Set-Location -Path "$PSScriptRoot\frontend-new"

Write-Host "Verificando dependencias de Node.js..." -ForegroundColor Green
npm install

Write-Host ""
Write-Host "Iniciando servidor de desarrollo local..." -ForegroundColor Green
Write-Host "URL: http://localhost:4200" -ForegroundColor White
Write-Host "Configuracion: Local (Mock API)" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales de prueba:" -ForegroundColor Yellow
Write-Host "Usuario: admin@sinseg.com" -ForegroundColor White
Write-Host "Password: password123" -ForegroundColor White
Write-Host ""
Write-Host "Presiona Ctrl+C para detener el servidor" -ForegroundColor Red
Write-Host ""

# Iniciar el servidor de desarrollo
npm run start:local