using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Statistics.Queries.TopService;
using Contracts.ApiWrapper;
using Contracts.Dtos.Requests;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Specifications;
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
            var orderSpec = new GetOrderItemSpecification(
                DateTime.Parse(query.From),
                DateTime.Parse(query.To),
                int.Parse(query.BranchId),
                listBranchUser
            );

            var orders = await unitOfWork
                .DynamicReadOnlyRepository<Order>()
                .ListAsync(orderSpec, queryParamRequest, cancellationToken);

            if (!orders.Any())
                return Result<IEnumerable<GetTopServiceResponse>>.Success(
                    Enumerable.Empty<GetTopServiceResponse>()
                );

            // Aggregate and transform results
            var topServices = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.ServiceId)
                .Select(g => new GetTopServiceResponse
                {
                    ServiceId = g.Key.ToString(),
                    ServiceName = g.First().ServiceName ?? "Unknown",
                    UsageCount = g.Count(),
                    TotalRevenue = g.Sum(oi => oi.Price * oi.Quantity),
                })
                .OrderByDescending(x => x.TotalRevenue)
                .Take(10)
                .ToList();

            return Result<IEnumerable<GetTopServiceResponse>>.Success(topServices);
        }
        catch (Exception)
        {
            throw;
        }
    }
}
