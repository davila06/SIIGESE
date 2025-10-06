USE SinsegAppDb;
GO

-- Verificar si ya existen datos de prueba
IF NOT EXISTS (SELECT 1 FROM Cobros WHERE NumeroRecibo LIKE 'REC-2024-%')
BEGIN
    PRINT 'Insertando datos de prueba en la tabla Cobros...';
    
    -- Obtener los primeros IDs de pólizas existentes
    DECLARE @PolizaId1 INT, @PolizaId2 INT, @PolizaId3 INT, @PolizaId4 INT, @PolizaId5 INT;
    
    SELECT TOP 5 @PolizaId1 = COALESCE(@PolizaId1, Id),
                 @PolizaId2 = CASE WHEN @PolizaId1 IS NOT NULL AND @PolizaId2 IS NULL AND Id > @PolizaId1 THEN Id ELSE @PolizaId2 END,
                 @PolizaId3 = CASE WHEN @PolizaId2 IS NOT NULL AND @PolizaId3 IS NULL AND Id > @PolizaId2 THEN Id ELSE @PolizaId3 END,
                 @PolizaId4 = CASE WHEN @PolizaId3 IS NOT NULL AND @PolizaId4 IS NULL AND Id > @PolizaId3 THEN Id ELSE @PolizaId4 END,
                 @PolizaId5 = CASE WHEN @PolizaId4 IS NOT NULL AND @PolizaId5 IS NULL AND Id > @PolizaId4 THEN Id ELSE @PolizaId5 END
    FROM Polizas WHERE EsActivo = 1 ORDER BY Id;
    
    -- Si no hay suficientes pólizas, usar la primera para todas
    SET @PolizaId1 = COALESCE(@PolizaId1, (SELECT TOP 1 Id FROM Polizas WHERE EsActivo = 1));
    SET @PolizaId2 = COALESCE(@PolizaId2, @PolizaId1);
    SET @PolizaId3 = COALESCE(@PolizaId3, @PolizaId1);
    SET @PolizaId4 = COALESCE(@PolizaId4, @PolizaId1);
    SET @PolizaId5 = COALESCE(@PolizaId5, @PolizaId1);
    
    -- Obtener números de póliza para referencias
    DECLARE @NumPoliza1 VARCHAR(50), @NumPoliza2 VARCHAR(50), @NumPoliza3 VARCHAR(50), @NumPoliza4 VARCHAR(50), @NumPoliza5 VARCHAR(50);
    
    SELECT @NumPoliza1 = NumeroPoliza FROM Polizas WHERE Id = @PolizaId1;
    SELECT @NumPoliza2 = NumeroPoliza FROM Polizas WHERE Id = @PolizaId2;
    SELECT @NumPoliza3 = NumeroPoliza FROM Polizas WHERE Id = @PolizaId3;
    SELECT @NumPoliza4 = NumeroPoliza FROM Polizas WHERE Id = @PolizaId4;
    SELECT @NumPoliza5 = NumeroPoliza FROM Polizas WHERE Id = @PolizaId5;
    
    -- Insertar cobros de prueba
    INSERT INTO Cobros (
        NumeroRecibo,
        NumeroPoliza,
        ClienteNombre,
        ClienteApellido,
        MontoTotal,
        FechaVencimiento,
        Estado,
        MetodoPago,
        Moneda,
        PolizaId,
        CreatedAt,
        CreatedBy,
        IsDeleted
    ) VALUES 
    -- Cobro pendiente vencido (Estado 0 = Pendiente, MetodoPago 1 = Transferencia)
    ('REC-2024-001', @NumPoliza1, 'María', 'González', 125000.00, '2024-11-15', 0, 1, 'CRC', @PolizaId1, GETUTCDATE(), '1', 0),
    
    -- Cobro pendiente próximo a vencer (MetodoPago 3 = TarjetaCredito)
    ('REC-2024-002', @NumPoliza2, 'Carlos', 'Rodríguez', 180000.00, '2025-01-15', 0, 3, 'CRC', @PolizaId2, GETUTCDATE(), '1', 0),
    
    -- Cobro cobrado en dólares (Estado 1 = Cobrado, MetodoPago 0 = Efectivo)
    ('REC-2024-003', @NumPoliza3, 'Ana', 'Jiménez', 450.00, '2024-12-01', 1, 0, 'USD', @PolizaId3, GETUTCDATE(), '1', 0),
    
    -- Cobro pendiente en euros (MetodoPago 2 = Cheque)
    ('REC-2024-004', @NumPoliza4, 'Luis', 'Vargas', 320.50, '2025-02-10', 0, 2, 'EUR', @PolizaId4, GETUTCDATE(), '1', 0),
    
    -- Cobro vencido en colones (Estado 2 = Vencido, MetodoPago 1 = Transferencia)
    ('REC-2024-005', @NumPoliza5, 'Patricia', 'Morales', 95000.00, '2024-10-30', 2, 1, 'CRC', @PolizaId5, GETUTCDATE(), '1', 0);

    -- Actualizar los cobros que fueron pagados con montos cobrados y fechas
    UPDATE Cobros 
    SET FechaCobro = DATEADD(day, -5, GETUTCDATE()),
        MontoCobrado = MontoTotal
    WHERE Estado = 1; -- Solo cobros marcados como cobrados

    PRINT 'Datos de prueba insertados exitosamente.';
    
    -- Mostrar resumen
    SELECT 
        'Resumen de cobros insertados' as Titulo,
        COUNT(*) as TotalCobros,
        SUM(CASE WHEN Estado = 0 THEN 1 ELSE 0 END) as Pendientes,
        SUM(CASE WHEN Estado = 1 THEN 1 ELSE 0 END) as Cobrados,
        SUM(CASE WHEN Estado = 2 THEN 1 ELSE 0 END) as Vencidos
    FROM Cobros WHERE NumeroRecibo LIKE 'REC-2024-%';
    
END
ELSE
BEGIN
    PRINT 'Los datos de prueba ya existen en la base de datos.';
    
    -- Mostrar resumen actual
    SELECT 
        'Resumen de cobros existentes' as Titulo,
        COUNT(*) as TotalCobros,
        SUM(CASE WHEN Estado = 0 THEN 1 ELSE 0 END) as Pendientes,
        SUM(CASE WHEN Estado = 1 THEN 1 ELSE 0 END) as Cobrados,
        SUM(CASE WHEN Estado = 2 THEN 1 ELSE 0 END) as Vencidos
    FROM Cobros;
END