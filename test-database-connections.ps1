# ===============================================
# SIIGESE - Test de Conexiones de Base de Datos
# ===============================================

Write-Host "🗄️  SIIGESE - Test de Conexiones de Base de Datos" -ForegroundColor Cyan
Write-Host "=================================================" -ForegroundColor Cyan
Write-Host ""

# Función para probar conexión
function Test-DatabaseConnection {
    param(
        [string]$ConnectionString,
        [string]$Name
    )
    
    Write-Host "🔍 Probando conexión: $Name" -ForegroundColor Yellow
    Write-Host "   Connection String: $ConnectionString" -ForegroundColor Gray
    
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection($ConnectionString)
        $connection.Open()
        
        # Probar query simple
        $command = $connection.CreateCommand()
        $command.CommandText = "SELECT 1 as Test"
        $result = $command.ExecuteScalar()
        
        $connection.Close()
        Write-Host "   ✅ CONEXIÓN EXITOSA" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "   ❌ ERROR: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    Write-Host ""
}

# 1. Probar SQL Server Express Local (Karo\SQLEXPRESS)
Write-Host "1️⃣  SQL SERVER EXPRESS LOCAL (Karo\SQLEXPRESS)" -ForegroundColor Cyan
$localConnection = "Server=Karo\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;Connection Timeout=10;"
$localResult = Test-DatabaseConnection -ConnectionString $localConnection -Name "SQL Server Express Local"

# 2. Probar Docker SQL Server
Write-Host "2️⃣  DOCKER SQL SERVER" -ForegroundColor Cyan
$dockerConnection = "Server=localhost,1433;Database=master;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;Connection Timeout=5;"
$dockerResult = Test-DatabaseConnection -ConnectionString $dockerConnection -Name "Docker SQL Server"

# 3. Probar Azure SQL
Write-Host "3️⃣  AZURE SQL DATABASE" -ForegroundColor Cyan
$azureConnection = "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadseg_admin;Password=P@ssw0rd123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=10;"
$azureResult = Test-DatabaseConnection -ConnectionString $azureConnection -Name "Azure SQL Database"

# Resumen de resultados
Write-Host ""
Write-Host "📊 RESUMEN DE CONEXIONES" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan

if ($localResult) {
    Write-Host "✅ SQL Server Local (Karo\SQLEXPRESS): DISPONIBLE" -ForegroundColor Green
    Write-Host "   ⭐ CONFIGURACIÓN PRINCIPAL" -ForegroundColor Yellow
} else {
    Write-Host "❌ SQL Server Local (Karo\SQLEXPRESS): NO DISPONIBLE" -ForegroundColor Red
    Write-Host "   Verificar que SQL Server Express esté ejecutándose" -ForegroundColor Yellow
}

if ($dockerResult) {
    Write-Host "✅ Docker SQL Server: DISPONIBLE" -ForegroundColor Green
    Write-Host "   Alternativa de desarrollo" -ForegroundColor Gray
} else {
    Write-Host "❌ Docker SQL Server: NO DISPONIBLE" -ForegroundColor Red
    Write-Host "   Para iniciarlo: docker-compose up db -d" -ForegroundColor Yellow
}

if ($azureResult) {
    Write-Host "✅ Azure SQL: DISPONIBLE" -ForegroundColor Green
    Write-Host "   Base de datos en la nube" -ForegroundColor Gray
} else {
    Write-Host "❌ Azure SQL: NO DISPONIBLE" -ForegroundColor Red
    Write-Host "   Verificar firewall y credenciales" -ForegroundColor Yellow
}

Write-Host ""

# Recomendación
if ($localResult) {
    Write-Host "🎯 CONFIGURACIÓN ACTIVA: SQL Server Local (Karo\SQLEXPRESS)" -ForegroundColor Green
    Write-Host "   Connection String: Server=Karo\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;" -ForegroundColor White
    Write-Host "   Base de Datos: SinsegAppDb" -ForegroundColor White
} elseif ($dockerResult) {
    Write-Host "🎯 ALTERNATIVA: Usar Docker SQL Server" -ForegroundColor Green  
    Write-Host "   Connection String: Server=localhost,1433;Database=MiAppDb;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;" -ForegroundColor White
} elseif ($azureResult) {
    Write-Host "🎯 ALTERNATIVA: Usar Azure SQL" -ForegroundColor Green
    Write-Host "   Connection String: Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;..." -ForegroundColor White
} else {
    Write-Host "⚠️  NINGUNA BASE DE DATOS DISPONIBLE" -ForegroundColor Yellow
    Write-Host "   Verifica que SQL Server Express esté ejecutándose en Karo\\SQLEXPRESS" -ForegroundColor Red
}

Write-Host ""
Write-Host "📚 Para más información, consulta: README_DATABASE.md" -ForegroundColor Cyan

Read-Host "Presiona Enter para salir"