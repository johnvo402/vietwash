    using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Feature.Common.Projections.Tariffs;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffCommand : TariffModel, IRequest<CreateTariffResponse>
    {

    }
}