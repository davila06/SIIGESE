# Solución al Error de Mixed Content

## Problema Detectado

El frontend desplegado en Azure Static Web Apps (HTTPS) estaba intentando hacer requests HTTP al backend, causando un error de **Mixed Content** bloqueado por el navegador:

```
Mixed Content: The page at 'https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/polizas/upload' was loaded over HTTPS, but requested an insecure XMLHttpRequest endpoint 'http://siinadseg-backend.westus.azurecontainer.io/api/polizas/upload'. This request has been blocked; the content must be served over HTTPS.
```

### Causas
1. **URL antigua del backend**: El código estaba usando `http://siinadseg-backend.westus.azurecontainer.io` (deployment anterior)
2. **Mixed Content**: Frontend en HTTPS intentando acceder a backend en HTTP

## Solución Implementada

### Opción 1: Proxy Reverse en Static Web App (Implementada)

Configuré un proxy reverso en Azure Static Web App para que todas las llamadas a `/api/*` se redirijan al backend a través del servidor de Azure, evitando el problema de Mixed Content.

**Cambios realizados:**

1. **staticwebapp.config.json** - Agregada regla de proxy:
```json
{
  "route": "/api/*",
  "allowedRoles": ["anonymous"],
  "rewrite": "http://siinadseg-api-7464.eastus2.azurecontainer.io:8080/api/{*}"
}
```

2. **environment.prod.ts** - Cambiada URL a relativa:
```typescript
export const environment = {
  production: true,
  apiUrl: '/api'  // Ahora usa el proxy de SWA
};
```

### Ventajas de esta Solución
- ✅ No requiere HTTPS en el backend
- ✅ Elimina problemas de CORS
- ✅ Elimina Mixed Content
- ✅ Todas las requests pasan por HTTPS
- ✅ No requiere costos adicionales de Application Gateway

## Estado de Deployment

### Frontend
- **URL**: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
- **API URL**: `/api` (proxy reverso)
- **Estado**: 🔄 Redesplegando con nueva configuración

### Backend  
- **URL Real**: http://siinadseg-api-7464.eastus2.azurecontainer.io:8080
- **URL Proxy**: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/api
- **Estado**: ✅ Running

## Flujo de Requests

```
Browser (HTTPS)
    ↓
Frontend: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
    ↓
API Call: /api/polizas/upload (HTTPS)
    ↓
Azure Static Web App Proxy
    ↓
Backend: http://siinadseg-api-7464.eastus2.azurecontainer.io:8080/api/polizas/upload (HTTP)
    ↓
Response back through proxy (HTTPS)
    ↓
Browser (HTTPS)
```

## Próximos Pasos

1. ✅ Build de frontend completado
2. 🔄 Deploy a Azure Static Web App
3. ⏳ Verificar que el proxy funciona correctamente
4. ⏳ Probar upload de archivos Excel

## Alternativas Consideradas (No Implementadas)

### Opción 2: HTTPS en Backend con Application Gateway
- **Costo**: ~$125/mes
- **Ventaja**: SSL/TLS nativo en backend
- **Desventaja**: Costo adicional significativo

### Opción 3: Azure API Management
- **Costo**: ~$50/mes (tier Consumption)
- **Ventaja**: Gestión completa de APIs, throttling, analytics
- **Desventaja**: Complejidad adicional

### Opción 4: Azure Front Door
- **Costo**: ~$35/mes + data transfer
- **Ventaja**: CDN global, WAF, SSL
- **Desventaja**: Overkill para este proyecto

## Comandos Ejecutados

```bash
# 1. Actualizar environment files
# environment.ts y environment.prod.ts actualizados

# 2. Configurar proxy en staticwebapp.config.json
# Regla de rewrite agregada

# 3. Rebuild frontend
ng build --configuration production

# 4. Redeploy a Azure
swa deploy ./dist/frontend-new --deployment-token $token --env production
```

## Verificación Post-Deployment

```bash
# Verificar que el frontend carga
curl https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net

# Verificar que el proxy funciona
curl https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/api/health

# Verificar upload (desde browser con auth token)
# POST https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/api/polizas/upload
```

---

*Última actualización: 2025-12-17 13:55 UTC*
