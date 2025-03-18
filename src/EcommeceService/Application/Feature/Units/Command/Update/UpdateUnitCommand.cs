using Application.Feature.Common.Projections.Units;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;


namespace Application.Feature.Units.Command.Update
{
	public class UpdateUnitCommand : IRequest<UpdateUnitResponse>
	{
		[FromRoute(Name = RouterBase.Id)]
		public string UnitId { get; set; } = string.Empty;
		[FromBody]
		public UnitModel Unit { get; set; } = new UnitModel();
	}
}
