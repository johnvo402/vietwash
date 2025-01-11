using ErrorOr;
using MediatR;
using Micro.Shared.Model;
using ProductService.Domain.Entities;

namespace ProductService.Application.Commands;

public record CreateProductCommand(ApiRequestPost<CreateUpdateProductCommandDto> Request) : IRequest<ErrorOr<string>>;

public record CreateUpdateProductCommandDto(string Name, decimal Price, int Stock);