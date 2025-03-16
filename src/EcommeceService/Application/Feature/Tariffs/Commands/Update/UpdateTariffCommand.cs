using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Tariffs.Commands.Update
{
    public class UpdateTariffCommand : IRequest<UpdateTariffResponse>
    {
        [FromRoute(Name = RouterBase.Id)]
        public string TariffId { get; set; } = string.Empty;

        [FromForm]
        public UpdateTariff? Tariff { get; set; }
    }
    public class UpdateTariff : TariffModel
    {

    }
}