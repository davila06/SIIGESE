# 🔄 CONFIGURACIÓN AUTOMÁTICA - BASE DE DATOS AZURE

## 📊 RESPUESTA DIRECTA A TU PREGUNTA:

### ❌ NO automáticamente
Ejecutar solo el script SQL **NO** conecta automáticamente tu aplicación. El script solo crea las tablas en Azure.

### ✅ PERO... Ya está casi todo listo!

Tu aplicación **YA** tiene la configuración de Azure, solo necesitamos:
1. ✅ Ejecutar el script SQL en Azure (crear tablas)
2. ✅ Verificar que los appsettings.json apunten a Azure (✅ YA ACTUALIZADO)
3. ✅ Reiniciar la aplicación

---

## 🔍 SITUACIÓN ACTUAL

### Tu Configuración Azure:
```
Servidor Azure:  siinadseg-sqlserver-1019.database.windows.net
Base de Datos:   SiinadsegDB
Usuario:         siinadmin
Password:        Siinadseg2025#@
```

### Estado de Configuración:
- ✅ **appsettings.json** - ACTUALIZADO a Azure correcta
- ✅ **appsettings.Production.json** - ACTUALIZADO a Azure correcta
- ⚠️ **Tablas en Azure** - PENDIENTE de crear
- ✅ **Frontend** - Ya apunta a Azure
- ✅ **Backend Docker** - Ya en Azure Container

---

## 🎯 QUÉ HACER AHORA (2 pasos)

### Paso 1: Crear Tablas en Azure
```powershell
.\setup-azure-database-excel.ps1
```

Este script:
- 🔌 Se conecta a tu Azure SQL existente
- 📊 Crea todas las tablas (8 tablas)
- 📝 Inserta datos de prueba
- ✅ Verifica la instalación

### Paso 2: Reiniciar tu Aplicación
```powershell
# Si está en Azure Container (recomendado)
az container restart --name siinadseg-backend-1019 --resource-group siinadseg-rg

# O si ejecutas localmente
cd backend\src\WebApi
dotnet run
```

---

## ✅ DESPUÉS DEL SCRIPT

Una vez ejecutes el script SQL, tu aplicación:

1. ✅ Se conectará automáticamente a Azure SQL
2. ✅ Verá todas las tablas creadas
3. ✅ Podrá insertar/leer pólizas
4. ✅ Podrá subir archivos Excel
5. ✅ Estará lista para producción

---

## 🔧 LO QUE YA SE ACTUALIZÓ AUTOMÁTICAMENTE

### Archivo: backend\src\WebApi\appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Initial Catalog=SiinadsegDB;User ID=siinadmin;Password=Siinadseg2025#@;Encrypt=True;"
  }
}
```

### Archivo: backend\src\WebApi\appsettings.Production.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Initial Catalog=SiinadsegDB;User ID=siinadmin;Password=Siinadseg2025#@;Encrypt=True;"
  }
}
```

✅ **Ambos archivos YA están configurados para Azure!**

---

## 📝 FLUJO COMPLETO

```
┌─────────────────────────────┐
│ 1. Script SQL              │
│    - Crea tablas en Azure   │
│    - Inserta datos prueba   │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│ 2. appsettings.json         │
│    - YA tiene Azure config  │ ✅ LISTO
│    - Connection string OK   │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│ 3. Aplicación inicia        │
│    - Lee appsettings.json   │
│    - Conecta a Azure SQL    │
│    - Usa las tablas nuevas  │
└─────────────┬───────────────┘
              │
              ▼
┌─────────────────────────────┐
│ 4. TODO FUNCIONA! 🎉        │
│    - Login con admin        │
│    - Subir Excel funciona   │
│    - Datos en Azure         │
└─────────────────────────────┘
```

---

## 🚀 COMANDO ÚNICO (Todo en Uno)

Ejecuta esto para hacer todo automáticamente:

```powershell
# 1. Crear tablas en Azure
.\setup-azure-database-excel.ps1

# 2. Si usas Azure Container (producción)
az container restart --name siinadseg-backend-1019 --resource-group siinadseg-rg

# 3. Verificar conexión
Start-Process "https://agreeable-water-06170cf10.1.azurestaticapps.net"
```

---

## 🔍 VERIFICAR QUE TODO FUNCIONA

### 1. Verificar Tablas en Azure:
```powershell
sqlcmd -S siinadseg-sqlserver-1019.database.windows.net -d SiinadsegDB -U siinadmin -P "Siinadseg2025#@" -Q "SELECT name FROM sys.tables"
```

### 2. Verificar Aplicación:
1. Ve a: https://agreeable-water-06170cf10.1.azurestaticapps.net
2. Login: `admin` / `Admin123!`
3. Ve a "Pólizas"
4. Deberías ver las 5 pólizas de prueba

### 3. Probar Excel:
1. Sube: `polizas_ejemplo_real.csv`
2. Deberían cargarse 20 pólizas
3. Verifica en la lista de pólizas

---

## ⚠️ IMPORTANTE: FIREWALL DE AZURE

Si obtienes error de conexión al ejecutar el script:

```
Error: Cannot open server 'siinadseg-sqlserver-1019' requested by the login
```

**Solución:**
1. Ve a [Azure Portal](https://portal.azure.com)
2. Busca: `siinadseg-sqlserver-1019`
3. Menú izquierdo: **Networking** o **Security > Networking**
4. En **Firewall rules**:
   - Click: **Add your client IPv4 address**
   - O habilita: **Allow Azure services and resources to access this server**
5. Click: **Save**
6. Espera 1 minuto
7. Ejecuta el script nuevamente

---

## 💡 DIFERENCIA LOCAL VS AZURE

### Desarrollo Local (Antes):
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=SinsegAppDb;Trusted_Connection=True;"
```

### Producción Azure (Ahora):
```json
"DefaultConnection": "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;User ID=siinadmin;Password=Siinadseg2025#@;"
```

✅ **Tu aplicación ahora usa Azure por defecto!**

---

## 📊 RESUMEN

| Componente | Estado | Acción Requerida |
|------------|--------|------------------|
| Azure SQL Server | ✅ Existe | Ninguna |
| Connection String | ✅ Configurado | Ninguna |
| Tablas en Azure | ⚠️ Faltan | **Ejecutar script** |
| Frontend Azure | ✅ Activo | Ninguna |
| Backend Azure | ✅ Activo | Restart después del script |
| Datos de Prueba | ⚠️ Faltan | **Script los crea** |

---

## 🎯 CONCLUSIÓN

**Respuesta directa:**

1. ❌ Solo ejecutar el script SQL **NO** conecta automáticamente
2. ✅ **PERO** tu aplicación **YA ESTÁ CONFIGURADA** para Azure
3. ✅ **Solo falta** ejecutar el script para crear las tablas
4. ✅ Después de ejecutar el script, **TODO funcionará automáticamente**

**Pasos finales:**
```powershell
# Ejecuta esto:
.\setup-azure-database-excel.ps1

# Y listo! Tu app ya se conectará a Azure DB
```

---

**🎉 Después de esto, tu aplicación estará 100% en Azure con las tablas listas para Excel!**
