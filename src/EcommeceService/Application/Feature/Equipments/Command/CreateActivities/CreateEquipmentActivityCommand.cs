using Application.Feature.Common.Projections.EquipmentActivities;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Equipments.Command.CreateActivities
{
	public class CreateEquipmentActivityCommand : IRequest<Result>
	{
		[FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }

		[FromBody]
		public EquipmentActivityModel EquipmentActivity { get; set; }

	}
}
