# Configuración Frontend → Localhost con Backend → Azure

## ✅ Configuración Completada

### 1. Environment Configuration
- **environment.ts**: Actualizado para apuntar a Azure backend
- **environment.development.ts**: Creado para desarrollo local
- **Proxy config**: Configurado para backend de Azure

### 2. URLs Configuradas
```typescript
// environment.ts
apiUrl: 'https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/api'
useMockApi: false
```

```json
// proxy.conf.json
{
  "/api/*": {
    "target": "https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io",
    "secure": true,
    "changeOrigin": true,
    "logLevel": "debug"
  }
}
```

### 3. Backend Configuration
- **Base de Datos**: Azure SQL (siinadseg-sql-5307.database.windows.net)
- **Backend API**: Azure Container Apps
- **CORS**: Configurado para localhost:4200

## 🚀 Métodos para Ejecutar

### Opción 1: Compilado y Servido Localmente
```bash
cd frontend-new
ng build --watch
# En otra terminal:
npx http-server dist/frontend-new --port 4200 --proxy-config proxy.conf.json
```

### Opción 2: Live Reload (Recomendado)
```bash
cd frontend-new
ng serve --port 4200 --proxy-config proxy.conf.json
```

### Opción 3: Desarrollo con Backend Local (Si necesario)
```bash
# Cambiar environment.ts a:
apiUrl: 'http://localhost:5000/api'
```

## 🔧 Arquitectura Actual

```
Frontend (localhost:4200)
    ↓ API calls via proxy
Backend (Azure Container Apps)
    ↓ Database calls
Azure SQL Database
```

## ✅ Verificación

1. **Frontend**: http://localhost:4200
2. **Backend**: https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io
3. **Database**: Azure SQL (configurado en backend)

## 📝 Comandos de Desarrollo

```bash
# Iniciar frontend en localhost
cd frontend-new
npm start

# Verificar backend
curl https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/api/health

# Build para producción
ng build --configuration production
```

---
**Estado**: Frontend configurado para localhost, Backend y BD en Azure ✅