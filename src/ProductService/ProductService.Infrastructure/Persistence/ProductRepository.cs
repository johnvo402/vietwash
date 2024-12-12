using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Guid> AddAsync(Product product, CancellationToken cancellationToken)
    {
        await _context.Products.AddAsync(product, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return product.Id;
    }

    public async Task<int> BulkAddAsync(IEnumerable<Product> products, CancellationToken cancellationToken)
    {
        const int batchSize = 100;
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            for (int i = 0; i < products.Count(); i += batchSize)
            {
                var productsToAdd = products.Skip(i).Take(batchSize);
                await _context.Products.AddRangeAsync(productsToAdd, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }
            await transaction.CommitAsync(cancellationToken);
            return products.Count();
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Products.FindAsync(id, cancellationToken);
    }

    public IQueryable<Product> GetQueryableAsync(CancellationToken cancellationToken)
    {
        return _context.Products.AsQueryable();
    }

    public async Task<bool> UpdateAsync(Product product, CancellationToken cancellationToken)
    {
        try
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
