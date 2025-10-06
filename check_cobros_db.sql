-- Check database and cobros data
USE SinsegAppDb;
GO

-- Check if Cobros table exists and count records
IF OBJECT_ID('Cobros', 'U') IS NOT NULL
BEGIN
    PRINT 'Tabla Cobros existe';
    SELECT COUNT(*) as TotalCobros FROM Cobros;
    SELECT COUNT(*) as CobrosActivos FROM Cobros WHERE IsDeleted = 0;
    
    -- Show sample records if any exist
    IF EXISTS (SELECT 1 FROM Cobros WHERE IsDeleted = 0)
    BEGIN
        PRINT 'Registros de muestra:';
        SELECT TOP 5 
            Id,
            NumeroRecibo,
            NumeroPoliza,
            ClienteNombre + ' ' + ClienteApellido AS Cliente,
            MontoTotal,
            Moneda,
            FechaVencimiento,
            CASE Estado 
                WHEN 0 THEN 'Pendiente'
                WHEN 1 THEN 'Cobrado'
                WHEN 2 THEN 'Vencido'
                ELSE 'Desconocido'
            END AS EstadoTexto
        FROM Cobros 
        WHERE IsDeleted = 0
        ORDER BY FechaVencimiento;
    END
    ELSE
    BEGIN
        PRINT 'No hay registros de cobros en la base de datos';
    END
END
ELSE
BEGIN
    PRINT 'Tabla Cobros NO existe';
END

-- Check Polizas table too
IF OBJECT_ID('Polizas', 'U') IS NOT NULL
BEGIN
    SELECT COUNT(*) as TotalPolizas FROM Polizas WHERE IsDeleted = 0;
END

GO