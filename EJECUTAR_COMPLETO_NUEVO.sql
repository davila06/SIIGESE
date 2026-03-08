-- =============================================
-- SCRIPT COMPLETO DE CREACIÓN DE BASE DE DATOS
-- Sistema SINSEG - Producción
-- =============================================

USE SiinadsegProdDB;
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
-- 3. TABLA USERROLES
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_UserRoles_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id]),
        CONSTRAINT [FK_UserRoles_Roles] FOREIGN KEY ([RoleId]) REFERENCES [Roles]([Id])
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
        [Codigo] nvarchar(20) NOT NULL,
        [RazonSocial] nvarchar(200) NOT NULL,
        [NombreComercial] nvarchar(200) NOT NULL,
        [NIT] nvarchar(20) NOT NULL,
        [Telefono] nvarchar(20) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [Direccion] nvarchar(300) NOT NULL,
        [Ciudad] nvarchar(100) NOT NULL,
        [Departamento] nvarchar(100) NOT NULL,
        [Pais] nvarchar(100) NOT NULL,
        [EsActivo] bit NOT NULL DEFAULT 1,
        [FechaRegistro] datetime2 NULL,
        [PerfilId] int NOT NULL DEFAULT 1,
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
        [Modalidad] nvarchar(50) NOT NULL,
        [NombreAsegurado] nvarchar(200) NOT NULL,
        [Prima] decimal(18,2) NOT NULL,
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',
        [FechaVigencia] datetime2 NOT NULL,
        [Frecuencia] nvarchar(50) NOT NULL,
        [Aseguradora] nvarchar(100) NOT NULL,
        [Placa] nvarchar(10) NOT NULL,
        [Marca] nvarchar(50) NOT NULL,
        [Modelo] nvarchar(50) NOT NULL,
        [PerfilId] int NOT NULL DEFAULT 1,
        [EsActivo] bit NOT NULL DEFAULT 1,
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
        [NumeroRecibo] nvarchar(50) NOT NULL,
        [PolizaId] int NOT NULL,
        [NumeroPoliza] nvarchar(50) NOT NULL,
        [ClienteNombre] nvarchar(100) NOT NULL,
        [ClienteApellido] nvarchar(100) NOT NULL,
        [FechaVencimiento] datetime2 NOT NULL,
        [FechaCobro] datetime2 NULL,
        [MontoTotal] decimal(18,2) NOT NULL,
        [MontoCobrado] decimal(18,2) NULL,
        [Estado] int NOT NULL DEFAULT 0,
        [MetodoPago] int NULL,
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',
        [Observaciones] nvarchar(500) NULL,
        [UsuarioCobroId] int NULL,
        [UsuarioCobroNombre] nvarchar(100) NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Cobros] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cobros_Polizas] FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id]),
        CONSTRAINT [FK_Cobros_Users] FOREIGN KEY ([UsuarioCobroId]) REFERENCES [Users]([Id])
    );
    
    CREATE UNIQUE INDEX [IX_Cobros_NumeroRecibo] ON [Cobros]([NumeroRecibo]);
    CREATE INDEX [IX_Cobros_PolizaId] ON [Cobros]([PolizaId]);
    CREATE INDEX [IX_Cobros_Estado] ON [Cobros]([Estado]);
    CREATE INDEX [IX_Cobros_FechaVencimiento] ON [Cobros]([FechaVencimiento]);
    
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
        [NumeroReclamo] nvarchar(50) NOT NULL,
        [PolizaId] int NOT NULL,
        [NumeroPoliza] nvarchar(50) NOT NULL,
        [ClienteNombre] nvarchar(100) NOT NULL,
        [ClienteApellido] nvarchar(100) NOT NULL,
        [NombreAsegurado] nvarchar(200) NOT NULL,
        [FechaReclamo] datetime2 NOT NULL DEFAULT GETDATE(),
        [FechaLimiteRespuesta] datetime2 NULL,
        [FechaResolucion] datetime2 NULL,
        [TipoReclamo] int NOT NULL DEFAULT 0,
        [Estado] int NOT NULL DEFAULT 0,
        [Prioridad] int NOT NULL DEFAULT 1,
        [Descripcion] nvarchar(max) NOT NULL,
        [MontoReclamado] decimal(18,2) NOT NULL,
        [MontoAprobado] decimal(18,2) NULL,
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',
        [Observaciones] nvarchar(max) NULL,
        [DocumentosAdjuntos] nvarchar(max) NULL,
        [UsuarioAsignadoId] int NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Reclamos] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Reclamos_Polizas] FOREIGN KEY ([PolizaId]) REFERENCES [Polizas]([Id]),
        CONSTRAINT [FK_Reclamos_Users] FOREIGN KEY ([UsuarioAsignadoId]) REFERENCES [Users]([Id])
    );
    
    CREATE UNIQUE INDEX [IX_Reclamos_NumeroReclamo] ON [Reclamos]([NumeroReclamo]);
    CREATE INDEX [IX_Reclamos_PolizaId] ON [Reclamos]([PolizaId]);
    CREATE INDEX [IX_Reclamos_Estado] ON [Reclamos]([Estado]);
    CREATE INDEX [IX_Reclamos_FechaReclamo] ON [Reclamos]([FechaReclamo]);
    
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
        [NumeroPoliza] nvarchar(50) NOT NULL,
        [NombreAsegurado] nvarchar(200) NOT NULL,
        [NombreSolicitante] nvarchar(200) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [TipoSeguro] nvarchar(50) NOT NULL,
        [Prima] decimal(18,2) NOT NULL,
        [Moneda] nvarchar(3) NOT NULL DEFAULT 'CRC',
        [FechaVigencia] datetime2 NOT NULL,
        [FechaCotizacion] datetime2 NOT NULL DEFAULT GETDATE(),
        [FechaCreacion] datetime2 NOT NULL DEFAULT GETDATE(),
        [FechaActualizacion] datetime2 NULL,
        [Aseguradora] nvarchar(100) NOT NULL,
        [Estado] nvarchar(50) NOT NULL DEFAULT 'Pendiente',
        [UsuarioId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_Cotizaciones] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Cotizaciones_Users] FOREIGN KEY ([UsuarioId]) REFERENCES [Users]([Id])
    );
    
    CREATE UNIQUE INDEX [IX_Cotizaciones_NumeroCotizacion] ON [Cotizaciones]([NumeroCotizacion]);
    CREATE INDEX [IX_Cotizaciones_Email] ON [Cotizaciones]([Email]);
    CREATE INDEX [IX_Cotizaciones_Estado] ON [Cotizaciones]([Estado]);
    CREATE INDEX [IX_Cotizaciones_FechaCotizacion] ON [Cotizaciones]([FechaCotizacion]);
    
    PRINT 'Tabla Cotizaciones creada.';
END
GO

-- =============================================
-- 9. TABLA DATARECORDS
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DataRecords' AND xtype='U')
BEGIN
    CREATE TABLE [DataRecords] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [FileType] nvarchar(50) NOT NULL,
        [FileSize] bigint NOT NULL,
        [TotalRecords] int NOT NULL,
        [ProcessedRecords] int NOT NULL,
        [ErrorRecords] int NOT NULL,
        [Status] nvarchar(50) NOT NULL,
        [ProcessedAt] datetime2 NOT NULL,
        [ErrorDetails] nvarchar(max) NULL,
        [UploadedByUserId] int NOT NULL,
        [PerfilId] int NOT NULL DEFAULT 1,
        [CreatedAt] datetime2 NOT NULL DEFAULT GETDATE(),
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_DataRecords] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_DataRecords_Users] FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users]([Id])
    );
    PRINT 'Tabla DataRecords creada.';
END
GO

-- =============================================
-- 10. TABLA EMAILCONFIG (para notificaciones)
-- =============================================
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='EmailConfig' AND xtype='U')
BEGIN
    CREATE TABLE [EmailConfig] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [SmtpServer] nvarchar(200) NOT NULL,
        [SmtpPort] int NOT NULL,
        [SmtpUsername] nvarchar(200) NOT NULL,
        [SmtpPassword] nvarchar(max) NOT NULL,
        [FromEmail] nvarchar(200) NOT NULL,
        [FromName] nvarchar(200) NOT NULL,
        [EnableSsl] bit NOT NULL DEFAULT 1,
        [IsActive] bit NOT NULL DEFAULT 1,
        [PerfilId] int NOT NULL DEFAULT 1,
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
-- 11. TABLA PASSWORDRESETTOKENS
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
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL DEFAULT 'System',
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL DEFAULT 0,
        CONSTRAINT [PK_PasswordResetTokens] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_PasswordResetTokens_Users] FOREIGN KEY ([UserId]) REFERENCES [Users]([Id])
    );
    PRINT 'Tabla PasswordResetTokens creada.';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'TODAS LAS TABLAS HAN SIDO CREADAS';
PRINT '=============================================';
PRINT '';
PRINT 'Resumen de módulos:';
PRINT ' Usuarios y Roles (Users, Roles, UserRoles)';
PRINT ' Clientes (Clientes)';
PRINT ' Pólizas (Polizas)';
PRINT ' Cobros (Cobros)';
PRINT ' Reclamos (Reclamos)';
PRINT ' Cotizaciones (Cotizaciones)';
PRINT ' Notificaciones (EmailConfig)';
PRINT ' Configuración (DataRecords, PasswordResetTokens)';
PRINT '';
