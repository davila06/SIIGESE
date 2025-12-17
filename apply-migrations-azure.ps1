# Script para aplicar migraciones a la nueva base de datos Azure

$sqlServer = "siinadseg-sql-3376.database.windows.net"
$sqlDatabase = "SiinadsegDB"
$sqlUser = "siinadsegadmin"

Write-Host "Configuracion de SQL Server:" -ForegroundColor Cyan
Write-Host "Server: $sqlServer" -ForegroundColor Yellow
Write-Host "Database: $sqlDatabase" -ForegroundColor Yellow
Write-Host "User: $sqlUser" -ForegroundColor Yellow

# Solicitar contraseña de forma segura
$sqlPasswordSecure = Read-Host "Ingresa la contrasena SQL (se mostro en pantalla durante la creacion)" -AsSecureString
$sqlPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto([System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPasswordSecure))

# Crear cadena de conexión
$connectionString = "Server=tcp:$sqlServer,1433;Initial Catalog=$sqlDatabase;Persist Security Info=False;User ID=$sqlUser;Password=$sqlPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "`nActualizando appsettings.json..." -ForegroundColor Cyan
$appsettingsPath = "backend\src\WebApi\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "appsettings.json actualizado correctamente" -ForegroundColor Green
} else {
    Write-Error "No se encuentra appsettings.json"
    exit 1
}

Write-Host "`nAplicando migraciones..." -ForegroundColor Cyan
Set-Location backend\src\Infrastructure
dotnet ef database update --startup-project ../WebApi/WebApi.csproj --context ApplicationDbContext

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nMigraciones aplicadas exitosamente!" -ForegroundColor Green
    Write-Host "Base de datos inicializada con:" -ForegroundColor Yellow
    Write-Host "- Todas las tablas creadas" -ForegroundColor White
    Write-Host "- Campo CorreoElectronico agregado a Cobros" -ForegroundColor White
    Write-Host "- Roles inicializados (Admin, User, etc.)" -ForegroundColor White
    Write-Host "- Datos seed insertados" -ForegroundColor White
} else {
    Write-Error "Error aplicando migraciones"
    exit 1
}

Set-Location ..\..\..

Write-Host "`nProximo paso: Crear usuario administrador" -ForegroundColor Magenta
Write-Host "Ejecuta: python create-admin-user.py" -ForegroundColor White
