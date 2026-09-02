using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Orders.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Orders.Queries.TotalOrderByStaff;

public class TotalOrderByStaffHandler(IUnitOfWork unitOfWork, ICurrentAccount currentAccount)
    : IRequestHandler<TotalOrderByStaffQuery, Result<TotalOrderByStaffResponse>>
{
    public async ValueTask<Result<TotalOrderByStaffResponse>> Handle(
        TotalOrderByStaffQuery request,
        CancellationToken cancellationToken
    )
    {
        if (!OrderActorAccess.IsStaffSide(currentAccount.Session?.Role))
            return Result<TotalOrderByStaffResponse>.Failure(new ForbiddenError(Message.FORBIDDEN));

        IReadOnlyList<long> branchIds = OrderBranchAccess
            .FromSession(currentAccount.Session?.Branches)
            .BranchIds;
        IQueryable<Order> query = unitOfWork
            .Repository<Order>()
            .QueryAsync(x =>
                x.StaffId == request.StaffId
                && x.Status == OrderStatus.Completed
                && branchIds.Contains(x.BranchId)
            );

        TotalOrderByStaffResponse result = new()
        {
            TotalOrder = await query.CountAsync(cancellationToken),
            TotalRevenue = await query.SumAsync(o => o.Total, cancellationToken),
        };

        return Result<TotalOrderByStaffResponse>.Success(result);
    }
}
