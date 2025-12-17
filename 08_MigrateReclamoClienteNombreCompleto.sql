-- =====================================================
-- SCRIPT PARA MIGRACIÓN DE CAMPOS DE CLIENTE EN TABLA RECLAMOS
-- =====================================================
-- Fecha: 2025-12-16
-- Descripción: Unifica ClienteNombre y ClienteApellido en ClienteNombreCompleto
--              para compatibilidad con campo NombreAsegurado de Polizas
-- =====================================================

USE [SiinadsegProdDB];
GO

PRINT '🔄 Iniciando migración de campos de cliente en Reclamos...';

-- =====================================================
-- 1. AGREGAR NUEVA COLUMNA ClienteNombreCompleto
-- =====================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.columns 
    WHERE object_id = OBJECT_ID('Reclamos') 
    AND name = 'ClienteNombreCompleto'
)
BEGIN
    PRINT '➕ Agregando columna ClienteNombreCompleto...';
    
    ALTER TABLE [Reclamos]
    ADD [ClienteNombreCompleto] nvarchar(200) NULL;
    
    PRINT '✅ Columna ClienteNombreCompleto agregada';
END
ELSE
BEGIN
    PRINT '⚠️  Columna ClienteNombreCompleto ya existe';
END
GO

-- =====================================================
-- 2. MIGRAR DATOS EXISTENTES
-- =====================================================
PRINT '🔄 Migrando datos existentes...';

-- Combinar ClienteNombre y ClienteApellido en ClienteNombreCompleto
UPDATE [Reclamos]
SET [ClienteNombreCompleto] = 
    CASE 
        WHEN [ClienteApellido] IS NOT NULL AND [ClienteApellido] <> ''
        THEN CONCAT([ClienteNombre], ' ', [ClienteApellido])
        ELSE [ClienteNombre]
    END
WHERE [ClienteNombreCompleto] IS NULL;

DECLARE @rowsUpdated INT = @@ROWCOUNT;
PRINT '✅ ' + CAST(@rowsUpdated AS VARCHAR(10)) + ' registros migrados';
GO

-- =====================================================
-- 3. VERIFICAR MIGRACIÓN
-- =====================================================
PRINT '🔍 Verificando migración...';

SELECT 
    COUNT(*) as TotalReclamos,
    COUNT([ClienteNombreCompleto]) as ConNombreCompleto,
    COUNT(CASE WHEN [ClienteNombreCompleto] IS NULL THEN 1 END) as SinNombreCompleto
FROM [Reclamos]
WHERE [IsDeleted] = 0;

-- Mostrar algunos ejemplos
PRINT '📋 Ejemplos de datos migrados (primeros 5):';
SELECT TOP 5
    [Id],
    [NumeroReclamo],
    [ClienteNombre] as NombreAntiguo,
    [ClienteApellido] as ApellidoAntiguo,
    [ClienteNombreCompleto] as NombreNuevo
FROM [Reclamos]
WHERE [IsDeleted] = 0
ORDER BY [Id];
GO

-- =====================================================
-- 4. HACER COLUMNA NO NULLABLE (después de verificar)
-- =====================================================
PRINT '🔒 Configurando columna como NOT NULL...';

-- Primero, establecer un valor por defecto para cualquier NULL restante
UPDATE [Reclamos]
SET [ClienteNombreCompleto] = 'Sin Nombre'
WHERE [ClienteNombreCompleto] IS NULL;

-- Modificar columna a NOT NULL
ALTER TABLE [Reclamos]
ALTER COLUMN [ClienteNombreCompleto] nvarchar(200) NOT NULL;

PRINT '✅ Columna ClienteNombreCompleto ahora es NOT NULL';
GO

-- =====================================================
-- 5. ELIMINAR COLUMNAS ANTIGUAS (OPCIONAL - COMENTADO)
-- =====================================================
-- ⚠️ DESCOMENTAR SOLO DESPUÉS DE VERIFICAR QUE TODO FUNCIONA CORRECTAMENTE
-- ⚠️ Y DESPUÉS DE ACTUALIZAR BACKEND Y FRONTEND

/*
PRINT '🗑️  Eliminando columnas ClienteNombre y ClienteApellido...';

-- Verificar que no hay NULL en ClienteNombreCompleto
IF NOT EXISTS (SELECT 1 FROM [Reclamos] WHERE [ClienteNombreCompleto] IS NULL)
BEGIN
    -- Eliminar índices que usen estas columnas (si existen)
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reclamos_ClienteNombre')
        DROP INDEX [IX_Reclamos_ClienteNombre] ON [Reclamos];
    
    IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Reclamos_ClienteApellido')
        DROP INDEX [IX_Reclamos_ClienteApellido] ON [Reclamos];
    
    -- Eliminar columnas
    ALTER TABLE [Reclamos] DROP COLUMN [ClienteNombre];
    ALTER TABLE [Reclamos] DROP COLUMN [ClienteApellido];
    
    PRINT '✅ Columnas antiguas eliminadas';
END
ELSE
BEGIN
    PRINT '❌ No se pueden eliminar columnas: hay valores NULL en ClienteNombreCompleto';
END
GO
*/

-- =====================================================
-- 6. CREAR ÍNDICE PARA BÚSQUEDAS
-- =====================================================
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes 
    WHERE name = 'IX_Reclamos_ClienteNombreCompleto' 
    AND object_id = OBJECT_ID('Reclamos')
)
BEGIN
    PRINT '📑 Creando índice para ClienteNombreCompleto...';
    
    CREATE NONCLUSTERED INDEX [IX_Reclamos_ClienteNombreCompleto]
    ON [Reclamos] ([ClienteNombreCompleto])
    INCLUDE ([NumeroReclamo], [NumeroPoliza], [Estado]);
    
    PRINT '✅ Índice creado';
END
ELSE
BEGIN
    PRINT '⚠️  Índice IX_Reclamos_ClienteNombreCompleto ya existe';
END
GO

-- =====================================================
-- 7. RESUMEN FINAL
-- =====================================================
PRINT '';
PRINT '========================================';
PRINT '✅ MIGRACIÓN COMPLETADA EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT '📊 Resumen:';
SELECT 
    'Total de Reclamos' as Metrica,
    COUNT(*) as Valor
FROM [Reclamos]
WHERE [IsDeleted] = 0
UNION ALL
SELECT 
    'Con Nombre Completo',
    COUNT(*)
FROM [Reclamos]
WHERE [IsDeleted] = 0 AND [ClienteNombreCompleto] IS NOT NULL
UNION ALL
SELECT 
    'Columnas Antiguas Eliminadas',
    CASE 
        WHEN EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Reclamos') AND name = 'ClienteNombre')
        THEN 0
        ELSE 1
    END;

PRINT '';
PRINT '⚠️  NOTA: Las columnas ClienteNombre y ClienteApellido AÚN EXISTEN';
PRINT '    Descomentar sección 5 del script cuando el backend y frontend estén actualizados';
PRINT '';
PRINT '🎯 Próximos pasos:';
PRINT '   1. Actualizar backend (entidades, DTOs, servicios) ✅';
PRINT '   2. Actualizar frontend (interfaces, componentes) ✅';
PRINT '   3. Hacer rebuild y desplegar';
PRINT '   4. Verificar funcionamiento en producción';
PRINT '   5. Descomentar sección 5 para eliminar columnas antiguas';
PRINT '';
