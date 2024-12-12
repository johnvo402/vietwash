using MediatR;
using Micro.Shared.Model;
using ProductService.Domain.DTOs;

namespace ProductService.Application.Commands;

public record UpdateProductCommand(ApiRequestPut<ProductCreateDto> Request) : IRequest<ApiResponse<bool>>;
