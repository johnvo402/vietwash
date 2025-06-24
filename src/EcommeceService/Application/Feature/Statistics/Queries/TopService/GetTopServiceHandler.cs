using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.TopService;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
using Domain.Aggregates.Services;
using Domain.Aggregates.Services.Specifications;
using Mediator;

public class GetTopServiceHandler(IUnitOfWork unitOfWork, ICurrentAccount currentUser)
    : IRequestHandler<GetTopServiceQuery, Result<IEnumerable<GetTopServiceResponse>>>
{
    public async ValueTask<Result<IEnumerable<GetTopServiceResponse>>> Handle(
        GetTopServiceQuery query,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var listBranchUser = currentUser.Session!.Branches!.ToList();
            var queryParamRequest = new QueryParamRequest();

            var orders = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .ListAsync(
                    new GetOrderItemSpecification(
                        DateTime.Parse(query.From),
                        DateTime.Parse(query.To),
                        Int32.Parse(query.BranchId),
                        listBranchUser
                    ),
                    queryParamRequest,
                    cancellationToken
                );

            var orderItems = orders.SelectMany(o => o.OrderItems).ToList();

            var groupedOrderItems = orderItems.GroupBy(oi => oi.ServiceId).ToList();

            var serviceDict = orderItems.ToDictionary(s => s.Id, s => s);

            var result = groupedOrderItems
                .Select(g => new GetTopServiceResponse
                {
                    ServiceId = g.Key.ToString(),
                    ServiceName = serviceDict.ContainsKey(g.Key)
                        ? serviceDict[g.Key].ServiceName
                        : "Unknown",
                    UsageCount = g.Count(),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity),
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            return Result<IEnumerable<GetTopServiceResponse>>.Success(result);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
