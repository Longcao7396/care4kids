using System.Data.Common;
using GiveAID.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Infrastructure.Persistence;

/// <summary>
/// Provides atomic database operations that cannot be safely expressed through EF Core
/// change tracking due to race conditions (e.g., read-modify-write on campaign RaisedAmount).
/// Uses raw SQL to ensure atomicity of the increment operation.
/// </summary>
public class AtomicCampaignUpdater : IAtomicCampaignUpdater
{
    private readonly GiveAIDDbContext _context;

    public AtomicCampaignUpdater(GiveAIDDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    /// C-04.1 FIX: Uses an atomic SQL UPDATE to increment RaisedAmount.
    /// SQL: UPDATE campaigns SET raised_amount = raised_amount + @amount WHERE campaign_id = @id
    /// The database handles the increment atomically — no read-modify-write race window.
    ///
    /// C-04.2: When an external transaction is provided, the raw SQL participates in that
    /// same transaction, ensuring donation insert + campaign update are fully atomic.
    /// </summary>
    public async Task IncrementRaisedAmountAsync(
        int campaignId,
        decimal amount,
        DbConnection? existingConnection = null,
        DbTransaction? existingTransaction = null,
        CancellationToken cancellationToken = default)
    {
        if (existingConnection != null && existingTransaction != null)
        {
            // C-04.2: Participate in caller's external transaction.
            // This ensures both SaveChanges (donation insert) and this UPDATE
            // commit together or rollback together.
            await using var cmd = existingConnection.CreateCommand();
            cmd.CommandText = "UPDATE campaigns SET raised_amount = raised_amount + @amount WHERE campaign_id = @id";
            cmd.Parameters.Add(CreateParam(cmd, "@amount", amount));
            cmd.Parameters.Add(CreateParam(cmd, "@id", campaignId));
            cmd.Transaction = existingTransaction;
            await cmd.ExecuteNonQueryAsync(cancellationToken);
        }
        else
        {
            // Standalone call — use EF Core's execution strategy (no external transaction)
            await _context.Database.ExecuteSqlInterpolatedAsync(
                $"UPDATE campaigns SET raised_amount = raised_amount + {amount} WHERE campaign_id = {campaignId}",
                cancellationToken);
        }
    }

    /// <summary>
    /// Creates a typed DbParameter for the current command's database provider.
    /// Works with SQL Server, PostgreSQL, SQLite, etc.
    /// </summary>
    private static DbParameter CreateParam(DbCommand cmd, string name, object value)
    {
        var param = cmd.CreateParameter();
        param.ParameterName = name;
        param.Value = value;
        return param;
    }
}
