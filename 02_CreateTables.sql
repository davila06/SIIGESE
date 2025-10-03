-- =============================================
-- Script para crear las tablas principales
-- =============================================

USE SinsegAppDb;
GO

-- Tabla Clientes
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
        [EsActivo] bit NOT NULL,
        [FechaRegistro] datetime2 NULL,
        [PerfilId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Clientes] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Clientes creada.';
END
GO

-- Tabla Polizas
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Polizas' AND xtype='U')
BEGIN
    CREATE TABLE [Polizas] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [NumeroPoliza] nvarchar(50) NOT NULL,
        [Modalidad] nvarchar(50) NOT NULL,
        [NombreAsegurado] nvarchar(200) NOT NULL,
        [Prima] decimal(18,2) NOT NULL,
        [Moneda] nvarchar(3) NOT NULL,
        [FechaVigencia] datetime2 NOT NULL,
        [Frecuencia] nvarchar(50) NOT NULL,
        [Aseguradora] nvarchar(100) NOT NULL,
        [Placa] nvarchar(10) NOT NULL,
        [Marca] nvarchar(50) NOT NULL,
        [Modelo] nvarchar(50) NOT NULL,
        [PerfilId] int NOT NULL,
        [EsActivo] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Polizas] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Polizas creada.';
END
GO

-- Tabla Roles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Roles' AND xtype='U')
BEGIN
    CREATE TABLE [Roles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [Name] nvarchar(50) NOT NULL,
        [Description] nvarchar(200) NOT NULL,
        [IsActive] bit NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Roles] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Roles creada.';
END
GO

-- Tabla Users
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE [Users] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserName] nvarchar(50) NOT NULL,
        [Email] nvarchar(100) NOT NULL,
        [FirstName] nvarchar(50) NOT NULL,
        [LastName] nvarchar(50) NOT NULL,
        [PasswordHash] nvarchar(max) NOT NULL,
        [IsActive] bit NOT NULL,
        [LastLoginAt] datetime2 NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla Users creada.';
END
GO

-- Tabla DataRecords
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
        [PerfilId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_DataRecords] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla DataRecords creada.';
END
GO

-- Tabla UserRoles
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE [UserRoles] (
        [Id] int IDENTITY(1,1) NOT NULL,
        [UserId] int NOT NULL,
        [RoleId] int NOT NULL,
        [CreatedAt] datetime2 NOT NULL,
        [UpdatedAt] datetime2 NULL,
        [CreatedBy] nvarchar(max) NOT NULL,
        [UpdatedBy] nvarchar(max) NULL,
        [IsDeleted] bit NOT NULL,
        CONSTRAINT [PK_UserRoles] PRIMARY KEY ([Id])
    );
    PRINT 'Tabla UserRoles creada.';
END
GO

PRINT 'Todas las tablas han sido creadas exitosamente.';
GO