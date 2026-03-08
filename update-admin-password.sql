-- Script para actualizar la contraseña del usuario admin a 'admin123'
USE SiinadsegDB;
GO

-- Verificar el usuario actual
SELECT Id, UserName, Email, IsActive, IsDeleted 
FROM Users 
WHERE Email = 'admin@sinseg.com';
GO

-- Actualizar el hash de la contraseña a 'admin123'
-- Hash generado con BCrypt workFactor=11 para 'admin123'
UPDATE Users 
SET PasswordHash = '$2a$11$AxU8l3p/agLA25MYXetyOupizYtCLh0MRI2uYwnQzYHmPBeORgNWC',
    IsActive = 1,
    IsDeleted = 0,
    UpdatedAt = GETUTCDATE(),
    UpdatedBy = 'System'
WHERE Email = 'admin@sinseg.com';
GO

PRINT 'Contraseña actualizada a: admin123';
GO

-- Verificar el cambio
SELECT Id, UserName, Email, IsActive, LEFT(PasswordHash, 20) + '...' as PasswordHash
FROM Users 
WHERE Email = 'admin@sinseg.com';
GO
