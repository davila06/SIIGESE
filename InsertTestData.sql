-- Script para insertar datos de prueba en la tabla Cobros
USE SINSEGDatabase;
GO

-- Insertar datos de prueba en la tabla Cobros
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
-- Cobro pendiente vencido
('POL-2024-001', 'REC-2024-001', 'María', 'González', 125000.00, '2024-11-15', 0, 'Transferencia', 'CRC', 1, GETUTCDATE(), 1, 0),

-- Cobro pendiente próximo a vencer
('POL-2024-002', 'REC-2024-002', 'Carlos', 'Rodríguez', 180000.00, '2025-01-15', 0, 'Tarjeta', 'CRC', 2, GETUTCDATE(), 1, 0),

-- Cobro cobrado en dólares
('POL-2024-003', 'REC-2024-003', 'Ana', 'Jiménez', 450.00, '2024-12-01', 1, 'Efectivo', 'USD', 3, GETUTCDATE(), 1, 0),

-- Cobro pendiente en euros
('POL-2024-004', 'REC-2024-004', 'Luis', 'Vargas', 320.50, '2025-02-10', 0, 'Cheque', 'EUR', 4, GETUTCDATE(), 1, 0),

-- Cobro vencido en colones
('POL-2024-005', 'REC-2024-005', 'Patricia', 'Morales', 95000.00, '2024-10-30', 2, 'Transferencia', 'CRC', 5, GETUTCDATE(), 1, 0);

-- Actualizar los cobros que fueron pagados con montos cobrados y fechas
UPDATE Cobros 
SET MontoCobrado = MontoTotal, 
    FechaCobro = '2024-12-01',
    UsuarioCobroId = 1,
    UsuarioCobroNombre = 'admin'
WHERE Estado = 1;

GO

-- Verificar los datos insertados
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
    MetodoPago
FROM Cobros 
WHERE IsDeleted = 0
ORDER BY FechaVencimiento;