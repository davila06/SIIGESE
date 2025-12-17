# Script simplificado para deploy usando Azure Container Instance
# No requiere Docker local

$resourceGroup = "rg-siinadseg-prod-2025"
$location = "eastus2"
$containerName = "siinadseg-backend"
$acrName = "acrsiinadseg$(Get-Random -Minimum 1000 -Maximum 9999)"
$imageName = "siinadseg-backend"
$backendPath = "backend"

Write-Host "=== DEPLOY BACKEND A AZURE ===" -ForegroundColor Green

# 1. Crear Azure Container Registry
Write-Host "`n[1/6] Creando Azure Container Registry..." -ForegroundColor Cyan
az acr create --resource-group $resourceGroup --name $acrName --sku Basic --location $location

if ($LASTEXITCODE -eq 0) {
    Write-Host "ACR creado: $acrName" -ForegroundColor Green
} else {
    Write-Host "Error creando ACR" -ForegroundColor Red
    exit 1
}

# 2. Habilitar admin en ACR
Write-Host "`n[2/6] Habilitando admin en ACR..." -ForegroundColor Cyan
az acr update --name $acrName --admin-enabled true

# 3. Obtener credenciales
Write-Host "`n[3/6] Obteniendo credenciales ACR..." -ForegroundColor Cyan
$acrUsername = az acr credential show --name $acrName --query "username" -o tsv
$acrPassword = az acr credential show --name $acrName --query "passwords[0].value" -o tsv

Write-Host "Username: $acrUsername" -ForegroundColor Yellow

# 4. Build y push de la imagen
Write-Host "`n[4/6] Construyendo imagen en Azure (esto puede tardar 3-5 minutos)..." -ForegroundColor Cyan
Set-Location $backendPath
az acr build --registry $acrName --image "${imageName}:latest" --file Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error en build" -ForegroundColor Red
    Set-Location ..
    exit 1
}

Set-Location ..
Write-Host "Imagen construida exitosamente" -ForegroundColor Green

# 5. Crear connection string para container
$connectionString = "Server=tcp:siinadseg-sql-3376.database.windows.net,1433;Initial Catalog=SiinadsegDB;Persist Security Info=False;User ID=siinadsegadmin;Password=n-IC*6GNdiKvuk#P;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 6. Crear Azure Container Instance
Write-Host "`n[5/6] Creando Azure Container Instance..." -ForegroundColor Cyan
az container create `
    --resource-group $resourceGroup `
    --name $containerName `
    --image "$acrName.azurecr.io/${imageName}:latest" `
    --registry-login-server "$acrName.azurecr.io" `
    --registry-username $acrUsername `
    --registry-password $acrPassword `
    --dns-name-label "siinadseg-backend-$(Get-Random -Minimum 1000 -Maximum 9999)" `
    --ports 8080 `
    --environment-variables `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "ConnectionStrings__DefaultConnection=$connectionString" `
        "Jwt__Secret=MySecretKey123456789MySecretKey123456789" `
        "Jwt__Issuer=SiinadsegApp" `
        "Jwt__Audience=SiinadsegApp" `
        "Jwt__ExpirationHours=8" `
    --cpu 1 `
    --memory 1.5 `
    --location $location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creando Container Instance" -ForegroundColor Red
    exit 1
}

# 7. Obtener FQDN
Write-Host "`n[6/6] Obteniendo URL del backend..." -ForegroundColor Cyan
$fqdn = az container show --resource-group $resourceGroup --name $containerName --query "ipAddress.fqdn" -o tsv
$backendUrl = "http://${fqdn}:8080"

Write-Host "`n=== DEPLOY COMPLETADO ===" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Backend URL: $backendUrl" -ForegroundColor Yellow
Write-Host "API URL: $backendUrl/api" -ForegroundColor Yellow
Write-Host "Health: $backendUrl/health" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

# Guardar información
$deployInfo = @"
# DEPLOYMENT INFO - $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Recursos Creados
Resource Group: $resourceGroup
SQL Server: siinadseg-sql-3376.database.windows.net
Database: SiinadsegDB
ACR: $acrName.azurecr.io
Container Instance: $containerName

## URLs
Backend: $backendUrl
API: $backendUrl/api
Health Check: $backendUrl/health

## Credenciales Base de Datos
Server: siinadseg-sql-3376.database.windows.net
Database: SiinadsegDB
User: siinadsegadmin
Password: n-IC*6GNdiKvuk#P

## Usuario Administrador
Email: admin@sinseg.com
Password: admin123

## Proximo Paso
Deploy del frontend y configurar la URL del backend en environment.ts
"@

$deployInfo | Out-File -FilePath "deployment-info-complete.txt" -Encoding UTF8
Write-Host "`nInformacion guardada en: deployment-info-complete.txt" -ForegroundColor Green

Write-Host "`nProbar backend con:" -ForegroundColor Magenta
Write-Host "curl $backendUrl/health" -ForegroundColor White
