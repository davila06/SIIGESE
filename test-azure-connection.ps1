# Test específico para Azure SQL Database
Write-Host "🔍 Probando conexión a Azure SQL Database..." -ForegroundColor Yellow

$connectionString = "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadseg_admin;Password=P@ssw0rd123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    Add-Type -AssemblyName System.Data
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "✅ Conexión establecida exitosamente" -ForegroundColor Green
    
    # Probar query simple
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT GETDATE() as CurrentTime, DB_NAME() as DatabaseName"
    $reader = $command.ExecuteReader()
    
    if ($reader.Read()) {
        Write-Host "📅 Fecha/Hora del servidor: $($reader['CurrentTime'])" -ForegroundColor Cyan
        Write-Host "🗄️ Base de datos: $($reader['DatabaseName'])" -ForegroundColor Cyan
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host "✅ Test completado exitosamente" -ForegroundColor Green
    
} catch {
    Write-Host "❌ Error de conexión:" -ForegroundColor Red
    Write-Host "   $($_.Exception.Message)" -ForegroundColor Red
    
    # Diagnóstico específico
    if ($_.Exception.Message -like "*Login failed*") {
        Write-Host "" -ForegroundColor Yellow
        Write-Host "🔧 POSIBLES SOLUCIONES:" -ForegroundColor Yellow
        Write-Host "1. Verificar credenciales: siinadseg_admin / P@ssw0rd123!" -ForegroundColor White
        Write-Host "2. Verificar firewall de Azure SQL" -ForegroundColor White
        Write-Host "3. Verificar que el usuario tenga permisos" -ForegroundColor White
        Write-Host "4. Revisar configuración de red de Azure" -ForegroundColor White
    }
}

Write-Host ""
Read-Host "Presiona Enter para continuar"