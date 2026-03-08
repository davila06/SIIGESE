# Script para iniciar y mantener los servicios funcionando
Write-Host "Iniciando servicios SINSEG..." -ForegroundColor Green

# Función para iniciar el backend
function Start-Backend {
    Write-Host "Iniciando Backend en puerto 5000..." -ForegroundColor Yellow
    Push-Location "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "dotnet run" -WindowStyle Normal
    Pop-Location
    Start-Sleep 5
}

# Función para iniciar el frontend
function Start-Frontend {
    Write-Host "Iniciando Frontend en puerto 4200..." -ForegroundColor Yellow
    Push-Location "C:\Users\davil\SINSEG\enterprise-web-app\frontend-new"
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "npm start" -WindowStyle Normal
    Pop-Location
    Start-Sleep 5
}

# Verificar si el backend está corriendo
$backendRunning = netstat -ano | findstr :5000
if (-not $backendRunning) {
    Write-Host "Backend no está corriendo. Iniciando..." -ForegroundColor Red
    Start-Backend
} else {
    Write-Host "Backend ya está corriendo en puerto 5000" -ForegroundColor Green
}

# Verificar si el frontend está corriendo
$frontendRunning = netstat -ano | findstr :4200
if (-not $frontendRunning) {
    Write-Host "Frontend no está corriendo. Iniciando..." -ForegroundColor Red
    Start-Frontend
} else {
    Write-Host "Frontend ya está corriendo en puerto 4200" -ForegroundColor Green
}

Write-Host "Servicios iniciados:" -ForegroundColor Green
Write-Host "- Backend: http://localhost:5000" -ForegroundColor Cyan
Write-Host "- Frontend: http://localhost:4200" -ForegroundColor Cyan
Write-Host ""
Write-Host "Para detener los servicios, cierre las ventanas de PowerShell correspondientes." -ForegroundColor Yellow