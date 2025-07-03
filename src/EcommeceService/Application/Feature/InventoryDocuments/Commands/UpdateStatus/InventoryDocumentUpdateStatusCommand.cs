using Application.Feature.Common.Projections.Inventories;
using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.InventoryDocuments.Commands.UpdateStatus
{
    public class InventoryDocumentUpdateStatusCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }

        [FromBody]
        public InventoryDocumentUpdateStatus ModelStatus { get; set; } = default!;
    }
}
