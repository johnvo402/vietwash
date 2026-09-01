using Application.Common.Interfaces.Services;
using Application.Common.Interfaces.UnitOfWorks;
using Application.Feature.Reports.Common;
using Application.Feature.Statistics.Queries.TopService;
using Contracts.ApiWrapper;
using Contracts.Application.Common.Exceptions;
using Contracts.Common.Messages;
using Domain.Aggregates.Orders;
using Domain.Aggregates.Orders.Enums;
using Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

public class GetTopServiceHandler(
    IUnitOfWork unitOfWork,
    ICurrentAccount currentUser,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<GetTopServiceQuery, Result<IEnumerable<GetTopServiceResponse>>>
{
    public async ValueTask<Result<IEnumerable<GetTopServiceResponse>>> Handle(
        GetTopServiceQuery request,
        CancellationToken cancellationToken
    )
    {
        if (
            !long.TryParse(request.BranchId, out long branchId)
            || !ReportBranchScope.IsAuthorized(currentUser.Session?.Branches, branchId)
        )
            return Result<IEnumerable<GetTopServiceResponse>>.Failure(
                new ForbiddenError(Message.FORBIDDEN)
            );

        TimeZoneInfo timeZone = ReportTimeRange.ResolveTimeZone(
            httpContextAccessor.HttpContext?.Request.Headers["Time-Zone"].ToString()
        );
        DateOnly fromDate = ReportTimeRange.ParseLocalDate(request.From, nameof(request.From));
        DateOnly toDate = ReportTimeRange.ParseLocalDate(request.To, nameof(request.To));
        ReportUtcRange range = ReportTimeRange.ForLocalDates(fromDate, toDate, timeZone);

        List<GetTopServiceResponse> topServices = await unitOfWork
            .Repository<Order>()
            .QueryAsync(order =>
                order.Status == OrderStatus.Completed
                && order.BranchId == branchId
                && order.OrderDate.HasValue
                && order.OrderDate >= range.UtcStartInclusive
                && order.OrderDate < range.UtcEndExclusive
            )
            .SelectMany(order => order.OrderItems)
            .GroupBy(item => new { item.ServiceId, item.ServiceName })
            .Select(group => new GetTopServiceResponse
            {
                ServiceId = group.Key.ServiceId.ToString(),
                ServiceName = group.Key.ServiceName ?? "Unknown",
                UsageCount = group.Sum(item => item.Quantity),
                // Gross service-line revenue; order-level discounts are not allocated here.
                TotalRevenue = group.Sum(item => item.Price * item.Quantity),
            })
            .OrderByDescending(service => service.TotalRevenue)
            .Take(10)
            .ToListAsync(cancellationToken);

        return Result<IEnumerable<GetTopServiceResponse>>.Success(topServices);
    }
}
