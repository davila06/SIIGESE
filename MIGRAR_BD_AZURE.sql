-- =============================================
-- MIGRACIÓN BD AZURE - ACTUALIZAR ESTRUCTURA
-- =============================================

-- Verificar y agregar columnas faltantes en Polizas
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'NumeroCedula')
BEGIN
    ALTER TABLE [Polizas] ADD [NumeroCedula] nvarchar(20) NULL;
    PRINT '✓ Columna NumeroCedula agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Placa')
BEGIN
    ALTER TABLE [Polizas] ADD [Placa] nvarchar(10) NULL;
    PRINT '✓ Columna Placa agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Marca')
BEGIN
    ALTER TABLE [Polizas] ADD [Marca] nvarchar(50) NULL;
    PRINT '✓ Columna Marca agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Modelo')
BEGIN
    ALTER TABLE [Polizas] ADD [Modelo] nvarchar(50) NULL;
    PRINT '✓ Columna Modelo agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Año')
BEGIN
    ALTER TABLE [Polizas] ADD [Año] nvarchar(4) NULL;
    PRINT '✓ Columna Año agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Correo')
BEGIN
    ALTER TABLE [Polizas] ADD [Correo] nvarchar(100) NULL;
    PRINT '✓ Columna Correo agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'NumeroTelefono')
BEGIN
    ALTER TABLE [Polizas] ADD [NumeroTelefono] nvarchar(20) NULL;
    PRINT '✓ Columna NumeroTelefono agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Frecuencia')
BEGIN
    ALTER TABLE [Polizas] ADD [Frecuencia] nvarchar(50) NULL;
    PRINT '✓ Columna Frecuencia agregada';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Aseguradora')
BEGIN
    ALTER TABLE [Polizas] ADD [Aseguradora] nvarchar(100) NULL;
    PRINT '✓ Columna Aseguradora agregada';
END

-- Hacer nullable las columnas opcionales
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Modalidad')
BEGIN
    ALTER TABLE [Polizas] ALTER COLUMN [Modalidad] nvarchar(50) NULL;
    PRINT '✓ Columna Modalidad ahora es nullable';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Placa')
BEGIN
    ALTER TABLE [Polizas] ALTER COLUMN [Placa] nvarchar(10) NULL;
    PRINT '✓ Columna Placa ahora es nullable';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Marca')
BEGIN
    ALTER TABLE [Polizas] ALTER COLUMN [Marca] nvarchar(50) NULL;
    PRINT '✓ Columna Marca ahora es nullable';
END

IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Modelo')
BEGIN
    ALTER TABLE [Polizas] ALTER COLUMN [Modelo] nvarchar(50) NULL;
    PRINT '✓ Columna Modelo ahora es nullable';
END

-- Insertar datos de prueba
PRINT '✓ Insertando pólizas de prueba...';

-- Polizas Auto
INSERT INTO [Polizas] (NumeroPoliza, NombreAsegurado, NumeroCedula, Prima, Moneda, FechaVigencia, Frecuencia, Aseguradora, Placa, Marca, Modelo, Año, Correo, NumeroTelefono, PerfilId, Modalidad, EsActivo, CreatedAt, CreatedBy, IsDeleted)
VALUES 
('POL-2024-0001', 'Juan Pérez García', '1-0234-0567', 125000.00, 'CRC', '2024-01-15', 'Mensual', 'INS', 'ABC123', 'Toyota', 'Corolla', '2022', 'juan.perez@email.com', '8888-1234', 1, 'Auto', 1, GETDATE(), 'System', 0),
('POL-2024-0002', 'María Rodríguez López', '2-0345-0678', 95000.00, 'CRC', '2024-02-01', 'Trimestral', 'Mapfre', 'DEF456', 'Honda', 'Civic', '2021', 'maria.rodriguez@email.com', '8888-2345', 1, 'Auto', 1, GETDATE(), 'System', 0),
('POL-2024-0003', 'Carlos Fernández Mora', '1-0456-0789', 150000.00, 'USD', '2024-03-10', 'Semestral', 'Assa', 'GHI789', 'Mazda', 'CX-5', '2023', 'carlos.fernandez@email.com', '8888-3456', 1, 'Auto', 1, GETDATE(), 'System', 0),
('POL-2024-0004', 'Ana Jiménez Castro', '2-0567-0890', 88000.00, 'CRC', '2024-04-05', 'Mensual', 'Sagicor', 'JKL012', 'Nissan', 'Sentra', '2020', 'ana.jimenez@email.com', '8888-4567', 1, 'Auto', 1, GETDATE(), 'System', 0),
('POL-2024-0005', 'Luis Vargas Soto', '1-0678-0901', 180000.00, 'USD', '2024-05-20', 'Anual', 'INS', 'MNO345', 'Ford', 'Explorer', '2024', 'luis.vargas@email.com', '8888-5678', 1, 'Auto', 1, GETDATE(), 'System', 0);

-- Polizas Vida (sin datos de vehículo)
INSERT INTO [Polizas] (NumeroPoliza, NombreAsegurado, NumeroCedula, Prima, Moneda, FechaVigencia, Frecuencia, Aseguradora, Correo, NumeroTelefono, PerfilId, Modalidad, Placa, Marca, Modelo, EsActivo, CreatedAt, CreatedBy, IsDeleted)
VALUES 
('POL-2024-0006', 'Sandra Méndez Rojas', '1-0789-0012', 75000.00, 'CRC', '2024-06-01', 'Mensual', 'Mapfre', 'sandra.mendez@email.com', '8888-6789', 1, 'Vida', 'N/A', 'N/A', 'N/A', 1, GETDATE(), 'System', 0),
('POL-2024-0007', 'Roberto Chaves Alfaro', '2-0890-0123', 120000.00, 'USD', '2024-07-15', 'Trimestral', 'Assa', 'roberto.chaves@email.com', '8888-7890', 1, 'Vida', 'N/A', 'N/A', 'N/A', 1, GETDATE(), 'System', 0),
('POL-2024-0008', 'Patricia Solís Herrera', '1-0901-0234', 65000.00, 'CRC', '2024-08-20', 'Semestral', 'Sagicor', 'patricia.solis@email.com', '8888-8901', 1, 'Vida', 'N/A', 'N/A', 'N/A', 1, GETDATE(), 'System', 0);

-- Polizas con datos parciales
INSERT INTO [Polizas] (NumeroPoliza, NombreAsegurado, NumeroCedula, Prima, Moneda, FechaVigencia, Frecuencia, Aseguradora, Placa, Marca, Modelo, PerfilId, Modalidad, EsActivo, CreatedAt, CreatedBy, IsDeleted)
VALUES 
('POL-2024-0009', 'Diego Ramírez Vargas', '1-1012-0345', 98000.00, 'CRC', '2024-09-10', 'Mensual', 'INS', 'PQR678', 'Kia', 'Sportage', 1, 'Auto', 1, GETDATE(), 'System', 0),
('POL-2024-0010', 'Laura Vega Campos', '2-1123-0456', 142000.00, 'USD', '2024-10-05', 'Anual', 'Mapfre', 'STU901', 'Hyundai', 'Tucson', 1, 'Auto', 1, GETDATE(), 'System', 0);

PRINT '✓ Migración completada!';
PRINT '✓ Se insertaron 10 pólizas de prueba';

-- Verificación
SELECT COUNT(*) AS TotalPolizas FROM Polizas WHERE IsDeleted = 0;
SELECT COUNT(*) AS TotalUsuarios FROM Users WHERE IsDeleted = 0;
