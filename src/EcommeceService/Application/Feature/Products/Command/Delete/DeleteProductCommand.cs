using Contracts.ApiWrapper;
using Mediator;
namespace Application.Feature.Products.Command.Delete;

public record DeleteProductCommand(long ProductId) : IRequest<Result>;
