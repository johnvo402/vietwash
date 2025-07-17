using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.ApiWrapper;
using Mediator;

namespace Application.Feature.EquipmentActivities.Command.Create
{
	public class CreateEquipmentActivityCommand 
		: EquipmentActivityModel, IRequest<Result>;
}
