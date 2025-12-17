# Script para deploy del frontend a Azure Static Web App

$resourceGroup = "rg-siinadseg-prod-2025"
$location = "eastus2"
$swaName = "swa-siinadseg-frontend"
$backendUrl = "http://siinadseg-api-7464.eastus2.azurecontainer.io:8080"

Write-Host "=== DEPLOY FRONTEND A AZURE STATIC WEB APP ===" -ForegroundColor Green

# 1. Crear Static Web App (modo manual sin GitHub)
Write-Host "`n[1/4] Creando Azure Static Web App..." -ForegroundColor Cyan
az staticwebapp create `
    --name $swaName `
    --resource-group $resourceGroup `
    --location $location `
    --sku Free

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creando Static Web App" -ForegroundColor Red
    exit 1
}

Write-Host "Static Web App creada exitosamente" -ForegroundColor Green

# 2. Obtener deployment token
Write-Host "`n[2/4] Obteniendo deployment token..." -ForegroundColor Cyan
$deploymentToken = az staticwebapp secrets list `
    --name $swaName `
    --resource-group $resourceGroup `
    --query "properties.apiKey" `
    --output tsv

if (!$deploymentToken) {
    Write-Host "Error obteniendo deployment token" -ForegroundColor Red
    exit 1
}

Write-Host "Deployment token obtenido" -ForegroundColor Green

# 3. Instalar SWA CLI si es necesario
Write-Host "`n[3/4] Verificando Azure Static Web Apps CLI..." -ForegroundColor Cyan
$swaInstalled = Get-Command swa -ErrorAction SilentlyContinue

if (!$swaInstalled) {
    Write-Host "Instalando Azure Static Web Apps CLI..." -ForegroundColor Yellow
  Deploy usando SWA CLI
swa deploy ./dist/frontend-new/browser `
    --deployment-token $deploymentToken `
    --env production

Set-Location ..

#

# Deploy usando SWA CLI
swa deploy ./dist/frontend-new/browser `
    --deployment-token $deploymentToken `
    --env production

Set-Location ..

# 4. Obtener URL del frontend
Write-Host "`nObteniendo URL del frontend..." -ForegroundColor Cyan
$frontendUrl = az staticwebapp show `
    --name $swaName `
    --resource-group $resourceGroup `
    --query "defaultHostname" `
    --output tsv

$frontendUrlComplete = "https://$frontendUrl"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "FRONTEND DEPLOYMENT COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nFrontend URL: $frontendUrlComplete" -ForegroundColor Yellow
Write-Host "Backend URL: $backendUrl/api" -ForegroundColor Yellow
Write-Host "`nPróximo paso:" -ForegroundColor Magenta
Write-Host "  Configurar CORS en el backend para permitir:" -ForegroundColor White
Write-Host "  $frontendUrlComplete" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

# Guardar información
$deploymentInfo = @"
# DEPLOYMENT COMPLETO - $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Frontend
URL: $frontendUrlComplete

## Backend  
URL: $backendUrl
API: $backendUrl/api

## Configuración pendiente
Agregar al appsettings.json del backend en Cors.AllowedOrigins:
  "$frontendUrlComplete"

## Usuario Administrador
Email: admin@sinseg.com
Password: admin123
"@

$deploymentInfo | Out-File -FilePath "deployment-complete-info.txt" -Encoding UTF8
Write-Host "`nInformacion guardada en: deployment-complete-info.txt" -ForegroundColor Green
