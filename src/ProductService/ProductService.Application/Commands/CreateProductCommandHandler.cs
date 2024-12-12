using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Micro.Shared.Model;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<string>>
{
    private readonly IProductRepository _repository;

    public CreateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Request?.Objects?.Count == 0 || request.Request?.Object == null)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid request data",
            };
        }
        if (request.Request.Objects?.Count > 0)
        {
            var products = request.Request.Objects.Select(o => new Product(o.Name ?? "Name", o.Price, o.StockQuantity));
            var effectRows = await _repository.BulkAddAsync(products, cancellationToken);

            return new ApiResponse<string>
            {
                Success = true,
                Value = effectRows.ToString(),
            };
        }

        var product = new Product(request.Request.Object.Name ?? "Name", request.Request.Object.Price, request.Request.Object.StockQuantity);
        await _repository.AddAsync(product, cancellationToken);
        return new ApiResponse<string>
        {
            Success = true,
            Value = product.Id.ToString(),
        };
    }
}
