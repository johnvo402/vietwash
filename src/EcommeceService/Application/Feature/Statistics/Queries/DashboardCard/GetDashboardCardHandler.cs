using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using Mediator;

public class GetDashboardCardHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetDashboardCardQuery, Result<GetDashboardCardResponse>>
{
    public async ValueTask<Result<GetDashboardCardResponse>> Handle(
        GetDashboardCardQuery request,
        CancellationToken cancellationToken
    )
    {
        var listBranchUser = currentUser.Session!.Branches!.ToList();
        var queryParamRequest = new QueryParamRequest();

        var orderList = await unitOfWork
            .DynamicReadOnlyRepository<Order>()
            .ListAsync(
                new ListOrderSpecification(
                    string.Empty,
                    string.Empty,
                    request.BranchId,
                    listBranchUser
                ),
                queryParamRequest,
                cancellationToken
            );

        if (orderList == null || !orderList.Any())
        {
            return Result<GetDashboardCardResponse>.Success();
        }

        var today = DateTime.UtcNow.Date;
        var yesterday = today.AddDays(-1);
        var lastMonth = today.AddMonths(-1);

        // Lọc những đơn hàng Completed hôm nay
        var completedTodayOrders = orderList
            .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date == today)
            .ToList();

        int numberOrder = completedTodayOrders.Count;

        decimal revenue = completedTodayOrders.Sum(o => o.Amount);

        // Nếu netRevenue có công thức khác thì sửa ở đây, còn không thì giữ bằng revenue
        decimal netRevenue = revenue;

        // Tổng doanh thu hôm qua
        decimal revenueYesterday = orderList
            .Where(o => o.Status == OrderStatus.Completed && o.OrderDate.Date == yesterday)
            .Sum(o => o.Amount);

        // Tổng doanh thu tháng trước
        decimal revenueLastMonth = orderList
            .Where(o =>
                o.Status == OrderStatus.Completed
                && o.OrderDate.Month == lastMonth.Month
                && o.OrderDate.Year == lastMonth.Year
            )
            .Sum(o => o.Amount);

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
