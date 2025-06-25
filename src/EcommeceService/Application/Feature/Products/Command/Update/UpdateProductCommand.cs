using Application.Feature.Common.Projections.Products;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
namespace Application.Feature.Products.Command.Update
{
	public class UpdateProductCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long ProductId { get; set; } = default!;

		[FromBody]
		public ProductModel Product { get; set; } = default!;
	}
}
