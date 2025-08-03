using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.RevenueReport;
using Contracts.ApiWrapper;
using Contracts.Dtos.Models;
using Contracts.Dtos.Responses;
using Contracts.Extensions.QueryExtensions;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Reports.FinancialReport
{
    public class FinancialReportHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<FinancialReportQuery, Result<FinancialReportResponse>>
    {
        public async ValueTask<Result<FinancialReportResponse>> Handle(
            FinancialReportQuery request,
            CancellationToken cancellationToken
        )
        {
            var from = DateTimeOffset
                .FromUnixTimeSeconds(request.From)
                .ToOffset(TimeSpan.FromHours(0));
            var to = DateTimeOffset.FromUnixTimeSeconds(request.To).ToOffset(TimeSpan.FromHours(0));

            var groupedOrders = await unitOfWork
                .Repository<Order>()
                .QueryAsync()
                .Where(o =>
                    o.CreatedAt >= from
                    && o.CreatedAt <= to
                    && request.BranchIds != null
                    && request.BranchIds.Contains(o.BranchId)
                )
                .GroupBy(o => o.Status)
                .Select(g => new
                {
                    Status = g.Key,
                    TotalAmount = g.Sum(o => o.Amount),
                    TotalDiscount = g.Sum(o => o.DiscountValue),
                    TotalPoint = g.Sum(o => o.Point),
                })
                .ToListAsync(cancellationToken);

            var completed = groupedOrders.FirstOrDefault(g => g.Status == OrderStatus.Completed);
            var cancelled = groupedOrders.FirstOrDefault(g => g.Status == OrderStatus.Cancelled);

            var report = new FinancialReportResponse
            {
                TotalRevenue = completed?.TotalAmount ?? 0,
                CancelValue = cancelled?.TotalAmount ?? 0,
                TotalDiscount = completed?.TotalDiscount ?? 0,
                TotalPoint = completed?.TotalPoint * 10 ?? 0,
                TotalNetRevenue = (completed?.TotalAmount ?? 0) - (completed?.TotalDiscount ?? 0), //1 point=10đ
            };

            return Result<FinancialReportResponse>.Success(report);
        }
    }
}
