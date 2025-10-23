# 🗄️ CONFIGURACIÓN DE BASE DE DATOS LOCAL - SIIGESE

## 📊 Opciones de Base de Datos Local

### 🐳 Opción 1: Docker SQL Server (RECOMENDADO)

#### 🚀 Inicio Rápido con Docker
```bash
# Iniciar contenedor de SQL Server
docker-compose up db -d

# Verificar que esté funcionando
docker ps
```

#### 📋 Configuración Docker
- **Servidor**: `localhost,1433`
- **Usuario**: `sa`
- **Contraseña**: `DevPassword123!`
- **Base de Datos**: `MiAppDb` (se crea automáticamente)

#### 🔗 Connection String
```
Server=localhost,1433;Database=MiAppDb;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;
```

### 🖥️ Opción 2: SQL Server Express Local

#### 📋 Configuración SQL Express
- **Servidor**: `(local)\SQLEXPRESS`
- **Base de Datos**: `SinsegAppDb`
- **Autenticación**: Windows Authentication (Trusted_Connection=True)

#### 🔗 Connection String
```
Server=(local)\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;
```

#### 🛠️ Setup Manual
1. **Instalar SQL Server Express** desde Microsoft
2. **Ejecutar script de creación**:
   ```sql
   -- Ejecutar EJECUTAR_COMPLETO.sql en SSMS
   ```

### ☁️ Opción 3: Azure SQL (Existente)

#### 📋 Configuración Azure
- **Servidor**: `siinadseg-sqlserver-1019.database.windows.net`
- **Base de Datos**: `SiinadsegDB`
- **Usuario**: `siinadseg_admin`
- **Contraseña**: `P@ssw0rd123!`

## Estructura de la Base de Datos

### Tablas Principales:
- **Users**: Usuarios del sistema
- **Roles**: Roles de usuario (Admin, DataLoader, User)
- **UserRoles**: Relación usuarios-roles
- **Polizas**: Pólizas de seguros con campos específicos para Excel
- **Clientes**: Información de clientes
- **DataRecords**: Registro de cargas de archivos

### Campos de Pólizas (Correspondientes a columnas de Excel):
- **POLIZA** → NumeroPoliza
- **MOD** → Modalidad  
- **NOMBRE** → NombreAsegurado
- **PRIMA** → Prima
- **MONEDA** → Moneda
- **FECHA** → FechaVigencia
- **FRECUENCIA** → Frecuencia
- **ASEGURADORA** → Aseguradora
- **PLACA** → Placa
- **MARCA** → Marca
- **MODELO** → Modelo

## Archivos Disponibles

### Opción 1: Script Completo (Recomendado)
- **`EJECUTAR_COMPLETO.sql`** - Ejecuta todo en un solo script

### Opción 2: Scripts Separados
1. **`01_CreateDatabase.sql`** - Crear base de datos
2. **`02_CreateTables.sql`** - Crear todas las tablas
3. **`03_CreateIndexes.sql`** - Crear índices para optimización
4. **`04_CreateForeignKeys.sql`** - Crear relaciones entre tablas
5. **`05_InsertInitialData.sql`** - Insertar datos iniciales

## Instrucciones de Instalación

### Para SQL Server Express:

1. **Abrir SQL Server Management Studio (SSMS)**
2. **Conectarse a tu instancia de SQL Server Express**
   - Servidor: `(local)\SQLEXPRESS` o `localhost\SQLEXPRESS`
3. **Ejecutar el script completo:**
   - Abrir `EJECUTAR_COMPLETO.sql`
   - Ejecutar todo el script (F5)

### Credenciales Iniciales Creadas:
- **Email**: `admin@sinseg.com`
- **Password**: `password123`
- **Rol**: Administrador

## Cadena de Conexión para la Aplicación

Actualiza tu `appsettings.json` con:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local)\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;",
    "LocalDbConnection": "Server=(local)\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

## Verificación de la Instalación

Ejecuta estas consultas para verificar:

```sql
-- Verificar tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Verificar usuario administrador
SELECT * FROM Users WHERE Email = 'admin@sinseg.com';

-- Verificar roles
SELECT * FROM Roles;

-- Verificar asignación de roles
SELECT u.Email, r.Name 
FROM Users u 
INNER JOIN UserRoles ur ON u.Id = ur.UserId 
INNER JOIN Roles r ON ur.RoleId = r.Id;
```

## Notas Importantes

1. **Compatibilidad**: Scripts diseñados específicamente para SQL Server Express
2. **Seguridad**: El hash de contraseña usa BCrypt para máxima seguridad
3. **Índices**: Incluye índices optimizados para consultas frecuentes
4. **Integridad**: Foreign keys para mantener integridad referencial
5. **Migración**: Compatible con Entity Framework Core

## Solución de Problemas

### Si ya existe la base de datos:
Los scripts verifican la existencia antes de crear, es seguro ejecutarlos múltiples veces.

### Si hay errores de permisos:
Asegúrate de ejecutar SSMS como administrador y tener permisos para crear bases de datos.

### Para conectarse desde la aplicación:
Verifica que SQL Server Express esté ejecutándose y acepte conexiones TCP/IP.