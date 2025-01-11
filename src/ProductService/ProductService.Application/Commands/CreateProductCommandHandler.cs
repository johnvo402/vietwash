using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Http;
using Micro.Shared.Extensions;
using Micro.Shared.Infrastructure.CurrentUserProvider;
using Utilities;
using ErrorOr;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ErrorOr<string>>
{
    private readonly IProductRepository _repository;


    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;

    }

    public async Task<ErrorOr<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {


        if (request.Request?.Objects?.Count == 0 && request.Request?.Object == null)
        {
            return Error.Failure("backend.product.empty");
        }
        if (request?.Request?.Objects?.Count > 0)
        {
            var products = request.Request.Objects.Select(o => new Product(o.Name ?? "Name", o.Price, o.Stock, Generator.GenerateKeywords(new List<string> { o.Name ?? "" })));
            var effectRows = await _repository.BulkAddAsync(products, cancellationToken);

            return effectRows.ToString();
        }

        var product = new Product(request?.Request?.Object?.Name ?? "Name", request?.Request?.Object?.Price ?? 0, request?.Request?.Object?.Stock ?? 0, Generator.GenerateKeywords(new List<string> { request?.Request?.Object?.Name ?? "" }));
        await _repository.CreateAsync(product, cancellationToken);
        return product.Id.ToString();
    }
}
