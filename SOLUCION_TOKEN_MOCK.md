# Solución al Error 401 - Token Mock

## 🔍 Problema Identificado

El frontend estaba usando un token "mock-jwt-token..." en lugar de un token real del backend:

```
🔐 AuthInterceptor: Agregando token a la request: {token: 'mock-jwt-token...'}
POST https://...azurecontainerapps.io/api/polizas/upload 401 (Unauthorized)
```

## ✅ Solución Aplicada

Se actualizó el `AuthService` para usar el `ApiService` real que se conecta al backend HTTPS:

### Cambios Realizados

1. **AuthService actualizado** ([auth.service.ts](frontend-new/src/app/services/auth.service.ts))
   - Ahora importa interfaces desde `user.interface.ts`
   - Usa `ApiService.login()` para conectarse al backend real
   - Elimina el login mockeado

2. **Frontend reconstruido y desplegado**
   - Build production completado exitosamente
   - Desplegado a Azure Static Web App

---

## 📋 Instrucciones para el Usuario

### Paso 1: Limpiar localStorage

Abre la consola del navegador (F12) y ejecuta:

```javascript
localStorage.clear();
location.reload();
```

O simplemente:

1. Abre el navegador en: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
2. Presiona **F12** para abrir DevTools
3. Ve a la pestaña **Application** o **Almacenamiento**
4. En el menú lateral, selecciona **Local Storage**
5. Click derecho → **Clear**
6. Recarga la página (**F5**)

### Paso 2: Hacer Login Real

1. Serás redirigido automáticamente a la página de login
2. Ingresa las credenciales del administrador:
   - **Email:** admin@sinseg.com
   - **Password:** admin123
3. Click en **Iniciar Sesión**

### Paso 3: Verificar Token Real

Después del login exitoso, abre la consola del navegador (F12) y ejecuta:

```javascript
console.log('Token:', localStorage.getItem('authToken'));
```

Deberías ver un JWT real (no "mock-jwt-token"), similar a:
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ...
```

### Paso 4: Probar Upload de Archivo

1. Ve a **Pólizas** → **Subir Excel**
2. Selecciona un archivo Excel
3. Click en **Subir**

Ahora debería funcionar correctamente con el token real.

---

## 🔐 Cómo Funciona Ahora

### Flujo de Autenticación

```
1. Usuario ingresa email/password en el login
   ↓
2. Frontend envía POST a: https://siinadseg-backend-app.../api/auth/login
   ↓
3. Backend valida credenciales contra la BD
   ↓
4. Backend genera JWT token real y lo retorna
   ↓
5. Frontend guarda el token en localStorage
   ↓
6. AuthInterceptor agrega el token a todas las requests
   ↓
7. Backend valida el token en cada request
```

### Verificación en DevTools

En la pestaña **Network** del navegador, cuando hagas cualquier request al backend, verás:

**Request Headers:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

En lugar del antiguo:
```
Authorization: Bearer mock-jwt-token
```

---

## ⚠️ Notas Importantes

### Si Sigues Viendo 401 Unauthorized

1. **Verifica que hiciste login de nuevo** (no uses el token mock antiguo)
2. **Verifica que el backend esté corriendo:**
   ```powershell
   az containerapp show --name siinadseg-backend-app --resource-group rg-siinadseg-prod-2025 --query "properties.runningStatus"
   ```
   Debe retornar: `"Running"`

3. **Verifica que el token sea válido:**
   - Abre DevTools → Application → Local Storage
   - Verifica que `authToken` no sea "mock-jwt-token"

4. **Si el problema persiste**, limpia localStorage y vuelve a hacer login

### Credenciales de Usuario Administrador

```
Email: admin@sinseg.com
Password: admin123
Roles: Admin (todos los permisos)
```

---

## 🎯 Resultado Esperado

Después de seguir estos pasos:

✅ El token será real (JWT generado por el backend)  
✅ Las requests al backend serán autorizadas (200 OK)  
✅ El upload de archivos Excel funcionará correctamente  
✅ Todas las operaciones CRUD funcionarán sin errores 401  

---

## 🔧 Para Desarrolladores

### Logs del Backend

Para ver qué está pasando en el backend:

```powershell
az containerapp logs show `
  --name siinadseg-backend-app `
  --resource-group rg-siinadseg-prod-2025 `
  --type console `
  --tail 50
```

### Verificar si el Login Funciona

Desde PowerShell:

```powershell
$body = @{
    email = "admin@sinseg.com"
    password = "admin123"
} | ConvertTo-Json

Invoke-WebRequest `
  -Uri "https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io/api/auth/login" `
  -Method POST `
  -Body $body `
  -ContentType "application/json" `
  -UseBasicParsing
```

Debería retornar 200 OK con un token JWT real.

---

## 📚 Referencias

- Backend URL: https://siinadseg-backend-app.greensmoke-63d5430a.eastus2.azurecontainerapps.io
- Frontend URL: https://agreeable-smoke-0b5eb210f.3.azurestaticapps.net
- Documentación completa: [MIGRACION_CONTAINER_APPS_EXITOSA.md](MIGRACION_CONTAINER_APPS_EXITOSA.md)
