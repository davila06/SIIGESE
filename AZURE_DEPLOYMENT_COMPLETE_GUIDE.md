# 🚀 GUÍA COMPLETA DE DESPLIEGUE A AZURE

## 📋 Recursos de Azure Necesarios

### 1. Azure SQL Database
- **Servidor**: `siinadseg-sql-server`
- **Base de datos**: `SiinadsegDB`
- **Ubicación**: East US
- **Nivel**: Basic (para desarrollo/pruebas)

### 2. Azure App Service
- **Nombre**: `siinadseg-api-app`
- **Plan de servicio**: `siinadseg-plan` (Basic B1)
- **Runtime**: .NET 8
- **Ubicación**: East US

### 3. Azure Static Web Apps
- **Nombre**: `siinadseg-frontend`
- **Ya existe**: `agreeable-water-06170cf10.1.azurestaticapps.net`

## 🔧 PASO 1: Crear Recursos con Azure CLI

### Instalar Azure CLI (si no está instalado)
```powershell
# Descargar e instalar Azure CLI desde:
# https://docs.microsoft.com/en-us/cli/azure/install-azure-cli-windows
```

### Login y configuración inicial
```powershell
# Login a Azure
az login

# Crear grupo de recursos
az group create --name "rg-siinadseg" --location "East US"

# Crear Azure SQL Server
az sql server create `
  --name "siinadseg-sql-server" `
  --resource-group "rg-siinadseg" `
  --location "East US" `
  --admin-user "sqladmin" `
  --admin-password "SiinadsegDB2024!"

# Configurar firewall para permitir servicios de Azure
az sql server firewall-rule create `
  --resource-group "rg-siinadseg" `
  --server "siinadseg-sql-server" `
  --name "AllowAzureServices" `
  --start-ip-address "0.0.0.0" `
  --end-ip-address "0.0.0.0"

# Crear Azure SQL Database
az sql db create `
  --resource-group "rg-siinadseg" `
  --server "siinadseg-sql-server" `
  --name "SiinadsegDB" `
  --service-objective "Basic"

# Crear App Service Plan
az appservice plan create `
  --name "siinadseg-plan" `
  --resource-group "rg-siinadseg" `
  --location "East US" `
  --sku "B1"

# Crear App Service
az webapp create `
  --resource-group "rg-siinadseg" `
  --plan "siinadseg-plan" `
  --name "siinadseg-api-app" `
  --runtime "DOTNET:8.0"
```

## 🔧 PASO 2: Configurar Connection String

### Obtener connection string
```powershell
# Obtener connection string de la base de datos
az sql db show-connection-string `
  --server "siinadseg-sql-server" `
  --name "SiinadsegDB" `
  --client "ado.net"
```

### Configurar App Service Settings
```powershell
# Configurar connection string en App Service
az webapp config connection-string set `
  --resource-group "rg-siinadseg" `
  --name "siinadseg-api-app" `
  --connection-string-type "SQLAzure" `
  --settings DefaultConnection="Server=tcp:siinadseg-sql-server.database.windows.net,1433;Initial Catalog=SiinadsegDB;Persist Security Info=False;User ID=sqladmin;Password=SiinadsegDB2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"

# Configurar CORS
az webapp cors add `
  --resource-group "rg-siinadseg" `
  --name "siinadseg-api-app" `
  --allowed-origins "https://agreeable-water-06170cf10.1.azurestaticapps.net"
```

## 🔧 PASO 3: Preparar y Desplegar Backend

### Configurar appsettings para Azure
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:siinadseg-sql-server.database.windows.net,1433;Initial Catalog=SiinadsegDB;Persist Security Info=False;User ID=sqladmin;Password=SiinadsegDB2024!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Cors": {
    "AllowedOrigins": [
      "https://agreeable-water-06170cf10.1.azurestaticapps.net",
      "http://localhost:4200"
    ]
  }
}
```

### Comandos de despliegue
```powershell
# Navegar al directorio del backend
cd backend/src/WebApi

# Crear package de publicación
dotnet publish -c Release -o ./publish

# Comprimir para deployment
Compress-Archive -Path "./publish/*" -DestinationPath "./deploy.zip" -Force

# Desplegar usando Azure CLI
az webapp deployment source config-zip `
  --resource-group "rg-siinadseg" `
  --name "siinadseg-api-app" `
  --src "./deploy.zip"
```

## 🔧 PASO 4: Actualizar Frontend

### Actualizar environment.prod.ts
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://siinadseg-api-app.azurewebsites.net/api',
  version: '20241019-azure-full',
  enableLogging: false
};
```

### Reconstruir y desplegar frontend
```powershell
# Navegar al frontend
cd frontend-new

# Build de producción
ng build --configuration=production

# Desplegar a Static Web Apps
cd ..
npx @azure/static-web-apps-cli deploy "frontend-new/dist/frontend-new" --deployment-token "ba9c8c8e6f2d1c04b52c77e1b5e47b7e2b0c5a8d9e3f4a5b6c7d8e9f0a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6e7f8a9b0c1d2e3f4a5b6c7d8e9f0a1b"
```

## 🔧 PASO 5: Configurar Base de Datos

### Ejecutar migraciones
```powershell
# Desde el directorio WebApi con connection string actualizado
dotnet ef database update
```

### Insertar datos iniciales
```sql
-- Ejecutar en Azure SQL Database
-- Usuario administrador
INSERT INTO Users (Email, Password, Role, Name, IsActive, CreatedAt)
VALUES ('admin@sinseg.com', '$2a$11$8K9XqF2L5N6M7P3Q4R8S1O...', 'admin', 'Administrador', 1, GETDATE());
```

## 🌐 URLs FINALES

- **Frontend**: https://agreeable-water-06170cf10.1.azurestaticapps.net
- **Backend API**: https://siinadseg-api-app.azurewebsites.net/api
- **Base de datos**: siinadseg-sql-server.database.windows.net

## 🔐 CREDENCIALES

- **SQL Admin**: sqladmin / SiinadsegDB2024!
- **App Login**: admin@sinseg.com / password123

## 📊 COSTOS ESTIMADOS (USD/mes)

- Azure SQL Basic: ~$5
- App Service B1: ~$13
- Static Web Apps: Gratis
- **Total**: ~$18/mes

## ✅ VERIFICACIÓN

1. Verificar App Service: https://siinadseg-api-app.azurewebsites.net/api/health
2. Verificar Frontend: https://agreeable-water-06170cf10.1.azurestaticapps.net/login
3. Probar login con admin@sinseg.com / password123