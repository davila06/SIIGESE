# Script para crear una nueva base de datos en Azure sin datos de prueba
# Este script crea la base de datos y ejecuta solo los scripts de estructura

param(
    [string]$ResourceGroup = "siinadseg-rg",
    [string]$Location = "eastus",
    [string]$ServerName = "siinadseg-sql-prod-$(Get-Random -Minimum 1000 -Maximum 9999)",
    [string]$DatabaseName = "SiinadsegProdDB",
    [string]$AdminUser = "sqladmin",
    [string]$AdminPassword = "Siinadseg2025!SecureProdPass"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Creando nueva Base de Datos en Azure" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Verificar que el usuario este autenticado en Azure
Write-Host "Verificando autenticacion en Azure..." -ForegroundColor Yellow
$account = az account show 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "No estas autenticado en Azure. Ejecutando 'az login'..." -ForegroundColor Red
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error al autenticar. Abortando." -ForegroundColor Red
        exit 1
    }
}
Write-Host "Autenticado correctamente" -ForegroundColor Green
Write-Host ""

# Crear el servidor SQL si no existe
Write-Host "Creando servidor SQL: $ServerName" -ForegroundColor Yellow
az sql server create `
    --name $ServerName `
    --resource-group $ResourceGroup `
    --location $Location `
    --admin-user $AdminUser `
    --admin-password $AdminPassword

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al crear el servidor SQL" -ForegroundColor Red
    exit 1
}
Write-Host "Servidor SQL creado exitosamente" -ForegroundColor Green
Write-Host ""

# Configurar firewall para permitir servicios de Azure
Write-Host "Configurando reglas de firewall..." -ForegroundColor Yellow
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $ServerName `
    --name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Permitir acceso desde tu IP local
$myIp = (Invoke-WebRequest -Uri "https://api.ipify.org" -UseBasicParsing).Content.Trim()
Write-Host "Tu IP publica: $myIp" -ForegroundColor Cyan
az sql server firewall-rule create `
    --resource-group $ResourceGroup `
    --server $ServerName `
    --name AllowClientIP `
    --start-ip-address $myIp `
    --end-ip-address $myIp

Write-Host "Reglas de firewall configuradas" -ForegroundColor Green
Write-Host ""

# Crear la base de datos
Write-Host "Creando base de datos: $DatabaseName" -ForegroundColor Yellow
az sql db create `
    --resource-group $ResourceGroup `
    --server $ServerName `
    --name $DatabaseName `
    --service-objective Basic `
    --max-size 2GB

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error al crear la base de datos" -ForegroundColor Red
    exit 1
}
Write-Host "Base de datos creada exitosamente" -ForegroundColor Green
Write-Host ""

# Construir la cadena de conexion
$ServerFQDN = "$ServerName.database.windows.net"
$ConnectionString = "Server=tcp:$ServerFQDN,1433;Initial Catalog=$DatabaseName;Persist Security Info=False;User ID=$AdminUser;Password=$AdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

Write-Host "========================================" -ForegroundColor Green
Write-Host "Base de datos creada exitosamente!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Informacion de la base de datos:" -ForegroundColor Cyan
Write-Host "  Servidor: $ServerFQDN" -ForegroundColor White
Write-Host "  Base de datos: $DatabaseName" -ForegroundColor White
Write-Host "  Usuario: $AdminUser" -ForegroundColor White
Write-Host "  Password: $AdminPassword" -ForegroundColor White
Write-Host ""
Write-Host "Cadena de conexion:" -ForegroundColor Cyan
Write-Host $ConnectionString -ForegroundColor White
Write-Host ""

# Guardar informacion en un archivo
$configInfo = @{
    server = $ServerFQDN
    database = $DatabaseName
    username = $AdminUser
    password = $AdminPassword
    connectionString = $ConnectionString
    createdDate = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
} | ConvertTo-Json

$configInfo | Out-File -FilePath "new-database-config.json" -Encoding UTF8
Write-Host "Configuracion guardada en: new-database-config.json" -ForegroundColor Green
Write-Host ""

# Ejecutar scripts de estructura (sin datos de prueba)
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ejecutando scripts de estructura" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptsToRun = @(
    "01_CreateDatabase.sql",
    "02_CreateTables.sql",
    "03_CreateIndexes.sql",
    "04_CreateForeignKeys.sql",
    "06_CreateCobrosTable.sql"
)

foreach ($script in $scriptsToRun) {
    $scriptPath = Join-Path -Path $PSScriptRoot -ChildPath $script
    if (Test-Path $scriptPath) {
        Write-Host "Ejecutando: $script" -ForegroundColor Yellow
        
        # Usar sqlcmd si esta disponible
        $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
        if ($sqlcmdPath) {
            sqlcmd -S $ServerFQDN -d $DatabaseName -U $AdminUser -P $AdminPassword -i $scriptPath -e
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  OK $script ejecutado exitosamente" -ForegroundColor Green
            } else {
                Write-Host "  ERROR al ejecutar $script" -ForegroundColor Red
            }
        } else {
            Write-Host "  NOTA: sqlcmd no esta instalado. Usa Azure Data Studio o SQL Server Management Studio" -ForegroundColor Yellow
            Write-Host "    para ejecutar manualmente: $scriptPath" -ForegroundColor Yellow
        }
    } else {
        Write-Host "  NOTA: Script no encontrado: $scriptPath" -ForegroundColor Yellow
    }
    Write-Host ""
}

Write-Host "========================================" -ForegroundColor Green
Write-Host "Proceso completado!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Siguiente paso:" -ForegroundColor Cyan
Write-Host "Ejecuta el script para actualizar la configuracion del backend:" -ForegroundColor White
Write-Host "  .\update-backend-connection.ps1" -ForegroundColor Yellow
Write-Host ""
