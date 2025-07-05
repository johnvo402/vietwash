using Application.Feature.Common.Projections.Tariffs;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Tariffs.Commands.Update
{
	public class UpdateTariffCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long TariffId { get; set; } = default!;

		[FromBody]
		public TariffModel Tariff { get; set; } = default!;
	}
}
