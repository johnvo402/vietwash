using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Tariffs.Queries.Detail
{
	public class GetTariffDetailQuery : IRequest<Result<GetTariffDetailResponse>>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long TariffId { get; set; }
	}
}
