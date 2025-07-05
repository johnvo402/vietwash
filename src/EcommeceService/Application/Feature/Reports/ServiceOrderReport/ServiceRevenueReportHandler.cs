using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Orders;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Reports.Queries.ServiceOrderReport
{
    public class ServiceRevenueReportHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<
            ServiceRevenueReportQuery,
            Result<PaginationResponse<ServiceRevenueReportResponse>>
        >
    {
        public async ValueTask<Result<PaginationResponse<ServiceRevenueReportResponse>>> Handle(
            ServiceRevenueReportQuery request,
            CancellationToken cancellationToken
        )
        {
            var from = DateTimeOffset
                .FromUnixTimeSeconds(request.From)
                .ToOffset(TimeSpan.FromHours(0));
            var to = DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(0));

            var query = await unitOfWork
                .Repository<Order>()
                .QueryAsync()
                .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
                .SelectMany(o => o.OrderItems)
                .Include(x => x.Service)
                .ThenInclude(x => x.UnitRelations)
                .GroupBy(x => new
                {
                    x.ServiceId,
                    x.ServiceName,
                    x.UnitRelationId,
                    x.UnitRelationName,
                })
                .Select(group => new ServiceRevenueReportResponse
                {
                    ServiceId = group.Key.ServiceId,
                    ServiceName = group.Key.ServiceName ?? string.Empty,
                    UnitId = group.Key.UnitRelationId,
                    UnitName = group.Key.UnitRelationName ?? string.Empty,
                    TotalOrderCount = group.Select(x => x.OrderId).Distinct().Count(),
                    TotalNetRevenue = group.Sum(x => x.Price * x.Quantity),
                    TotalDiscount = 0,
                    TotalRevenue = group.Sum(x => x.Price * x.Quantity),
                })
                .Search(request.Keyword, request.Targets)
                .Sort($"TotalOrderCount{OrderTerm.DELIMITER}{OrderTerm.DESC}")
                .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

            return Result<PaginationResponse<ServiceRevenueReportResponse>>.Success(query);
        }
    }
}
