using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Products.Queries.Detail
{
	public record GetProductDetailQuery([FromRoute(Name = RouterBase.Id)] long ProductId)
		: IRequest<Result<GetProductDetailResponse>>;
}
