using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Application.Feature.Statistics.Queries.SaleResult;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;

public class GetSaleResultHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetSaleResultQuery, IEnumerable<GetSaleResultResponse>>
{
    public async ValueTask<IEnumerable<GetSaleResultResponse>> Handle(
        GetSaleResultQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderList = await unitOfWork
            .Repository<Order>()
            .ListAsync<ListOrderResponse>(cancellationToken);

        if (orderList == null || !orderList.Any())
        {
            return new List<GetSaleResultResponse>();
        }

        int numberOrder = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.CreatedAt.Date == DateTime.UtcNow.Date)
            )
            .Count();

        decimal revenue = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.CreatedAt.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal netRevenue = orderList
            .Where(o =>
                (o.Status == OrderStatus.Completed) && (o.CreatedAt.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal revenueYesterday = orderList
            .Where(o =>
                (o.OrderDate.Date == DateTime.UtcNow.Date.AddDays(-1))
                && (o.Status == OrderStatus.Completed)
                && (o.CreatedAt.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        decimal revenueLastMonth = orderList
            .Where(o =>
                (o.OrderDate.Month == DateTime.UtcNow.AddMonths(-1).Month)
                && (o.Status == OrderStatus.Completed)
                && (o.CreatedAt.Date == DateTime.UtcNow.Date)
            )
            .Sum(o => o.Amount);

        var saleResults = new List<GetSaleResultResponse>
        {
            new GetSaleResultResponse
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
        float result = 0;
        if (last != 0)
        {
            if (today != 0)
            {
                result = (float)(today / last) * 100;
            }
            result = 100;
        }
        return result;
    }
}
