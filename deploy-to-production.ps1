# =============================================
# SCRIPT DE DESPLIEGUE A PRODUCCION AZURE
# SINSEG - Sistema Integral de Seguros
# =============================================

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  DESPLIEGUE A PRODUCCION - AZURE" -ForegroundColor Cyan
Write-Host "  SINSEG v1.0.0" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Configuracion de Azure
$AzureSqlServer = "siinadseg-sqlserver-1019.database.windows.net"
$DatabaseName = "SiinadsegDB"
$SqlUsername = "siinadmin"
$SqlPassword = "Siinadseg2025#@"
$ResourceGroup = "siinadseg-rg"
$BackendContainer = "siinadseg-backend-1019"
$FrontendUrl = "https://agreeable-water-06170cf10.1.azurestaticapps.net"

$ErrorActionPreference = "Continue"

Write-Host "PLAN DE DESPLIEGUE:" -ForegroundColor Yellow
Write-Host "  1. Configurar Base de Datos Azure SQL" -ForegroundColor White
Write-Host "  2. Verificar Backend en Azure Container" -ForegroundColor White
Write-Host "  3. Compilar Frontend para produccion" -ForegroundColor White
Write-Host "  4. Desplegar Frontend a Azure Static Web Apps" -ForegroundColor White
Write-Host "  5. Verificar funcionamiento completo" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Deseas continuar con el despliegue? (S/N)"
if ($confirm -ne "S" -and $confirm -ne "s") {
    Write-Host "Despliegue cancelado por el usuario." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  PASO 1: CONFIGURAR BASE DE DATOS AZURE SQL" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Ejecutando script de base de datos..." -ForegroundColor Yellow

$setupDbScript = Join-Path $PSScriptRoot "setup-azure-database-excel.ps1"
if (Test-Path $setupDbScript) {
    & $setupDbScript
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al configurar base de datos, pero continuando..." -ForegroundColor Yellow
    } else {
        Write-Host "Base de datos configurada exitosamente" -ForegroundColor Green
    }
} else {
    Write-Host "Script de BD no encontrado, asumiendo que ya esta configurada" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  PASO 2: VERIFICAR BACKEND EN AZURE" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Verificando backend en Azure Container..." -ForegroundColor Yellow

try {
    $backendUrl = "http://siinadseg-backend-1019.eastus.azurecontainer.io/api"
    Write-Host "   Probando: $backendUrl" -ForegroundColor Gray
    
    $response = Invoke-WebRequest -Uri $backendUrl -Method GET -TimeoutSec 10 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        Write-Host "Backend esta en linea y funcionando" -ForegroundColor Green
    } else {
        Write-Host "Backend responde pero con codigo: $($response.StatusCode)" -ForegroundColor Yellow
    }
} catch {
    Write-Host "No se pudo verificar el backend, pero continuando..." -ForegroundColor Yellow
    Write-Host "   Asegurate de que el container este ejecutandose" -ForegroundColor Gray
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  PASO 3: COMPILAR FRONTEND PARA PRODUCCION" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$frontendPath = Join-Path $PSScriptRoot "frontend-new"
if (Test-Path $frontendPath) {
    Write-Host "Compilando frontend Angular..." -ForegroundColor Yellow
    Push-Location $frontendPath
    
    Write-Host "   Instalando dependencias..." -ForegroundColor Gray
    npm install --silent
    
    Write-Host "   Compilando en modo produccion..." -ForegroundColor Gray
    npm run build --prod
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "Frontend compilado exitosamente" -ForegroundColor Green
    } else {
        Write-Host "Error en compilacion, verificar manualmente" -ForegroundColor Yellow
    }
    
    Pop-Location
} else {
    Write-Host "Carpeta frontend-new no encontrada" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  PASO 4: INFORMACION DE DESPLIEGUE" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Para desplegar el frontend a Azure Static Web Apps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "Opcion 1: Usando GitHub Actions (Recomendado)" -ForegroundColor Cyan
Write-Host "  1. Push los cambios a GitHub" -ForegroundColor Gray
Write-Host "  2. Azure Static Web Apps desplegara automaticamente" -ForegroundColor Gray
Write-Host ""
Write-Host "Opcion 2: Usando Azure CLI" -ForegroundColor Cyan
Write-Host "  cd frontend-new" -ForegroundColor Gray
Write-Host "  az staticwebapp deploy --name agreeable-water-06170cf10 --resource-group $ResourceGroup --source ./dist" -ForegroundColor Gray
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  PASO 5: VERIFICACION FINAL" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

Write-Host "Lista de verificacion:" -ForegroundColor Yellow
Write-Host "  [ ] Base de datos configurada con tablas" -ForegroundColor White
Write-Host "  [ ] Backend container ejecutandose" -ForegroundColor White
Write-Host "  [ ] Frontend compilado sin errores" -ForegroundColor White
Write-Host "  [ ] Frontend desplegado en Azure Static Web Apps" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Green
Write-Host "  PREPARACION COMPLETADA" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Green
Write-Host ""

Write-Host "ENDPOINTS DE PRODUCCION:" -ForegroundColor Cyan
Write-Host ""
Write-Host "Frontend:" -ForegroundColor Yellow
Write-Host "  $FrontendUrl" -ForegroundColor White
Write-Host ""
Write-Host "Backend API:" -ForegroundColor Yellow
Write-Host "  http://siinadseg-backend-1019.eastus.azurecontainer.io/api" -ForegroundColor White
Write-Host ""
Write-Host "Base de Datos:" -ForegroundColor Yellow
Write-Host "  Server: $AzureSqlServer" -ForegroundColor White
Write-Host "  Database: $DatabaseName" -ForegroundColor White
Write-Host ""

Write-Host "CREDENCIALES DE ADMIN:" -ForegroundColor Cyan
Write-Host "  Usuario: admin" -ForegroundColor White
Write-Host "  Email: admin@sinseg.com" -ForegroundColor White
Write-Host "  Password: Admin123!" -ForegroundColor White
Write-Host ""

Write-Host "PASOS SIGUIENTES:" -ForegroundColor Yellow
Write-Host "  1. Abre: $FrontendUrl" -ForegroundColor White
Write-Host "  2. Inicia sesion con las credenciales de admin" -ForegroundColor White
Write-Host "  3. Prueba subir el archivo: polizas_ejemplo_real.csv" -ForegroundColor White
Write-Host "  4. Verifica que las 20 polizas se carguen correctamente" -ForegroundColor White
Write-Host ""

Write-Host "IMPORTANTE - SEGURIDAD:" -ForegroundColor Red
Write-Host "  * Cambia la contrasena del admin despues del primer login" -ForegroundColor White
Write-Host "  * Configura firewall de Azure SQL para IPs especificas" -ForegroundColor White
Write-Host "  * Revisa los logs de Azure Container regularmente" -ForegroundColor White
Write-Host ""

$openBrowser = Read-Host "Deseas abrir la aplicacion en el navegador? (S/N)"
if ($openBrowser -eq "S" -or $openBrowser -eq "s") {
    Start-Process $FrontendUrl
    Write-Host "Navegador abierto" -ForegroundColor Green
}

Write-Host ""
Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
