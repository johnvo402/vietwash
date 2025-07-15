using Application.Feature.Common.Projections.Equipments;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.Equipments.Command.Create
{
    public class CreateEquipmentCommand : EquipmentModel, IRequest<Result>;
}
