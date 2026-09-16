-- ============================================================================
-- Migration: Add idempotency key + payment-gateway tracking to Donations
-- ============================================================================
-- Previously donations were created with PaymentStatus = "Completed"
-- immediately on POST /api/donations, which was a financial-reporting bug
-- (no payment gateway was actually called). This migration:
--   1. Adds idempotency_key column (unique per user) so duplicate submits
--      are deduplicated instead of creating multiple rows.
--   2. Adds gateway_transaction_id + payment_confirmed_at columns so the
--      "pending -> confirmed" lifecycle can be tracked properly.
--
-- Run via:
--   .\scripts\RunSqlFile.ps1 -File '.\database\Donations_Idempotency_PaymentStatus.sql'
-- ============================================================================

USE [GiveAIDDB];
GO

-- 1) Idempotency key.
IF COL_LENGTH('Donations', 'idempotency_key') IS NULL
BEGIN
    ALTER TABLE [dbo].[Donations] ADD [idempotency_key] NVARCHAR(100) NULL;
END

-- Unique composite index — multiple NULL keys are allowed (filtered via logic).
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'UX_Donations_UserId_IdempotencyKey'
      AND object_id = OBJECT_ID('dbo.Donations')
)
BEGIN
    CREATE UNIQUE INDEX [UX_Donations_UserId_IdempotencyKey]
        ON [dbo].[Donations] ([user_id], [idempotency_key])
        WHERE [idempotency_key] IS NOT NULL;
END

-- 2) Payment-gateway tracking.
IF COL_LENGTH('Donations', 'gateway_transaction_id') IS NULL
BEGIN
    ALTER TABLE [dbo].[Donations] ADD [gateway_transaction_id] NVARCHAR(100) NULL;
END

IF COL_LENGTH('Donations', 'payment_confirmed_at') IS NULL
BEGIN
    ALTER TABLE [dbo].[Donations] ADD [payment_confirmed_at] DATETIME NULL;
END

-- 3) Backfill: any pre-existing "Completed" donation is now considered
--    "legacy/manual" and stays Completed — but we tag it as confirmed.
UPDATE [dbo].[Donations]
SET [payment_confirmed_at] = COALESCE([donation_date], GETUTCDATE())
WHERE [payment_status] = 'Completed' AND [payment_confirmed_at] IS NULL;

PRINT 'Donations_Idempotency_PaymentStatus migration applied.';
