# ============================================
# FIX: Actualizar Container App con credenciales del registry
# ============================================

Write-Host "`n=== ACTUALIZANDO CONTAINER APP CON CREDENCIALES ===" -ForegroundColor Green

$registryName = "acrsiinadseg"
$registryServer = "acrsiinadseg.azurecr.io"
$imageName = "siinadseg-backend"
$imageTag = "latest"
$containerAppName = "app-siinadseg-backend"
$resourceGroup = "rg-siinadseg"

# 1. Obtener credenciales del registry
Write-Host "`n[1/2] Obteniendo credenciales del registry..." -ForegroundColor Cyan

$acrUser = az acr credential show --name $registryName --query username --output tsv
$acrPassword = az acr credential show --name $registryName --query "passwords[0].value" --output tsv

if (-not $acrUser -or -not $acrPassword) {
    Write-Host "ERROR: No se pudieron obtener las credenciales" -ForegroundColor Red
    exit 1
}

Write-Host "Credenciales obtenidas exitosamente" -ForegroundColor Green

# 2. Actualizar Container App con credenciales
Write-Host "`n[2/2] Actualizando Container App con credenciales..." -ForegroundColor Cyan

az containerapp registry set `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --server $registryServer `
    --username $acrUser `
    --password $acrPassword

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Fallo configurar el registry" -ForegroundColor Red
    exit 1
}

Write-Host "Registry configurado correctamente" -ForegroundColor Green

# Ahora actualizar la imagen
Write-Host "Actualizando imagen del container..." -ForegroundColor Cyan

az containerapp update `
    --name $containerAppName `
    --resource-group $resourceGroup `
    --image "${registryServer}/${imageName}:${imageTag}"

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Fallo la actualizacion" -ForegroundColor Red
    exit 1
}

Write-Host "`n=== ACTUALIZACION COMPLETADA ===" -ForegroundColor Green

# Verificar estado
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

Write-Host "`nEstado del Container App: $status" -ForegroundColor $(if ($status -eq "Running") { "Green" } else { "Yellow" })
Write-Host "`nBackend disponible en:" -ForegroundColor Cyan
Write-Host "  https://$fqdn" -ForegroundColor Blue
Write-Host "`nPrueba el fix:" -ForegroundColor Yellow
Write-Host "  curl https://$fqdn/api/users" -ForegroundColor Gray
Write-Host "`nLos usuarios eliminados ya NO apareceran" -ForegroundColor Green
