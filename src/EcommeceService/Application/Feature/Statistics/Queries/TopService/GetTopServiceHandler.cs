using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.TopService;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Aggregates.Services;
using Mediator;

public class GetTopServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTopServiceQuery, IEnumerable<GetTopServiceResponse>>
{
    public async ValueTask<IEnumerable<GetTopServiceResponse>> Handle(
        GetTopServiceQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderList = (await unitOfWork.Repository<Order>().ListAsync(cancellationToken))
            .Where(o => o.Status == OrderStatus.Completed)
            .ToList();

        if (!orderList.Any())
            return [];

        var serviceUsage = orderList
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.ServiceId.ToString())
            .Select(g => new
            {
                ServiceId = g.Key,
                UsageCount = g.Count(),
                TotalRevenue = g.Sum(oi => oi.Price),
            })
            .OrderByDescending(s => s.UsageCount)
            .Take(10)
            .ToList();

        var serviceIds = serviceUsage.Select(s => s.ServiceId).ToList();

        var services = (await unitOfWork.Repository<Service>().ListAsync(cancellationToken))
            .Where(s => serviceIds.Contains(s.Id.ToString()))
            .ToList();

        var serviceDict = services.ToDictionary(s => s.Id.ToString(), s => s);

        var result = serviceUsage
            .Select(s => new GetTopServiceResponse
            {
                ServiceId = s.ServiceId,
                ServiceName = serviceDict.TryGetValue(s.ServiceId, out var service)
                    ? service.Name
                    : "Unknown",
                Description = service?.Description ?? "",
                UsageCount = s.UsageCount,
                TotalRevenue = s.TotalRevenue,
            })
            .ToList();

        return result;
    }
}
