# Guía para Desplegar Backend en Azure App Service

## Paso 1: Crear Azure App Service
1. Ve a Azure Portal: https://portal.azure.com
2. Crear recurso → Web App
3. Configuración:
   - Nombre: siinadseg-api
   - Runtime: .NET 8
   - Región: Misma que Static Web App
   - Plan: Basic B1 o superior

## Paso 2: Configurar Connection String
En App Service → Configuration → Connection strings:
- Nombre: DefaultConnection
- Valor: Server=tcp:siinadseg-sql-5307.database.windows.net,1433;Initial Catalog=SiinadsegDB;User ID=siinadsegadmin;Password=Siinadseg2024!SecurePass;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
- Tipo: SQLServer

## Paso 3: Configurar CORS
En App Service → CORS:
- Orígenes permitidos: https://agreeable-water-06170cf10.1.azurestaticapps.net

## Paso 4: Deploy del Backend
Usar Visual Studio Code o Azure CLI:
```bash
az webapp deployment source config-zip --resource-group <resource-group> --name siinadseg-api --src backend.zip
```

## Paso 5: Actualizar Frontend
Cambiar en environment.prod.ts:
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://siinadseg-api.azurewebsites.net/api',
  version: '20251015-000000',
  enableLogging: false
};
```

## Paso 6: Re-desplegar Frontend
```bash
ng build --configuration=production
swa deploy
```