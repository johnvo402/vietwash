

using Application.Common.Auth;
using Application.Common.Interfaces.Services.DistributedCache;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Features.Accounts.Queries.List;
using Application.Features.Common.Helpers;
using Domain.Aggregates.Accounts.Specifications;
using Domain.Aggregates.Accounts;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;
using JohnChum.SharedKernel.Extensions;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;

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
                    new ListAccountSpecification(["CUSTOMER"]),
                    query.ValidateQuery().ValidateFilter(typeof(ListCustomerResponse))
                );
        }

    }
}
