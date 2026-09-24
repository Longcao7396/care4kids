using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace GiveAID.Infrastructure.Migrations;

/// <summary>
/// M-13: Idempotency Redesign Migration
/// 
/// CHANGES:
/// 1. Adds UNIQUE constraint on IdempotencyKey alone (nullable, allows multiple nulls)
///    - Previous: UX on (UserId, IdempotencyKey) - only worked for authenticated users
///    - Now: UX on just IdempotencyKey - works for both authenticated AND anonymous
/// 2. Ensures IdempotencyKey can store full GUIDs (up to 36 chars, was 100 max - no change needed)
///
/// BEHAVIORAL CHANGES (application-level, not migration-level):
/// - REMOVED fingerprint-based deduplication (GetHashCode was non-stable)
/// - Client MUST provide IdempotencyKey for retry safety
/// - If no key provided, server generates new Guid (no dedup)
/// - Same donation details can now be made multiple times (correct business behavior)
///
/// Run via:
///   dotnet ef database update --project src/Infrastructure
///   OR
///   .\scripts\RunSqlFile.ps1 -File '.\database\M13_Idempotency_Redesign.sql'
/// </summary>
public partial class M13_IdempotencyRedesign : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // M-13: Drop the old composite index that only worked for authenticated users
        // This allows anonymous donations to also benefit from idempotency
        if (IndexExists(migrationBuilder, "IX_Donations_UserId_IdempotencyKey"))
        {
            migrationBuilder.DropIndex(name: "IX_Donations_UserId_IdempotencyKey", table: "donations");
        }

        // M-13: Create a new UNIQUE index on just IdempotencyKey
        // Multiple NULL values are allowed (filtered index not supported in all SQL Server versions)
        // The application logic ensures non-null keys are unique
        if (!IndexExists(migrationBuilder, "IX_Donations_IdempotencyKey_Unique"))
        {
            migrationBuilder.CreateIndex(
                name: "IX_Donations_IdempotencyKey_Unique",
                table: "donations",
                column: "idempotency_key",
                unique: true,
                filter: "[idempotency_key] IS NOT NULL");
        }

        // Log the migration
        migrationBuilder.Sql(@"
            PRINT 'M-13 Idempotency Redesign migration applied.';
            PRINT 'IMPORTANT: Idempotency now relies on client-provided keys.';
            PRINT 'If no key is provided, server generates a new Guid (no dedup).';
        ");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Rollback: drop the new index
        if (IndexExists(migrationBuilder, "IX_Donations_IdempotencyKey_Unique"))
        {
            migrationBuilder.DropIndex(name: "IX_Donations_IdempotencyKey_Unique", table: "donations");
        }

        // Restore original composite index
        if (!IndexExists(migrationBuilder, "IX_Donations_UserId_IdempotencyKey"))
        {
            migrationBuilder.CreateIndex(
                name: "IX_Donations_UserId_IdempotencyKey",
                table: "donations",
                columns: new[] { "user_id", "idempotency_key" },
                unique: true,
                filter: "[idempotency_key] IS NOT NULL");
        }
    }

    private bool IndexExists(MigrationBuilder migrationBuilder, string indexName)
    {
        return migrationBuilder.Sql($@"
            SELECT COUNT(*) FROM sys.indexes 
            WHERE name = '{indexName}'
        ").ToString().Trim() != "0";
    }
}
