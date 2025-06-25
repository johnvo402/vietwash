using Application.Feature.Common.Projections.Units;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Units.Command.Create
{
    public class CreateUnitCommand : UnitModel, IRequest<Result>;
}
