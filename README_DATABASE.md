# ðŸ—„ï¸ CONFIGURACIÃ“N DE BASE DE DATOS LOCAL - OmnIA

## ðŸ“Š Opciones de Base de Datos Local

### ðŸ³ OpciÃ³n 1: Docker SQL Server (RECOMENDADO)

#### ðŸš€ Inicio RÃ¡pido con Docker
```bash
# Iniciar contenedor de SQL Server
docker-compose up db -d

# Verificar que estÃ© funcionando
docker ps
```

#### ðŸ“‹ ConfiguraciÃ³n Docker
- **Servidor**: `localhost,1433`
- **Usuario**: `sa`
- **ContraseÃ±a**: `DevPassword123!`
- **Base de Datos**: `MiAppDb` (se crea automÃ¡ticamente)

#### ðŸ”— Connection String
```
Server=localhost,1433;Database=MiAppDb;User Id=sa;Password=DevPassword123!;TrustServerCertificate=true;
```

### ðŸ–¥ï¸ OpciÃ³n 2: SQL Server Express Local

#### ðŸ“‹ ConfiguraciÃ³n SQL Express
- **Servidor**: `(local)\SQLEXPRESS`
- **Base de Datos**: `SinsegAppDb`
- **AutenticaciÃ³n**: Windows Authentication (Trusted_Connection=True)

#### ðŸ”— Connection String
```
Server=(local)\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;
```

#### ðŸ› ï¸ Setup Manual
1. **Instalar SQL Server Express** desde Microsoft
2. **Ejecutar script de creaciÃ³n**:
   ```sql
   -- Ejecutar EJECUTAR_COMPLETO.sql en SSMS
   ```

### â˜ï¸ OpciÃ³n 3: Azure SQL (Existente)

#### ðŸ“‹ ConfiguraciÃ³n Azure
- **Servidor**: `siinadseg-sqlserver-1019.database.windows.net`
- **Base de Datos**: `SiinadsegDB`
- **Usuario**: `siinadseg_admin`
- **ContraseÃ±a**: `P@ssw0rd123!`

## Estructura de la Base de Datos

### Tablas Principales:
- **Users**: Usuarios del sistema
- **Roles**: Roles de usuario (Admin, DataLoader, User)
- **UserRoles**: RelaciÃ³n usuarios-roles
- **Polizas**: PÃ³lizas de seguros con campos especÃ­ficos para Excel
- **Clientes**: InformaciÃ³n de clientes
- **DataRecords**: Registro de cargas de archivos

### Campos de PÃ³lizas (Correspondientes a columnas de Excel):
- **POLIZA** â†’ NumeroPoliza
- **MOD** â†’ Modalidad  
- **NOMBRE** â†’ NombreAsegurado
- **PRIMA** â†’ Prima
- **MONEDA** â†’ Moneda
- **FECHA** â†’ FechaVigencia
- **FRECUENCIA** â†’ Frecuencia
- **ASEGURADORA** â†’ Aseguradora
- **PLACA** â†’ Placa
- **MARCA** â†’ Marca
- **MODELO** â†’ Modelo

## Archivos Disponibles

### OpciÃ³n 1: Script Completo (Recomendado)
- **`EJECUTAR_COMPLETO.sql`** - Ejecuta todo en un solo script

### OpciÃ³n 2: Scripts Separados
1. **`01_CreateDatabase.sql`** - Crear base de datos
2. **`02_CreateTables.sql`** - Crear todas las tablas
3. **`03_CreateIndexes.sql`** - Crear Ã­ndices para optimizaciÃ³n
4. **`04_CreateForeignKeys.sql`** - Crear relaciones entre tablas
5. **`05_InsertInitialData.sql`** - Insertar datos iniciales

## Instrucciones de InstalaciÃ³n

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

## Cadena de ConexiÃ³n para la AplicaciÃ³n

Actualiza tu `appsettings.json` con:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(local)\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;",
    "LocalDbConnection": "Server=(local)\\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;MultipleActiveResultSets=True;TrustServerCertificate=True;"
  }
}
```

## VerificaciÃ³n de la InstalaciÃ³n

Ejecuta estas consultas para verificar:

```sql
-- Verificar tablas creadas
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE';

-- Verificar usuario administrador
SELECT * FROM Users WHERE Email = 'admin@sinseg.com';

-- Verificar roles
SELECT * FROM Roles;

-- Verificar asignaciÃ³n de roles
SELECT u.Email, r.Name 
FROM Users u 
INNER JOIN UserRoles ur ON u.Id = ur.UserId 
INNER JOIN Roles r ON ur.RoleId = r.Id;
```

## Notas Importantes

1. **Compatibilidad**: Scripts diseÃ±ados especÃ­ficamente para SQL Server Express
2. **Seguridad**: El hash de contraseÃ±a usa BCrypt para mÃ¡xima seguridad
3. **Ãndices**: Incluye Ã­ndices optimizados para consultas frecuentes
4. **Integridad**: Foreign keys para mantener integridad referencial
5. **MigraciÃ³n**: Compatible con Entity Framework Core

## SoluciÃ³n de Problemas

### Si ya existe la base de datos:
Los scripts verifican la existencia antes de crear, es seguro ejecutarlos mÃºltiples veces.

### Si hay errores de permisos:
AsegÃºrate de ejecutar SSMS como administrador y tener permisos para crear bases de datos.

### Para conectarse desde la aplicaciÃ³n:
Verifica que SQL Server Express estÃ© ejecutÃ¡ndose y acepte conexiones TCP/IP.
