using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Equipments.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.EquipmentActivities.Command.Update;

public class UpdateEquipmentActivityCommand : IRequest<Result>
{
	[FromRoute(Name = RouterBase.Id)]
	public long EquipmentActivityId { get; set; } = default!;
	public ActivityStatus? Status { get; set; }

	[FromBody]
	public EquipmentActivityModel EquipmentActivity { get; set; } = default!;
}
