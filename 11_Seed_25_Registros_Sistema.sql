-- =============================================
-- Seed funcional de 25 registros para SINSEG
-- Compatible con el modelo EF actual (ApplicationDbContextModelSnapshot)
-- =============================================
-- Objetivo: dejar datos funcionales para módulos principales:
-- Clientes, Polizas, Cobros, Reclamos, Historial, Cotizaciones,
-- DataRecords, EmailConfigs y Chat.
--
-- Importante:
-- 1) Ejecutar luego de migraciones y de datos base (admin/roles).
-- 2) Script idempotente: usa IF NOT EXISTS por llaves naturales.
-- =============================================

USE [SinsegAppDb];
GO

SET NOCOUNT ON;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Now DATETIME2 = GETUTCDATE();
    DECLARE @PerfilId INT = 1;
    DECLARE @AdminUserId INT;

    SELECT @AdminUserId = Id
    FROM dbo.Users
    WHERE Email = 'admin@sinseg.com' AND IsDeleted = 0;

    IF @AdminUserId IS NULL
    BEGIN
        THROW 51000, 'No se encontro el usuario admin@sinseg.com. Ejecuta primero 05_InsertInitialData.sql.', 1;
    END

    -- =====================================================
    -- 1) CLIENTES (4)
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.Clientes WHERE Codigo = 'CL-SEED-001' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.Clientes
        (
            Codigo, RazonSocial, NombreComercial, NIT, Telefono, Email,
            Direccion, Ciudad, Departamento, Pais, EsActivo, FechaRegistro,
            PerfilId, CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        ('CL-SEED-001', 'Transportes Andinos S.A.', 'Andinos Cargo', '310100101', '2222-1001', 'operaciones@andinos.cr',
         'San Jose, La Uruca, Calle 5', 'San Jose', 'San Jose', 'Costa Rica', 1, @Now,
         @PerfilId, @Now, 'SeedScript', 0),
        ('CL-SEED-002', 'Comercial Delta LTDA', 'Delta Retail', '310100102', '2222-1002', 'finanzas@delta.cr',
         'Heredia, Centro, Avenida 2', 'Heredia', 'Heredia', 'Costa Rica', 1, @Now,
         @PerfilId, @Now, 'SeedScript', 0),
        ('CL-SEED-003', 'Servicios Medicos Horizonte S.R.L.', 'Clinica Horizonte', '310100103', '2222-1003', 'admin@horizonte.cr',
         'Alajuela, San Ramon, Barrio Central', 'Alajuela', 'Alajuela', 'Costa Rica', 1, @Now,
         @PerfilId, @Now, 'SeedScript', 0),
        ('CL-SEED-004', 'Constructora Pacifica S.A.', 'Pacifica Proyectos', '310100104', '2222-1004', 'contacto@pacifica.cr',
         'Puntarenas, El Roble, Ruta 1', 'Puntarenas', 'Puntarenas', 'Costa Rica', 1, @Now,
         @PerfilId, @Now, 'SeedScript', 0);
    END

    -- =====================================================
    -- 2) POLIZAS (4)
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.Polizas WHERE NumeroPoliza = 'POL-SEED-001' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.Polizas
        (
            NumeroPoliza, Modalidad, NombreAsegurado, NumeroCedula, Prima, Moneda,
            FechaVigencia, Frecuencia, Aseguradora, Placa, Marca, Modelo,
            Correo, NumeroTelefono, Observaciones, PerfilId, EsActivo,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        ('POL-SEED-001', 'Automovil', 'Carlos Mendez', '1-1001-0001', 185000.00, 'CRC',
         DATEADD(DAY, 25, @Now), 'Mensual', 'INS', 'ABC123', 'Toyota', 'Corolla',
         'carlos.mendez@andinos.cr', '8888-1001', 'Poliza de flotilla - unidad 1', @PerfilId, 1,
         @Now, 'SeedScript', 0),
        ('POL-SEED-002', 'Automovil', 'Laura Brenes', '1-1002-0002', 210000.00, 'CRC',
         DATEADD(DAY, 45, @Now), 'Mensual', 'Sagicor', 'DEF456', 'Nissan', 'X-Trail',
         'laura.brenes@delta.cr', '8888-1002', 'Poliza corporativa de ventas', @PerfilId, 1,
         @Now, 'SeedScript', 0),
        ('POL-SEED-003', 'Salud', 'Clinica Horizonte', '3-101-556677', 950.00, 'USD',
         DATEADD(DAY, 70, @Now), 'Trimestral', 'BlueCross', 'SAL001', 'N/A', 'N/A',
         'admin@horizonte.cr', '8888-1003', 'Cobertura salud ejecutiva', @PerfilId, 1,
         @Now, 'SeedScript', 0),
        ('POL-SEED-004', 'Incendio', 'Constructora Pacifica', '3-101-889900', 1450.00, 'USD',
         DATEADD(DAY, 120, @Now), 'Semestral', 'ASSA', 'INC001', 'N/A', 'N/A',
         'contacto@pacifica.cr', '8888-1004', 'Cobertura de bodega principal', @PerfilId, 1,
         @Now, 'SeedScript', 0);
    END

    DECLARE @Poliza1 INT = (SELECT TOP 1 Id FROM dbo.Polizas WHERE NumeroPoliza = 'POL-SEED-001' AND IsDeleted = 0);
    DECLARE @Poliza2 INT = (SELECT TOP 1 Id FROM dbo.Polizas WHERE NumeroPoliza = 'POL-SEED-002' AND IsDeleted = 0);
    DECLARE @Poliza3 INT = (SELECT TOP 1 Id FROM dbo.Polizas WHERE NumeroPoliza = 'POL-SEED-003' AND IsDeleted = 0);
    DECLARE @Poliza4 INT = (SELECT TOP 1 Id FROM dbo.Polizas WHERE NumeroPoliza = 'POL-SEED-004' AND IsDeleted = 0);

    -- =====================================================
    -- 3) COBROS (4)
    -- Estados: 0 Pendiente, 2 Cobrado, 3 Vencido
    -- MetodoPago: 1 Efectivo, 2 Tarjeta, 3 Transferencia
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.Cobros WHERE NumeroRecibo = 'REC-SEED-001' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.Cobros
        (
            NumeroRecibo, MontoTotal, MontoCobrado, FechaCobro, FechaVencimiento,
            Estado, MetodoPago, Moneda, Observaciones, PolizaId, NumeroPoliza,
            ClienteNombreCompleto, CorreoElectronico, UsuarioCobroId, UsuarioCobroNombre,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        ('REC-SEED-001', 185000.00, 0.00, @Now, DATEADD(DAY, 7, @Now),
         0, 3, 'CRC', 'Cobro pendiente de la primera cuota', @Poliza1, 'POL-SEED-001',
         'Carlos Mendez', 'carlos.mendez@andinos.cr', @AdminUserId, 'Administrador Sistema',
         @Now, 'SeedScript', 0),
        ('REC-SEED-002', 210000.00, 210000.00, DATEADD(DAY, -4, @Now), DATEADD(DAY, 10, @Now),
         2, 2, 'CRC', 'Cobro aplicado por tarjeta', @Poliza2, 'POL-SEED-002',
         'Laura Brenes', 'laura.brenes@delta.cr', @AdminUserId, 'Administrador Sistema',
         @Now, 'SeedScript', 0),
        ('REC-SEED-003', 950.00, 0.00, @Now, DATEADD(DAY, -3, @Now),
         3, 1, 'USD', 'Cobro vencido pendiente de gestion', @Poliza3, 'POL-SEED-003',
         'Clinica Horizonte', 'admin@horizonte.cr', @AdminUserId, 'Administrador Sistema',
         @Now, 'SeedScript', 0),
        ('REC-SEED-004', 1450.00, 1450.00, DATEADD(DAY, -1, @Now), DATEADD(DAY, 20, @Now),
         2, 3, 'USD', 'Cobro semestral procesado', @Poliza4, 'POL-SEED-004',
         'Constructora Pacifica', 'contacto@pacifica.cr', @AdminUserId, 'Administrador Sistema',
         @Now, 'SeedScript', 0);
    END

    -- =====================================================
    -- 4) RECLAMOS (3)
    -- TipoReclamo: 0 Siniestro, 1 Queja, 3 Reclamo
    -- Estado: 1 Abierto, 2 EnRevision, 6 Resuelto
    -- Prioridad: 1 Media, 2 Alta
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.Reclamos WHERE NumeroReclamo = 'RCL-SEED-001' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.Reclamos
        (
            NumeroReclamo, NumeroPoliza, TipoReclamo, Descripcion,
            FechaReclamo, FechaLimiteRespuesta, FechaResolucion, Estado, Prioridad,
            MontoReclamado, MontoAprobado, NombreAsegurado, ClienteNombreCompleto,
            Observaciones, DocumentosAdjuntos, UsuarioAsignadoId, PolizaId, Moneda,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        ('RCL-SEED-001', 'POL-SEED-001', 0, 'Siniestro por colision menor en parqueo.',
         DATEADD(DAY, -2, @Now), DATEADD(DAY, 8, @Now), NULL, 1, 2,
         550000.00, NULL, 'Carlos Mendez', 'Transportes Andinos S.A.',
         'Pendiente de inspeccion tecnica.', 'foto1.jpg;parte_policial.pdf', @AdminUserId, @Poliza1, 'CRC',
         @Now, 'SeedScript', 0),
        ('RCL-SEED-002', 'POL-SEED-002', 3, 'Disconformidad con deducible aplicado.',
         DATEADD(DAY, -6, @Now), DATEADD(DAY, 4, @Now), NULL, 2, 1,
         125000.00, NULL, 'Laura Brenes', 'Comercial Delta LTDA',
         'Cliente solicita recalculo y detalle.', 'correo_cliente.eml', @AdminUserId, @Poliza2, 'CRC',
         @Now, 'SeedScript', 0),
        ('RCL-SEED-003', 'POL-SEED-003', 1, 'Queja por demora en autorizacion de procedimiento.',
         DATEADD(DAY, -20, @Now), DATEADD(DAY, -8, @Now), DATEADD(DAY, -5, @Now), 6, 1,
         800.00, 700.00, 'Clinica Horizonte', 'Servicios Medicos Horizonte S.R.L.',
         'Caso cerrado con aprobacion parcial.', 'resolucion.pdf', @AdminUserId, @Poliza3, 'USD',
         @Now, 'SeedScript', 0);
    END

    DECLARE @Reclamo1 INT = (SELECT TOP 1 Id FROM dbo.Reclamos WHERE NumeroReclamo = 'RCL-SEED-001' AND IsDeleted = 0);

    -- =====================================================
    -- 5) RECLAMO HISTORIAL (1)
    -- =====================================================
    IF @Reclamo1 IS NOT NULL
       AND NOT EXISTS (
           SELECT 1
           FROM dbo.ReclamoHistoriales
           WHERE ReclamoId = @Reclamo1
             AND TipoEvento = 'Creacion'
             AND Descripcion = 'Reclamo creado desde seed funcional.'
             AND IsDeleted = 0
       )
    BEGIN
        INSERT INTO dbo.ReclamoHistoriales
        (
            ReclamoId, TipoEvento, ValorAnterior, ValorNuevo, Descripcion,
            Usuario, CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        (
            @Reclamo1, 'Creacion', NULL, 'Abierto', 'Reclamo creado desde seed funcional.',
            'admin@sinseg.com', @Now, 'SeedScript', 0
        );
    END

    -- =====================================================
    -- 6) COTIZACIONES (3)
    -- =====================================================
        IF NOT EXISTS (SELECT 1 FROM dbo.Cotizaciones WHERE NumeroCotizacion = 'COT-SEED-001' AND IsDeleted = 0)
        BEGIN
                DECLARE @AnoCol NVARCHAR(20) = QUOTENAME(N'A' + NCHAR(241) + N'o');
                DECLARE @SqlCot NVARCHAR(MAX);

                SET @SqlCot =
                N'INSERT INTO dbo.Cotizaciones
                    (
                        NumeroCotizacion, NumeroPoliza, NombreAsegurado, NombreSolicitante,
                        NumeroCedula, Email, NumeroTelefono, TipoSeguro, Modalidad,
                        Prima, Moneda, FechaVigencia, Frecuencia, FechaCotizacion,
                        FechaCreacion, FechaActualizacion, Aseguradora, Placa, Marca,
                        Modelo, ' + @AnoCol + N', Correo, Estado, UsuarioId, PerfilId,
                        CreatedAt, CreatedBy, IsDeleted
                    )
                    VALUES
                    (''COT-SEED-001'', ''POL-SEED-001'', ''Carlos Mendez'', ''Carlos Mendez'',
                     ''1-1001-0001'', ''carlos.mendez@andinos.cr'', ''8888-1001'', ''Auto'', ''Individual'',
                     178000.00, ''CRC'', DATEADD(DAY, 30, @Now), ''Mensual'', DATEADD(DAY, -3, @Now),
                     DATEADD(DAY, -3, @Now), @Now, ''INS'', ''ABC123'', ''Toyota'',
                     ''Corolla'', ''2021'', ''carlos.mendez@andinos.cr'', ''Aprobada'', @AdminUserId, @PerfilId,
                     @Now, ''SeedScript'', 0),
                    (''COT-SEED-002'', ''POL-SEED-002'', ''Laura Brenes'', ''Laura Brenes'',
                     ''1-1002-0002'', ''laura.brenes@delta.cr'', ''8888-1002'', ''Auto'', ''Corporativa'',
                     205000.00, ''CRC'', DATEADD(DAY, 45, @Now), ''Mensual'', DATEADD(DAY, -1, @Now),
                     DATEADD(DAY, -1, @Now), @Now, ''Sagicor'', ''DEF456'', ''Nissan'',
                     ''X-Trail'', ''2022'', ''laura.brenes@delta.cr'', ''EnRevision'', @AdminUserId, @PerfilId,
                     @Now, ''SeedScript'', 0),
                    (''COT-SEED-003'', ''POL-SEED-004'', ''Constructora Pacifica'', ''Ana Solis'',
                     ''2-2001-0003'', ''ana.solis@pacifica.cr'', ''8888-1010'', ''Incendio'', ''Empresarial'',
                     1400.00, ''USD'', DATEADD(DAY, 60, @Now), ''Semestral'', DATEADD(DAY, -7, @Now),
                     DATEADD(DAY, -7, @Now), @Now, ''ASSA'', ''INC001'', ''N/A'',
                     ''N/A'', ''2026'', ''ana.solis@pacifica.cr'', ''Emitida'', @AdminUserId, @PerfilId,
                     @Now, ''SeedScript'', 0);';

                EXEC sp_executesql
                        @SqlCot,
                        N'@Now DATETIME2, @AdminUserId INT, @PerfilId INT',
                        @Now = @Now,
                        @AdminUserId = @AdminUserId,
                        @PerfilId = @PerfilId;
        END

    -- =====================================================
    -- 7) DATA RECORDS (2)
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.DataRecords WHERE FileName = 'seed-polizas-lote1.xlsx' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.DataRecords
        (
            FileName, FileType, FileSize, TotalRecords, ProcessedRecords, ErrorRecords,
            Status, ProcessedAt, ErrorDetails, UploadedByUserId, PerfilId,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        ('seed-polizas-lote1.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
         24576, 120, 120, 0, 'Completed', DATEADD(MINUTE, -30, @Now), NULL, @AdminUserId, @PerfilId,
         @Now, 'SeedScript', 0),
        ('seed-cobros-lote1.xlsx', 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet',
         16384, 85, 83, 2, 'Completed', DATEADD(MINUTE, -15, @Now), '2 filas con monto invalido', @AdminUserId, @PerfilId,
         @Now, 'SeedScript', 0);
    END

    -- =====================================================
    -- 8) EMAIL CONFIG (1)
    -- =====================================================
    IF NOT EXISTS (SELECT 1 FROM dbo.EmailConfigs WHERE ConfigName = 'SEED-SMTP-LOCAL' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.EmailConfigs
        (
            ConfigName, SmtpServer, SmtpPort, Username, Password,
            EnableSsl, UseSSL, UseTLS, FromEmail, FromName, IsDefault, IsActive,
            Description, CompanyName, CompanyAddress, CompanyPhone,
            CompanyWebsite, CompanyLogo, TimeoutSeconds, MaxRetries,
            LastTested, LastTestSuccessful, LastTestError,
            CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        (
            'SEED-SMTP-LOCAL', 'smtp.mailtrap.io', 587, 'seed_user', 'seed_password',
            1, 1, 1, 'noreply@sinseg.local', 'SINSEG Notificaciones', 1, 1,
            'Configuracion local para pruebas de notificaciones',
            'SINSEG', 'San Jose, Costa Rica', '+506 2222-0000',
            'https://sinseg.local', '/assets/logo-sinseg.png', 30, 3,
            @Now, 1, NULL,
            @Now, 'SeedScript', 0
        );
    END

    -- =====================================================
    -- 9) CHAT (1 session + 2 messages = 3)
    -- =====================================================
    DECLARE @ChatSessionId INT;

    IF NOT EXISTS (SELECT 1 FROM dbo.ChatSessions WHERE SessionId = 'chat-seed-admin-20260411' AND IsDeleted = 0)
    BEGIN
        INSERT INTO dbo.ChatSessions
        (
            SessionId, UserId, Title, Status, LastMessage, MessageCount,
            LastActivityAt, CreatedAt, CreatedBy, IsDeleted
        )
        VALUES
        (
            'chat-seed-admin-20260411', @AdminUserId, 'Consulta de cobros y reclamos', 1,
            'Te puedo ayudar con cobros pendientes y reclamos abiertos.', 2,
            @Now, @Now, 'SeedScript', 0
        );
    END

    SELECT @ChatSessionId = Id
    FROM dbo.ChatSessions
    WHERE SessionId = 'chat-seed-admin-20260411' AND IsDeleted = 0;

    IF @ChatSessionId IS NOT NULL
    BEGIN
        IF NOT EXISTS (
            SELECT 1 FROM dbo.ChatMessages
            WHERE ChatSessionId = @ChatSessionId
              AND Content = 'Necesito ver los cobros pendientes de hoy.'
              AND IsDeleted = 0
        )
        BEGIN
            INSERT INTO dbo.ChatMessages
            (
                ChatSessionId, Content, MessageType, Status, RichContent,
                QuickReplies, ReactionScore, IsRead, ProcessingTimeMs,
                CreatedAt, CreatedBy, IsDeleted
            )
            VALUES
            (
                @ChatSessionId, 'Necesito ver los cobros pendientes de hoy.', 1, 2, NULL,
                NULL, NULL, 1, NULL,
                DATEADD(MINUTE, -2, @Now), 'SeedScript', 0
            );
        END

        IF NOT EXISTS (
            SELECT 1 FROM dbo.ChatMessages
            WHERE ChatSessionId = @ChatSessionId
              AND Content = 'Tengo 1 cobro pendiente y 1 vencido en los datos seed.'
              AND IsDeleted = 0
        )
        BEGIN
            INSERT INTO dbo.ChatMessages
            (
                ChatSessionId, Content, MessageType, Status, RichContent,
                QuickReplies, ReactionScore, IsRead, ProcessingTimeMs,
                CreatedAt, CreatedBy, IsDeleted
            )
            VALUES
            (
                @ChatSessionId, 'Tengo 1 cobro pendiente y 1 vencido en los datos seed.', 2, 1,
                '{"type":"summary","items":[{"estado":"Pendiente","cantidad":1},{"estado":"Vencido","cantidad":1}]}',
                '["Ver detalle","Enviar recordatorio","Abrir reclamos"]', 1, 0, 420,
                DATEADD(MINUTE, -1, @Now), 'SeedScript', 0
            );
        END
    END

    COMMIT TRANSACTION;

    PRINT 'Seed funcional aplicado correctamente.';
    PRINT 'Registros esperados del seed: 25';
    PRINT '- Clientes: 4';
    PRINT '- Polizas: 4';
    PRINT '- Cobros: 4';
    PRINT '- Reclamos: 3';
    PRINT '- ReclamoHistoriales: 1';
    PRINT '- Cotizaciones: 3';
    PRINT '- DataRecords: 2';
    PRINT '- EmailConfigs: 1';
    PRINT '- ChatSessions: 1';
    PRINT '- ChatMessages: 2';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();

    PRINT 'Error aplicando seed funcional.';
    THROW 51001, @ErrMsg, 1;
END CATCH;
GO
