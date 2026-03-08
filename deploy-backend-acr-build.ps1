# Script alternativo para deploy usando Azure ACR Build (no requiere Docker local)

param(
    [string]$ResourceGroup = "siinadseg-rg",
    [string]$ContainerName = "siinadseg-backend",
    [string]$ImageName = "siinadseg-backend:latest",
    [string]$Location = "westus",
    [string]$AcrName = "siinadsegacr"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Desplegando Backend usando Azure ACR Build" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar autenticacion en Azure
Write-Host "Verificando autenticacion en Azure..." -ForegroundColor Yellow
az account show > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "No estas autenticado en Azure. Ejecutando 'az login'..." -ForegroundColor Red
    az login
}
Write-Host "Autenticado correctamente" -ForegroundColor Green
Write-Host ""

# Verificar/Crear Azure Container Registry
Write-Host "Verificando Azure Container Registry..." -ForegroundColor Yellow
$acrExists = az acr show --name $AcrName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Creando Azure Container Registry: $AcrName" -ForegroundColor Yellow
    az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --location $Location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al crear ACR" -ForegroundColor Red
        exit 1
    }
}
Write-Host "ACR disponible: $AcrName" -ForegroundColor Green
Write-Host ""

# Habilitar admin en ACR
Write-Host "Habilitando admin en ACR..." -ForegroundColor Yellow
az acr update --name $AcrName --admin-enabled true
Write-Host ""

# Usar ACR Build para construir la imagen en la nube
Write-Host "Construyendo imagen en Azure ACR Build (puede tardar varios minutos)..." -ForegroundColor Yellow
az acr build --registry $AcrName --image $ImageName --file backend/Dockerfile backend

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al construir la imagen en ACR" -ForegroundColor Red
    exit 1
}
Write-Host "Imagen construida exitosamente en ACR" -ForegroundColor Green
Write-Host ""

# Obtener credenciales de ACR
Write-Host "Obteniendo credenciales de ACR..." -ForegroundColor Yellow
$acrUsername = az acr credential show --name $AcrName --query username -o tsv
$acrPassword = az acr credential show --name $AcrName --query "passwords[0].value" -o tsv
$acrLoginServer = az acr show --name $AcrName --query loginServer -o tsv
Write-Host "ACR Login Server: $acrLoginServer" -ForegroundColor Cyan
Write-Host ""

# Leer configuracion de la base de datos
if (-not (Test-Path "new-database-config.json")) {
    Write-Host "Error: No se encuentra new-database-config.json" -ForegroundColor Red
    exit 1
}
$dbConfig = Get-Content "new-database-config.json" | ConvertFrom-Json

# Eliminar el contenedor existente si existe
Write-Host "Verificando contenedor existente..." -ForegroundColor Yellow
$existingContainer = az container show --name $ContainerName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Eliminando contenedor existente..." -ForegroundColor Yellow
    az container delete --name $ContainerName --resource-group $ResourceGroup --yes
    Write-Host "Esperando a que se elimine completamente..." -ForegroundColor Yellow
    Start-Sleep -Seconds 15
}
Write-Host ""

# Crear nuevo contenedor con la nueva configuracion
Write-Host "Desplegando nuevo contenedor..." -ForegroundColor Yellow
$fullImageName = "$acrLoginServer/$ImageName"
az container create `
    --resource-group $ResourceGroup `
    --name $ContainerName `
    --image $fullImageName `
    --cpu 1 `
    --memory 1 `
    --os-type Linux `
    --registry-login-server $acrLoginServer `
    --registry-username $acrUsername `
    --registry-password $acrPassword `
    --dns-name-label $ContainerName `
    --ports 80 `
    --environment-variables `
        "ASPNETCORE_ENVIRONMENT=Production" `
        "ConnectionStrings__DefaultConnection=$($dbConfig.connectionString)" `
    --location $Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al crear el contenedor" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Obtener la URL del contenedor
$containerFQDN = az container show --name $ContainerName --resource-group $ResourceGroup --query "ipAddress.fqdn" -o tsv
$backendUrl = "http://$containerFQDN"

Write-Host "========================================" -ForegroundColor Green
Write-Host "Backend desplegado exitosamente!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "URL del backend: $backendUrl" -ForegroundColor Cyan
Write-Host "URL API: $backendUrl/api" -ForegroundColor Cyan
Write-Host ""

# Actualizar azure-deployment-config.json con la nueva URL del backend
if (Test-Path "azure-deployment-config.json") {
    Write-Host "Actualizando configuracion de Azure con la nueva URL..." -ForegroundColor Yellow
    $azureConfig = Get-Content "azure-deployment-config.json" | ConvertFrom-Json
    $azureConfig.azure.resources.backendContainer = $backendUrl
    $azureConfig.azure.endpoints.api = "$backendUrl/api"
    $azureConfig.azure.endpoints.auth = "$backendUrl/api/auth"
    $azureConfig.azure.endpoints.polizas = "$backendUrl/api/polizas"
    $azureConfig | ConvertTo-Json -Depth 10 | Set-Content "azure-deployment-config.json" -Encoding UTF8
    Write-Host "  OK Configuracion actualizada" -ForegroundColor Green
}
Write-Host ""

Write-Host "Verificando estado del contenedor..." -ForegroundColor Yellow
Write-Host ""
az container show --name $ContainerName --resource-group $ResourceGroup --query "{Name:name, State:instanceView.state, IP:ipAddress.ip, FQDN:ipAddress.fqdn}" -o table
Write-Host ""

Write-Host "Para ver logs del contenedor:" -ForegroundColor Cyan
Write-Host "  az container logs --name $ContainerName --resource-group $ResourceGroup" -ForegroundColor Yellow
Write-Host ""
Write-Host "Para verificar que funciona:" -ForegroundColor Cyan
Write-Host "  curl $backendUrl/api/health" -ForegroundColor Yellow
Write-Host ""
