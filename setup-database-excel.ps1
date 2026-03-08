# =============================================
# Script PowerShell para Setup de Base de Datos
# SINSEG - Sistema de Seguros
# =============================================

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  SETUP BASE DE DATOS SINSEG" -ForegroundColor Cyan
Write-Host "  Soporte para Archivos Excel (14 columnas)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Variables de configuración
$ScriptPath = Join-Path $PSScriptRoot "SETUP_DATABASE_EXCEL.sql"
$DatabaseName = "SinsegAppDb"

# Verificar que existe el script SQL
if (-not (Test-Path $ScriptPath)) {
    Write-Host "❌ ERROR: No se encontró el archivo SETUP_DATABASE_EXCEL.sql" -ForegroundColor Red
    Write-Host "   Ubicación esperada: $ScriptPath" -ForegroundColor Yellow
    exit 1
}

Write-Host "✓ Script SQL encontrado: $ScriptPath" -ForegroundColor Green
Write-Host ""

# Solicitar información del servidor
Write-Host "📝 CONFIGURACIÓN DE CONEXIÓN" -ForegroundColor Yellow
Write-Host "─────────────────────────────" -ForegroundColor Yellow
Write-Host ""

$ServerName = Read-Host "Ingresa el nombre del servidor SQL (por defecto: localhost\SQLEXPRESS)"
if ([string]::IsNullOrWhiteSpace($ServerName)) {
    $ServerName = "localhost\SQLEXPRESS"
}

Write-Host ""
Write-Host "Selecciona el tipo de autenticación:" -ForegroundColor Cyan
Write-Host "  1. Windows Authentication (Recomendado)" -ForegroundColor White
Write-Host "  2. SQL Server Authentication" -ForegroundColor White
$AuthChoice = Read-Host "Opción (1 o 2)"

$UseWindowsAuth = $true
$Username = $null
$Password = $null

if ($AuthChoice -eq "2") {
    $UseWindowsAuth = $false
    $Username = Read-Host "Usuario SQL Server"
    $SecurePassword = Read-Host "Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($SecurePassword)
    $Password = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "  EJECUTANDO SCRIPT SQL" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

try {
    # Construir comando sqlcmd
    $sqlcmdArgs = @(
        "-S", $ServerName,
        "-i", $ScriptPath,
        "-b"  # Stop on error
    )

    if ($UseWindowsAuth) {
        $sqlcmdArgs += "-E"  # Use Windows Authentication
        Write-Host "🔐 Conectando con Windows Authentication..." -ForegroundColor Yellow
    } else {
        $sqlcmdArgs += @("-U", $Username, "-P", $Password)
        Write-Host "🔐 Conectando con SQL Server Authentication..." -ForegroundColor Yellow
    }

    Write-Host "🔌 Servidor: $ServerName" -ForegroundColor Cyan
    Write-Host "📁 Base de Datos: $DatabaseName" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "⏳ Por favor espera..." -ForegroundColor Yellow
    Write-Host ""

    # Ejecutar sqlcmd
    $output = & sqlcmd @sqlcmdArgs 2>&1

    # Mostrar output
    $output | ForEach-Object {
        $line = $_.ToString()
        if ($line -match "✓") {
            Write-Host $line -ForegroundColor Green
        } elseif ($line -match "ERROR|error") {
            Write-Host $line -ForegroundColor Red
        } elseif ($line -match "⚠|WARNING|warning") {
            Write-Host $line -ForegroundColor Yellow
        } else {
            Write-Host $line -ForegroundColor White
        }
    }

    # Verificar si hubo errores
    if ($LASTEXITCODE -ne 0) {
        throw "El script SQL retornó un error (Código: $LASTEXITCODE)"
    }

    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "  ✅ BASE DE DATOS CONFIGURADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    
    Write-Host "📊 INFORMACIÓN DE ACCESO:" -ForegroundColor Cyan
    Write-Host "  • Base de Datos: $DatabaseName" -ForegroundColor White
    Write-Host "  • Servidor: $ServerName" -ForegroundColor White
    Write-Host "  • Usuario Admin: admin" -ForegroundColor White
    Write-Host "  • Email: admin@sinseg.com" -ForegroundColor White
    Write-Host "  • Password: Admin123!" -ForegroundColor White
    Write-Host ""
    
    Write-Host "📁 ARCHIVOS DISPONIBLES:" -ForegroundColor Cyan
    Write-Host "  • GUIA_SETUP_DATABASE_EXCEL.md - Guía completa" -ForegroundColor White
    Write-Host "  • polizas_ejemplo_real.csv - 20 pólizas de ejemplo" -ForegroundColor White
    Write-Host ""
    
    Write-Host "🚀 PRÓXIMOS PASOS:" -ForegroundColor Yellow
    Write-Host "  1. Inicia tu aplicación web" -ForegroundColor White
    Write-Host "  2. Inicia sesión como 'admin' con password 'Admin123!'" -ForegroundColor White
    Write-Host "  3. Prueba subir el archivo 'polizas_ejemplo_real.csv'" -ForegroundColor White
    Write-Host "  4. Lee la guía en GUIA_SETUP_DATABASE_EXCEL.md" -ForegroundColor White
    Write-Host ""

    # Verificar conexión y datos
    Write-Host "🔍 VERIFICANDO INSTALACIÓN..." -ForegroundColor Cyan
    Write-Host ""

    $verifyQuery = @"
USE $DatabaseName;
SELECT 'Roles' AS Tabla, COUNT(*) AS Registros FROM Roles
UNION ALL SELECT 'Users', COUNT(*) FROM Users
UNION ALL SELECT 'Perfiles', COUNT(*) FROM Perfiles
UNION ALL SELECT 'Polizas', COUNT(*) FROM Polizas;
"@

    $verifyArgs = @(
        "-S", $ServerName,
        "-Q", $verifyQuery,
        "-h", "-1",  # No headers
        "-W"  # Remove trailing spaces
    )

    if ($UseWindowsAuth) {
        $verifyArgs += "-E"
    } else {
        $verifyArgs += @("-U", $Username, "-P", $Password)
    }

    $verifyOutput = & sqlcmd @verifyArgs 2>&1
    
    Write-Host "📊 RESUMEN DE DATOS:" -ForegroundColor Green
    $verifyOutput | ForEach-Object {
        if ($_ -match "\S") {
            Write-Host "  $_" -ForegroundColor White
        }
    }

} catch {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Red
    Write-Host "  ❌ ERROR AL CONFIGURAR LA BASE DE DATOS" -ForegroundColor Red
    Write-Host "================================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "💡 POSIBLES SOLUCIONES:" -ForegroundColor Yellow
    Write-Host "  1. Verifica que SQL Server esté ejecutándose" -ForegroundColor White
    Write-Host "  2. Verifica el nombre del servidor (ej: localhost\SQLEXPRESS)" -ForegroundColor White
    Write-Host "  3. Verifica las credenciales de acceso" -ForegroundColor White
    Write-Host "  4. Verifica que sqlcmd esté instalado" -ForegroundColor White
    Write-Host "  5. Ejecuta PowerShell como Administrador" -ForegroundColor White
    Write-Host ""
    Write-Host "📝 Verifica los servicios de SQL Server:" -ForegroundColor Yellow
    Write-Host "  Get-Service -Name '*SQL*' | Select Name, Status" -ForegroundColor Gray
    Write-Host ""
    exit 1
}

Write-Host ""
Write-Host "Presiona cualquier tecla para salir..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
