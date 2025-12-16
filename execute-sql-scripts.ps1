# Script para ejecutar los scripts SQL en Azure usando Azure Data Studio o conexion directa
# Como alternativa a sqlcmd que tiene problemas con TLS

param(
    [string]$ConfigFile = "new-database-config.json"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ejecutando Scripts SQL en Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que exista el archivo de configuracion
if (-not (Test-Path $ConfigFile)) {
    Write-Host "Error: No se encuentra el archivo $ConfigFile" -ForegroundColor Red
    exit 1
}

# Leer configuracion
$config = Get-Content $ConfigFile | ConvertFrom-Json
$server = $config.server
$database = $config.database
$username = $config.username
$password = $config.password

Write-Host "Conectandose a:" -ForegroundColor Yellow
Write-Host "  Servidor: $server" -ForegroundColor White
Write-Host "  Base de datos: $database" -ForegroundColor White
Write-Host ""

# Scripts a ejecutar
$scriptsToRun = @(
    "01_CreateDatabase.sql",
    "02_CreateTables.sql",
    "03_CreateIndexes.sql",
    "04_CreateForeignKeys.sql",
    "06_CreateCobrosTable.sql"
)

Write-Host "OPCION 1: Usar Azure Data Studio (Recomendado)" -ForegroundColor Cyan
Write-Host "--------------------------------------------" -ForegroundColor Cyan
Write-Host "1. Abre Azure Data Studio" -ForegroundColor White
Write-Host "2. Crea una nueva conexion con estos datos:" -ForegroundColor White
Write-Host "   - Server: $server" -ForegroundColor Yellow
Write-Host "   - Database: $database" -ForegroundColor Yellow
Write-Host "   - Auth Type: SQL Login" -ForegroundColor Yellow
Write-Host "   - Username: $username" -ForegroundColor Yellow
Write-Host "   - Password: $password" -ForegroundColor Yellow
Write-Host "3. Ejecuta cada uno de estos scripts en orden:" -ForegroundColor White
foreach ($script in $scriptsToRun) {
    $fullPath = Join-Path -Path $PSScriptRoot -ChildPath $script
    if (Test-Path $fullPath) {
        Write-Host "   - $fullPath" -ForegroundColor Green
    } else {
        Write-Host "   - $fullPath (NO ENCONTRADO)" -ForegroundColor Red
    }
}
Write-Host ""

Write-Host "OPCION 2: Usar Invoke-Sqlcmd (Si tienes SqlServer PowerShell module)" -ForegroundColor Cyan
Write-Host "--------------------------------------------" -ForegroundColor Cyan
Write-Host "Intentando usar Invoke-Sqlcmd..." -ForegroundColor Yellow

# Verificar si el modulo SqlServer esta instalado
$sqlServerModule = Get-Module -ListAvailable -Name SqlServer
if ($sqlServerModule) {
    Write-Host "Modulo SqlServer encontrado. Ejecutando scripts..." -ForegroundColor Green
    Import-Module SqlServer -ErrorAction SilentlyContinue
    
    foreach ($script in $scriptsToRun) {
        $scriptPath = Join-Path -Path $PSScriptRoot -ChildPath $script
        if (Test-Path $scriptPath) {
            Write-Host "Ejecutando: $script" -ForegroundColor Yellow
            try {
                Invoke-Sqlcmd -ServerInstance $server -Database $database -Username $username -Password $password -InputFile $scriptPath -Verbose
                Write-Host "  OK $script ejecutado exitosamente" -ForegroundColor Green
            } catch {
                Write-Host "  ERROR: $_" -ForegroundColor Red
            }
        } else {
            Write-Host "  NOTA: Script no encontrado: $scriptPath" -ForegroundColor Yellow
        }
        Write-Host ""
    }
} else {
    Write-Host "Modulo SqlServer no esta instalado." -ForegroundColor Yellow
    Write-Host "Para instalarlo, ejecuta:" -ForegroundColor Yellow
    Write-Host "  Install-Module -Name SqlServer -AllowClobber -Force" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "O usa la OPCION 1 con Azure Data Studio" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Proceso completado!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Cyan
Write-Host "Ejecuta el script para actualizar la configuracion del backend:" -ForegroundColor White
Write-Host "  .\update-backend-connection.ps1" -ForegroundColor Yellow
Write-Host ""
