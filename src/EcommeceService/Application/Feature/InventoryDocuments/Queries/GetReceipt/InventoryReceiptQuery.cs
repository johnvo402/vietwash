using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.InventoryDocuments.Queries.GetReceipt
{
    public class InventoryReceiptQuery : IRequest<Result<InventoryReceiptResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long Id { get; set; }

        [FromRoute(Name = nameof(SupplierId))]
        public long SupplierId { get; set; }
    }
}
