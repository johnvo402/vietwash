using Application.Feature.Common.Projections.Units;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Units.Command.Update
{
    public class UpdateUnitCommand : IRequest<Result<UpdateUnitResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long UnitId { get; set; }

        [FromBody]
        public UnitModel Unit { get; set; } = new UnitModel();
    }
}
