using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace API.Infrastructure.Persistence;

public static class UsersStorageInitializationExtensions
{
    private const string InMemoryProvider = "InMemory";
    private const string PostgresProvider = "Postgres";

    public static async Task EnsureUsersStorageInitializedAsync(
        this IServiceProvider services,
        IConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        var provider = configuration["UsersRepository:Provider"] ?? InMemoryProvider;
        if (!string.Equals(provider, PostgresProvider, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var connectionString = configuration.GetConnectionString("Main");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'Main' is required when UsersRepository:Provider is set to Postgres.");
        }

        await EnsureDatabaseExistsAsync(connectionString, cancellationToken);

        using var scope = services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApiDbContext>();
        await dbContext.Database.MigrateAsync(cancellationToken);
    }

    private static async Task EnsureDatabaseExistsAsync(string connectionString, CancellationToken cancellationToken)
    {
        var connectionBuilder = new NpgsqlConnectionStringBuilder(connectionString);
        if (string.IsNullOrWhiteSpace(connectionBuilder.Database))
        {
            throw new InvalidOperationException("Database name is missing in connection string 'Main'.");
        }

        var targetDatabase = connectionBuilder.Database;
        var adminConnectionBuilder = new NpgsqlConnectionStringBuilder(connectionString)
        {
            Database = "postgres"
        };

        await using var connection = new NpgsqlConnection(adminConnectionBuilder.ConnectionString);
        await connection.OpenAsync(cancellationToken);

        await using var existsCommand = connection.CreateCommand();
        existsCommand.CommandText = "SELECT 1 FROM pg_database WHERE datname = @databaseName";
        existsCommand.Parameters.AddWithValue("databaseName", targetDatabase);
        var exists = await existsCommand.ExecuteScalarAsync(cancellationToken) is not null;
        if (exists)
        {
            return;
        }

        var escapedDatabaseName = targetDatabase.Replace("\"", "\"\"");
        await using var createCommand = connection.CreateCommand();
        createCommand.CommandText = $"CREATE DATABASE \"{escapedDatabaseName}\"";
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}
