using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Tariffs.Commands.Delete
{
    public record DeleteTariffCommand : IRequest<Result>

	{
		[FromRoute(Name = RouterBase.Id)]
		public long TariffId { get; set; } = default!;
	}
}
