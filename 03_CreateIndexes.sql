-- =============================================
-- Script para crear índices y constraints
-- =============================================

USE SinsegAppDb;
GO

PRINT 'Creando índices y constraints...';
GO

-- Índices para tabla Clientes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_Codigo')
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_Codigo] ON [Clientes] ([Codigo]);
    PRINT 'Índice IX_Clientes_Codigo creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_NIT')
BEGIN
    CREATE UNIQUE INDEX [IX_Clientes_NIT] ON [Clientes] ([NIT]);
    PRINT 'Índice IX_Clientes_NIT creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Clientes_PerfilId')
BEGIN
    CREATE INDEX [IX_Clientes_PerfilId] ON [Clientes] ([PerfilId]);
    PRINT 'Índice IX_Clientes_PerfilId creado.';
END
GO

-- Índices para tabla Polizas
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_NumeroPoliza')
BEGIN
    CREATE UNIQUE INDEX [IX_Polizas_NumeroPoliza] ON [Polizas] ([NumeroPoliza]);
    PRINT 'Índice IX_Polizas_NumeroPoliza creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_PerfilId')
BEGIN
    CREATE INDEX [IX_Polizas_PerfilId] ON [Polizas] ([PerfilId]);
    PRINT 'Índice IX_Polizas_PerfilId creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_Aseguradora')
BEGIN
    CREATE INDEX [IX_Polizas_Aseguradora] ON [Polizas] ([Aseguradora]);
    PRINT 'Índice IX_Polizas_Aseguradora creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Polizas_Placa')
BEGIN
    CREATE INDEX [IX_Polizas_Placa] ON [Polizas] ([Placa]);
    PRINT 'Índice IX_Polizas_Placa creado.';
END
GO

-- Índices para tabla Roles
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Roles_Name')
BEGIN
    CREATE UNIQUE INDEX [IX_Roles_Name] ON [Roles] ([Name]);
    PRINT 'Índice IX_Roles_Name creado.';
END
GO

-- Índices para tabla Users
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email')
BEGIN
    CREATE UNIQUE INDEX [IX_Users_Email] ON [Users] ([Email]);
    PRINT 'Índice IX_Users_Email creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_UserName')
BEGIN
    CREATE UNIQUE INDEX [IX_Users_UserName] ON [Users] ([UserName]);
    PRINT 'Índice IX_Users_UserName creado.';
END
GO

-- Índices para tabla DataRecords
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_DataRecords_UploadedByUserId')
BEGIN
    CREATE INDEX [IX_DataRecords_UploadedByUserId] ON [DataRecords] ([UploadedByUserId]);
    PRINT 'Índice IX_DataRecords_UploadedByUserId creado.';
END
GO

-- Índices para tabla UserRoles
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserRoles_RoleId')
BEGIN
    CREATE INDEX [IX_UserRoles_RoleId] ON [UserRoles] ([RoleId]);
    PRINT 'Índice IX_UserRoles_RoleId creado.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_UserRoles_UserId_RoleId')
BEGIN
    CREATE UNIQUE INDEX [IX_UserRoles_UserId_RoleId] ON [UserRoles] ([UserId], [RoleId]);
    PRINT 'Índice IX_UserRoles_UserId_RoleId creado.';
END
GO

PRINT 'Todos los índices han sido creados exitosamente.';
GO