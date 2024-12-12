using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository
{
    IQueryable<Product> GetQueryableAsync(CancellationToken cancellationToken);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Guid> AddAsync(Product product, CancellationToken cancellationToken);
    Task<int> BulkAddAsync(IEnumerable<Product> products, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken);
}
