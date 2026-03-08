# GUÍA: CONFIGURACIÓN DE BASE DE DATOS PARA CARGA DE ARCHIVOS EXCEL

## 📋 DESCRIPCIÓN

Esta guía te ayudará a configurar la base de datos SINSEG para soportar la carga de archivos Excel con 14 columnas de información de pólizas de seguros.

---

## 🚀 PASOS DE INSTALACIÓN

### 1. Ejecutar Script de Base de Datos

#### Opción A: Usando SQL Server Management Studio (SSMS)
1. Abre **SQL Server Management Studio**
2. Conéctate a tu instancia de SQL Server
3. Haz clic en **File > Open > File...**
4. Selecciona el archivo: `SETUP_DATABASE_EXCEL.sql`
5. Haz clic en el botón **Execute** (F5)
6. Verifica que todos los mensajes digan "✓" (exitoso)

#### Opción B: Usando PowerShell
```powershell
# Ejecutar desde PowerShell
cd C:\Users\davil\SINSEG\enterprise-web-app

# Con autenticación de Windows
sqlcmd -S localhost\SQLEXPRESS -i SETUP_DATABASE_EXCEL.sql

# Con autenticación SQL Server
sqlcmd -S localhost\SQLEXPRESS -U sa -P TuPassword -i SETUP_DATABASE_EXCEL.sql
```

#### Opción C: Usando Azure Data Studio
1. Abre **Azure Data Studio**
2. Conéctate a tu servidor
3. Haz clic derecho en el servidor > **New Query**
4. Abre el archivo `SETUP_DATABASE_EXCEL.sql`
5. Haz clic en **Run** (F5)

---

## 📊 ESTRUCTURA DE LA BASE DE DATOS

### Tablas Creadas:
1. **Roles** - Roles de usuario (Admin, Agente, Usuario)
2. **Users** - Usuarios del sistema
3. **Perfiles** - Perfiles de clientes
4. **Clientes** - Información de clientes
5. **Polizas** - Pólizas de seguros (con 14 campos del Excel)
6. **Cobros** - Cobros asociados a pólizas
7. **Reclamos** - Reclamos de pólizas

---

## 📄 FORMATO DEL ARCHIVO EXCEL

El archivo Excel/CSV debe tener **exactamente** estas 14 columnas en este orden:

### Columnas Obligatorias (1-8):
1. **POLIZA** - Número único de póliza (ej: POL-2024-001)
2. **NOMBRE** - Nombre completo del asegurado
3. **NUMEROCEDULA** - Número de cédula (formato: 1-1234-5678)
4. **PRIMA** - Monto de la prima (número decimal)
5. **MONEDA** - Moneda: CRC, USD o EUR
6. **FECHA** - Fecha de vigencia (formatos: dd/MM/yyyy, MM/dd/yyyy, yyyy-MM-dd)
7. **FRECUENCIA** - Frecuencia de pago: MENSUAL, BIMENSUAL, TRIMESTRAL, SEMESTRAL, ANUAL
8. **ASEGURADORA** - Nombre de la compañía aseguradora

### Columnas Opcionales (9-14):
9. **PLACA** - Placa del vehículo (si aplica)
10. **MARCA** - Marca del vehículo (si aplica)
11. **MODELO** - Modelo del vehículo (si aplica)
12. **AÑO** - Año del vehículo (si aplica)
13. **CORREO** - Email del asegurado
14. **NUMEROTELEFONO** - Teléfono del asegurado (formato: +506 8888-1234)

### 📝 Ejemplo de Fila:
```
POL-2024-101,Juan Carlos Pérez García,1-1234-5678,150000,CRC,31/12/2024,MENSUAL,INS,ABC123,Toyota,Corolla,2020,juan.perez@email.com,+506 8888-1234
```

---

## ✅ DATOS DE PRUEBA INCLUIDOS

El script crea automáticamente:

- **3 Roles**: Admin, Agente, Usuario
- **1 Usuario Admin**:
  - Usuario: `admin`
  - Email: `admin@sinseg.com`
  - Password: `Admin123!`
- **2 Perfiles**: Perfil General, Perfil Corporativo
- **5 Pólizas de Ejemplo**: Con datos completos

---

## 📁 ARCHIVOS DE EJEMPLO

### Archivo de Ejemplo Real
Usa el archivo `polizas_ejemplo_real.csv` para probar la carga:
- ✅ 20 pólizas con datos realistas
- ✅ Incluye pólizas de auto y vida
- ✅ Algunos registros sin datos opcionales (para probar validación)
- ✅ Formato listo para importar

---

## 🧪 PRUEBAS Y VALIDACIÓN

### 1. Verificar Tablas Creadas
```sql
USE SinsegAppDb;
GO

-- Ver todas las tablas
SELECT TABLE_NAME 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_NAME;
```

### 2. Verificar Datos Iniciales
```sql
-- Ver roles
SELECT * FROM Roles;

-- Ver usuario admin
SELECT * FROM Users;

-- Ver pólizas de prueba
SELECT 
    NumeroPoliza, 
    NombreAsegurado, 
    NumeroCedula, 
    Prima, 
    Moneda,
    Aseguradora
FROM Polizas;
```

### 3. Verificar Estructura de Polizas
```sql
-- Ver columnas de la tabla Polizas
SELECT 
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Polizas'
ORDER BY ORDINAL_POSITION;
```

---

## 🔒 INFORMACIÓN DE ACCESO

### Base de Datos
- **Nombre**: SinsegAppDb
- **Servidor**: localhost\SQLEXPRESS (ajusta según tu configuración)

### Usuario Admin del Sistema
- **Usuario**: admin
- **Email**: admin@sinseg.com
- **Password**: Admin123!
- **Rol**: Admin

---

## 📤 CÓMO USAR EL ARCHIVO DE EJEMPLO

### Método 1: Importar Desde la Aplicación Web
1. Inicia sesión en la aplicación con el usuario admin
2. Ve a la sección "Pólizas"
3. Haz clic en "Cargar Pólizas" o "Upload"
4. Selecciona el archivo `polizas_ejemplo_real.csv`
5. Haz clic en "Subir" o "Upload"
6. Verifica que las 20 pólizas se hayan importado correctamente

### Método 2: Importar Directamente a SQL Server
```sql
-- Si quieres importar el CSV directamente
BULK INSERT Polizas
FROM 'C:\Users\davil\SINSEG\enterprise-web-app\polizas_ejemplo_real.csv'
WITH (
    FIRSTROW = 2, -- Omitir encabezados
    FIELDTERMINATOR = ',',
    ROWTERMINATOR = '\n',
    FORMAT = 'CSV'
);
```

---

## 🛠️ SOLUCIÓN DE PROBLEMAS

### Error: "Base de datos ya existe"
**Solución**: El script detecta si existe y solo agrega columnas faltantes. Si quieres empezar de cero:
```sql
USE master;
DROP DATABASE SinsegAppDb;
-- Luego ejecuta SETUP_DATABASE_EXCEL.sql nuevamente
```

### Error: "Columna no reconocida en Excel"
**Solución**: Verifica que:
1. El archivo tenga exactamente 14 columnas
2. Los nombres de columnas sean EXACTOS (mayúsculas)
3. No haya columnas extras
4. La primera fila sea el encabezado

### Error: "Valor de moneda inválido"
**Solución**: Solo se aceptan: CRC, USD, EUR

### Error: "Fecha en formato incorrecto"
**Solución**: Formatos aceptados:
- dd/MM/yyyy (31/12/2024)
- MM/dd/yyyy (12/31/2024)
- yyyy-MM-dd (2024-12-31)

---

## 📊 VERIFICACIÓN POST-INSTALACIÓN

Ejecuta este query para verificar todo:

```sql
USE SinsegAppDb;
GO

PRINT '=== VERIFICACIÓN DE INSTALACIÓN ===';
PRINT '';
PRINT 'Roles: ' + CAST((SELECT COUNT(*) FROM Roles) AS VARCHAR);
PRINT 'Usuarios: ' + CAST((SELECT COUNT(*) FROM Users) AS VARCHAR);
PRINT 'Perfiles: ' + CAST((SELECT COUNT(*) FROM Perfiles) AS VARCHAR);
PRINT 'Pólizas: ' + CAST((SELECT COUNT(*) FROM Polizas) AS VARCHAR);
PRINT '';

-- Ver estructura de Polizas
SELECT 
    'Columnas en tabla Polizas' AS Info,
    COUNT(*) AS Total
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Polizas';

-- Ver ejemplo de datos
SELECT TOP 3 
    NumeroPoliza,
    NombreAsegurado,
    Prima,
    Moneda,
    Aseguradora
FROM Polizas;
```

---

## 📞 PRÓXIMOS PASOS

1. ✅ Ejecutar `SETUP_DATABASE_EXCEL.sql`
2. ✅ Verificar que todas las tablas se crearon
3. ✅ Iniciar sesión con usuario admin
4. ✅ Probar carga del archivo `polizas_ejemplo_real.csv`
5. ✅ Verificar que las 20 pólizas se cargaron correctamente
6. ✅ Crear tu propio archivo Excel con tus datos reales

---

## 💡 NOTAS IMPORTANTES

- ⚠️ **Backup**: Si tienes datos existentes, haz backup antes de ejecutar
- 🔐 **Seguridad**: Cambia la contraseña del usuario admin en producción
- 📝 **Validación**: El sistema valida automáticamente los datos del Excel
- 🚀 **Performance**: Los índices están optimizados para búsquedas rápidas
- 📊 **Campos Opcionales**: Los campos 9-14 pueden estar vacíos sin problema

---

## ✨ CARACTERÍSTICAS ADICIONALES

### Validaciones Automáticas:
- ✓ Número de póliza único
- ✓ Moneda válida (CRC, USD, EUR)
- ✓ Prima mayor o igual a 0
- ✓ Formato de fecha correcto
- ✓ Cédula en formato correcto

### Campos Calculados:
- CreatedAt: Fecha de creación automática
- UpdatedAt: Fecha de última modificación
- CreatedBy: Usuario que creó el registro
- IsDeleted: Soft delete (no se borra físicamente)

---

**¡Base de datos lista para recibir archivos Excel reales! 🎉**
