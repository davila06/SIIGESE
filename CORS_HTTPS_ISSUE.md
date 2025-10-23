# ⚠️ PROBLEMA IDENTIFICADO: CORS HTTPS → HTTP

## El Issue
El sitio en Azure (HTTPS) no puede conectarse al backend local (HTTP) por restricciones de seguridad del navegador.

## Soluciones Disponibles

### Opción 1: Usar ngrok (Recomendado para pruebas rápidas)
```bash
# 1. Instalar ngrok: https://ngrok.com/download
# 2. Iniciar backend local en puerto 5000
# 3. Crear túnel HTTPS:
ngrok http 5000

# 4. Actualizar environment.prod.ts con URL de ngrok
# 5. Redeploy frontend
```

### Opción 2: Usar Azure App Service (Recomendado para producción)
```bash
# Seguir guía: AZURE_APP_SERVICE_GUIDE.md
```

### Opción 3: Configurar HTTPS local
```bash
# 1. Generar certificado local
dotnet dev-certs https --trust

# 2. Ejecutar backend en HTTPS
dotnet run --urls "https://localhost:5001"

# 3. Actualizar environment.prod.ts
apiUrl: 'https://localhost:5001/api'

# ⚠️ Problema: Certificado no válido para sitio externo
```

### Opción 4: Deshabilitar HTTPS en frontend (Solo desarrollo)
```typescript
// En environment.prod.ts
export const environment = {
  production: false,  // ← Cambiar a false
  apiUrl: 'http://localhost:5000/api',
  version: '20251017-local',
  enableLogging: true
};
```

## Recomendación Inmediata
1. **Para prueba rápida**: Usar ngrok
2. **Para solución permanente**: Azure App Service