-- ============================================================================
-- NGO_Database_PaymentGateway_Migration.sql
-- Adds Payment Gateway integration tables and columns.
-- Run AFTER the main schema (NGO_Database_Schema.sql) and all prior migrations.
--
-- Compatible with: SQL Server 2012+
-- Author: Care4Kids NGO
-- Date: 2026-09-17
-- ============================================================================

SET NOCOUNT ON;

-- ──────────────────────────────────────────────────────────────────────────────
-- 1.  Add PaymentGateway + ClientSecret columns to Donations table
-- ──────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Donations'
      AND COLUMN_NAME = 'PaymentGateway'
)
BEGIN
    ALTER TABLE dbo.Donations
    ADD PaymentGateway NVARCHAR(20) NULL;
    PRINT '  [OK] Column Donations.PaymentGateway added.';
END
ELSE
    PRINT '  [SKIP] Column Donations.PaymentGateway already exists.';

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
    WHERE TABLE_NAME = 'Donations'
      AND COLUMN_NAME = 'ClientSecret'
)
BEGIN
    ALTER TABLE dbo.Donations
    ADD ClientSecret NVARCHAR(500) NULL;
    PRINT '  [OK] Column Donations.ClientSecret added.';
END
ELSE
    PRINT '  [SKIP] Column Donations.ClientSecret already exists.';

GO

-- ──────────────────────────────────────────────────────────────────────────────
-- 2.  WebhookLogs table — stores every gateway webhook event for audit/debug
-- ──────────────────────────────────────────────────────────────────────────────

IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.TABLES
    WHERE TABLE_NAME = 'WebhookLogs'
)
BEGIN
    CREATE TABLE dbo.WebhookLogs (
        WebhookLogId           BIGINT IDENTITY(1,1) NOT NULL,
        Gateway                NVARCHAR(20)    NOT NULL,
        EventType             NVARCHAR(100)   NOT NULL,
        EventId               NVARCHAR(100)   NULL,
        RawPayload            NVARCHAR(4000)  NULL,
        Signature             NVARCHAR(500)   NULL,
        SignatureValid        BIT             NOT NULL DEFAULT (1),
        ProcessingStatus      NVARCHAR(20)    NOT NULL DEFAULT N'Processed',
        ErrorMessage          NVARCHAR(500)   NULL,
        DonationTransactionId NVARCHAR(100)   NULL,
        DonationId            INT             NULL,
        ReceivedAt            DATETIME        NOT NULL DEFAULT GETUTCDATE(),
        ProcessedAt           DATETIME        NULL,

        CONSTRAINT PK_WebhookLogs PRIMARY KEY CLUSTERED (WebhookLogId ASC)
            WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF,
                  IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON,
                  ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
    );

    -- Index for deduping by EventId (gateway-provided idempotency key)
    CREATE NONCLUSTERED INDEX IX_WebhookLogs_EventId
        ON dbo.WebhookLogs (EventId ASC)
        WHERE EventId IS NOT NULL;

    -- Index for finding events by gateway and time
    CREATE NONCLUSTERED INDEX IX_WebhookLogs_Gateway_ReceivedAt
        ON dbo.WebhookLogs (Gateway ASC, ReceivedAt DESC);

    -- Index for linking to Donation
    CREATE NONCLUSTERED INDEX IX_WebhookLogs_DonationId
        ON dbo.WebhookLogs (DonationId ASC)
        WHERE DonationId IS NOT NULL;

    PRINT '  [OK] Table dbo.WebhookLogs created.';
END
ELSE
    PRINT '  [SKIP] Table dbo.WebhookLogs already exists.';

GO

-- ──────────────────────────────────────────────────────────────────────────────
-- 3.  Log a marker so this migration is auditable
-- ──────────────────────────────────────────────────────────────────────────────
PRINT '';
PRINT '=============================================================';
PRINT ' PaymentGateway_Migration completed at: ' + CONVERT(NVARCHAR(30), GETDATE(), 121);
PRINT '=============================================================';
