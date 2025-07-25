using Application.Feature.Common.Projections.Equipments;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Equipments.Command.Update
{
    public class UpdateEquipmentCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long EquipmentId { get; set; } = default!;

        [FromBody]
        public EquipmentUpdateModel Equipment { get; set; } = default!;
    }
}
