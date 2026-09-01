using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class GetDashboardCardHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentUser,
    IHttpContextAccessor httpContextAccessor,
    TimeProvider timeProvider
) : IRequestHandler<GetDashboardCardQuery, Result<GetDashboardCardResponse>>
{
    public async ValueTask<Result<GetDashboardCardResponse>> Handle(
        GetDashboardCardQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!ReportBranchScope.IsAuthorized(currentUser.Session?.Branches, request.BranchId))
            return Result<GetDashboardCardResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        TimeZoneInfo timeZone = ReportTimeRange.ResolveTimeZone(
            httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString()
        );
        DateOnly today = DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTime(timeProvider.GetUtcNow(), timeZone).DateTime
        );

        ReportUtcRange todayRange = ReportTimeRange.ForLocalDay(today, timeZone);
        ReportUtcRange yesterdayRange = ReportTimeRange.ForLocalDay(today.AddDays(-1), timeZone);
        ReportUtcRange sameDayLastMonthRange = ReportTimeRange.ForLocalDay(
            today.AddMonths(-1),
            timeZone
        );

        IQueryable<Order> completedOrders = unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.BranchId == request.BranchId
                && order.Status == OrderStatus.Completed
                && order.OrderDate.HasValue
            );

        var todaySummary = await completedOrders
            .Where(order =>
                order.OrderDate >= todayRange.UtcStartInclusive
                && order.OrderDate < todayRange.UtcEndExclusive
            )
            .GroupBy(_ => 1)
            .Select(group => new { Count = group.Count(), Revenue = group.Sum(order => order.Total) })
            .SingleOrDefaultAsync(cancellationToken);

        decimal revenue = todaySummary?.Revenue ?? 0m;
        decimal revenueYesterday = await completedOrders
            .Where(order =>
                order.OrderDate >= yesterdayRange.UtcStartInclusive
                && order.OrderDate < yesterdayRange.UtcEndExclusive
            )
            .SumAsync(order => order.Total, cancellationToken);
        decimal revenueSameDayLastMonth = await completedOrders
            .Where(order =>
                order.OrderDate >= sameDayLastMonthRange.UtcStartInclusive
                && order.OrderDate < sameDayLastMonthRange.UtcEndExclusive
            )
            .SumAsync(order => order.Total, cancellationToken);

        return Result<GetDashboardCardResponse>.Success(
            new GetDashboardCardResponse
            {
                NumberOrder = todaySummary?.Count ?? 0,
                Revenue = revenue,
                // Retained for API compatibility. Both fields mean collected revenue.
                NetRevenue = revenue,
                RevenueYesterday = revenueYesterday,
                RevenueLastMonth = revenueSameDayLastMonth,
                PercentageChangeDay = (float)
                    OrderRevenuePolicy.CalculatePercentageChange(revenue, revenueYesterday),
                PercentageChangeMonth = (float)
                    OrderRevenuePolicy.CalculatePercentageChange(
                        revenue,
                        revenueSameDayLastMonth
                    ),
            }
        );
    }
}
