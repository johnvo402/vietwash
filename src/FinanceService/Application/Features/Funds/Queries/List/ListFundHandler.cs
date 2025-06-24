using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Funds;
using Domain.Aggregates.Funds.Specifications;
using Mediator;

namespace Application.Features.Funds.Queries.List
{
    public class ListFundHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListFundQuery, Result<PaginationResponse<ListFundResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListFundResponse>>> Handle(
            ListFundQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListFundQuery, ListFundResponse>();

            if (validation != null)
            {
                return validation;
            }
            return Result<PaginationResponse<ListFundResponse>>.Success(
                await unitOfWork
                    .DynamicReadOnlyRepository<Fund>()
                    .PagedListAsync(
                        new ListFundSpecification(request.From, request.To),
                        request,
                        ListFundMapping.Selector(),
                        cancellationToken
                    )
            );
        }
    }
}
