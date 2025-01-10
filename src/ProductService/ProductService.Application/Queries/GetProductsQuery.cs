using MediatR;
using ProductService.Domain.DTOs;
using Micro.Shared.Model;
using ProductService.Domain.Entities;

namespace ProductService.Application.Queries;
public record GetProductsQuery() : IRequest<IEnumerable<Product>>;
