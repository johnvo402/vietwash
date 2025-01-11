using MediatR;
using Micro.Shared.Model;

namespace ProductService.Application.Commands;

public record UpdateProductCommand(ApiRequestPut<CreateUpdateProductCommandDto> Request) : IRequest<ApiResponse<bool>>;
