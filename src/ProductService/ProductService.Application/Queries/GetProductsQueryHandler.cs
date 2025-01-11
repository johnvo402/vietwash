using MediatR;
using Micro.Shared.Model;
using ProductService.Application.Interfaces;
using ProductService.Application.Queries;
using ProductService.Domain.Entities;
using ErrorOr;

namespace ProductService.Application.Queries;

public class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, ErrorOr<IEnumerable<Product>>>
{
    private readonly IProductRepository _repository;

    public GetProductsQueryHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ErrorOr<IEnumerable<Product>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _repository.GetAllAsync(request.query);
        return products.ToList();
    }


}
