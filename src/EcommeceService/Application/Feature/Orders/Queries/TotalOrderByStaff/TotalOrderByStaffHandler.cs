using Application.Common.Interfaces.UnitOfWorks;
using Contracts.ApiWrapper;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Queries.TotalOrderByStaff
{
    public class TotalOrderByStaffHandler(IUnitOfWork unitOfWork)
        : IRequestHandler<TotalOrderByStaffQuery, Result<TotalOrderByStaffResponse>>
    {
        public async ValueTask<Result<TotalOrderByStaffResponse>> Handle(
            TotalOrderByStaffQuery request,
            CancellationToken cancellationToken
        )
        {
            var query = unitOfWork
                .Repository<Order>()
                .QueryAsync(x => x.StaffId == request.StaffId && x.Status == OrderStatus.Completed);

            var result = new TotalOrderByStaffResponse
            {
                TotalOrder = await query.CountAsync(cancellationToken),
                TotalRevenue = await query.SumAsync(o => o.Total, cancellationToken),
            };

            return Result<TotalOrderByStaffResponse>.Success(result);
        }
    }
}
