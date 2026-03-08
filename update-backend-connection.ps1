# Script para actualizar la configuracion del backend con la nueva base de datos

param(
    [string]$ConfigFile = "new-database-config.json"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Actualizando configuracion del Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que exista el archivo de configuracion
if (-not (Test-Path $ConfigFile)) {
    Write-Host "Error: No se encuentra el archivo $ConfigFile" -ForegroundColor Red
    Write-Host "Primero ejecuta: .\create-new-azure-database.ps1" -ForegroundColor Yellow
    exit 1
}

# Leer configuracion
$config = Get-Content $ConfigFile | ConvertFrom-Json
$connectionString = $config.connectionString

Write-Host "Leyendo configuracion de base de datos..." -ForegroundColor Yellow
Write-Host "  Servidor: $($config.server)" -ForegroundColor White
Write-Host "  Base de datos: $($config.database)" -ForegroundColor White
Write-Host ""

# Actualizar appsettings.Production.json del backend
$backendAppSettingsPath = "backend\src\WebApi\appsettings.Production.json"

if (Test-Path $backendAppSettingsPath) {
    Write-Host "Actualizando: $backendAppSettingsPath" -ForegroundColor Yellow
    
    $appSettings = Get-Content $backendAppSettingsPath -Raw | ConvertFrom-Json
    $appSettings.ConnectionStrings.DefaultConnection = $connectionString
    
    $appSettings | ConvertTo-Json -Depth 10 | Set-Content $backendAppSettingsPath -Encoding UTF8
    Write-Host "  OK Actualizado correctamente" -ForegroundColor Green
} else {
    Write-Host "  NOTA: No se encontro: $backendAppSettingsPath" -ForegroundColor Yellow
}
Write-Host ""

# Actualizar azure-deployment-config.json
$azureConfigPath = "azure-deployment-config.json"

if (Test-Path $azureConfigPath) {
    Write-Host "Actualizando: $azureConfigPath" -ForegroundColor Yellow
    
    $azureConfig = Get-Content $azureConfigPath -Raw | ConvertFrom-Json
    $azureConfig.azure.resources.sqlServer = $config.server
    $azureConfig.azure.resources.database = $config.database
    $azureConfig.azure.credentials.sqlUsername = $config.username
    $azureConfig.azure.credentials.sqlPassword = $config.password
    $azureConfig.deployment.lastUpdate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    
    $azureConfig | ConvertTo-Json -Depth 10 | Set-Content $azureConfigPath -Encoding UTF8
    Write-Host "  OK Actualizado correctamente" -ForegroundColor Green
} else {
    Write-Host "  NOTA: No se encontro: $azureConfigPath" -ForegroundColor Yellow
}
Write-Host ""

Write-Host "========================================" -ForegroundColor Green
Write-Host "Configuracion actualizada exitosamente!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Cyan
Write-Host "Reconstruir y redesplegar el contenedor del backend:" -ForegroundColor White
Write-Host "  .\rebuild-and-deploy-backend.ps1" -ForegroundColor Yellow
Write-Host ""
