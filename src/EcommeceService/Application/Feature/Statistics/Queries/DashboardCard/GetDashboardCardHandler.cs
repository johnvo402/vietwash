using System.Linq.Expressions;
using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

public class GetDashboardCardHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetDashboardCardQuery, Result<GetDashboardCardResponse>>
{
    public async ValueTask<Result<GetDashboardCardResponse>> Handle(
        GetDashboardCardQuery request,
        CancellationToken cancellationToken
    )
    {
        var listBranchUser = currentUser.Session!.Branches!.ToList();

        // Base query for orders
        Expression<Func<Order, bool>> baseCriteria = o =>
            listBranchUser.Contains(o.BranchId.ToString())
            || o.BranchId == request.BranchId && o.OrderDate != null;

        var today = DateTimeOffset.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var lastMonth = today.AddMonths(-1);

        // Query for completed orders today
        Expression<Func<Order, bool>> todayCriteria = o =>
            baseCriteria.Compile()(o)
            && o.Status == OrderStatus.Completed
            && o.OrderDate!.Value.Date == today;

        var completedTodayOrdersQuery = unitOfWork.Repository<Order>().QueryAsync(todayCriteria);

        var completedTodayOrders = await completedTodayOrdersQuery.ToListAsync(cancellationToken);

        if (!completedTodayOrders.Any())
        {
            return Result<GetDashboardCardResponse>.Success();
        }

        int numberOrder = completedTodayOrders.Count;
        decimal revenue = completedTodayOrders.Sum(o => o.Amount);
        decimal netRevenue = revenue;

        // Query for yesterday's revenue
        Expression<Func<Order, bool>> yesterdayCriteria = o =>
            baseCriteria.Compile()(o)
            && o.Status == OrderStatus.Completed
            && o.OrderDate!.Value.Date == yesterday;

        var yesterdayOrdersQuery = unitOfWork.Repository<Order>().QueryAsync(yesterdayCriteria);

        decimal revenueYesterday = await yesterdayOrdersQuery.SumAsync(
            o => o.Amount,
            cancellationToken
        );

        // Query for last month's revenue
        Expression<Func<Order, bool>> lastMonthCriteria = o =>
            baseCriteria.Compile()(o)
            && o.Status == OrderStatus.Completed
            && o.OrderDate!.Value.Month == lastMonth.Month
            && o.OrderDate!.Value.Year == lastMonth.Year;

        var lastMonthOrdersQuery = unitOfWork.Repository<Order>().QueryAsync(lastMonthCriteria);

        decimal revenueLastMonth = await lastMonthOrdersQuery.SumAsync(
            o => o.Amount,
            cancellationToken
        );

        return Result<GetDashboardCardResponse>.Success(
            new GetDashboardCardResponse
            {
                NumberOrder = numberOrder,
                Revenue = revenue,
                NetRevenue = netRevenue,
                RevenueYesterday = revenueYesterday,
                RevenueLastMonth = revenueLastMonth,
                PercentageChangeDay = CalculatePercentageChange(
                    (float)revenue,
                    (float)revenueYesterday
                ),
                PercentageChangeMonth = CalculatePercentageChange(
                    (float)revenue,
                    (float)revenueLastMonth
                ),
            }
        );
    }

    private float CalculatePercentageChange(float today, float last)
    {
        if (last == 0)
        {
            return today == 0 ? 0 : 100;
        }

        return ((today - last) / last) * 100;
    }
}
