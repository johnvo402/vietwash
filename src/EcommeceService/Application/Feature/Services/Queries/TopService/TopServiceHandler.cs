using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Enums;
using Domain.Aggregates.Orders;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Services.Queries.TopService;

public class TopServiceHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<TopServiceQuery, Result<IEnumerable<TopServiceResponse>>>
{
    public async ValueTask<Result<IEnumerable<TopServiceResponse>>> Handle(
        TopServiceQuery request,
        CancellationToken cancellationToken
    )
    {
        var orderItemsQuery = unitOfWork
            .Repository<Order>()
            .QueryAsync()
            .SelectMany(order =>
                order.OrderItems.Where(item => item.Service.Status == ActivationStatus.Active)
            )
            .Include(item => item.Service)
            .ThenInclude(service => service.UnitRelations);

        var topServices = await orderItemsQuery
            .GroupBy(item => item.ServiceId)
            .Select(TopServiceMapping.Selector())
            .OrderByDescending(x => x.TotalUsed)
            .Take(10)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<TopServiceResponse>>.Success(topServices);
    }
}
