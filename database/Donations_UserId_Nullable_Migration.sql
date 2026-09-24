-- =====================================================================
-- Migration: Make donations.user_id nullable for anonymous donations
-- =====================================================================
-- Purpose: Allow anonymous donations without FK violations
-- Date: 2026-09-22
-- 
-- This script makes the user_id column nullable in the donations table.
-- - Anonymous donations: user_id = NULL
-- - Authenticated donations: user_id = valid user ID
-- 
-- IMPORTANT: This migration assumes there are no FK violations (no donations
-- with user_id values that don't exist in users table).
-- If you have such data, clean it up before running this migration.
-- =====================================================================

SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Only run against GiveAIDDB
DECLARE @DBName NVARCHAR(128) = DB_NAME();
IF @DBName <> 'GiveAIDDB'
BEGIN
    PRINT 'This migration must be run against GiveAIDDB. Current database: ' + @DBName;
    RAISERROR('Wrong database', 16, 1);
    RETURN;
END

PRINT 'Starting migration: Make donations.user_id nullable';
PRINT 'Database: ' + @DBName;

BEGIN TRANSACTION;

BEGIN TRY
    -- Step 1: Check for any orphaned user_ids (values not in users table)
    DECLARE @OrphanedCount INT;
    SELECT @OrphanedCount = COUNT(*)
    FROM donations d
    LEFT JOIN users u ON d.user_id = u.Id
    WHERE d.user_id IS NOT NULL AND u.Id IS NULL;

    IF @OrphanedCount > 0
    BEGIN
        PRINT 'WARNING: Found ' + CAST(@OrphanedCount AS VARCHAR(10)) + ' orphaned user_id values in donations table.';
        PRINT 'These donations reference non-existent users.';
        PRINT 'Please resolve these records before running this migration.';
        PRINT 'Option 1: Delete the orphaned donations';
        PRINT 'Option 2: Set their user_id to NULL manually';
        
        -- List the orphaned donation IDs
        SELECT DonationId, UserId AS OrphanedUserId, Amount, DonationDate
        FROM donations d
        WHERE NOT EXISTS (SELECT 1 FROM users u WHERE u.Id = d.user_id);
        
        RAISERROR('Orphaned user_id values found. Resolve before proceeding.', 16, 1);
        RETURN;
    END

    -- Step 2: Make user_id nullable (drop and recreate FK)
    PRINT 'Making user_id nullable...';

    -- Drop existing FK constraint
    IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_donations_users_UserId' AND parent_object_id = OBJECT_ID('donations'))
    BEGIN
        ALTER TABLE donations DROP CONSTRAINT FK_donations_users_UserId;
        PRINT 'Dropped FK constraint FK_donations_users_UserId';
    END

    -- Drop the column and recreate as nullable
    DECLARE @SQL NVARCHAR(MAX);
    
    -- SQL Server doesn't support ALTER COLUMN in simple form for NOT NULL to NULL
    -- We need to drop and recreate
    SET @SQL = N'
        ALTER TABLE donations ALTER COLUMN user_id INT NULL;
    ';
    EXEC sp_executesql @SQL;
    PRINT 'Altered user_id column to nullable';

    -- Re-add FK constraint (now allows NULL)
    ALTER TABLE donations
    ADD CONSTRAINT FK_donations_users_UserId
    FOREIGN KEY (user_id) REFERENCES users(Id)
    ON DELETE RESTRICT;
    PRINT 'Re-added FK constraint FK_donations_users_UserId (now nullable)';

    -- Step 3: Create index on user_id if not exists (nullable columns may need filtered index)
    -- Standard index still works for nullable columns
    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_donations_UserId' AND object_id = OBJECT_ID('donations'))
    BEGIN
        CREATE INDEX IX_donations_UserId ON donations(user_id);
        PRINT 'Created index IX_donations_UserId';
    END

    -- Step 4: Update existing seed data (if any anonymous donations have user_id = 0 or similar invalid values)
    -- This is a safety check - in proper operation, anonymous donations should have NULL
    UPDATE donations
    SET user_id = NULL
    WHERE user_id IS NOT NULL 
      AND user_id <= 0
      AND NOT EXISTS (SELECT 1 FROM users WHERE Id = donations.user_id);
    
    DECLARE @UpdatedCount INT = @@ROWCOUNT;
    IF @UpdatedCount > 0
    BEGIN
        PRINT 'Updated ' + CAST(@UpdatedCount AS VARCHAR(10)) + ' donations with invalid user_id to NULL';
    END

    COMMIT TRANSACTION;
    
    PRINT '';
    PRINT 'Migration completed successfully!';
    PRINT 'Summary:';
    PRINT '  - donations.user_id is now nullable';
    PRINT '  - Anonymous donations can have user_id = NULL';
    PRINT '  - Authenticated donations must have user_id = valid user ID';
    PRINT '  - FK constraint still enforced for non-null values';
    PRINT '';

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;
    
    PRINT 'Migration failed with error:';
    PRINT ERROR_MESSAGE();
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    
    RAISERROR('Migration failed', 16, 1);
END CATCH
GO
