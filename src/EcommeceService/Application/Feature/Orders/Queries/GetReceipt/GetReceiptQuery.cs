using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Feature.Orders.Queries.GetReceipt
{
    public class GetReceiptQuery : IRequest<Result<GetReceiptResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long OrderId { get; set; }
    }
}
