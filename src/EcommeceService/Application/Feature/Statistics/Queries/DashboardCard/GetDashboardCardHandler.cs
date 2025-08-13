using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Contracts.ApiWrapper;
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
        var listBranchUser = currentUser.Session!.Branches!.Select(b => b.ToString()).ToList();

        var today = DateTimeOffset.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var lastMonth = today.AddMonths(-1);

        var repo = unitOfWork.Repository<Order>();

        // Orders hôm nay
        var completedTodayOrders = await repo.QueryAsync(o =>
                listBranchUser.Contains(o.BranchId.ToString())
                && o.BranchId == request.BranchId
                && o.OrderDate != null
                && o.Status == OrderStatus.Completed
                && o.OrderDate.Value >= today
                && o.OrderDate.Value < today.AddDays(1)
            )
            .ToListAsync(cancellationToken);

        if (!completedTodayOrders.Any())
        {
            return Result<GetDashboardCardResponse>.Success();
        }

        int numberOrder = completedTodayOrders.Count;
        decimal revenue = completedTodayOrders.Sum(o => o.Amount);
        decimal netRevenue = revenue;

        // Orders hôm qua
        decimal revenueYesterday = await repo.QueryAsync(o =>
                (listBranchUser.Contains(o.BranchId.ToString()) || o.BranchId == request.BranchId)
                && o.OrderDate != null
                && o.Status == OrderStatus.Completed
                && o.OrderDate.Value >= yesterday
                && o.OrderDate.Value < today
            )
            .SumAsync(o => o.Amount, cancellationToken);

        // Orders tháng trước
        decimal revenueLastMonth = await repo.QueryAsync(o =>
                (listBranchUser.Contains(o.BranchId.ToString()) || o.BranchId == request.BranchId)
                && o.OrderDate != null
                && o.Status == OrderStatus.Completed
                && o.OrderDate.Value.Month == lastMonth.Month
                && o.OrderDate.Value.Year == lastMonth.Year
            )
            .SumAsync(o => o.Amount, cancellationToken);

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
