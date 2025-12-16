-- Script para crear usuario administrador inicial en la nueva base de datos
-- Base de datos: SiinadsegProdDB
-- Usuario: admin@siinadseg.com
-- Password: Admin123!

-- Crear rol Admin si no existe
IF NOT EXISTS (SELECT * FROM Roles WHERE Name = 'Admin')
BEGIN
    INSERT INTO Roles (Name, Description, IsActive, CreatedAt, CreatedBy, IsDeleted)
    VALUES ('Admin', 'Administrador del sistema', 1, GETDATE(), 'SYSTEM', 0)
    PRINT 'Rol Admin creado'
END
ELSE
BEGIN
    PRINT 'Rol Admin ya existe'
END

-- Crear usuario admin
-- Password hash para "Admin123!"
DECLARE @userId INT
DECLARE @adminRoleId INT

-- Verificar si el usuario ya existe
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'admin@siinadseg.com')
BEGIN
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
        'Admin',
        'Administrador',
        'AQAAAAIAAYagAAAAELq9FYt9IqZxK8yZwJmF8xZ0Y5mRKX5vGr0kBnNqZ8fYx2iQw+HZJqR8kF7PWJW8Qw==',
        1,
        GETDATE(),
        'SYSTEM',
        0
    )
    
    SET @userId = SCOPE_IDENTITY()
    PRINT 'Usuario admin creado con ID: ' + CAST(@userId AS VARCHAR(10))
    
    -- Asignar rol Admin al usuario
    SELECT @adminRoleId = Id FROM Roles WHERE Name = 'Admin'
    
    IF @adminRoleId IS NOT NULL
    BEGIN
        INSERT INTO UserRoles (UserId, RoleId, CreatedAt, CreatedBy, IsDeleted)
        VALUES (@userId, @adminRoleId, GETDATE(), 'SYSTEM', 0)
        PRINT 'Rol Admin asignado al usuario'
    END
END
ELSE
BEGIN
    PRINT 'Usuario admin@siinadseg.com ya existe'
END

-- Verificar la creación
SELECT 
    u.Id,
    u.UserName,
    u.Email,
    u.FirstName,
    u.LastName,
    u.IsActive,
    u.CreatedAt,
    r.Name as RoleName
FROM Users u
LEFT JOIN UserRoles ur ON u.Id = ur.UserId
LEFT JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Email = 'admin@siinadseg.com'

PRINT 'Usuario administrador creado exitosamente'
PRINT 'Email: admin@siinadseg.com'
PRINT 'Password: Admin123!'
