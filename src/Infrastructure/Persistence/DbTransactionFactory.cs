using System.Data.Common;
using GiveAID.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Infrastructure.Persistence;

/// <summary>
/// Creates database transactions from the EF Core DbContext's connection.
/// Used to ensure atomicity for operations that mix EF Core SaveChanges with raw SQL (C-04).
/// </summary>
public class DbTransactionFactory : IDbTransactionFactory
{
    private readonly GiveAIDDbContext _context;

    public DbTransactionFactory(GiveAIDDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    /// Opens the DbContext's underlying connection and starts a transaction.
    /// The caller receives both the connection and transaction, and is responsible for disposing them.
    /// </summary>
    public async Task<(DbConnection Connection, DbTransaction Transaction)> BeginTransactionAsync(
        CancellationToken cancellationToken = default)
    {
        // Open connection if not already open
        if (_context.Database.GetDbConnection().State != System.Data.ConnectionState.Open)
        {
            await _context.Database.OpenConnectionAsync(cancellationToken);
        }

        var connection = _context.Database.GetDbConnection();
        var transaction = connection.BeginTransaction();

        return (connection, transaction);
    }
}
