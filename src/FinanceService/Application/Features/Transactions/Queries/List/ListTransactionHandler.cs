using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using Mediator;

namespace Application.Features.Transactions.Queries.List
{
    public class ListTransactionHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListTransactionQuery, Result<PaginationResponse<ListTransactionResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListTransactionResponse>>> Handle(
            ListTransactionQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListTransactionQuery, ListTransactionResponse>();

            if (validation != null)
            {
                return validation;
            }
            return Result<PaginationResponse<ListTransactionResponse>>.Success(
                await unitOfWork
                    .DynamicReadOnlyRepository<Transaction>()
                    .PagedListAsync(
                        new ListTransactionSpecification(),
                        request,
                        ListTransactionMapping.Selector(),
                        cancellationToken
                    )
            );
        }
    }
}
