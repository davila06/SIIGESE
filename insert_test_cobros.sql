-- Script para insertar datos de prueba en la tabla Cobros
-- Primero insertar algunos datos en la tabla Polizas si no existen

-- Insertar algunos cobros de prueba
INSERT INTO [dbo].[Cobros] (
    [NumeroRecibo], 
    [PolizaId], 
    [NumeroPoliza], 
    [ClienteNombre], 
    [ClienteApellido], 
    [FechaVencimiento], 
    [MontoTotal], 
    [Moneda],
    [Estado], 
    [CreatedAt], 
    [CreatedBy], 
    [IsDeleted]
) VALUES 
('REC-2025-001', 1, 'POL-2025-001', 'Juan', 'Pérez', '2025-01-15', 250000.00, 'CRC', 0, GETDATE(), 'System', 0),
('REC-2025-002', 2, 'POL-2025-002', 'María', 'González', '2025-01-20', 180000.00, 'CRC', 1, GETDATE(), 'System', 0),
('REC-2025-003', 3, 'POL-2025-003', 'Carlos', 'Rodríguez', '2024-12-30', 320000.00, 'CRC', 2, GETDATE(), 'System', 0),
('REC-2025-004', 4, 'POL-2025-004', 'Ana', 'Martínez', '2025-02-01', 150000.00, 'USD', 0, GETDATE(), 'System', 0),
('REC-2025-005', 5, 'POL-2025-005', 'Roberto', 'López', '2025-01-25', 280000.00, 'CRC', 0, GETDATE(), 'System', 0),
('REC-2025-006', 6, 'POL-2025-006', 'Carmen', 'Vargas', '2025-01-10', 450000.00, 'CRC', 1, GETDATE(), 'System', 0),
('REC-2025-007', 7, 'POL-2025-007', 'Luis', 'Hernández', '2024-12-25', 200000.00, 'CRC', 2, GETDATE(), 'System', 0),
('REC-2025-008', 8, 'POL-2025-008', 'Elena', 'Jiménez', '2025-02-15', 380000.00, 'EUR', 0, GETDATE(), 'System', 0);

-- Actualizar algunos cobros como pagados
UPDATE [dbo].[Cobros] 
SET [FechaCobro] = DATEADD(day, -5, GETDATE()), 
    [MontoCobrado] = [MontoTotal], 
    [MetodoPago] = 1 
WHERE [NumeroRecibo] IN ('REC-2025-002', 'REC-2025-006');

-- Verificar los datos insertados
SELECT * FROM [dbo].[Cobros] WHERE [IsDeleted] = 0;