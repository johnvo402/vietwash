using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Services;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Services.Queries.TopService
{
    public class TopServiceHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<TopServiceQuery, Result<IEnumerable<TopServiceResponse>>>
    {
        public async ValueTask<Result<IEnumerable<TopServiceResponse>>> Handle(
            TopServiceQuery request,
            CancellationToken cancellationToken
        )
        {
            var query = await unitOfWork
                .Repository<Order>()
                .QueryAsync()
                .SelectMany(order => order.OrderItems)
                .Include(x => x.Service)
                .ThenInclude(x => x.UnitRelations)
                .GroupBy(x => x.ServiceId)
                .Select(TopServiceMapping.Selector())
                .OrderByDescending(x => x.TotalUsed)
                .Take(10)
                .AsSplitQuery()
                .ToListAsync(cancellationToken);

            return Result<IEnumerable<TopServiceResponse>>.Success(query);
        }
    }
}
