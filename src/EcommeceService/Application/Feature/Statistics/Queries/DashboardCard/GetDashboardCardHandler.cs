using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Orders.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using Mediator;

public class GetDashboardCardHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetDashboardCardQuery, IEnumerable<GetDashboardCardResponse>>
{
    public async ValueTask<IEnumerable<GetDashboardCardResponse>> Handle(
        GetDashboardCardQuery request,
        CancellationToken cancellationToken
    )
    {
        var listBranchUser = currentUser.Session!.Branches!.ToList();
        var queryParamRequest = new QueryParamRequest();
        var orderList = await unitOfWork
            .Repository<Order>()
            .ListAsync(
                new ListOrderSpecification(null, null, request.BranchId, listBranchUser),
                queryParamRequest,
                cancellationToken
            );

        if (orderList == null || !orderList.Any())
        {
            return new List<GetDashboardCardResponse>();
        }

        int numberOrder = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.OrderDate.Date == DateTime.UtcNow.Date)
            )
            .Count();

        decimal revenue = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.OrderDate.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal netRevenue = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.OrderDate.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal revenueYesterday = orderList
            .Where(o =>
                (o.OrderDate.Date == DateTime.UtcNow.Date.AddDays(-1))
                && (o.Status == OrderStatus.Completed)
                && (o.OrderDate.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal revenueLastMonth = orderList
            .Where(o =>
                (o.OrderDate.Month == DateTime.UtcNow.AddMonths(-1).Month)
                && (o.Status == OrderStatus.Completed)
                && (o.OrderDate.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        var saleResults = new List<GetDashboardCardResponse>
        {
            new GetDashboardCardResponse
            {
                NumberOrder = numberOrder,
                Revenue = revenue,
                NetRevenue = netRevenue,
                RevenueYesterday = revenueYesterday,
                RevenueLastMonth = revenueLastMonth,
                PercentageChangeDay = CaculatePercentageChange(
                    (float)revenue,
                    (float)revenueYesterday
                ),
                PercentageChangeMonth = CaculatePercentageChange(
                    (float)revenue,
                    (float)revenueLastMonth
                ),
            },
        };

        return saleResults;
    }

    public float CaculatePercentageChange(float today, float last)
    {
        if (last == 0)
        {
            return today == 0 ? 0 : 100;
        }

        return ((today - last) / last) * 100;
    }
}
