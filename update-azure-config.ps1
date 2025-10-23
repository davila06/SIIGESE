# Script para actualizar configuración del frontend con recursos reales de Azure
param(
    [string]$BackendUrl = "http://siinadseg-backend-1019.eastus.azurecontainer.io",
    [string]$SqlServer = "siinadseg-sqlserver-1019.database.windows.net",
    [string]$Database = "SiinadsegDB"
)

Write-Host "🔧 Actualizando configuración del frontend para usar recursos reales de Azure..." -ForegroundColor Green

# Actualizar environment.prod.ts
$envProdContent = @"
export const environment = {
  production: true,
  apiUrl: '$BackendUrl/api',
  version: '$(Get-Date -Format "yyyyMMdd-HHmm")-azure-real',
  enableLogging: false,
  azure: {
    sqlServer: '$SqlServer',
    database: '$Database',
    containerUrl: '$BackendUrl'
  }
};
"@

$envProdPath = "frontend-new\src\environments\environment.prod.ts"
$envProdContent | Out-File -FilePath $envProdPath -Encoding UTF8
Write-Host "✅ Actualizado: $envProdPath" -ForegroundColor Green

# Crear archivo de configuración de Azure
$azureConfigContent = @"
{
  "azure": {
    "resources": {
      "sqlServer": "$SqlServer",
      "database": "$Database",
      "backendContainer": "$BackendUrl",
      "staticWebApp": "https://agreeable-water-06170cf10.1.azurestaticapps.net",
      "resourceGroup": "siinadseg-rg",
      "region": "westus2"
    },
    "credentials": {
      "sqlUsername": "siinadmin",
      "sqlPasswordRef": "Stored in Azure Key Vault or Environment Variables"
    },
    "endpoints": {
      "api": "$BackendUrl/api",
      "auth": "$BackendUrl/api/auth",
      "polizas": "$BackendUrl/api/polizas"
    }
  },
  "deployment": {
    "lastUpdate": "$(Get-Date -Format "yyyy-MM-dd HH:mm:ss")",
    "status": "Azure resources created successfully",
    "components": {
      "frontend": "Azure Static Web Apps",
      "backend": "Azure Container Instance", 
      "database": "Azure SQL Database",
      "storage": "Azure SQL Basic tier"
    }
  }
}
"@

$azureConfigPath = "azure-deployment-config.json"
$azureConfigContent | Out-File -FilePath $azureConfigPath -Encoding UTF8
Write-Host "✅ Creado: $azureConfigPath" -ForegroundColor Green

# Mostrar resumen
Write-Host "`n📊 RESUMEN DE RECURSOS AZURE CREADOS:" -ForegroundColor Cyan
Write-Host "🗄️  SQL Server: $SqlServer" -ForegroundColor Yellow
Write-Host "💾 Database: $Database" -ForegroundColor Yellow  
Write-Host "🐳 Backend Container: $BackendUrl" -ForegroundColor Yellow
Write-Host "🌐 Frontend: https://agreeable-water-06170cf10.1.azurestaticapps.net" -ForegroundColor Yellow

Write-Host "`n🔑 CREDENCIALES:" -ForegroundColor Cyan
Write-Host "SQL Admin: siinadmin" -ForegroundColor White
Write-Host "SQL Password: Siinadseg2025#@" -ForegroundColor White

Write-Host "`n🚀 PRÓXIMOS PASOS:" -ForegroundColor Cyan
Write-Host "1. Construir frontend: cd frontend-new && ng build --prod" -ForegroundColor White
Write-Host "2. Desplegar frontend actualizado" -ForegroundColor White
Write-Host "3. (Opcional) Crear imagen Docker personalizada del backend" -ForegroundColor White

Write-Host "`n✅ Configuración completada exitosamente!" -ForegroundColor Green