using Micro.Shared.Repository;
using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces;

public interface IProductRepository : IRepository<Product, Guid>
{

}
