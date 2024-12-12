using MediatR;
using Micro.Shared.Model;
using ProductService.Domain.DTOs;
using ProductService.Domain.Entities;

namespace ProductService.Application.Commands;

public record CreateProductCommand(ApiRequestPost<ProductCreateDto> Request) : IRequest<ApiResponse<string>>;

