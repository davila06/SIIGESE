# Script para reconstruir y redesplegar el backend con la nueva configuracion

param(
    [string]$ResourceGroup = "siinadseg-rg",
    [string]$ContainerName = "siinadseg-backend",
    [string]$ImageName = "siinadseg-backend:latest",
    [string]$Location = "westus"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Reconstruyendo y desplegando Backend" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que Docker este corriendo
Write-Host "Verificando Docker..." -ForegroundColor Yellow
docker ps > $null 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Docker no esta corriendo" -ForegroundColor Red
    Write-Host "Por favor inicia Docker Desktop" -ForegroundColor Yellow
    exit 1
}
Write-Host "Docker esta corriendo" -ForegroundColor Green
Write-Host ""

# Construir la imagen Docker del backend
Write-Host "Construyendo imagen Docker del backend..." -ForegroundColor Yellow
Set-Location backend

docker build -t $ImageName -f Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al construir la imagen Docker" -ForegroundColor Red
    Set-Location ..
    exit 1
}
Set-Location ..
Write-Host "Imagen construida exitosamente" -ForegroundColor Green
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

# Crear o usar Azure Container Registry
$acrName = "siinadsegacr$(Get-Random -Minimum 100 -Maximum 999)"
Write-Host "Verificando Azure Container Registry..." -ForegroundColor Yellow

$acrExists = az acr show --name $acrName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Creando Azure Container Registry: $acrName" -ForegroundColor Yellow
    az acr create --resource-group $ResourceGroup --name $acrName --sku Basic --location $Location
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al crear ACR" -ForegroundColor Red
        exit 1
    }
}
Write-Host "ACR disponible: $acrName" -ForegroundColor Green
Write-Host ""

# Habilitar admin en ACR
Write-Host "Habilitando admin en ACR..." -ForegroundColor Yellow
az acr update --name $acrName --admin-enabled true
Write-Host ""

# Obtener credenciales de ACR
Write-Host "Obteniendo credenciales de ACR..." -ForegroundColor Yellow
$acrUsername = az acr credential show --name $acrName --query username -o tsv
$acrPassword = az acr credential show --name $acrName --query "passwords[0].value" -o tsv
$acrLoginServer = az acr show --name $acrName --query loginServer -o tsv
Write-Host "ACR Login Server: $acrLoginServer" -ForegroundColor Cyan
Write-Host ""

# Login en ACR
Write-Host "Autenticando con ACR..." -ForegroundColor Yellow
docker login $acrLoginServer -u $acrUsername -p $acrPassword
Write-Host ""

# Tag y push de la imagen
Write-Host "Etiquetando imagen para ACR..." -ForegroundColor Yellow
$acrImageName = "$acrLoginServer/$ImageName"
docker tag $ImageName $acrImageName
Write-Host ""

Write-Host "Subiendo imagen a ACR..." -ForegroundColor Yellow
docker push $acrImageName
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al subir la imagen" -ForegroundColor Red
    exit 1
}
Write-Host "Imagen subida exitosamente" -ForegroundColor Green
Write-Host ""

# Leer configuracion de la base de datos
$dbConfig = Get-Content "new-database-config.json" | ConvertFrom-Json

# Eliminar el contenedor existente si existe
Write-Host "Verificando contenedor existente..." -ForegroundColor Yellow
$existingContainer = az container show --name $ContainerName --resource-group $ResourceGroup 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "Eliminando contenedor existente..." -ForegroundColor Yellow
    az container delete --name $ContainerName --resource-group $ResourceGroup --yes
    Start-Sleep -Seconds 10
}
Write-Host ""

# Crear nuevo contenedor con la nueva configuracion
Write-Host "Desplegando nuevo contenedor..." -ForegroundColor Yellow
az container create `
    --resource-group $ResourceGroup `
    --name $ContainerName `
    --image $acrImageName `
    --cpu 1 `
    --memory 1 `
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

Write-Host "Siguiente paso:" -ForegroundColor Cyan
Write-Host "Verificar que el backend este funcionando:" -ForegroundColor White
Write-Host "  curl $backendUrl/api/health" -ForegroundColor Yellow
Write-Host ""
Write-Host "Si es necesario, actualiza el frontend con la nueva URL del backend" -ForegroundColor White
Write-Host ""
