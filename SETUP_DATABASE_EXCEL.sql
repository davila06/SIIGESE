-- =============================================
-- SCRIPT COMPLETO PARA BASE DE DATOS SINSEG
-- SOPORTE PARA CARGA DE ARCHIVOS EXCEL (14 COLUMNAS)
-- Fecha: 15 de diciembre, 2025
-- =============================================

USE master;
GO

-- ===================================
-- 1. CREAR BASE DE DATOS
-- ===================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SinsegAppDb')
BEGIN
    CREATE DATABASE SinsegAppDb;
    PRINT '✓ Base de datos SinsegAppDb creada exitosamente.';
END
ELSE
BEGIN
    PRINT '✓ La base de datos SinsegAppDb ya existe.';
END
GO

USE SinsegAppDb;
GO

-- ===================================
-- 2. TABLA DE MIGRACIONES EF
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
    PRINT '✓ Tabla __EFMigrationsHistory creada.';
END
GO

-- ===================================
-- 3. TABLA ROLES
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [Roles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Roles_Name] UNIQUE ([Name])
    );
    PRINT '✓ Tabla Roles creada.';
END
GO

-- ===================================
-- 4. TABLA USUARIOS
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE [Users] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserName] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [RoleId] int NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Users_UserName] UNIQUE ([UserName]),
        CONSTRAINT [UK_Users_Email] UNIQUE ([Email]),
        CONSTRAINT [FK_Users_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])
    );
    PRINT '✓ Tabla Users creada.';
END
GO

-- ===================================
-- 5. TABLA PERFILES
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Perfiles' AND xtype='U')
BEGIN
    CREATE TABLE [Perfiles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Descripcion] nvarchar(300) NULL,
        [EsActivo] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Perfiles] PRIMARY KEY ([Id])
    );
    PRINT '✓ Tabla Perfiles creada.';
END
GO

-- ===================================
-- 6. TABLA CLIENTES
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Codigo] nvarchar(20) NOT NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [NombreComercial] nvarchar(200) NOT NULL,
        [NIT] nvarchar(20) NOT NULL,
        [Telefono] nvarchar(20) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(300) NOT NULL,
        [Ciudad] nvarchar(100) NOT NULL,
        [Departamento] nvarchar(100) NOT NULL,
        [Pais] nvarchar(100) NOT NULL DEFAULT 'Costa Rica',
        [EsActivo] bit NOT NULL DEFAULT 1,
        [FechaRegistro] datetime2 NULL DEFAULT GETDATE(),
        [PerfilId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Clientes_Codigo] UNIQUE ([Codigo]),
        CONSTRAINT [FK_Clientes_Perfiles] FOREIGN KEY ([PerfilId]) REFERENCES [Perfiles]([Id])
    );
    PRINT '✓ Tabla Clientes creada.';
END
GO

-- ===================================
-- 7. TABLA POLIZAS (CON SOPORTE EXCEL 14 COLUMNAS)
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Polizas' AND xtype='U')
BEGIN
    CREATE TABLE [Polizas] (
        [Id] int IDENTITY(1,1) NOT NULL,
        -- Columnas del Excel (14 columnas)
        [NumeroPoliza] nvarchar(50) NOT NULL,              -- Col 1: POLIZA
        [NombreAsegurado] nvarchar(200) NOT NULL,          -- Col 2: NOMBRE
        [NumeroCedula] nvarchar(20) NOT NULL,              -- Col 3: NUMEROCEDULA
        [Prima] decimal(18,2) NOT NULL,                     -- Col 4: PRIMA
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',       -- Col 5: MONEDA
        [FechaVigencia] datetime2 NOT NULL,                 -- Col 6: FECHA
        [Frecuencia] nvarchar(50) NOT NULL,                 -- Col 7: FRECUENCIA
        [Aseguradora] nvarchar(100) NOT NULL,               -- Col 8: ASEGURADORA
        [Placa] nvarchar(10) NULL,                          -- Col 9: PLACA (Opcional)
        [Marca] nvarchar(50) NULL,                          -- Col 10: MARCA (Opcional)
        [Modelo] nvarchar(50) NULL,                         -- Col 11: MODELO (Opcional)
        [Año] nvarchar(4) NULL,                             -- Col 12: AÑO (Opcional)
        [Correo] nvarchar(100) NULL,                        -- Col 13: CORREO (Opcional)
        [NumeroTelefono] nvarchar(20) NULL,                 -- Col 14: NUMEROTELEFONO (Opcional)
        
        -- Campos de control interno
        [PerfilId] int NOT NULL,
        [EsActivo] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        
        CONSTRAINT [PK_Polizas] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Polizas_NumeroPoliza] UNIQUE ([NumeroPoliza]),
        CONSTRAINT [FK_Polizas_Perfiles] FOREIGN KEY ([PerfilId]) REFERENCES [Perfiles]([Id]),
        CONSTRAINT [CK_Polizas_Moneda] CHECK ([Moneda] IN ('CRC', 'USD', 'EUR')),
        CONSTRAINT [CK_Polizas_Prima] CHECK ([Prima] >= 0)
    );
    PRINT '✓ Tabla Polizas creada con soporte para 14 columnas de Excel.';
END
ELSE
BEGIN
    -- Si la tabla existe, verificar y agregar columnas faltantes
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'NumeroCedula')
    BEGIN
        ALTER TABLE [Polizas] ADD [NumeroCedula] nvarchar(20) NULL;
        PRINT '✓ Columna NumeroCedula agregada a Polizas.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Año')
    BEGIN
        ALTER TABLE [Polizas] ADD [Año] nvarchar(4) NULL;
        PRINT '✓ Columna Año agregada a Polizas.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'Correo')
    BEGIN
        ALTER TABLE [Polizas] ADD [Correo] nvarchar(100) NULL;
        PRINT '✓ Columna Correo agregada a Polizas.';
    END
    
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Polizas') AND name = 'NumeroTelefono')
    BEGIN
        ALTER TABLE [Polizas] ADD [NumeroTelefono] nvarchar(20) NULL;
        PRINT '✓ Columna NumeroTelefono agregada a Polizas.';
    END
END
GO

-- ===================================
-- 8. TABLA COBROS
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cobros' AND xtype='U')
BEGIN
    CREATE TABLE [Cobros] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PolizaId] int NOT NULL,
        [NumeroCobro] nvarchar(50) NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [FechaPago] datetime2 NULL,
        [Monto] decimal(18,2) NOT NULL,
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',
        [Estado] nvarchar(50) NOT NULL,
        [Notas] nvarchar(500) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Cobros] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cobros_Polizas] FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id]) ON DELETE CASCADE,
        CONSTRAINT [CK_Cobros_Monto] CHECK ([Monto] >= 0)
    );
    PRINT '✓ Tabla Cobros creada.';
END
GO

-- ===================================
-- 9. TABLA RECLAMOS
-- ===================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reclamos' AND xtype='U')
BEGIN
    CREATE TABLE [Reclamos] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PolizaId] int NOT NULL,
        [NumeroReclamo] nvarchar(50) NOT NULL,
        [FechaReclamo] datetime2 NOT NULL DEFAULT GETDATE(),
        [Descripcion] nvarchar(1000) NOT NULL,
        [MontoReclamado] decimal(18,2) NOT NULL,
        [MontoAprobado] decimal(18,2) NULL,
        [Estado] nvarchar(50) NOT NULL,
        [FechaResolucion] datetime2 NULL,
        [NotasResolucion] nvarchar(1000) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(100) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(100) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Reclamos] PRIMARY KEY ([Id]),
        CONSTRAINT [UK_Reclamos_NumeroReclamo] UNIQUE ([NumeroReclamo]),
        CONSTRAINT [FK_Reclamos_Polizas] FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id])
    );
    PRINT '✓ Tabla Reclamos creada.';
END
GO

-- ===================================
-- 10. ÍNDICES PARA OPTIMIZACIÓN
-- ===================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_PerfilId')
    CREATE INDEX [IX_Polizas_PerfilId] ON [Polizas]([PerfilId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_NumeroPoliza')
    CREATE INDEX [IX_Polizas_NumeroPoliza] ON [Polizas]([NumeroPoliza]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_NumeroCedula')
    CREATE INDEX [IX_Polizas_NumeroCedula] ON [Polizas]([NumeroCedula]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_FechaVigencia')
    CREATE INDEX [IX_Polizas_FechaVigencia] ON [Polizas]([FechaVigencia]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Cobros_PolizaId')
    CREATE INDEX [IX_Cobros_PolizaId] ON [Cobros]([PolizaId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Reclamos_PolizaId')
    CREATE INDEX [IX_Reclamos_PolizaId] ON [Reclamos]([PolizaId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_RoleId')
    CREATE INDEX [IX_Users_RoleId] ON [Users]([RoleId]);

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_PerfilId')
    CREATE INDEX [IX_Clientes_PerfilId] ON [Clientes]([PerfilId]);

PRINT '✓ Índices creados para optimización.';
GO

-- ===================================
-- 11. DATOS INICIALES - ROLES
-- ===================================
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Admin')
BEGIN
    INSERT INTO [Roles] ([Name], [Description], [IsActive], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
        ('Admin', 'Administrador del sistema con acceso total', 1, GETDATE(), 'System', 0),
        ('Agente', 'Agente de seguros con acceso a pólizas', 1, GETDATE(), 'System', 0),
        ('Usuario', 'Usuario básico con permisos limitados', 1, GETDATE(), 'System', 0);
    PRINT '✓ Roles iniciales insertados.';
END
GO

-- ===================================
-- 12. DATOS INICIALES - USUARIO ADMIN
-- ===================================
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [UserName] = 'admin')
BEGIN
    DECLARE @RoleId INT = (SELECT [Id] FROM [Roles] WHERE [Name] = 'Admin');
    
    INSERT INTO [Users] ([UserName], [Email], [FirstName], [LastName], [PasswordHash], [IsActive], [RoleId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
        ('admin', 'admin@sinseg.com', 'Administrador', 'Sistema', 
         'AQAAAAIAAYagAAAAEFD/vIq7rNgRTUGPJQaQCFPCzJhLMOz9xPmBQPM+YF0CgQ5KT8sVp9HQ4C6qZhvMQQ==', -- Password: Admin123!
         1, @RoleId, GETDATE(), 'System', 0);
    PRINT '✓ Usuario admin creado (Password: Admin123!).';
END
GO

-- ===================================
-- 13. DATOS INICIALES - PERFILES
-- ===================================
IF NOT EXISTS (SELECT 1 FROM [Perfiles])
BEGIN
    INSERT INTO [Perfiles] ([Nombre], [Descripcion], [EsActivo], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
        ('Perfil General', 'Perfil general para clientes y pólizas', 1, GETDATE(), 'System', 0),
        ('Perfil Corporativo', 'Perfil para clientes corporativos', 1, GETDATE(), 'System', 0);
    PRINT '✓ Perfiles iniciales insertados.';
END
GO

-- ===================================
-- 14. DATOS DE PRUEBA - POLIZAS
-- ===================================
IF NOT EXISTS (SELECT 1 FROM [Polizas])
BEGIN
    DECLARE @PerfilId INT = (SELECT TOP 1 [Id] FROM [Perfiles] WHERE [Nombre] = 'Perfil General');
    
    INSERT INTO [Polizas] 
    ([NumeroPoliza], [NombreAsegurado], [NumeroCedula], [Prima], [Moneda], [FechaVigencia], 
     [Frecuencia], [Aseguradora], [Placa], [Marca], [Modelo], [Año], [Correo], [NumeroTelefono],
     [PerfilId], [EsActivo], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
    -- Póliza 1: Auto completa
    ('POL-2024-001', 'Juan Carlos Pérez García', '1-1234-5678', 150000.00, 'CRC', '2024-12-31',
     'MENSUAL', 'SINSEG Seguros', 'ABC123', 'Toyota', 'Corolla', '2020', 'juan.perez@email.com', '+506 8888-1234',
     @PerfilId, 1, GETDATE(), 'System', 0),
    
    -- Póliza 2: Auto completa
    ('POL-2024-002', 'María Elena García López', '2-2345-6789', 250000.00, 'CRC', '2025-01-15',
     'TRIMESTRAL', 'SINSEG Seguros', 'DEF456', 'Honda', 'Civic', '2019', 'maria.garcia@email.com', '+506 7777-5678',
     @PerfilId, 1, GETDATE(), 'System', 0),
    
    -- Póliza 3: Vida (sin datos vehiculares)
    ('POL-2024-003', 'Carlos Roberto Ramírez Solís', '3-3456-7890', 180000.00, 'USD', '2024-06-01',
     'ANUAL', 'Seguros Vida Plus', NULL, NULL, NULL, NULL, 'carlos.ramirez@email.com', '+506 6666-9999',
     @PerfilId, 1, GETDATE(), 'System', 0),
    
    -- Póliza 4: Auto sin contacto
    ('POL-2024-004', 'Ana Sofía Vargas Chacón', '4-4567-8901', 95000.00, 'CRC', '2025-03-10',
     'MENSUAL', 'INS Instituto Nacional de Seguros', 'GHI789', 'Nissan', 'Sentra', '2021', NULL, NULL,
     @PerfilId, 1, GETDATE(), 'System', 0),
    
    -- Póliza 5: Auto completa
    ('POL-2024-005', 'Luis Fernando Morales Quesada', '5-5678-9012', 320000.00, 'CRC', '2024-11-20',
     'SEMESTRAL', 'ASSA Compañía de Seguros', 'JKL012', 'Mazda', 'CX-5', '2022', 'luis.morales@email.com', '+506 8888-7777',
     @PerfilId, 1, GETDATE(), 'System', 0);
    
    PRINT '✓ Pólizas de prueba insertadas (5 registros).';
END
GO

-- ===================================
-- 15. VERIFICACIÓN FINAL
-- ===================================
PRINT '';
PRINT '========================================';
PRINT 'RESUMEN DE LA BASE DE DATOS';
PRINT '========================================';
SELECT 'Roles' AS Tabla, COUNT(*) AS Registros FROM [Roles]
UNION ALL
SELECT 'Users', COUNT(*) FROM [Users]
UNION ALL
SELECT 'Perfiles', COUNT(*) FROM [Perfiles]
UNION ALL
SELECT 'Clientes', COUNT(*) FROM [Clientes]
UNION ALL
SELECT 'Polizas', COUNT(*) FROM [Polizas]
UNION ALL
SELECT 'Cobros', COUNT(*) FROM [Cobros]
UNION ALL
SELECT 'Reclamos', COUNT(*) FROM [Reclamos];

PRINT '';
PRINT '========================================';
PRINT '✓ BASE DE DATOS CONFIGURADA EXITOSAMENTE';
PRINT '========================================';
PRINT '';
PRINT '📊 DETALLES DE LA CONFIGURACIÓN:';
PRINT '  • Base de datos: SinsegAppDb';
PRINT '  • Tablas creadas: 8';
PRINT '  • Usuario admin: admin@sinseg.com';
PRINT '  • Password admin: Admin123!';
PRINT '  • Pólizas de prueba: 5 registros';
PRINT '';
PRINT '📁 FORMATO EXCEL SOPORTADO (14 columnas):';
PRINT '  1. POLIZA           - Número de póliza (obligatorio)';
PRINT '  2. NOMBRE           - Nombre del asegurado (obligatorio)';
PRINT '  3. NUMEROCEDULA     - Cédula del asegurado (obligatorio)';
PRINT '  4. PRIMA            - Valor de la prima (obligatorio)';
PRINT '  5. MONEDA           - CRC/USD/EUR (obligatorio)';
PRINT '  6. FECHA            - Fecha vigencia (obligatorio)';
PRINT '  7. FRECUENCIA       - Frecuencia de pago (obligatorio)';
PRINT '  8. ASEGURADORA      - Nombre aseguradora (obligatorio)';
PRINT '  9. PLACA            - Placa vehículo (opcional)';
PRINT '  10. MARCA           - Marca vehículo (opcional)';
PRINT '  11. MODELO          - Modelo vehículo (opcional)';
PRINT '  12. AÑO             - Año vehículo (opcional)';
PRINT '  13. CORREO          - Email asegurado (opcional)';
PRINT '  14. NUMEROTELEFONO  - Teléfono asegurado (opcional)';
PRINT '';
PRINT '✅ La base de datos está lista para recibir archivos Excel!';
PRINT '========================================';
GO
