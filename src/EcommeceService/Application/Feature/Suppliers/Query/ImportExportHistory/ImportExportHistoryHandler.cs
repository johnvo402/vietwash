using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Common.QueryStringProcessing;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Inventories;
using Domain.Aggregates.Inventories.Enums;
using Mediator;

namespace Application.Feature.Suppliers.Query.ImportExportHistory
{
    public class ImportExportHistoryHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            ImportExportHistoryQuery,
            Result<PaginationResponse<ImportExportHistoryResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ImportExportHistoryResponse>>> Handle(
            ImportExportHistoryQuery request,
            CancellationToken cancellationToken
        )
        {
            var validation = request.Validate<
                ImportExportHistoryQuery,
                ImportExportHistoryResponse
            >();

            if (validation != null)
            {
                return validation;
            }
            var query = await unitOfWork
                .Repository<InventoryDocument>()
                .QueryAsync(id => id.Status == InventoryStatus.Completed)
                .SelectMany(id =>
                    id.ProductSupplyings.Select(ps => new
                    {
                        InvDocId = id.Id,
                        InvDocPublicId = id.PublicId,
                        id.TransactionAt,
                        DocumentCode = id.Code,
                        Total = ps.Quantity * ps.Price,
                        id.Type,
                        ps.SupplierId,
                    })
                )
                .GroupBy(x => x.SupplierId)
                .Select(group => new ImportExportHistoryResponse
                {
                    SupplierId = group.Key,
                    Total = group.Sum(x => x.Total),
                    TransactionAt = group.Max(x => x.TransactionAt), // hoặc tùy ý
                    DocumentCode = group
                        .OrderByDescending(x => x.TransactionAt)
                        .First()
                        .DocumentCode,
                    Type = group.First().Type,
                    InvDocId = group.First().InvDocId,
                    InvDocPublicId = group.First().InvDocPublicId,
                })
                .Filter(request.Filter)
                .Search(request.Keyword, request.Targets)
                .Sort($"TransactionAt{OrderTerm.DELIMITER}{OrderTerm.DESC}")
                .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

            return Result<PaginationResponse<ImportExportHistoryResponse>>.Success(query);
        }
    }
}
