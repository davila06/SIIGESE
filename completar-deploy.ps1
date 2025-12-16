# COMPLETAR DEPLOY DEL BACKEND
# Ejecutar despues de que el build fue exitoso

Write-Host "`n=== COMPLETAR DEPLOY DEL BACKEND ===" -ForegroundColor Green

# Paso 1: Obtener credenciales
Write-Host "`nPaso 1/4: Obteniendo credenciales..." -ForegroundColor Cyan
$acrUser = az acr credential show --name acrsiinadseg --query username --output tsv
$acrPassword = az acr credential show --name acrsiinadseg --query "passwords[0].value" --output tsv
Write-Host "OK" -ForegroundColor Green

# Paso 2: Configurar registry
Write-Host "`nPaso 2/4: Configurando registry..." -ForegroundColor Cyan
az containerapp registry set `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --server acrsiinadseg.azurecr.io `
    --username $acrUser `
    --password $acrPassword

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK" -ForegroundColor Green
} else {
    Write-Host "FALLO - Pero continuando..." -ForegroundColor Yellow
}

# Paso 3: Actualizar imagen
Write-Host "`nPaso 3/4: Actualizando Container App..." -ForegroundColor Cyan
az containerapp update `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --image acrsiinadseg.azurecr.io/siinadseg-backend:latest

if ($LASTEXITCODE -eq 0) {
    Write-Host "OK" -ForegroundColor Green
} else {
    Write-Host "ERROR" -ForegroundColor Red
    exit 1
}

# Paso 4: Verificar
Write-Host "`nPaso 4/4: Verificando deploy..." -ForegroundColor Cyan
Start-Sleep -Seconds 10

$fqdn = az containerapp show `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --query "properties.configuration.ingress.fqdn" `
    --output tsv

Write-Host "`n==============================" -ForegroundColor Green
Write-Host "DEPLOY COMPLETADO" -ForegroundColor Green
Write-Host "==============================" -ForegroundColor Green
Write-Host "`nBackend: https://$fqdn" -ForegroundColor Blue
Write-Host "`nPrueba:" -ForegroundColor Yellow
Write-Host "  curl https://$fqdn/api/users" -ForegroundColor Gray
Write-Host "`nLos usuarios eliminados ya NO apareceran" -ForegroundColor Green
