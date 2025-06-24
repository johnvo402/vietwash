using Application.Feature.Common.Projections.Tariffs;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffCommand : IRequest<Result<UpdateTariffResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string TariffId { get; set; } = string.Empty;

        [FromBody]
        public TariffModel? Tariff { get; set; }
    }
}
