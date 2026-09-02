using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;

namespace Application.Feature.Orders.Queries.GetLinkPayment
{
    public class GetLinkPaymentQuery : IRequest<Result<CreatePaymentResult>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long OrderId { get; set; }
    }
}
