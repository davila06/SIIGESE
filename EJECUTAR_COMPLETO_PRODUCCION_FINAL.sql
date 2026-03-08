-- =============================================
-- SCRIPT COMPLETO DE CREACIÓN DE BASE DE DATOS
-- Sistema SINSEG - PRODUCCIÓN FINAL
-- =============================================

USE SiinadsegProdFinal;
GO

-- =============================================
-- 1. TABLA ROLES
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [Roles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Roles creada.';
END
GO

-- =============================================
-- 2. TABLA USERS
-- =============================================
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
        [LastLoginAt] datetime2 NULL,
        [LastPasswordChangeAt] datetime2 NULL,
        [RequiresPasswordChange] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Users creada.';
END
GO

-- =============================================
-- 3. TABLA USER_ROLES (Relación muchos a muchos)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [UserRoles] (
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([UserId], [RoleId])
    );
    PRINT 'Tabla UserRoles creada.';
END
GO

-- =============================================
-- 4. TABLA CLIENTES
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Clientes' AND xtype='U')
BEGIN
    CREATE TABLE [Clientes] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [NumeroCliente] nvarchar(50) NOT NULL,
        [Nombre] nvarchar(200) NOT NULL,
        [RFC] nvarchar(13) NULL,
        [Telefono] nvarchar(20) NULL,
        [Email] nvarchar(100) NULL,
        [Direccion] nvarchar(500) NULL,
        [Ciudad] nvarchar(100) NULL,
        [Estado] nvarchar(100) NULL,
        [CodigoPostal] nvarchar(10) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Clientes creada.';
END
GO

-- =============================================
-- 5. TABLA POLIZAS
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Polizas' AND xtype='U')
BEGIN
    CREATE TABLE [Polizas] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [NumeroPoliza] nvarchar(50) NOT NULL,
        [ClienteId] int NULL,
        [TipoSeguro] nvarchar(100) NULL,
        [Aseguradora] nvarchar(100) NULL,
        [FechaEmision] date NULL,
        [FechaVencimiento] date NULL,
        [PrimaTotal] decimal(18,2) NULL,
        [FormaPago] nvarchar(50) NULL,
        [Periodicidad] nvarchar(50) NULL,
        [Estado] nvarchar(50) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Polizas] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Polizas creada.';
END
GO

-- =============================================
-- 6. TABLA COBROS
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cobros' AND xtype='U')
BEGIN
    CREATE TABLE [Cobros] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PolizaId] int NOT NULL,
        [FechaCobro] date NOT NULL,
        [MontoCobrado] decimal(18,2) NOT NULL,
        [MetodoPago] nvarchar(50) NULL,
        [Referencia] nvarchar(100) NULL,
        [Estado] nvarchar(50) NULL,
        [Observaciones] nvarchar(500) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Cobros] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Cobros creada.';
END
GO

-- =============================================
-- 7. TABLA RECLAMOS
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reclamos' AND xtype='U')
BEGIN
    CREATE TABLE [Reclamos] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [PolizaId] int NOT NULL,
        [NumeroReclamo] nvarchar(50) NOT NULL,
        [FechaReclamo] date NOT NULL,
        [TipoReclamo] nvarchar(100) NULL,
        [MontoReclamado] decimal(18,2) NULL,
        [MontoAprobado] decimal(18,2) NULL,
        [Estado] nvarchar(50) NULL,
        [Descripcion] nvarchar(max) NULL,
        [FechaResolucion] date NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Reclamos] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Reclamos creada.';
END
GO

-- =============================================
-- 8. TABLA COTIZACIONES
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Cotizaciones' AND xtype='U')
BEGIN
    CREATE TABLE [Cotizaciones] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [NumeroCotizacion] nvarchar(50) NOT NULL,
        [ClienteId] int NULL,
        [TipoSeguro] nvarchar(100) NULL,
        [FechaCotizacion] date NOT NULL,
        [FechaVencimientoCotizacion] date NULL,
        [MontoAsegurado] decimal(18,2) NULL,
        [PrimaEstimada] decimal(18,2) NULL,
        [Estado] nvarchar(50) NULL,
        [Observaciones] nvarchar(max) NULL,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Cotizaciones] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Cotizaciones creada.';
END
GO

-- =============================================
-- 9. TABLA EMAIL_CONFIG
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EmailConfig' AND xtype='U')
BEGIN
    CREATE TABLE [EmailConfig] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [SmtpServer] nvarchar(100) NOT NULL,
        [SmtpPort] int NOT NULL,
        [SmtpUsername] nvarchar(100) NOT NULL,
        [SmtpPassword] nvarchar(max) NOT NULL,
        [SenderEmail] nvarchar(100) NOT NULL,
        [SenderName] nvarchar(100) NOT NULL,
        [EnableSsl] bit NOT NULL DEFAULT 1,
        [IsDefault] bit NOT NULL DEFAULT 0,
        [IsActive] bit NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_EmailConfig] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla EmailConfig creada.';
END
GO

-- =============================================
-- 10. TABLA DATA_RECORDS (Para excel uploads)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DataRecords' AND xtype='U')
BEGIN
    CREATE TABLE [DataRecords] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [UploadDate] datetime2 NOT NULL DEFAULT GETDATE(),
        [RecordType] nvarchar(50) NOT NULL,
        [RecordData] nvarchar(max) NOT NULL,
        [IsProcessed] bit NOT NULL DEFAULT 0,
        [ProcessedDate] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_DataRecords] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla DataRecords creada.';
END
GO

-- =============================================
-- 11. TABLA PASSWORD_RESET_TOKENS
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='PasswordResetTokens' AND xtype='U')
BEGIN
    CREATE TABLE [PasswordResetTokens] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [Token] nvarchar(max) NOT NULL,
        [ExpiresAt] datetime2 NOT NULL,
        [IsUsed] bit NOT NULL DEFAULT 0,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UsedAt] datetime2 NULL,
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla PasswordResetTokens creada.';
END
GO

-- =============================================
-- CREACIÓN DE ÍNDICES
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Users_Email creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Users_UserName' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_UserName ON Users(UserName) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Users_UserName creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Clientes_NumeroCliente' AND object_id = OBJECT_ID('Clientes'))
BEGIN
    CREATE UNIQUE INDEX IX_Clientes_NumeroCliente ON Clientes(NumeroCliente) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Clientes_NumeroCliente creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Polizas_NumeroPoliza' AND object_id = OBJECT_ID('Polizas'))
BEGIN
    CREATE UNIQUE INDEX IX_Polizas_NumeroPoliza ON Polizas(NumeroPoliza) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Polizas_NumeroPoliza creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Reclamos_NumeroReclamo' AND object_id = OBJECT_ID('Reclamos'))
BEGIN
    CREATE UNIQUE INDEX IX_Reclamos_NumeroReclamo ON Reclamos(NumeroReclamo) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Reclamos_NumeroReclamo creado.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Cotizaciones_NumeroCotizacion' AND object_id = OBJECT_ID('Cotizaciones'))
BEGIN
    CREATE UNIQUE INDEX IX_Cotizaciones_NumeroCotizacion ON Cotizaciones(NumeroCotizacion) WHERE IsDeleted = 0;
    PRINT 'Índice IX_Cotizaciones_NumeroCotizacion creado.';
END
GO

-- =============================================
-- FOREIGN KEYS
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_UserRoles_Users_UserId')
BEGIN
    ALTER TABLE [UserRoles] 
    ADD CONSTRAINT [FK_UserRoles_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE;
    PRINT 'FK UserRoles->Users creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_UserRoles_Roles_RoleId')
BEGIN
    ALTER TABLE [UserRoles] 
    ADD CONSTRAINT [FK_UserRoles_Roles_RoleId] 
    FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id]) ON DELETE CASCADE;
    PRINT 'FK UserRoles->Roles creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_Polizas_Clientes_ClienteId')
BEGIN
    ALTER TABLE [Polizas] 
    ADD CONSTRAINT [FK_Polizas_Clientes_ClienteId] 
    FOREIGN KEY ([ClienteId]) REFERENCES [Clientes]([Id]) ON DELETE SET NULL;
    PRINT 'FK Polizas->Clientes creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_Cobros_Polizas_PolizaId')
BEGIN
    ALTER TABLE [Cobros] 
    ADD CONSTRAINT [FK_Cobros_Polizas_PolizaId] 
    FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id]) ON DELETE CASCADE;
    PRINT 'FK Cobros->Polizas creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_Reclamos_Polizas_PolizaId')
BEGIN
    ALTER TABLE [Reclamos] 
    ADD CONSTRAINT [FK_Reclamos_Polizas_PolizaId] 
    FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id]) ON DELETE CASCADE;
    PRINT 'FK Reclamos->Polizas creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_Cotizaciones_Clientes_ClienteId')
BEGIN
    ALTER TABLE [Cotizaciones] 
    ADD CONSTRAINT [FK_Cotizaciones_Clientes_ClienteId] 
    FOREIGN KEY ([ClienteId]) REFERENCES [Clientes]([Id]) ON DELETE SET NULL;
    PRINT 'FK Cotizaciones->Clientes creada.';
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name='FK_PasswordResetTokens_Users_UserId')
BEGIN
    ALTER TABLE [PasswordResetTokens] 
    ADD CONSTRAINT [FK_PasswordResetTokens_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]) ON DELETE CASCADE;
    PRINT 'FK PasswordResetTokens->Users creada.';
END
GO

-- =============================================
-- DATOS INICIALES - ROLES
-- =============================================
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES 
        ('Admin', 'Administrador del sistema con acceso completo', 1, GETDATE(), 'System', 0),
        ('User', 'Usuario estándar con permisos básicos', 1, GETDATE(), 'System', 0),
        ('DataLoader', 'Usuario con permisos para cargar datos desde Excel', 1, GETDATE(), 'System', 0),
        ('Viewer', 'Usuario de solo lectura', 1, GETDATE(), 'System', 0);
    PRINT 'Roles iniciales insertados.';
END
GO

-- =============================================
-- DATOS INICIALES - USUARIO ADMIN
-- =============================================
-- Password: Admin123!
-- Hash generado con BCrypt
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@siinadseg.com')
BEGIN
    DECLARE @AdminUserId INT;
    
    INSERT INTO Users (UserName, Email, FirstName, LastName, PasswordHash, IsActive, RequiresPasswordChange, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('admin', 'admin@siinadseg.com', 'Administrador', 'Sistema', '$2a$11$XBv.zQdJY9KJqN0YCMvWjuqBvPPvzBz7wJG7kNLVxZvVVLkHmLGV2', 1, 0, GETDATE(), 'System', 0);
    
    SET @AdminUserId = SCOPE_IDENTITY();
    
    -- Asignar rol Admin al usuario
    INSERT INTO UserRoles (UserId, RoleId)
    SELECT @AdminUserId, Id FROM Roles WHERE Name = 'Admin';
    
    PRINT 'Usuario administrador creado con éxito.';
    PRINT 'Email: admin@siinadseg.com';
    PRINT 'Password: Admin123!';
END
GO

PRINT '=============================================';
PRINT 'BASE DE DATOS CREADA EXITOSAMENTE';
PRINT '=============================================';
PRINT '';
PRINT 'Credenciales de acceso:';
PRINT '  Email: admin@siinadseg.com';
PRINT '  Password: Admin123!';
PRINT '';
PRINT 'Estadísticas:';
SELECT 
    'Tablas Creadas' as Tipo,
    COUNT(*) as Total 
FROM INFORMATION_SCHEMA.TABLES 
WHERE TABLE_TYPE='BASE TABLE' AND TABLE_NAME != '__EFMigrationsHistory'
UNION ALL
SELECT 'Roles', COUNT(*) FROM Roles WHERE IsDeleted=0
UNION ALL
SELECT 'Usuarios', COUNT(*) FROM Users WHERE IsDeleted=0;
GO
