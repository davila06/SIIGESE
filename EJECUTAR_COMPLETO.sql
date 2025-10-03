-- =============================================
-- Script maestro para ejecutar todos los scripts
-- =============================================

-- INSTRUCCIONES:
-- 1. Abrir SQL Server Management Studio (SSMS)
-- 2. Conectarse a la instancia de SQL Server Express
-- 3. Ejecutar este script completo

PRINT '=============================================';
PRINT 'INICIANDO CREACIÓN DE BASE DE DATOS SINSEG';
PRINT 'Fecha: ' + CONVERT(VARCHAR, GETDATE(), 120);
PRINT '=============================================';
PRINT '';

-- Paso 1: Crear la base de datos
PRINT 'PASO 1: Creando base de datos...';
GO

USE master;
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SinsegAppDb')
BEGIN
    CREATE DATABASE SinsegAppDb;
    PRINT 'Base de datos SinsegAppDb creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La base de datos SinsegAppDb ya existe.';
END
GO

USE SinsegAppDb;
GO

-- Crear tabla de control de migraciones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END
GO

-- Paso 2: Crear tablas
PRINT '';
PRINT 'PASO 2: Creando tablas...';
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

-- Paso 3: Crear índices
PRINT '';
PRINT 'PASO 3: Creando índices...';
GO

-- Índices para Clientes
CREATE UNIQUE INDEX [IX_Clientes_Codigo] ON [Clientes] ([Codigo]);
CREATE UNIQUE INDEX [IX_Clientes_NIT] ON [Clientes] ([NIT]);
CREATE INDEX [IX_Clientes_PerfilId] ON [Clientes] ([PerfilId]);

-- Índices para Polizas
CREATE UNIQUE INDEX [IX_Polizas_NumeroPoliza] ON [Polizas] ([NumeroPoliza]);
CREATE INDEX [IX_Polizas_PerfilId] ON [Polizas] ([PerfilId]);
CREATE INDEX [IX_Polizas_Aseguradora] ON [Polizas] ([Aseguradora]);
CREATE INDEX [IX_Polizas_Placa] ON [Polizas] ([Placa]);

-- Índices para Roles
CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);

-- Índices para Users
CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);

-- Índices para DataRecords
CREATE INDEX [IX_DataRecords_UploadedByUserId] ON [DataRecords] ([UploadedByUserId]);

-- Índices para UserRoles
CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);

PRINT 'Índices creados exitosamente.';
GO

-- Paso 4: Crear foreign keys
PRINT '';
PRINT 'PASO 4: Creando foreign keys...';
GO

ALTER TABLE [DataRecords] 
ADD CONSTRAINT [FK_DataRecords_Users_UploadedByUserId] 
FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]);

ALTER TABLE [UserRoles] 
ADD CONSTRAINT [FK_UserRoles_Users_UserId] 
FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;

ALTER TABLE [UserRoles] 
ADD CONSTRAINT [FK_UserRoles_Roles_RoleId] 
FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;

PRINT 'Foreign keys creadas exitosamente.';
GO

-- Paso 5: Insertar datos iniciales
PRINT '';
PRINT 'PASO 5: Insertando datos iniciales...';
GO

-- Insertar Roles
INSERT INTO [Roles] ([Name], [Description], [IsActive], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES 
    ('Admin', 'Administrador del sistema', 1, GETUTCDATE(), 'System', 0),
    ('DataLoader', 'Cargador de datos', 1, GETUTCDATE(), 'System', 0),
    ('User', 'Usuario estándar', 1, GETUTCDATE(), 'System', 0);

-- Insertar Usuario Administrador
INSERT INTO [Users] ([UserName], [Email], [FirstName], [LastName], [PasswordHash], [IsActive], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES (
    'admin', 
    'admin@sinseg.com', 
    'Administrador', 
    'Sistema', 
    '$2a$11$Xed1MsTJ.11zGnnvwDLGaeGav13ki.M4gEB5LSGg/vhxtWT5FC8Xm', -- password123
    1, 
    GETUTCDATE(), 
    'System', 
    0
);

-- Asignar rol Admin al usuario
DECLARE @AdminUserId INT = (SELECT Id FROM [Users] WHERE Email = 'admin@sinseg.com');
DECLARE @AdminRoleId INT = (SELECT Id FROM [Roles] WHERE Name = 'Admin');

INSERT INTO [UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [IsDeleted])
VALUES (@AdminUserId, @AdminRoleId, GETUTCDATE(), 'System', 0);

-- Insertar registro de migración
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES ('20251001170500_UpdateAdminCredentials', '8.0.0');

PRINT 'Datos iniciales insertados exitosamente.';
GO

PRINT '';
PRINT '============================================';
PRINT 'BASE DE DATOS CREADA EXITOSAMENTE!';
PRINT '';
PRINT 'CREDENCIALES DE ACCESO:';
PRINT 'Email: admin@sinseg.com';
PRINT 'Password: password123';
PRINT '';
PRINT 'La aplicación ya puede conectarse a:';
PRINT 'Server=(local)\SQLEXPRESS;Database=SinsegAppDb;Trusted_Connection=True;';
PRINT '============================================';
GO