# Azure Resource Creation Script
# SIINADSEG Enterprise Web Application
# Created: 2025-10-23

# Variables de configuración
$resourceGroupName = "rg-siinadseg-prod"
$location = "East US"
$sqlServerName = "siinadseg-sql-server-$(Get-Random)"
$sqlDatabaseName = "SiinadsegDB"
$sqlAdminUser = "siinadsegadmin"
$appServicePlanName = "asp-siinadseg-backend"
$backendAppName = "app-siinadseg-backend-$(Get-Random)"
$staticWebAppName = "swa-siinadseg-frontend"
$applicationInsightsName = "ai-siinadseg-monitoring"
$keyVaultName = "kv-siinadseg-$(Get-Random)"

Write-Host "🚀 Iniciando creación de recursos Azure para SIINADSEG..." -ForegroundColor Green
Write-Host "📍 Ubicación: $location" -ForegroundColor Yellow
Write-Host "📦 Resource Group: $resourceGroupName" -ForegroundColor Yellow

# 1. Verificar login en Azure
Write-Host "`n1️⃣ Verificando autenticación en Azure..." -ForegroundColor Cyan
$account = az account show --query "user.name" -o tsv 2>$null
if (-not $account) {
    Write-Host "❌ No estás autenticado en Azure. Ejecutando 'az login'..." -ForegroundColor Red
    az login
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error en autenticación. Verifica tu cuenta de Azure."
        exit 1
    }
} else {
    Write-Host "✅ Autenticado como: $account" -ForegroundColor Green
}

# 2. Crear Resource Group
Write-Host "`n2️⃣ Creando Resource Group..." -ForegroundColor Cyan
az group create --name $resourceGroupName --location $location
if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Resource Group creado: $resourceGroupName" -ForegroundColor Green
} else {
    Write-Error "❌ Error creando Resource Group"
    exit 1
}

# 3. Generar contraseña segura para SQL Server
Write-Host "`n3️⃣ Generando contraseña segura para SQL Server..." -ForegroundColor Cyan
$sqlAdminPassword = -join ((65..90) + (97..122) + (48..57) + 33,35,36,37,38,42,43,45,61,63,64 | Get-Random -Count 16 | ForEach-Object {[char]$_})
Write-Host "✅ Contraseña generada (guárdala en lugar seguro)" -ForegroundColor Green

# 4. Crear SQL Server
Write-Host "`n4️⃣ Creando SQL Server..." -ForegroundColor Cyan
az sql server create `
    --name $sqlServerName `
    --resource-group $resourceGroupName `
    --location $location `
    --admin-user $sqlAdminUser `
    --admin-password $sqlAdminPassword

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SQL Server creado: $sqlServerName" -ForegroundColor Green
} else {
    Write-Error "❌ Error creando SQL Server"
    exit 1
}

# 5. Configurar reglas de firewall para SQL Server
Write-Host "`n5️⃣ Configurando firewall de SQL Server..." -ForegroundColor Cyan
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

Write-Host "✅ Firewall configurado. Tu IP: $myIP" -ForegroundColor Green

# 6. Crear Base de Datos SQL
Write-Host "`n6️⃣ Creando Base de Datos SQL..." -ForegroundColor Cyan
az sql db create `
    --resource-group $resourceGroupName `
    --server $sqlServerName `
    --name $sqlDatabaseName `
    --service-objective S1 `
    --backup-storage-redundancy Local

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Base de datos creada: $sqlDatabaseName" -ForegroundColor Green
} else {
    Write-Error "❌ Error creando base de datos"
    exit 1
}

# 7. Crear App Service Plan
Write-Host "`n7️⃣ Creando App Service Plan..." -ForegroundColor Cyan
az appservice plan create `
    --name $appServicePlanName `
    --resource-group $resourceGroupName `
    --location $location `
    --sku B1 `
    --is-linux false

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ App Service Plan creado: $appServicePlanName" -ForegroundColor Green
} else {
    Write-Error "❌ Error creando App Service Plan"
    exit 1
}

# 8. Crear App Service para Backend
Write-Host "`n8️⃣ Creando App Service para Backend..." -ForegroundColor Cyan
az webapp create `
    --resource-group $resourceGroupName `
    --plan $appServicePlanName `
    --name $backendAppName `
    --runtime "DOTNET|8.0"

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ App Service Backend creado: $backendAppName" -ForegroundColor Green
} else {
    Write-Error "❌ Error creando App Service Backend"
    exit 1
}

# 9. Crear Application Insights
Write-Host "`n9️⃣ Creando Application Insights..." -ForegroundColor Cyan
az monitor app-insights component create `
    --app $applicationInsightsName `
    --location $location `
    --resource-group $resourceGroupName `
    --kind web

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Application Insights creado: $applicationInsightsName" -ForegroundColor Green
} else {
    Write-Host "⚠️ Application Insights no se pudo crear (puede no estar disponible en tu región)" -ForegroundColor Yellow
}

# 10. Crear Static Web App para Frontend
Write-Host "`n🔟 Creando Static Web App para Frontend..." -ForegroundColor Cyan
Write-Host "ℹ️ Nota: Static Web App requiere conexión con GitHub para CI/CD automático" -ForegroundColor Yellow
Write-Host "📝 Puedes crear manualmente desde Azure Portal si prefieres" -ForegroundColor Yellow

# 11. Generar cadena de conexión
Write-Host "`n1️⃣1️⃣ Generando cadena de conexión..." -ForegroundColor Cyan
$connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=$sqlDatabaseName;Persist Security Info=False;User ID=$sqlAdminUser;Password=$sqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# 12. Configurar variables de entorno del App Service
Write-Host "`n1️⃣2️⃣ Configurando variables de entorno del Backend..." -ForegroundColor Cyan
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

Write-Host "✅ Variables de entorno configuradas" -ForegroundColor Green

# 13. Mostrar resumen
Write-Host "`n🎉 ¡Recursos creados exitosamente!" -ForegroundColor Green
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "📋 RESUMEN DE RECURSOS CREADOS:" -ForegroundColor White
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
Write-Host "🗂️  Resource Group: $resourceGroupName" -ForegroundColor Yellow
Write-Host "🗄️  SQL Server: $sqlServerName.database.windows.net" -ForegroundColor Yellow
Write-Host "💾 Base de Datos: $sqlDatabaseName" -ForegroundColor Yellow
Write-Host "👤 Usuario SQL: $sqlAdminUser" -ForegroundColor Yellow
Write-Host "🔑 Contraseña SQL: $sqlAdminPassword" -ForegroundColor Red
Write-Host "🖥️  Backend App: https://$backendAppName.azurewebsites.net" -ForegroundColor Yellow
Write-Host "📊 App Insights: $applicationInsightsName" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

# 14. Guardar información en archivo
Write-Host "`n1️⃣4️⃣ Guardando información de deployment..." -ForegroundColor Cyan
$deploymentInfo = @"
# SIINADSEG - Información de Deployment Azure
# Creado: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")

## Recursos Creados
Resource Group: $resourceGroupName
SQL Server: $sqlServerName.database.windows.net
Base de Datos: $sqlDatabaseName
Usuario SQL: $sqlAdminUser
Contraseña SQL: $sqlAdminPassword
Backend App: https://$backendAppName.azurewebsites.net
Application Insights: $applicationInsightsName

## Cadena de Conexión
$connectionString

## URLs de Gestión
- Azure Portal: https://portal.azure.com
- Kudu Backend: https://$backendAppName.scm.azurewebsites.net
- Logs Backend: https://portal.azure.com/#@/resource/subscriptions/{subscription}/resourceGroups/$resourceGroupName/providers/Microsoft.Web/sites/$backendAppName/logStream

## Próximos Pasos
1. Configurar CI/CD desde GitHub/Azure DevOps
2. Subir código del backend al App Service
3. Crear Static Web App para frontend
4. Ejecutar migraciones de base de datos
5. Configurar dominio personalizado (opcional)

## Comandos Útiles
# Ver logs en tiempo real
az webapp log tail --name $backendAppName --resource-group $resourceGroupName

# Reiniciar app
az webapp restart --name $backendAppName --resource-group $resourceGroupName

# Ver configuración
az webapp config appsettings list --name $backendAppName --resource-group $resourceGroupName
"@

$deploymentInfo | Out-File -FilePath "azure-deployment-info.txt" -Encoding UTF8
Write-Host "✅ Información guardada en: azure-deployment-info.txt" -ForegroundColor Green

Write-Host "`n🎯 SIGUIENTES PASOS:" -ForegroundColor Magenta
Write-Host "1️⃣ Revisar el archivo 'azure-deployment-info.txt'" -ForegroundColor White
Write-Host "2️⃣ Ejecutar migraciones de base de datos" -ForegroundColor White
Write-Host "3️⃣ Configurar CI/CD para deployment automático" -ForegroundColor White
Write-Host "4️⃣ Crear Static Web App para el frontend" -ForegroundColor White

Write-Host "`n⚠️ IMPORTANTE: Guarda la contraseña SQL en un lugar seguro!" -ForegroundColor Red
Write-Host "🔐 Contraseña SQL: $sqlAdminPassword" -ForegroundColor Red

Write-Host "`n✅ Script completado exitosamente!" -ForegroundColor Green