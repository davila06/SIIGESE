# Script para desplegar el frontend con la corrección de columnas
# Hash del build: e014abbf7c332960

Write-Host "=== Desplegando Frontend Corregido ===" -ForegroundColor Cyan
Write-Host "Corrección: Eliminadas referencias hardcoded a columnas 'prima' y 'vehiculo'" -ForegroundColor Yellow
Write-Host ""

# Cambiar al directorio del frontend
Set-Location -Path 'c:\Users\davil\SINSEG\enterprise-web-app\frontend-new'

# Verificar que el build existe
if (!(Test-Path './dist/frontend-new')) {
    Write-Host "ERROR: No se encontró el directorio de build" -ForegroundColor Red
    Write-Host "Ejecutando npm run build..." -ForegroundColor Yellow
    npm run build
}

# Obtener el token de deployment
Write-Host "Obteniendo token de deployment..." -ForegroundColor Cyan
$token = az staticwebapp secrets list --name 'swa-siinadseg-frontend' --query 'properties.apiKey' -o tsv 2>$null
$token = $token.Trim()

if ([string]::IsNullOrEmpty($token)) {
    Write-Host "ERROR: No se pudo obtener el token" -ForegroundColor Red
    exit 1
}

Write-Host "Token obtenido exitosamente (longitud: $($token.Length))" -ForegroundColor Green
Write-Host ""

# Intentar despliegue método 1: swa deploy
Write-Host "Método 1: Intentando deployment con SWA CLI..." -ForegroundColor Cyan
try {
    $env:SWA_CLI_DEPLOYMENT_TOKEN = $token
    swa deploy ./dist/frontend-new --env production --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nDeployment exitoso!" -ForegroundColor Green
        Write-Host "URL: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -ForegroundColor Cyan
        exit 0
    }
} catch {
    Write-Host "Método 1 falló: $_" -ForegroundColor Yellow
}

# Método alternativo: usar npx directamente
Write-Host "`nMétodo 2: Intentando con npx..." -ForegroundColor Cyan
try {
    npx @azure/static-web-apps-cli deploy ./dist/frontend-new --deployment-token $token --env production --verbose
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nDeployment exitoso!" -ForegroundColor Green
        Write-Host "URL: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -ForegroundColor Cyan
        exit 0
    }
} catch {
    Write-Host "Método 2 falló: $_" -ForegroundColor Yellow
}

Write-Host "`n=== INSTRUCCIONES MANUALES ===" -ForegroundColor Yellow
Write-Host "Si el deployment automático falla, puedes desplegar manualmente:" -ForegroundColor White
Write-Host "1. Ve a Azure Portal" -ForegroundColor White
Write-Host "2. Navega a la Static Web App 'swa-siinadseg-frontend'" -ForegroundColor White
Write-Host "3. En el menú lateral, selecciona 'Environment' o 'Environments'" -ForegroundColor White
Write-Host "4. Sube el archivo ZIP: c:\Users\davil\SINSEG\enterprise-web-app\frontend-new\frontend-deployment.zip" -ForegroundColor White
Write-Host "`nO usa GitHub Actions si está configurado." -ForegroundColor White
