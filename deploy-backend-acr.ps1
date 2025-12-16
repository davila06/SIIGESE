# ============================================
# DEPLOY BACKEND A AZURE SIN DOCKER LOCAL
# Usando Azure Container Registry Tasks
# ============================================

Write-Host "`n=== DEPLOY BACKEND CON ACR TASKS ===" -ForegroundColor Green
Write-Host "No requiere Docker Desktop instalado" -ForegroundColor Gray

# Configuración
$registryName = "acrsiinadseg"
$imageName = "siinadseg-backend"
$imageTag = "latest"
$containerAppName = "app-siinadseg-backend"
$resourceGroup = "rg-siinadseg"
$backendPath = "C:\Users\davil\SINSEG\enterprise-web-app\backend"

# 1. Build de la imagen en Azure
Write-Host "`n[1/3] Construyendo imagen en Azure Container Registry..." -ForegroundColor Cyan
Write-Host "  Esto puede tardar 2-3 minutos..." -ForegroundColor Yellow
Write-Host "  Directorio: $backendPath" -ForegroundColor Gray

Set-Location $backendPath

az acr build `
    --registry $registryName `
    --image "${imageName}:${imageTag}" `
    --file Dockerfile `
    --platform linux `
    .

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Fallo el build en ACR" -ForegroundColor Red
    exit 1
}
Write-Host "Build exitoso en Azure" -ForegroundColor Green

# 2. Actualizar Container App
Write-Host "`n[2/3] Actualizando Azure Container App..." -ForegroundColor Cyan
Write-Host "  Container App: $containerAppName" -ForegroundColor Gray
Write-Host "  Resource Group: $resourceGroup" -ForegroundColor Gray

az containerapp update `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --image "${registryName}.azurecr.io/${imageName}:${imageTag}"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Fallo la actualizacion del Container App" -ForegroundColor Red
    exit 1
}
Write-Host "Container App actualizado" -ForegroundColor Green

# 3. Verificar el estado
Write-Host "`n[3/3] Verificando deploy..." -ForegroundColor Cyan
Write-Host "  Esperando a que el contenedor inicie..." -ForegroundColor Gray

Start-Sleep -Seconds 10

$fqdn = az containerapp show `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --query "properties.configuration.ingress.fqdn" `
    --output tsv

$status = az containerapp show `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --query "properties.runningStatus" `
    --output tsv

Write-Host "`n=== DEPLOY COMPLETADO ===" -ForegroundColor Green
Write-Host "`nEstado: $status" -ForegroundColor $(if ($status -eq "Running") { "Green" } else { "Yellow" })
Write-Host "`nBackend actualizado en:" -ForegroundColor Cyan
Write-Host "  https://$fqdn" -ForegroundColor Blue
Write-Host "`nEndpoints para probar:" -ForegroundColor Cyan
Write-Host "  GET https://$fqdn/api/users" -ForegroundColor Gray
Write-Host "  (Los usuarios eliminados ya NO apareceran)" -ForegroundColor Green
Write-Host "`nPrueba ahora:" -ForegroundColor Yellow
Write-Host "  curl https://$fqdn/api/users" -ForegroundColor Gray
