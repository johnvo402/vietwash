using Contracts.ApiWrapper;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.EInvoices.Queries.GetByOrderId
{
    public class GetEInvoiceByOrderIdQuery : IRequest<Result<GetEInvoiceByOrderIdResponse>>
    {
        [FromRoute(Name = "OrderId")]
        public long OrderId { get; set; }
    }
}
