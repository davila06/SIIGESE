-- Script para insertar usuario administrador en LocalDB
USE SinsegAppDb;

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

-- Obtener IDs para la asignación de roles
DECLARE @AdminRoleId INT = (SELECT Id FROM [Roles] WHERE [Name] = 'Admin');
DECLARE @AdminUserId INT = (SELECT Id FROM [Users] WHERE [Email] = 'admin@sinseg.com');

-- Asignar rol Admin al usuario administrador
IF NOT EXISTS (SELECT 1 FROM [UserRoles] WHERE [UserId] = @AdminUserId AND [RoleId] = @AdminRoleId)
BEGIN
    INSERT INTO [UserRoles] ([UserId], [RoleId], [IsActive], [CreatedAt], [CreatedBy], [IsDeleted])
    VALUES (@AdminUserId, @AdminRoleId, 1, GETUTCDATE(), 'System', 0);
    PRINT 'Rol Admin asignado al usuario administrador.';
END

PRINT 'Configuración inicial completada.';