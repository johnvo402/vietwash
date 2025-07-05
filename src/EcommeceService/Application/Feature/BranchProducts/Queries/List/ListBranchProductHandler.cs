using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Products;
using Domain.Aggregates.Products.Specifications;
using Mediator;

namespace Application.Feature.BranchProducts.Queries.List
{
    public class ListBranchProductHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            ListBranchProductQuery,
            Result<PaginationResponse<ListBranchProductResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ListBranchProductResponse>>> Handle(
            ListBranchProductQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<ListBranchProductQuery, ListBranchProductResponse>();

            if (validation != null)
            {
                return validation;
            }
            var response = await unitOfWork
                .DynamicReadOnlyRepository<BranchProduct>()
                .PagedListAsync(
                    new ListBranchProductSpecification(),
                    request,
                    ListBranchProductMapping.Selector(),
                    cancellationToken
                );

            return Result<PaginationResponse<ListBranchProductResponse>>.Success(response);
        }
    }
}
