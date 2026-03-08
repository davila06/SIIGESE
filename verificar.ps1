# Verificacion de Deployment
Write-Host "=== VERIFICACION DEL DEPLOYMENT ===" -ForegroundColor Cyan
Write-Host ""

$url = "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net"
Write-Host "URL de la aplicacion: $url" -ForegroundColor Yellow
Write-Host ""

Write-Host "Verificando backend..." -ForegroundColor Cyan
try {
    $backendUrl = "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/auth/login"
    $body = @{
        email = "admin@sinseg.com"
        password = "Admin123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri $backendUrl -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    
    if ($response.token) {
        Write-Host "Backend funcionando correctamente" -ForegroundColor Green
        Write-Host "Autenticacion exitosa" -ForegroundColor Green
        
        $headers = @{
            Authorization = "Bearer $($response.token)"
        }
        $polizas = Invoke-RestMethod -Uri "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/polizas" -Headers $headers -ErrorAction Stop
        Write-Host "API de polizas accesible - $($polizas.Count) registros" -ForegroundColor Green
    }
} catch {
    Write-Host "Error verificando backend: $($_.Exception.Message)" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== INSTRUCCIONES ===" -ForegroundColor Cyan
Write-Host "1. Abre: $url" -ForegroundColor White
Write-Host "2. Haz hard refresh (Ctrl + Shift + R)" -ForegroundColor White
Write-Host "3. Login: admin@sinseg.com / Admin123!" -ForegroundColor White
Write-Host "4. Ve al Dashboard de Polizas" -ForegroundColor White
Write-Host "5. Cambia a vista de TABLA" -ForegroundColor White
Write-Host ""
Write-Host "Columnas correctas: Poliza, Nombre, Aseguradora, Fecha, Frecuencia, Observaciones" -ForegroundColor Green
Write-Host "Build Hash: e014abbf7c332960" -ForegroundColor Gray
