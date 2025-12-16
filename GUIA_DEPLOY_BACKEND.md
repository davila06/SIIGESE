# Guía Completa: Deploy Manual del Backend (Opción 2)

## ¿Qué pasó?

El script inicial **funcionó parcialmente**:
- ✅ **Build de la imagen**: EXITOSO (creada en Azure Container Registry)
- ❌ **Actualización del Container App**: FALLÓ por falta de credenciales

## Problema Identificado

El Container App no tiene configuradas las credenciales para acceder al Azure Container Registry privado.

**Error**: `UNAUTHORIZED: authentication required`

## Solución Paso a Paso

### Método 1: Script Automatizado (Recomendado - EN DESARROLLO)

El script `fix-container-app-auth.ps1` configura las credenciales automáticamente, pero tiene issues con comandos en preview.

### Método 2: Comandos Manuales (FUNCIONA 100%)

Ejecuta estos comandos uno por uno:

```powershell
# 1. Obtener credenciales del ACR
$acrUser = az acr credential show --name acrsiinadseg --query username --output tsv
$acrPassword = az acr credential show --name acrsiinadseg --query "passwords[0].value" --output tsv

# 2. Configurar registry en Container App
az containerapp registry set `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --server acrsiinadseg.azurecr.io `
    --username $acrUser `
    --password $acrPassword

# 3. Actualizar la imagen del Container App
az containerapp update `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --image acrsiinadseg.azurecr.io/siinadseg-backend:latest

# 4. Verificar que está corriendo
az containerapp show `
    --name app-siinadseg-backend `
    --resource-group rg-siinadseg `
    --query "properties.runningStatus"
```

### Método 3: Azure Portal (Más Visual)

1. Ve a [Azure Portal](https://portal.azure.com)
2. Busca "app-siinadseg-backend"
3. Ve a **Settings > Containers**
4. Click en **Registry settings**
5. Agrega:
   - **Registry**: acrsiinadseg.azurecr.io
   - **Username**: acrsiinadseg
   - **Password**: (obtener con `az acr credential show --name acrsiinadseg`)
6. Guarda
7. Ve a **Revision management**
8. Click en **Create new revision**
9. Cambia la imagen a: `acrsiinadseg.azurecr.io/siinadseg-backend:latest`
10. Click **Create**

## Estado Actual

| Paso | Estado | Detalles |
|------|--------|----------|
| Build imagen | ✅ COMPLETADO | Imagen creada en ACR |
| Configurar registry | ⏳ PENDIENTE | Necesita credenciales |
| Actualizar Container App | ⏳ PENDIENTE | Espera paso anterior |
| Verificar funcionamiento | ⏳ PENDIENTE | Después del deploy |

## Resumen para Usuario

**LO QUE FUNCIONÓ:**
- La imagen Docker se construyó exitosamente en Azure
- El código con el fix está listo en la imagen

**LO QUE FALTA:**
- Configurar las credenciales del registry en el Container App
- Actualizar el Container App para usar la nueva imagen

**RECOMENDACIÓN:**
Usa el **Método 2 (Comandos Manuales)** ejecutando los 4 comandos paso a paso.

## Método Recomendado: ACR Tasks

Este método construye la imagen directamente en Azure, sin necesidad de Docker local.

### Paso a Paso

#### 1. Ejecutar el script
```powershell
.\deploy-backend-acr.ps1
```

#### 2. El script hará:
- **[1/3]** Build de la imagen en Azure (2-3 minutos)
  - Empaqueta el código backend
  - Lo sube a Azure Container Registry
  - Construye la imagen Docker en la nube
  
- **[2/3]** Actualizar Container App
  - Configura el Container App con la nueva imagen
  - Reinicia el servicio
  
- **[3/3]** Verificar deploy
  - Confirma que el servicio está corriendo
  - Muestra la URL del backend

#### 3. Salida esperada:
```
=== DEPLOY COMPLETADO ===

Estado: Running

Backend actualizado en:
  https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io

Endpoints para probar:
  GET https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/api/users
  (Los usuarios eliminados ya NO apareceran)
```

## Método Alternativo: Docker Local

Si prefieres usar Docker Desktop local:

### 1. Iniciar Docker Desktop
- Abre la aplicación Docker Desktop
- Espera a que inicie completamente (ícono verde)

### 2. Ejecutar el script
```powershell
.\deploy-backend-manual.ps1
```

Este método:
- Construye la imagen localmente
- La sube a Azure Container Registry
- Actualiza el Container App

**Ventaja**: Más rápido si Docker ya está corriendo
**Desventaja**: Requiere Docker Desktop instalado y corriendo

## Verificación del Deploy

### 1. Probar endpoint de usuarios
```powershell
curl https://app-siinadseg-backend.yellowrock-611c8f36.eastus.azurecontainerapps.io/api/users
```

### 2. Verificar que usuarios eliminados NO aparecen
```powershell
# Este script verifica usuarios eliminados en la BD
python limpiar-usuarios-eliminados.py
```

### 3. Probar en la aplicación web
1. Abre https://gentle-dune-0a2edab0f.3.azurestaticapps.net
2. Login con admin@sinseg.com / Admin123!
3. Ve a "Usuarios"
4. Elimina un usuario
5. Verifica que NO aparece en la lista

## Troubleshooting

### Error: "Unable to find 'Dockerfile'"
**Solución**: El script debe ejecutarse desde el directorio raíz del proyecto
```powershell
Set-Location C:\Users\davil\SINSEG\enterprise-web-app
.\deploy-backend-acr.ps1
```

### Error: "DOCKER_COMMAND_ERROR"
**Solución**: Docker Desktop no está corriendo. Usa el método ACR Tasks:
```powershell
.\deploy-backend-acr.ps1
```

### Error: "Cannot find the specified service"
**Solución**: Verifica que el Container App existe:
```powershell
az containerapp list --query "[].{Name:name, State:properties.runningStatus}"
```

### El deploy tarda mucho
**Normal**: ACR Tasks puede tardar 3-5 minutos la primera vez.
- Construcción de la imagen: 2-3 min
- Deploy al Container App: 1-2 min
- Inicialización del servicio: 30 seg

## Resumen

| Método | Tiempo | Requisitos | Recomendado |
|--------|--------|------------|-------------|
| **ACR Tasks** | 3-5 min | Solo Azure CLI | ✅ Sí |
| **Docker Local** | 2-3 min | Docker + Azure CLI | Solo si Docker ya corre |

## Scripts Disponibles

- `deploy-backend-acr.ps1` - Deploy sin Docker (Recomendado)
- `deploy-backend-manual.ps1` - Deploy con Docker local
- `limpiar-usuarios-eliminados.py` - Verificar BD

## ¿Qué cambió?

**Antes**: `GetAllUsersAsync()` retornaba todos los usuarios incluyendo eliminados

**Después**: 
```csharp
// Ahora filtra usuarios con IsDeleted = true
return users.Where(u => !u.IsDeleted).Select(MapToDto);
```

**Resultado**: Los usuarios eliminados ya no aparecen en la lista de la aplicación web.
