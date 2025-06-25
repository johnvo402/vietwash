using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Branches.Specifications;
using Mediator;

namespace Application.Features.Branches.Queries
{
    public class ListBranchHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<ListBranchQuery, Result<PaginationResponse<ListBranchResponse>>>
    {
        public async ValueTask<Result<PaginationResponse<ListBranchResponse>>> Handle(
            ListBranchQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListBranchQuery, ListBranchResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<Domain.Aggregates.Branches.Branch>()
                .PagedListAsync<ListBranchResponse>(
                    new ListBranchSpecification(),
                    request,
                    ListBranchMapping.Selector(),
                    cancellationToken
                );

            return Result<PaginationResponse<ListBranchResponse>>.Success(response);
        }
    }
}
