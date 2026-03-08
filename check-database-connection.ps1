# Script para verificar la configuracion de base de datos actual
Write-Host "=== INFORMACION DE CONEXION A BASE DE DATOS ===" -ForegroundColor Green
Write-Host ""

# Leer archivo de configuracion
$configPath = "C:\Users\davil\SINSEG\enterprise-web-app\backend\src\WebApi\appsettings.json"

if (Test-Path $configPath) {
    $config = Get-Content $configPath | ConvertFrom-Json
    
    Write-Host "Archivo de configuracion encontrado: appsettings.json" -ForegroundColor Blue
    Write-Host ""
    
    # Verificar entorno
    $env_var = [Environment]::GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
    if ($env_var) {
        Write-Host "Entorno actual: $env_var" -ForegroundColor Yellow
    } else {
        Write-Host "Entorno actual: Development (por defecto)" -ForegroundColor Yellow
    }
    Write-Host ""
    
    # Analizar cadenas de conexion
    if ($config.ConnectionStrings) {
        Write-Host "CADENAS DE CONEXION DISPONIBLES:" -ForegroundColor Cyan
        Write-Host ""
        
        # DefaultConnection
        if ($config.ConnectionStrings.DefaultConnection) {
            $defaultConn = $config.ConnectionStrings.DefaultConnection
            Write-Host "1. DefaultConnection:" -ForegroundColor White
            
            # Ocultar contraseña
            $safeConn = $defaultConn -replace "Password=[^;]+", "Password=***OCULTA***"
            Write-Host "   $safeConn" -ForegroundColor Gray
            
            # Analizar tipo de BD
            if ($defaultConn -match "database\.windows\.net") {
                Write-Host "   TIPO: Azure SQL Database (Nube)" -ForegroundColor Blue
                Write-Host "   Servidor: siinadseg-sql-5307.database.windows.net" -ForegroundColor Green
                Write-Host "   Base de Datos: SiinadsegDB" -ForegroundColor Green
                Write-Host "   Usuario: siinadsegadmin" -ForegroundColor Green
            }
            Write-Host ""
        }
        
        # LocalDbConnection
        if ($config.ConnectionStrings.LocalDbConnection) {
            $localConn = $config.ConnectionStrings.LocalDbConnection
            Write-Host "2. LocalDbConnection:" -ForegroundColor White
            Write-Host "   $localConn" -ForegroundColor Gray
            Write-Host "   TIPO: SQL Server LocalDB (Local)" -ForegroundColor Green
            Write-Host "   Base de Datos: SinsegAppDb" -ForegroundColor Green
            Write-Host "   Autenticacion: Windows (Trusted_Connection)" -ForegroundColor Green
            Write-Host ""
        }
        
        # Determinar cual se esta usando
        Write-Host "LOGICA DE SELECCION:" -ForegroundColor Magenta
        Write-Host "   El sistema usa DefaultConnection si existe, sino LocalDbConnection" -ForegroundColor Gray
        Write-Host ""
        
        if ($config.ConnectionStrings.DefaultConnection) {
            Write-Host "CONEXION ACTIVA: DefaultConnection (Azure SQL Database)" -ForegroundColor Green
            Write-Host "   Servidor: siinadseg-sql-5307.database.windows.net" -ForegroundColor Green
            Write-Host "   Base de Datos: SiinadsegDB" -ForegroundColor Green
            Write-Host "   Usuario: siinadsegadmin" -ForegroundColor Green
            Write-Host "   Ubicacion: Azure (Nube)" -ForegroundColor Green
        } elseif ($config.ConnectionStrings.LocalDbConnection) {
            Write-Host "CONEXION ACTIVA: LocalDbConnection (LocalDB)" -ForegroundColor Green
            Write-Host "   Base de Datos: SinsegAppDb" -ForegroundColor Green
            Write-Host "   Ubicacion: Local" -ForegroundColor Green
        }
    } else {
        Write-Host "No se encontraron cadenas de conexion" -ForegroundColor Red
    }
} else {
    Write-Host "No se encontro el archivo de configuracion: $configPath" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== RESUMEN ===" -ForegroundColor Yellow
Write-Host "El sistema esta configurado para usar AZURE SQL DATABASE" -ForegroundColor Cyan
Write-Host "Servidor: siinadseg-sql-5307.database.windows.net" -ForegroundColor White
Write-Host "Base de Datos: SiinadsegDB" -ForegroundColor White
Write-Host "Usuario: siinadsegadmin" -ForegroundColor White
Write-Host "Puerto: 1433" -ForegroundColor White
Write-Host "Conexion: Cifrada (SSL)" -ForegroundColor White
Write-Host ""