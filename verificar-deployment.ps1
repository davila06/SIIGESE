# Script para verificar que el deployment fue exitoso
Write-Host "=== VERIFICACIÓN DEL DEPLOYMENT ===" -ForegroundColor Cyan
Write-Host ""

$url = "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net"

Write-Host "URL de la aplicación: $url" -ForegroundColor Yellow
Write-Host ""

Write-Host "1. Abre la URL en tu navegador" -ForegroundColor White
Write-Host "2. Haz hard refresh (Ctrl + Shift + R) o limpia caché" -ForegroundColor White
Write-Host "3. Inicia sesión con:" -ForegroundColor White
Write-Host "   Email: admin@sinseg.com" -ForegroundColor Cyan
Write-Host "   Password: Admin123!" -ForegroundColor Cyan
Write-Host ""
Write-Host "4. Ve al Dashboard de Pólizas" -ForegroundColor White
Write-Host "5. Cambia a vista de TABLA (ícono de tabla en la parte superior)" -ForegroundColor White
Write-Host ""
Write-Host "=== VERIFICAR COLUMNAS ===" -ForegroundColor Yellow
Write-Host "La tabla debe mostrar estas columnas (en orden):" -ForegroundColor White
Write-Host "  ✓ Póliza (Número de póliza)" -ForegroundColor Green
Write-Host "  ✓ Nombre (Asegurado)" -ForegroundColor Green
Write-Host "  ✓ Aseguradora" -ForegroundColor Green
Write-Host "  ✓ Fecha (Vigencia)" -ForegroundColor Green
Write-Host "  ✓ Frecuencia" -ForegroundColor Green
Write-Host "  ✓ Observaciones" -ForegroundColor Green
Write-Host "  ✓ Acciones" -ForegroundColor Green
Write-Host ""
Write-Host "=== NO DEBE APARECER ===" -ForegroundColor Red
Write-Host "  ✗ Prima" -ForegroundColor Red
Write-Host "  ✗ Vehículo" -ForegroundColor Red
Write-Host "  ✗ Modalidad (dentro de Nombre)" -ForegroundColor Red
Write-Host ""
Write-Host "Si ves el error 'Could not find column with id prima', limpia el caché completamente." -ForegroundColor Yellow
Write-Host ""

# Intentar verificar que el backend esté funcionando
Write-Host "Verificando backend..." -ForegroundColor Cyan
try {
    $backendUrl = "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/auth/login"
    $body = @{
        email = "admin@sinseg.com"
        password = "Admin123!"
    } | ConvertTo-Json
    
    $response = Invoke-RestMethod -Uri $backendUrl -Method POST -Body $body -ContentType "application/json" -ErrorAction Stop
    
    if ($response.token) {
        Write-Host "✓ Backend funcionando correctamente" -ForegroundColor Green
        Write-Host "✓ Autenticación exitosa" -ForegroundColor Green
        
        # Probar endpoint de pólizas
        $headers = @{
            Authorization = "Bearer $($response.token)"
        }
        $polizas = Invoke-RestMethod -Uri "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/polizas" -Headers $headers -ErrorAction Stop
        Write-Host "✓ API de pólizas accesible - $($polizas.Count) registros" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠ Error verificando backend: $($_.Exception.Message)" -ForegroundColor Yellow
    Write-Host "  Esto no afecta el deployment del frontend" -ForegroundColor Gray
}

Write-Host ""
Write-Host "=== DEPLOYMENT INFO ===" -ForegroundColor Cyan
Write-Host "Build Hash: e014abbf7c332960" -ForegroundColor White
Write-Host "Cambios aplicados:" -ForegroundColor White
Write-Host "  - Eliminadas columnas prima y vehiculo" -ForegroundColor Gray
Write-Host "  - Agregadas columnas frecuencia y observaciones" -ForegroundColor Gray
Write-Host "  - Removida modalidad de columna nombre" -ForegroundColor Gray
Write-Host "  - Cambiado Vigencia a Fecha" -ForegroundColor Gray
Write-Host ""
