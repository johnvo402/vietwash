using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Reports.RevenueReport;

public class RevenueReportHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<RevenueReportQuery, Result<PaginationResponse<RevenueReportResponse>>>
{
    public async ValueTask<Result<PaginationResponse<RevenueReportResponse>>> Handle(
        RevenueReportQuery request,
        CancellationToken cancellationToken
    )
    {
        
        var from = DateTimeOffset.FromUnixTimeSeconds(request.From).ToOffset(TimeSpan.FromHours(7));
        var to = DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(7));

        var query = await unitOfWork
            .Repository<Order>()
            .QueryAsync()
            .Where(o => (o.CreatedAt >= from && o.CreatedAt <= to) && (o.Status == OrderStatus.Completed))
            .GroupBy(o => o.CreatedAt.Date)
            .Select(g => new RevenueReportResponse
            {
                Date = DateOnly.FromDateTime(g.Key),
                OrderQuantity = g.Count(),
                CustomerQuantity = g.Select(x => x.CustomerId).Distinct().Count(),
                TotalRevenue = g.Sum(x => x.Amount),
                TotalDiscount = g.Sum(x => x.DiscountValue),
                TotalNetRevenue = g.Sum(x => x.Total),
                AverageRevenuePerOrder =
                    g.Count() == 0 ? 0 : Math.Round(g.Sum(x => x.Amount) / g.Count(), 2),
                AverageRevenuePerCustomer =
                    g.Select(x => x.CustomerId).Distinct().Count() == 0
                        ? 0
                        : Math.Round(
                            g.Sum(x => x.Amount) / g.Select(x => x.CustomerId).Distinct().Count(),
                            2
                        ),
            })
            .Search(request.Keyword, request.Targets)
            .Sort($"TotalRevenue{OrderTerm.DELIMITER}{OrderTerm.DESC}")
            .ToPagedListAsync(request.Page, request.PageSize, cancellationToken);

        return Result<PaginationResponse<RevenueReportResponse>>.Success(query);
    }
}
