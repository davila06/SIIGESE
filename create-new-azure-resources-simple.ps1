# Azure Resource Creation Script - SIINADSEG
# Simplified version without emojis

# Variables de configuración
$resourceGroupName = "rg-siinadseg-prod-2025"
$location = "East US 2"
$sqlServerName = "siinadseg-sql-$(Get-Random -Minimum 1000 -Maximum 9999)"
$sqlDatabaseName = "SiinadsegDB"
$sqlAdminUser = "siinadsegadmin"
$appServicePlanName = "asp-siinadseg-backend"
$backendAppName = "app-siinadseg-backend-$(Get-Random -Minimum 1000 -Maximum 9999)"
$applicationInsightsName = "ai-siinadseg-monitoring"

Write-Host "Iniciando creacion de recursos Azure para SIINADSEG..." -ForegroundColor Green
Write-Host "Ubicacion: $location" -ForegroundColor Yellow
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor Yellow

# 1. Verificar login en Azure
Write-Host "`n[1/12] Verificando autenticacion en Azure..." -ForegroundColor Cyan
$account = az account show --query "user.name" -o tsv 2>$null
if (-not $account) {
    Write-Host "No estas autenticado en Azure. Ejecutando 'az login'..." -ForegroundColor Red
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error en autenticacion. Verifica tu cuenta de Azure."
        exit 1
    }
} else {
    Write-Host "Autenticado como: $account" -ForegroundColor Green
}

# 2. Crear Resource Group
Write-Host "`n[2/12] Creando Resource Group..." -ForegroundColor Cyan
az group create --name $resourceGroupName --location $location
if ($LASTEXITCODE -eq 0) {
    Write-Host "Resource Group creado: $resourceGroupName" -ForegroundColor Green
} else {
    Write-Error "Error creando Resource Group"
    exit 1
}

# 3. Generar contraseña segura para SQL Server
Write-Host "`n[3/12] Generando contrasena segura para SQL Server..." -ForegroundColor Cyan
$sqlAdminPassword = -join ((65..90) + (97..122) + (48..57) + 33,35,36,37,38,42,43,45,61,63,64 | Get-Random -Count 16 | ForEach-Object {[char]$_})
Write-Host "Contrasena generada (guardala en lugar seguro)" -ForegroundColor Green

# 4. Crear SQL Server
Write-Host "`n[4/12] Creando SQL Server..." -ForegroundColor Cyan
az sql server create `
    --name $sqlServerName `
    --resource-group $resourceGroupName `
    --location $location `
    --admin-user $sqlAdminUser `
    --admin-password $sqlAdminPassword

if ($LASTEXITCODE -eq 0) {
    Write-Host "SQL Server creado: $sqlServerName" -ForegroundColor Green
} else {
    Write-Error "Error creando SQL Server"
    exit 1
}

# 5. Configurar reglas de firewall para SQL Server
Write-Host "`n[5/12] Configurando firewall de SQL Server..." -ForegroundColor Cyan
# Permitir servicios de Azure
az sql server firewall-rule create `
    --resource-group $resourceGroupName `
    --server $sqlServerName `
    --name "AllowAzureServices" `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0

# Obtener IP pública para acceso desde tu máquina
$myIP = (Invoke-WebRequest -Uri "https://ipinfo.io/ip" -UseBasicParsing).Content.Trim()
az sql server firewall-rule create `
    --resource-group $resourceGroupName `
    --server $sqlServerName `
    --name "AllowMyIP" `
    --start-ip-address $myIP `
    --end-ip-address $myIP

Write-Host "Firewall configurado. Tu IP: $myIP" -ForegroundColor Green

# 6. Crear Base de Datos SQL
Write-Host "`n[6/12] Creando Base de Datos SQL..." -ForegroundColor Cyan
az sql db create `
    --resource-group $resourceGroupName `
    --server $sqlServerName `
    --name $sqlDatabaseName `
    --service-objective S0 `
    --backup-storage-redundancy Local

if ($LASTEXITCODE -eq 0) {
    Write-Host "Base de datos creada: $sqlDatabaseName" -ForegroundColor Green
} else {
    Write-Error "Error creando base de datos"
    exit 1
}

# 7. Crear App Service Plan
Write-Host "`n[7/12] Creando App Service Plan..." -ForegroundColor Cyan
az appservice plan create `
    --name $appServicePlanName `
    --resource-group $resourceGroupName `
    --location $location `
    --sku B1 `
    --is-linux

if ($LASTEXITCODE -eq 0) {
    Write-Host "App Service Plan creado: $appServicePlanName" -ForegroundColor Green
} else {
    Write-Error "Error creando App Service Plan"
    exit 1
}

# 8. Crear App Service para Backend
Write-Host "`n[8/12] Creando App Service para Backend..." -ForegroundColor Cyan
az webapp create `
    --resource-group $resourceGroupName `
    --plan $appServicePlanName `
    --name $backendAppName `
    --runtime "DOTNETCORE:8.0"

if ($LASTEXITCODE -eq 0) {
    Write-Host "App Service Backend creado: $backendAppName" -ForegroundColor Green
} else {
    Write-Error "Error creando App Service Backend"
    exit 1
}

# 9. Generar cadena de conexión
Write-Host "`n[9/12] Generando cadena de conexion..." -ForegroundColor Cyan
$connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$sqlDatabaseName;Persist Security Info=False;User ID=$sqlAdminUser;Password=$sqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 10. Configurar variables de entorno del App Service
Write-Host "`n[10/12] Configurando variables de entorno del Backend..." -ForegroundColor Cyan
az webapp config appsettings set `
    --resource-group $resourceGroupName `
    --name $backendAppName `
    --settings `
    "ConnectionStrings__DefaultConnection=$connectionString" `
    "Jwt__Secret=MySecretKey123456789MySecretKey123456789" `
    "Jwt__Issuer=SiinadsegApp" `
    "Jwt__Audience=SiinadsegApp" `
    "Jwt__ExpirationHours=8" `
    "ASPNETCORE_ENVIRONMENT=Production"

Write-Host "Variables de entorno configuradas" -ForegroundColor Green

# 11. Actualizar appsettings.json local con la nueva cadena de conexión
Write-Host "`n[11/12] Actualizando appsettings.json local..." -ForegroundColor Cyan
$appsettingsPath = "backend\src\WebApi\appsettings.json"
if (Test-Path $appsettingsPath) {
    $appsettings = Get-Content $appsettingsPath | ConvertFrom-Json
    $appsettings.ConnectionStrings.DefaultConnection = $connectionString
    $appsettings | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
    Write-Host "appsettings.json actualizado" -ForegroundColor Green
}

# 12. Mostrar resumen
Write-Host "`n[12/12] Recursos creados exitosamente!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "RESUMEN DE RECURSOS CREADOS:" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Resource Group: $resourceGroupName" -ForegroundColor Yellow
Write-Host "SQL Server: $sqlServerName.database.windows.net" -ForegroundColor Yellow
Write-Host "Base de Datos: $sqlDatabaseName" -ForegroundColor Yellow
Write-Host "Usuario SQL: $sqlAdminUser" -ForegroundColor Yellow
Write-Host "Contrasena SQL: $sqlAdminPassword" -ForegroundColor Red
Write-Host "Backend App: https://$backendAppName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "========================================" -ForegroundColor Cyan

# Guardar información en archivo
$deploymentInfo = @"
# SIINADSEG - Informacion de Deployment Azure
# Creado: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Recursos Creados
Resource Group: $resourceGroupName
SQL Server: $sqlServerName.database.windows.net
Base de Datos: $sqlDatabaseName
Usuario SQL: $sqlAdminUser
Contrasena SQL: $sqlAdminPassword
Backend App: https://$backendAppName.azurewebsites.net

## Cadena de Conexion
$connectionString

## Proximos Pasos
1. Ejecutar migraciones de base de datos
2. Crear usuario administrador
3. Deploy del backend
4. Deploy del frontend

## Comandos Utiles
# Ver logs en tiempo real
az webapp log tail --name $backendAppName --resource-group $resourceGroupName

# Reiniciar app
az webapp restart --name $backendAppName --resource-group $resourceGroupName
"@

$deploymentInfo | Out-File -FilePath "azure-deployment-info-new.txt" -Encoding UTF8
Write-Host "`nInformacion guardada en: azure-deployment-info-new.txt" -ForegroundColor Green

Write-Host "`nSIGUIENTES PASOS:" -ForegroundColor Magenta
Write-Host "1. Ejecutar migraciones de base de datos" -ForegroundColor White
Write-Host "2. Crear roles y usuario administrador" -ForegroundColor White
Write-Host "3. Deploy del backend a Azure" -ForegroundColor White
Write-Host "4. Deploy del frontend a Azure" -ForegroundColor White

Write-Host "`nIMPORTANTE: Guarda la contrasena SQL en un lugar seguro!" -ForegroundColor Red
Write-Host "Contrasena SQL: $sqlAdminPassword" -ForegroundColor Red

Write-Host "`nScript completado exitosamente!" -ForegroundColor Green

# Exportar variables para uso posterior
$env:AZURE_SQL_SERVER = "$sqlServerName.database.windows.net"
$env:AZURE_SQL_DATABASE = $sqlDatabaseName
$env:AZURE_SQL_USER = $sqlAdminUser
$env:AZURE_SQL_PASSWORD = $sqlAdminPassword
$env:AZURE_RESOURCE_GROUP = $resourceGroupName
$env:AZURE_BACKEND_APP = $backendAppName
$env:AZURE_CONNECTION_STRING = $connectionString

Write-Host "`nVariables de entorno configuradas para esta sesion de PowerShell" -ForegroundColor Cyan
