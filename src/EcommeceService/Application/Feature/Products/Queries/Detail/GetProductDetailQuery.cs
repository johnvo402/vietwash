using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Products.Queries.Detail
{
	public record GetProductDetailQuery
		: IRequest<Result<GetProductDetailResponse>>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long ProductId { get; set; }
	}
}
