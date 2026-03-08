# INFORMACIÓN DE ACCESO - SIINADSEG

## 🌐 URLs de Acceso

### 🌍 Producción (Azure)
- **URL Principal**: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
- **URL Preview**: https://agreeable-smoke-0b5eb210f-preview.westus2.3.azurestaticapps.net

### 🏠 Desarrollo Local
- **URL Local**: http://localhost:4200
- **Configuración**: Mock API habilitado
- **Scripts**: `npm run start:local`
- **Base de Datos**: `Karo\SQLEXPRESS - SinsegAppDb`

### Backend API
- **URL API**: http://siinadseg-backend-1019.eastus.azurecontainer.io/api

### Base de Datos
- **Servidor**: siinadseg-sqlserver-1019.database.windows.net
- **Base de Datos**: SiinadsegDB

## 🔐 Credenciales de Acceso

### Aplicación Web
- **Administrador**: admin@sinseg.com / password123 (Todos los permisos)
- **Cargador de Datos**: dataloader@sinseg.com / password123 (Solo carga de pólizas)
- **Usuario**: user@sinseg.com / password123 (Solo visualización)

### Base de Datos SQL
- **Usuario**: siinadseg_admin
- **Contraseña**: P@ssw0rd123!

## 📋 Azure Resources

### Resource Group
- **Nombre**: siinadseg-rg
- **Ubicación**: East US

### Static Web App
- **Nombre**: siinadseg-frontend-new
- **ID**: /subscriptions/3832b5df-115d-4092-9fc8-2105d7b0af21/resourceGroups/siinadseg-rg/providers/Microsoft.Web/staticSites/siinadseg-frontend-new
- **Tier**: Free
- **Ubicación**: West US 2

### Azure Container Instance
- **Nombre**: siinadseg-backend-1019
- **FQDN**: siinadseg-backend-1019.eastus.azurecontainer.io
- **Puerto**: 80

### Azure SQL Database
- **Servidor**: siinadseg-sqlserver-1019
- **Base de Datos**: SiinadsegDB
- **Tier**: Basic (5 DTU)

## ✅ Estado del Sistema

- ✅ Frontend desplegado y funcionando
- ✅ Base de datos SQL operativa
- ✅ Container Instance backend ejecutándose
- ✅ Autenticación configurada con Mock API
- ✅ Routing corregido y funcionando
- ✅ CRUD de pólizas completo (Create, Read, Update, Delete)
- ✅ Sistema listo para uso completo

## 🚀 Cómo Acceder

### 🌍 Producción (Azure)
1. **Navegar a**: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net/login
2. **Abrir la consola del navegador (F12)** para ver logs de debugging
3. **Iniciar sesión con**:
   - Email: admin@sinseg.com
   - Password: password123
4. **Explorar las funcionalidades**:
   - Dashboard principal
   - Gestión de pólizas
   - Sistema de cobros
   - Gestión de reclamos

### 🏠 Desarrollo Local
1. **Verificar base de datos**:
   ```powershell
   .\setup-local-database.ps1
   ```
2. **Ejecutar script de inicio**:
   ```cmd
   .\start-local-dev.bat
   ```
   O manualmente:
   ```cmd
   cd frontend-new
   npm install
   npm run start:local
   ```
3. **Abrir navegador**: http://localhost:4200
4. **Iniciar sesión con las mismas credenciales**
5. **Ver documentación**: README_LOCAL_DEV.md

## 🔧 Debugging del Routing

Si tienes problemas de navegación, revisa la consola del navegador para estos logs:

- `🔄 Mock API Interceptor` - Interceptación de requests
- `✅ Mock Login successful` - Login exitoso
- `🛡️ AuthGuard` - Verificación de permisos
- `✅ Navigation to /polizas result` - Resultado de navegación
- `🔄 App.component - User state changed` - Cambios de estado de usuario

## 📝 Notas Importantes

- El sistema usa un Mock API interceptor para autenticación y operaciones CRUD
- Interceptor maneja: Login, GET/POST/PUT/DELETE de pólizas
- La base de datos está configurada con datos de prueba
- El backend container está ejecutando una aplicación .NET de ejemplo
- Todos los recursos están en el plan gratuito de Azure
- **ÚLTIMO UPDATE**: Corregido módulo de carga de pólizas - roles ajustados (20 Oct 2025)

## 🔧 Deployment Token

```
93766c8de396c161e4e03d28fadfed450b37b31d90a50e487982cbed34edee8203-68898e8e-452d-4e8f-9fed-588a19c4490e01e25290f3168a1e
```

*Fecha de actualización: 19 de octubre de 2024*