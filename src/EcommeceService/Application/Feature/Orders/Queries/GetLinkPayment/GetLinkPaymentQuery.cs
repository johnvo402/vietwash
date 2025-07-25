using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [FromQuery]
        public string ReturnUrl { get; set; }
    }
}
