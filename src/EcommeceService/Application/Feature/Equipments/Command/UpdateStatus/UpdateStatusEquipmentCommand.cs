using Contracts.ApiWrapper;
using Contracts.Routers;
using Domain.Aggregates.Equipments.Enums;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Equipments.Command.UpdateStatus
{
	public class UpdateStatusEquipmentCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
		public long EquipmentId { get; set; }
		public EquipmentStatus? Status { get; set; }
	}
}
