using System.Data;
using Dapper;
using Micro.Shared.Domain;
using Micro.Shared.Infrastructure.Policies;
using Micro.Shared.Model;
using Micro.Shared.QueryServices;
using Microsoft.EntityFrameworkCore;

namespace Micro.Shared.Repository;

public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
       where TContext : DbContext
       where TEntity : class
       where TKey : IEquatable<TKey>
{
    protected readonly TContext _context;
    protected readonly IDbConnection _dbConnection;
    protected DbSet<TEntity> DbSet { get; }
    private readonly IDapperQueryBuilder _dapperQueryBuilder;

    public Repository(TContext context, IDbConnection dbConnection, IDapperQueryBuilder dapperQueryBuilder)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        DbSet = context.Set<TEntity>();
        _dbConnection = dbConnection;
        _dapperQueryBuilder = dapperQueryBuilder;
        Dapper.SqlMapper.SetTypeMap(
           typeof(TContext),
           new SnakeCaseTypeMap(typeof(TContext))
       );
    }

    /// <inheritdoc/>
    public TEntity Create(TEntity t)
    {
        var result = DbSet.Add(t);
        _context.SaveChanges();
        return result.Entity;
    }

    /// <inheritdoc/>
    public TEntity? GetByID(TKey id) => DbSet.Find(id);

    /// <inheritdoc/>
    public IQueryable<TEntity> GetAll() => DbSet.AsNoTracking().AsQueryable();

    /// <inheritdoc/>
    public async Task<IEnumerable<TEntity>> GetAllAsync(QueryParameters? param)
    {
        string query = await _dapperQueryBuilder.BuildQuery<TEntity>(param, out DynamicParameters dapperParams);

        var data = await _dbConnection.QueryAsync<TEntity>(query, dapperParams);
        Console.WriteLine("log: info, created_at: " + DateTimeOffset.UtcNow + ", query: " + query);
        return data;
    }

    /// <inheritdoc/>
    public bool Update(TEntity t)
    {
        DbSet.Update(t);
        return _context.SaveChanges() > 0;
    }

    /// <inheritdoc/>
    public bool Delete(TKey id)
    {
        var entity = DbSet.Find(id);
        if (entity != null)
        {
            DbSet.Remove(entity);
            return _context.SaveChanges() > 0;
        }
        return false;
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity> CreateAsync(TEntity t, CancellationToken cancellationToken = default)
    {
        var result = await DbSet.AddAsync(t, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return result.Entity;
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity?> GetByIDAsync(TKey id)
    {
        var request = new QueryParameters { Where = $"Id = '{id}'" };
        string query = await _dapperQueryBuilder.BuildQuery<TEntity>(request, out DynamicParameters dapperParams);
        Console.WriteLine("log: info, created_at: " + DateTimeOffset.UtcNow + ", query: " + query);
        var data = await _dbConnection.QueryFirstOrDefaultAsync<TEntity>(query, dapperParams);
        return data;
    }

    /// <inheritdoc/>
    public async ValueTask<bool> UpdateAsync(TEntity t, CancellationToken cancellationToken = default)
    {
        DbSet.Update(t);
        return await SaveChangesAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async ValueTask<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FindAsync(new object[] { id }, cancellationToken);
        if (entity != null)
        {
            DbSet.Remove(entity);
            return await SaveChangesAsync(cancellationToken);
        }
        return false;
    }

    /// <summary>
    /// Saves changes to the database asynchronously with error handling and cancellation support.
    /// </summary>
    private async ValueTask<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken) > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Handle concurrency exception as needed
            return false;
        }
        catch (DbUpdateException)
        {
            // Handle update exception as needed
            return false;
        }
    }

    /// <inheritdoc/>
    public async Task<int> BulkAddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        const int chunkSize = 50; // Define the chunk size
        var entitiesList = entities.ToList(); // Convert to List for chunking
        var totalRowsAffected = 0;

        if (entitiesList.Count > 100)
        {
            // Chunk the entities into smaller batches of chunkSize
            for (int i = 0; i < entitiesList.Count; i += chunkSize)
            {
                var chunk = entitiesList.Skip(i).Take(chunkSize);

                // Add the current chunk
                await DbSet.AddRangeAsync(chunk, cancellationToken);
                totalRowsAffected += await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            // If the list size is <= 100, process normally
            await DbSet.AddRangeAsync(entitiesList, cancellationToken);
            totalRowsAffected = await _context.SaveChangesAsync(cancellationToken);
        }

        return totalRowsAffected;
    }

    /// <inheritdoc/>
    public async Task<int> BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        if (entities == null) throw new ArgumentNullException(nameof(entities));

        const int chunkSize = 150;
        var entitiesList = entities.ToList();
        var totalRowsAffected = 0;

        if (entitiesList.Count > 300)
        {

            var entityChunk = entitiesList.Chunk(chunkSize).ToList();
            foreach (var chunk in entityChunk)
            {
                DbSet.UpdateRange(chunk);
                totalRowsAffected += await _context.SaveChangesAsync(cancellationToken);
            }
        }
        else
        {
            DbSet.UpdateRange(entitiesList);
            totalRowsAffected = await _context.SaveChangesAsync(cancellationToken);
        }

        return totalRowsAffected;
    }

}
