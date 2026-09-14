-- ===========================================================
-- Migration: Invitations table for Invite Friends feature
-- Task 8 — User Communication Features
-- Idempotent — safe to re-run.
-- ===========================================================

IF OBJECT_ID('dbo.Invitations', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Invitations (
        InvitationId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        InviterUserId     INT NULL,
        InviteeName       NVARCHAR(150) NOT NULL,
        InviteeEmail      NVARCHAR(150) NOT NULL,
        PersonalMessage   NVARCHAR(500) NULL,
        Status            NVARCHAR(20)  NOT NULL DEFAULT 'Pending',
        InvitationToken   NVARCHAR(64)  NULL,
        SentAt            DATETIME NULL,
        RegisteredAt      DATETIME NULL,
        FailureReason     NVARCHAR(MAX) NULL,
        CreatedAt         DATETIME NOT NULL DEFAULT (GETDATE()),
        CONSTRAINT FK_Invitations_Users FOREIGN KEY (InviterUserId)
            REFERENCES dbo.Users(UserId)
    );
    PRINT 'Created Invitations table.';
END
ELSE
BEGIN
    PRINT 'Invitations table already exists — skipping CREATE.';
END
GO

-- Helpful indices
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Invitations_InviterUserId' AND object_id = OBJECT_ID('dbo.Invitations'))
    CREATE NONCLUSTERED INDEX IX_Invitations_InviterUserId ON dbo.Invitations(InviterUserId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Invitations_InviteeEmail' AND object_id = OBJECT_ID('dbo.Invitations'))
    CREATE NONCLUSTERED INDEX IX_Invitations_InviteeEmail ON dbo.Invitations(InviteeEmail);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Invitations_Status' AND object_id = OBJECT_ID('dbo.Invitations'))
    CREATE NONCLUSTERED INDEX IX_Invitations_Status ON dbo.Invitations(Status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Invitations_Token' AND object_id = OBJECT_ID('dbo.Invitations'))
    CREATE NONCLUSTERED INDEX IX_Invitations_Token ON dbo.Invitations(InvitationToken);
GO
