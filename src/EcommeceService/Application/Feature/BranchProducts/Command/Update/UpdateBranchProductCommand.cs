using Application.Feature.Common.Projections.BranchProducts;
using Application.Feature.Common.Projections.Services;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.BranchProducts.Command.Update
{
	public class UpdateBranchProductCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long BranchProductId { get; set; } = default!;

		[FromBody]
		public UpdateBranchProductModel BranchProduct { get; set; } = default!;
	}
}
