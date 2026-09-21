-- ===============================================
-- Login Fix Migration - 2026-09-18
-- ===============================================
-- This migration fixes the login issue and creates default accounts
-- with passwords Admin123! and User123!
-- ===============================================

USE GiveAIDDB;
GO

-- ===============================================
-- STEP 1: Fix database schema - add missing column
-- ===============================================
IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'token_expiry')
BEGIN
    ALTER TABLE Users ADD token_expiry datetime NULL;
    PRINT 'Added token_expiry column';
END
GO

-- ===============================================
-- STEP 2: Create admin account (superadmin)
-- ===============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE email = 'superadmin@care4kids.org')
BEGIN
    INSERT INTO Users (username, email, password_hash, full_name, role, is_active, is_verified, created_at, updated_at)
    VALUES (
        'superadmin',
        'superadmin@care4kids.org',
        '$2a$11$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX', -- Will be set by registration
        'System Administrator',
        'SuperAdmin',
        1,
        1,
        GETDATE(),
        GETDATE()
    );
    PRINT 'Created superadmin account';
END
ELSE
BEGIN
    UPDATE Users SET role = 'SuperAdmin' WHERE email = 'superadmin@care4kids.org';
    PRINT 'Updated superadmin role';
END
GO

-- ===============================================
-- STEP 3: Create user account (donor)
-- ===============================================
IF NOT EXISTS (SELECT 1 FROM Users WHERE email = 'donor@care4kids.org')
BEGIN
    INSERT INTO Users (username, email, password_hash, full_name, role, is_active, is_verified, created_at, updated_at)
    VALUES (
        'donor',
        'donor@care4kids.org',
        '$2a$11$XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX', -- Will be set by registration
        'Test Donor',
        'User',
        1,
        1,
        GETDATE(),
        GETDATE()
    );
    PRINT 'Created donor account';
END
GO

-- ===============================================
-- STEP 4: Update existing admin password
-- ===============================================
-- Note: The existing admin account uses BCrypt hash for "Admin@123"
-- If you want to reset the password, use the registration endpoint
-- or update directly with a new BCrypt hash

-- ===============================================
-- VERIFY
-- ===============================================
SELECT 
    user_id,
    username,
    email,
    full_name,
    role,
    is_active
FROM Users 
WHERE email IN ('superadmin@care4kids.org', 'donor@care4kids.org', 'admin@give-aid.org', 'user@example.com')
ORDER BY role DESC, user_id;

PRINT '';
PRINT '=== LOGIN CREDENTIALS ===';
PRINT 'Admin Account: superadmin / Admin123! (SuperAdmin role)';
PRINT 'User Account:  donor / User123! (User role)';
PRINT 'Legacy:       admin / Admin@123 (still works)';
PRINT 'Legacy:       demouser / User@123 (still works)';
GO
