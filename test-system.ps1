#!/usr/bin/env pwsh

Write-Host "ðŸ” Verificando estado del sistema OmnIA..."
Write-Host ""

# Verificar Backend
Write-Host "1. Backend (Puerto 5000):"
try {
    $backendHealth = Invoke-WebRequest -Uri "http://localhost:5000/health" -ErrorAction Stop
    Write-Host "   âœ… Backend: $($backendHealth.StatusCode) - $($backendHealth.Content)" -ForegroundColor Green
} catch {
    Write-Host "   âŒ Backend: Error - $($_.Exception.Message)" -ForegroundColor Red
}

# Verificar Frontend
Write-Host ""
Write-Host "2. Frontend (Puerto 4200):"
try {
    $frontendTest = Invoke-WebRequest -Uri "http://localhost:4200" -ErrorAction Stop
    Write-Host "   âœ… Frontend: $($frontendTest.StatusCode) - Cargando correctamente" -ForegroundColor Green
} catch {
    Write-Host "   âŒ Frontend: Error - $($_.Exception.Message)" -ForegroundColor Red
}

# Verificar Login
Write-Host ""
Write-Host "3. Sistema de Login:"
try {
    $loginData = @{
        email = "admin@sinseg.com"
        password = "password123"
    } | ConvertTo-Json
    
    $loginResponse = Invoke-WebRequest -Uri "http://localhost:5000/api/auth/login" -Method POST -Body $loginData -ContentType "application/json" -ErrorAction Stop
    Write-Host "   âœ… Login: $($loginResponse.StatusCode) - AutenticaciÃ³n exitosa" -ForegroundColor Green
} catch {
    Write-Host "   âŒ Login: Error - $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host ""
Write-Host "ðŸŒ URLs del Sistema:"
Write-Host "   Frontend: http://localhost:4200"
Write-Host "   Backend:  http://localhost:5000"
Write-Host "   Swagger:  http://localhost:5000/swagger"
Write-Host ""
Write-Host "ðŸ” Credenciales de Admin:"
Write-Host "   Email:    admin@sinseg.com"
Write-Host "   Password: password123"
