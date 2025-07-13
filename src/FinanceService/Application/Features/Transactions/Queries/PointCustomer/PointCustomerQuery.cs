using Contracts.ApiWrapper;
using Mediator;

namespace Application.Features.Transactions.Queries.PointCustomer
{
    public class PointCustomerQuery : IRequest<Result<PointCustomerResponse>>;
}
