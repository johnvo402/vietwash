using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Responses;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Spectifications;
using Mediator;

namespace Application.Feature.InventoryDocuments.Queries.List
{
    public class ListInventoryDocumentHandler(
        IUnitOfWork unitOfWork,
        ICurrentAccount currentAccount
    )
        : IRequestHandler<
            ListInventoryDocumentQuery,
            Result<PaginationResponse<ListInventoryDocumentResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ListInventoryDocumentResponse>>> Handle(
            ListInventoryDocumentQuery query,
            CancellationToken cancellationToken
        )
        {
            var validation = query.Validate<
                ListInventoryDocumentQuery,
                ListInventoryDocumentResponse
            >();

            if (validation != null)
            {
                return validation;
            }

            var listBranchUser = currentAccount
                .Session!.Branches!.Select(x => long.Parse(x))
                .ToList();
            var response = await unitOfWork
                .DynamicReadOnlyRepository<InventoryDocument>()
                .PagedListAsync(
                    new ListInventoryDocumentSpecification(listBranchUser),
                    query,
                    ListInventoryDocumentMapping.Selector(),
                    cancellationToken: cancellationToken
                );

            return Result<PaginationResponse<ListInventoryDocumentResponse>>.Success(response);
        }
    }
}
