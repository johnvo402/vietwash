using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.InventoryDocuments.Queries.Detail
{
    public class InventoryDocumentDetailQuery : IRequest<Result<InventoryDocumentDetailResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }
    }
}
