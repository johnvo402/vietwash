using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Equipments.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.EquipmentActivities.Command.UpdateStatus
{
	public class UpdateStatusEquipmentActivityCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long EquipmentActivityId { get; set; }
		public ActivityStatus? Status { get; set; }
	}
}
