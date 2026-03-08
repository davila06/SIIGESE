# 🌐 CONFIGURACIÓN DE BASE DE DATOS - LOCAL vs AZURE

## 📊 OPCIONES DISPONIBLES

Tienes **2 opciones** para crear la base de datos:

### ☁️ **OPCIÓN 1: AZURE SQL DATABASE** (Recomendado)
✅ Base de datos en la nube  
✅ Accesible desde cualquier lugar  
✅ Ya tienes configuración Azure existente  
✅ Producción lista  

### 💻 **OPCIÓN 2: SQL SERVER LOCAL**
✅ Base de datos en tu computadora  
✅ Desarrollo y pruebas locales  
✅ Sin costos de Azure  
✅ Más rápido para desarrollo  

---

## ☁️ OPCIÓN 1: AZURE SQL DATABASE

### Tu Configuración Actual de Azure:
```
Servidor:   siinadseg-sqlserver-1019.database.windows.net
Base Datos: SiinadsegDB
Usuario:    siinadmin
Password:   Siinadseg2025#@
```

### Ejecutar Setup en Azure:

```powershell
# Paso 1: Ejecutar script de Azure
.\setup-azure-database-excel.ps1
```

El script:
1. ✅ Se conecta a tu Azure SQL Database
2. ✅ Crea todas las tablas con soporte Excel (14 columnas)
3. ✅ Inserta datos de prueba
4. ✅ Crea usuario admin
5. ✅ Verifica la instalación

### ⚠️ Importante - Configurar Firewall:

**Si obtienes error de conexión**, necesitas permitir tu IP en Azure:

1. Ve a [Azure Portal](https://portal.azure.com)
2. Busca: `siinadseg-sqlserver-1019`
3. Ve a: **Networking** o **Firewall and virtual networks**
4. Agrega tu IP actual o habilita: **Allow Azure services**
5. Guarda cambios

### Verificar en Azure:

```powershell
# Conectar a Azure SQL
sqlcmd -S siinadseg-sqlserver-1019.database.windows.net -d SiinadsegDB -U siinadmin -P "Siinadseg2025#@"

# Verificar tablas
SELECT name FROM sys.tables;
GO

# Verificar pólizas
SELECT COUNT(*) FROM Polizas;
GO
```

---

## 💻 OPCIÓN 2: SQL SERVER LOCAL

### Para Desarrollo Local:

```powershell
# Paso 1: Ejecutar script local
.\setup-database-excel.ps1
```

El script te preguntará:
- Servidor SQL (por defecto: `localhost\SQLEXPRESS`)
- Tipo de autenticación (Windows o SQL Server)

### Connection String Local:
```
Server=localhost\SQLEXPRESS;Database=SinsegAppDb;Integrated Security=True;
```

---

## 🔄 AMBAS OPCIONES

### Puedes tener AMBAS configuradas:

1. **Local** para desarrollo y pruebas
2. **Azure** para producción

### Cambiar entre Local y Azure:

**En appsettings.json:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=SinsegAppDb;Integrated Security=True;"
  }
}
```

**Para Azure:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=tcp:siinadseg-sqlserver-1019.database.windows.net,1433;Initial Catalog=SiinadsegDB;User ID=siinadmin;Password=Siinadseg2025#@;Encrypt=True;"
  }
}
```

---

## 🎯 RECOMENDACIÓN

### ✨ Para TI (Uso Actual):

Usa **AZURE SQL DATABASE** porque:
- ✅ Ya tienes infraestructura Azure configurada
- ✅ Tu frontend está en Azure Static Web Apps
- ✅ Tu backend está en Azure Container Instance
- ✅ Todo funciona junto en la nube
- ✅ Accesible desde cualquier lugar

### Ejecuta:
```powershell
.\setup-azure-database-excel.ps1
```

---

## 📋 COMPARACIÓN RÁPIDA

| Característica | Azure SQL | Local SQL |
|---------------|-----------|-----------|
| Ubicación | ☁️ Nube | 💻 PC Local |
| Costo | 💰 ~$5/mes | 🆓 Gratis |
| Acceso remoto | ✅ Sí | ❌ No |
| Velocidad | 🌐 Internet | ⚡ Muy rápida |
| Backup automático | ✅ Sí | ❌ Manual |
| Escalabilidad | ✅ Fácil | ⚠️ Limitada |
| Desarrollo | ⚠️ Más lento | ✅ Ideal |
| Producción | ✅ Ideal | ❌ No |

---

## 🚀 INICIO RÁPIDO

### Para Azure (Recomendado para tu caso):

```powershell
# 1. Ejecutar setup
.\setup-azure-database-excel.ps1

# 2. Si hay error de firewall, agrega tu IP en Azure Portal

# 3. Accede a tu aplicación
https://agreeable-water-06170cf10.1.azurestaticapps.net

# 4. Login
Usuario: admin
Password: Admin123!

# 5. Sube el archivo
polizas_ejemplo_real.csv
```

### Para Local (Desarrollo):

```powershell
# 1. Ejecutar setup
.\setup-database-excel.ps1

# 2. Selecciona: Windows Authentication

# 3. Inicia tu app local
cd Application\WebApi
dotnet run

# 4. Frontend local
cd client
npm start
```

---

## 🔍 VERIFICAR INSTALACIÓN

### En Azure:
```powershell
sqlcmd -S siinadseg-sqlserver-1019.database.windows.net -d SiinadsegDB -U siinadmin -P "Siinadseg2025#@" -Q "SELECT COUNT(*) FROM Polizas"
```

### Local:
```powershell
sqlcmd -S localhost\SQLEXPRESS -d SinsegAppDb -E -Q "SELECT COUNT(*) FROM Polizas"
```

---

## 📁 ARCHIVOS DISPONIBLES

| Archivo | Para Azure | Para Local |
|---------|-----------|------------|
| **setup-azure-database-excel.ps1** | ✅ Usar este | ❌ |
| **setup-database-excel.ps1** | ❌ | ✅ Usar este |
| **SETUP_DATABASE_EXCEL.sql** | ✅ | ✅ |
| **polizas_ejemplo_real.csv** | ✅ | ✅ |

---

## 💡 NOTAS IMPORTANTES

### Para Azure:
- ⚠️ Configura el firewall para permitir tu IP
- ⚠️ Verifica que tienes internet estable
- ⚠️ La primera conexión puede tardar unos segundos
- ✅ Los cambios son permanentes y globales

### Para Local:
- ⚠️ Requiere SQL Server instalado localmente
- ⚠️ Solo accesible desde tu PC
- ✅ Ideal para desarrollo sin conexión
- ✅ Más rápido para pruebas

---

## 🎉 RESULTADO

Después de ejecutar cualquiera de los scripts, tendrás:

- ✅ Base de datos con 8 tablas
- ✅ Soporte para Excel de 14 columnas
- ✅ Usuario admin configurado
- ✅ 5 pólizas de prueba
- ✅ Archivo con 20 pólizas ejemplo
- ✅ Sistema listo para subir archivos Excel reales

---

## 🆘 AYUDA RÁPIDA

### Error en Azure: "Cannot open server"
```
Solución: Configura el firewall en Azure Portal
Ve a: SQL Server > Networking > Add your client IP
```

### Error Local: "SQL Server no existe"
```
Solución: Instala SQL Server Express
https://www.microsoft.com/en-us/sql-server/sql-server-downloads
```

### ¿Qué opción elegir?
```
¿Tienes Azure configurado? → Usa Azure
¿Solo desarrollo local? → Usa Local
¿No estás seguro? → Usa Azure (es tu caso actual)
```

---

**🎯 Para tu caso específico, usa:**
```powershell
.\setup-azure-database-excel.ps1
```

**Porque ya tienes toda la infraestructura Azure lista! ☁️**
