using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Transactions.Queries.PointCustomer
{
    public class PointCustomerQuery : IRequest<Result<PointCustomerResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long CustomerId { get; set; }
    };
}
