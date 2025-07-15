using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.ApiWrapper;
using Domain.Aggregates.Equipments.Enums;
using Mediator;

namespace Application.Feature.EquipmentActivities.Command.Create
{
	public class CreateEquipmentActivityCommand : EquipmentActivityModel, IRequest<Result>
	{
		public long EquipmentId { get; set; }
		public TypeActivity Type { get; set; }
	}
}
