using MediatR;
using ProductService.Application.Commands;
using ProductService.Application.Interfaces;
using ProductService.Domain.Entities;
using Micro.Shared.Model;

public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, ApiResponse<bool>>
{
    private readonly IProductRepository _repository;

    public UpdateProductCommandHandler(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _repository.GetByIDAsync(request.Request.Id, cancellationToken);
        if (product == null)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Product not found",
            };
        }
        product.Name = request.Request.Object.Name;
        product.Price = request.Request.Object.Price;
        product.StockQuantity = request.Request.Object.StockQuantity;
        var result = await _repository.UpdateAsync(product, cancellationToken);
        if (!result)
        {
            return new ApiResponse<bool>
            {
                Success = false,
                Message = "Failed to update product",
            };
        }
        return new ApiResponse<bool>
        {
            Success = false,
            Message = "Product updated successfully",
            Value = true,
        };
    }
}
