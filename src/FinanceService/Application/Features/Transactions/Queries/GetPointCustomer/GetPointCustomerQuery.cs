using Contracts.ApiWrapper;
using Contracts.Routers;
using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace Application.Features.Transactions.Queries.GetPointCustomer
{
    public class GetPointCustomerQuery : IRequest<Result<GetPointCustomerResponse>>
    {
        [FromRoute(Name = RouterBase.Id)]
        public long CustomerId { get; set; }
    };
}
