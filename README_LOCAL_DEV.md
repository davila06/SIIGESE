# 🏠 SIIGESE - Configuración para Desarrollo Local

## 🚀 Inicio Rápido

### 📋 Prerequisitos
- **Node.js** (versión 18+)
- **npm** (incluido con Node.js)
- **Angular CLI** (se instala automáticamente)
- **SQL Server Express** ejecutándose en `Karo\SQLEXPRESS`
- **Base de Datos** `SinsegAppDb` configurada

### ⚡ Método 1: Script Automático

#### Windows (Batch)
```cmd
# Verificar base de datos primero
.\setup-local-database.ps1

# Luego iniciar aplicación
.\start-local-dev.bat
```

#### Windows (PowerShell)
```powershell
# Verificar base de datos primero
.\setup-local-database.ps1

# Luego iniciar aplicación
.\start-local-dev.ps1
```

### 🔧 Método 2: Manual

```cmd
# 1. Verificar base de datos
.\setup-local-database.ps1

# 2. Configurar aplicación
cd frontend-new
npm install
npm run start:local
```

## 🗄️ Configuración de Base de Datos

### 📋 Tu Configuración Local
- **Servidor**: `Karo\SQLEXPRESS`
- **Base de Datos**: `SinsegAppDb`
- **Autenticación**: Windows Authentication
- **Connection String**: `Server=Karo\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;`

### 🛠️ Setup Base de Datos
```powershell
# Verificar estado de la base de datos
.\setup-local-database.ps1

# Si necesita crear la BD, usar SSMS:
# 1. Conectar a Karo\SQLEXPRESS
# 2. Ejecutar EJECUTAR_COMPLETO.sql
```

## 📱 Acceso a la Aplicación

- **URL Local**: http://localhost:4200
- **Usuario**: admin@sinseg.com
- **Contraseña**: password123

## ⚙️ Configuraciones Disponibles

### 🏠 Local Development (`start:local`)
- **Environment**: `environment.local.ts`
- **Mock API**: ✅ Habilitado
- **Debug**: ✅ Habilitado
- **Puerto**: 4200
- **Logs**: ✅ Detallados

### 🔧 Development (`start`)
- **Environment**: `environment.ts`
- **Mock API**: ✅ Habilitado
- **Proxy**: ✅ proxy.conf.json
- **Puerto**: 4200

### 🏭 Production (`build:prod`)
- **Environment**: `environment.prod.ts`
- **Mock API**: ❌ Deshabilitado
- **Azure Backend**: ✅ Habilitado

## 📝 Scripts Disponibles

```json
{
  "start:local": "Desarrollo local con Mock API",
  "build:local": "Build para desarrollo local",
  "watch:local": "Build continuo para desarrollo",
  "start": "Desarrollo estándar con proxy",
  "build:prod": "Build de producción para Azure"
}
```

## 🔍 Debugging

### Console Logs
Con la configuración local verás logs como:
```
🔄 Mock API Interceptor: POST /api/auth/login
✅ Mock Login successful for: admin@sinseg.com
🛡️ AuthGuard - Access granted
✅ Navigation to /polizas result: true
```

### Dev Tools
- **F12**: Abrir herramientas de desarrollador
- **Console**: Ver logs de la aplicación
- **Network**: Ver llamadas interceptadas por Mock API

## 🗂️ Estructura del Proyecto

```
frontend-new/
├── src/
│   ├── environments/
│   │   ├── environment.ts          # Desarrollo estándar
│   │   ├── environment.local.ts    # 🏠 Desarrollo local
│   │   └── environment.prod.ts     # Producción Azure
│   ├── app/
│   │   ├── interceptors/
│   │   │   └── mock-api.interceptor.ts  # Mock API
│   │   └── ...
│   └── ...
├── angular.json        # Configuraciones de build
├── package.json        # Scripts y dependencias
└── proxy.conf.json     # Configuración de proxy
```

## 🧪 Funcionalidades en Local

### ✅ Completamente Funcional
- 🔐 **Autenticación**: Login/logout con Mock API
- 📄 **Pólizas**: CRUD completo (Create, Read, Update, Delete)
- 🧭 **Routing**: Navegación entre módulos
- 📱 **UI/UX**: Interfaz completa con Angular Material

### 🔄 Simulado (Mock API)
- 🌐 **Backend API**: Interceptor simula todas las llamadas
- 💾 **Base de Datos**: Datos de prueba en memoria
- 🔑 **JWT Tokens**: Generación y validación simulada

## 🛠️ Personalización

### Cambiar Puerto
En `package.json`, editar:
```json
"start:local": "ng serve --configuration local --port 4200"
```

### Agregar Datos de Prueba
En `mock-api.interceptor.ts`, editar los arrays de datos:
```typescript
private users = [...];  // Usuarios de prueba
private polizas = [...]; // Pólizas de prueba
```

### Configurar Debugging
En `environment.local.ts`:
```typescript
local: {
  debugMode: true,     // Logs detallados
  skipAuth: false,     // Saltar autenticación
  useLocalStorage: true // Persistir datos en localStorage
}
```

## 🆘 Solución de Problemas

### Puerto ya en uso
```cmd
# Encontrar proceso usando puerto 4200
netstat -ano | findstr :4200
# Terminar proceso
taskkill /PID <PID> /F
```

### Problemas de Node.js
```cmd
# Limpiar cache de npm
npm cache clean --force
# Reinstalar dependencias
rm -rf node_modules package-lock.json
npm install
```

### Problemas de Angular
```cmd
# Reinstalar Angular CLI
npm uninstall -g @angular/cli
npm install -g @angular/cli@latest
```

## 📞 Soporte

Para problemas con el desarrollo local:
1. Verificar que Node.js esté instalado correctamente
2. Comprobar que el puerto 4200 esté libre
3. Revisar logs en la consola del navegador
4. Verificar que todas las dependencias estén instaladas

---

**¡Happy Coding! 🚀**