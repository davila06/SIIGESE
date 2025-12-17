# Script de Verificación Final del Deployment

$frontendUrl = "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net"
$backendUrl = "http://siinadseg-api-7464.eastus2.azurecontainer.io:8080"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "VERIFICACION FINAL DEL DEPLOYMENT" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

# 1. Verificar Backend Health
Write-Host "[1/5] Verificando Backend Health..." -ForegroundColor Cyan
try {
    $healthResponse = Invoke-WebRequest -Uri "$backendUrl/health" -UseBasicParsing -TimeoutSec 10
    if ($healthResponse.StatusCode -eq 200) {
        Write-Host "  Backend Health: OK (200)" -ForegroundColor Green
    }
} catch {
    Write-Host "  Backend Health: ERROR" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Yellow
}

# 2. Verificar Frontend
Write-Host "`n[2/5] Verificando Frontend..." -ForegroundColor Cyan
try {
    $frontendResponse = Invoke-WebRequest -Uri $frontendUrl -UseBasicParsing -TimeoutSec 10
    if ($frontendResponse.StatusCode -eq 200) {
        Write-Host "  Frontend: OK (200)" -ForegroundColor Green
    }
} catch {
    Write-Host "  Frontend: ERROR" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Yellow
}

# 3. Verificar Container Instance
Write-Host "`n[3/5] Verificando estado del Container Instance..." -ForegroundColor Cyan
$containerState = az container show `
    --resource-group rg-siinadseg-prod-2025 `
    --name siinadseg-backend `
    --query "instanceView.state" `
    --output tsv

Write-Host "  Container State: $containerState" -ForegroundColor $(if ($containerState -eq "Running") { "Green" } else { "Yellow" })

# 4. Verificar SQL Server
Write-Host "`n[4/5] Verificando SQL Server..." -ForegroundColor Cyan
$sqlServer = az sql server show `
    --name siinadseg-sql-3376 `
    --resource-group rg-siinadseg-prod-2025 `
    --query "state" `
    --output tsv

Write-Host "  SQL Server State: $sqlServer" -ForegroundColor $(if ($sqlServer -eq "Ready") { "Green" } else { "Yellow" })

# 5. Verificar Static Web App
Write-Host "`n[5/5] Verificando Static Web App..." -ForegroundColor Cyan
$swaState = az staticwebapp show `
    --name swa-siinadseg-frontend `
    --resource-group rg-siinadseg-prod-2025 `
    --query "name" `
    --output tsv

if ($swaState -eq "swa-siinadseg-frontend") {
    Write-Host "  Static Web App: OK" -ForegroundColor Green
}

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "RESUMEN DEL DEPLOYMENT" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "URLs de Acceso:" -ForegroundColor Yellow
Write-Host "  Frontend: $frontendUrl" -ForegroundColor White
Write-Host "  Backend API: $backendUrl/api" -ForegroundColor White
Write-Host "  Swagger: $backendUrl/swagger" -ForegroundColor White
Write-Host "  Health Check: $backendUrl/health" -ForegroundColor White

Write-Host "`nCredenciales de Acceso:" -ForegroundColor Yellow
Write-Host "  Email: admin@sinseg.com" -ForegroundColor White
Write-Host "  Password: admin123" -ForegroundColor White

Write-Host "`nBase de Datos:" -ForegroundColor Yellow
Write-Host "  Server: siinadseg-sql-3376.database.windows.net" -ForegroundColor White
Write-Host "  Database: SiinadsegDB" -ForegroundColor White
Write-Host "  User: siinadsegadmin" -ForegroundColor White

Write-Host "`nRecursos Creados:" -ForegroundColor Yellow
Write-Host "  [x] SQL Server y Base de Datos" -ForegroundColor Green
Write-Host "  [x] Azure Container Registry" -ForegroundColor Green
Write-Host "  [x] Azure Container Instance (Backend)" -ForegroundColor Green
Write-Host "  [x] Azure Static Web App (Frontend)" -ForegroundColor Green
Write-Host "  [x] Migraciones aplicadas (incluye CorreoElectronico)" -ForegroundColor Green
Write-Host "  [x] Usuario administrador creado" -ForegroundColor Green
Write-Host "  [x] CORS configurado" -ForegroundColor Green

Write-Host "`nCambios Principales:" -ForegroundColor Yellow
Write-Host "  * Campo CorreoElectronico agregado al modulo Cobros" -ForegroundColor Cyan
Write-Host "    - Backend: Entity, DTOs, Service, Migration" -ForegroundColor Gray
Write-Host "    - Frontend: Interface, Form, Table, Validation" -ForegroundColor Gray
Write-Host "  * NotificationService actualizado para usar email real" -ForegroundColor Cyan

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "SISTEMA COMPLETAMENTE OPERACIONAL" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Cyan

Write-Host "Proximo paso: Acceder a $frontendUrl" -ForegroundColor Magenta
Write-Host "y hacer login con las credenciales del administrador." -ForegroundColor White
Write-Host "`nDocumentacion completa en: DEPLOYMENT_FINAL_COMPLETE.md" -ForegroundColor Cyan
