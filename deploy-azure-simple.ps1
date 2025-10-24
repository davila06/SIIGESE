# Script simple para deployment a Azure
param(
    [string]$ResourceGroupName = "rg-siinadseg",
    [string]$Location = "eastus"
)

Write-Host "Iniciando deployment a Azure..." -ForegroundColor Green
Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
Write-Host "Location: $Location" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Gray

try {
    # Verificar Azure CLI
    Write-Host "`nVerificando Azure CLI..." -ForegroundColor Yellow
    $account = az account show --query "name" -o tsv 2>$null
    if (!$account) {
        Write-Host "Error: No estas logueado en Azure CLI" -ForegroundColor Red
        Write-Host "Ejecuta: az login" -ForegroundColor White
        exit 1
    }
    Write-Host "✓ Azure CLI configurado - Cuenta: $account" -ForegroundColor Green

    # Verificar si el resource group existe
    Write-Host "`nVerificando Resource Group..." -ForegroundColor Yellow
    $rgExists = az group exists --name $ResourceGroupName
    if ($rgExists -eq "false") {
        Write-Host "Creando Resource Group: $ResourceGroupName..." -ForegroundColor Yellow
        az group create --name $ResourceGroupName --location $Location
        if ($LASTEXITCODE -ne 0) {
            throw "Error creando Resource Group"
        }
        Write-Host "✓ Resource Group creado" -ForegroundColor Green
    } else {
        Write-Host "✓ Resource Group existe" -ForegroundColor Green
    }

    # Crear SQL Server y Database
    Write-Host "`nCreando SQL Server y Database..." -ForegroundColor Yellow
    $sqlServerName = "sql-siinadseg-$(Get-Random -Maximum 9999)"
    $sqlAdminUser = "sqladmin"
    $sqlAdminPassword = "TempPassword123!"
    
    Write-Host "Creando SQL Server: $sqlServerName..." -ForegroundColor White
    az sql server create `
        --name $sqlServerName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --admin-user $sqlAdminUser `
        --admin-password $sqlAdminPassword
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error creando SQL Server"
    }
    Write-Host "✓ SQL Server creado" -ForegroundColor Green

    # Configurar firewall para permitir servicios de Azure
    Write-Host "Configurando firewall de SQL Server..." -ForegroundColor White
    az sql server firewall-rule create `
        --resource-group $ResourceGroupName `
        --server $sqlServerName `
        --name "AllowAzureServices" `
        --start-ip-address 0.0.0.0 `
        --end-ip-address 0.0.0.0

    # Crear database
    Write-Host "Creando database..." -ForegroundColor White
    az sql db create `
        --resource-group $ResourceGroupName `
        --server $sqlServerName `
        --name "SiinadsegDB" `
        --service-objective Basic
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error creando database"
    }
    Write-Host "✓ Database creada" -ForegroundColor Green

    # Crear App Service Plan
    Write-Host "`nCreando App Service Plan..." -ForegroundColor Yellow
    $appServicePlanName = "asp-siinadseg"
    az appservice plan create `
        --name $appServicePlanName `
        --resource-group $ResourceGroupName `
        --location $Location `
        --sku B1
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error creando App Service Plan"
    }
    Write-Host "✓ App Service Plan creado" -ForegroundColor Green

    # Crear Web App para backend
    Write-Host "`nCreando Web App para backend..." -ForegroundColor Yellow
    $webAppName = "app-siinadseg-backend-$(Get-Random -Maximum 9999)"
    az webapp create `
        --name $webAppName `
        --resource-group $ResourceGroupName `
        --plan $appServicePlanName `
        --runtime "DOTNET:8.0"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Error creando Web App"
    }
    Write-Host "✓ Web App creado" -ForegroundColor Green

    # Configurar connection string
    Write-Host "Configurando connection string..." -ForegroundColor White
    $connectionString = "Server=tcp:$sqlServerName.database.windows.net,1433;Initial Catalog=SiinadsegDB;Persist Security Info=False;User ID=$sqlAdminUser;Password=$sqlAdminPassword;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
    
    az webapp config connection-string set `
        --resource-group $ResourceGroupName `
        --name $webAppName `
        --connection-string-type SQLAzure `
        --settings DefaultConnection="$connectionString"

    Write-Host "✓ Connection string configurado" -ForegroundColor Green

    # Mostrar información de deployment
    Write-Host "`n========================================" -ForegroundColor Gray
    Write-Host "DEPLOYMENT COMPLETADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Gray
    Write-Host "Resource Group: $ResourceGroupName" -ForegroundColor White
    Write-Host "SQL Server: $sqlServerName.database.windows.net" -ForegroundColor White
    Write-Host "Database: SiinadsegDB" -ForegroundColor White
    Write-Host "Web App: $webAppName.azurewebsites.net" -ForegroundColor White
    Write-Host "SQL Admin User: $sqlAdminUser" -ForegroundColor White
    Write-Host "SQL Admin Password: $sqlAdminPassword" -ForegroundColor Yellow
    Write-Host "`nNOTA: Guarda esta información de conexión!" -ForegroundColor Red

    # Crear archivo con la información de deployment
    $deploymentInfo = @"
DEPLOYMENT INFORMATION
======================
Fecha: $(Get-Date)
Resource Group: $ResourceGroupName
SQL Server: $sqlServerName.database.windows.net
Database: SiinadsegDB
Web App: $webAppName.azurewebsites.net
SQL Admin User: $sqlAdminUser
SQL Admin Password: $sqlAdminPassword

Connection String:
$connectionString

Web App URL: https://$webAppName.azurewebsites.net
"@

    $deploymentInfo | Out-File -FilePath "DEPLOYMENT_SUCCESS.md" -Encoding UTF8
    Write-Host "`n✓ Información guardada en DEPLOYMENT_SUCCESS.md" -ForegroundColor Green

} catch {
    Write-Host "`nERROR EN DEPLOYMENT:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor White
    Write-Host "`nTROUBLESHOOTING:" -ForegroundColor Yellow
    Write-Host "• Verifica que tienes permisos en tu suscripción de Azure" -ForegroundColor White
    Write-Host "• Asegurate de estar logueado: az login" -ForegroundColor White
    Write-Host "• Revisa los logs detallados arriba" -ForegroundColor White
    exit 1
}