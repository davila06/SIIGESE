# ðŸ  OmnIA - ConfiguraciÃ³n para Desarrollo Local

## ðŸš€ Inicio RÃ¡pido

### ðŸ“‹ Prerequisitos
- **Node.js** (versiÃ³n 18+)
- **npm** (incluido con Node.js)
- **Angular CLI** (se instala automÃ¡ticamente)
- **SQL Server Express** ejecutÃ¡ndose en `Karo\SQLEXPRESS`
- **Base de Datos** `SinsegAppDb` configurada

### âš¡ MÃ©todo 1: Script AutomÃ¡tico

#### Windows (Batch)
```cmd
# Verificar base de datos primero
.\setup-local-database.ps1

# Luego iniciar aplicaciÃ³n
.\start-local-dev.bat
```

#### Windows (PowerShell)
```powershell
# Verificar base de datos primero
.\setup-local-database.ps1

# Luego iniciar aplicaciÃ³n
.\start-local-dev.ps1
```

### ðŸ”§ MÃ©todo 2: Manual

```cmd
# 1. Verificar base de datos
.\setup-local-database.ps1

# 2. Configurar aplicaciÃ³n
cd frontend-new
npm install
npm run start:local
```

## ðŸ—„ï¸ ConfiguraciÃ³n de Base de Datos

### ðŸ“‹ Tu ConfiguraciÃ³n Local
- **Servidor**: `Karo\SQLEXPRESS`
- **Base de Datos**: `SinsegAppDb`
- **AutenticaciÃ³n**: Windows Authentication
- **Connection String**: `Server=Karo\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;`

### ðŸ› ï¸ Setup Base de Datos
```powershell
# Verificar estado de la base de datos
.\setup-local-database.ps1

# Si necesita crear la BD, usar SSMS:
# 1. Conectar a Karo\SQLEXPRESS
# 2. Ejecutar EJECUTAR_COMPLETO.sql
```

## ðŸ“± Acceso a la AplicaciÃ³n

- **URL Local**: http://localhost:4200
- **Usuario**: admin@sinseg.com
- **ContraseÃ±a**: password123

## âš™ï¸ Configuraciones Disponibles

### ðŸ  Local Development (`start:local`)
- **Environment**: `environment.local.ts`
- **Mock API**: âœ… Habilitado
- **Debug**: âœ… Habilitado
- **Puerto**: 4200
- **Logs**: âœ… Detallados

### ðŸ”§ Development (`start`)
- **Environment**: `environment.ts`
- **Mock API**: âœ… Habilitado
- **Proxy**: âœ… proxy.conf.json
- **Puerto**: 4200

### ðŸ­ Production (`build:prod`)
- **Environment**: `environment.prod.ts`
- **Mock API**: âŒ Deshabilitado
- **Azure Backend**: âœ… Habilitado

## ðŸ“ Scripts Disponibles

```json
{
  "start:local": "Desarrollo local con Mock API",
  "build:local": "Build para desarrollo local",
  "watch:local": "Build continuo para desarrollo",
  "start": "Desarrollo estÃ¡ndar con proxy",
  "build:prod": "Build de producciÃ³n para Azure"
}
```

## ðŸ” Debugging

### Console Logs
Con la configuraciÃ³n local verÃ¡s logs como:
```
ðŸ”„ Mock API Interceptor: POST /api/auth/login
âœ… Mock Login successful for: admin@sinseg.com
ðŸ›¡ï¸ AuthGuard - Access granted
âœ… Navigation to /polizas result: true
```

### Dev Tools
- **F12**: Abrir herramientas de desarrollador
- **Console**: Ver logs de la aplicaciÃ³n
- **Network**: Ver llamadas interceptadas por Mock API

## ðŸ—‚ï¸ Estructura del Proyecto

```
frontend-new/
â”œâ”€â”€ src/
â”‚   â”œâ”€â”€ environments/
â”‚   â”‚   â”œâ”€â”€ environment.ts          # Desarrollo estÃ¡ndar
â”‚   â”‚   â”œâ”€â”€ environment.local.ts    # ðŸ  Desarrollo local
â”‚   â”‚   â””â”€â”€ environment.prod.ts     # ProducciÃ³n Azure
â”‚   â”œâ”€â”€ app/
â”‚   â”‚   â”œâ”€â”€ interceptors/
â”‚   â”‚   â”‚   â””â”€â”€ mock-api.interceptor.ts  # Mock API
â”‚   â”‚   â””â”€â”€ ...
â”‚   â””â”€â”€ ...
â”œâ”€â”€ angular.json        # Configuraciones de build
â”œâ”€â”€ package.json        # Scripts y dependencias
â””â”€â”€ proxy.conf.json     # ConfiguraciÃ³n de proxy
```

## ðŸ§ª Funcionalidades en Local

### âœ… Completamente Funcional
- ðŸ” **AutenticaciÃ³n**: Login/logout con Mock API
- ðŸ“„ **PÃ³lizas**: CRUD completo (Create, Read, Update, Delete)
- ðŸ§­ **Routing**: NavegaciÃ³n entre mÃ³dulos
- ðŸ“± **UI/UX**: Interfaz completa con Angular Material

### ðŸ”„ Simulado (Mock API)
- ðŸŒ **Backend API**: Interceptor simula todas las llamadas
- ðŸ’¾ **Base de Datos**: Datos de prueba en memoria
- ðŸ”‘ **JWT Tokens**: GeneraciÃ³n y validaciÃ³n simulada

## ðŸ› ï¸ PersonalizaciÃ³n

### Cambiar Puerto
En `package.json`, editar:
```json
"start:local": "ng serve --configuration local --port 4200"
```

### Agregar Datos de Prueba
En `mock-api.interceptor.ts`, editar los arrays de datos:
```typescript
private users = [...];  // Usuarios de prueba
private polizas = [...]; // PÃ³lizas de prueba
```

### Configurar Debugging
En `environment.local.ts`:
```typescript
local: {
  debugMode: true,     // Logs detallados
  skipAuth: false,     // Saltar autenticaciÃ³n
  useLocalStorage: true // Persistir datos en localStorage
}
```

## ðŸ†˜ SoluciÃ³n de Problemas

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

## ðŸ“ž Soporte

Para problemas con el desarrollo local:
1. Verificar que Node.js estÃ© instalado correctamente
2. Comprobar que el puerto 4200 estÃ© libre
3. Revisar logs en la consola del navegador
4. Verificar que todas las dependencias estÃ©n instaladas

---

**Â¡Happy Coding! ðŸš€**
