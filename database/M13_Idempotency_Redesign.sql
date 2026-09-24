-- ============================================================================
-- M-13 Idempotency Redesign
-- ============================================================================
-- 
-- PROBLEM (before):
-- 1. GetHashCode() used for fingerprint - NOT stable across processes/framework
-- 2. Fingerprint-based dedup: same email+campaign+amount blocked (WRONG business logic)
--    A donor wanting to donate $100 twice would be rejected!
--
-- SOLUTION (after):
-- 1. Client MUST provide IdempotencyKey (GUID) for retry safety
-- 2. If no key provided, server generates new Guid (NO deduplication)
-- 3. Server only dedups when SAME IdempotencyKey sent twice
-- 4. Same donation details can now be made multiple times (correct!)
--
-- Run via:
--   .\scripts\RunSqlFile.ps1 -File '.\database\M13_Idempotency_Redesign.sql'
-- ============================================================================

USE [GiveAIDDB];
GO

PRINT '============================================================';
PRINT 'M-13 Idempotency Redesign Migration';
PRINT '============================================================';

-- 1) Drop old composite index (only worked for authenticated users)
IF EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Donations_UserId_IdempotencyKey'
      AND object_id = OBJECT_ID('dbo.donations')
)
BEGIN
    DROP INDEX [IX_Donations_UserId_IdempotencyKey] ON [dbo].[donations];
    PRINT 'Dropped: IX_Donations_UserId_IdempotencyKey';
END
ELSE
BEGIN
    PRINT 'Note: IX_Donations_UserId_IdempotencyKey not found (may already be dropped)';
END

-- 2) Create new UNIQUE index on IdempotencyKey alone
-- This supports BOTH authenticated AND anonymous donations
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Donations_IdempotencyKey_Unique'
      AND object_id = OBJECT_ID('dbo.donations')
)
BEGIN
    CREATE UNIQUE INDEX [IX_Donations_IdempotencyKey_Unique]
        ON [dbo].[donations] ([idempotency_key])
        WHERE [idempotency_key] IS NOT NULL;
    PRINT 'Created: IX_Donations_IdempotencyKey_Unique';
END
ELSE
BEGIN
    PRINT 'Note: IX_Donations_IdempotencyKey_Unique already exists';
END

-- 3) Verify the changes
PRINT '';
PRINT 'Verification:';
SELECT 
    i.name AS IndexName,
    i.is_unique AS IsUnique,
    i.filter_definition AS FilterDefinition,
    COL_NAME(ic.object_id, ic.column_id) AS ColumnName
FROM sys.indexes i
JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
WHERE i.object_id = OBJECT_ID('dbo.donations')
  AND i.name LIKE '%IdempotencyKey%';

PRINT '';
PRINT '============================================================';
PRINT 'M-13 Migration Complete!';
PRINT '';
PRINT 'NEW BEHAVIOR:';
PRINT '  - Client MUST send IdempotencyKey for retry safety';
PRINT '  - No key = server generates new Guid (no dedup)';
PRINT '  - Same email+campaign+amount now ALLOWED';
PRINT '  - Server only dedups on matching IdempotencyKey';
PRINT '============================================================';
GO
