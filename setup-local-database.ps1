# ===============================================
# SIIGESE - Configuración Base de Datos Local
# Karo\SQLEXPRESS - SinsegAppDb
# ===============================================

Write-Host "🗄️  SIIGESE - Configuración Base de Datos Local" -ForegroundColor Cyan
Write-Host "Instancia: Karo\SQLEXPRESS" -ForegroundColor Cyan
Write-Host "Base de Datos: SinsegAppDb" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

$ConnectionString = "Server=Karo\SQLEXPRESS;Database=master;Trusted_Connection=True;Connection Timeout=10;"
$DatabaseName = "SinsegAppDb"

# Función para ejecutar SQL
function Invoke-SqlCommand {
    param(
        [string]$ConnectionString,
        [string]$Query
    )
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        $command = $connection.CreateCommand()
        $command.CommandText = $Query
        $result = $command.ExecuteScalar()
        
        $connection.Close()
        return $result
    }
    catch {
        throw $_.Exception.Message
    }
}

# 1. Verificar conexión al servidor
Write-Host "1️⃣  Verificando conexión al servidor..." -ForegroundColor Yellow
try {
    $serverVersion = Invoke-SqlCommand -ConnectionString $ConnectionString -Query "SELECT @@VERSION"
    Write-Host "   ✅ CONEXIÓN EXITOSA" -ForegroundColor Green
    Write-Host "   📋 Versión: SQL Server Express" -ForegroundColor Gray
}
catch {
    Write-Host "   ❌ ERROR DE CONEXIÓN: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host ""
    Write-Host "🔧 SOLUCIONES:" -ForegroundColor Yellow
    Write-Host "   1. Verificar que SQL Server Express esté ejecutándose" -ForegroundColor White
    Write-Host "   2. Comprobar el nombre de la instancia: Karo\SQLEXPRESS" -ForegroundColor White
    Write-Host "   3. Verificar que el servicio SQL Server (SQLEXPRESS) esté iniciado" -ForegroundColor White
    Read-Host "Presiona Enter para salir"
    exit 1
}

# 2. Verificar si la base de datos existe
Write-Host ""
Write-Host "2️⃣  Verificando base de datos SinsegAppDb..." -ForegroundColor Yellow
try {
    $dbExists = Invoke-SqlCommand -ConnectionString $ConnectionString -Query "SELECT COUNT(*) FROM sys.databases WHERE name = '$DatabaseName'"
    
    if ($dbExists -eq 1) {
        Write-Host "   ✅ Base de datos SinsegAppDb YA EXISTE" -ForegroundColor Green
        
        # Verificar conexión a la base de datos específica
        $dbConnectionString = "Server=Karo\SQLEXPRESS;Database=$DatabaseName;Trusted_Connection=True;Connection Timeout=10;"
        try {
            $tableCount = Invoke-SqlCommand -ConnectionString $dbConnectionString -Query "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'"
            Write-Host "   📊 Tablas encontradas: $tableCount" -ForegroundColor Gray
            
            if ($tableCount -gt 0) {
                Write-Host "   ✅ Base de datos configurada correctamente" -ForegroundColor Green
            } else {
                Write-Host "   ⚠️  Base de datos vacía - necesita ejecutar scripts de creación" -ForegroundColor Yellow
            }
        }
        catch {
            Write-Host "   ❌ Error accediendo a la base de datos: $($_.Exception.Message)" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   ❌ Base de datos SinsegAppDb NO EXISTE" -ForegroundColor Red
        Write-Host "   🔧 Necesita crear la base de datos" -ForegroundColor Yellow
    }
}
catch {
    Write-Host "   ❌ Error verificando base de datos: $($_.Exception.Message)" -ForegroundColor Red
}

# 3. Verificar usuario administrador (si la BD existe)
if ($dbExists -eq 1) {
    Write-Host ""
    Write-Host "3️⃣  Verificando usuario administrador..." -ForegroundColor Yellow
    try {
        $dbConnectionString = "Server=Karo\SQLEXPRESS;Database=$DatabaseName;Trusted_Connection=True;Connection Timeout=10;"
        $adminExists = Invoke-SqlCommand -ConnectionString $dbConnectionString -Query "SELECT COUNT(*) FROM Users WHERE Email = 'admin@sinseg.com'"
        
        if ($adminExists -eq 1) {
            Write-Host "   ✅ Usuario administrador existe" -ForegroundColor Green
            Write-Host "   📧 Email: admin@sinseg.com" -ForegroundColor Gray
            Write-Host "   🔑 Password: password123" -ForegroundColor Gray
        } else {
            Write-Host "   ❌ Usuario administrador NO existe" -ForegroundColor Red
            Write-Host "   🔧 Necesita ejecutar scripts de datos iniciales" -ForegroundColor Yellow
        }
    }
    catch {
        Write-Host "   ⚠️  No se puede verificar usuarios - posible estructura incompleta" -ForegroundColor Yellow
    }
}

# Resumen y recomendaciones
Write-Host ""
Write-Host "📊 RESUMEN DE CONFIGURACIÓN" -ForegroundColor Cyan
Write-Host "============================" -ForegroundColor Cyan
Write-Host "🖥️  Servidor: Karo\SQLEXPRESS" -ForegroundColor White
Write-Host "🗄️  Base de Datos: SinsegAppDb" -ForegroundColor White
Write-Host "🔐 Autenticación: Windows Authentication" -ForegroundColor White
Write-Host "🔗 Connection String:" -ForegroundColor White
Write-Host "   Server=Karo\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;" -ForegroundColor Gray

Write-Host ""
if ($dbExists -eq 1) {
    Write-Host "🎯 ESTADO: Base de datos configurada y lista" -ForegroundColor Green
    Write-Host ""
    Write-Host "📱 Para usar la aplicación:" -ForegroundColor Cyan
    Write-Host "   1. cd frontend-new" -ForegroundColor White
    Write-Host "   2. npm run start:local" -ForegroundColor White
    Write-Host "   3. Navegar a http://localhost:4200" -ForegroundColor White
    Write-Host "   4. Login: admin@sinseg.com / password123" -ForegroundColor White
} else {
    Write-Host "🔧 ACCIÓN REQUERIDA: Crear base de datos" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "📝 Para crear la base de datos:" -ForegroundColor Cyan
    Write-Host "   1. Abrir SQL Server Management Studio" -ForegroundColor White
    Write-Host "   2. Conectar a: Karo\SQLEXPRESS" -ForegroundColor White
    Write-Host "   3. Ejecutar: EJECUTAR_COMPLETO.sql" -ForegroundColor White
    Write-Host "   4. Volver a ejecutar este script para verificar" -ForegroundColor White
}

Write-Host ""
Read-Host "Presiona Enter para salir"