# Script mejorado para mantener los servicios SINSEG funcionando
Write-Host "=== INICIANDO SERVICIOS SINSEG ===" -ForegroundColor Green

# Función para verificar si un puerto está en uso
function Test-Port {
    param([int]$Port)
    $connections = netstat -ano | findstr ":$Port"
    return $connections.Count -gt 0
}

# Función para iniciar el backend en una ventana separada
function Start-Backend {
    Write-Host "Iniciando Backend..." -ForegroundColor Yellow
    $backendScript = @"
Push-Location "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi"
Write-Host "Backend iniciando en puerto 5000..." -ForegroundColor Green
dotnet run
"@
    
    $scriptBlock = [ScriptBlock]::Create($backendScript)
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$scriptBlock}" -WindowStyle Normal
    Write-Host "Backend iniciado en ventana separada" -ForegroundColor Green
}

# Función para iniciar el frontend en una ventana separada
function Start-Frontend {
    Write-Host "Iniciando Frontend..." -ForegroundColor Yellow
    $frontendScript = @"
Push-Location "C:\Users\davil\SINSEG\enterprise-web-app\frontend-new"
Write-Host "Frontend iniciando en puerto 4200..." -ForegroundColor Green
ng serve --proxy-config proxy.conf.json --open
"@
    
    $scriptBlock = [ScriptBlock]::Create($frontendScript)
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "& {$scriptBlock}" -WindowStyle Normal
    Write-Host "Frontend iniciado en ventana separada" -ForegroundColor Green
}

# Verificar y iniciar Backend
if (-not (Test-Port 5000)) {
    Write-Host "Puerto 5000 libre. Iniciando Backend..." -ForegroundColor Yellow
    Start-Backend
    Start-Sleep 10
} else {
    Write-Host "Backend ya está corriendo en puerto 5000" -ForegroundColor Green
}

# Verificar y iniciar Frontend
if (-not (Test-Port 4200)) {
    Write-Host "Puerto 4200 libre. Iniciando Frontend..." -ForegroundColor Yellow
    Start-Frontend
    Start-Sleep 10
} else {
    Write-Host "Frontend ya está corriendo en puerto 4200" -ForegroundColor Green
}

# Verificar estado final
Write-Host "`n=== ESTADO DE SERVICIOS ===" -ForegroundColor Cyan
if (Test-Port 5000) {
    Write-Host "✅ Backend: http://localhost:5000" -ForegroundColor Green
} else {
    Write-Host "❌ Backend: No está corriendo" -ForegroundColor Red
}

if (Test-Port 4200) {
    Write-Host "✅ Frontend: http://localhost:4200" -ForegroundColor Green
} else {
    Write-Host "❌ Frontend: No está corriendo" -ForegroundColor Red
}

Write-Host "`n=== CREDENCIALES ===" -ForegroundColor Cyan
Write-Host "Email: admin@sinseg.com" -ForegroundColor Yellow
Write-Host "Password: password123" -ForegroundColor Yellow

Write-Host "`nLos servicios están ejecutándose en ventanas separadas." -ForegroundColor Green
Write-Host "Para detenerlos, cierre las ventanas correspondientes." -ForegroundColor Yellow