USE SINSEGDatabase;
GO

-- Verificar si ya existen datos de prueba
IF NOT EXISTS (SELECT 1 FROM Cobros WHERE NumeroPoliza LIKE 'POL-2024-%')
BEGIN
    PRINT 'Insertando datos de prueba en la tabla Cobros...';
    
    -- Asegurar que existe al menos una póliza para referenciar
    IF NOT EXISTS (SELECT 1 FROM Polizas WHERE Id = 1)
    BEGIN
        SET IDENTITY_INSERT Polizas ON;
        INSERT INTO Polizas (Id, NumeroPoliza, NombreAsegurado, Prima, Frecuencia, FechaVigencia, Modalidad, Marca, Modelo, Placa, Aseguradora, Moneda, EsActivo, PerfilId, CreatedAt, CreatedBy, IsDeleted)
        VALUES (1, 'POL-2024-001', 'María González', 125000.00, 'Mensual', '2024-12-31', 'Completa', 'Toyota', 'Corolla', 'ABC-123', 'INS Nacional', 'CRC', 1, 1, GETUTCDATE(), 1, 0);
        SET IDENTITY_INSERT Polizas OFF;
    END
    
    -- Insertar cobros de prueba
    INSERT INTO Cobros (
        NumeroPoliza,
        NumeroRecibo,
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
    -- Cobro pendiente vencido (Estado 0 = Pendiente)
    ('POL-2024-001', 'REC-2024-001', 'María', 'González', 125000.00, '2024-11-15', 0, 'Transferencia', 'CRC', 1, GETUTCDATE(), 1, 0),
    
    -- Cobro pendiente próximo a vencer
    ('POL-2024-002', 'REC-2024-002', 'Carlos', 'Rodríguez', 180000.00, '2025-01-15', 0, 'Tarjeta', 'CRC', 1, GETUTCDATE(), 1, 0),
    
    -- Cobro cobrado en dólares (Estado 1 = Cobrado)
    ('POL-2024-003', 'REC-2024-003', 'Ana', 'Jiménez', 450.00, '2024-12-01', 1, 'Efectivo', 'USD', 1, GETUTCDATE(), 1, 0),
    
    -- Cobro pendiente en euros
    ('POL-2024-004', 'REC-2024-004', 'Luis', 'Vargas', 320.50, '2025-02-10', 0, 'Cheque', 'EUR', 1, GETUTCDATE(), 1, 0),
    
    -- Cobro vencido en colones (Estado 2 = Vencido)
    ('POL-2024-005', 'REC-2024-005', 'Patricia', 'Morales', 95000.00, '2024-10-30', 2, 'Transferencia', 'CRC', 1, GETUTCDATE(), 1, 0);

    -- Actualizar los cobros que fueron pagados con montos cobrados y fechas
    UPDATE Cobros 
    SET MontoCobrado = MontoTotal, 
        FechaCobro = '2024-12-01',
        UsuarioCobroId = 1,
        UsuarioCobroNombre = 'admin'
    WHERE Estado = 1 AND NumeroPoliza LIKE 'POL-2024-%';

    PRINT 'Datos de prueba insertados exitosamente!';
END
ELSE
BEGIN
    PRINT 'Ya existen datos de prueba en la tabla Cobros.';
END

GO

-- Mostrar los datos insertados
SELECT 
    Id,
    NumeroPoliza,
    NumeroRecibo,
    ClienteNombre + ' ' + ClienteApellido AS Cliente,
    MontoTotal,
    Moneda,
    FechaVencimiento,
    CASE Estado 
        WHEN 0 THEN 'Pendiente'
        WHEN 1 THEN 'Cobrado'
        WHEN 2 THEN 'Vencido'
        ELSE 'Desconocido'
    END AS EstadoTexto,
    MetodoPago,
    CreatedAt
FROM Cobros 
WHERE IsDeleted = 0 AND NumeroPoliza LIKE 'POL-2024-%'
ORDER BY FechaVencimiento;

GO