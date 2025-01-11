using MediatR;
using ProductService.Domain.Entities;
using ErrorOr;
using Micro.Shared.Model;

namespace ProductService.Application.Queries;
public record GetProductsQuery(QueryParameters query) : IRequest<ErrorOr<IEnumerable<Product>>>;
