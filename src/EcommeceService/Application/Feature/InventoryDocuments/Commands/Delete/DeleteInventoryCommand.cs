using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.InventoryDocuments.Commands.Delete
{
    public class DeleteInventoryCommand : IRequest<Result>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long InventoryId { get; set; }
    }
}
