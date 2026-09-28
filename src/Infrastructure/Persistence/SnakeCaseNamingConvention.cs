using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Infrastructure.Persistence;

/// <summary>
/// EF Core naming convention that maps C# PascalCase property/table names to
/// snake_case database names. Matches the canonical SQL Server schema used by
/// the legacy 4.7.2 project so existing data and migration scripts remain
/// compatible.
/// </summary>
public static class SnakeCaseNamingConvention
{
    private static readonly Regex PascalToSnake = new(
        @"(?<!^)([A-Z][a-z])",
        RegexOptions.Compiled);

    public static string ToSnakeCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return PascalToSnake.Replace(input, "_$1").ToLowerInvariant();
    }

    /// <summary>
    /// Apply snake_case naming to all tables, columns, indexes, FKs, PKs in the model.
    /// Call from OnModelCreating before ApplyConfigurationsFromAssembly.
    /// </summary>
    public static void ApplyToModel(ModelBuilder modelBuilder)
    {
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Table name
            var tableName = entity.GetTableName();
            if (!string.IsNullOrEmpty(tableName) && tableName.Contains(' ') == false)
            {
                entity.SetTableName(ToSnakeCase(tableName));
            }

            // Columns
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(ToSnakeCase(property.Name));
            }

            // Keys
            foreach (var key in entity.GetKeys())
            {
                var keyName = key.GetName();
                if (!string.IsNullOrEmpty(keyName))
                {
                    key.SetName(ToSnakeCase(keyName));
                }
            }

            // Foreign keys
            foreach (var fk in entity.GetForeignKeys())
            {
                var constraintName = fk.GetConstraintName();
                if (!string.IsNullOrEmpty(constraintName))
                {
                    fk.SetConstraintName(ToSnakeCase(constraintName));
                }
            }

            // Indexes
            foreach (var index in entity.GetIndexes())
            {
                var databaseName = index.GetDatabaseName();
                if (!string.IsNullOrEmpty(databaseName))
                {
                    index.SetDatabaseName(ToSnakeCase(databaseName));
                }
            }
        }
    }
}
