using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Micro.Shared.Model;
using Microsoft.AspNetCore.Http;
using Micro.Shared.Extensions;

public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ApiResponse<string>>
{
    private readonly IProductRepository _repository;
        private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateProductCommandHandler(IProductRepository repository, IHttpContextAccessor httpContextAccessor )
    {
        _repository = repository;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<ApiResponse<string>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
         var userAccess = _httpContextAccessor.HttpContext?.GetUserAccessOrDefault();
        if (userAccess == null || string.IsNullOrEmpty(userAccess.UserId))
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
        if (request.Request.Objects?.Count > 0)
        {
            var products = request.Request.Objects.Select(o => new Product(o.Name ?? "Name", o.Price, o.StockQuantity, userAccess.UserId));
            var effectRows = await _repository.BulkAddAsync(products, cancellationToken);

            return new ApiResponse<string>
            {
                Success = true,
                Value = effectRows.ToString(),
            };
        }

        var product = new Product(request.Request.Object.Name ?? "Name", request.Request.Object.Price, request.Request.Object.StockQuantity, userAccess.UserId);
        await _repository.CreateAsync(product, cancellationToken);
        return new ApiResponse<string>
        {
            Success = true,
            Value = product.Id.ToString(),
        };
    }
}
