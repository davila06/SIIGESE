# Guía de Deployment - Nueva Base de Datos Azure (Sin Datos de Prueba)

## 📋 Resumen

Esta guía te ayudará a crear una nueva base de datos limpia en Azure SQL Database y configurar tu aplicación para que apunte a ella. La base de datos NO contendrá datos de prueba, solo la estructura de tablas necesaria.

## 🚀 Pasos a Seguir

### Paso 1: Crear la Base de Datos en Azure

Ejecuta el siguiente comando en PowerShell:

```powershell
.\create-new-azure-database.ps1
```

**¿Qué hace este script?**
- ✅ Crea un nuevo servidor SQL en Azure con un nombre único
- ✅ Configura las reglas de firewall necesarias
- ✅ Crea la base de datos con tier Basic (2GB)
- ✅ Ejecuta los scripts de estructura (tablas, índices, foreign keys)
- ✅ NO ejecuta scripts de datos de prueba (05_InsertInitialData.sql, insert_test_cobros*.sql, etc.)
- ✅ Guarda la configuración en `new-database-config.json`

**Nota:** Si `sqlcmd` no está instalado en tu máquina, el script te indicará que ejecutes manualmente los scripts SQL usando Azure Data Studio o SQL Server Management Studio.

### Paso 2: Actualizar la Configuración del Backend

Una vez creada la base de datos, ejecuta:

```powershell
.\update-backend-connection.ps1
```

**¿Qué hace este script?**
- ✅ Lee la configuración de la nueva base de datos
- ✅ Actualiza `backend/src/WebApi/appsettings.Production.json` con la nueva cadena de conexión
- ✅ Actualiza `azure-deployment-config.json` con los nuevos detalles

### Paso 3: Reconstruir y Redesplegar el Backend

Finalmente, despliega el backend con la nueva configuración:

```powershell
.\rebuild-and-deploy-backend.ps1
```

**¿Qué hace este script?**
- ✅ Construye una nueva imagen Docker del backend
- ✅ Crea/usa Azure Container Registry (ACR)
- ✅ Sube la imagen a ACR
- ✅ Elimina el contenedor antiguo
- ✅ Crea un nuevo contenedor con la configuración actualizada
- ✅ Actualiza las URLs del backend en la configuración

## 📝 Scripts SQL que se Ejecutan (SIN DATOS DE PRUEBA)

El proceso ejecuta **solo** los siguientes scripts de estructura:

1. ✅ `01_CreateDatabase.sql` - Configuración de la base de datos
2. ✅ `02_CreateTables.sql` - Creación de todas las tablas
3. ✅ `03_CreateIndexes.sql` - Índices para optimización
4. ✅ `04_CreateForeignKeys.sql` - Relaciones entre tablas
5. ✅ `06_CreateCobrosTable.sql` - Tabla de cobros

**NO se ejecutan:**
- ❌ `05_InsertInitialData.sql` - Datos de prueba
- ❌ `insert_test_cobros.sql` - Cobros de prueba
- ❌ `insert_test_cobros_v2.sql` - Más cobros de prueba
- ❌ `InsertTestData.sql` - Datos de testing

## 🔑 Crear Usuario Administrador Inicial

Después del deployment, necesitarás crear al menos un usuario administrador. Puedes usar uno de estos métodos:

### Opción A: Usando SQL Script

Conéctate a la base de datos y ejecuta:

```sql
-- Ver el contenido de insert_admin.sql y ejecutarlo manualmente
```

### Opción B: Usando la API (después de que esté desplegada)

```powershell
# Registrar primer usuario admin desde la API
$apiUrl = "http://tu-backend-url.azurecontainer.io/api"
$body = @{
    email = "admin@siinadseg.com"
    password = "Admin123!"
    nombre = "Administrador"
    rol = "Admin"
} | ConvertTo-Json

Invoke-RestMethod -Uri "$apiUrl/auth/register" -Method POST -Body $body -ContentType "application/json"
```

## 📊 Verificación del Deployment

### 1. Verificar la Base de Datos

```powershell
# Conectarse con Azure Data Studio o SSMS
# Server: [tu-servidor].database.windows.net
# Database: SiinadsegProdDB
# Username: sqladmin
# Password: [guardado en new-database-config.json]

# Verificar tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'
```

### 2. Verificar el Backend

```powershell
# Obtener la URL del backend desde azure-deployment-config.json
$config = Get-Content azure-deployment-config.json | ConvertFrom-Json
$backendUrl = $config.azure.resources.backendContainer

# Test de health
Invoke-RestMethod -Uri "$backendUrl/api/health"
```

### 3. Verificar Logs del Contenedor

```powershell
az container logs --name siinadseg-backend --resource-group siinadseg-rg
```

## 🔧 Configuración Generada

Después de ejecutar todos los scripts, tendrás:

**Archivos actualizados:**
- ✅ `new-database-config.json` - Detalles de la nueva BD
- ✅ `backend/src/WebApi/appsettings.Production.json` - Connection string actualizada
- ✅ `azure-deployment-config.json` - URLs y configuración de Azure

**Recursos de Azure creados:**
- ✅ SQL Server: `siinadseg-sql-prod-XXXX.database.windows.net`
- ✅ Database: `SiinadsegProdDB`
- ✅ Container Registry: `siinadsegacrXXX`
- ✅ Container Instance: `siinadseg-backend` (actualizado)

## ⚠️ Notas Importantes

1. **Respaldo de configuración anterior**: Los scripts no eliminan la configuración anterior. Si necesitas volver atrás, puedes editar manualmente los archivos de configuración.

2. **Credenciales**: Las credenciales se guardan en `new-database-config.json`. Asegúrate de no commitear este archivo a git (debería estar en `.gitignore`).

3. **Costos**: La base de datos Basic tier tiene un costo mensual. Revisa la [página de precios de Azure SQL Database](https://azure.microsoft.com/pricing/details/sql-database/).

4. **Firewall**: El script configura el firewall para permitir tu IP actual. Si tu IP cambia, necesitarás actualizar las reglas de firewall.

## 🆘 Solución de Problemas

### Error: "sqlcmd no está instalado"

Instala SQL Server Command Line Tools:
- Windows: https://docs.microsoft.com/sql/tools/sqlcmd-utility
- O usa Azure Data Studio: https://azure.microsoft.com/products/data-studio/

### Error: "Docker no está corriendo"

Inicia Docker Desktop antes de ejecutar el script de rebuild.

### Error de autenticación en Azure

```powershell
az login
az account set --subscription "tu-subscription-id"
```

### El backend no puede conectarse a la BD

Verifica las reglas de firewall:

```powershell
$config = Get-Content new-database-config.json | ConvertFrom-Json
$serverName = $config.server -replace '.database.windows.net', ''

az sql server firewall-rule create `
    --resource-group siinadseg-rg `
    --server $serverName `
    --name AllowAll `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 255.255.255.255
```

**Nota:** La regla anterior permite TODO el tráfico. Úsala solo temporalmente para debugging.

## 📞 Contacto

Si tienes problemas durante el deployment, revisa los logs del contenedor o contacta al equipo de desarrollo.
