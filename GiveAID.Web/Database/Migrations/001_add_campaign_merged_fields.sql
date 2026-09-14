-- ============================================================
-- Migration: add merged Campaign fields
-- The Campaign model absorbed Programme-style fields but the
-- Campaigns table never had them, causing EF6 to throw
-- "InvalidOperationException: column not found" on every read.
-- ============================================================

USE GiveAIDDB;
GO

-- Idempotent: only add if missing.
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'organization_id')
BEGIN
    ALTER TABLE dbo.Campaigns ADD organization_id INT NULL;
    PRINT 'Added organization_id';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'programme_type')
BEGIN
    ALTER TABLE dbo.Campaigns ADD programme_type NVARCHAR(50) NULL;
    PRINT 'Added programme_type';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'registration_required')
BEGIN
    ALTER TABLE dbo.Campaigns ADD registration_required BIT NOT NULL CONSTRAINT DF_Campaigns_registration_required DEFAULT 0;
    PRINT 'Added registration_required';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'max_participants')
BEGIN
    ALTER TABLE dbo.Campaigns ADD max_participants INT NULL;
    PRINT 'Added max_participants';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'target_beneficiaries')
BEGIN
    ALTER TABLE dbo.Campaigns ADD target_beneficiaries INT NULL;
    PRINT 'Added target_beneficiaries';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'expected_budget')
BEGIN
    ALTER TABLE dbo.Campaigns ADD expected_budget DECIMAL(18,2) NULL;
    PRINT 'Added expected_budget';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Campaigns') AND name = 'actual_budget')
BEGIN
    ALTER TABLE dbo.Campaigns ADD actual_budget DECIMAL(18,2) NULL;
    PRINT 'Added actual_budget';
END
GO

-- Verify
SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Campaigns' ORDER BY ORDINAL_POSITION;
