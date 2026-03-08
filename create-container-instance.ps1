# Script para crear Azure Container Instance después del build
# Ejecutar este script DESPUÉS de que termine el build en ACR

$resourceGroup = "rg-siinadseg-prod-2025"
$location = "eastus2"
$containerName = "siinadseg-backend"
$acrName = "acrsiinadseg7512"
$imageName = "siinadseg-backend"
$dnsLabel = "siinadseg-api-$(Get-Random -Minimum 1000 -Maximum 9999)"

Write-Host "=== CREANDO AZURE CONTAINER INSTANCE ===" -ForegroundColor Green

# 1. Obtener credenciales ACR
Write-Host "`n[1/3] Obteniendo credenciales ACR..." -ForegroundColor Cyan
$acrUsername = az acr credential show --name $acrName --query "username" -o tsv
$acrPassword = az acr credential show --name $acrName --query "passwords[0].value" -o tsv

if (!$acrUsername -or !$acrPassword) {
    Write-Host "Error obteniendo credenciales ACR" -ForegroundColor Red
    exit 1
}

Write-Host "ACR Username: $acrUsername" -ForegroundColor Yellow

# 2. Connection string para la BD
$connectionString = "Server=tcp:siinadseg-sql-3376.database.windows.net,1433;Initial Catalog=SiinadsegDB;Persist Security Info=False;User ID=siinadsegadmin;Password=n-IC*6GNdiKvuk#P;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 3. Crear Container Instance
Write-Host "`n[2/3] Creando Azure Container Instance..." -ForegroundColor Cyan
Write-Host "Esto puede tardar 1-2 minutos..." -ForegroundColor Yellow

az container create `
    --resource-group $resourceGroup `
    --name $containerName `
    --image "$acrName.azurecr.io/${imageName}:latest" `
    --registry-login-server "$acrName.azurecr.io" `
    --registry-username $acrUsername `
    --registry-password $acrPassword `
    --dns-name-label $dnsLabel `
    --ports 8080 `
    --os-type Linux `
    --environment-variables `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "ASPNETCORE_URLS=http://+:8080" `
        "ConnectionStrings__DefaultConnection=$connectionString" `
        "Jwt__Secret=MySecretKey123456789MySecretKey123456789" `
        "Jwt__Issuer=SiinadsegApp" `
        "Jwt__Audience=SiinadsegApp" `
        "Jwt__ExpirationHours=8" `
    --cpu 1 `
    --memory 1.5 `
    --location $location `
    --restart-policy Always

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nError creando Container Instance" -ForegroundColor Red
    exit 1
}

Write-Host "`nContainer Instance creado exitosamente" -ForegroundColor Green

# 4. Obtener información del contenedor
Write-Host "`n[3/3] Obteniendo información del contenedor..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

$fqdn = az container show --resource-group $resourceGroup --name $containerName --query "ipAddress.fqdn" -o tsv
$state = az container show --resource-group $resourceGroup --name $containerName --query "instanceView.state" -o tsv

$backendUrl = "http://${fqdn}:8080"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "BACKEND DEPLOYMENT COMPLETADO" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "`nEstado: $state" -ForegroundColor $(if ($state -eq "Running") { "Green" } else { "Yellow" })
Write-Host "`nURLs del Backend:" -ForegroundColor Yellow
Write-Host "  Base URL: $backendUrl" -ForegroundColor White
Write-Host "  API: $backendUrl/api" -ForegroundColor White
Write-Host "  Health: $backendUrl/health" -ForegroundColor White
Write-Host "  Swagger: $backendUrl/swagger" -ForegroundColor White
Write-Host "`nProbar conexión:" -ForegroundColor Cyan
Write-Host "  curl $backendUrl/health" -ForegroundColor Gray
Write-Host "`nPróximo paso:" -ForegroundColor Magenta
Write-Host "  1. Deploy del frontend a Azure Static Web Apps" -ForegroundColor White
Write-Host "  2. Configurar backend URL en environment.prod.ts: $backendUrl" -ForegroundColor White
Write-Host "  3. Configurar CORS en el backend con la URL del frontend" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan

# Guardar información
$deploymentInfo = @"
# BACKEND DEPLOYMENT INFO
Fecha: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Backend
URL Base: $backendUrl
API: $backendUrl/api
Health Check: $backendUrl/health
Swagger: $backendUrl/swagger
Estado: $state

## Base de Datos
Server: siinadseg-sql-3376.database.windows.net
Database: SiinadsegDB
User: siinadsegadmin
Password: n-IC*6GNdiKvuk#P

## Usuario Administrador
Email: admin@sinseg.com
Password: admin123

## Recursos Azure
Resource Group: $resourceGroup
Container Instance: $containerName
ACR: $acrName.azurecr.io
DNS Label: $dnsLabel

## Para actualizar el backend
1. Hacer cambios en el código
2. Ejecutar: az acr build --registry $acrName --image ${imageName}:latest --file Dockerfile .
3. Reiniciar container: az container restart --resource-group $resourceGroup --name $containerName

## Logs del contenedor
az container logs --resource-group $resourceGroup --name $containerName --follow
"@

$deploymentInfo | Out-File -FilePath "backend-deployment-info.txt" -Encoding UTF8
Write-Host "`nInformacion guardada en: backend-deployment-info.txt" -ForegroundColor Green
