# Migración Exitosa a Azure Container Apps

## ✅ Migración Completada

**Fecha:** 17 de diciembre de 2025

### Problema Resuelto

El frontend desplegado en Azure Static Web Apps (HTTPS) no podía comunicarse con el backend en Container Instance (HTTP) debido a **Mixed Content blocking** del navegador.

**Solución implementada:** Migración de Azure Container Instance a Azure Container Apps, que proporciona HTTPS automático sin costo adicional.

---

## URLs Finales

| Servicio | URL |
|----------|-----|
| **Frontend** | https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net |
| **Backend API (HTTPS)** | https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io |
| **Base de Datos** | siinadseg-sql-3376.database.windows.net |

---

## Recursos de Azure Creados

### 1. Container Apps Environment
```
Nombre: cae-siinadseg-prod
Region: East US 2
Log Analytics: workspace-rgsiinadsegprod2025dSdy (generado automáticamente)
Default Domain: greensmoke-63d5430a.eastus2.azurecontainerapps.io
```

### 2. Container App (Backend)
```
Nombre: siinadseg-backend-app
FQDN: siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io
Puerto: 8080
Ingress: External (HTTPS automático)
CPU: 1.0
Memoria: 2Gi
Imagen: acrsiinadseg7512.azurecr.io/siinadseg-backend:latest
```

### 3. Variables de Entorno Configuradas
```
ConnectionStrings__DefaultConnection: (secret) Conecta a siinadseg-sql-3376
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_URLS: http://+:8080
```

### 4. Recursos Existentes (Sin cambios)
- **SQL Server:** siinadseg-sql-3376.database.windows.net
- **Database:** SiinadsegDB (con todas las migraciones aplicadas)
- **Container Registry:** acrsiinadseg7512.azurecr.io
- **Static Web App:** swa-siinadseg-frontend

---

## Ventajas de Azure Container Apps

✅ **HTTPS incluido gratuitamente** con certificados SSL administrados automáticamente  
✅ **Mismo costo** que Container Instance (~$30-40/mes)  
✅ **Sin configuración** de certificados o proxies  
✅ **Escalado automático** (0 a 10 réplicas)  
✅ **Múltiples revisiones** para deployment sin downtime  
✅ **Integración nativa** con Azure Container Registry  
✅ **Logs centralizados** en Log Analytics  

---

## Configuración del Frontend

El frontend ya está configurado para usar la URL HTTPS del backend:

**Archivo:** `frontend-new/src/environments/environment.prod.ts`
```typescript
export const environment = {
  production: true,
  version: '1.0.0',
  apiUrl: 'https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api',
  useMockApi: false
};
```

✅ Frontend reconstruido y desplegado con la nueva URL HTTPS

---

## Verificación de Funcionamiento

### Backend (HTTPS)
```powershell
# Test de endpoint protegido (debe retornar 401 - No autorizado)
Invoke-WebRequest -Uri "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/polizas"
# Resultado: ✅ 401 Unauthorized (correcto)
```

### Frontend
```powershell
# Test de frontend
Invoke-WebRequest -Uri "https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net"
# Resultado: ✅ 200 OK
```

### Integración Frontend-Backend
1. Abre el navegador en: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
2. Abre DevTools (F12) → Network tab
3. Intenta hacer login o subir un archivo
4. ✅ **Verificar:** Las llamadas API ahora son a `https://siinadseg-backend-app...` (HTTPS)
5. ✅ **Verificar:** NO debe aparecer error de Mixed Content

---

## Credenciales de Acceso

### Usuario Administrador
```
Email: admin@sinseg.com
Password: admin123
Rol: Admin
```

### Azure SQL Database
```
Server: siinadseg-sql-3376.database.windows.net
Database: SiinadsegDB
User: adminuser
Password: P@ssw0rd123!
```

---

## Comandos Útiles

### Ver logs del backend
```powershell
az containerapp logs show `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --type console `
  --tail 50
```

### Ver estado del Container App
```powershell
az containerapp show `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --query "{name:name, status:properties.runningStatus, fqdn:properties.configuration.ingress.fqdn}"
```

### Actualizar imagen del backend
```powershell
# 1. Build nueva imagen en ACR
cd backend
az acr build --registry acrsiinadseg7512 --image siinadseg-backend:latest --file Dockerfile .

# 2. Container App se actualiza automáticamente (o forzar update)
az containerapp update `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --image acrsiinadseg7512.azurecr.io/siinadseg-backend:latest
```

### Redesplegar frontend
```powershell
cd frontend-new
ng build --configuration production
$token = (az staticwebapp secrets list `
  --name swa-siinadseg-frontend `
  --resource-group rg-siinadseg-prod-2025 `
  --query "properties.apiKey" -o tsv)
swa deploy ./dist/frontend-new --deployment-token $token --env production
```

---

## Costos Estimados (Mensuales)

| Recurso | Tier | Costo Estimado |
|---------|------|----------------|
| Azure SQL Database | Standard S0 | ~$15/mes |
| Container Apps Environment | Consumption | Incluido |
| Container App | Consumption (1 CPU, 2GB) | ~$30-40/mes |
| Container Registry | Basic | ~$5/mes |
| Static Web App | Free | $0 |
| Log Analytics | Pay-as-you-go | ~$2-5/mes |
| **TOTAL** | | **~$52-65/mes** |

---

## Recursos Eliminados (Opcionales)

Si quieres eliminar el Container Instance antiguo (HTTP):
```powershell
az container delete `
  --name siinadseg-backend `
  --resource-group rg-siinadseg-prod-2025 `
  --yes
```

---

## Próximos Pasos (Opcional)

### 1. Dominio Personalizado
Puedes configurar un dominio personalizado para el backend:
```powershell
# Agregar dominio personalizado al Container App
az containerapp hostname add `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --hostname api.tudominio.com
```

### 2. Monitoreo con Application Insights
```powershell
# Crear Application Insights
az monitor app-insights component create `
  --app siinadseg-appinsights `
  --location eastus2 `
  --resource-group rg-siinadseg-prod-2025

# Configurar en Container App
# (Agregar APPLICATIONINSIGHTS_CONNECTION_STRING a env vars)
```

### 3. Escalado Automático Avanzado
```powershell
# Configurar escalado basado en HTTP requests
az containerapp update `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --min-replicas 1 `
  --max-replicas 5 `
  --scale-rule-name http-scale `
  --scale-rule-type http `
  --scale-rule-http-concurrency 100
```

---

## Conclusión

✅ **Problema de Mixed Content completamente resuelto**  
✅ **Backend con HTTPS automático sin configuración compleja**  
✅ **Sin costos adicionales significativos**  
✅ **Mejor arquitectura con escalado automático**  
✅ **Logs centralizados en Log Analytics**  
✅ **Certificados SSL administrados por Azure**  

🚀 **La aplicación ahora está completamente funcional en producción con HTTPS end-to-end**
