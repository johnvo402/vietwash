using MediatR;
using Micro.Shared.Model;
using ProductService.Domain.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries;
using ProductService.Domain.Entities;

namespace ProductService.Application.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, IQueryable<Product>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<IQueryable<Product>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = _repository.GetAllAsync(cancellationToken);
        return products.Result;
    }


}
