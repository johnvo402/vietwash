using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Queries.List;
using Application.Feature.Statistics.Queries.RevenueStatistic;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;

public class GetRevenueStatisticHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetRevenueStatisticQuery, IEnumerable<GetRevenueStatisticResponse>>
{
    public async ValueTask<IEnumerable<GetRevenueStatisticResponse>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderList = await unitOfWork
            .Repository<Order>()
            .ListAsync<ListOrderResponse>(cancellationToken);

        if (orderList == null || !orderList.Any())
        {
            return new List<GetRevenueStatisticResponse>();
        }

        //DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

        DateTime today = DateTime.UtcNow;

        DateTime firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
        DateTime lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

        var dateSeries = Enumerable
            .Range(0, (lastDayOfMonth - firstDayOfMonth).Days + 1)
            .Select(ds => firstDayOfMonth.AddDays(ds))
            .ToList();

        var revenueStatistics = dateSeries
            .Select(date => new GetRevenueStatisticResponse
            {
                Date = date,
                Revenue = orderList
                    .Where(o =>
                        (o.Status == OrderStatus.Completed) && (o.CreatedAt.Date == date.Date)
                    )
                    .Sum(o => o.Amount),
            })
            .ToList();

        return revenueStatistics;
    }
}
