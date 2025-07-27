using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Mediator;

namespace Application.Feature.BranchProducts.Queries.ListCardInv
{
    public class BranchProductCardInventoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            BranchProductCardInventoryQuery,
            Result<PaginationResponse<BranchProductCardInventoryResponse>>
        >
    {
        public async ValueTask<
            Result<PaginationResponse<BranchProductCardInventoryResponse>>
        > Handle(BranchProductCardInventoryQuery request, CancellationToken cancellationToken)
        {
            var validation = request.Validate<
                BranchProductCardInventoryQuery,
                BranchProductCardInventoryResponse
            >();

            if (validation != null)
            {
                return validation;
            }
            var query = await unitOfWork
                .Repository<InventoryDocument>()
                .QueryAsync(id => id.Status == InventoryStatus.Completed)
                .SelectMany(id =>
                    id.ProductSupplyings.Select(ps => new BranchProductCardInventoryResponse
                    {
                        ProductId = ps.ProductId,
                        TransactionAt = id.TransactionAt,
                        DocumentCode = id.Code,
                        Quantity = ps.Quantity,
                        Type = id.Type,
                    })
                )
                .Filter(request.Filter)
                .Search(request.Keyword, request.Targets)
                .Sort($"TransactionAt{OrderTerm.DELIMITER}{OrderTerm.DESC}")
                .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

            return Result<PaginationResponse<BranchProductCardInventoryResponse>>.Success(query);
        }
    }
}
