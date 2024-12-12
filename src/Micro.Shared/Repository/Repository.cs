using Microsoft.EntityFrameworkCore;

namespace Micro.Shared.Repository;

public abstract class Repository<TContext, TEntity, TKey> : IRepository<TEntity, TKey>
       where TContext : DbContext
       where TEntity : class, new()
       where TKey : IEquatable<TKey>
{
    protected readonly TContext _context;
    protected DbSet<TEntity> DbSet { get; }

    // Constructor that takes TContext and initializes the DbSet
    public Repository(TContext context)
    {
        _context = context;
        DbSet = context.Set<TEntity>();
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
    public IEnumerable<TEntity> GetAll() => DbSet.ToList();

    /// <inheritdoc/>
    public async Task<IEnumerable<TEntity>> GetAllAsync() => await DbSet.ToListAsync(); // Make this async

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
    public async ValueTask<TEntity> CreateAsync(TEntity t)
    {
        var result = await DbSet.AddAsync(t);
        await _context.SaveChangesAsync();
        return result.Entity;
    }

    /// <inheritdoc/>
    public async ValueTask<TEntity?> GetByIDAsync(TKey id) => await DbSet.FindAsync(id);

    /// <inheritdoc/>
    public async IAsyncEnumerable<TEntity> GetAllAsyncEnumerable()
    {
        await foreach (var entity in DbSet.AsAsyncEnumerable())
        {
            yield return entity;
        }
    }
    private async ValueTask<bool> SaveChangesAsync()
    {
        try
        {
            return await _context.SaveChangesAsync() > 0;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
        catch (DbUpdateException)
        {
            return false;
        }
    }
    /// <inheritdoc/>
    public async ValueTask<bool> UpdateAsync(TEntity t)
    {
        DbSet.Update(t);
        return await SaveChangesAsync();
    }

    /// <inheritdoc/>
    public async ValueTask<bool> DeleteAsync(TKey id)
    {
        var entity = await DbSet.FindAsync(id);
        if (entity != null)
        {
            DbSet.Remove(entity);
            return await SaveChangesAsync();
        }
        return false;
    }
}
