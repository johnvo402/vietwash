using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
namespace Application.Feature.Products.Command.Delete;

public class DeleteProductCommand : IRequest<Result>
{
	[FromRoute(Name = RouterBase.Id)]
	public long ProductId { get; set; } = default!;
}
