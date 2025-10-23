#!/usr/bin/env pwsh

Write-Host "🔧 SIINADSEG - Prueba Local con Backend" -ForegroundColor Green
Write-Host "==========================================" -ForegroundColor Cyan

Write-Host "📋 Pasos para probar:" -ForegroundColor Yellow
Write-Host "1. Ejecutar start-backend.bat en una terminal" -ForegroundColor White
Write-Host "2. Ejecutar este script en otra terminal" -ForegroundColor White
Write-Host "3. Abrir http://localhost:4200 en el navegador" -ForegroundColor White
Write-Host ""

# Verificar si el backend está corriendo
Write-Host "🔍 Verificando backend local..." -ForegroundColor Cyan
try {
    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method GET -UseBasicParsing -TimeoutSec 5 -ErrorAction Stop
    Write-Host "❌ Backend respondió con GET (debe usar POST)" -ForegroundColor Red
} catch {
    if ($_.Exception.Message -like "*404*") {
        Write-Host "❌ Backend no está ejecutándose en puerto 5000" -ForegroundColor Red
        Write-Host "   Ejecuta: start-backend.bat" -ForegroundColor Yellow
        exit 1
    } elseif ($_.Exception.Message -like "*conexión*" -or $_.Exception.Message -like "*connection*") {
        Write-Host "❌ No se puede conectar al backend" -ForegroundColor Red
        Write-Host "   Ejecuta: start-backend.bat" -ForegroundColor Yellow
        exit 1
    } else {
        Write-Host "✅ Backend parece estar corriendo" -ForegroundColor Green
    }
}

# Probar login con backend local
Write-Host "🔐 Probando login con admin..." -ForegroundColor Cyan
try {
    $body = @{
        email = "admin@sinseg.com"
        password = "password123"
    } | ConvertTo-Json

    $response = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $body -ContentType "application/json" -UseBasicParsing
    
    Write-Host "✅ Login exitoso! Status: $($response.StatusCode)" -ForegroundColor Green
    $result = $response.Content | ConvertFrom-Json
    Write-Host "🔑 Token generado (primeros 50 chars): $($result.token.Substring(0, 50))..." -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error en login: $($_.Exception.Message)" -ForegroundColor Red
}

# Configurar frontend para desarrollo local
Write-Host ""
Write-Host "🌐 Configurando frontend..." -ForegroundColor Cyan
Set-Location "frontend-new"

# Crear archivo temporal para desarrollo local
$localConfig = @"
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000/api',
  version: '20251017-local',
  enableLogging: true
};
"@

$localConfig | Out-File -FilePath "src\environments\environment.ts" -Encoding UTF8 -Force

Write-Host "✅ Frontend configurado para usar backend local" -ForegroundColor Green

# Iniciar frontend
Write-Host "🚀 Iniciando frontend en http://localhost:4200..." -ForegroundColor Green
Write-Host "   Presiona Ctrl+C para detener" -ForegroundColor Yellow

ng serve --open --port 4200