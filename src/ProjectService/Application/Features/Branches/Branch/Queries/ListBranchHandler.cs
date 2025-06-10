using Application.Common.Interfaces.UnitOfWorks;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Common.QueryStringProcessing;
using Domain.Aggregates.Branches.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Responses;
using Mediator;

namespace Application.Features.Branches.Branch.Queries
{
    public class ListBranchHandler(IUnitOfWork unitOfWork) : IRequestHandler<ListBranchQuery, PaginationResponse<ListBranchResponse>>
    {
        public async ValueTask<PaginationResponse<ListBranchResponse>> Handle(ListBranchQuery request,
            CancellationToken cancellationToken) => await unitOfWork.Repository<Domain.Aggregates.Branches.Branch>()
                                                                    .PagedListAsync<ListBranchResponse>
                                                                    (
                                                                          new ListBranchSpecification(),
                                                                          request.ValidateQuery().ValidateFilter(typeof(ListBranchResponse))
                                                                    );
    }
}
