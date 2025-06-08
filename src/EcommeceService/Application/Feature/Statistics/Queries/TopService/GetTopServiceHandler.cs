using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.TopService;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using JohnChum.SharedKernel.SpecificationQuery.LHS.Dtos.Requests;
using Mediator;

public class GetTopServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<GetTopServiceQuery, IEnumerable<GetTopServiceResponse>>
{
    public async ValueTask<IEnumerable<GetTopServiceResponse>> Handle(
        GetTopServiceQuery query,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var queryParamRequest = new QueryParamRequest();

            var orders = await unitOfWork
                .Repository<Order>()
                .ListAsync(
                    new GetOrderItemSpecification(
                        DateTime.Parse(query.From),
                        DateTime.Parse(query.To),
                        Int32.Parse(query.BranchId)
                    ),
                    queryParamRequest,
                    cancellationToken
                );

            var orderItems = orders.SelectMany(o => o.OrderItems).ToList();

            var groupedOrderItems = orderItems.GroupBy(oi => oi.ServiceId).ToList();

            var serviceIds = groupedOrderItems.Select(g => g.Key).Distinct().ToList();

            var services = await unitOfWork
                .Repository<Service>()
                .ListAsync(
                    new GetServiceByIdsSpecification(serviceIds),
                    queryParamRequest,
                    cancellationToken
                );

            var serviceDict = services.ToDictionary(s => s.Id, s => s);

            var result = groupedOrderItems
                .Select(g => new GetTopServiceResponse
                {
                    ServiceId = g.Key.ToString(),
                    ServiceName = serviceDict.ContainsKey(g.Key)
                        ? serviceDict[g.Key].Name
                        : "Unknown",
                    Description = serviceDict.ContainsKey(g.Key)
                        ? serviceDict[g.Key].Description
                        : "No description",
                    UsageCount = g.Count(),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity),
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            return result;
        }
        catch (Exception ex)
        {
            throw;
        }
    }
}
