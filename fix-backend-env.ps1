# Script para configurar correctamente las variables de entorno del Container App

Write-Host "=== CONFIGURANDO BACKEND CONTAINER APP ===" -ForegroundColor Cyan
Write-Host ""

# Obtener password del ACR
Write-Host "1. Obteniendo credenciales del ACR..." -ForegroundColor Yellow
$acrPassword = az acr credential show --name acrsiinadseg7512 --query "passwords[0].value" -o tsv

# Actualizar Container App con variables de entorno y secrets
Write-Host "2. Actualizando Container App con variables de entorno..." -ForegroundColor Yellow

# Primero, crear/actualizar el secret para la connection string
az containerapp secret set `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --secrets "connection-string=Server=siinadseg-sql-3376.database.windows.net;Database=SiinadsegDB;User Id=adminuser;Password=P@ssw0rd123!;TrustServerCertificate=True;"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error configurando secret" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Secret configurado" -ForegroundColor Green

# Ahora actualizar el Container App para usar el secret
Write-Host "3. Actualizando configuración del Container App..." -ForegroundColor Yellow

az containerapp update `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --set-env-vars `
    "ASPNETCORE_ENVIRONMENT=Production" `
    "ASPNETCORE_URLS=http://+:8080" `
  --replace-env-vars `
    "ConnectionStrings__DefaultConnection=secretref:connection-string"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error actualizando Container App" -ForegroundColor Red
    exit 1
}

Write-Host "✓ Container App actualizado" -ForegroundColor Green
Write-Host ""

# Esperar a que se despliegue la nueva revisión
Write-Host "4. Esperando 30 segundos para que la nueva revisión inicie..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

# Verificar el estado
Write-Host "5. Verificando estado del Container App..." -ForegroundColor Yellow
$status = az containerapp show `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --query "properties.runningStatus" -o tsv

Write-Host "   Estado: $status" -ForegroundColor Cyan

# Probar el endpoint de login
Write-Host ""
Write-Host "6. Probando endpoint de login..." -ForegroundColor Yellow

$body = @{
    email = "admin@sinseg.com"
    password = "admin123"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest `
        -Uri "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/auth/login" `
        -Method POST `
        -Body $body `
        -ContentType "application/json" `
        -UseBasicParsing
    
    Write-Host "✓ Login exitoso! Status: $($response.StatusCode)" -ForegroundColor Green
    $responseData = $response.Content | ConvertFrom-Json
    Write-Host "   Token recibido: $($responseData.token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "✗ Error en login: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== CONFIGURACIÓN COMPLETADA ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "URLs de la aplicación:" -ForegroundColor Magenta
Write-Host "  Frontend: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -ForegroundColor White
Write-Host "  Backend:  https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io" -ForegroundColor White
Write-Host ""
Write-Host "Credenciales:" -ForegroundColor Magenta
Write-Host "  Email:    admin@sinseg.com" -ForegroundColor White
Write-Host "  Password: admin123" -ForegroundColor White
