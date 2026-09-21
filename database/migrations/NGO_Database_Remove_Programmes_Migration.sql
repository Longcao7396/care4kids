-- ============================================================
-- Migration: Remove legacy Programme tables
-- Only run this AFTER confirming ALL Programme data has been
-- reviewed, archived (if needed), and the application code
-- has been updated to no longer depend on these tables.
--
-- This script:
--   1. Backs up row counts to a temp table
--   2. Drops the foreign-key constraint on Gallery.programme_id
--      (nullable, was used to link gallery photos to programmes)
--   3. Drops the three legacy tables
--
-- SAFETY: If you need to undo, restore from the seed scripts
-- (03_Programmes_Seed.ps1 and 06_Registrations_Seed.ps1) which
-- are idempotent (they skip rows that already exist).
-- ============================================================

USE GiveAIDDB;
GO

-- Step 0: Verify tables exist before touching anything
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProgrammePhotos')
BEGIN
    PRINT 'ProgrammePhotos table does not exist - nothing to do.';
    RETURN;
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProgrammeRegistrations')
BEGIN
    PRINT 'ProgrammeRegistrations table does not exist - nothing to do.';
    RETURN;
END
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Programmes')
BEGIN
    PRINT 'Programmes table does not exist - nothing to do.';
    RETURN;
END
GO

-- Step 1: Archive row counts (informational snapshot before we drop)
DECLARE @ProgCount INT = (SELECT COUNT(*) FROM dbo.Programmes);
DECLARE @ProgRegCount INT = (SELECT COUNT(*) FROM dbo.ProgrammeRegistrations);
DECLARE @PhotoCount INT = (SELECT COUNT(*) FROM dbo.ProgrammePhotos);

PRINT '=====================================================';
PRINT 'Pre-drop row counts (for record-keeping):';
PRINT ('  Programmes:              ' + CAST(@ProgCount AS VARCHAR(10)));
PRINT ('  ProgrammeRegistrations: ' + CAST(@ProgRegCount AS VARCHAR(10)));
PRINT ('  ProgrammePhotos:         ' + CAST(@PhotoCount AS VARCHAR(10)));
PRINT '=====================================================';

-- Step 2: Drop Gallery FK to ProgrammePhotos (if still present)
-- The Gallery table has a nullable programme_id FK pointing to Programmes.
-- ProgrammePhotos has a FK to Programmes; Gallery has its own nullable FK.
-- Check for and drop any FK pointing from Gallery to Programmes.
DECLARE @FKName NVARCHAR(128);
DECLARE @FKCursor CURSOR;

SET @FKCursor = CURSOR FOR
    SELECT fk.name
    FROM sys.foreign_keys fk
    JOIN sys.tables t ON fk.parent_object_id = t.object_id
    WHERE fk.referenced_object_id = OBJECT_ID('dbo.Programmes')
      AND t.name = 'Gallery';

OPEN @FKCursor;
FETCH NEXT FROM @FKCursor INTO @FKName;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Sql NVARCHAR(200) = N'ALTER TABLE dbo.Gallery DROP CONSTRAINT [' + @FKName + '];';
    PRINT ('Dropping FK: ' + @Sql);
    EXEC sp_executesql @Sql;
    FETCH NEXT FROM @FKCursor INTO @FKName;
END

CLOSE @FKCursor;
DEALLOCATE @FKCursor;
GO

-- Step 3: Drop foreign-key constraints on ProgrammeRegistrations
-- (FK: ProgrammeRegistrations -> Programmes)
DECLARE @FKName2 NVARCHAR(128);
DECLARE @FKCursor2 CURSOR;

SET @FKCursor2 = CURSOR FOR
    SELECT fk.name
    FROM sys.foreign_keys fk
    WHERE fk.referenced_object_id = OBJECT_ID('dbo.Programmes');

OPEN @FKCursor2;
FETCH NEXT FROM @FKCursor2 INTO @FKName2;

WHILE @@FETCH_STATUS = 0
BEGIN
    DECLARE @Sql2 NVARCHAR(200) = N'ALTER TABLE dbo.ProgrammeRegistrations DROP CONSTRAINT [' + @FKName2 + '];';
    PRINT ('Dropping FK: ' + @Sql2);
    EXEC sp_executesql @Sql2;
    FETCH NEXT FROM @FKCursor2 INTO @FKName2;
END

CLOSE @FKCursor2;
DEALLOCATE @FKCursor2;
GO

-- Step 4: Drop the tables (in correct order: children first)
-- Order: ProgrammePhotos -> ProgrammeRegistrations -> Programmes

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProgrammePhotos')
BEGIN
    DROP TABLE IF EXISTS dbo.ProgrammePhotos;
    PRINT 'Dropped table: dbo.ProgrammePhotos';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ProgrammeRegistrations')
BEGIN
    DROP TABLE IF EXISTS dbo.ProgrammeRegistrations;
    PRINT 'Dropped table: dbo.ProgrammeRegistrations';
END
GO

IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Programmes')
BEGIN
    DROP TABLE IF EXISTS dbo.Programmes;
    PRINT 'Dropped table: dbo.Programmes';
END
GO

-- Step 5: Clean up the nullable programme_id column from Gallery
-- (keep it nullable - gallery rows may have photos linked to now-deleted programmes)
-- This is informational; the column is left in place for data archaeology.
PRINT '';
PRINT 'NOTE: Gallery.programme_id column has been left in place (nullable).';
PRINT '      It may contain orphaned IDs pointing to deleted programmes.';
PRINT '      To clean it up: UPDATE dbo.Gallery SET programme_id = NULL WHERE programme_id IS NOT NULL;';
PRINT '';
PRINT 'Migration complete: legacy Programme tables removed.';
PRINT '=====================================================';
GO
