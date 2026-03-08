# Test de conexion Azure SQL
Write-Host "Probando conexion a Azure SQL Database..." -ForegroundColor Yellow

$connectionString = "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Database=SiinadsegDB;User ID=siinadseg_admin;Password=P@ssw0rd123!;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

try {
    $connection = New-Object System.Data.SqlClient.SqlConnection($connectionString)
    $connection.Open()
    
    Write-Host "Conexion establecida exitosamente" -ForegroundColor Green
    
    $command = $connection.CreateCommand()
    $command.CommandText = "SELECT GETDATE() as CurrentTime"
    $result = $command.ExecuteScalar()
    
    Write-Host "Fecha/Hora del servidor: $result" -ForegroundColor Cyan
    
    $connection.Close()
    Write-Host "Test completado exitosamente" -ForegroundColor Green
    
} catch {
    Write-Host "Error de conexion:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    
    if ($_.Exception.Message -like "*Login failed*") {
        Write-Host ""
        Write-Host "POSIBLES SOLUCIONES:" -ForegroundColor Yellow
        Write-Host "1. Verificar credenciales en Azure Portal" -ForegroundColor White
        Write-Host "2. Verificar firewall de Azure SQL" -ForegroundColor White
        Write-Host "3. Verificar que el usuario tenga permisos" -ForegroundColor White
        Write-Host "4. Revisar configuracion de red de Azure" -ForegroundColor White
    }
}

Read-Host "Presiona Enter para continuar"