-- COPIA Y PEGA ESTE SCRIPT COMPLETO EN AZURE PORTAL QUERY EDITOR

-- 1. Actualizar la contraseña del admin a 'admin123'
UPDATE Users 
SET PasswordHash = '$2a$11$AxU8l3p/agLA25MYXetyOupizYtCLh0MRI2uYwnQzYHmPBeORgNWC',
    IsActive = 1,
    IsDeleted = 0
WHERE Email = 'admin@sinseg.com';

-- 2. Verificar el cambio
SELECT Id, UserName, Email, IsActive, IsDeleted FROM Users WHERE Email = 'admin@sinseg.com';
