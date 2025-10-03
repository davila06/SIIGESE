-- =============================================
-- Script para crear foreign keys
-- =============================================

USE SinsegAppDb;
GO

PRINT 'Creando foreign keys...';
GO

-- Foreign Key: DataRecords -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_DataRecords_Users_UploadedByUserId')
BEGIN
    ALTER TABLE [DataRecords] 
    ADD CONSTRAINT [FK_DataRecords_Users_UploadedByUserId] 
    FOREIGN KEY ([UploadedByUserId]) REFERENCES [Users] ([Id]);
    PRINT 'Foreign Key FK_DataRecords_Users_UploadedByUserId creada.';
END
GO

-- Foreign Key: UserRoles -> Users
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserRoles_Users_UserId')
BEGIN
    ALTER TABLE [UserRoles] 
    ADD CONSTRAINT [FK_UserRoles_Users_UserId] 
    FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE;
    PRINT 'Foreign Key FK_UserRoles_Users_UserId creada.';
END
GO

-- Foreign Key: UserRoles -> Roles
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserRoles_Roles_RoleId')
BEGIN
    ALTER TABLE [UserRoles] 
    ADD CONSTRAINT [FK_UserRoles_Roles_RoleId] 
    FOREIGN KEY ([RoleId]) REFERENCES [Roles] ([Id]) ON DELETE CASCADE;
    PRINT 'Foreign Key FK_UserRoles_Roles_RoleId creada.';
END
GO

PRINT 'Todas las foreign keys han sido creadas exitosamente.';
GO