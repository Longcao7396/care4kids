-- ===========================================================
-- Migration: EmailLogs table for persistent email audit & retry
-- Agent #1 — SMTP Email Service Implementation
-- Idempotent — safe to re-run.
-- Uses snake_case column names (matches EF6 SnakeCaseColumnNameConvention).
-- ===========================================================

IF OBJECT_ID('dbo.EmailLogs', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.EmailLogs (
        email_log_id      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        to_email          NVARCHAR(254) NOT NULL,
        subject           NVARCHAR(500) NOT NULL,
        body              NVARCHAR(MAX) NULL,
        category          NVARCHAR(50) NULL,           -- invitation | donation_receipt | registration_confirmation | contact_reply | general
        related_id        INT NULL,                   -- FK to DonationId, InvitationId, etc.
        status            NVARCHAR(20) NOT NULL DEFAULT 'Pending',  -- Sent | Failed | MockSent | Pending | PendingRetry
        sent_at           DATETIME NULL,
        error_message     NVARCHAR(2000) NULL,
        retry_count       INT NOT NULL DEFAULT 0,
        created_at        DATETIME NOT NULL DEFAULT (GETUTCDATE()),
        updated_at        DATETIME NOT NULL DEFAULT (GETUTCDATE())
    );
    PRINT 'Created EmailLogs table.';
END
ELSE
BEGIN
    PRINT 'EmailLogs table already exists — skipping CREATE.';
END
GO

-- Add updated_at column for existing databases (non-breaking for fresh installs)
IF OBJECT_ID('dbo.EmailLogs', 'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.EmailLogs') AND name = 'updated_at')
BEGIN
    ALTER TABLE dbo.EmailLogs ADD updated_at DATETIME NOT NULL DEFAULT (GETUTCDATE());
    PRINT 'Added updated_at column to EmailLogs.';
END
GO

-- Indexes for common query paths in AdminEmailLogsController
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLogs_Status' AND object_id = OBJECT_ID('dbo.EmailLogs'))
    CREATE NONCLUSTERED INDEX IX_EmailLogs_Status ON dbo.EmailLogs(status);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLogs_Category' AND object_id = OBJECT_ID('dbo.EmailLogs'))
    CREATE NONCLUSTERED INDEX IX_EmailLogs_Category ON dbo.EmailLogs(category);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLogs_ToEmail' AND object_id = OBJECT_ID('dbo.EmailLogs'))
    CREATE NONCLUSTERED INDEX IX_EmailLogs_ToEmail ON dbo.EmailLogs(to_email);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLogs_CreatedAt' AND object_id = OBJECT_ID('dbo.EmailLogs'))
    CREATE NONCLUSTERED INDEX IX_EmailLogs_CreatedAt ON dbo.EmailLogs(created_at DESC);
GO

-- Composite index for retry query: (Status, RetryCount) — used by RetryFailedEmails()
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EmailLogs_Status_RetryCount' AND object_id = OBJECT_ID('dbo.EmailLogs'))
    CREATE NONCLUSTERED INDEX IX_EmailLogs_Status_RetryCount
        ON dbo.EmailLogs(status, retry_count)
        WHERE status IN ('Failed', 'Pending');
GO

PRINT 'EmailLogs indexes created / verified.';
GO
