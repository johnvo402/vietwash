using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Inventories.Enums;
using Domain.Aggregates.Suppliers;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Reports.ProductSupplierReport
{
    public class ProductSupplierReportHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            ProductSupplierReportQuery,
            Result<PaginationResponse<ProductSupplierReportResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ProductSupplierReportResponse>>> Handle(
            ProductSupplierReportQuery request,
            CancellationToken cancellationToken
        )
        {
            var from = DateTimeOffset
                .FromUnixTimeSeconds(request.From)
                .ToOffset(TimeSpan.FromHours(0));
            var to = DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(0));

            var query = await unitOfWork
                .Repository<Supplier>()
                .QueryAsync()
                .SelectMany(
                    s => s.ProductSupplyings,
                    (supplier, ps) => new { Supplier = supplier, ProductSupplying = ps }
                )
                .Where(x =>
                    x.ProductSupplying.CreatedAt >= from
                    && x.ProductSupplying.CreatedAt <= to
                    && x.ProductSupplying.InventoryDocument.Status == InventoryStatus.Completed
                    && request.BranchIds != null
                    && x.ProductSupplying.InventoryDocument.BranchId.HasValue
                    && request.BranchIds.Contains(
                        x.ProductSupplying.InventoryDocument.BranchId.Value
                    )
                )
                .GroupBy(x => new
                {
                    x.Supplier.Id,
                    x.Supplier.Code,
                    x.Supplier.Name,
                    BranchId = x.ProductSupplying.InventoryDocument.BranchId,
                })
                .Select(group => new ProductSupplierReportResponse
                {
                    SupplierId = group.Key.Id,
                    Code = group.Key.Code ?? string.Empty,
                    Name = group.Key.Name ?? string.Empty,

                    SupplierProductTypeCount = group
                        .Select(x => x.ProductSupplying.ProductId)
                        .Distinct()
                        .Count(),
                    BranchId = group.Key.BranchId ?? 0,
                    ImportedValueTotal = group
                        .Where(x =>
                            (x.ProductSupplying.InventoryDocument.Type == InventoryType.Import)
                        )
                        .Sum(x => x.ProductSupplying.Price * x.ProductSupplying.Quantity),

                    ExportedValueTotal = group
                        .Where(x =>
                            x.ProductSupplying.InventoryDocument.Type == InventoryType.Export
                        )
                        .Sum(x => x.ProductSupplying.Price * x.ProductSupplying.Quantity),
                })
                .Search(request.Keyword, request.Targets)
                .Sort($"ImportedValueTotal{OrderTerm.DELIMITER}{OrderTerm.DESC}")
                .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

            return Result<PaginationResponse<ProductSupplierReportResponse>>.Success(query);
        }
    }
}
