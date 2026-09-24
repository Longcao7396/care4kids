using System.Data.Common;

namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Provides atomic database operations that cannot be expressed safely through EF Core
/// change tracking (e.g., incrementing a counter that must not lose updates under
/// concurrent load).
/// </summary>
public interface IAtomicCampaignUpdater
{
    /// <summary>
    /// Atomically increments the RaisedAmount of a campaign by the given delta.
    /// Safe against concurrent donation race conditions.
    /// Optionally participates in an external transaction (C-04.2) by using the same connection
    /// and transaction, ensuring both the donation insert and campaign update commit or rollback together.
    /// </summary>
    Task IncrementRaisedAmountAsync(
        int campaignId,
        decimal amount,
        DbConnection? existingConnection = null,
        DbTransaction? existingTransaction = null,
        CancellationToken cancellationToken = default);
}
