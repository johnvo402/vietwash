using System.Threading;
using Micro.Shared.Model;

namespace Micro.Shared.Repository;

public interface IRepository<TEntity, TKey>
       where TEntity : class
       where TKey : IEquatable<TKey>
{
    // Synchronous methods
    TEntity Create(TEntity t);

    TEntity? GetByID(TKey id);

    IEnumerable<TEntity> GetAll(QueryParameters? param = default);

    bool Update(TEntity t);

    bool Delete(TKey id);

    // Asynchronous methods with CancellationToken
    ValueTask<TEntity> CreateAsync(TEntity t, CancellationToken cancellationToken = default);

    ValueTask<TEntity?> GetByIDAsync(TKey id);

    Task<IEnumerable<TEntity>> GetAllAsync(QueryParameters? param = default);

    ValueTask<bool> UpdateAsync(TEntity t, CancellationToken cancellationToken = default);

    ValueTask<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);

    Task<int> BulkAddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);

    Task<int> BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
}
