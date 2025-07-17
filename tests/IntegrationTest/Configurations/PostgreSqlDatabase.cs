using System.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Respawn;

namespace Configurations;

public class PostgreSqlDatabase<TDbContext> : IDatabase
    where TDbContext : DbContext
{
    private NpgsqlConnection? _connection;
    private readonly string? _connectionString;
    private Respawner? _respawner;
    private readonly string _environmentName;

    public PostgreSqlDatabase()
    {
        _environmentName =
            Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .AddJsonFile(
                $"appsettings.Testing-{_environmentName}.json",
                optional: true,
                reloadOnChange: true
            )
            .Build();

        _connectionString = configuration["DatabaseSettings:DatabaseConnection"];
    }

    public async Task InitialiseAsync()
    {
        _connection = new NpgsqlConnection(_connectionString);

        var options = new DbContextOptionsBuilder<TDbContext>()
            .UseNpgsql(_connectionString)
            .Options;
        var context =
            Activator.CreateInstance(typeof(TDbContext), options) as TDbContext
            ?? throw new InvalidOperationException(
                $"Could not create instance of {typeof(TDbContext).Name}"
            );

        context.Database.EnsureDeleted();
        context.Database.Migrate();

        await _connection.OpenAsync();
        _respawner = await Respawner.CreateAsync(
            _connection,
            new RespawnerOptions
            {
                DbAdapter = DbAdapter.Postgres,
                TablesToIgnore = ["__EFMigrationsHistory"],
            }
        );
        await _connection.CloseAsync();
    }

    public DbConnection GetConnection() =>
        _connection ?? throw new InvalidOperationException("Connection not initialized");

    public string GetConnectionString() =>
        _connectionString
        ?? throw new InvalidOperationException("Connection string not initialized");

    public string GetEnvironmentVariable() => $"Testing-{_environmentName}"!;

    public async Task ResetAsync()
    {
        if (_respawner != null && _connection != null)
        {
            await _connection.OpenAsync();
            await _respawner.ResetAsync(_connection);
            await _connection.CloseAsync();
        }
    }

    public async Task DisposeAsync()
    {
        if (_connection != null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }
    }
}
