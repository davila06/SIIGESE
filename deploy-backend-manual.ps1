# ============================================
# DEPLOY MANUAL DEL BACKEND A AZURE
# Opción 2: Docker + Azure CLI
# ============================================

Write-Host "`n=== DEPLOY BACKEND A AZURE CONTAINER APP ===" -ForegroundColor Green

# Configuración
$registryName = "acrsiinadseg"
$registryServer = "acrsiinadseg.azurecr.io"
$imageName = "siinadseg-backend"
$imageTag = "latest"
$containerAppName = "app-siinadseg-backend"
$resourceGroup = "rg-siinadseg"
$backendPath = "C:\Users\davil\SINSEG\enterprise-web-app\backend"

# 1. Login al Azure Container Registry
Write-Host "`n[1/5] Autenticando con Azure Container Registry..." -ForegroundColor Cyan
az acr login --name $registryName

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la autenticacion con ACR" -ForegroundColor Red
    exit 1
}
Write-Host "Autenticacion exitosa" -ForegroundColor Green

# 2. Build de la imagen Docker
Write-Host "`n[2/5] Construyendo imagen Docker..." -ForegroundColor Cyan
Write-Host "  Path: $backendPath" -ForegroundColor Gray
Write-Host "  Imagen: $registryServer/${imageName}:${imageTag}" -ForegroundColor Gray

Set-Location $backendPath

docker build -t "${registryServer}/${imageName}:${imageTag}" -f Dockerfile .

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo el build de la imagen Docker" -ForegroundColor Red
    exit 1
}
Write-Host "Build exitoso" -ForegroundColor Green

# 3. Push de la imagen al registry
Write-Host "`n[3/5] Subiendo imagen a Azure Container Registry..." -ForegroundColor Cyan

docker push "${registryServer}/${imageName}:${imageTag}"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo el push de la imagen" -ForegroundColor Red
    exit 1
}
Write-Host "Push exitoso" -ForegroundColor Green

# 4. Actualizar Container App con la nueva imagen
Write-Host "`n[4/5] Actualizando Azure Container App..." -ForegroundColor Cyan
Write-Host "  Container App: $containerAppName" -ForegroundColor Gray
Write-Host "  Resource Group: $resourceGroup" -ForegroundColor Gray

az containerapp update `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --image "${registryServer}/${imageName}:${imageTag}"

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Fallo la actualizacion del Container App" -ForegroundColor Red
    exit 1
}
Write-Host "Container App actualizado" -ForegroundColor Green

# 5. Verificar el estado
Write-Host "`n[5/5] Verificando estado del Container App..." -ForegroundColor Cyan

Start-Sleep -Seconds 5

$status = az containerapp show `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --query "properties.runningStatus" `
    --output tsv

Write-Host "Estado: $status" -ForegroundColor $(if ($status -eq "Running") { "Green" } else { "Yellow" })

# Obtener URL
$fqdn = az containerapp show `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    --output tsv

Write-Host "`n=== DEPLOY COMPLETADO ===" -ForegroundColor Green
Write-Host "`nBackend desplegado en:" -ForegroundColor Cyan
Write-Host "  https://$fqdn" -ForegroundColor Blue
Write-Host "`nPrueba el endpoint de usuarios:" -ForegroundColor Cyan
Write-Host "  https://$fqdn/api/users" -ForegroundColor Blue
Write-Host "`nAhora los usuarios eliminados NO apareceran en la lista" -ForegroundColor Green
