using System.Data.Common;

namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Factory for creating database transactions.
/// Defined in Application layer so handlers can request transaction-scoped operations.
/// Implemented in Infrastructure layer using the concrete database provider.
/// Used by C-04 to ensure donation insert + campaign update are in the same transaction.
/// </summary>
public interface IDbTransactionFactory
{
    /// <summary>
    /// Creates a new database transaction and opens a connection.
    /// The transaction and connection must both be kept alive for the duration of the atomic operation.
    /// Caller is responsible for disposing the transaction (and optionally the connection if opened).
    /// </summary>
    Task<(DbConnection Connection, DbTransaction Transaction)> BeginTransactionAsync(CancellationToken cancellationToken = default);
}
