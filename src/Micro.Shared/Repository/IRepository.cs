namespace Micro.Shared.Repository;

public interface IRepository<TEntity, TKey>
       where TEntity : class, new()
       where TKey : IEquatable<TKey>
{
    TEntity Create(TEntity t);

    TEntity? GetByID(TKey id);

    IEnumerable<TEntity> GetAll();

    bool Update(TEntity t);

    bool Delete(TKey id);

    ValueTask<TEntity> CreateAsync(TEntity t);

    ValueTask<TEntity?> GetByIDAsync(TKey id);

    Task<IEnumerable<TEntity>> GetAllAsync();

    ValueTask<bool> UpdateAsync(TEntity t);

    ValueTask<bool> DeleteAsync(TKey id);
}
