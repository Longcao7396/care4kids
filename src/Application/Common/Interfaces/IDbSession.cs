namespace GiveAID.Application.Common.Interfaces;

/// <summary>
/// Abstracts transaction lifecycle for operations that need atomicity guarantees.
/// Keeps the Infrastructure layer (where transactions live) out of the Application layer interface contract.
/// </summary>
public interface IDbSession
{
    /// <summary>
    /// Commits the current transaction. Must be called after all operations succeed.
    /// </summary>
    Task CommitAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Rolls back the current transaction. Called automatically on failure.
    /// </summary>
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
