-- Agregar columna Observaciones a la tabla Polizas
-- Fecha: 2025-12-17
-- Descripción: Permite almacenar notas y observaciones sobre cada póliza

USE SiinadsegDB;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('Polizas') 
    AND name = 'Observaciones'
)
BEGIN
    ALTER TABLE Polizas
    ADD Observaciones NVARCHAR(500) NULL;
    
    PRINT 'Columna Observaciones agregada exitosamente a la tabla Polizas';
END
ELSE
BEGIN
    PRINT 'La columna Observaciones ya existe en la tabla Polizas';
END
GO
