-- Script para crear roles y usuario administrador inicial
-- Base de datos Azure: SiinadsegProdDB
-- Fecha: 2025-12-16

-- ==========================================
-- 1. CREAR ROLES INICIALES
-- ==========================================

PRINT '=== Creando Roles Iniciales ==='

-- Rol: Admin
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('Admin', 'Administrador del sistema con acceso completo', 1, GETUTCDATE(), 'SYSTEM', 0)
    PRINT '✓ Rol Admin creado'
END
ELSE
BEGIN
    PRINT '- Rol Admin ya existe'
END

-- Rol: User
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'User')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('User', 'Usuario estándar del sistema', 1, GETUTCDATE(), 'SYSTEM', 0)
    PRINT '✓ Rol User creado'
END
ELSE
BEGIN
    PRINT '- Rol User ya existe'
END

-- Rol: DataLoader
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'DataLoader')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('DataLoader', 'Cargador de datos masivos', 1, GETUTCDATE(), 'SYSTEM', 0)
    PRINT '✓ Rol DataLoader creado'
END
ELSE
BEGIN
    PRINT '- Rol DataLoader ya existe'
END

-- Rol: Viewer
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Viewer')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('Viewer', 'Usuario solo lectura', 1, GETUTCDATE(), 'SYSTEM', 0)
    PRINT '✓ Rol Viewer creado'
END
ELSE
BEGIN
    PRINT '- Rol Viewer ya existe'
END

PRINT ''
PRINT '=== Roles creados exitosamente ==='
PRINT ''

-- ==========================================
-- 2. CREAR USUARIO ADMINISTRADOR
-- ==========================================

PRINT '=== Creando Usuario Administrador ==='

-- Variables
DECLARE @AdminUserId INT
DECLARE @AdminRoleId INT

-- Verificar si el usuario ya existe
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@siinadseg.com')
BEGIN
    -- Crear usuario admin
    -- Password: Admin123! (hash SHA256 simple para testing)
    -- Hash de "Admin123!" usando SHA256
    INSERT INTO Users (
        UserName, 
        Email, 
        FirstName,
        LastName,
        PasswordHash, 
        IsActive,
        CreatedAt,
        CreatedBy,
        IsDeleted
    )
    VALUES (
        'admin',
        'admin@siinadseg.com',
        'Administrador',
        'Sistema',
        'jGl25bVBBBW96Qi9Te4V37Fnqchz/Eu4qB9vKrRIqRg=', -- Hash SHA256 de "Admin123!"
        1,
        GETUTCDATE(),
        'SYSTEM',
        0
    )
    
    SET @AdminUserId = SCOPE_IDENTITY()
    PRINT '✓ Usuario admin creado con ID: ' + CAST(@AdminUserId AS VARCHAR(10))
    PRINT '  Email: admin@siinadseg.com'
    PRINT '  Password: Admin123!'
END
ELSE
BEGIN
    SET @AdminUserId = (SELECT Id FROM Users WHERE Email = 'admin@siinadseg.com')
    PRINT '- Usuario admin ya existe con ID: ' + CAST(@AdminUserId AS VARCHAR(10))
END

PRINT ''

-- ==========================================
-- 3. ASIGNAR ROL ADMIN AL USUARIO
-- ==========================================

PRINT '=== Asignando Rol Admin al Usuario ==='

-- Obtener ID del rol Admin
SET @AdminRoleId = (SELECT Id FROM Roles WHERE Name = 'Admin')

IF @AdminRoleId IS NULL
BEGIN
    PRINT '✗ ERROR: No se encontró el rol Admin'
END
ELSE
BEGIN
    -- Asignar rol Admin al usuario
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @AdminUserId AND RoleId = @AdminRoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, IsActive, CreatedAt, CreatedBy, IsDeleted)
        VALUES (@AdminUserId, @AdminRoleId, 1, GETUTCDATE(), 'SYSTEM', 0)
        PRINT '✓ Rol Admin asignado correctamente al usuario'
    END
    ELSE
    BEGIN
        PRINT '- El usuario ya tiene el rol Admin asignado'
    END
END

PRINT ''
PRINT '=========================================='
PRINT '=== CONFIGURACIÓN INICIAL COMPLETADA ==='
PRINT '=========================================='
PRINT ''
PRINT 'Credenciales de acceso:'
PRINT '  Email:    admin@siinadseg.com'
PRINT '  Password: Admin123!'
PRINT ''
PRINT 'IMPORTANTE: Cambia la contraseña después del primer login'
PRINT ''

-- Mostrar resumen
PRINT 'Resumen:'
SELECT 
    (SELECT COUNT(*) FROM Roles WHERE IsDeleted = 0) as TotalRoles,
    (SELECT COUNT(*) FROM Users WHERE IsDeleted = 0) as TotalUsuarios,
    (SELECT COUNT(*) FROM UserRoles WHERE IsDeleted = 0) as TotalAsignaciones
