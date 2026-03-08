# Script para verificar el proxy de Static Web App

Write-Host "=== VERIFICACIÓN DEL PROXY ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Probar el endpoint directo del backend
Write-Host "1. Probando backend directo (HTTP)..." -ForegroundColor Yellow
try {
    $backendResponse = Invoke-WebRequest -Uri "http://siinadseg-api-7464.eastus2.azurecontainer.io:8080/api/health" -UseBasicParsing
    Write-Host "   ✓ Backend responde: $($backendResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Backend no responde: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 2: Probar el frontend
Write-Host "2. Probando frontend (HTTPS)..." -ForegroundColor Yellow
try {
    $frontendResponse = Invoke-WebRequest -Uri "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -UseBasicParsing
    Write-Host "   ✓ Frontend responde: $($frontendResponse.StatusCode)" -ForegroundColor Green
} catch {
    Write-Host "   ✗ Frontend no responde: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""

# Test 3: Probar el proxy del Static Web App
Write-Host "3. Probando proxy /api/health (HTTPS a través de SWA)..." -ForegroundColor Yellow
try {
    $proxyResponse = Invoke-WebRequest -Uri "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/api/health" -UseBasicParsing
    Write-Host "   ✓ Proxy responde: $($proxyResponse.StatusCode)" -ForegroundColor Green
    Write-Host "   Contenido: $($proxyResponse.Content)" -ForegroundColor Gray
} catch {
    Write-Host "   ✗ Proxy no responde: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "   StatusCode: $($_.Exception.Response.StatusCode.value__)" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== PRUEBA COMPLETADA ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "INSTRUCCIONES PARA EL USUARIO:" -ForegroundColor Magenta
Write-Host "1. Abre el navegador en: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net" -ForegroundColor White
Write-Host "2. Abre las DevTools (F12)" -ForegroundColor White
Write-Host "3. Ve a la pestaña Network/Red" -ForegroundColor White
Write-Host "4. Intenta subir un archivo Excel en /polizas/upload" -ForegroundColor White
Write-Host "5. Verifica que las llamadas API ahora son:" -ForegroundColor White
Write-Host "   https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/api/..." -ForegroundColor Green
Write-Host "6. Ya NO debe aparecer el error de Mixed Content" -ForegroundColor Green
