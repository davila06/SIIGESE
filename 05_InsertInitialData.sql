-- =============================================
-- Script para insertar datos iniciales
-- =============================================

USE SinsegAppDb;
GO

PRINT 'Insertando datos iniciales...';
GO

-- Insertar Roles iniciales
IF NOT EXISTS (SELECT 1 FROM [Roles] WHERE [Name] = 'Admin')
BEGIN
    INSERT INTO [Roles] ([Name], [Description], [IsActive], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES 
        ('Admin', 'Administrador del sistema', 1, GETUTCDATE(), 'System', 0),
        ('DataLoader', 'Cargador de datos', 1, GETUTCDATE(), 'System', 0),
        ('User', 'Usuario estándar', 1, GETUTCDATE(), 'System', 0);
    PRINT 'Roles iniciales insertados.';
END
ELSE
BEGIN
    PRINT 'Los roles ya existen en la base de datos.';
END
GO

-- Insertar Usuario Administrador
IF NOT EXISTS (SELECT 1 FROM [Users] WHERE [Email] = 'admin@sinseg.com')
BEGIN
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
    PRINT 'Usuario administrador insertado.';
END
ELSE
BEGIN
    PRINT 'El usuario administrador ya existe.';
END
GO

-- Asignar rol Admin al usuario administrador
IF NOT EXISTS (SELECT 1 FROM [UserRoles] ur 
               INNER JOIN [Users] u ON ur.UserId = u.Id 
               INNER JOIN [Roles] r ON ur.RoleId = r.Id 
               WHERE u.Email = 'admin@sinseg.com' AND r.Name = 'Admin')
BEGIN
    DECLARE @AdminUserId INT = (SELECT Id FROM [Users] WHERE Email = 'admin@sinseg.com');
    DECLARE @AdminRoleId INT = (SELECT Id FROM [Roles] WHERE Name = 'Admin');
    
    INSERT INTO [UserRoles] ([UserId], [RoleId], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@AdminUserId, @AdminRoleId, GETUTCDATE(), 'System', 0);
    PRINT 'Rol Admin asignado al usuario administrador.';
END
ELSE
BEGIN
    PRINT 'El usuario administrador ya tiene el rol Admin asignado.';
END
GO

-- Insertar registro en __EFMigrationsHistory
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251001170500_UpdateAdminCredentials')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251001170500_UpdateAdminCredentials', '8.0.0');
    PRINT 'Registro de migración insertado.';
END
GO

PRINT 'Datos iniciales insertados exitosamente.';
PRINT '';
PRINT '========================================';
PRINT 'CREDENCIALES DE ACCESO:';
PRINT 'Email: admin@sinseg.com';
PRINT 'Password: password123';
PRINT '========================================';
GO