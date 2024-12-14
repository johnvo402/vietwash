using System.Threading;

namespace Micro.Shared.Repository;

public interface IRepository<TEntity, TKey>
       where TEntity : class, new()
       where TKey : IEquatable<TKey>
{
    // Synchronous methods
    TEntity Create(TEntity t);

    TEntity? GetByID(TKey id);

    IQueryable<TEntity> GetAll();

    bool Update(TEntity t);

    bool Delete(TKey id);

    // Asynchronous methods with CancellationToken
    ValueTask<TEntity> CreateAsync(TEntity t, CancellationToken cancellationToken = default);

    ValueTask<TEntity?> GetByIDAsync(TKey id, CancellationToken cancellationToken = default);

    Task<IQueryable<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);

    ValueTask<bool> UpdateAsync(TEntity t, CancellationToken cancellationToken = default);

    ValueTask<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task<int> BulkAddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
