# Script simple para ejecutar setup de BD en Azure
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "EJECUTAR SCRIPT SQL EN AZURE" -ForegroundColor Yellow
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$SqlServer = "sql-siinadseg-7266.database.windows.net"
$Database = "SiinadsegDB"
$Username = "sqladmin"
$Password = "TempPassword123!"
$ScriptPath = "SETUP_DATABASE_EXCEL.sql"

Write-Host "Servidor: $SqlServer" -ForegroundColor Cyan
Write-Host "Database: $Database" -ForegroundColor Cyan
Write-Host "Usuario: $Username" -ForegroundColor Cyan
Write-Host ""

Write-Host "Ejecutando script SQL..." -ForegroundColor Yellow
Write-Host ""

sqlcmd -S $SqlServer -d $Database -U $Username -P $Password -i $ScriptPath

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "================================================" -ForegroundColor Green
    Write-Host "BASE DE DATOS CONFIGURADA EXITOSAMENTE" -ForegroundColor Green
    Write-Host "================================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Verificando datos..." -ForegroundColor Yellow
    
    $verifyQuery = "SELECT 'Roles' AS Tabla, COUNT(*) AS Total FROM Roles UNION ALL SELECT 'Users', COUNT(*) FROM Users UNION ALL SELECT 'Polizas', COUNT(*) FROM Polizas;"
    
    sqlcmd -S $SqlServer -d $Database -U $Username -P $Password -Q $verifyQuery -h -1 -W
    
    Write-Host ""
    Write-Host "CREDENCIALES ADMIN:" -ForegroundColor Cyan
    Write-Host "  Usuario: admin" -ForegroundColor White
    Write-Host "  Email: admin@sinseg.com" -ForegroundColor White
    Write-Host "  Password: Admin123!" -ForegroundColor White
    Write-Host ""
    Write-Host "Tu aplicacion ya puede conectarse a Azure SQL!" -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "ERROR: No se pudo ejecutar el script" -ForegroundColor Red
    Write-Host "Verifica que el firewall este configurado" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Ve a Azure Portal:" -ForegroundColor Yellow
    Write-Host "https://portal.azure.com" -ForegroundColor Cyan
    Write-Host "Busca: sql-siinadseg-7266" -ForegroundColor Cyan
    Write-Host "Security > Networking > Add your client IP" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "Presiona Enter para continuar..."
Read-Host
