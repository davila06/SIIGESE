-- Script de migración para convertir registros GTQ a CRC
-- Este script debe ejecutarse para asegurar que no existan registros con monedas no soportadas
USE SINSEGDatabase;
GO

PRINT 'Iniciando migración de monedas...';

-- Verificar registros con GTQ antes de la migración
PRINT 'Verificando registros con GTQ antes de la migración:';

-- Revisar tabla Polizas
IF EXISTS (SELECT 1 FROM Polizas WHERE Moneda = 'GTQ' AND IsDeleted = 0)
BEGIN
    PRINT 'Registros en Polizas con GTQ encontrados:';
    SELECT Id, NumeroPoliza, NombreAsegurado, Prima, Moneda, CreatedAt 
    FROM Polizas 
    WHERE Moneda = 'GTQ' AND IsDeleted = 0;
END
ELSE
BEGIN
    PRINT 'No se encontraron registros en Polizas con GTQ.';
END

-- Revisar tabla Cobros
IF EXISTS (SELECT 1 FROM Cobros WHERE Moneda = 'GTQ' AND IsDeleted = 0)
BEGIN
    PRINT 'Registros en Cobros con GTQ encontrados:';
    SELECT Id, NumeroPoliza, NumeroRecibo, ClienteNombre, ClienteApellido, MontoTotal, Moneda, CreatedAt 
    FROM Cobros 
    WHERE Moneda = 'GTQ' AND IsDeleted = 0;
END
ELSE
BEGIN
    PRINT 'No se encontraron registros en Cobros con GTQ.';
END

-- Revisar tabla Reclamos si existe
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reclamos')
BEGIN
    IF EXISTS (SELECT 1 FROM Reclamos WHERE Moneda = 'GTQ' AND IsDeleted = 0)
    BEGIN
        PRINT 'Registros en Reclamos con GTQ encontrados:';
        SELECT Id, NumeroReclamo, NombreReclamante, MontoReclamado, Moneda, CreatedAt 
        FROM Reclamos 
        WHERE Moneda = 'GTQ' AND IsDeleted = 0;
    END
    ELSE
    BEGIN
        PRINT 'No se encontraron registros en Reclamos con GTQ.';
    END
END

-- Iniciar transacción para las actualizaciones
BEGIN TRANSACTION;

BEGIN TRY
    DECLARE @PolizasActualizadas INT = 0;
    DECLARE @CobrosActualizados INT = 0;
    DECLARE @ReclamosActualizados INT = 0;

    -- Actualizar tabla Polizas
    UPDATE Polizas 
    SET Moneda = 'CRC', 
        UpdatedAt = GETUTCDATE(), 
        UpdatedBy = 'MigrationScript'
    WHERE Moneda = 'GTQ' AND IsDeleted = 0;
    
    SET @PolizasActualizadas = @@ROWCOUNT;

    -- Actualizar tabla Cobros
    UPDATE Cobros 
    SET Moneda = 'CRC', 
        UpdatedAt = GETUTCDATE(), 
        UpdatedBy = 'MigrationScript'
    WHERE Moneda = 'GTQ' AND IsDeleted = 0;
    
    SET @CobrosActualizados = @@ROWCOUNT;

    -- Actualizar tabla Reclamos si existe
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reclamos')
    BEGIN
        UPDATE Reclamos 
        SET Moneda = 'CRC', 
            UpdatedAt = GETUTCDATE(), 
            UpdatedBy = 'MigrationScript'
        WHERE Moneda = 'GTQ' AND IsDeleted = 0;
        
        SET @ReclamosActualizados = @@ROWCOUNT;
    END

    -- Confirmar la transacción
    COMMIT TRANSACTION;

    -- Mostrar resultados
    PRINT 'Migración completada exitosamente:';
    PRINT 'Pólizas actualizadas: ' + CAST(@PolizasActualizadas AS VARCHAR(10));
    PRINT 'Cobros actualizados: ' + CAST(@CobrosActualizados AS VARCHAR(10));
    PRINT 'Reclamos actualizados: ' + CAST(@ReclamosActualizados AS VARCHAR(10));

    -- Verificar que no queden registros con GTQ
    PRINT 'Verificación final - Registros restantes con GTQ:';
    
    DECLARE @PolizasGTQ INT = (SELECT COUNT(*) FROM Polizas WHERE Moneda = 'GTQ' AND IsDeleted = 0);
    DECLARE @CobrosGTQ INT = (SELECT COUNT(*) FROM Cobros WHERE Moneda = 'GTQ' AND IsDeleted = 0);
    DECLARE @ReclamosGTQ INT = 0;
    
    IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reclamos')
    BEGIN
        SET @ReclamosGTQ = (SELECT COUNT(*) FROM Reclamos WHERE Moneda = 'GTQ' AND IsDeleted = 0);
    END
    
    PRINT 'Pólizas con GTQ: ' + CAST(@PolizasGTQ AS VARCHAR(10));
    PRINT 'Cobros con GTQ: ' + CAST(@CobrosGTQ AS VARCHAR(10));
    PRINT 'Reclamos con GTQ: ' + CAST(@ReclamosGTQ AS VARCHAR(10));
    
    IF (@PolizasGTQ + @CobrosGTQ + @ReclamosGTQ) = 0
    BEGIN
        PRINT 'MIGRACIÓN EXITOSA: No quedan registros con moneda GTQ en el sistema.';
    END
    ELSE
    BEGIN
        PRINT 'ADVERTENCIA: Aún existen registros con moneda GTQ que requieren revisión manual.';
    END

END TRY
BEGIN CATCH
    -- Rollback en caso de error
    ROLLBACK TRANSACTION;
    
    PRINT 'Error durante la migración:';
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + ERROR_MESSAGE();
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
    THROW;
END CATCH

-- Mostrar resumen de monedas válidas en el sistema
PRINT '';
PRINT 'Resumen de monedas en el sistema después de la migración:';
PRINT 'Monedas válidas: CRC (Colón Costarricense), USD (Dólar Americano), EUR (Euro)';

-- Estadísticas por tabla
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Polizas')
BEGIN
    PRINT '';
    PRINT 'Distribución de monedas en Pólizas:';
    SELECT Moneda, COUNT(*) as Cantidad
    FROM Polizas 
    WHERE IsDeleted = 0 
    GROUP BY Moneda 
    ORDER BY Moneda;
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Cobros')
BEGIN
    PRINT '';
    PRINT 'Distribución de monedas en Cobros:';
    SELECT Moneda, COUNT(*) as Cantidad
    FROM Cobros 
    WHERE IsDeleted = 0 
    GROUP BY Moneda 
    ORDER BY Moneda;
END

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Reclamos')
BEGIN
    PRINT '';
    PRINT 'Distribución de monedas en Reclamos:';
    SELECT Moneda, COUNT(*) as Cantidad
    FROM Reclamos 
    WHERE IsDeleted = 0 
    GROUP BY Moneda 
    ORDER BY Moneda;
END

GO