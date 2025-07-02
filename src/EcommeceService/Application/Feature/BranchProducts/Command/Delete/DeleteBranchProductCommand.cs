using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;


namespace Application.Feature.BranchProducts.Command.Delete
{
	public record DeleteBranchProductCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long BranchProductId { get; set; } = default!;
	}
}
