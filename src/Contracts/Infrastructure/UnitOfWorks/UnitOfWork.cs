using System.Data;
using System.Data.Common;
using Application.Common.Interfaces.UnitOfWorks;
using AutoMapper;
using Contracts.Infrastructure.UnitOfWorks.Repositories;
using Infrastructure.UnitOfWorks.CachedRepositories;
using Infrastructure.UnitOfWorks.Repositories;
using JohnChum.SharedKernel.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Memory;
using Npgsql;
using Serilog;

namespace Infrastructure.UnitOfWorks;

public class UnitOfWork(IMapper mapper, IDbContext dbContext, IMemoryCache cache, ILogger logger)
    : IUnitOfWork
{
    public DbTransaction? CurrentTransaction { get; set; }

    private readonly Dictionary<string, object?> repositories = [];
    private bool disposed = false;

    public IRepository<TEntity> Repository<TEntity>()
        where TEntity : class
    {
        typeof(TEntity).IsValidBaseType();
        string type = typeof(TEntity).FullName!;

        if (!repositories.TryGetValue(type, out object? value))
        {
            Type repositoryType = typeof(Repository<>);
            object? repositoryInstance = Activator.CreateInstance(
                repositoryType.MakeGenericType(typeof(TEntity)),
                [dbContext, mapper]
            );
            value = repositoryInstance;
            repositories.Add(type, value);
        }

        return (IRepository<TEntity>)value!;
    }

    public IRepositoryFunction<TEntity> RepositoryFunction<TEntity>()
        where TEntity : new()
    {
        string type = typeof(TEntity).FullName!;

        if (!repositories.TryGetValue(type, out object? value))
        {
            Type repositoryType = typeof(RepositoryFunction<>);
            var repositoryInstance = Activator.CreateInstance(
                repositoryType.MakeGenericType(typeof(TEntity)),
                new object[] { this }
            );
            value = repositoryInstance;
            repositories.Add(type, value);
        }

        return (IRepositoryFunction<TEntity>)value!;
    }

    public IRepository<TEntity> CachedRepository<TEntity>()
        where TEntity : class
    {
        typeof(TEntity).IsValidBaseType();
        string type = $"{typeof(TEntity).FullName}-cached";

        if (!repositories.TryGetValue(type, out object? value))
        {
            Type cachedRepositoryType = typeof(CachedRepository<>);
            Type repositoryType = typeof(Repository<>);

            object? repositoryInstance = Activator.CreateInstance(
                repositoryType.MakeGenericType(typeof(TEntity)),
                [dbContext, mapper]
            );
            // proxy design pattern
            object? cachedRepositoryInstance = Activator.CreateInstance(
                cachedRepositoryType.MakeGenericType(typeof(TEntity)),
                [repositoryInstance, cache, logger]
            );
            value = cachedRepositoryInstance;
            repositories.Add(type, value);
        }

        return (IRepository<TEntity>)value!;
    }

    public async Task<DbTransaction> CreateTransactionAsync(
        CancellationToken cancellationToken = default
    )
    {
        if (CurrentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress.");
        }

        IDbContextTransaction currentTransaction =
            await dbContext.DatabaseFacade.BeginTransactionAsync(cancellationToken);

        CurrentTransaction = currentTransaction.GetDbTransaction();
        return CurrentTransaction;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            throw new InvalidOperationException("No transaction started.");
        }

        try
        {
            await CurrentTransaction.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            await RollbackAsync(cancellationToken);
            throw new Exception("Transaction commit failed. Rolled back.", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (CurrentTransaction == null)
        {
            logger.Warning("Thre is no transaction started.");
            return;
        }

        try
        {
            await CurrentTransaction.RollbackAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            throw new Exception("Transaction rollback failed.", ex);
        }
        finally
        {
            await DisposeTransactionAsync();
        }
    }

    public int ExecuteSqlCommand(string sql, params object[] parameters) =>
        dbContext.DatabaseFacade.ExecuteSqlRaw(sql, parameters);

    public async Task SaveAsync(CancellationToken cancellationToken = default) =>
        await dbContext.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        Dispose(true);

        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed && disposing)
        {
            repositories?.Clear();
            dbContext.Dispose();
        }

        disposed = true;
    }

    private async Task DisposeTransactionAsync()
    {
        if (CurrentTransaction != null)
        {
            await CurrentTransaction.DisposeAsync();
            CurrentTransaction = null;
        }
    }

    public async Task<T> ExecuteScalarAsync<T>(
        string sql,
        IEnumerable<NpgsqlParameter> parameters,
        CancellationToken cancellationToken = default
    )
    {
        var conn = dbContext.DatabaseFacade.GetDbConnection();
        if (conn.State != ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        foreach (var param in parameters)
        {
            cmd.Parameters.Add(param);
        }

        var result = await cmd.ExecuteScalarAsync(cancellationToken);
        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<List<T>> ExecuteSqlQueryAsync<T>(
        string sql,
        IEnumerable<NpgsqlParameter> parameters,
        CancellationToken cancellationToken = default
    )
        where T : new()
    {
        var conn = dbContext.DatabaseFacade.GetDbConnection();
        if (conn.State != System.Data.ConnectionState.Open)
            await conn.OpenAsync(cancellationToken);

        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.CommandType = CommandType.Text;

        foreach (var p in parameters)
            cmd.Parameters.Add(p);

        var results = new List<T>();

        using var reader = await cmd.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(UniOfWorkExtension.MapReaderToObject<T>(reader));
        }

        return results;
    }
}
