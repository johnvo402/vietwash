using Application.Feature.Common.Projections.Tariffs;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Tariffs.Commands.Create
{
    public class CreateTariffCommand : TariffModel, IRequest<Result>;
}
