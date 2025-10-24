# 🚀 SIINADSEG - Guía de Deployment en Azure

## 📦 Scripts Creados para tu Deployment

He creado un conjunto completo de scripts para deployar tu aplicación SIINADSEG en Azure. Aquí tienes todo lo necesario:

### 📋 Scripts Disponibles

| Script | Descripción | Uso |
|--------|-------------|-----|
| `create-azure-resources.ps1` | Crea todos los recursos de Azure desde cero | Nuevos deployments |
| `deploy-with-existing-resources.ps1` | Usa tus recursos Azure existentes | **Recomendado para ti** |
| `migrate-azure-database.ps1` | Ejecuta migraciones de BD | Actualizar esquema |
| `deploy-backend.ps1` | Despliega backend a App Service | Backend deployment |
| `build-frontend.ps1` | Construye frontend para producción | Frontend build |
| `deploy-complete.ps1` | Deployment completo automatizado | Todo en uno |

### 🎯 Para Tu Caso Específico

Como ya tienes recursos Azure configurados, usa el script optimizado:

```powershell
# Ejecutar deployment con recursos existentes
.\deploy-with-existing-resources.ps1
```

Este script:
- ✅ Lee tu configuración actual de `azure-deployment-config.json`
- ✅ Ejecuta migraciones en tu BD existente
- ✅ Construye el frontend con tu backend URL
- ✅ Prepara archivos para deployment

### 🏗️ Recursos Azure Detectados en tu Config

```json
{
  "SQL Server": "siinadseg-sqlserver-1019.database.windows.net",
  "Database": "SiinadsegDB", 
  "Backend": "http://siinadseg-backend-1019.eastus.azurecontainer.io",
  "Frontend": "https://agreeable-water-06170cf10.1.azurestaticapps.net",
  "Resource Group": "siinadseg-rg"
}
```

### 📋 Pasos Simplificados

#### 1. **Preparar Entorno**
```powershell
# Verificar herramientas
az --version
dotnet --version
node --version
```

#### 2. **Ejecutar Deployment**
```powershell
# Desde la raíz del proyecto
.\deploy-with-existing-resources.ps1
```

#### 3. **Actualizar Frontend**
- El script genera `frontend-deployment.zip`
- Sube este archivo a tu Static Web App desde Azure Portal

#### 4. **Verificar Aplicación**
- Frontend: https://agreeable-water-06170cf10.1.azurestaticapps.net
- Backend API: http://siinadseg-backend-1019.eastus.azurecontainer.io/api

### 🔧 CI/CD Automático

He incluido un workflow de GitHub Actions (`.github/workflows/azure-deploy.yml`) que:
- ✅ Construye backend y frontend automáticamente
- ✅ Ejecuta tests
- ✅ Despliega en cada push a main/V1
- ✅ Notifica el estado del deployment

### 💰 Costos Estimados

Basado en tu configuración actual:
- **SQL Database**: ~$30/mes
- **Container Instance**: ~$10/mes  
- **Static Web App**: Gratis
- **Total**: ~$40/mes

### 🚀 Inicio Rápido

1. **Ejecuta el script principal:**
   ```powershell
   .\deploy-with-existing-resources.ps1
   ```

2. **Sube el frontend:**
   - Ve a Azure Portal
   - Busca tu Static Web App
   - Sube `frontend-deployment.zip`

3. **¡Listo!** Tu aplicación estará funcionando

### 🆘 Soporte

Si necesitas ayuda:
- Revisa `AZURE_DEPLOYMENT_GUIDE.md` para documentación completa
- Los scripts incluyen logging detallado
- Cada paso muestra mensajes de éxito/error claros

### 🎉 Ventajas de esta Configuración

- ✅ **Escalable**: App Service Plan B1 puede escalar
- ✅ **Seguro**: HTTPS, bases de datos encriptadas
- ✅ **Monitoreado**: Application Insights incluido
- ✅ **Automatizado**: CI/CD con GitHub Actions
- ✅ **Económico**: ~$40/mes para aplicación empresarial

---

**¿Listo para empezar?** Ejecuta: `.\deploy-with-existing-resources.ps1` 🚀