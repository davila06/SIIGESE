# Script para deploy manual forzado de Azure Static Web Apps

$deployToken = "cde1c36ae1c9a4bebe5d36bd53cfc1106b79a6c2c9bbe86cbd0d1125a0749b9203-76b82a6a-0887-4448-9ad2-ef74b8d5343c00f040202a282f0f"
$appPath = "frontend-new\dist\frontend-new"
$apiPath = "frontend-new\api"

Write-Host "=== Deploy Manual a Azure Static Web Apps ===" -ForegroundColor Cyan
Write-Host ""

# Verificar que los directorios existen
if (-not (Test-Path $appPath)) {
    Write-Host "Error: No existe $appPath" -ForegroundColor Red
    exit 1
}

if (-not (Test-Path $apiPath)) {
    Write-Host "Warning: No existe $apiPath (se deployará sin API)" -ForegroundColor Yellow
    $apiParam = ""
} else {
    $apiParam = "--api-location `"$apiPath`""
}

Write-Host "Deployando desde: $appPath" -ForegroundColor Green
Write-Host "API desde: $apiPath" -ForegroundColor Green
Write-Host ""

# Intentar con npx
Write-Host "Ejecutando deploy..." -ForegroundColor Yellow
$cmd = "npx @azure/static-web-apps-cli@latest deploy `"$appPath`" $apiParam --deployment-token `"$deployToken`" --env production"

Write-Host "Comando: $cmd" -ForegroundColor Gray
Write-Host ""

Invoke-Expression $cmd

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en el deploy" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Deploy completado ===" -ForegroundColor Green
Write-Host "URL: https://green-beach-02a282f0f.3.azurestaticapps.net" -ForegroundColor Cyan
Write-Host ""
Write-Host "Recuerda hacer hard refresh (Ctrl+Shift+R) en el navegador" -ForegroundColor Yellow
