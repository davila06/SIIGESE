# Migrar a Azure Container Apps para HTTPS

Write-Host "=== MIGRACIÓN A AZURE CONTAINER APPS ===" -ForegroundColor Cyan
Write-Host "Esto resolverá el problema de Mixed Content proporcionando HTTPS automático" -ForegroundColor Yellow
Write-Host ""

# Paso 1: Crear Container Apps Environment
Write-Host "Paso 1: Creando Container Apps Environment..." -ForegroundColor Green
az containerapp env create `
  --name cae-siinadseg-prod `
  --resource-group rg-siinadseg-prod-2025 `
  --location eastus2

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creando Container Apps Environment" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Container Apps Environment creado" -ForegroundColor Green
Write-Host ""

# Paso 2: Habilitar admin en ACR
Write-Host "Paso 2: Habilitando acceso admin al Container Registry..." -ForegroundColor Green
az acr update --name acrsiinadseg7512 --admin-enabled true

$acrPassword = az acr credential show --name acrsiinadseg7512 --query "passwords[0].value" -o tsv

Write-Host "✓ ACR admin habilitado" -ForegroundColor Green
Write-Host ""

# Paso 3: Crear Container App
Write-Host "Paso 3: Creando Container App con HTTPS..." -ForegroundColor Green
az containerapp create `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --environment cae-siinadseg-prod `
  --image acrsiinadseg7512.azurecr.io/siinadseg-backend:latest `
  --target-port 8080 `
  --ingress 'external' `
  --registry-server acrsiinadseg7512.azurecr.io `
  --registry-username acrsiinadseg7512 `
  --registry-password $acrPassword `
  --cpu 1 --memory 2.0Gi `
  --env-vars `
    "ConnectionStrings__DefaultConnection=Server=siinadseg-sql-3376.database.windows.net;Database=SiinadsegDB;User Id=adminuser;Password=P@ssw0rd123!;TrustServerCertificate=True;" `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "ASPNETCORE_URLS=http://+:8080"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error creando Container App" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Container App creado con HTTPS" -ForegroundColor Green
Write-Host ""

# Paso 4: Obtener URL HTTPS
Write-Host "Paso 4: Obteniendo URL HTTPS del backend..." -ForegroundColor Green
$backendFqdn = az containerapp show `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --query "properties.configuration.ingress.fqdn" -o tsv

$backendUrl = "https://$backendFqdn"

Write-Host "✓ Backend URL HTTPS: $backendUrl" -ForegroundColor Green
Write-Host ""

# Paso 5: Actualizar environment.prod.ts
Write-Host "Paso 5: Actualizando configuración del frontend..." -ForegroundColor Green
$envProdPath = "frontend-new\src\environments\environment.prod.ts"
$envContent = @"
export const environment = {
  production: true,
  apiUrl: '$backendUrl/api'
};
"@

Set-Content -Path $envProdPath -Value $envContent
Write-Host "✓ environment.prod.ts actualizado" -ForegroundColor Green
Write-Host ""

# Paso 6: Rebuild y redeploy frontend
Write-Host "Paso 6: Reconstruyendo frontend con nueva URL HTTPS..." -ForegroundColor Green
Push-Location frontend-new
ng build --configuration production

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error construyendo frontend" -ForegroundColor Red
    Pop-Location
    exit 1
}

Write-Host "✓ Frontend construido" -ForegroundColor Green
Write-Host ""

Write-Host "Paso 7: Desplegando frontend..." -ForegroundColor Green
$token = az staticwebapp secrets list --name swa-siinadseg-frontend --resource-group rg-siinadseg-prod-2025 --query "properties.apiKey" -o tsv
swa deploy ./dist/frontend-new --deployment-token $token --env production

Pop-Location

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error desplegando frontend" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Frontend desplegado" -ForegroundColor Green
Write-Host ""

# Resumen
Write-Host "=== MIGRACIÓN COMPLETADA ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "URLs Finales:" -ForegroundColor Magenta
Write-Host "  Frontend: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -ForegroundColor White
Write-Host "  Backend:  $backendUrl" -ForegroundColor White
Write-Host ""
Write-Host "✓ El backend ahora usa HTTPS automáticamente" -ForegroundColor Green
Write-Host "✓ No más errores de Mixed Content" -ForegroundColor Green
Write-Host "✓ Certificados SSL administrados por Azure" -ForegroundColor Green
Write-Host ""
Write-Host "OPCIONAL: Eliminar Container Instance antiguo" -ForegroundColor Yellow
Write-Host "az container delete --name siinadseg-backend --resource-group rg-siinadseg-prod-2025 --yes" -ForegroundColor Gray
