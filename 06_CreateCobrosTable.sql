-- =====================================================
-- SCRIPT PARA CREACIÓN DE TABLAS DE COBROS - SINSEG
-- =====================================================
-- Fecha: 2025-10-04
-- Descripción: Script completo para el módulo de cobros
-- =====================================================

-- =====================================================
-- 1. TABLA PRINCIPAL DE COBROS
-- =====================================================

-- La tabla Cobros ya fue creada automáticamente por Entity Framework
-- con la migración: 20251005011905_AddCobrosTable

-- Estructura de la tabla:
/*
CREATE TABLE [Cobros] (
    [Id] int NOT NULL IDENTITY,
    [NumeroRecibo] nvarchar(50) NOT NULL,
    [PolizaId] int NOT NULL,
    [NumeroPoliza] nvarchar(50) NOT NULL,
    [ClienteNombre] nvarchar(100) NOT NULL,
    [ClienteApellido] nvarchar(100) NOT NULL,
    [FechaVencimiento] datetime2 NOT NULL,
    [FechaCobro] datetime2 NULL,
    [MontoTotal] decimal(18,2) NOT NULL,
    [MontoCobrado] decimal(18,2) NULL,
    [Estado] int NOT NULL,
    [MetodoPago] int NULL,
    [Observaciones] nvarchar(500) NULL,
    [UsuarioCobroId] int NULL,
    [UsuarioCobroNombre] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NOT NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsDeleted] bit NOT NULL,
    
    CONSTRAINT [PK_Cobros] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Cobros_Polizas_PolizaId] FOREIGN KEY ([PolizaId]) 
        REFERENCES [Polizas] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_Cobros_Users_UsuarioCobroId] FOREIGN KEY ([UsuarioCobroId]) 
        REFERENCES [Users] ([Id]) ON DELETE SET NULL
);
*/

-- =====================================================
-- 2. ÍNDICES PARA OPTIMIZACIÓN
-- =====================================================

-- Índices ya creados por la migración:
/*
CREATE UNIQUE INDEX [IX_Cobros_NumeroRecibo] ON [Cobros] ([NumeroRecibo]);
CREATE INDEX [IX_Cobros_PolizaId] ON [Cobros] ([PolizaId]);
CREATE INDEX [IX_Cobros_Estado] ON [Cobros] ([Estado]);
CREATE INDEX [IX_Cobros_FechaVencimiento] ON [Cobros] ([FechaVencimiento]);
CREATE INDEX [IX_Cobros_UsuarioCobroId] ON [Cobros] ([UsuarioCobroId]);
*/

-- =====================================================
-- 3. ENUMERACIONES
-- =====================================================

-- EstadoCobro:
-- 0 = Pendiente
-- 1 = Cobrado
-- 2 = Vencido
-- 3 = Cancelado

-- MetodoPago:
-- 0 = Efectivo
-- 1 = Transferencia
-- 2 = Cheque
-- 3 = TarjetaCredito
-- 4 = TarjetaDebito

-- =====================================================
-- 4. DATOS DE PRUEBA (OPCIONAL)
-- =====================================================

-- Insertar algunos cobros de ejemplo (requiere que existan pólizas)
/*
INSERT INTO [Cobros] (
    [NumeroRecibo], [PolizaId], [NumeroPoliza], [ClienteNombre], [ClienteApellido],
    [FechaVencimiento], [MontoTotal], [Estado], [Observaciones],
    [CreatedAt], [CreatedBy], [IsDeleted]
) VALUES 
-- Ejemplo 1: Cobro pendiente
('REC-20251004-1001', 1, 'POL-001', 'Juan', 'Pérez', '2025-11-15', 150000.00, 0, 
 'Cobro mensual póliza vehículo', GETDATE(), 'System', 0),

-- Ejemplo 2: Cobro vencido
('REC-20251004-1002', 1, 'POL-001', 'Juan', 'Pérez', '2025-09-15', 150000.00, 2, 
 'Cobro vencido - contactar cliente', GETDATE(), 'System', 0),

-- Ejemplo 3: Cobro pagado
('REC-20251004-1003', 1, 'POL-001', 'Juan', 'Pérez', '2025-10-15', 150000.00, 1, 
 'Cobro pagado en efectivo', GETDATE(), 'System', 0);

-- Actualizar el cobro pagado con información de pago
UPDATE [Cobros] 
SET [FechaCobro] = '2025-10-15', 
    [MontoCobrado] = 150000.00, 
    [MetodoPago] = 0, -- Efectivo
    [UsuarioCobroId] = 1,
    [UsuarioCobroNombre] = 'Admin',
    [UpdatedAt] = GETDATE(),
    [UpdatedBy] = 'System'
WHERE [NumeroRecibo] = 'REC-20251004-1003';
*/

-- =====================================================
-- 5. VISTAS ÚTILES PARA REPORTES
-- =====================================================

-- Vista de cobros con información detallada
CREATE VIEW vw_CobrosDetalle AS
SELECT 
    c.Id,
    c.NumeroRecibo,
    c.PolizaId,
    c.NumeroPoliza,
    c.ClienteNombre,
    c.ClienteApellido,
    c.ClienteNombre + ' ' + c.ClienteApellido AS ClienteCompleto,
    c.FechaVencimiento,
    c.FechaCobro,
    c.MontoTotal,
    c.MontoCobrado,
    CASE c.Estado 
        WHEN 0 THEN 'Pendiente'
        WHEN 1 THEN 'Cobrado'
        WHEN 2 THEN 'Vencido' 
        WHEN 3 THEN 'Cancelado'
    END AS EstadoDescripcion,
    CASE c.MetodoPago
        WHEN 0 THEN 'Efectivo'
        WHEN 1 THEN 'Transferencia'
        WHEN 2 THEN 'Cheque'
        WHEN 3 THEN 'Tarjeta de Crédito'
        WHEN 4 THEN 'Tarjeta de Débito'
        ELSE NULL
    END AS MetodoPagoDescripcion,
    c.Observaciones,
    c.UsuarioCobroNombre,
    c.CreatedAt AS FechaCreacion,
    c.UpdatedAt AS FechaActualizacion,
    -- Campos calculados
    CASE 
        WHEN c.Estado = 0 AND c.FechaVencimiento < CAST(GETDATE() AS DATE) THEN 1 
        ELSE 0 
    END AS EsVencido,
    DATEDIFF(DAY, GETDATE(), c.FechaVencimiento) AS DiasParaVencimiento,
    p.Aseguradora,
    p.NombreAsegurado AS ClientePoliza
FROM [Cobros] c
INNER JOIN [Polizas] p ON c.PolizaId = p.Id
WHERE c.IsDeleted = 0;

-- Vista de estadísticas de cobros
CREATE VIEW vw_EstadisticasCobros AS
SELECT 
    COUNT(*) AS TotalCobros,
    COUNT(CASE WHEN Estado = 0 THEN 1 END) AS CobrosPendientes,
    COUNT(CASE WHEN Estado = 1 THEN 1 END) AS CobrosCobrados,
    COUNT(CASE WHEN Estado = 2 THEN 1 END) AS CobrosVencidos,
    COUNT(CASE WHEN Estado = 3 THEN 1 END) AS CobrosCancelados,
    SUM(CASE WHEN Estado = 0 THEN MontoTotal ELSE 0 END) AS MontoTotalPendiente,
    SUM(CASE WHEN Estado = 1 THEN MontoCobrado ELSE 0 END) AS MontoTotalCobrado,
    SUM(CASE WHEN Estado = 2 THEN MontoTotal ELSE 0 END) AS MontoTotalVencido,
    AVG(CASE WHEN Estado = 1 THEN MontoCobrado END) AS PromedioMontoCobrado,
    -- Estadísticas por mes actual
    COUNT(CASE WHEN Estado = 1 AND MONTH(FechaCobro) = MONTH(GETDATE()) 
               AND YEAR(FechaCobro) = YEAR(GETDATE()) THEN 1 END) AS CobrosEsteMes,
    SUM(CASE WHEN Estado = 1 AND MONTH(FechaCobro) = MONTH(GETDATE()) 
             AND YEAR(FechaCobro) = YEAR(GETDATE()) THEN MontoCobrado ELSE 0 END) AS MontosCobradosEsteMes
FROM [Cobros]
WHERE IsDeleted = 0;

-- =====================================================
-- 6. STORED PROCEDURES ÚTILES
-- =====================================================

-- Procedimiento para obtener cobros próximos a vencer
CREATE PROCEDURE sp_GetCobrosProximosVencer
    @Dias INT = 7
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT c.*, p.Aseguradora, p.NombreAsegurado
    FROM [Cobros] c
    INNER JOIN [Polizas] p ON c.PolizaId = p.Id
    WHERE c.IsDeleted = 0 
      AND c.Estado = 0 -- Pendiente
      AND c.FechaVencimiento >= CAST(GETDATE() AS DATE)
      AND c.FechaVencimiento <= CAST(DATEADD(DAY, @Dias, GETDATE()) AS DATE)
    ORDER BY c.FechaVencimiento ASC;
END;

-- Procedimiento para generar número de recibo único
CREATE PROCEDURE sp_GenerarNumeroRecibo
    @NumeroRecibo NVARCHAR(50) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @Contador INT = 1;
    DECLARE @Fecha NVARCHAR(8) = FORMAT(GETDATE(), 'yyyyMMdd');
    DECLARE @NumeroTemporal NVARCHAR(50);
    
    WHILE @Contador <= 9999
    BEGIN
        SET @NumeroTemporal = 'REC-' + @Fecha + '-' + FORMAT(@Contador, '0000');
        
        IF NOT EXISTS (SELECT 1 FROM [Cobros] WHERE NumeroRecibo = @NumeroTemporal)
        BEGIN
            SET @NumeroRecibo = @NumeroTemporal;
            BREAK;
        END
        
        SET @Contador = @Contador + 1;
    END
    
    IF @Contador > 9999
    BEGIN
        -- Si se agotaron los números del día, agregar timestamp
        SET @NumeroRecibo = 'REC-' + @Fecha + '-' + FORMAT(DATEPART(HOUR, GETDATE()), '00') + 
                           FORMAT(DATEPART(MINUTE, GETDATE()), '00') + 
                           FORMAT(DATEPART(SECOND, GETDATE()), '00');
    END
END;

-- =====================================================
-- 7. TRIGGERS PARA AUDITORÍA
-- =====================================================

-- Trigger para actualizar automáticamente el estado a vencido
CREATE TRIGGER tr_ActualizarEstadoVencido
ON [Cobros]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE c
    SET Estado = 2, -- Vencido
        UpdatedAt = GETDATE(),
        UpdatedBy = 'System-Trigger'
    FROM [Cobros] c
    INNER JOIN inserted i ON c.Id = i.Id
    WHERE c.Estado = 0 -- Solo pendientes
      AND c.FechaVencimiento < CAST(GETDATE() AS DATE)
      AND c.IsDeleted = 0;
END;

-- =====================================================
-- 8. ÍNDICES ADICIONALES PARA PERFORMANCE
-- =====================================================

-- Índice compuesto para consultas comunes
CREATE INDEX IX_Cobros_Estado_FechaVencimiento 
ON [Cobros] (Estado, FechaVencimiento) 
INCLUDE (MontoTotal, NumeroRecibo);

-- Índice para búsquedas por fecha de cobro
CREATE INDEX IX_Cobros_FechaCobro 
ON [Cobros] (FechaCobro) 
WHERE FechaCobro IS NOT NULL;

-- Índice para estadísticas mensuales
CREATE INDEX IX_Cobros_EstadisticasMensuales 
ON [Cobros] (Estado, FechaCobro) 
INCLUDE (MontoCobrado);

-- =====================================================
-- 9. CONFIGURACIÓN DE BACKUP Y MANTENIMIENTO
-- =====================================================

-- Script para limpieza de cobros antiguos (ejecutar periódicamente)
/*
-- Marcar como eliminados los cobros cancelados de más de 1 año
UPDATE [Cobros] 
SET IsDeleted = 1, 
    UpdatedAt = GETDATE(), 
    UpdatedBy = 'System-Cleanup'
WHERE Estado = 3 -- Cancelado
  AND CreatedAt < DATEADD(YEAR, -1, GETDATE())
  AND IsDeleted = 0;
*/

-- =====================================================
-- 10. VERIFICACIÓN DEL SISTEMA
-- =====================================================

-- Script para verificar integridad de datos
SELECT 
    'Verificación del Sistema de Cobros' AS Mensaje,
    (SELECT COUNT(*) FROM [Cobros] WHERE IsDeleted = 0) AS TotalCobros,
    (SELECT COUNT(*) FROM [Cobros] WHERE Estado = 0 AND IsDeleted = 0) AS CobrosPendientes,
    (SELECT COUNT(*) FROM [Cobros] WHERE Estado = 1 AND IsDeleted = 0) AS CobrosCobrados,
    (SELECT COUNT(*) FROM [Cobros] WHERE Estado = 2 AND IsDeleted = 0) AS CobrosVencidos,
    (SELECT COUNT(*) FROM [Cobros] WHERE NumeroRecibo IS NULL OR NumeroRecibo = '') AS ErroresNumeroRecibo,
    (SELECT COUNT(*) FROM [Cobros] WHERE PolizaId NOT IN (SELECT Id FROM [Polizas])) AS ErroresPolizaInexistente;

-- =====================================================
-- FIN DEL SCRIPT
-- =====================================================

PRINT 'Script de creación de tablas de cobros ejecutado exitosamente.';
PRINT 'Sistema de cobros SINSEG configurado correctamente.';
PRINT 'Entidades creadas:';
PRINT '- Tabla Cobros con índices y restricciones';
PRINT '- Vistas para reportes (vw_CobrosDetalle, vw_EstadisticasCobros)';
PRINT '- Stored procedures (sp_GetCobrosProximosVencer, sp_GenerarNumeroRecibo)';
PRINT '- Triggers para auditoría (tr_ActualizarEstadoVencido)';
PRINT '- Índices adicionales para performance';