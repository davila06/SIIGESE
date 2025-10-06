-- Script para crear Master Database y estructura multi-tenant
USE master;
GO

-- Crear Master Database si no existe
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'SinsegMaster')
BEGIN
    CREATE DATABASE SinsegMaster;
END
GO

USE SinsegMaster;
GO

-- Tabla principal de empresas/tenants
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Tenants' AND xtype='U')
BEGIN
    CREATE TABLE Tenants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId NVARCHAR(50) UNIQUE NOT NULL, -- slug empresa (ej: "empresa-abc")
        CompanyName NVARCHAR(200) NOT NULL,
        Domain NVARCHAR(100) NULL, -- custom domain (ej: abc.siinadseg.com)
        DatabaseName NVARCHAR(100) NOT NULL,
        ConnectionString NVARCHAR(500) NOT NULL,
        IsActive BIT DEFAULT 1,
        SubscriptionPlan NVARCHAR(50) NOT NULL DEFAULT 'Basic', -- Basic, Professional, Enterprise
        MaxUsers INT DEFAULT 10,
        MaxPolizas INT DEFAULT 1000,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        
        -- Configuraciones por empresa
        LogoUrl NVARCHAR(300),
        PrimaryColor NVARCHAR(7) DEFAULT '#1976d2', -- #hexcolor
        SecondaryColor NVARCHAR(7) DEFAULT '#424242',
        CustomCss NVARCHAR(MAX),
        
        -- Facturación
        BillingEmail NVARCHAR(200),
        BillingAddress NVARCHAR(500),
        LastPaymentDate DATETIME2,
        NextBillingDate DATETIME2,
        MonthlyFee DECIMAL(10,2) DEFAULT 99.99,
        
        -- Contacto
        ContactName NVARCHAR(200),
        ContactPhone NVARCHAR(50),
        ContactEmail NVARCHAR(200),
        
        INDEX IX_Tenants_TenantId (TenantId),
        INDEX IX_Tenants_Domain (Domain),
        INDEX IX_Tenants_IsActive (IsActive)
    );
END
GO

-- Usuarios del sistema (pueden tener acceso a múltiples tenants)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='SystemUsers' AND xtype='U')
BEGIN
    CREATE TABLE SystemUsers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Email NVARCHAR(200) UNIQUE NOT NULL,
        PasswordHash NVARCHAR(500) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        IsSuperAdmin BIT DEFAULT 0,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        LastLoginAt DATETIME2,
        
        INDEX IX_SystemUsers_Email (Email),
        INDEX IX_SystemUsers_IsActive (IsActive)
    );
END
GO

-- Relación usuarios-tenants (un usuario puede estar en múltiples empresas)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserTenants' AND xtype='U')
BEGIN
    CREATE TABLE UserTenants (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        TenantId NVARCHAR(50) NOT NULL,
        Role NVARCHAR(50) NOT NULL DEFAULT 'User', -- SuperAdmin, Admin, Manager, User, ReadOnly
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedByUserId UNIQUEIDENTIFIER,
        
        FOREIGN KEY (UserId) REFERENCES SystemUsers(Id),
        FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
        UNIQUE(UserId, TenantId),
        
        INDEX IX_UserTenants_UserId (UserId),
        INDEX IX_UserTenants_TenantId (TenantId),
        INDEX IX_UserTenants_Role (Role)
    );
END
GO

-- Configuraciones por tenant
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenantConfigurations' AND xtype='U')
BEGIN
    CREATE TABLE TenantConfigurations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId NVARCHAR(50) NOT NULL,
        ConfigKey NVARCHAR(100) NOT NULL,
        ConfigValue NVARCHAR(MAX),
        ConfigType NVARCHAR(50) DEFAULT 'String', -- String, Number, Boolean, JSON
        IsEncrypted BIT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2,
        
        FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
        UNIQUE(TenantId, ConfigKey),
        
        INDEX IX_TenantConfigs_TenantId (TenantId),
        INDEX IX_TenantConfigs_Key (ConfigKey)
    );
END
GO

-- Auditoría de acciones por tenant
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenantAuditLog' AND xtype='U')
BEGIN
    CREATE TABLE TenantAuditLog (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId NVARCHAR(50) NOT NULL,
        UserId UNIQUEIDENTIFIER,
        Action NVARCHAR(100) NOT NULL,
        EntityType NVARCHAR(100),
        EntityId NVARCHAR(100),
        OldValues NVARCHAR(MAX),
        NewValues NVARCHAR(MAX),
        IPAddress NVARCHAR(45),
        UserAgent NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
        FOREIGN KEY (UserId) REFERENCES SystemUsers(Id),
        
        INDEX IX_AuditLog_TenantId (TenantId),
        INDEX IX_AuditLog_UserId (UserId),
        INDEX IX_AuditLog_CreatedAt (CreatedAt),
        INDEX IX_AuditLog_Action (Action)
    );
END
GO

-- Métricas de uso por tenant
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TenantUsageMetrics' AND xtype='U')
BEGIN
    CREATE TABLE TenantUsageMetrics (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        TenantId NVARCHAR(50) NOT NULL,
        MetricDate DATE NOT NULL,
        ActiveUsers INT DEFAULT 0,
        TotalPolizas INT DEFAULT 0,
        TotalCobros INT DEFAULT 0,
        TotalReclamos INT DEFAULT 0,
        StorageUsedMB DECIMAL(10,2) DEFAULT 0,
        ApiCallsCount INT DEFAULT 0,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        
        FOREIGN KEY (TenantId) REFERENCES Tenants(TenantId),
        UNIQUE(TenantId, MetricDate),
        
        INDEX IX_UsageMetrics_TenantId (TenantId),
        INDEX IX_UsageMetrics_Date (MetricDate)
    );
END
GO

-- Insertar tenant por defecto para migración
IF NOT EXISTS (SELECT 1 FROM Tenants WHERE TenantId = 'empresa-default')
BEGIN
    INSERT INTO Tenants (
        TenantId, 
        CompanyName, 
        DatabaseName, 
        ConnectionString, 
        SubscriptionPlan,
        MaxUsers,
        MaxPolizas,
        BillingEmail,
        ContactName,
        ContactEmail,
        PrimaryColor,
        SecondaryColor
    ) VALUES (
        'empresa-default',
        'Mi Empresa Default',
        'SinsegTenant_Default',
        'Server=(localdb)\\mssqllocaldb;Database=SinsegTenant_Default;Trusted_Connection=true;MultipleActiveResultSets=true',
        'Professional',
        50,
        5000,
        'admin@miempresa.com',
        'Administrador',
        'admin@miempresa.com',
        '#1976d2',
        '#424242'
    );
    
    PRINT 'Tenant por defecto creado exitosamente';
END
GO

-- Insertar usuario super admin por defecto
IF NOT EXISTS (SELECT 1 FROM SystemUsers WHERE Email = 'superadmin@siinadseg.com')
BEGIN
    INSERT INTO SystemUsers (
        Email,
        PasswordHash,
        FirstName,
        LastName,
        IsSuperAdmin
    ) VALUES (
        'superadmin@siinadseg.com',
        '$2a$11$wK0FZHR6o.l1Q8q8dXmQ7uWJ9u5HJV8uJEXFhVz7Q8YWOzQU6J6nK', -- password: Admin@123
        'Super',
        'Admin',
        1
    );
    
    DECLARE @SuperAdminId UNIQUEIDENTIFIER = (SELECT Id FROM SystemUsers WHERE Email = 'superadmin@siinadseg.com');
    
    -- Asignar super admin al tenant por defecto
    INSERT INTO UserTenants (UserId, TenantId, Role)
    VALUES (@SuperAdminId, 'empresa-default', 'SuperAdmin');
    
    PRINT 'Super Admin creado exitosamente';
END
GO

-- Configuraciones iniciales para tenant por defecto
IF NOT EXISTS (SELECT 1 FROM TenantConfigurations WHERE TenantId = 'empresa-default')
BEGIN
    INSERT INTO TenantConfigurations (TenantId, ConfigKey, ConfigValue, ConfigType) VALUES
    ('empresa-default', 'SMTP_SERVER', 'smtp.gmail.com', 'String'),
    ('empresa-default', 'SMTP_PORT', '587', 'Number'),
    ('empresa-default', 'SMTP_USERNAME', '', 'String'),
    ('empresa-default', 'SMTP_PASSWORD', '', 'String'),
    ('empresa-default', 'ENABLE_NOTIFICATIONS', 'true', 'Boolean'),
    ('empresa-default', 'TIMEZONE', 'America/Argentina/Buenos_Aires', 'String'),
    ('empresa-default', 'CURRENCY', 'ARS', 'String'),
    ('empresa-default', 'LANGUAGE', 'es-AR', 'String');
    
    PRINT 'Configuraciones iniciales creadas';
END
GO

-- Procedimiento para crear nuevo tenant
CREATE OR ALTER PROCEDURE CreateNewTenant
    @TenantId NVARCHAR(50),
    @CompanyName NVARCHAR(200),
    @AdminEmail NVARCHAR(200),
    @AdminFirstName NVARCHAR(100),
    @AdminLastName NVARCHAR(100),
    @SubscriptionPlan NVARCHAR(50) = 'Basic',
    @Domain NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @DatabaseName NVARCHAR(100) = 'SinsegTenant_' + @TenantId;
    DECLARE @ConnectionString NVARCHAR(500) = 'Server=(localdb)\mssqllocaldb;Database=' + @DatabaseName + ';Trusted_Connection=true;MultipleActiveResultSets=true';
    
    BEGIN TRY
        BEGIN TRANSACTION;
        
        -- 1. Crear tenant
        INSERT INTO Tenants (
            TenantId, CompanyName, Domain, DatabaseName, ConnectionString, 
            SubscriptionPlan, ContactEmail, BillingEmail
        ) VALUES (
            @TenantId, @CompanyName, @Domain, @DatabaseName, @ConnectionString,
            @SubscriptionPlan, @AdminEmail, @AdminEmail
        );
        
        -- 2. Crear usuario admin
        DECLARE @AdminUserId UNIQUEIDENTIFIER = NEWID();
        INSERT INTO SystemUsers (Id, Email, PasswordHash, FirstName, LastName)
        VALUES (
            @AdminUserId, @AdminEmail, 
            '$2a$11$wK0FZHR6o.l1Q8q8dXmQ7uWJ9u5HJV8uJEXFhVz7Q8YWOzQU6J6nK', -- default password
            @AdminFirstName, @AdminLastName
        );
        
        -- 3. Asignar admin al tenant
        INSERT INTO UserTenants (UserId, TenantId, Role)
        VALUES (@AdminUserId, @TenantId, 'Admin');
        
        -- 4. Crear configuraciones básicas
        INSERT INTO TenantConfigurations (TenantId, ConfigKey, ConfigValue, ConfigType) VALUES
        (@TenantId, 'TIMEZONE', 'America/Argentina/Buenos_Aires', 'String'),
        (@TenantId, 'CURRENCY', 'ARS', 'String'),
        (@TenantId, 'LANGUAGE', 'es-AR', 'String'),
        (@TenantId, 'ENABLE_NOTIFICATIONS', 'true', 'Boolean');
        
        COMMIT TRANSACTION;
        
        SELECT 'SUCCESS' as Status, @TenantId as TenantId, @AdminUserId as AdminUserId;
        
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        
        SELECT 'ERROR' as Status, ERROR_MESSAGE() as ErrorMessage;
    END CATCH
END
GO

PRINT 'Master Database configurada correctamente para Multi-Tenancy';
PRINT 'Tenant por defecto: empresa-default';
PRINT 'Super Admin: superadmin@siinadseg.com / Admin@123';