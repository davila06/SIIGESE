# 📊 BASE DE DATOS PARA CARGA DE ARCHIVOS EXCEL - SINSEG

## 🎯 ¿Qué es esto?

Sistema completo de base de datos configurado para **recibir archivos Excel reales** con información de pólizas de seguros. Incluye:

- ✅ Base de datos completa con 8 tablas
- ✅ Soporte para 14 columnas de Excel
- ✅ Datos de prueba incluidos
- ✅ 20 pólizas de ejemplo listas para importar
- ✅ Usuario admin preconfigurado
- ✅ Scripts automáticos de instalación

---

## ⚡ INICIO RÁPIDO (3 pasos)

### 1️⃣ Ejecutar Script de Instalación

**OPCIÓN A: AZURE SQL DATABASE (☁️ Recomendado - Ya tienes Azure configurado)**
```powershell
.\setup-azure-database-excel.ps1
```

**OPCIÓN B: SQL SERVER LOCAL (💻 Desarrollo Local)**
```powershell
.\setup-database-excel.ps1
```

**OPCIÓN MANUAL - SQL Server Management Studio:**
1. Abre SQL Server Management Studio
2. Conecta a Azure: `siinadseg-sqlserver-1019.database.windows.net`
3. Abre el archivo: `SETUP_DATABASE_EXCEL.sql`
4. Presiona F5 para ejecutar

> 💡 **Nota:** Tienes infraestructura Azure configurada, por lo que se recomienda usar la Opción A.  
> Ver detalles en: [SETUP_LOCAL_VS_AZURE.md](SETUP_LOCAL_VS_AZURE.md)

### 2️⃣ Verificar Instalación

Deberías ver:
```
✓ Base de datos configurada (Azure: SiinadsegDB o Local: SinsegAppDb)
✓ Tablas creadas: 8
✓ Usuario admin creado
✓ 5 pólizas de prueba insertadas
```

**Tu configuración Azure actual:**
- Servidor: `siinadseg-sqlserver-1019.database.windows.net`
- Base Datos: `SiinadsegDB`
- Usuario: `siinadmin`

### 3️⃣ Probar con Archivo Real

Sube el archivo: **`polizas_ejemplo_real.csv`**
- 20 pólizas reales
- Formato correcto de 14 columnas
- Listo para importar

---

## 📁 ARCHIVOS INCLUIDOS

| Archivo | Descripción |
|---------|-------------|
| **SETUP_DATABASE_EXCEL.sql** | Script completo de creación de BD |
| **setup-database-excel.ps1** | Script PowerShell automático |
| **polizas_ejemplo_real.csv** | 20 pólizas de ejemplo |
| **GUIA_SETUP_DATABASE_EXCEL.md** | Guía completa detallada |
| **README_DATABASE_EXCEL.md** | Este archivo |

---

## 📋 FORMATO EXCEL REQUERIDO

El archivo Excel/CSV debe tener **14 columnas** en este orden:

### 🔴 OBLIGATORIAS (1-8):
1. **POLIZA** - Número único (ej: POL-2024-001)
2. **NOMBRE** - Nombre completo del asegurado
3. **NUMEROCEDULA** - Cédula (formato: 1-1234-5678)
4. **PRIMA** - Monto (número decimal)
5. **MONEDA** - CRC, USD o EUR
6. **FECHA** - Fecha vigencia (dd/MM/yyyy)
7. **FRECUENCIA** - MENSUAL, TRIMESTRAL, ANUAL, etc.
8. **ASEGURADORA** - Nombre de la aseguradora

### 🟡 OPCIONALES (9-14):
9. **PLACA** - Placa vehículo
10. **MARCA** - Marca vehículo
11. **MODELO** - Modelo vehículo
12. **AÑO** - Año vehículo
13. **CORREO** - Email
14. **NUMEROTELEFONO** - Teléfono

---

## 🔑 ACCESO ADMIN

Después de la instalación:

```
Usuario: admin
Email: admin@sinseg.com
Password: Admin123!
```

⚠️ **IMPORTANTE:** Cambia esta contraseña en producción

---

## 🗄️ ESTRUCTURA DE LA BASE DE DATOS

```
SinsegAppDb/
├── Roles (Admin, Agente, Usuario)
├── Users (Usuarios del sistema)
├── Perfiles (Perfiles de clientes)
├── Clientes (Información de clientes)
├── Polizas ⭐ (14 campos de Excel)
├── Cobros (Cobros de pólizas)
├── Reclamos (Reclamos de pólizas)
└── __EFMigrationsHistory (Control EF)
```

---

## ✅ EJEMPLO DE FILA EXCEL

```csv
POLIZA,NOMBRE,NUMEROCEDULA,PRIMA,MONEDA,FECHA,FRECUENCIA,ASEGURADORA,PLACA,MARCA,MODELO,AÑO,CORREO,NUMEROTELEFONO
POL-2024-101,Juan Pérez,1-1234-5678,150000,CRC,31/12/2024,MENSUAL,INS,ABC123,Toyota,Corolla,2020,juan@email.com,+506 8888-1234
```

---

## 🚀 CÓMO USARLO

### Paso 1: Instalar Base de Datos
```powershell
# Ejecutar en PowerShell
.\setup-database-excel.ps1
```

### Paso 2: Iniciar Aplicación Web
```powershell
# Backend
cd Application\WebApi
dotnet run

# Frontend
cd client
npm start
```

### Paso 3: Subir Archivo Excel
1. Inicia sesión como `admin`
2. Ve a "Pólizas" > "Cargar"
3. Selecciona `polizas_ejemplo_real.csv`
4. Click "Subir"
5. ¡Listo! 20 pólizas importadas

---

## 🔍 VERIFICAR INSTALACIÓN

```sql
USE SinsegAppDb;

-- Ver resumen de datos
SELECT 'Roles' AS Tabla, COUNT(*) AS Total FROM Roles
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM Users
UNION ALL
SELECT 'Pólizas', COUNT(*) FROM Polizas;

-- Ver pólizas de ejemplo
SELECT TOP 5
    NumeroPoliza,
    NombreAsegurado,
    Prima,
    Moneda,
    Aseguradora
FROM Polizas;
```

---

## 🛠️ SOLUCIÓN RÁPIDA DE PROBLEMAS

### ❌ Error: "No se puede conectar"
**Solución:**
```powershell
# Verificar que SQL Server esté corriendo
Get-Service -Name '*SQL*'

# Iniciar SQL Server
Start-Service 'MSSQL$SQLEXPRESS'
```

### ❌ Error: "Columna no encontrada"
**Solución:** Verifica que el Excel tenga exactamente 14 columnas con los nombres correctos (en MAYÚSCULAS)

### ❌ Error: "Moneda inválida"
**Solución:** Solo se aceptan: CRC, USD, EUR

---

## 📊 DATOS DE PRUEBA INCLUIDOS

Al instalar automáticamente obtienes:

- ✅ 3 Roles (Admin, Agente, Usuario)
- ✅ 1 Usuario Admin configurado
- ✅ 2 Perfiles de cliente
- ✅ 5 Pólizas de ejemplo en la BD
- ✅ 20 Pólizas en el archivo CSV de ejemplo

---

## 💡 CARACTERÍSTICAS

### Validaciones Automáticas:
- ✓ Número de póliza único
- ✓ Moneda válida (CRC/USD/EUR)
- ✓ Prima ≥ 0
- ✓ Formato de fecha flexible
- ✓ Cédula en formato correcto

### Optimizaciones:
- ⚡ Índices en campos clave
- 🔍 Búsquedas rápidas por número, cédula
- 🗑️ Soft delete (no borrado físico)
- 📅 Timestamps automáticos

---

## 📚 DOCUMENTACIÓN ADICIONAL

- **Guía Completa:** [GUIA_SETUP_DATABASE_EXCEL.md](GUIA_SETUP_DATABASE_EXCEL.md)
- **Formato Excel:** [formato_excel_polizas.txt](formato_excel_polizas.txt)
- **Archivo Ejemplo:** [polizas_ejemplo_real.csv](polizas_ejemplo_real.csv)

---

## 🎯 CASOS DE USO

### 1. Importar Pólizas de Auto
```csv
POL-AUTO-001,Juan Pérez,1-1234-5678,150000,CRC,31/12/2024,MENSUAL,INS,ABC123,Toyota,Corolla,2020,juan@mail.com,+506 8888-1234
```

### 2. Importar Pólizas de Vida (sin vehículo)
```csv
POL-VIDA-001,María García,2-2345-6789,250000,USD,15/01/2025,ANUAL,Seguros Vida,,,,,maria@mail.com,+506 7777-5678
```

### 3. Importar Pólizas Básicas (solo obligatorios)
```csv
POL-2024-003,Carlos López,3-3456-7890,180000,CRC,01/06/2024,TRIMESTRAL,MNK Seguros,,,,,
```

---

## 🎉 ¡LISTO!

Tu base de datos está configurada y lista para:

- ✅ Recibir archivos Excel reales
- ✅ Procesar 14 columnas de datos
- ✅ Validar automáticamente la información
- ✅ Almacenar pólizas de seguros
- ✅ Gestionar clientes y cobros

---

**🚀 Ahora puedes subir tus archivos Excel reales al sistema!**

Para más detalles, consulta: [GUIA_SETUP_DATABASE_EXCEL.md](GUIA_SETUP_DATABASE_EXCEL.md)
