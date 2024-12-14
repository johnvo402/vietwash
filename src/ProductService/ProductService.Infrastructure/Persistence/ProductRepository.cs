using Micro.Shared.Repository;
using Microsoft.EntityFrameworkCore;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using ProductService.Infrastructure.Persistence;

namespace ProductService.Infrastructure.Repositories;

public class ProductRepository : Repository<ApplicationDbContext, Product, Guid>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

}
