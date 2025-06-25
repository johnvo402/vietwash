using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Accounts;
using Domain.Aggregates.Accounts.Specifications;
using Infrastructure.Constants;
using Mediator;

namespace Application.Features.Customers.Queries.ListCustomer
{
    public class ListCustomerHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListCustomerQuery, Result<PaginationResponse<ListCustomerResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListCustomerResponse>>> Handle(
            ListCustomerQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<ListCustomerQuery, ListCustomerResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Account>()
                .PagedListAsync(
                    new ListAccountSpecification([ROLE.CUSTOMER]),
                    query,
                    ListCustomerMapping.Selector(),
                    cancellationToken: cancellationToken
                );

            return Result<PaginationResponse<ListCustomerResponse>>.Success(response);
        }
    }
}
