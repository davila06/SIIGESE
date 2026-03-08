-- Actualizar contraseña con hash SHA256 (el backend usa SHA256, no BCrypt)
UPDATE Users 
SET PasswordHash = 'JAvlGPq9JyTdtvBO6x2llnRI1+gxwIyPqCKAn3THIKk=',
    IsActive = 1,
    IsDeleted = 0
WHERE Email = 'admin@sinseg.com';

-- Verificar
SELECT Id, UserName, Email, IsActive, PasswordHash FROM Users WHERE Email = 'admin@sinseg.com';
