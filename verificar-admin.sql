-- Verificar usuario admin y generar nuevo hash
SELECT 
    Id,
    UserName,
    Email,
    PasswordHash,
    IsActive,
    IsDeleted,
    CreatedAt,
    UpdatedAt
FROM Users 
WHERE Email = 'admin@sinseg.com';
