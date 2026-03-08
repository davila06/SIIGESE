-- Script para verificar y arreglar el usuario admin en Azure SQL
USE SiinadsegDB;
GO

-- Verificar si existe el usuario admin
SELECT * FROM Users WHERE Email = 'admin@sinseg.com';
GO

-- Si no existe, crearlo
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@sinseg.com')
BEGIN
    -- Crear el usuario admin con el hash correcto de BCrypt para 'password123'
    INSERT INTO Users (UserName, Email, FirstName, LastName, PasswordHash, IsActive, CreatedBy, CreatedAt)
    VALUES ('admin', 'admin@sinseg.com', 'Administrador', 'Sistema', 
            '$2a$11$Xed1MsTJ.11zGnnvwDLGaeGav13ki.M4gEB5LSGg/vhxtWT5FC8Xm', 
            1, 'System', GETUTCDATE());
    
    PRINT 'Usuario admin creado exitosamente';
END
ELSE
BEGIN
    -- Si existe pero el hash es diferente, actualizarlo
    UPDATE Users 
    SET PasswordHash = '$2a$11$Xed1MsTJ.11zGnnvwDLGaeGav13ki.M4gEB5LSGg/vhxtWT5FC8Xm',
        IsActive = 1,
        IsDeleted = 0
    WHERE Email = 'admin@sinseg.com';
    
    PRINT 'Usuario admin actualizado exitosamente';
END
GO

-- Verificar que el rol Admin existe
IF NOT EXISTS (SELECT 1 FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description, CreatedBy, CreatedAt)
    VALUES ('Admin', 'Administrador', 'System', GETUTCDATE());
    
    PRINT 'Rol Admin creado';
END
GO

-- Asignar el rol Admin al usuario
DECLARE @UserId INT = (SELECT Id FROM Users WHERE Email = 'admin@sinseg.com');
DECLARE @RoleId INT = (SELECT Id FROM Roles WHERE Name = 'Admin');

IF @UserId IS NOT NULL AND @RoleId IS NOT NULL
BEGIN
    IF NOT EXISTS (SELECT 1 FROM UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, CreatedBy, CreatedAt)
        VALUES (@UserId, @RoleId, 'System', GETUTCDATE());
        
        PRINT 'Rol Admin asignado al usuario admin';
    END
    ELSE
    BEGIN
        PRINT 'El usuario admin ya tiene el rol Admin';
    END
END
GO

-- Verificar el resultado final
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.FirstName,
    u.LastName,
    u.IsActive,
    u.IsDeleted,
    r.Name as RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@sinseg.com';
GO
