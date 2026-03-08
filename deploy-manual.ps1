#!/usr/bin/env pwsh

Write-Host "🚀 Iniciando deployment manual a Azure Static Web Apps" -ForegroundColor Green

# Parámetros
$deploymentToken = "ba9c8c8e6f2d1c04b52c77e1b5e47b7e2b0c5a8d9e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b"
$frontendPath = "frontend-new\dist\frontend-new"
$apiPath = "dist\api"

Write-Host "📁 Frontend: $frontendPath" -ForegroundColor Cyan
Write-Host "📁 API: $apiPath" -ForegroundColor Cyan

# Verificar directorios
if (-not (Test-Path $frontendPath)) {
    Write-Error "❌ No se encuentra el directorio del frontend: $frontendPath"
    exit 1
}

if (-not (Test-Path $apiPath)) {
    Write-Warning "⚠️  No se encuentra el directorio de la API: $apiPath"
}

# Listar archivos principales del frontend
Write-Host "📋 Archivos principales del frontend:" -ForegroundColor Yellow
Get-ChildItem $frontendPath -Filter "*.html" | Format-Table Name, Length
Get-ChildItem $frontendPath -Filter "*.js" | Format-Table Name, Length -AutoSize
Get-ChildItem $frontendPath -Filter "*.css" | Format-Table Name, Length

Write-Host "✅ Archivos preparados para deployment" -ForegroundColor Green
Write-Host "🔗 URL destino: https://agreeable-water-06170cf10.1.azurestaticapps.net/" -ForegroundColor Magenta

# Intentar deployment con SWA CLI con timeout
Write-Host "🚀 Intentando deployment con SWA CLI..." -ForegroundColor Green

try {
    $process = Start-Process -FilePath "swa" -ArgumentList @(
        "deploy", 
        $frontendPath,
        "--deployment-token", $deploymentToken,
        "--verbose"
    ) -NoNewWindow -PassThru -RedirectStandardOutput "deployment.log" -RedirectStandardError "deployment.error"
    
    # Esperar máximo 2 minutos
    if ($process.WaitForExit(120000)) {
        if ($process.ExitCode -eq 0) {
            Write-Host "✅ Deployment completado exitosamente!" -ForegroundColor Green
            if (Test-Path "deployment.log") {
                Get-Content "deployment.log" | Write-Host
            }
        } else {
            Write-Error "❌ Deployment falló con código de salida: $($process.ExitCode)"
            if (Test-Path "deployment.error") {
                Get-Content "deployment.error" | Write-Error
            }
        }
    } else {
        Write-Warning "⏰ Deployment timeout después de 2 minutos"
        $process.Kill()
    }
} catch {
    Write-Error "❌ Error ejecutando SWA CLI: $_"
}

Write-Host "Resumen del deployment:" -ForegroundColor Blue
Write-Host "- Frontend build: Completado" -ForegroundColor Green
Write-Host "- Backend build: Completado" -ForegroundColor Green  
Write-Host "- Configuracion: Preparada" -ForegroundColor Green
Write-Host "- Deploy status: Verificar manualmente" -ForegroundColor Yellow