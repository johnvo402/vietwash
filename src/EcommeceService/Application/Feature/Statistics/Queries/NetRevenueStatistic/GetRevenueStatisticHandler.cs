using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Domain.Functions;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Feature.Statistics.Queries.RevenueStatistic;

public class GetRevenueStatisticHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentUser,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetRevenueStatisticQuery, Result<IEnumerable<GetRevenueStatistic>>>
{
    public async ValueTask<Result<IEnumerable<GetRevenueStatistic>>> Handle(
        GetRevenueStatisticQuery request,
        CancellationToken cancellationToken
    )
    {
        if (
            !long.TryParse(request.BranchId, out long branchId)
            || !ReportBranchScope.IsAuthorized(currentUser.Session?.Branches, branchId)
        )
            return Result<IEnumerable<GetRevenueStatistic>>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        TimeZoneInfo timeZone = ReportTimeRange.ResolveTimeZone(
            httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString()
        );
        DateOnly fromDate = ReportTimeRange.ParseLocalDate(request.From, nameof(request.From));
        DateOnly toDate = ReportTimeRange.ParseLocalDate(request.To, nameof(request.To));
        ReportUtcRange range = ReportTimeRange.ForLocalDates(fromDate, toDate, timeZone);

        var completedOrders = await unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.Status == OrderStatus.Completed
                && order.BranchId == branchId
                && order.OrderDate.HasValue
                && order.OrderDate >= range.UtcStartInclusive
                && order.OrderDate < range.UtcEndExclusive
            )
            .Select(order => new { FinancialDate = order.OrderDate!.Value, order.Total })
            .ToListAsync(cancellationToken);

        Dictionary<DateOnly, decimal> revenueByDate = completedOrders
            .GroupBy(order =>
                DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(order.FinancialDate, timeZone).DateTime)
            )
            .ToDictionary(group => group.Key, group => group.Sum(order => order.Total));

        List<GetRevenueStatistic> result = ReportTimeRange
            .EnumerateLocalDates(fromDate, toDate)
            .Select(date => new GetRevenueStatistic
            {
                RevenueDate = date,
                TotalRevenue = revenueByDate.GetValueOrDefault(date),
            })
            .ToList();

        return Result<IEnumerable<GetRevenueStatistic>>.Success(result);
    }
}
