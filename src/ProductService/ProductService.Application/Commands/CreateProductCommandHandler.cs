using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Http;
using Micro.Shared.Extensions;
using Micro.Shared.Infrastructure.CurrentUserProvider;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<string>>
{
    private readonly IProductRepository _repository;
    private readonly ICurrentUser _userAccess;

    public CreateProductCommandHandler(IProductRepository repository, ICurrentUser userAccess )
    {
        _repository = repository;
        _userAccess = userAccess;
    }

    public async Task<ApiResponse<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        
        if (_userAccess == null || string.IsNullOrEmpty(_userAccess.Id.ToString()))
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "User ID not found in claims"
            };
        }
        if (request.Request?.Objects?.Count == 0 && request.Request?.Object == null)
        {
            return new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid request data",
            };
        }
        if (request?.Request?.Objects?.Count > 0)
        {
            var products = request.Request.Objects.Select(o => new Product(o.Name ?? "Name", o.Price, o.StockQuantity, _userAccess.Id.ToString()));
            var effectRows = await _repository.BulkAddAsync(products, cancellationToken);

            return new ApiResponse<string>
            {
                Success = true,
                Value = effectRows.ToString(),
            };
        }

        var product = new Product(request?.Request?.Object?.Name ?? "Name", request?.Request?.Object?.Price ?? 0, request?.Request?.Object?.StockQuantity ?? 0, _userAccess.Id.ToString());
        await _repository.CreateAsync(product, cancellationToken);
        return new ApiResponse<string>
        {
            Success = true,
            Value = product.Id.ToString(),
        };
    }
}
