using Application.Common.Interfaces.UnitOfWorks;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListCustomerQuery, PaginationResponse<ListCustomerResponse>>
    {
        public async ValueTask<PaginationResponse<ListCustomerResponse>> Handle(
            ListCustomerQuery query,
            CancellationToken cancellationToken
        )
        {
            return await unitOfWork
                .Repository<Account>()
                .PagedListAsync<ListCustomerResponse>(
                    new ListAccountSpecification([ROLE.CUSTOMER]),
                    query.ValidateQuery().ValidateFilter(typeof(ListCustomerResponse))
                );
        }
    }
}
