-- =============================================
-- Script: 10_CreateChatTables.sql
-- Description: Creates ChatSessions and ChatMessages tables
--              for the enterprise chatbot feature.
-- Run after: 09_AddObservacionesToPolizas.sql
-- =============================================

-- ChatSessions table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'ChatSessions' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
    CREATE TABLE [dbo].[ChatSessions] (
        [Id]              INT           IDENTITY(1,1) NOT NULL,
        [SessionId]       NVARCHAR(64)  NOT NULL,
        [UserId]          INT           NOT NULL,
        [Title]           NVARCHAR(200) NOT NULL DEFAULT 'Nueva conversación',
        [Status]          INT           NOT NULL DEFAULT 1,   -- 1=Active, 2=Closed, 3=Archived
        [LastMessage]     NVARCHAR(300) NULL,
        [MessageCount]    INT           NOT NULL DEFAULT 0,
        [LastActivityAt]  DATETIME2     NULL,
        [CreatedAt]       DATETIME2     NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]       DATETIME2     NULL,
        [IsDeleted]       BIT           NOT NULL DEFAULT 0,
        [CreatedBy]       NVARCHAR(100) NOT NULL DEFAULT '',
        [UpdatedBy]       NVARCHAR(100) NULL,

        CONSTRAINT [PK_ChatSessions] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatSessions_Users] FOREIGN KEY ([UserId])
            REFERENCES [dbo].[Users]([Id]) ON DELETE CASCADE
    );

    CREATE UNIQUE NONCLUSTERED INDEX [IX_ChatSessions_SessionId]
        ON [dbo].[ChatSessions] ([SessionId]);

    CREATE NONCLUSTERED INDEX [IX_ChatSessions_UserId]
        ON [dbo].[ChatSessions] ([UserId], [IsDeleted], [LastActivityAt] DESC);

    PRINT 'Table ChatSessions created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ChatSessions already exists, skipping.';
END
GO

-- ChatMessages table
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'ChatMessages' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id]                INT            IDENTITY(1,1) NOT NULL,
        [ChatSessionId]     INT            NOT NULL,
        [Content]           NVARCHAR(2000) NOT NULL,
        [MessageType]       INT            NOT NULL DEFAULT 1,  -- 1=User, 2=Bot, 3=System
        [Status]            INT            NOT NULL DEFAULT 1,  -- 1=Sent, 2=Read, 3=Error
        [RichContent]       NVARCHAR(MAX)  NULL,   -- JSON
        [QuickReplies]      NVARCHAR(MAX)  NULL,   -- JSON array
        [ReactionScore]     INT            NULL,   -- 1=like, -1=dislike
        [IsRead]            BIT            NOT NULL DEFAULT 0,
        [ProcessingTimeMs]  INT            NULL,
        [CreatedAt]         DATETIME2      NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt]         DATETIME2      NULL,
        [IsDeleted]         BIT            NOT NULL DEFAULT 0,
        [CreatedBy]         NVARCHAR(100)  NOT NULL DEFAULT '',
        [UpdatedBy]         NVARCHAR(100)  NULL,

        CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatMessages_ChatSessions] FOREIGN KEY ([ChatSessionId])
            REFERENCES [dbo].[ChatSessions]([Id]) ON DELETE CASCADE
    );

    CREATE NONCLUSTERED INDEX [IX_ChatMessages_SessionId_CreatedAt]
        ON [dbo].[ChatMessages] ([ChatSessionId], [IsDeleted], [CreatedAt] ASC);

    PRINT 'Table ChatMessages created successfully.';
END
ELSE
BEGIN
    PRINT 'Table ChatMessages already exists, skipping.';
END
GO

PRINT 'Chat tables migration completed.';
GO
